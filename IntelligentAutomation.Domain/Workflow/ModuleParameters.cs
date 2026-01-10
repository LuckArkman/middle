namespace IntelligentAutomation.Domain.Workflow;
public abstract class BaseModuleParameters { }

public class HttpRequestModuleParameters : BaseModuleParameters
{
    public string Url { get; set; } = "https://api.example.com/data";
    public string Method { get; set; } = "GET";
    public string? Body { get; set; }
    public Dictionary<string, string> Headers { get; set; } = new();
}

public class SendEmailModuleParameters : BaseModuleParameters
{
    public string To { get; set; } = "recipient@example.com";
    public string Subject { get; set; } = "Assunto do E-mail";
    public string Body { get; set; } = "Corpo da mensagem.";
}