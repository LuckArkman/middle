using System.Text.RegularExpressions;
using AgentSaaS.Core.Entities;
using AgentSaaS.Infrastructure.Data;
using Microsoft.Extensions.Compliance.Redaction;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace AgentSaaS.AgentRunner.Services;

public class ChatHistoryService
{
    private readonly AppDbContext _context;
    private readonly Guid _agentId;

    public ChatHistoryService(AppDbContext context, Guid agentId)
    {
        _context = context;
        _agentId = agentId;
    }

    // Carrega o histórico do banco para o objeto do Semantic Kernel
    public async Task<ChatHistory> LoadHistoryAsync(string systemPrompt)
    {
        var history = new ChatHistory(systemPrompt);

        // Pega as últimas X mensagens (ex: últimas 20 para não estourar o contexto)
        var messages = await _context.AgentMessages
            .Where(m => m.AgentId == _agentId)
            .OrderByDescending(m => m.Timestamp)
            .Take(20)
            .ToListAsync();

        // Reverte para ordem cronológica (Antiga -> Nova)
        foreach (var msg in messages.OrderBy(m => m.Timestamp))
        {
            if (msg.Role == "User") history.AddUserMessage(msg.Content);
            else if (msg.Role == "Assistant") history.AddAssistantMessage(msg.Content);
            // Tratamento especial para Tools se necessário
        }

        return history;
    }

    // Salva a nova interação
    public async Task SaveInteractionAsync(string userMessage, string assistantResponse, int userTokens, int assistantTokens)
    {
        var userMsg = new AgentMessage 
        { 
            AgentId = _agentId, Role = "User", Content = userMessage, TokenCount = userTokens 
        };
        
        var aiMsg = new AgentMessage 
        { 
            AgentId = _agentId, Role = "Assistant", Content = assistantResponse, TokenCount = assistantTokens 
        };

        _context.AgentMessages.AddRange(userMsg, aiMsg);
        await _context.SaveChangesAsync();
    }
    
    public async Task OptimizeHistoryAsync(ChatHistory history)
    {
        // Se tivermos mais de 30 mensagens, vamos resumir as primeiras 15
        if (history.Count > 30)
        {
            var oldMessages = history.Take(15).Select(m => $"{m.Role}: {m.Content}");
            var textToSummarize = string.Join("\n", oldMessages);

            var summaryPrompt = $"Resuma a conversa abaixo mantendo fatos chaves como nomes, datas e intenções:\n{textToSummarize}";
        
            // Usa um Kernel leve para resumir
            var summary = await _cheapKernel.InvokePromptAsync(summaryPrompt);

            // Remove as antigas e insere o resumo como "Contexto do Sistema" ou mensagem injetada
            for(int i=0; i<15; i++) history.RemoveAt(0);
        
            history.Insert(0, new ChatMessageContent(AuthorRole.System, $"Resumo da conversa anterior: {summary}"));
        }
    }
    
    public class SecureLogger
    {
        private readonly IRedactorProvider _redactorProvider;

        public SecureLogger(IRedactorProvider redactorProvider)
        {
            _redactorProvider = redactorProvider;
        }

        public string SanitizeLog(string rawContent)
        {
            // Exemplo simples com Regex manual (para ilustrar)
            // O Redactor nativo do .NET 8 é mais performático para pipelines de logging,
            // mas para texto de chat, um Regex compilado resolve rápido.
        
            var cpfRegex = new Regex(@"\d{3}\.\d{3}\.\d{3}-\d{2}");
            var creditCardRegex = new Regex(@"\b(?:\d[ -]*?){13,16}\b");

            var safeContent = cpfRegex.Replace(rawContent, "[CPF REMOVIDO]");
            safeContent = creditCardRegex.Replace(safeContent, "[CARTÃO REMOVIDO]");

            return safeContent;
        }
    }
}