# 🛒 Shopping Cart Microservices

![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?logo=dotnet) ![Kafka](https://img.shields.io/badge/Apache%20Kafka-2.3-231F20?logo=apachekafka) ![Redis](https://img.shields.io/badge/Redis-7.0-DC382D?logo=redis) ![PostgreSQL](https://img.shields.io/badge/PostgreSQL-15-4169E1?logo=postgresql) ![Docker](https://img.shields.io/badge/Docker-24.0-2496ED?logo=docker) ![ML.NET](https://img.shields.io/badge/ML.NET-3.0.1-512BD4?logo=dotnet)

A distributed, event-driven shopping cart system built with .NET 9, leveraging CQRS, Event Sourcing, and Microservices Architecture for scalability and resilience.

## 📋 Table of Contents

- Features
- System Architecture
- Services
- Project Structure
- Getting Started
- Configuration
- API Usage
- Authentication & Authorization
- Development
- Monitoring & Observability
- Troubleshooting
- Future Enhancements
- Contributing
- License

## ✨ Features

- **Event-Driven Architecture**: Powered by Apache Kafka for asynchronous communication.
- **CQRS Pattern**: Separates read and write models for optimized performance.
- **Event Sourcing**: Stores full history of cart changes in PostgreSQL.
- **Dockerized Deployment**: Simplifies setup and scaling with Docker Compose.
- **Real-time Updates**: Uses Redis for fast read model projections.
- **Fraud Detection**: Real-time fraud detection using ML.NET with unsupervised anomaly detection (Randomized PCA) to identify suspicious cart activities.
- **Payment Processing**: Asynchronous billing via dedicated service.
- **Notifications**: Supports email and SMS notifications for user events.
- **Authentication & Authorization**: JWT-based authentication with role-based access control (Customer, Admin).
- **Monitoring**: Integrated with OpenTelemetry, Prometheus, Grafana, Elasticsearch, and Kibana.

## 🏗️ System Architecture

```mermaid
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
```

## 📦 Services

| Service | Description | Port |
| --- | --- | --- |
| **Cart.API** | API gateway and cart management | 5105 |
| **Billing.Service** | Payment and billing processing | 5201 |
| **Fraud.Service** | Fraud detection using ML.NET anomaly detection | 5202 |
| **Notification.Service** | Email and SMS notifications | 5203 |
| **Cart.Denormalizer** | Read model projection for Redis | 5204 |

## 📁 Project Structure

```
proj/
├── src/
│   ├── Cart.API/                 # API gateway with authentication and cart handling
│   ├── Cart.Domain/              # Shared domain models and events
│   ├── Billing.Service/          # Payment processing
│   ├── Fraud.Service/            # Fraud detection with ML.NET model
│   ├── Fraud.ModelTrainer/       # Training ML.NET model for fraud detection
│   ├── Notification.Service/     # Notification handling
│   ├── Cart.Denormalizer/        # Read model denormalization
│   └── Shared.Kernel/            # Shared infrastructure (Kafka, logging)
└── infra/
    ├── docker-compose.yml        # Docker setup for services
    ├── grafana/                  # Grafana dashboards and provisioning
    ├── otel-collector/           # OpenTelemetry collector configuration
    ├── prometheus.yml            # Prometheus configuration
    └── tempo/                    # Tempo tracing configuration
```

## 🚀 Getting Started

### Prerequisites

- .NET 9 SDK
- Docker & Docker Compose
- Git

### Installation

1. **Clone the repository**:

   ```bash
   git clone https://github.com/sheikh-mohamadi/shopping-cart-microservices.git
   cd shopping-cart-microservices
   ```

2. **Start infrastructure**:

   ```bash
   docker-compose -f infra/docker-compose.yml up -d
   ```

3. **Build the solution**:

   ```bash
   dotnet restore
   dotnet build
   ```

4. **Train the ML.NET fraud detection model**:

   ```bash
   dotnet run --project src/Fraud.ModelTrainer
   ```

   This generates `fraudModel.zip` in the `Fraud.ModelTrainer` output directory. Copy it to `src/Fraud.Service`:

   ```bash
   cp src/Fraud.ModelTrainer/bin/Debug/net9.0/fraudModel.zip src/Fraud.Service/
   ```

5. **Run the services**:

   ```bash
   dotnet run --project src/Cart.API
   dotnet run --project src/Billing.Service
   dotnet run --project src/Fraud.Service
   dotnet run --project src/Notification.Service
   dotnet run --project src/Cart.Denormalizer
   ```

   Alternatively, run all services together:

   ```bash
   dotnet run --project src/ShoppingCart.sln
   ```

## ⚙️ Configuration

### Environment Variables

Create a `.env` file in the project root or set these variables:

```bash
KAFKA_BOOTSTRAP_SERVERS=localhost:9092
REDIS_CONNECTION_STRING=localhost:6379
POSTGRES_CONNECTION_STRING=Host=localhost;Database=shopping_cart;Username=postgres;Password=password
ASPNETCORE_ENVIRONMENT=Development
JWT_KEY=your-secure-jwt-key-here
```

> **Note**: Replace `your-secure-jwt-key-here` with a strong, unique key for JWT signing.

### AppSettings

Each service includes configuration files:

- `appsettings.json`: Base configuration.
- `appsettings.Development.json`: Development-specific settings.
- `appsettings.Docker.json`: Docker-specific settings.

### Fraud Detection Model

The `Fraud.Service` uses a pre-trained ML.NET model (`fraudModel.zip`) for anomaly detection. Ensure the model file is present in the `Fraud.Service` directory and included in the `.csproj`:

```xml
<ItemGroup>
    <Content Include="fraudModel.zip">
        <CopyToOutputDirectory>Always</CopyToOutputDirectory>
    </Content>
</ItemGroup>
```

## 📡 API Usage

All endpoints (except authentication) require a JWT token in the `Authorization` header.

### Authentication & Authorization

#### Register a User

```http
POST /api/auth/register
Content-Type: application/json

{
    "username": "user123",
    "password": "securePassword123",
    "role": "Customer"
}
```

#### Login

```http
POST /api/auth/login
Content-Type: application/json

{
    "username": "user123",
    "password": "securePassword123"
}
```

**Response**:

```json
{
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}
```

Include the token in subsequent requests:

```http
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Roles**:

- **Customer**: Can manage their cart (add/remove items).
- **Admin**: Can view cart events.

### Add Item to Cart

```http
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
```

### Remove Item from Cart

```http
DELETE /api/cart/{cartId}/items/{productId}
Content-Type: application/json
Authorization: Bearer {token}

{
    "userId": "user-123"
}
```

### Get Cart View

```http
GET /api/cart/view/{cartId}
Authorization: Bearer {token}
```

### Example Request

```bash
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
```

## 🔧 Development

### Running Services Individually

Run each service separately for development:

```bash
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
```

### Training the Fraud Detection Model

To retrain the ML.NET model for fraud detection:

```bash
dotnet run --project src/Fraud.ModelTrainer
```

Copy the generated `fraudModel.zip` to `src/Fraud.Service`:

```bash
cp src/Fraud.ModelTrainer/bin/Debug/net9.0/fraudModel.zip src/Fraud.Service/
```

### Database Migrations

Apply migrations for the Cart API:

```bash
dotnet ef migrations add InitialCreate --project src/Cart.API
dotnet ef database update --project src/Cart.API
```

## 📊 Monitoring & Observability

Access monitoring tools:

- **Grafana**: http://localhost:3000 (Dashboards: Prometheus Metrics, Elasticsearch Logs, Tempo Traces)
- **Prometheus**: http://localhost:9090
- **Kibana**: http://localhost:5601

Configuration files are located in `infra/`:

- `grafana/provisioning/`: Dashboard and datasource configurations.
- `otel-collector/`: OpenTelemetry collector settings.
- `prometheus.yml`: Prometheus scrape configurations.
- `tempo/`: Tempo tracing settings.

### Fraud Detection Metrics

The `Fraud.Service` logs fraud detection scores to OpenTelemetry. Monitor these in Grafana for insights into model performance.

## 🐛 Troubleshooting

### Common Issues

1. **Kafka Connection Issues**:

   - Ensure Kafka and Zookeeper containers are running (`docker ps`).
   - Verify `KAFKA_BOOTSTRAP_SERVERS` in `.env` or `appsettings.json`.

2. **Redis Connection Errors**:

   - Confirm Redis container is active.
   - Check `REDIS_CONNECTION_STRING` configuration.

3. **PostgreSQL Errors**:

   - Ensure PostgreSQL container is running.
   - Verify `POSTGRES_CONNECTION_STRING`.

4. **Authentication Errors**:

   - Ensure valid JWT token is provided in the `Authorization` header.
   - Check `Jwt:Key` in `appsettings.json` matches the signing key.

5. **OpenTelemetry Failures**:

   - Verify OTEL collector container is running.
   - Check `Otlp:Endpoint` in `appsettings.json`.

6. **Fraud Detection Model Errors**:

   - Ensure `fraudModel.zip` exists in `src/Fraud.Service/`.
   - Verify the model was trained using `Fraud.ModelTrainer` and copied correctly.
   - Check ML.NET dependencies in `Fraud.Service.csproj`:
     ```xml
     <ItemGroup>
         <PackageReference Include="Microsoft.ML" Version="3.0.1" />
         <PackageReference Include="Microsoft.ML.FastTree" Version="3.0.1" />
     </ItemGroup>
     ```

### Debugging

Enable detailed logging by setting Serilog to Debug in `appsettings.json`:

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Debug"
    }
  }
}
```
