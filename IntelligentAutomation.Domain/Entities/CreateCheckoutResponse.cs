namespace IntelligentAutomation.Domain.Entities;

public class CreateCheckoutResponse
{
    public string CheckoutUrl { get; set; } // URL para onde o usuário será redirecionado
    public string PreferenceId { get; set; }
}