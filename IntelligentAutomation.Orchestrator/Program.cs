using IntelligentAutomation.Application.Services;
using IntelligentAutomation.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Adicionar conexão com o Banco de Dados
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddHttpClient<IContainerManagerService, ContainerManagerService>(client =>
{
    // O endereço base é o do API Gateway, mas em desenvolvimento podemos apontar direto
    // Em um ambiente de produção/kubernetes, isso seria o nome do serviço (ex: http://container-manager)
    client.BaseAddress = new Uri(builder.Configuration["Services:ContainerManagerUrl"] 
                                 ?? "http://localhost:5002"); // Porta padrão do ContainerManager
});
// 2. Adicionar serviços de API
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

// ... (código existente do Program.cs)

// Adicione esta linha ANTES de 'builder.Services.AddControllers();'
// Configura o IHttpClientFactory para o ContainerManager

// 2. Adicionar serviços de API
app.MapControllers();

app.Run();