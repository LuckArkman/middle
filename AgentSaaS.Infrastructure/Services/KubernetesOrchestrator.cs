using k8s;
using k8s.Models;

public class KubernetesOrchestrator : IContainerOrchestrator
{
    private readonly Kubernetes _client;

    public KubernetesOrchestrator()
    {
        // Carrega config de dentro do cluster (In-Cluster Config) ou ~/.kube/config
        var config = KubernetesClientConfiguration.BuildDefaultConfig();
        _client = new Kubernetes(config);
    }

    public async Task<string> StartAgentAsync(Guid agentId, string image, Dictionary<string, string> envVars)
    {
        var podName = $"agent-{agentId}";
        
        var pod = new V1Pod
        {
            Metadata = new V1ObjectMeta { Name = podName, Labels = new Dictionary<string, string> { { "app", "agent-runner" } } },
            Spec = new V1PodSpec
            {
                Containers = new List<V1Container>
                {
                    new V1Container
                    {
                        Name = "runner",
                        Image = image,
                        Env = envVars.Select(kv => new V1EnvVar(kv.Key, kv.Value)).ToList(),
                        Resources = new V1ResourceRequirements
                        {
                            Limits = new Dictionary<string, ResourceQuantity>
                            {
                                { "memory", new ResourceQuantity("512Mi") },
                                { "cpu", new ResourceQuantity("0.5") }
                            }
                        }
                    }
                },
                RestartPolicy = "Never" // O Orchestrator decide se reinicia
            }
        };

        await _client.CoreV1.CreateNamespacedPodAsync(pod, "agents-namespace");
        return podName;
    }

    public async Task StopAgentAsync(string containerId)
    {
        await _client.CoreV1.DeleteNamespacedPodAsync(containerId, "agents-namespace");
    }

    public async Task<Stream> GetLogsStreamAsync(string containerId)
    {
        return await _client.CoreV1.ReadNamespacedPodLogAsync(containerId, "agents-namespace", follow: true);
    }
}