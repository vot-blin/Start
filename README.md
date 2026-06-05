# SmartSearch

Проект состоит из:
- `SmartSearch/SmartSearch` — ASP.NET Core API (поиск/анализ, векторный поиск)
- `smart-search-ui` — фронтенд (Vite + Vue)

## Векторная база (Qdrant) и загрузка товаров

В API добавлены эндпоинты:
- `POST /api/vector/reindex` — создать коллекцию (если нужно) и загрузить товары
- `GET /api/vector/search?q=...&limit=10` — поиск по векторному сходству

Если Qdrant недоступен, API автоматически переключается на **in-memory** векторное хранилище (для разработки/демо).

### Вариант A: локально без Docker (in-memory)

1) Запустите бэкенд:

```powershell
cd .\SmartSearch\SmartSearch
dotnet run
```

2) Загрузите товары:

```powershell
Invoke-WebRequest -UseBasicParsing -Method Post -Uri "http://localhost:5124/api/vector/reindex"
```

3) Выполните векторный поиск:

```powershell
Invoke-WebRequest -UseBasicParsing -Uri "http://localhost:5124/api/vector/search?q=серая%20куртка%20с%20мехом"
```

### Вариант B: Qdrant через Docker Desktop (рекомендуется)

1) Установите Docker Desktop и убедитесь, что команда `docker` работает в терминале.

2) Поднимите Qdrant:

```powershell
cd .\
docker compose up -d
```

3) Перезапустите бэкенд. Он автоматически начнёт использовать Qdrant по `QDRANT_URL` (см. `.env`).

## Переменные окружения

Файл: `SmartSearch/SmartSearch/.env`

- `QDRANT_URL` — URL Qdrant (по умолчанию `http://localhost:6333`)

