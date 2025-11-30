using Infrastructure.Data;

namespace Infrastructure.Repositories;

public interface ICategoryRepository : IGenericRepository<Category>
{
}
public class CategoryRepository(ApplicationDbContext _context) : GenericRepository<Category>(_context), ICategoryRepository
{
}
