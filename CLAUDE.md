# Claude Code development guide

This is a local-first personal organizer. Backend: `backend/LifePilot.Api` (ASP.NET Core 8 minimal APIs, EF Core SQLite). Frontend: `frontend` (React 18, TypeScript, Vite).

Run backend: `dotnet run --project backend/LifePilot.Api/LifePilot.Api.csproj`
Run frontend: `cd frontend && npm install && npm run dev`

Keep the application offline-capable. Do not introduce paid APIs, telemetry, or cloud dependencies. Preserve SQLite local storage. Add tests and migrations when extending the schema. Never commit `.db` files or secrets. Suggested next features: recurring reminders, monthly category budgets, local notifications, password protection, CSV exports, and optional Ollama integration.
