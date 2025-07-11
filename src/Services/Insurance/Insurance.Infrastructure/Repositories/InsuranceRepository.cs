using Insurance.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using InsuranceEntity = Insurance.Core.Entities.Insurance;

namespace Insurance.Infrastructure.Repositories;

public class InsuranceRepository : Core.Repositories.IInsuranceRepository
{
    private readonly InsuranceDbContext _context;

    public InsuranceRepository(InsuranceDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }
    public async Task AddAsync(InsuranceEntity insurance, CancellationToken cancellationToken)
    {
        _context.Insurances.Add(insurance);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<InsuranceEntity>> GetByPersonalIdAsync(string personalId, CancellationToken cancellationToken)
    {
        return await _context.Insurances.Where(i => i.PersonalId == personalId).ToListAsync(cancellationToken);
    }
}
