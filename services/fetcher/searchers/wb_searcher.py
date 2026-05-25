import json
import logging

import requests
import time
import random

import searchers_pb2
import searchers_pb2_grpc

logger = logging.getLogger(__name__)

class WBParserService(searchers_pb2_grpc.WbParserServicer):
    def __init__(self):
        self.session = requests.Session()

        self.headers = {
            "User-Agent": (
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) "
                "AppleWebKit/537.36 (KHTML, like Gecko) "
                "Chrome/122.0.0.0 Safari/537.36"
            ),
            "Accept": "application/json, text/plain, */*",
            "Accept-Language": "ru-RU,ru;q=0.9,en;q=0.8",
            "Referer": "https://www.wildberries.ru/",
            "Origin": "https://www.wildberries.ru",
            "Connection": "keep-alive",
        }

        self.base_url = "https://u-search.wb.ru/exactmatch/ru/common/v18/search"

    def _sleep(self, attempt):
        base = min(20, 1.2 * (2 ** attempt))
        jitter = random.uniform(0.5, 3.0)
        time.sleep(base + jitter)

    def _handle_429(self, resp, attempt):
        retry_after = resp.headers.get("Retry-After")

        if retry_after:
            wait = int(retry_after)
        else:
            wait = min(30, (2 ** attempt) + random.uniform(1, 5))

        logger.warning(f"429 → sleep {wait:.1f}s")
        time.sleep(wait)

    def Search(self, request, context):
        logger.warning(f"Received search request: {request.itemName}")
        retries = 5
        page = request.page
        query = request.itemName
        params = {
            "appType": 1,
            "curr": "rub",
            "dest": -1029256,
            "page": page,
            "query": query,
            "resultset": "catalog",
            "spp": 30,
        }

        for attempt in range(retries):
            try:
                time.sleep(random.uniform(1.5, 4.0))

                resp = requests.get(
                    self.base_url,
                    params=params,
                    headers=self.headers,
                    timeout=10
                )

                print(resp.status_code)

                if resp.status_code == 429:
                    self._handle_429(resp, attempt)
                    continue

                if resp.status_code != 200:
                    print(f"[HTTP {resp.status_code}] retry {attempt + 1}")
                    self._sleep(attempt)
                    continue

                data = resp.json()
                yield searchers_pb2.SearchResponse(status = 1, raw_json = json.dumps(data))
                return

            except Exception as e:
                print("Exception:", e)
                self._sleep(attempt)

        yield searchers_pb2.SearchResponse(status = 0, raw_json = "")
        return