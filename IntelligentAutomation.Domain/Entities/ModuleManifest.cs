namespace IntelligentAutomation.Domain.Entities;


public class ModuleManifest : BaseEntity
{
    public string Type { get; set; } // Ex: "HttpRequest", "BinancePlaceOrder"
    public string DisplayName { get; set; } // Ex: "Requisição HTTP", "Criar Ordem (Binance)"
    public string Description { get; set; }
    public string Area { get; set; } // A área de atuação, ex: "Finanças & Compliance", "Day Trading"
    public List<ModuleParameter> Parameters { get; set; } = new();
}