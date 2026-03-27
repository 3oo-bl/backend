# backend

## Для сброки образа
docker build -t profitableviewapi .

## Для запуска
Сейчас по умолчанию поднимается fake API, если надо обычный - в compose.yml последнюю строку надо закомментировать (сейчас там ничего нет и будет NotImplementedException) 

docker compose up
