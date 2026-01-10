using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgentSaaS.Web.Controllers;

[Authorize]
public class ApprovalsController : Controller
{
    [HttpPost]
    public async Task<IActionResult> Approve(Guid requestId)
    {
        var request = await _context.AgentApprovalRequests.FindAsync(requestId);
        request.Status = "Approved";
        await _context.SaveChangesAsync();

        // Recupera os argumentos e executa a ação real agora
        var args = JsonSerializer.Deserialize<EmailArgs>(request.ArgumentsJson);
        await _emailService.SendRealEmailAsync(args.To, args.Body);

        // Opcional: Injetar no histórico do agente que foi aprovado
        await _chatHistoryService.AddSystemMessageAsync(request.AgentId, "Ação aprovada pelo humano. O e-mail foi enviado com sucesso.");

        return RedirectToAction("Index");
    }
}