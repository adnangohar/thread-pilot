namespace Insurance.Core.Common;

public class PersonInsurancesResult
{
    public string PersonalIdentificationNumber { get; set; } = string.Empty;
    public List<InsuranceResponse> Insurances { get; set; } = new();
    public decimal TotalMonthlyCost { get; set; }
}
