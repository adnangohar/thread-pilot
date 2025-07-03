namespace Insurance.Contracts;

public class PersonInsurancesDto
{
    public string PersonalIdentificationNumber { get; set; } = string.Empty;
    public List<InsuranceDto> Insurances { get; set; } = new();
    public decimal TotalMonthlyCost { get; set; }
}
