using AgentSaaS.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stripe;

namespace AgentSaaS.Web.Controllers;

[Route("api/stripe")]
[ApiController]
public class StripeWebhookController : ControllerBase
{
    private readonly AppDbContext _context; // Importante: Aqui precisamos de um DbContext SEM filtro de Tenant, pois o Webhook é global

    public StripeWebhookController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> Index()
    {
        var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
        
        try
        {
            var stripeEvent = EventUtility.ConstructEvent(json, 
                Request.Headers["Stripe-Signature"], "whsec_..."); // Webhook Secret

            if (stripeEvent.Type == Events.CheckoutSessionCompleted)
            {
                var session = stripeEvent.Data.Object as Stripe.Checkout.Session;
                
                // Recupera dados dos metadados
                if (session.Metadata.TryGetValue("TenantId", out var tenantIdStr) &&
                    session.Metadata.TryGetValue("PlanType", out var planType))
                {
                    var tenantId = Guid.Parse(tenantIdStr);
                    
                    // ATUALIZAÇÃO DO PLANO NO BANCO
                    // Atenção: Use IgnoreQueryFilters() para achar o tenant sem estar logado
                    var tenant = await _context.Tenants
                        .IgnoreQueryFilters() 
                        .FirstOrDefaultAsync(t => t.Id == tenantId);

                    if (tenant != null)
                    {
                        tenant.PlanType = planType;
                        tenant.MaxAgents = planType == "Pro" ? 10 : 20;
                        await _context.SaveChangesAsync();
                    }
                }
            }
            return Ok();
        }
        catch (StripeException e)
        {
            return BadRequest();
        }
    }
}