# OVERVIEW

Simple browser-based chat application using .NET core  
This application allows several users to talk in a chatroom and also to get stock quotes  
from an API using a specific command.

---
### Tech stack
##### Backend
		- .NET 10 (preview) — runtime and SDK                                                                                                           
		- ASP.NET Core                                           
		- SignalR                                                                                                 
		- ASP.NET Identity               
		- JWT Bearer
		- Entity Framework Core
		- SQLite
		- RabbitMQ.Client 7.2
		- CsvHelper 33.1

##### Frontend
		- Reactjs
		- React Router
		- @microsoft/signalr 10

##### Infrastructure
		- RabbitMQ 3
		- Docker / Docker Compose

##### Testing
		- xUnit 2.9
		- Moq 4.20

---

## Step-by-step setup
Make sure theres an instance of rabbitmq is running locally

### Option A: start-script (recommended for easy setup)
make sure docker daemon is running in your local

#### On mac:
1. Open up terminal
2. cd path-to-project-directory/scripts
3. Then run ./start-dev.sh

#### On Windows
1. Open up Bash terminal
2. cd path-to-project-directory/scripts
3. Then run ./start-dev.sh

Then open two browser tabs at http://localhost:3000, register two different users, and start chatting. Type `/stock=aapl.us` to test the bot.

---

### Option B: Local development

Open 3 separate terminals:

#### Terminal 1 — Start RabbitMQ
```bash
docker compose up rabbitmq
```

#### Terminal 2 — Start the API
```bash
cd apps/backend && dotnet run --project ChatApp.Api
```

#### Terminal 3 — Start the Bot
```bash
cd apps/backend && dotnet run --project ChatApp.Bot
```

#### Terminal 4 — Start the Frontend
```bash
cd apps/frontend && npm start
```

Then open two browser tabs at http://localhost:3000, register two different users, and start chatting. Type `/stock=aapl.us` to test the bot.

---

### Option C: Docker (full stack installer)
Make sure docker daemon is running in your local

```bash
docker compose up --build
```

This spins up RabbitMQ + API + Bot. Then start the frontend separately with `cd apps/frontend && npm start`.

---

## Folder Architecture

```text
chat-app/
│
├── apps/
│   ├── backend/
│   │   ├── ChatApp.Api/
│   │   │   ├── Controllers/
│   │   │   ├── Hubs/
│   │   │   ├── Messaging/
│   │   │   ├── Models/
│   │   │   ├── Services/
│   │   │   ├── Program.cs
│   │   │   └── appsettings.json
│   │
│   │   ├── ChatApp.Bot/
│   │   │   ├── Consumers/
│   │   │   ├── Program.cs
│   │   │   └── appsettings.json
│   │
│   │   ├── ChatApp.Domain/
│   │   │   ├── Entities/
│   │   │   └── Interfaces/
│   │
│   │   ├── ChatApp.Infrastructure/
│   │   │   ├── ExternalApis/
│   │   │   ├── Messaging/
│   │   │   └── Persistence/
│   │
│   │   ├── ChatApp.Tests/
│   │   │   ├── Bot/
│   │   │   └── Services/
│   │
│   │   ├── ChatApp.sln
│   │   ├── Dockerfile.api
│   │   └── Dockerfile.bot
│   │
│   └── frontend/
│       ├── public/
│       ├── src/
│       │   ├── api
│       │   ├── components
│       │   └── pages
│       ├── package.json
│       └── package-lock.json
│
├── scripts/
│   └── start-dev.sh
│
├── .gitignore
├── README.md
└── docker-compose.yml
```

---

## The Complete Message Flow

**Normal message:**  
User types "Hello" → React calls `hub.invoke('SendMessage', 1, 'Hello')` → API's ChatHub.SendMessage saves to SQLite → broadcasts  
via SignalR → all browsers in that room receive ReceiveMessage event → React appends to message list

**Stock command:**  
User types `/stock=aapl.us` → React calls `hub.invoke('SendMessage', 1, '/stock=aapl.us')` → API's ChatHub detects the command via  
regex → publishes `{StockCode: "aapl.us", ChatRoomId: 1}` to RabbitMQ stock_commands queue → nothing saved to DB, nothing sent back to user yet →  
Bot picks up message → calls `https://stooq.com/q/l/?s=aapl.us&f=sd2t2ohlcv&h&e=csv` → parses CSV → publishes `{Message: "AAPL.US quote is $93.42  
per share", ChatRoomId: 1}` to bot_responses queue → API's BotResponseConsumer picks it up → saves to DB with IsBot=true, Username="StockBot" →  
broadcasts via SignalR → all browsers see the green bot message
