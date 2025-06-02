# PowerShell script to fix RabbitMQ Management UI access issues
# Specifically addresses 431 Request Header Fields Too Large error

Write-Host "RabbitMQ Management UI Fix Script" -ForegroundColor Cyan
Write-Host "==================================" -ForegroundColor Cyan
Write-Host ""

# Check if RabbitMQ container is running
$rabbitContainer = docker ps --filter "name=fib-rabbitmq" --format "table {{.Names}}\t{{.Status}}" | Select-String "fib-rabbitmq"

if ($rabbitContainer) {
    Write-Host "✅ RabbitMQ container is running: $rabbitContainer" -ForegroundColor Green
} else {
    Write-Host "❌ RabbitMQ container is not running!" -ForegroundColor Red
    Write-Host "Starting containers..." -ForegroundColor Yellow
    docker-compose up -d
    Write-Host "Waiting 15 seconds for RabbitMQ to start..." -ForegroundColor Yellow
    Start-Sleep -Seconds 15
}

Write-Host ""
Write-Host "RabbitMQ Management UI Information:" -ForegroundColor Cyan
Write-Host "   URL: http://localhost:15672" -ForegroundColor White
Write-Host "   Username: guest" -ForegroundColor White
Write-Host "   Password: guest" -ForegroundColor White
Write-Host ""

Write-Host "If you get 431 'Request Header Fields Too Large' error:" -ForegroundColor Yellow
Write-Host ""
Write-Host "1. Clear Browser Data for localhost:15672:" -ForegroundColor White
Write-Host "   - Chrome: Settings > Privacy > Clear browsing data > Cookies and site data" -ForegroundColor Gray
Write-Host "   - Firefox: Settings > Privacy > Clear Data > Cookies and Site Data" -ForegroundColor Gray
Write-Host "   - Edge: Settings > Privacy > Choose what to clear > Cookies and website data" -ForegroundColor Gray
Write-Host ""
Write-Host "2. Try Incognito/Private Browsing mode" -ForegroundColor White
Write-Host ""
Write-Host "3. Try a different browser" -ForegroundColor White
Write-Host ""
Write-Host "4. Hard refresh the page (Ctrl+F5 or Ctrl+Shift+R)" -ForegroundColor White
Write-Host ""

Write-Host "Container Status:" -ForegroundColor Cyan
docker ps --filter "name=fib-" --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"

Write-Host ""
Write-Host "Recent RabbitMQ Logs:" -ForegroundColor Cyan
docker logs fib-rabbitmq --tail 3

Write-Host ""
Write-Host "Additional Troubleshooting:" -ForegroundColor Yellow
Write-Host "   - Check container logs: docker logs fib-rabbitmq" -ForegroundColor Gray
Write-Host "   - Restart containers: docker-compose restart rabbitmq" -ForegroundColor Gray
Write-Host "   - Reset everything: docker-compose down && docker-compose up -d" -ForegroundColor Gray 