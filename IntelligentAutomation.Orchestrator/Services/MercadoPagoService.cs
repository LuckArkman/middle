using IntelligentAutomation.Application.Interfaces;
using IntelligentAutomation.Domain.Entities;
using IntelligentAutomation.Domain.Enums;
using IntelligentAutomation.Infrastructure.Persistence;
using MercadoPago.Client.Payment;
using MercadoPago.Client.Preference;
using MercadoPago.Config;
using MercadoPago.Resource.Payment;
using MercadoPago.Resource.Preference;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace IntelligentAutomation.Orchestrator.Services;

public class MercadoPagoService : IPaymentGatewayService
{
    readonly ILogger<MercadoPagoService> _logger;
    private readonly MercadoPagoSettings _settings;
    private readonly IMongoCollection<User> _users;
    private readonly IMongoCollection<Plan> _plans;
    private readonly IMongoCollection<Subscription> _subscriptions;

    public MercadoPagoService(
        IOptions<MercadoPagoSettings> settings,
        MongoDbContext mongoContext,
        IMongoCollection<Subscription> subscriptions)
    {
        _settings = settings.Value;
        _users = mongoContext.Users;
        _plans = mongoContext.Plans;
        _subscriptions = subscriptions;
        MercadoPagoConfig.AccessToken = _settings.AccessToken;
    }

    public async Task<CreateCheckoutResponse> CreateCheckoutPreference(string userId, string planId, string successUrl, string failureUrl)
    {
        var user = await _users.Find(u => u.Id == userId).SingleOrDefaultAsync();
        var plan = await _plans.Find(p => p.Id == planId).SingleOrDefaultAsync();

        if (user == null || plan == null)
            throw new Exception("Usuário ou Plano não encontrado.");

        var request = new PreferenceRequest
        {
            Items = new List<PreferenceItemRequest>
            {
                new()
                {
                    Id = plan.Id,
                    Title = $"Assinatura do Plano {plan.Name}",
                    Description = $"Acesso ao plano {plan.Name} da plataforma.",
                    Quantity = 1,
                    CurrencyId = "BRL",
                    UnitPrice = plan.MonthlyPrice
                }
            },
            Payer = new PreferencePayerRequest
            {
                Email = user.Email
            },
            BackUrls = new PreferenceBackUrlsRequest
            {
                Success = successUrl,
                Failure = failureUrl
            },
            AutoReturn = "approved",
            ExternalReference = userId // Vincula a preferência ao nosso usuário
        };
        
        var client = new PreferenceClient();
        Preference preference = await client.CreateAsync(request);

        return new CreateCheckoutResponse
        {
            PreferenceId = preference.Id,
            CheckoutUrl = preference.InitPoint // URL de checkout do Mercado Pago
        };
    }

    public Task HandleWebhookEvent(object webhookPayload)
    {
        // Lógica do Webhook será implementada na próxima etapa
        throw new NotImplementedException();
    }
    
    public async Task HandleWebhookEvent(WebhookNotification notification)
{
    // O Mercado Pago envia notificações para vários tópicos. Estamos interessados em 'payment'.
    if (notification.Type != "payment")
    {
        return;
    }

    _logger.LogInformation("Recebido webhook de pagamento do Mercado Pago. ID do Pagamento: {PaymentId}", notification.Data.Id);

    try
    {
        var client = new PaymentClient();
        Payment payment = await client.GetAsync(long.Parse(notification.Data.Id));

        // Verificamos se o pagamento foi aprovado
        if (payment.Status == "approved")
        {
            var userId = payment.ExternalReference;
            var planId = payment.Order.Id.ToString(); // Assumindo que o ID do item é o ID do plano

            // Verifica se já existe uma assinatura para este pagamento para evitar duplicidade
            var existingSubscription = await _subscriptions.Find(s => s.StripeSubscriptionId == payment.Id.ToString()).SingleOrDefaultAsync();
            if (existingSubscription != null)
            {
                _logger.LogWarning("Assinatura para o pagamento ID {PaymentId} já foi processada.", payment.Id);
                return;
            }

            // Cria uma nova assinatura em nosso banco de dados
            var newSubscription = new Subscription
            {
                UserId = userId,
                PlanId = planId, 
                Status = SubscriptionStatus.Active,
                // O Mercado Pago para pagamentos únicos não tem 'período'. 
                // Para assinaturas recorrentes, o fluxo seria diferente e usaria a API de Subscriptions.
                // Aqui, vamos assumir um plano mensal.
                CurrentPeriodEnd = DateTime.UtcNow.AddMonths(1),
                StripeSubscriptionId = payment.Id.ToString() // Usamos este campo para guardar o ID do pagamento/assinatura do MP
            };
            
            await _subscriptions.InsertOneAsync(newSubscription);
            
            // Atualiza o usuário para vincular à nova assinatura
            var userUpdate = Builders<User>.Update.Set<string>(u => u.CurrentSubscriptionId!, newSubscription.Id);
            await _users.UpdateOneAsync(u => u.Id == userId, userUpdate);

            _logger.LogInformation("Assinatura ativada com sucesso para o Usuário ID: {UserId} no Plano ID: {PlanId}", userId, planId);
        }
        else
        {
            _logger.LogInformation("Status do pagamento não é 'approved'. Status: {PaymentStatus}", payment.Status);
            // Aqui poderíamos tratar outros status, como 'rejected', 'in_process', etc.
        }
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Erro ao processar o webhook do Mercado Pago para o pagamento ID: {PaymentId}", notification.Data.Id);
        // É importante lançar a exceção para que o Mercado Pago saiba que a notificação falhou e tente reenviar.
        throw;
    }
}
}