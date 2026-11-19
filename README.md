# Pest Control Contract Intelligence

Pest Control Contract Intelligence is a Windows desktop client and centralized
ASP.NET Core backend for securely searching and asking questions about company
contracts.

## Architecture

- **Domain** contains business entities and invariants.
- **Application** contains use cases, DTOs and provider-neutral ports.
- **Infrastructure** implements persistence, storage, document processing and AI adapters.
- **Api** is the authenticated HTTPS trust boundary for desktop clients.
- **Worker** owns durable, long-running document ingestion.
- **Desktop** is the WPF client and communicates only with the API.

The initial implementation is intentionally a modular monolith. SQL Server,
server-side document storage and AI providers are not exposed to desktop clients.

## Prerequisites

- Windows
- .NET 10 SDK
- SQL Server for persistence milestones
- An approved server-side contract folder for ingestion milestones

## Build and test

```powershell
dotnet restore
dotnet build -c Release
dotnet test -c Release
```

Feature status is tracked in [docs/BUILD_STATUS.md](docs/BUILD_STATUS.md).
The implementation sequence is documented outside the repository in the
project build plan.

## Security baseline

Do not commit credentials, API keys, production configuration or customer
contracts. Desktop clients must never connect directly to SQL Server or a
shared document folder.
