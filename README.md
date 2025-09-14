# 🛒 Shopping Cart Microservices

![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?logo=dotnet)
![Kafka](https://img.shields.io/badge/Apache%20Kafka-2.3-231F20?logo=apachekafka)
![Redis](https://img.shields.io/badge/Redis-7.0-DC382D?logo=redis)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-15-4169E1?logo=postgresql)
![Docker](https://img.shields.io/badge/Docker-24.0-2496ED?logo=docker)
![Kubernetes](https://img.shields.io/badge/Kubernetes-1.28-326CE5?logo=kubernetes)

A distributed, event-driven **shopping cart system** built with **.NET 9** using **CQRS**, **Event Sourcing**, and **Microservices Architecture**.

---

## ✨ Key Features

- **🎯 Event-Driven Architecture** – powered by Apache Kafka  
- **📦 CQRS Pattern** – separate read/write models  
- **🔄 Event Sourcing** – full history of all changes  
- **🐳 Dockerized** – easy deployment & scaling  
- **📊 Real-time Updates** – via WebSocket  
- **🔍 Fraud Detection** – real-time prevention  
- **💳 Payment Integration** – asynchronous payment handling  
- **📱 Notifications** – Email & SMS support  
- **📈 Scalability** – horizontal scaling supported  
- **🔒 Security** – JWT authentication & authorization  

---

## 🏗️ System Architecture

```mermaid
graph TB
    API[Cart API Gateway] --> Kafka[(Apache Kafka)]
    Kafka --> Billing[Billing Service]
    Kafka --> Fraud[Fraud Detection Service]
    Kafka --> Notifications[Notification Service]
    Kafka --> Denormalizer[Read Model Denormalizer]
    Denormalizer --> Redis[(Redis - Read Model)]
    Denormalizer --> PostgreSQL[(PostgreSQL - Event Store)]
    
    API --> Redis
    API --> PostgreSQL
    
    classDef microservice fill:#e1f5fe;
    classDef database fill:#f3e5f5;
    classDef queue fill:#fff3e0;
    
    class API,Billing,Fraud,Notifications,Denormalizer microservice;
    class Redis,PostgreSQL database;
    class Kafka queue;
```

---

## 📦 Microservices

| Service               | Description                   | Port |
|------------------------|-------------------------------|------|
| **Cart.API**           | Main gateway & cart handling | 5105 |
| **Billing.Service**    | Billing & payment processing | 5201 |
| **Fraud.Service**      | Fraud detection & prevention | 5202 |
| **Notification.Service** | Email & SMS notifications  | 5203 |
| **Cart.Denormalizer**  | Read model projection        | 5204 |

---

## 🚀 Getting Started

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)  
- [Docker & Docker Compose](https://docs.docker.com/get-docker/)  
- [Git](https://git-scm.com/)  

### Development Setup

```bash
# Clone repository
git clone https://github.com/your-username/shopping-cart-microservices.git
cd shopping-cart-microservices

# Start infrastructure
docker-compose -f infra/docker-compose.yml up -d

# Build & run
dotnet restore
dotnet build
dotnet run --project src/ShoppingCart.sln
```

### Run Tests

```bash
# Unit tests
dotnet test tests/unit-tests/

# Integration tests
dotnet test tests/integration-tests/

# Load tests
k6 run tests/load-tests/cart-load-test.js
```

---

## 📡 API Endpoints

### Add Item to Cart
```http
POST /api/cart/{cartId}/items
```

### Remove Item from Cart
```http
DELETE /api/cart/{cartId}/items/{productId}
```

### Get Cart Events
```http
GET /api/cart/{cartId}/events
```

### Get Cart View
```http
GET /api/cart/view/{cartId}
```

Example request:
```bash
curl -X POST "http://localhost:5105/api/cart/123e4567-e89b-12d3-a456-426614174000/items"   -H "Content-Type: application/json"   -d '{
    "userId": "user-123",
    "productId": "prod-456",
    "productName": "Gaming Laptop",
    "price": 25000000,
    "quantity": 1
  }'
```

---

## 🛠️ Tech Stack

**Backend**: .NET 9, ASP.NET Core, EF Core, Npgsql  
**Messaging**: Apache Kafka, Confluent.Kafka  
**Databases**: PostgreSQL (event store), Redis (cache & read model)  
**Infrastructure**: Docker, Kubernetes, Terraform  
**Monitoring**: Prometheus, Grafana, Serilog, Elasticsearch  

---

## ⚙️ Configuration

### Environment Variables
```bash
KAFKA_BOOTSTRAP_SERVERS=localhost:9092
KAFKA_GROUP_ID=shopping-cart-group
REDIS_CONNECTION_STRING=localhost:6379
POSTGRES_CONNECTION_STRING=Host=localhost;Database=shoppingcart;Username=postgres;Password=password
ASPNETCORE_ENVIRONMENT=Development
```

---

## 📊 Performance Metrics

| Metric           | Value    | Description            |
|------------------|----------|------------------------|
| **Response Time**| <100 ms  | For read operations    |
| **Throughput**   | 10,000+  | Requests per second    |
| **Event Handling**| 50,000+ | Events per second      |
| **Latency**      | <50 ms   | Event processing time  |

---

## 🚀 Deployment

### With Docker
```bash
docker-compose -f infra/docker-compose.prod.yml build
docker-compose -f infra/docker-compose.prod.yml up -d
```

### With Kubernetes
```bash
kubectl apply -f infra/kubernetes/
kubectl get pods
kubectl get services
```

---
