# LifePilot — Local Personal Dashboard

An open-source-ready personal life organizer built with **C# / ASP.NET Core 8**, **EF Core + SQLite**, and **React + TypeScript + Vite**. No paid APIs, accounts, or cloud services required.

## Requirements
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js 20+](https://nodejs.org/)
- VS Code (recommended)

## Run locally in VS Code
1. Extract the project and open the `LifePilot` folder in VS Code.
2. Open **Terminal 1** and run:
   ```bash
   dotnet restore backend/LifePilot.Api/LifePilot.Api.csproj
   dotnet run --project backend/LifePilot.Api/LifePilot.Api.csproj
   ```
3. Open **Terminal 2** and run:
   ```bash
   cd frontend
   npm install
   npm run dev
   ```
4. Open **http://localhost:5173**. The backend runs on **http://localhost:5080**.
5. SQLite database is created automatically at the backend working directory as `lifepilot.db`. Database files are ignored by Git.

## Features
- Dashboard: open tasks, monthly expenses, grocery count, due/overdue items
- Tasks and reminders (date, completion)
- Expense logging and monthly totals
- Grocery list and completion
- Calendar event tracking
- Document **expiry reminders** (metadata only; no file upload/storage)
- Rule-based local command assistant (no LLM needed)
- JSON export and import (import **adds** records)
- Responsive frontend, local SQLite persistence

## Assistant commands
- `add milk to groceries`
- `remind me to call mom`
- `spent 12.50 on lunch`

## API
`GET /api/health`, `GET /api/items`, `POST /api/items`, `PUT /api/items/{id}`, `DELETE /api/items/{id}`, `GET /api/backup`, `POST /api/restore`, `POST /api/assistant`.

## GitHub
```bash
git init
git add .
git commit -m "Initial LifePilot full-stack application"
git branch -M main
git remote add origin https://github.com/YOUR_USERNAME/LifePilot.git
git push -u origin main
```
Create an empty GitHub repository first and replace YOUR_USERNAME.

## Scope / security
This is a **single-user, local development application**. There is no authentication, encryption, actual document upload, notification scheduler, or LLM. Do **not** expose port 5080 to the public internet or store sensitive documents. Review and harden the project before production deployment.

## Development verification
Frontend TypeScript build can be checked with `npm run build`. Backend requires .NET 8 SDK to compile and run.
