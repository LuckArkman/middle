namespace IntelligentAutomation.Domain.Entities;

public class CreatePreferenceRequest
{
    public string PlanId { get; set; } // O ObjectId do plano no MongoDB
    public string SuccessUrl { get; set; }
    public string FailureUrl { get; set; }
}