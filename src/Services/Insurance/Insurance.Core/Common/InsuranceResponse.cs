using Insurance.Core.Enums;

namespace Insurance.Core.Common;

public class InsuranceResponse
{
    public decimal MonthlyCost { get;  set; }
    public InsuranceType Type { get;  set; }
    public VehicleResponse? VehicleInfo { get; set; } // For car insurance
}
