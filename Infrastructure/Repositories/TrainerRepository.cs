using Infrastructure.Data;

namespace Infrastructure.Repositories;

public interface ITrainerRepository : IGenericRepository<Trainer>
{
}

public class TrainerRepository(ApplicationDbContext _context) : GenericRepository<Trainer>(_context), ITrainerRepository
{
}
