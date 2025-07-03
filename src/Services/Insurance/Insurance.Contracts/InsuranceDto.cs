namespace Insurance.Contracts;

public abstract class InsuranceDto
{
    public string Type { get; set; } = string.Empty;
    public decimal MonthlyCost { get; set; }
}

public class PetInsuranceDto : InsuranceDto
{
    public string PetName { get; set; } = string.Empty;
    public string PetType { get; set; } = string.Empty;
}

public class PersonalHealthInsuranceDto : InsuranceDto
{
    public string CoverageLevel { get; set; } = string.Empty;
}

public class CarInsuranceDto : InsuranceDto
{
    public VehicleResponse? Vehicle { get; set; }
}