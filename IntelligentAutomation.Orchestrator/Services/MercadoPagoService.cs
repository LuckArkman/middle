using IntelligentAutomation.Application.Interfaces;
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

namespace IntelligentAutomation.Orchestrator.Services;

public class MercadoPagoService : IPaymentGatewayService
{
    private readonly ILogger<MercadoPagoService> _logger;
    private readonly MercadoPagoSettings _settings;
    private readonly IMongoCollection<User> _users;
    private readonly IMongoCollection<Plan> _plans;
    private readonly IMongoCollection<Subscription> _subscriptions;

    public MercadoPagoService(
        IOptions<MercadoPagoSettings> settings,
        MongoDbContext mongoContext,
        ILogger<MercadoPagoService> logger) // CORREÇÃO: Logger injetado corretamente
    {
        _settings = settings.Value;
        _users = mongoContext.Users;
        _plans = mongoContext.Plans;
        _subscriptions = mongoContext.Subscriptions; // CORREÇÃO: Obtido do contexto
        _logger = logger; // CORREÇÃO: Logger inicializado
        MercadoPagoConfig.AccessToken = _settings.AccessToken;
    }

    public async Task<CreateCheckoutResponse> CreateCheckoutPreference(string userId, string planId, string successUrl, string failureUrl)
    {
        var user = await _users.Find(u => u.Id == userId).SingleOrDefaultAsync();
        var plan = await _plans.Find(p => p.Id == planId).SingleOrDefaultAsync();

        if (user == null || plan == null)
            throw new Exception("Usuário ou Plano não encontrado.");

        var request = new PreferenceRequest { /* ... (código existente sem alterações) ... */ };
        
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
                var userId = payment.ExternalReference;
                // CORREÇÃO: Obter o ID do item do pagamento
                var planId = payment.AdditionalInfo.Items.FirstOrDefault()?.Id;
                
                if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(planId))
                {
                    _logger.LogError("Webhook recebido sem ExternalReference (UserId) ou ItemId (PlanId). PaymentId: {PaymentId}", payment.Id);
                    return;
                }

                var existingSubscription = await _subscriptions.Find(s => s.StripeSubscriptionId == payment.Id.ToString()).SingleOrDefaultAsync();
                if (existingSubscription != null)
                {
                    _logger.LogWarning("Assinatura para o pagamento ID {PaymentId} já processada.", payment.Id);
                    return;
                }

                var newSubscription = new Subscription
                {
                    UserId = userId, // Agora a conversão é válida (string para string)
                    PlanId = planId, // Agora a conversão é válida (string para string)
                    Status = SubscriptionStatus.Active,
                    CurrentPeriodEnd = DateTime.UtcNow.AddMonths(1),
                    StripeSubscriptionId = payment.Id.ToString()
                };
            
                await _subscriptions.InsertOneAsync(newSubscription);
            
                // CORREÇÃO: O tipo genérico no Set() foi removido para inferência correta
                var userUpdate = Builders<User>.Update.Set(u => u.CurrentSubscriptionId, newSubscription.Id);
                await _users.UpdateOneAsync(u => u.Id == userId, userUpdate);

                _logger.LogInformation("Assinatura ativada para Usuário {UserId} no Plano {PlanId}", userId, planId);
            }
            else
            {
                _logger.LogInformation("Status do pagamento: {PaymentStatus}", payment.Status);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar webhook para o pagamento ID: {PaymentId}", notification.Data.Id);
            throw;
        }
    }
}