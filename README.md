🛒 Shopping Cart Microservices

A distributed, event-driven shopping cart system built with .NET 9, leveraging CQRS, Event Sourcing, and Microservices Architecture for scalability and resilience.
📋 Table of Contents

Features
System Architecture
Services
Project Structure
Getting Started
Configuration
API Usage
Authentication & Authorization
Development
Monitoring & Observability
Troubleshooting
Future Enhancements
Contributing
License

✨ Features

Event-Driven Architecture: Powered by Apache Kafka for asynchronous communication.
CQRS Pattern: Separates read and write models for optimized performance.
Event Sourcing: Stores full history of cart changes in PostgreSQL.
Dockerized Deployment: Simplifies setup and scaling with Docker Compose.
Real-time Updates: Uses Redis for fast read model projections.
Fraud Detection: Real-time analysis to prevent fraudulent activities.
Payment Processing: Asynchronous billing via dedicated service.
Notifications: Supports email and SMS notifications for user events.
Authentication & Authorization: JWT-based authentication with role-based access control (Customer, Admin).
Monitoring: Integrated with OpenTelemetry, Prometheus, Grafana, Elasticsearch, and Kibana.

🏗️ System Architecture
graph TB
    Client[Client] -->|JWT Auth| API[Cart API Gateway]
    API -->|Reads| Redis[(Redis - Read Model)]
    API -->|Publishes Events| Kafka[(Apache Kafka)]
    Kafka --> Billing[Billing Service]
    Kafka --> Fraud[Fraud Detection Service]
    Kafka --> Notifications[Notification Service]
    Kafka --> Denormalizer[Read Model Denormalizer]
    Denormalizer -->|Updates| Redis
    API -->|Stores Users| Postgres[(PostgreSQL - User Data)]
    
    classDef microservice fill:#e1f5fe;
    classDef database fill:#f3e5f5;
    classDef queue fill:#fff3e0;
    
    class Client,API,Billing,Fraud,Notifications,Denormalizer microservice;
    class Redis,Postgres database;
    class Kafka queue;

📦 Services



Service
Description
Port



Cart.API
API gateway and cart management
5105


Billing.Service
Payment and billing processing
5201


Fraud.Service
Fraud detection and prevention
5202


Notification.Service
Email and SMS notifications
5203


Cart.Denormalizer
Read model projection for Redis
5204


📁 Project Structure
proj/
├── src/
│   ├── Cart.API/                 # API gateway with authentication and cart handling
│   ├── Cart.Domain/              # Shared domain models and events
│   ├── Billing.Service/          # Payment processing
│   ├── Fraud.Service/            # Fraud detection
│   ├── Notification.Service/     # Notification handling
│   ├── Cart.Denormalizer/        # Read model denormalization
│   └── Shared.Kernel/            # Shared infrastructure (Kafka, logging)
└── infra/
    ├── docker-compose.yml        # Docker setup for services
    ├── grafana/                  # Grafana dashboards and provisioning
    ├── otel-collector/           # OpenTelemetry collector configuration
    ├── prometheus.yml            # Prometheus configuration
    └── tempo/                    # Tempo tracing configuration

🚀 Getting Started
Prerequisites

.NET 9 SDK
Docker & Docker Compose
Git

Installation

Clone the repository:
git clone https://github.com/sheikh-mohamadi/shopping-cart-microservices.git
cd shopping-cart-microservices


Start infrastructure:
docker-compose -f infra/docker-compose.yml up -d


Build and run the solution:
dotnet restore
dotnet build
dotnet run --project src/ShoppingCart.sln



⚙️ Configuration
Environment Variables
Create a .env file in the project root or set these variables:
KAFKA_BOOTSTRAP_SERVERS=localhost:9092
REDIS_CONNECTION_STRING=localhost:6379
POSTGRES_CONNECTION_STRING=Host=localhost;Database=shopping_cart;Username=postgres;Password=password
ASPNETCORE_ENVIRONMENT=Development
JWT_KEY=your-secure-jwt-key-here


Note: Replace your-secure-jwt-key-here with a strong, unique key for JWT signing.

AppSettings
Each service includes configuration files:

appsettings.json: Base configuration.
appsettings.Development.json: Development-specific settings.
appsettings.Docker.json: Docker-specific settings.

📡 API Usage
All endpoints (except authentication) require a JWT token in the Authorization header.
Authentication & Authorization
Register a User
POST /api/auth/register
Content-Type: application/json

{
    "username": "user123",
    "password": "securePassword123",
    "role": "Customer"
}

Login
POST /api/auth/login
Content-Type: application/json

{
    "username": "user123",
    "password": "securePassword123"
}

Response:
{
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}

Include the token in subsequent requests:
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...

Roles:

Customer: Can manage their cart (add/remove items).
Admin: Can view cart events.

Add Item to Cart
POST /api/cart/{cartId}/items
Content-Type: application/json
Authorization: Bearer {token}

{
    "userId": "user-123",
    "productId": "prod-456",
    "productName": "Gaming Laptop",
    "price": 25000000,
    "quantity": 1
}

Remove Item from Cart
DELETE /api/cart/{cartId}/items/{productId}
Content-Type: application/json
Authorization: Bearer {token}

{
    "userId": "user-123"
}

Get Cart View
GET /api/cart/view/{cartId}
Authorization: Bearer {token}

Example Request
curl -X POST "http://localhost:5105/api/cart/123e4567-e89b-12d3-a456-426614174000/items" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..." \
  -d '{
    "userId": "user-123",
    "productId": "prod-456",
    "productName": "Gaming Laptop",
    "price": 25000000,
    "quantity": 1
  }'

🔧 Development
Running Services Individually
Run each service separately for development:
# Cart API
dotnet run --project src/Cart.API/

# Billing Service
dotnet run --project src/Billing.Service/

# Fraud Service
dotnet run --project src/Fraud.Service/

# Notification Service
dotnet run --project src/Notification.Service/

# Denormalizer
dotnet run --project src/Cart.Denormalizer/

Database Migrations
Apply migrations for the Cart API:
dotnet ef migrations add InitialCreate --project src/Cart.API
dotnet ef database update --project src/Cart.API

📊 Monitoring & Observability
Access monitoring tools:

Grafana: http://localhost:3000 (Dashboards: Prometheus Metrics, Elasticsearch Logs, Tempo Traces)
Prometheus: http://localhost:9090
Kibana: http://localhost:5601

Configuration files are located in infra/:

grafana/provisioning/: Dashboard and datasource configurations.
otel-collector/: OpenTelemetry collector settings.
prometheus.yml: Prometheus scrape configurations.
tempo/: Tempo tracing settings.

🐛 Troubleshooting
Common Issues

Kafka Connection Issues:

Ensure Kafka and Zookeeper containers are running (docker ps).
Verify KAFKA_BOOTSTRAP_SERVERS in .env or appsettings.json.


Redis Connection Errors:

Confirm Redis container is active.
Check REDIS_CONNECTION_STRING configuration.


PostgreSQL Errors:

Ensure PostgreSQL container is running.
Verify POSTGRES_CONNECTION_STRING.


Authentication Errors:

Ensure valid JWT token is provided in the Authorization header.
Check Jwt:Key in appsettings.json matches the signing key.


OpenTelemetry Failures:

Verify OTEL collector container is running.
Check Otlp:Endpoint in appsettings.json.



Debugging
Enable detailed logging by setting Serilog to Debug in appsettings.json:
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Debug"
    }
  }
}

🔮 Future Enhancements

 Payment gateway integration (e.g., Stripe, PayPal)
 Advanced fraud detection with machine learning
 Customizable email/SMS notification templates
 Load testing suite for performance optimization
 Enhanced Grafana dashboards for deeper insights
