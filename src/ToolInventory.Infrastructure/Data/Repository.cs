using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using ToolInventory.Core.Interfaces;

namespace ToolInventory.Infrastructure.Data;

public class Repository<T>(AppDbContext context) : IRepository<T> where T : class
{
    private readonly DbSet<T> _set = context.Set<T>();

    public async Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => await _set.FindAsync([id], cancellationToken);

    public async Task<IEnumerable<T>> GetAllAsync(bool asNoTracking = true, CancellationToken cancellationToken = default, params Expression<Func<T, object>>[] includes)
    {
        var query = BuildQuery(asNoTracking, includes);
        return await query.ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<T>> GetPagedAsync(int page, int pageSize, bool asNoTracking = true, CancellationToken cancellationToken = default, params Expression<Func<T, object>>[] includes)
    {
        var query = BuildQuery(asNoTracking, includes)
            .Skip((page - 1) * pageSize)
            .Take(pageSize);

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<T>> GetPagedAsync(Expression<Func<T, bool>> predicate, int page, int pageSize, bool asNoTracking = true, CancellationToken cancellationToken = default, params Expression<Func<T, object>>[] includes)
    {
        var query = BuildQuery(asNoTracking, includes)
            .Where(predicate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize);

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, bool asNoTracking = true, CancellationToken cancellationToken = default, params Expression<Func<T, object>>[] includes)
    {
        var query = BuildQuery(asNoTracking, includes).Where(predicate);
        return await query.ToListAsync(cancellationToken);
    }

    public async Task AddAsync(T entity) => await _set.AddAsync(entity);
    public void Update(T entity) => _set.Update(entity);
    public void Remove(T entity) => _set.Remove(entity);

    private IQueryable<T> BuildQuery(bool asNoTracking, params Expression<Func<T, object>>[] includes)
    {
        IQueryable<T> query = _set;

        if (asNoTracking)
        {
            query = query.AsNoTracking();
        }

        foreach (var include in includes)
        {
            query = query.Include(include);
        }

        return query;
    }
}
