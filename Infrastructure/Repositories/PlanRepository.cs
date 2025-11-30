using Infrastructure.Data;

namespace Infrastructure.Repositories;

public interface IPlanRepository : IGenericRepository<Plan>
{
}

public class PlanRepository(ApplicationDbContext _context) : GenericRepository<Plan>(_context), IPlanRepository
{
}
