using System.Reflection;
using System.Text.Json;
using IntelligentAutomation.AgentRuntime;
using IntelligentAutomation.AgentRuntime.Interfaces;
using IntelligentAutomation.AgentRuntime.Modules;
using IntelligentAutomation.Domain.Workflow;
using IntelligentAutomation.AgentRuntime;
using IntelligentAutomation.AgentRuntime.Modules;
// ...

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        // Adiciona Logging, HttpClientFactory, etc.
        services.AddHttpClient();
        
        // Registra o ModuleRunner
        services.AddSingleton<IModuleRunner, ModuleRunner>();

        // Registra todas as implementações de IModule
        var moduleTypes = Assembly.GetExecutingAssembly().GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && t.IsAssignableTo(typeof(IModule)));
        foreach (var type in moduleTypes)
        {
            services.AddTransient(type);
        }
    })
    .Build();

Console.WriteLine("Agent Runtime iniciado. Aguardando definição do workflow...");

// Em um cenário real, o runtime obteria a definição do workflow
// de um volume montado no container, de uma variável de ambiente ou de uma API.
string workflowJson = File.ReadAllText("workflow-definition.json");
var definition = JsonSerializer.Deserialize<WorkflowDefinition>(workflowJson, GetJsonOptions());

var moduleRunner = host.Services.GetRequiredService<IModuleRunner>();
var logger = host.Services.GetRequiredService<ILogger<WorkflowEngine>>();

var engine = new WorkflowEngine(definition, moduleRunner, logger);
await engine.ExecuteAsync(CancellationToken.None);

Console.WriteLine("Agent Runtime encerrado.");
JsonSerializerOptions GetJsonOptions() { /* ... */ }