using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using IntelligentAutomation.Infrastructure.Persistence;
using IntelligentAutomation.Interfaces;
using IntelligentAutomation.Services;
using IntelligentAutomation.Domain.Entities;
using Microsoft.AspNetCore.Authentication.Cookies;
using MongoDB.Driver;
using Quartz;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddQuartz(q => { });
builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

MongoDB.Bson.Serialization.BsonSerializer.RegisterSerializer(new MongoDB.Bson.Serialization.Serializers.GuidSerializer(MongoDB.Bson.GuidRepresentation.Standard));

// Multi-tenancy
builder.Services.AddScoped<ITenantService, TenantService>();

// MongoDB
builder.Services.AddSingleton<MongoDbContext>();
builder.Services.AddScoped<IMongoDatabase>(sp =>
{
    var mongoUrl = new MongoUrl(builder.Configuration.GetConnectionString("MongoDbConnection"));
    var client = new MongoClient(mongoUrl);
    return client.GetDatabase(mongoUrl.DatabaseName);
});

// Business Services
builder.Services.AddScoped<IPasswordService, PasswordService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAgentLogService, AgentLogService>();
builder.Services.AddScoped<IPaymentGatewayService, MercadoPagoService>();
builder.Services.AddScoped<IAgentSchedulingService, AgentSchedulingService>();
builder.Services.AddScoped<IQuotaService, QuotaService>();

builder.Services.Configure<MercadoPagoSettings>(builder.Configuration.GetSection("MercadoPago"));

builder.Services.AddHttpClient<IContainerManagerService, ContainerManagerService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:ContainerManagerUrl"] ?? "http://localhost:5002");
});

// Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Tenant Middleware
app.UseMiddleware<IntelligentAutomation.Portal.Middleware.PortalTenantMiddleware>();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
