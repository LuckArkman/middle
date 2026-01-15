using IntelligentAutomation.Interfaces;
using IntelligentAutomation.Services;
using IntelligentAutomation.Domain.Entities;
using IntelligentAutomation.Infrastructure.Persistence;
using IntelligentAutomation.Orchestrator.Controllers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using Quartz;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Adiciona o serviço hospedado que inicia o agendador Quartz
builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);
BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));
builder.Services.Configure<MercadoPagoSettings>(builder.Configuration.GetSection("MercadoPagoSettings"));
builder.Services.AddScoped<IPaymentGatewayService, MercadoPagoService>();
builder.Services.AddSingleton<MongoDbContext>();
builder.Services.AddScoped<IQuotaService, QuotaService>();
builder.Services.AddScoped<IAgentSchedulingService, AgentSchedulingService>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"]
        };
    });
builder.Services.AddHttpClient<IContainerManagerService, ContainerManagerService>(client =>
{
    // O endereço base é o do API Gateway, mas em desenvolvimento podemos apontar direto
    // Em um ambiente de produção/kubernetes, isso seria o nome do serviço (ex: http://container-manager)
    client.BaseAddress = new Uri(builder.Configuration["Services:ContainerManagerUrl"] 
                                 ?? "http://localhost:5002"); // Porta padrão do ContainerManager
});

builder.Services.AddScoped<IMongoDatabase>(sp =>
{
    var client = sp.GetRequiredService<IMongoClient>();
    var databaseName = new MongoUrl(builder.Configuration.GetConnectionString("MongoDbConnection")).DatabaseName;
    return client.GetDatabase(databaseName);
});
builder.Services.AddScoped<IPasswordService, PasswordService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors();
app.UseRouting(); // 1. Habilita o roteamento

app.MapControllers();

app.Run();