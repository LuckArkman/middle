using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace IntelligentAutomation.Domain.Workflow;

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
                DerivedTypes =
                {
                    new JsonDerivedType(typeof(HttpRequestModuleParameters), "http"),
                    new JsonDerivedType(typeof(IntelligentAutomation.Domain.Entities.BinancePlaceOrderModuleParameters), "binanceOrder"),
                    new JsonDerivedType(typeof(IntelligentAutomation.Domain.Entities.DelayModuleParameters), "delay"),
                    new JsonDerivedType(typeof(IntelligentAutomation.Domain.Entities.ConditionModuleParameters), "condition"),
                    new JsonDerivedType(typeof(IntelligentAutomation.Domain.Entities.LlmModuleParameters), "llm"),
                    new JsonDerivedType(typeof(SendEmailModuleParameters), "email")
                }
            };
        }
        return jsonTypeInfo;
    }
}
