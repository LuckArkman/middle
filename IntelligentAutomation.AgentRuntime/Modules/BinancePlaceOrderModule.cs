using System.Collections.Concurrent;
using Binance.Net.Clients;                // Contém a classe BinanceRestClient
using Binance.Net.Enums;                  // Contém OrderSide e SpotOrderType
using Binance.Net.Objects;                // Contém ApiCredentials
using CryptoExchange.Net.Authentication;
using IntelligentAutomation.AgentRuntime.Interfaces;
using IntelligentAutomation.AgentRuntime.Modules;
using IntelligentAutomation.Domain.Entities;
using IntelligentAutomation.Domain.Workflow;
using Microsoft.Extensions.Logging;

namespace IntelligentAutomationSaaS.AgentRuntime.Implementations;

[Module("BinancePlaceOrder")]
public class BinancePlaceOrderModule : IModule
{
    private readonly ILogger<BinancePlaceOrderModule> _logger;

    public BinancePlaceOrderModule(ILogger<BinancePlaceOrderModule> logger)
    {
        _logger = logger;
    }

    public async Task<object> ExecuteAsync(BaseModuleParameters? parameters, ConcurrentDictionary<string, object> context)
    {
        if (parameters is not BinancePlaceOrderModuleParameters orderParams)
        {
            throw new ArgumentException("Parâmetros inválidos para o módulo BinancePlaceOrder.", nameof(parameters));
        }

        var apiKey = Environment.GetEnvironmentVariable("BINANCE_API_KEY");
        var apiSecret = Environment.GetEnvironmentVariable("BINANCE_API_SECRET");

        if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(apiSecret))
        {
            _logger.LogError("As credenciais da Binance (BINANCE_API_KEY, BINANCE_API_SECRET) não foram encontradas nas variáveis de ambiente.");
            throw new InvalidOperationException("Credenciais da Binance não configuradas.");
        }

        var client = new BinanceRestClient(options =>
        {
            options.ApiCredentials = new ApiCredentials(apiKey, apiSecret);
        });
        
        // Converte os parâmetros de string para os enums corretos da biblioteca
        var side = Enum.Parse<OrderSide>(orderParams.Side, true);
        var type = Enum.Parse<SpotOrderType>(orderParams.OrderType, true);

        _logger.LogInformation("Enviando ordem {Side} {Type} para o símbolo {Symbol} com quantidade {Quantity}",
            side, type, orderParams.Symbol, orderParams.Quantity);

        // A chamada ao método `PlaceOrderAsync` agora é unívoca
        var result = await client.SpotApi.Trading.PlaceOrderAsync(
            orderParams.Symbol,
            side,
            type,
            quantity: orderParams.Quantity,
            price: type == SpotOrderType.Limit ? orderParams.Price : null // Inclui o preço apenas para ordens Limit
        );

        if (!result.Success)
        {
            var errorMessage = $"Falha ao enviar ordem para a Binance: {result.Error?.Message}";
            _logger.LogError(errorMessage);
            throw new Exception(errorMessage);
        }
        
        _logger.LogInformation("Ordem para {Symbol} enviada com sucesso. OrderId: {OrderId}", 
            orderParams.Symbol, result.Data.Id);

        return result.Data; // Retorna o objeto real da resposta da API
    }
}