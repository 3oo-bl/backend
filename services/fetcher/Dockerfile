FROM python:3.11 AS builder

WORKDIR /

RUN apt-get update && apt-get install -y --no-install-recommends \
    chromium \
    chromium-driver \
    libnss3 \
    libgbm1 \
    libasound2 \
    fonts-liberation \
    && rm -rf /var/lib/apt/lists/*

COPY requirements.txt .
RUN pip install --no-cache-dir -r requirements.txt
COPY . .


FROM python:3.11-slim

ENV CHROME_BIN=/usr/bin/chromium
ENV CHROMEDRIVER_PATH=/usr/bin/chromedriver

RUN apt-get update && apt-get install -y --no-install-recommends \
   chromium \
   chromium-driver \
   libnss3 \
   libgbm1 \
   libasound2 \
   fonts-liberation \
   && rm -rf /var/lib/apt/lists/*

WORKDIR /

COPY --from=builder /usr/local/lib/python3.11/site-packages /usr/local/lib/python3.11/site-packages
COPY --from=builder /usr/local/bin /usr/local/bin 

COPY . .

EXPOSE 50051

CMD ["python", "server.py"]
