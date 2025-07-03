using System;
using Insurance.Domain.ValueObjects;

namespace Insurance.Domain.Repositories;

public interface IInsuranceRepository
{
    Task<IEnumerable<Entities.Insurance>> GetByOwnerAsync(PersonalIdentificationNumber owner);
    Task AddAsync(Entities.Insurance insurance);
}
