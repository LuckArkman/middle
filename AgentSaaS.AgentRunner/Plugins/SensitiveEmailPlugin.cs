using System.ComponentModel;
using System.Text.Json;
using AgentSaaS.Core.Entities;
using AgentSaaS.Infrastructure.Data;
using Microsoft.SemanticKernel;

namespace AgentSaaS.AgentRunner.Plugins;

public class SensitiveEmailPlugin
{
    private readonly AppDbContext _context;
    private readonly Guid _agentId;

    public SensitiveEmailPlugin(AppDbContext context, Guid agentId)
    {
        _context = context;
        _agentId = agentId;
    }

    [KernelFunction, Description("Prepara um e-mail importante para envio, mas aguarda aprovação humana.")]
    public async Task<string> DraftEmailForApproval(
        [Description("Destinatário")] string to, 
        [Description("Conteúdo")] string body)
    {
        // 1. Não envia o e-mail. Cria o registro no banco.
        var request = new AgentApprovalRequest
        {
            AgentId = _agentId,
            FunctionName = "SendContractEmail",
            ArgumentsJson = JsonSerializer.Serialize(new { to, body })
        };

        _context.AgentApprovalRequests.Add(request);
        await _context.SaveChangesAsync();

        // 2. Avisa o Agente para parar.
        return "SOLICITAÇÃO DE APROVAÇÃO CRIADA: O e-mail foi preparado e está aguardando um humano clicar em 'Aprovar' no painel. Avise o usuário que você está aguardando.";
    }
}