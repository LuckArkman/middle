using IntelligentAutomation.Interfaces;
using IntelligentAutomation.Services;
using IntelligentAutomation.Domain.Entities;
using IntelligentAutomation.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using Quartz;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// --- Início da Configuração do Quartz.NET ---
builder.Services.AddQuartz(q =>
{
    // O JobStore em memória é bom para desenvolvimento. Em produção, usaríamos um JobStore com banco de dados (JDBC) para persistência.
});

// Adiciona o serviço hospedado que inicia o agendador Quartz
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
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? "SecretKeyVeryLong1234567890")),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"]
        };
    });

builder.Services.AddScoped<IMongoDatabase>(sp =>
{
    var client = sp.GetRequiredService<IMongoClient>();
    var databaseName = new MongoUrl(builder.Configuration.GetConnectionString("MongoDbConnection")).DatabaseName;
    return client.GetDatabase(databaseName);
});
builder.Services.AddScoped<IPasswordService, PasswordService>();

builder.Services.AddControllers();
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

app.UseRouting();  // Primeiro: Habilita o roteamento

app.UseCors("AllowAll");  // Segundo: Aplica a policy "AllowAll"

app.UseAuthentication();  // Terceiro: Autenticação
app.UseAuthorization();   // Quarto: Autorização


app.MapControllers();  // Último: Mapeia os controllers

app.Run();