using IntelligentAutomation.Interfaces;
using IntelligentAutomation.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("[controller]")]
public class BillingController : ControllerBase
{
    private readonly IPaymentGatewayService _paymentGatewayService;

    public BillingController(IPaymentGatewayService paymentGatewayService)
    {
        _paymentGatewayService = paymentGatewayService;
    }

    [HttpPost("create-preference")]
    public async Task<IActionResult> CreatePreference([FromBody] CreatePreferenceRequest request)
    {
        // Mock do ID do usuário (viria do token de autenticação)
        var mockUserId = "ObjectId(...)"; // Use um ObjectId de um usuário no seu DB
        
        try
        {
            var response = await _paymentGatewayService.CreateCheckoutPreference(
                mockUserId, 
                request.PlanId, 
                request.SuccessUrl, 
                request.FailureUrl);
                
            return Ok(response);
        }
        catch (Exception ex)
        {
            // Logar o erro
            return BadRequest(new { message = ex.Message });
        }
    }
    
    [HttpPost("mp-webhook")]
    public async Task<IActionResult> MercadoPagoWebhook([FromBody] WebhookNotification notification)
    {
        // O Mercado Pago espera uma resposta rápida (HTTP 200 OK) para confirmar o recebimento.
        // O processamento real pode ser demorado, então o ideal seria enfileirar essa tarefa
        // e retornar OK imediatamente. Mas, para simplificar, vamos processar diretamente.

        try
        {
            await _paymentGatewayService.HandleWebhookEvent(notification);
        
            // Retorna 200 OK para confirmar ao Mercado Pago que recebemos a notificação com sucesso.
            return Ok();
        }
        catch (Exception ex)
        {
            // Se algo der errado, retornamos um erro. O Mercado Pago tentará reenviar a notificação.
            // Logar o erro é crucial aqui.
            return StatusCode(500, new { message = "Erro interno ao processar webhook.", error = ex.Message });
        }
    }
}