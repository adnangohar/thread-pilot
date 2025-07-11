using Insurance.Core.Common;

namespace Insurance.Core.Queries.GetPersonInsurances;

public interface IGetPersonInsurancesQueryHandler
{
    Task<PersonInsurancesResult?> Handle(GetPersonInsurancesQuery request, CancellationToken cancellationToken);
}
