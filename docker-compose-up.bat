cd /d %~dp0
docker-compose down && docker-compose -f docker-compose.yml -f docker-compose.override.windows.yml -f docker-compose.agile.config.yml up --build -d