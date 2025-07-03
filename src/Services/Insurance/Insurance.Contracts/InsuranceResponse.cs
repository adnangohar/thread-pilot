using Vehicle.Contracts;

namespace Insurance.Contracts;

public abstract class InsuranceResponse
{
    public string Type { get; set; } = string.Empty;
    public decimal MonthlyCost { get; set; }
}

public class PetInsuranceResponse : InsuranceResponse
{
    public string PetName { get; set; } = string.Empty;
    public string PetType { get; set; } = string.Empty;
}

public class PersonalHealthInsuranceResponse : InsuranceResponse
{
    public string CoverageLevel { get; set; } = string.Empty;
}

public class CarInsuranceResponse : InsuranceResponse
{
    public VehicleResponse? Vehicle { get; set; }
}