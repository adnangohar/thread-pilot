using Insurance.Domain.Repositories;
using Insurance.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Insurance.Infrastructure.Repositories;

public class InsuranceRepository : IInsuranceRepository
{
    private readonly InsuranceDbContext _context;

    public InsuranceRepository(InsuranceDbContext context)
    {
        _context = context;
    }
    public async Task AddAsync(Domain.Entities.Insurance insurance, CancellationToken cancellationToken)
    {
        _context.Insurances.Add(insurance);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<Domain.Entities.Insurance>> GetByPersonalIdAsync(string personalId, CancellationToken cancellationToken)
    {
        return await _context.Insurances.Where(i => i.PersonalId == personalId).ToListAsync(cancellationToken);
    }
}
