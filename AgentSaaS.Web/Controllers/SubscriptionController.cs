using AgentSaaS.Infrastructure.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;

namespace AgentSaaS.Web.Controllers;

[Authorize]
public class SubscriptionController : Controller
{
    private readonly ITenantProvider _tenantProvider;
    
    public SubscriptionController(ITenantProvider tenantProvider)
    {
        _tenantProvider = tenantProvider;
        StripeConfiguration.ApiKey = "sk_test_..."; // Use User Secrets em prod
    }

    [HttpPost]
    public IActionResult CreateCheckoutSession(string planType) // "Pro", "Enterprise"
    {
        var tenantId = _tenantProvider.GetTenantId();
        
        // Define preços (Hardcoded ou vindos do DB)
        var priceId = planType == "Pro" ? "price_H5ggY..." : "price_J2kkZ...";

        var options = new SessionCreateOptions
        {
            PaymentMethodTypes = new List<string> { "card" },
            LineItems = new List<SessionLineItemOptions>
            {
                new SessionLineItemOptions
                {
                    Price = priceId,
                    Quantity = 1,
                },
            },
            Mode = "subscription",
            SuccessUrl = "https://meusaas.com/Subscription/Success",
            CancelUrl = "https://meusaas.com/Subscription/Cancel",
            // Metadados cruciais para o Webhook saber quem pagou
            Metadata = new Dictionary<string, string>
            {
                { "TenantId", tenantId.ToString() },
                { "PlanType", planType }
            }
        };

        var service = new SessionService();
        Session session = service.Create(options);

        return Redirect(session.Url);
    }
}