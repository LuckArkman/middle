using IntelligentAutomation.Domain.Entities;
using IntelligentAutomation.Infrastructure.Persistence;
using IntelligentAutomation.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System.Security.Claims;

namespace IntelligentAutomation.Portal.Controllers;

[Authorize]
public class BillingController : Controller
{
    private readonly MongoDbContext _db;
    private readonly IPaymentGatewayService _paymentGateway;
    private readonly ILogger<BillingController> _logger;

    public BillingController(MongoDbContext db, IPaymentGatewayService paymentGateway, ILogger<BillingController> logger)
    {
        _db = db;
        _paymentGateway = paymentGateway;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        var plans = await _db.Plans.Find(p => p.TenantId == "system" || p.TenantId == null).SortBy(p => p.MonthlyPrice).ToListAsync();

        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (Guid.TryParse(userIdStr, out var userId))
        {
            ViewBag.CurrentSubscription = await _db.Subscriptions
                .Find(s => s.UserId == userId)
                .SortByDescending(s => s.CreatedAt)
                .FirstOrDefaultAsync();
        }

        return View(plans);
    }

    [HttpPost]
    public async Task<IActionResult> Subscribe(Guid planId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        try
        {
            var successUrl = Url.Action("PaymentSuccess", "Billing", null, Request.Scheme);
            var failureUrl = Url.Action("PaymentFailure", "Billing", null, Request.Scheme);

            var checkout = await _paymentGateway.CreateCheckoutPreference(userId, planId.ToString(), successUrl!, failureUrl!);

            return Redirect(checkout.CheckoutUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar preferência de pagamento.");
            TempData["Error"] = "Não foi possível iniciar o checkout. Tente novamente mais tarde.";
            return RedirectToAction(nameof(Index));
        }
    }

    public IActionResult PaymentSuccess()
    {
        ViewBag.Message = "Pagamento aprovado! Sua assinatura está sendo processada.";
        return View();
    }

    public IActionResult PaymentFailure()
    {
        ViewBag.Message = "Houve um problema com seu pagamento.";
        return View();
    }
}
