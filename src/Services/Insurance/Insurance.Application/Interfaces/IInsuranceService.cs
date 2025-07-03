using System;
using Insurance.Contracts;

namespace Insurance.Application.Interfaces;

public interface IInsuranceService
{
    Task<PersonInsurancesDto?> GetPersonInsurancesAsync(string personalIdentificationNumber);
}
