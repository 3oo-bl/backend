import json
import logging
import re
import time
from selenium.webdriver.common.by import By
from selenium.webdriver.support.ui import WebDriverWait
from selenium.webdriver.support import expected_conditions as EC
from selenium.common.exceptions import TimeoutException
import searchers_pb2
import searchers_pb2_grpc
from utils.links_parser import OzonLinksParser
from utils.selenium_manager import SeleniumManager

logger = logging.getLogger(__name__)

class OzonParserService(searchers_pb2_grpc.OzonParserServicer):
    def __init__(self):
        self.OzonLinksParser = OzonLinksParser()
        self.selenium_manager = SeleniumManager()
        self.driver = self.selenium_manager.create_driver_with_logging()
        self.results = None
        self.api_url_start = "https://www.ozon.ru/api/composer-api.bx/page/json/v2?url=/product/"
        self.api_url_end = "&__rr=1"

    def Search(self, request, context):
        links = self.OzonLinksParser.parse_links(request)
        if links.status == 1:
            return searchers_pb2.SearchResponse(status = 1, raw_json = json.dumps(self.parse_products(links.raw_json.split("\n"))))
        self.cleanup()
        return links
    
    def parse_products(self, products):
        print(f"Collected product links: {len(products)}")
        articles = set()

        for url in products:
            article = self.extract_article_from_url(url)
            if article:
                articles.add(article)

        print(f"Прочитано артикулов: {len(articles)}")
        results = []

        for article in articles:
            data = self.wait_product_info(article)
            if data:
                results.append(data)

        return results
        
            
    def extract_article_from_url(self, url: str) -> str:
        try:
            match = re.search(r'/product/[^/]+-(\d+)/', url)
            return match.group(1) if match else ""
        except Exception:
            return ""
        
    def wait_product_info(self, article):
        results = []
        max_retries = 3
        full_url = f"{self.api_url_start}{article}{self.api_url_end}"
        for attempt in range(max_retries):
            if not self.selenium_manager.navigate_to_url(full_url):
                logger.warning(f"Не удалось загрузить API URL: {full_url} на попытке {attempt + 1}")
                if attempt < max_retries - 1:
                    time.sleep(2)
                    continue
                return "Не удалось загрузить данные о товаре"
            json_content = self.selenium_manager.get_json_via_logs()
            if not json_content:
                if attempt < max_retries - 1:
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

            widget_states = data.get("widgetStates", {})

            result = {}

            for key, value in widget_states.items():
                if key.startswith("webStickyProducts-"):
                    try:
                        sticky = json.loads(value)
                        result["name"] = sticky.get("name")
                        result["image"] = sticky.get("coverImageUrl")

                        seller = sticky.get("seller", {})
                        result["seller_name"] = seller.get("name")
                        result["seller_inn"] = seller.get("inn")
                    except:
                        pass

            for key, value in widget_states.items():
                if key.startswith("webPrice-"):
                    try:
                        price = json.loads(value)
                        result["price"] = price.get("price")
                        result["card_price"] = price.get("cardPrice")
                    except:
                        pass

            return result

        except Exception as e:
            logger.error(f"parse error: {e}")
            return {}


    def cleanup(self):
        if self.selenium_manager:
            self.selenium_manager.close()