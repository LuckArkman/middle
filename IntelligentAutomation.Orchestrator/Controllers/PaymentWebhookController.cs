using IntelligentAutomation.Interfaces;
using IntelligentAutomation.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace IntelligentAutomation.Orchestrator.Controllers;

[ApiController]
[Route("api/payments/webhooks")]
public class PaymentWebhookController : ControllerBase
{
    private readonly IPaymentGatewayService _paymentService;
    private readonly ILogger<PaymentWebhookController> _logger;

    public PaymentWebhookController(IPaymentGatewayService paymentService, ILogger<PaymentWebhookController> logger)
    {
        _paymentService = paymentService;
        _logger = logger;
    }

    [HttpPost("mercadopago")]
    public async Task<IActionResult> HandleMercadoPagoWebhook([FromBody] WebhookNotification notification)
    {
        _logger.LogInformation("Webhook do Mercado Pago recebido: {Id}", notification.Data.Id);

        try
        {
            await _paymentService.HandleWebhookEvent(notification);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar webhook do Mercado Pago.");
            // Retorna OK para o MP não ficar tentando reenviar se for erro de lógica, mas o ideal é 200 sempre que o payload for válido
            return Ok();
        }
    }
}
