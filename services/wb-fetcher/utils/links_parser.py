import logging
import time
from selenium.webdriver.common.by import By
from selenium.webdriver.support.ui import WebDriverWait
from selenium.webdriver.support import expected_conditions as EC
import searchers_pb2
from utils.selenium_manager import SeleniumManager

logger = logging.getLogger(__name__)

class OzonLinksParser:
    def __init__(self):
        self.base_link = "https://www.ozon.ru/search/?from_global=true&text="
        self.target_products = -1
        self.itemName = None
        self.driver = None
        self.selenium_manager = SeleniumManager()
        self.collected_links = set()

    def parse_links(self, request):
        print(f"Received search request: {request.itemName}")
        self.target_products = request.quantity
        self.itemName = request.itemName
        self.driver = self.selenium_manager.create_driver_with_logging()
        if not self.load_page():
            self.cleanup()
            return searchers_pb2.SearchResponse(status = 0, raw_json = "Антибот-защита при загрузке страницы не пропустила")
        self.collect_links()
        if not self.collected_links:
            self.cleanup()
            return searchers_pb2.SearchResponse(status = 0, raw_json = "Не удалось собрать ссылки на товары")
        responce = searchers_pb2.SearchResponse(status = 1, raw_json = "\n".join(self.collected_links))
        #self.cleanup()
        return responce
    
    def load_page(self):
        max_drivers = 3
        url = self.base_link + self.itemName
        print(f"Target URL: {url}")

        for attempt in range(max_drivers):
            try:
                logger.info(f"Попытка загрузить {url} номер {attempt + 1}")

                if attempt > 0:
                    self.selenium_manager.close()
                    self.driver = self.selenium_manager.create_driver_with_logging()
                
                if not self.selenium_manager.navigate_to_url(url):
                    if attempt < max_drivers - 1:
                        logger.warning(f"Не удалось загрузить {url} на попытке {attempt + 1}")
                        continue
                    return False
                
                WebDriverWait(self.driver, 10).until(
                    EC.presence_of_element_located((By.ID, "contentScrollPaginator")))
                
                logger.info(f"Успешно загрузили {url} на попытке {attempt + 1}")
                return True
            except Exception as e:
                logger.error(f"Ошибка при загрузке {url} на попытке {attempt + 1}: {e}")
                if attempt < max_drivers - 1:
                    logger.info("Пробуем снова...")
                else:
                    logger.error("Превышено количество попыток загрузки страницы.")
                    return False
                
    def collect_links(self):
        seen = set()
        no_growth = 0

        while len(seen) < self.target_products:

            self.driver.execute_script(
                "window.scrollTo(0, document.body.scrollHeight);"
            )
            time.sleep(0.7)
            links = self.driver.execute_script("""
                return Array.from(document.querySelectorAll("[class*='tile-root'] a[data-prerender='true']"))
                    .map(a => a.href)
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

        self.collected_links = list(seen)[:self.target_products]
    
    def extract_links(self):
        return self.driver.execute_script("""
            return Array.from(document.querySelectorAll("[class*='tile-root'] a[data-prerender='true']"))
                .map(a => a.href)
        """)
    
    def cleanup(self):
        if self.collect_links:
            self.collected_links.clear()