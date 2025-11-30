using Infrastructure.Data;

namespace Infrastructure.Repositories;

public interface IUnitOfWork
{
    IGenericRepository<TEntity> GetRepository<TEntity>() where TEntity : BaseEntity;
    ISessionRepository SessionRepository { get; set; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

public class UnitOfWork(ApplicationDbContext _context) : IUnitOfWork
{
    private readonly Dictionary<string, object> _repositories = [];
    public ISessionRepository SessionRepository { get; set; } = new SessionRepository(_context);

    public IGenericRepository<TEntity> GetRepository<TEntity>() where TEntity : BaseEntity
    {
        string typeName = typeof(TEntity).Name;
        if (_repositories.TryGetValue(typeName, out object? repository))
        {
            return (IGenericRepository<TEntity>)repository;
        }
        var newRepository = new GenericRepository<TEntity>(_context);
        _repositories.Add(typeName, newRepository);
        return newRepository;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}
