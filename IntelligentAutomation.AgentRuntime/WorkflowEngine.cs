using IntelligentAutomation.Domain.Workflow;
using System.Collections.Concurrent;
using IntelligentAutomation.AgentRuntime.Interfaces;

namespace IntelligentAutomation.AgentRuntime;

public class WorkflowEngine
{
    private readonly WorkflowDefinition _definition;
    private readonly IModuleRunner _moduleRunner;
    private readonly ILogger<WorkflowEngine> _logger;

    public WorkflowEngine(WorkflowDefinition definition, IModuleRunner moduleRunner, ILogger<WorkflowEngine> logger)
    {
        _definition = definition;
        _moduleRunner = moduleRunner;
        _logger = logger;
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Iniciando execução do workflow...");

        // O 'executionContext' armazenará os resultados de cada nó para que nós subsequentes possam usá-los.
        var executionContext = new ConcurrentDictionary<string, object>();

        // Encontra o nó inicial (o que está conectado ao gatilho)
        var startConnection = _definition.Connections.FirstOrDefault(c => c.SourceNodeId == _definition.Trigger.Id);
        if (startConnection == null)
        {
            _logger.LogWarning("Nenhum nó inicial encontrado conectado ao gatilho. Workflow encerrado.");
            return;
        }

        var currentNode = _definition.Nodes.FirstOrDefault(n => n.Id == startConnection.TargetNodeId);

        while (currentNode != null && !cancellationToken.IsCancellationRequested)
        {
            if (currentNode is ModuleNode moduleNode)
            {
                _logger.LogInformation("Executando módulo: {ModuleName} (Tipo: {ModuleType})", moduleNode.Name, moduleNode.ModuleType);

                try
                {
                    var result = await _moduleRunner.RunAsync(moduleNode, executionContext);
                    executionContext[currentNode.Id] = result; // Armazena o resultado

                    // Lógica de Branching (If/Else)
                    Connection? nextConnection = null;
                    if (result is bool boolResult)
                    {
                        var label = boolResult ? "True" : "False";
                        nextConnection = _definition.Connections.FirstOrDefault(c => c.SourceNodeId == currentNode.Id && c.Label == label);

                        // Fallback se não houver label específico (segue fluxo linear)
                        if (nextConnection == null)
                        {
                            nextConnection = _definition.Connections.FirstOrDefault(c => c.SourceNodeId == currentNode.Id && string.IsNullOrEmpty(c.Label));
                        }
                    }
                    else
                    {
                        // Fluxo linear padrão
                        nextConnection = _definition.Connections.FirstOrDefault(c => c.SourceNodeId == currentNode.Id);
                    }

                    currentNode = nextConnection != null
                        ? _definition.Nodes.FirstOrDefault(n => n.Id == nextConnection.TargetNodeId)
                        : null;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao executar o módulo {ModuleName}. Encerrando workflow.", moduleNode.Name);
                    return;
                }
            }
            else
            {
                // Para outros tipos de nós (se houver), encerra por enquanto
                currentNode = null;
            }
        }

        _logger.LogInformation("Execução do workflow concluída.");
    }
}