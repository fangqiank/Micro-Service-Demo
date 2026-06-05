# MicroServiceDemo / 微服务演示

A .NET 10 microservices demo with Kubernetes deployment, demonstrating synchronous (HTTP, gRPC) and asynchronous (RabbitMQ) inter-service communication.

![Architecture](MicroServiceDemo-architecture.svg)

## Tech Stack / 技术栈

| Category | Technology |
|----------|-----------|
| Runtime | .NET 10, ASP.NET Core |
| Database | PostgreSQL 16 (PlatformService), InMemory (CommandService) |
| ORM | EF Core 10 + Npgsql |
| gRPC | Grpc.AspNetCore 2.80 |
| Message Bus | RabbitMQ 3 (fanout exchange) |
| Mapping | AutoMapper 16 |
| API Docs | Swashbuckle.AspNetCore 10 |
| Container | Docker, Kubernetes (Docker Desktop) |
| Ingress | NGINX Ingress Controller |

## Modules / 模块

| Service | Project | Responsibility |
|---------|---------|---------------|
| PlatformService | `PlatformService/` | CRUD for platforms, publishes changes to CommandService via HTTP + RabbitMQ, exposes gRPC server |
| CommandService | `CommandService/` | CRUD for commands per platform, receives platforms via RabbitMQ + gRPC client |

## Data Flow / 数据流

1. **Create Platform**: `POST /api/platform` → PlatformService saves to PostgreSQL → HTTP POST sync to CommandService → RabbitMQ async publish → CommandService creates local Platform
2. **Startup Sync**: CommandService starts → gRPC call to PlatformService:666 → seeds all existing platforms (with retry)
3. **External Access**: NGINX Ingress routes `acme.com/api/platform` → PlatformService, `acme.com/api/cmd/platform` → CommandService

```
External Client
      │
      ▼
 NGINX Ingress (acme.com)
   ┌────┴────┐
   ▼         ▼
PlatformService ──HTTP POST──► CommandService
      │                              ▲
      ├──RabbitMQ (fanout)───────────┘
      │                              │
      └──gRPC Server :666     gRPC Client (startup)
      │                              │
 PostgreSQL 16                  InMemory DB
```

## Quick Start / 快速开始

### Local Development

```bash
# PlatformService (https://localhost:5001)
cd PlatformService && dotnet run

# CommandService (https://localhost:6001)
cd CommandService && dotnet run
```

Requires local PostgreSQL (dev uses InMemory) and RabbitMQ.

### Build Docker Images

```bash
cd PlatformService && docker build -t huamu/platformservice:latest .
cd CommandService && docker build -t huamu/commandservice:latest .
```

### Deploy to Kubernetes

```bash
# 1. Storage
kubectl apply -f K8S/local-pvc.yaml

# 2. PostgreSQL
kubectl create secret generic postgres --from-literal=POSTGRES_PASSWORD="pa55w0rd!"
kubectl apply -f K8S/postgres-depl.yaml

# 3. RabbitMQ
kubectl apply -f K8S/rabbitmq-depl.yaml

# 4. Services
kubectl apply -f K8S/platforms-depl.yaml
kubectl apply -f K8S/platforms-np-srv.yaml
kubectl apply -f K8S/commands-depl.yaml

# 5. Ingress
kubectl apply -f https://raw.githubusercontent.com/kubernetes/ingress-nginx/main/deploy/static/provider/cloud/deploy.yaml
kubectl apply -f K8S/ingress-srv.yaml
```

### EF Core Migrations (PlatformService)

```bash
cd PlatformService
dotnet ef migrations add <Name>
dotnet ef database update
```

Migrations apply automatically in Production environment via `PrepDb`.

> **Note**: `DesignTimeDbContextFactory` uses `POSTGRES_PASSWORD` env var (falls back to dev default). In K8S Production, this password is injected via the `postgres` secret — the PlatformService Startup substitutes the `PA55W0RD_PLACEHOLDER` token in the connection string at runtime.

## API Endpoints

### PlatformService

| Method | Route | Description |
|--------|-------|-------------|
| GET | `/api/platform` | Get all platforms |
| GET | `/api/platform/{id}` | Get platform by ID |
| POST | `/api/platform` | Create platform (triggers sync + async publish) |

### CommandService

| Method | Route | Description |
|--------|-------|-------------|
| GET | `/api/cmd/platform` | Get all platforms |
| POST | `/api/cmd/platform` | Test inbound connection |
| GET | `/api/cmd/platforms/{platformId}/commands` | Get commands for a platform |
| GET | `/api/cmd/platforms/{platformId}/commands/{commandId}` | Get specific command |
| POST | `/api/cmd/platforms/{platformId}/commands` | Create command for a platform |

### gRPC

| Service | Method | Description |
|---------|--------|-------------|
| GrpcPlatform | `GetAllPlatforms` | Returns all platforms (PlatformService:666) |

## Example Requests

```bash
# Create a platform
curl -X POST http://acme.com/api/platform \
  -H "Content-Type: application/json" \
  -d '{"name":"Kubernetes","publisher":"CNCF","cost":"Free"}'

# Get all platforms
curl http://acme.com/api/platform

# Create a command for platform 1
curl -X POST http://acme.com/api/cmd/platforms/1/commands \
  -H "Content-Type: application/json" \
  -d '{"howTo":"Deploy to cluster","commandLine":"kubectl apply -f deployment.yaml"}'
```

## K8S Resources

| File | Resources |
|------|----------|
| `K8S/local-pvc.yaml` | PVC for PostgreSQL (200Mi, RWO) |
| `K8S/postgres-depl.yaml` | PostgreSQL 16 Deployment + ClusterIP + LoadBalancer |
| `K8S/rabbitmq-depl.yaml` | RabbitMQ 3 Management Deployment + ClusterIP + LoadBalancer |
| `K8S/platforms-depl.yaml` | PlatformService Deployment + ClusterIP (80 + 666 gRPC) |
| `K8S/platforms-np-srv.yaml` | NodePort for external PlatformService access |
| `K8S/commands-depl.yaml` | CommandService Deployment + ClusterIP |
| `K8S/ingress-srv.yaml` | NGINX Ingress routing rules |
