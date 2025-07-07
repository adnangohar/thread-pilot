using MediatR;
using Insurance.Application.Common;

namespace Insurance.Application.Queries.GetPersonInsurances;

public record GetPersonInsurancesQuery(string PersonalIdentificationNumber) : IRequest<PersonInsurancesResult?>;
