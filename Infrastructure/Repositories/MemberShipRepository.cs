using Infrastructure.Data;

namespace Infrastructure.Repositories;

public interface IMembershipRepository : IGenericRepository<MemberShip>
{
    Task<IEnumerable<MemberShip>> GetAllMembershipsWithMemberAndPlanAsync(Expression<Func<MemberShip, bool>> predicate, CancellationToken cancellationToken = default);
    Task<MemberShip?> GetMembershipByIdWithMemberAndPlanAsync(int id, CancellationToken cancellationToken = default);
}

public class MembershipRepository(ApplicationDbContext _context) : GenericRepository<MemberShip>(_context), IMembershipRepository
{
    public async Task<IEnumerable<MemberShip>> GetAllMembershipsWithMemberAndPlanAsync(Expression<Func<MemberShip, bool>> predicate, CancellationToken cancellationToken = default)
	{
		return await _context.MemberShips
			.Include(x => x.Plan)
			.Include(x => x.Member)
			.AsNoTracking()
			.Where(predicate)
            .ToListAsync(cancellationToken);
	}

	public async Task<MemberShip?> GetMembershipByIdWithMemberAndPlanAsync(int id, CancellationToken cancellationToken = default)
	{
		return await _context.MemberShips
			.Include(x => x.Plan)
			.Include(x => x.Member)
			.AsNoTracking()
			.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
	}
}
