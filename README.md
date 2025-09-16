Shopping Cart Microservices

A distributed, event-driven shopping cart system built with .NET 9 using CQRS, Event Sourcing, and Microservices Architecture.

Features:

- Event-Driven Architecture - Powered by Apache Kafka
- CQRS Pattern - Separate read/write models
- Event Sourcing - Full history of all cart changes
- Dockerized - Easy deployment & scaling
- Real-time Updates - Via Redis-based read model
- Fraud Detection - Real-time prevention system
- Payment Processing - Asynchronous billing handling
- Notifications - Email & SMS support
- Monitoring - OpenTelemetry, Prometheus, and Grafana integration

System Architecture:

- Cart API Gateway -> Apache Kafka
- Apache Kafka -> Billing Service
- Apache Kafka -> Fraud Detection Service
- Apache Kafka -> Notification Service
- Apache Kafka -> Read Model Denormalizer -> Redis (Read Model)
- Cart API Gateway -> Redis

Services:

- Cart.API: Main gateway & cart handling, Port 5105
- Billing.Service: Billing & payment processing, Port 5201
- Fraud.Service: Fraud detection & prevention, Port 5202
- Notification.Service: Email & SMS notifications, Port 5203
- Cart.Denormalizer: Read model projection, Port 5204

Project Structure:

proj/
├── src/
│   ├── Cart.API/                 # Main API gateway
│   ├── Cart.Domain/              # Shared domain models
│   ├── Billing.Service/          # Payment processing
│   ├── Fraud.Service/            # Fraud detection
│   ├── Notification.Service/     # Notifications
│   ├── Cart.Denormalizer/        # Read model denormalizer
│   └── Shared.Kernel/            # Shared infrastructure
└── infra/
    ├── docker-compose.yml        # Docker setup
    └── monitoring configs        # OTEL, Prometheus, Grafana

Getting Started:

Prerequisites:
- .NET 9 SDK
- Docker & Docker Compose

Installation:
1. Clone repository:
   git clone <repository-url>
   cd shopping-cart-microservices
2. Start infrastructure:
   docker-compose -f infra/docker-compose.yml up -d
3. Build and run:
   dotnet restore
   dotnet build
   dotnet run --project src/ShoppingCart.sln

Configuration:

Environment Variables:
- KAFKA_BOOTSTRAP_SERVERS=localhost:9092
- REDIS_CONNECTION_STRING=localhost:6379
- POSTGRES_CONNECTION_STRING=Host=localhost;Database=shoppingcart;Username=postgres;Password=password
- ASPNETCORE_ENVIRONMENT=Development

AppSettings:
- appsettings.json - Base configuration
- appsettings.Development.json - Development settings
- appsettings.Docker.json - Docker-specific settings

API Usage:

Add Item to Cart:
POST /api/cart/{cartId}/items
Content-Type: application/json
{
    "userId": "user-123",
    "productId": "prod-456",
    "productName": "Gaming Laptop",
    "price": 25000000,
    "quantity": 1
}

Remove Item from Cart:
DELETE /api/cart/{cartId}/items/{productId}

Get Cart View:
GET /api/cart/view/{cartId}

Example Request:
curl -X POST "http://localhost:5105/api/cart/123e4567-e89b-12d3-a456-426614174000/items" \
  -H "Content-Type: application/json" \
  -d '{
    "userId": "user-123",
    "productId": "prod-456",
    "productName": "Gaming Laptop",
    "price": 25000000,
    "quantity": 1
  }'

Development:

Running Services Individually:
- Cart API: dotnet run --project src/Cart.API/
- Billing Service: dotnet run --project src/Billing.Service/
- Fraud Service: dotnet run --project src/Fraud.Service/
- Notification Service: dotnet run --project src/Notification.Service/
- Denormalizer: dotnet run --project src/Cart.Denormalizer/

Monitoring:
- Grafana: http://localhost:3000
- Prometheus: http://localhost:9090
- Kibana: http://localhost:5601
