Write-Host "🚀 Iniciando Setup do AgentSaaS..." -ForegroundColor Green

# 1. Verifica Dependências
if (-not (Get-Command docker -ErrorAction SilentlyContinue)) { Write-Error "Docker não instalado!"; exit }
if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) { Write-Error ".NET SDK não instalado!"; exit }

# 2. Sobe Infraestrutura
Write-Host "🐳 Subindo Docker Compose (Postgres, Redis, Seq)..." -ForegroundColor Cyan
docker-compose up -d postgres redis seq

Write-Host "⏳ Aguardando Banco de Dados ficar pronto..." -ForegroundColor Yellow
Start-Sleep -Seconds 10

# 3. Migrations e Seed
Write-Host "💾 Aplicando Migrations e Criando Admin..." -ForegroundColor Cyan
dotnet ef database update --project AgentSaaS.Infrastructure --startup-project AgentSaaS.Web

# 4. Build dos Agentes
Write-Host "🤖 Compilando Imagem do Agente..." -ForegroundColor Cyan
docker build -t agentsaas/runner:latest -f AgentSaaS.AgentRunner/Dockerfile .

# 5. Inicia Aplicação
Write-Host "✅ Setup Concluído!" -ForegroundColor Green
Write-Host "👉 WebApp: https://localhost:5001"
Write-Host "👉 Logs (Seq): http://localhost:5341"
Write-Host "👉 Login Admin: admin@agentsaas.com / Admin123!"