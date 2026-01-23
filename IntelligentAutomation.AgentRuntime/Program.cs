using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using IntelligentAutomation.AgentRuntime;
using IntelligentAutomation.AgentRuntime.Interfaces;
using IntelligentAutomation.AgentRuntime.Modules;
using IntelligentAutomation.Domain.Workflow;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

// --- Configuração do Host ---
var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddHttpClient();
        services.AddSingleton<IModuleRunner, ModuleRunner>();

        // Registro automático de módulos
        var moduleTypes = Assembly.GetExecutingAssembly().GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && t.IsAssignableTo(typeof(IModule)));
        foreach (var type in moduleTypes)
        {
            services.AddTransient(type);
        }
    })
    .Build();

// --- Variáveis de Ambiente ---
var agentId = Environment.GetEnvironmentVariable("AGENT_ID");
var orchestratorUrl = Environment.GetEnvironmentVariable("ORCHESTRATOR_URL") ?? "http://localhost:5001";

if (string.IsNullOrEmpty(agentId))
{
    Console.Error.WriteLine("AGENT_ID não configurado.");
    return;
}

Console.WriteLine($"Agent Runtime iniciado para Agente: {agentId}");

var httpClient = host.Services.GetRequiredService<IHttpClientFactory>().CreateClient();

// --- Busca da Definição ---
WorkflowDefinition? definition = null;
try
{
    var response = await httpClient.GetAsync($"{orchestratorUrl}/agents/{agentId}/definition");
    response.EnsureSuccessStatusCode();
    var json = await response.Content.ReadAsStringAsync();

    var options = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() },
        TypeInfoResolver = new PolymorphicTypeResolver()
    };

    definition = JsonSerializer.Deserialize<WorkflowDefinition>(json, options);
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Erro ao buscar definição: {ex.Message}");
    return;
}

if (definition == null)
{
    Console.Error.WriteLine("Definição de workflow inválida ou vazia.");
    return;
}

// --- Execução ---
var moduleRunner = host.Services.GetRequiredService<IModuleRunner>();
var logger = new RemoteLogger(httpClient, orchestratorUrl, agentId);

var engine = new WorkflowEngine(definition, moduleRunner, logger);
await engine.ExecuteAsync(CancellationToken.None);

Console.WriteLine("Agent Runtime encerrado.");