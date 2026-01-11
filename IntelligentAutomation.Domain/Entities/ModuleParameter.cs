namespace IntelligentAutomation.Domain.Entities;

public class ModuleParameter
{
    public string Name { get; set; } // Ex: "Url", "Symbol", "Quantity"
    public string DisplayName { get; set; } // Ex: "URL", "Ativo (ex: BTCUSDT)", "Quantidade"
    public string Type { get; set; } // Ex: "string", "number", "options", "secret"
    public bool IsRequired { get; set; }
    public List<string>? Options { get; set; } // Para campos do tipo "options" (dropdown)
    public string? DefaultValue { get; set; }
}