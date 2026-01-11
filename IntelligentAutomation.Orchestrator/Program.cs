using IntelligentAutomation.Application.Interfaces;
using IntelligentAutomation.Application.Services;
using IntelligentAutomation.Domain.Entities;
using IntelligentAutomation.Infrastructure.Persistence;
using IntelligentAutomation.Orchestrator.Services;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using Quartz;

var builder = WebApplication.CreateBuilder(args);


// --- Início da Configuração do Quartz.NET ---
builder.Services.AddQuartz(q =>
{
    // O JobStore em memória é bom para desenvolvimento. Em produção, usaríamos um JobStore com banco de dados (JDBC) para persistência.
    q.UseMicrosoftDependencyInjectionJobFactory();
});

// Adiciona o serviço hospedado que inicia o agendador Quartz
builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);
BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));
builder.Services.Configure<MercadoPagoSettings>(builder.Configuration.GetSection("MercadoPagoSettings"));
builder.Services.AddScoped<IPaymentGatewayService, MercadoPagoService>();
builder.Services.AddSingleton<MongoDbContext>();
builder.Services.AddScoped<IQuotaService, QuotaService>();
builder.Services.AddScoped<IAgentSchedulingService, AgentSchedulingService>();
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