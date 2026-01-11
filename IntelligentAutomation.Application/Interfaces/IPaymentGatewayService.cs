using IntelligentAutomation.Domain.Entities;

namespace IntelligentAutomation.Application.Interfaces;

public interface IPaymentGatewayService
{
    Task<CreateCheckoutResponse> CreateCheckoutPreference(string userId, string planId, string successUrl, string failureUrl);
    Task HandleWebhookEvent(WebhookNotification notification);
}