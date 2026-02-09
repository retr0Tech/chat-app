#!/bin/bash
# Start all services for local development
set -e

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
ROOT_DIR="$(dirname "$SCRIPT_DIR")"

echo "==> Starting RabbitMQ..."
docker compose -f "$ROOT_DIR/docker-compose.yml" up -d rabbitmq

echo "==> Waiting for RabbitMQ to be ready..."
until docker compose -f "$ROOT_DIR/docker-compose.yml" exec rabbitmq rabbitmqctl status > /dev/null 2>&1; do
  sleep 2
done
echo "==> RabbitMQ is ready."

echo "==> Starting API (http://localhost:5006)..."
dotnet run --project "$ROOT_DIR/apps/backend/ChatApp.Api" &
API_PID=$!

echo "==> Starting Bot worker..."
dotnet run --project "$ROOT_DIR/apps/backend/ChatApp.Bot" &
BOT_PID=$!

echo "==> Starting Frontend (http://localhost:3000)..."
cd "$ROOT_DIR/apps/frontend" && npm start &
FRONTEND_PID=$!

echo ""
echo "============================================"
echo "  Chat App is running!"
echo "  Frontend: http://localhost:3000"
echo "  API:      http://localhost:5006"
echo "  RabbitMQ: http://localhost:15672"
echo "============================================"
echo ""

# Wait for any process to exit, then kill all
trap "kill $API_PID $BOT_PID $FRONTEND_PID 2>/dev/null; docker compose -f $ROOT_DIR/docker-compose.yml down" EXIT
wait
