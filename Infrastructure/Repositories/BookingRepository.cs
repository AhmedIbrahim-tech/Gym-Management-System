using Infrastructure.Data;

namespace Infrastructure.Repositories;

public interface IBookingRepository : IGenericRepository<Booking>
{
    Task<IEnumerable<Booking>> GetBySessionIdAsync(int sessionId, CancellationToken cancellationToken = default);
}

public class BookingRepository(ApplicationDbContext _context) : GenericRepository<Booking>(_context), IBookingRepository
{
    public async Task<IEnumerable<Booking>> GetBySessionIdAsync(int sessionId, CancellationToken cancellationToken = default)
	{
		return await _context.Bookings
			.Include(x => x.Member)
			.AsNoTracking()
			.Where(x => x.SessionId == sessionId)
			.ToListAsync(cancellationToken);
	}
}
