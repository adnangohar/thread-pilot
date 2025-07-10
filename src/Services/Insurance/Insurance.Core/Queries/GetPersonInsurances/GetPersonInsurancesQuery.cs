using MediatR;
using Insurance.Core.Common;

namespace Insurance.Core.Queries.GetPersonInsurances;

public record GetPersonInsurancesQuery(string PersonalIdentificationNumber) : IRequest<PersonInsurancesResult?>;
