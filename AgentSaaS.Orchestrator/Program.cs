using AgentSaaS.Orchestrator;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();

var resourceBuilder = ResourceBuilder.CreateDefault()
    .AddService(builder.Environment.ApplicationName);

// 1. Logging
builder.Logging.AddOpenTelemetry(logging =>
{
    logging.SetResourceBuilder(resourceBuilder);
    logging.AddOtlpExporter(); // Envia para o coletor (Jaeger/Aspire)
});

// 2. Tracing (Rastreamento de Requisições)
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing =>
    {
        tracing.SetResourceBuilder(resourceBuilder)
            .AddAspNetCoreInstrumentation() // Rastreia Requests recebidos
            .AddHttpClientInstrumentation() // Rastreia chamadas externas (OpenAI/Stripe)
            .AddRedisInstrumentation();     // Rastreia Redis (se usar o pacote contrib)
            
        tracing.AddOtlpExporter();
    })
// 3. Metrics (CPU, Memória, Requests/sec)
    .WithMetrics(metrics =>
    {
        metrics.SetResourceBuilder(resourceBuilder)
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation();

        metrics.AddOtlpExporter();
    });

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddSingleton<IContainerOrchestrator, DockerOrchestrator>();
}
else
{
    builder.Services.AddSingleton<IContainerOrchestrator, KubernetesOrchestrator>();
}
var host = builder.Build();
host.Run();