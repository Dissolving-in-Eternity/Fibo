# PowerShell script to fix RabbitMQ Management UI issues
# Fixes 431 Request Header Fields Too Large error

Write-Host "🔧 Fixing RabbitMQ Management UI..." -ForegroundColor Yellow

# Stop and remove existing containers
Write-Host "📦 Stopping existing containers..." -ForegroundColor Blue
docker-compose down

# Remove RabbitMQ specific containers and volumes if they exist
Write-Host "🗑️ Cleaning up RabbitMQ containers and data..." -ForegroundColor Blue
docker stop fib-rabbitmq 2>$null
docker rm fib-rabbitmq 2>$null

# Remove RabbitMQ volume to start fresh
Write-Host "💾 Removing RabbitMQ data volume..." -ForegroundColor Blue
docker volume rm fibonacciapp_rabbitmq_data 2>$null

# Build and start containers
Write-Host "🚀 Starting fresh containers..." -ForegroundColor Green
docker-compose up --build -d

# Wait for services to be ready
Write-Host "⏳ Waiting for RabbitMQ to start (30 seconds)..." -ForegroundColor Yellow
Start-Sleep -Seconds 30

# Check RabbitMQ status
Write-Host "🔍 Checking RabbitMQ status..." -ForegroundColor Blue
docker logs fib-rabbitmq --tail 10

Write-Host ""
Write-Host "✅ Setup complete!" -ForegroundColor Green
Write-Host ""
Write-Host "🌐 Access RabbitMQ Management UI:" -ForegroundColor Cyan
Write-Host "   URL: http://localhost:15672" -ForegroundColor White
Write-Host "   Username: guest" -ForegroundColor White
Write-Host "   Password: guest" -ForegroundColor White
Write-Host ""
Write-Host "🧹 If you still get 431 errors, clear your browser:" -ForegroundColor Yellow
Write-Host "   1. Clear cookies and site data for localhost:15672" -ForegroundColor White
Write-Host "   2. Try incognito/private browsing mode" -ForegroundColor White
Write-Host "   3. Try a different browser" -ForegroundColor White
Write-Host ""
Write-Host "📊 Check container status:" -ForegroundColor Cyan
Write-Host "   docker ps" -ForegroundColor White
Write-Host "   docker logs fib-rabbitmq" -ForegroundColor White 