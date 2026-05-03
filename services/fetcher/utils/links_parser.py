import logging
import time
from selenium.webdriver.common.by import By
from selenium.webdriver.support.ui import WebDriverWait
from selenium.webdriver.support import expected_conditions as EC
import searchers_pb2
from utils.selenium_manager import SeleniumManager

logger = logging.getLogger(__name__)


class OzonLinksParser:
    def __init__(self, selenium_manager: SeleniumManager):
        self.base_link = "https://www.ozon.ru/search/?from_global=true&text="
        self.target_products = -1
        self.itemName = None
        self.selenium_manager = selenium_manager

    def parse_links(self, request) -> searchers_pb2.SearchResponse:
        logger.info(f"Получен запрос на поиск: {request.itemName}")
        self.target_products = request.quantity
        self.itemName = request.itemName

        if not self.load_page():
            return searchers_pb2.SearchResponse(
                status=0,
                raw_json="Антибот-защита при загрузке страницы не пропустила",
            )

        collected = self.collect_links()

        if not collected:
            return searchers_pb2.SearchResponse(
                status=0,
                raw_json="Не удалось собрать ссылки на товары",
            )

        return searchers_pb2.SearchResponse(
            status=1,
            raw_json="\n".join(collected),
        )

    def load_page(self) -> bool:
        url = self.base_link + self.itemName
        logger.info(f"Целевой URL: {url}")

        for attempt in range(3):
            try:
                logger.info(f"Попытка загрузить страницу #{attempt + 1}")

                if not self.selenium_manager.navigate_to_url(url):
                    logger.warning(f"navigate_to_url вернул False на попытке {attempt + 1}")
                    continue

                WebDriverWait(self.selenium_manager.driver, 10).until(
                    EC.presence_of_element_located((By.ID, "contentScrollPaginator"))
                )

                logger.info(f"Страница успешно загружена на попытке {attempt + 1}")
                return True

            except Exception as e:
                logger.warning(f"Содержимое страницы при блокировке: {self.driver.page_source}")
                logger.error(f"Ошибка загрузки страницы на попытке {attempt + 1}: {e}")

        logger.error("Превышено количество попыток загрузки страницы")
        return False

    def collect_links(self) -> list:
        seen = set()
        no_growth = 0

        while len(seen) < self.target_products:
            self.selenium_manager.driver.execute_script(
                "window.scrollTo(0, document.body.scrollHeight);"
            )
            time.sleep(0.7)

            links = self.selenium_manager.driver.execute_script("""
                return Array.from(
                    document.querySelectorAll("[class*='tile-root'] a[data-prerender='true']")
                ).map(a => a.href)
            """)

            before = len(seen)
            seen.update(links)
            after = len(seen)

            if after == before:
                no_growth += 1
            else:
                no_growth = 0

            if no_growth >= 3:
                break

        return list(seen)[: self.target_products]
    
    def cleanup(self):
        if self.collected_links:
            self.collected_links.clear()