using IntelligentAutomation.Interfaces;
using IntelligentAutomation.Domain.Entities;
using IntelligentAutomation.Domain.Enums;
using IntelligentAutomation.Infrastructure.Persistence;
using MercadoPago.Client.Payment;
using MercadoPago.Client.Preference;
using MercadoPago.Config;
using MercadoPago.Resource.Payment;
using MercadoPago.Resource.Preference;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace IntelligentAutomation.Services;

public class MercadoPagoService : IPaymentGatewayService
{
    private readonly ILogger<MercadoPagoService> _logger;
    private readonly MercadoPagoSettings _settings;
    private readonly MongoDbContext _db;

    public MercadoPagoService(
        IOptions<MercadoPagoSettings> settings,
        MongoDbContext db,
        ILogger<MercadoPagoService> logger)
    {
        _settings = settings.Value;
        _db = db;
        _logger = logger;
        MercadoPagoConfig.AccessToken = _settings.AccessToken;
    }

    public async Task<CreateCheckoutResponse> CreateCheckoutPreference(string userId, string planId, string successUrl, string failureUrl)
    {
        if (!Guid.TryParse(userId, out var userGuid) || !Guid.TryParse(planId, out var planGuid))
        {
            throw new Exception("IDs inválidos.");
        }

        var user = await _db.Users.Find(u => u.Id == userGuid).FirstOrDefaultAsync();
        var plan = await _db.Plans.Find(p => p.Id == planGuid).FirstOrDefaultAsync();

        if (user == null || plan == null)
            throw new Exception("Usuário ou Plano não encontrado.");

        var request = new PreferenceRequest
        {
            Items = new List<PreferenceItemRequest>
            {
                new PreferenceItemRequest
                {
                    Id = planId,
                    Title = $"Assinatura: {plan.Name}",
                    Quantity = 1,
                    CurrencyId = "BRL",
                    UnitPrice = plan.MonthlyPrice
                }
            },
            ExternalReference = userId,
            BackUrls = new PreferenceBackUrlsRequest
            {
                Success = successUrl,
                Failure = failureUrl,
                Pending = failureUrl
            },
            AutoReturn = "approved"
        };

        var client = new PreferenceClient();
        Preference preference = await client.CreateAsync(request);

        return new CreateCheckoutResponse
        {
            PreferenceId = preference.Id,
            CheckoutUrl = preference.InitPoint
        };
    }

    public async Task HandleWebhookEvent(WebhookNotification notification)
    {
        if (notification.Type != "payment") return;

        _logger.LogInformation("Recebido webhook de pagamento. ID: {PaymentId}", notification.Data.Id);

        try
        {
            var client = new PaymentClient();
            Payment payment = await client.GetAsync(long.Parse(notification.Data.Id));

            if (payment.Status == "approved")
            {
                var userIdStr = payment.ExternalReference;
                var planIdStr = payment.AdditionalInfo.Items.FirstOrDefault()?.Id;

                if (string.IsNullOrEmpty(userIdStr) || string.IsNullOrEmpty(planIdStr) ||
                    !Guid.TryParse(userIdStr, out var userId) || !Guid.TryParse(planIdStr, out var planId))
                {
                    _logger.LogError("Webhook inválido. UserId: {UserId}, PlanId: {PlanId}", userIdStr, planIdStr);
                    return;
                }

                // Verifica se já existe a assinatura para evitar duplicidade
                var existingSubscription = await _db.Subscriptions.Find(s => s.ExternalSubscriptionId == payment.Id.ToString()).FirstOrDefaultAsync();

                if (existingSubscription != null)
                {
                    _logger.LogWarning("Assinatura para o pagamento ID {PaymentId} já processada.", payment.Id);
                    return;
                }

                var user = await _db.Users.Find(u => u.Id == userId).FirstOrDefaultAsync();
                if (user == null) return;

                var newSubscription = new Subscription
                {
                    UserId = userId,
                    PlanId = planId,
                    Status = SubscriptionStatus.Active,
                    CurrentPeriodEnd = DateTime.UtcNow.AddMonths(1),
                    ExternalSubscriptionId = payment.Id.ToString(),
                    TenantId = user.TenantId // Herda o tenant do usuário
                };

                await _db.Subscriptions.InsertOneAsync(newSubscription);

                var update = Builders<User>.Update.Set(u => u.CurrentSubscriptionId, newSubscription.Id.ToString());
                await _db.Users.UpdateOneAsync(u => u.Id == userId, update);

                _logger.LogInformation("Assinatura ativada para Usuário {UserId} no Plano {PlanId}", userId, planId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar webhook MercadoPago.");
            throw;
        }
    }
}