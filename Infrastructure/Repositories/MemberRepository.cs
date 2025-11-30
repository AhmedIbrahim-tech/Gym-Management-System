using Infrastructure.Data;

namespace Infrastructure.Repositories;

public interface IMemberRepository : IGenericRepository<Member>
{
}

public class MemberRepository(ApplicationDbContext _context) : GenericRepository<Member>(_context), IMemberRepository
{
}
