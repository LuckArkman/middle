using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using IntelligentAutomation.AgentRuntime;
using IntelligentAutomation.AgentRuntime.Interfaces;
using IntelligentAutomation.AgentRuntime.Modules;
using IntelligentAutomation.Domain.Entities;
using IntelligentAutomation.Domain.Workflow;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

// --- Configuração do Host e Serviços ---
var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddHttpClient();
        services.AddSingleton<IModuleRunner, ModuleRunner>();

        var moduleTypes = Assembly.GetExecutingAssembly().GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && t.IsAssignableTo(typeof(IModule)));
        foreach (var type in moduleTypes)
        {
            services.AddTransient(type);
        }
    })
    .Build();

// --- Lógica Principal de Execução do Agente ---
Console.WriteLine("Agent Runtime iniciado. Aguardando definição do workflow...");

string workflowJson = File.ReadAllText("workflow-definition.json");
var definition = JsonSerializer.Deserialize<WorkflowDefinition>(workflowJson, GetJsonOptions());

if (definition == null)
{
    Console.Error.WriteLine("Falha ao desserializar a definição do workflow.");
    return;
}

var moduleRunner = host.Services.GetRequiredService<IModuleRunner>();
var logger = host.Services.GetRequiredService<ILogger<WorkflowEngine>>();

var engine = new WorkflowEngine(definition, moduleRunner, logger);
await engine.ExecuteAsync(CancellationToken.None);

Console.WriteLine("Agent Runtime encerrado.");


// --- Funções e Classes Auxiliares ---

JsonSerializerOptions GetJsonOptions()
{
    return new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() },
        TypeInfoResolver = new PolymorphicTypeResolver()
    };
}

// Resolvedor de tipo para desserializar corretamente os parâmetros dos módulos
public class PolymorphicTypeResolver : DefaultJsonTypeInfoResolver
{
    public override JsonTypeInfo GetTypeInfo(Type type, JsonSerializerOptions options)
    {
        JsonTypeInfo jsonTypeInfo = base.GetTypeInfo(type, options);
        if (jsonTypeInfo.Type == typeof(BaseModuleParameters))
        {
            jsonTypeInfo.PolymorphismOptions = new JsonPolymorphismOptions
            {
                TypeDiscriminatorPropertyName = "$type",
                IgnoreUnrecognizedTypeDiscriminators = true,
                UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FailSerialization, // Este erro agora será resolvido
                DerivedTypes =
                {
                    new JsonDerivedType(typeof(HttpRequestModuleParameters), "http"),
                    new JsonDerivedType(typeof(BinancePlaceOrderModuleParameters), "binanceOrder")
                }
            };
        }
        return jsonTypeInfo;
    }
}