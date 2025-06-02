# Fibonacci Application with Factorial Processing

A modern microservices-based application for calculating Fibonacci numbers with asynchronous factorial processing. Built with .NET 9.0, gRPC, RabbitMQ, Redis, and PostgreSQL.

## 🏗️ Architecture Overview

- **FibonacciBFF**: REST API Backend for Frontend (HTTP → gRPC)
- **Fibonacci.Calculator**: gRPC service with Redis cache and PostgreSQL persistence
- **FactorialCalculator**: Background service consuming RabbitMQ messages for factorial calculations
- **PostgreSQL**: Persistent storage for Fibonacci results
- **Redis**: High-performance caching layer
- **RabbitMQ**: Message broker for asynchronous processing

```
HTTP Request → BFF → gRPC → Fibonacci Calculator → Database/Cache
                                    ↓
                              RabbitMQ Message
                                    ↓
                           Factorial Calculator
```

## 📋 Prerequisites

### For Docker (Recommended)
- [Docker](https://www.docker.com/get-started)
- [Docker Compose](https://docs.docker.com/compose/install/)

### For Local Development
- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [PostgreSQL 15+](https://www.postgresql.org/download/)
- [Redis](https://redis.io/download)
- [RabbitMQ](https://www.rabbitmq.com/download.html) with management plugin

## 🚀 Quick Start

### Option 1: Docker Environment (Recommended)

1. **Clone and start all services:**
   ```bash
   git clone https://github.com/Dissolving-in-Eternity/Fibo.git
   cd FibonacciApp
   docker-compose up --build
   ```

2. **Test the application:**
   ```bash
   # Test Fibonacci calculation
   curl http://localhost:5100/api/fibonacci/10
   # Expected response: 55
   
   # Test with a new number to trigger factorial processing
   curl http://localhost:5100/api/fibonacci/15
   # Expected response: 610
   ```

3. **Monitor factorial processing:**
   ```bash
   # Watch factorial calculator logs in real-time
   docker logs fib-factorial --follow
   
   # Should see:
   # Processing message: {"Number":15,"FibonacciResult":610}
   # Deserialized message - Number: 15, FibonacciResult: 610
   # Calculated factorial of Fibonacci result 610 = [huge number]
   ```

4. **Access services:**
   - **API (Swagger)**: http://localhost:5100
   - **RabbitMQ Management**: http://localhost:15672 (guest/guest)

### Option 2: Local Development

#### Step 1: Setup Infrastructure
```bash
# Start PostgreSQL (create database 'fibdb') and run migrations with dotnet ef database update
# Start Redis on port 6379
# Start RabbitMQ with management plugin
```
I recommend running the whole thing in docker and then stopping apps fib-factorial, fib-grpc, fib-bff and running them via Visual Studio for debugging etc.

#### Step 2: Run Services (3 separate terminals)

**Terminal 1 - gRPC Service:**
```bash
cd Fibonacci.Calculator
dotnet run
# Runs on: http://localhost:5283
```

**Terminal 2 - BFF API:**
```bash
cd FibonacciBFF
dotnet run
# Runs on: http://localhost:5100
```

**Terminal 3 - Factorial Consumer:**
```bash
cd FactorialCalculator
dotnet run
# Listens for RabbitMQ messages
```

## 🧪 Testing Examples

### Basic Fibonacci Calculation
```bash
# Calculate Fibonacci(8) = 21
curl http://localhost:5100/api/fibonacci/8
# Response: 21

# Calculate Fibonacci(12) = 144  
curl http://localhost:5100/api/fibonacci/12
# Response: 144
```

### Testing Message Flow (New Calculations)
```bash
# Use a number not calculated before
curl http://localhost:5100/api/fibonacci/20
# Response: 6765

# Check factorial processing logs
docker logs fib-factorial --tail 10
# OR for local: check FactorialCalculator console output
```

### Expected Log Output
When testing with a new Fibonacci number, you should see:

**gRPC Service logs:**
```
info: Fibonacci.Calculator.Services.FibonacciService[0]
      Calculating Fibonacci(20)
info: Fibonacci.Calculator.Services.RabbitMqPublisher[0]
      Published message to exchange factorial.calculations with routing key fib.result
```

**Factorial Consumer logs:**
```
info: FactorialCalculator.FactorialConsumerService[0]
      Processing message: {"Number":20,"FibonacciResult":6765}
info: FactorialCalculator.FactorialConsumerService[0]
      Deserialized message - Number: 20, FibonacciResult: 6765
info: FactorialCalculator.FactorialConsumerService[0]
      Calculated factorial of Fibonacci result 6765 = [very large number]
```

## 📊 Monitoring & Debugging

### Docker Environment

**View all container logs:**
```bash
docker-compose logs -f
```

**Monitor specific services:**
```bash
# Factorial calculator (message processing)
docker logs fib-factorial --follow

# gRPC service (Fibonacci calculations)
docker logs fib-grpc --follow

# BFF API (HTTP requests)
docker logs fib-bff --follow
```

**Check container status:**
```bash
docker-compose ps
```

### RabbitMQ Management

1. **Access UI**: http://localhost:15672 (guest/guest)
2. **Check queue**: Look for `factorial.queue`
3. **Monitor messages**: Published/Delivered counters
4. **Verify exchange**: `factorial.calculations`

### Database & Cache

**PostgreSQL:**
```bash
# Connect to database
docker exec -it fib-postgres psql -U postgres -d fibdb

# Check Fibonacci results
SELECT * FROM "FibonacciResults" ORDER BY "Number";
```

**Redis:**
```bash
# Connect to Redis
docker exec -it fib-redis redis-cli

# Check cached values
KEYS fibonacci:*
GET fibonacci:10
```

## 🛠️ Configuration

### Docker Environment Variables

Services automatically use Docker hostnames:
- **RabbitMQ**: `fib-rabbitmq:5672`
- **PostgreSQL**: `fib-postgres:5432`
- **Redis**: `fib-redis:6379`
- **gRPC**: `fib-grpc:80`

### Local Development Settings

**Fibonacci.Calculator/appsettings.json:**
```json
{
  "ConnectionStrings": {
    "Postgres": "Host=localhost;Port=5432;Database=fibdb;Username=postgres;Password=postgres",
    "Redis": "localhost:6379"
  },
  "RabbitMq": {
    "HostName": "localhost",
    "Port": "5672",
    "UserName": "guest",
    "Password": "guest"
  }
}
```

**FibonacciBFF/appsettings.json:**
```json
{
  "GrpcServiceUrl": "http://localhost:5283"
}
```

## 🔧 API Reference

### REST Endpoints

**Calculate Fibonacci:**
```http
GET /api/fibonacci/{number}
```

**Examples:**
```bash
curl http://localhost:5100/api/fibonacci/5   # Returns: 5
curl http://localhost:5100/api/fibonacci/10  # Returns: 55
curl http://localhost:5100/api/fibonacci/15  # Returns: 610
```

**Swagger UI:** http://localhost:5100

### gRPC Service

**Service:** `FibonacciCalculator`  
**Method:** `Calculate(FibonacciRequest) → FibonacciReply`

## 📡 Message Flow Details

1. **HTTP Request** → BFF receives Fibonacci calculation request
2. **gRPC Call** → BFF calls Fibonacci.Calculator service
3. **Cache Check** → Redis cache lookup
4. **Database Check** → PostgreSQL lookup if cache miss
5. **Calculation** → Compute Fibonacci if not found
6. **Storage** → Save to PostgreSQL and Redis
7. **Message Publishing** → Publish to RabbitMQ for factorial processing
8. **Message Consumption** → FactorialCalculator processes factorial
9. **Response** → Return Fibonacci result to client

### RabbitMQ Configuration

- **Exchange**: `factorial.calculations` (direct)
- **Queue**: `factorial.queue`
- **Routing Key**: `fib.result`
- **Message Format**: `{"Number": 10, "FibonacciResult": 55}`

## 🐛 Troubleshooting

### Common Issues

**gRPC HTTP/2 Errors (Local):**
```
✅ Fixed: HTTP/2 unencrypted support enabled in both client and server
```

**RabbitMQ Management UI 431 Error:**
```bash
# Clear browser cache for localhost:15672
# Or use incognito/private browsing mode
```

**Factorial Consumer Not Receiving Messages:**
```bash
# Check if using cached results (no new calculations = no messages)
# Test with new Fibonacci numbers to trigger message publishing
curl http://localhost:5100/api/fibonacci/25  # New calculation
docker logs fib-factorial --tail 5           # Check processing
```

**Container Issues:**
```bash
# Restart all services
docker-compose down
docker-compose up --build

# Check container health
docker-compose ps
docker logs <container-name>
```

### Performance Notes

- **Small Fibonacci numbers (≤20)**: Fast factorial calculation
- **Large Fibonacci numbers (≥25)**: Factorial calculations can be very slow
- **Caching**: Subsequent requests for same numbers use cache (no message publishing)

## 🏗️ Development

### Project Structure
```
FibonacciApp/
├── docker-compose.yml          # Container orchestration
├── FibonacciBFF/              # REST API service
├── Fibonacci.Calculator/       # gRPC service
├── FactorialCalculator/       # RabbitMQ consumer
└── README.md                  # This file
```

### Tech Stack
- **.NET 9.0**: Core framework
- **gRPC**: Service-to-service communication
- **RabbitMQ**: Message queue (v7.1.2)
- **Redis**: Caching layer
- **PostgreSQL**: Data persistence
- **Docker**: Containerization

## 📝 License

This project is for educational/demonstration purposes.
