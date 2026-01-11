using IntelligentAutomation.Domain.Workflow;

namespace IntelligentAutomation.Domain.Entities;

public class BinancePlaceOrderModuleParameters : BaseModuleParameters
{
    public string Symbol { get; set; } = "BTCUSDT";
    public string Side { get; set; } = "Buy"; // Valores: "Buy" ou "Sell"
    public string OrderType { get; set; } = "Market"; // Valores: "Market" ou "Limit"
    public decimal Quantity { get; set; }
    public decimal? Price { get; set; } // Obrigatório apenas para ordens do tipo "Limit"
}