namespace Insurance.Core.Repositories;

public interface IInsuranceRepository
{
    Task<IEnumerable<Entities.Insurance>> GetByPersonalIdAsync(string personalId, CancellationToken cancellationToken);
    Task AddAsync(Entities.Insurance insurance, CancellationToken cancellationToken);
}
