using Insurance.Core.Common;
using Insurance.Core.Entities;

namespace Insurance.Core.Extensions;

public static class InsuranceMappingExtensions
{
    public static InsuranceResponse ToResponse(this Entities.Insurance insurance)
    {
        return new InsuranceResponse
        {
            MonthlyCost = insurance.MonthlyCost,
            Type = insurance.Type,
            VehicleInfo = null // Will be populated separately if needed
        };
    }

    public static IEnumerable<InsuranceResponse> ToResponse(this IEnumerable<Entities.Insurance> insurances)
    {
        return insurances.Select(insurance => insurance.ToResponse());
    }
}
