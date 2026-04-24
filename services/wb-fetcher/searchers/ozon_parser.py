import json
import logging
import re
import threading
import time
from selenium.common.exceptions import TimeoutException
import searchers_pb2
import searchers_pb2_grpc
from utils.links_parser import OzonLinksParser
from utils.selenium_manager import SeleniumManager

logger = logging.getLogger(__name__)


class OzonParserService(searchers_pb2_grpc.OzonParserServicer):
    def __init__(self):
        self._semaphore = threading.Semaphore(3)
        self.api_url_start = "https://www.ozon.ru/api/composer-api.bx/page/json/v2?url=/product/"
        self.api_url_end = "&__rr=1"

    def Search(self, request, context):
        with self._semaphore:
            selenium_manager = SeleniumManager()
            links_parser = OzonLinksParser(selenium_manager)
            try:
                selenium_manager.create_driver_with_logging()
                links = links_parser.parse_links(request)
                if links.status == 1:
                    return searchers_pb2.SearchResponse(
                        status=1,
                        raw_json=json.dumps(self.parse_products(links.raw_json.split("\n"), selenium_manager))
                    )
                return links
            except Exception as e:
                logger.error(f"Ошибка в Search: {e}", exc_info=True)
                return searchers_pb2.SearchResponse(status=0, raw_json="Произошла ошибка при обработке запроса")
            finally:
                selenium_manager.close()

    def parse_products(self, products, selenium_manager):
        print(f"Collected product links: {len(products)}")
        articles = set()
        for url in products:
            article = self.extract_article_from_url(url)
            if article:
                articles.add(article)

        print(f"Прочитано артикулов: {len(articles)}")
        results = []
        for article in articles:
            data = self.wait_product_info(article, selenium_manager)
            if data:
                results.append(data)
        return results

    def extract_article_from_url(self, url: str) -> str:
        try:
            match = re.search(r'/product/[^/]+-(\d+)/', url)
            return match.group(1) if match else ""
        except Exception:
            return ""

    def wait_product_info(self, article, selenium_manager):
        results = []
        full_url = f"{self.api_url_start}{article}{self.api_url_end}"
        for attempt in range(3):
            if not selenium_manager.navigate_to_url(full_url):
                logger.warning(f"Не удалось загрузить {full_url} на попытке {attempt + 1}")
                if attempt < 2:
                    time.sleep(2)
                    continue
                return "Не удалось загрузить данные о товаре"
            json_content = selenium_manager.get_json_via_logs()
            if not json_content:
                if attempt < 2:
                    time.sleep(2)
                    continue
                return "Не удалось получить данные о товаре"
            results.append(self.collect_product_info(json_content))
            break
        return results

    def collect_product_info(self, json_content):
        try:
            data = json.loads(json_content)
            print("JSON в обработке")
            return data.get("widgetStates", {})
        except Exception as e:
            logger.error(f"parse error: {e}")
            return {}