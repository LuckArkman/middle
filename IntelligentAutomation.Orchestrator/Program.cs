using IntelligentAutomation.Interfaces;
using IntelligentAutomation.Services;
using IntelligentAutomation.Domain.Entities;
using IntelligentAutomation.Infrastructure.Persistence;
using IntelligentAutomation.Orchestrator.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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
builder.Services.AddQuartz(q =>
{
});

BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));

// Multi-tenancy
builder.Services.AddScoped<ITenantService, TenantService>();
builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.Configure<MercadoPagoSettings>(builder.Configuration.GetSection("MercadoPagoSettings"));
builder.Services.AddScoped<IPaymentGatewayService, MercadoPagoService>();
builder.Services.AddSingleton<MongoDbContext>();
builder.Services.AddScoped<IQuotaService, QuotaService>();
builder.Services.AddScoped<IAgentLogService, AgentLogService>();
builder.Services.AddScoped<IAgentSchedulingService, AgentSchedulingService>();
builder.Services.AddScoped<IPasswordService, PasswordService>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? "MinhaChaveSuperSecreta123!")),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"]
        };
    });

builder.Services.AddHttpClient<IContainerManagerService, ContainerManagerService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:ContainerManagerUrl"] ?? "http://localhost:5002");
});

builder.Services.AddScoped<IMongoDatabase>(sp =>
{
    var mongoUrl = new MongoUrl(builder.Configuration.GetConnectionString("MongoDbConnection"));
    var client = new MongoClient(mongoUrl);
    return client.GetDatabase(mongoUrl.DatabaseName);
});

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

// Inicialização do Banco de Dados MongoDB
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<MongoDbContext>();
    await DbInitializer.Initialize(context);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");

app.UseMiddleware<TenantMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();