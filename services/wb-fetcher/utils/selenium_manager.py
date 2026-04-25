import logging
from socket import timeout
import time
import json
from selenium import webdriver
from selenium.webdriver.chrome.options import Options
from selenium.webdriver.support.ui import WebDriverWait
from selenium.webdriver.support import expected_conditions as EC
from selenium.common.exceptions import TimeoutException, WebDriverException
from selenium_stealth import stealth
from typing import Optional

logger = logging.getLogger(__name__)

class SeleniumManager:
    def __init__(self, headless=False):
        self.driver: Optional[webdriver.Chrome] = None
        self.wait: Optional[WebDriverWait] = None
        self.headless = headless

    def create_driver_with_logging(self) -> webdriver.Chrome:
        if self.driver:
            logger.warning("Драйвер уже создан, повторная инициализация пропущена")
            return self.driver
        chrome_options = Options()
        
        chrome_options.add_argument("--no-sandbox")
        chrome_options.add_argument("--disable-dev-shm-usage")
        chrome_options.add_argument("--disable-blink-features=AutomationControlled")
        chrome_options.add_argument("--disable-gpu")
        chrome_options.add_argument("--blink-settings=imagesEnabled=false")
        chrome_options.add_argument("--disable-dev-shm-usage")
        chrome_options.add_experimental_option("excludeSwitches", ["enable-automation"])
        chrome_options.add_experimental_option('useAutomationExtension', False)
        chrome_options.set_capability('goog:loggingPrefs', {'performance': 'ALL'})
        chrome_options.add_argument("--disable-images")
        chrome_options.page_load_strategy = "eager"
        
        chrome_options.add_argument("--disable-extensions")
        chrome_options.add_argument("--disable-plugins")
        
        if self.headless:
            chrome_options.add_argument("--headless")
        
        chrome_options.add_argument("--window-size=1920,1080")
        
        try:
            driver = webdriver.Chrome(options=chrome_options)
            driver.execute_cdp_cmd("Network.setBlockedURLs", {
                "urls": ["*.png", "*.jpg", "*.jpeg", "*.gif", "*.css", "*.woff2"]
            })
            driver.execute_cdp_cmd("Network.enable", {})
            
            stealth(driver,
                   languages=["ru-RU", "ru"],
                   vendor="Google Inc.",
                   platform="Win32",
                   webgl_vendor="Intel Inc.",
                   renderer="Intel Iris OpenGL Engine",
                   fix_hairline=True)
            
            driver.implicitly_wait(0) #was 20
            driver.set_page_load_timeout(20) #was 60
            
            driver.execute_script("Object.defineProperty(navigator, 'webdriver', {get: () => undefined})")
            
            self.driver = driver
            self.wait = WebDriverWait(driver, 5)#self.wait = 0 #was 20
            
            logger.info("Chrome драйвер с логированием создан успешно")
            return driver
            
        except WebDriverException as e:
            logger.error(f"Ошибка создания Chrome драйвера с логированием: {e}")
            raise
    
    def navigate_to_url(self, url: str):
        if not self.driver:
            logger.error("Драйвер не инициализирован")
            return False
        
        try:
            logger.info(f"Переход к URL: {url}")
            self.driver.get(url)
            self.wait_for_antibot()
            return True
        except TimeoutException:
            logger.warning(f"Таймаут при переходе: {url}")
        except WebDriverException as e:
            logger.error(f"Ошибка при навигации к URL {url}: {e}")

    def wait_for_antibot(self, max_wait_time = 120):
        start_time = time.time()
        attempts = 0
        max_attempts = 3
        while time.time() - start_time < max_wait_time:
            try:
                if self.is_blocked():
                    if attempts < max_attempts:
                        logger.warning("Обнаружен антибот элемент, обновляем страницу")
                        attempts += 1
                        time.sleep(2)
                        continue
                    else:
                        logger.error("Превышено количество попыток обновления страницы из-за антибота")
                        break
                else:
                    logger.info("Антибот пройден")
                    return
            except Exception as e:
                if "Access blocked" in str(e):
                    raise
                time.sleep(2)
                continue
        logger.warning("Превышено максимальное время ожидания антибота")
        raise TimeoutException("Превышено максимальное время ожидания антибота")
    
    def is_blocked(self):
        if not self.driver:
            return True
        try:
            blocked_flags = [
                "cloudflare", "checking your browser", "enable javascript",
                "access denied", "blocked", "ddos-guard", "проверка браузера",
                "доступ ограничен", "access restricted"
            ]
            page_source = self.driver.page_source.lower()

            for flag in blocked_flags:
                if flag in page_source:
                    return True
            return False
        except WebDriverException as e:
            logger.error(f"Ошибка при проверке блокировки: {e}")
            return True
    
    def _extract_json_from_html(self, html_content: str) -> Optional[str]:
        try:
            import re
            
            pre_pattern = r'<pre[^>]*>(.*?)</pre>'
            pre_match = re.search(pre_pattern, html_content, re.DOTALL | re.IGNORECASE)
            
            if pre_match:
                json_content = pre_match.group(1).strip()
                logger.debug("JSON найден в <pre> теге")
                return json_content
            

            first_brace = html_content.find('{')
            last_brace = html_content.rfind('}')
            
            if first_brace != -1 and last_brace != -1 and first_brace < last_brace:
                json_content = html_content[first_brace:last_brace + 1]
                logger.debug("JSON найден по поиску скобок")
                return json_content
            
            return None
            
        except Exception as e:
            logger.error(f"Ошибка извлечения JSON из HTML: {e}")
            return None
        
    def close(self):
        if self.driver:
            try:
                self.driver.quit()
                logger.info("Chrome driver закрыт")
            except Exception as e:
                logger.error(f"Ошибка при закрытии драйвера: {e}")
            finally:
                self.driver = None
                self.wait = None
            
    def get_json_via_logs(self):
        try:
            logs = self.driver.get_log("performance")

            for log in logs:
                message = json.loads(log["message"])["message"]
                if message.get("method") != "Network.responseReceived":
                    continue
                response = message["params"]["response"]
                request_id = message["params"]["requestId"]
                url = response.get("url", "")
                if "/api/composer-api.bx/page/json" in url and "product" in url:

                    body = self.driver.execute_cdp_cmd(
                        "Network.getResponseBody",
                        {"requestId": request_id}
                    )
                    content = body.get("body")
                    if body.get("base64Encoded"):
                        import base64
                        content = base64.b64decode(content).decode("utf-8")
                    return content

        except Exception as e:
            logger.error(f"Ошибка получения JSON из логов: {e}")

        return None