namespace Insurance.Contracts;

public class InsuranceResponse
{
    public decimal MonthlyCost { get;  set; }
    public InsuranceType Type { get;  set; }
    public VehicleResponse? VehicleInfo { get; set; } // For car insurance
}
