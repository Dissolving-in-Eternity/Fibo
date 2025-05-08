For running in docker: No code changes nessesary, just clone the repo and run:

# 1. Make sure you're in the project root (where docker-compose.yml lives)
cd C:\Projects\FibonacciApp

# 2. Build all services from scratch (no cache)
docker-compose build --no-cache

# 3. Start up everything (builds if missing, creates fresh containers)
docker-compose up -d

 


For running LOCALLY & debugging:
In Fibonacci.Calculator:
Go to appsettings.json and comment out 

"ConnectionStrings": {
    "Postgres": "Host=fib-postgres;Port=5432;Database=fibdb;Username=postgres;Password=postgres",
    "Redis": "fib-redis:6379"
}

Uncomment

//"ConnectionStrings": {
//    "Postgres": "Host=localhost;Port=5432;Database=fibdb;Username=postgres;Password=postgres",
//    "Redis": "localhost:6379"
//}


In FibonacciBFF:
Go to Program.cs
Comment out

o.Address = new Uri("http://fib-grpc:80");  // for docker - use the container name as the hostname

And uncomment:

//o.Address = new Uri("http://localhost:5000"); // for local

Set BFF as startup project and run it from VS
