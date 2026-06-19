using System.Linq.Expressions;

namespace ToolInventory.Core.Interfaces;

public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> GetAllAsync(bool asNoTracking = true, CancellationToken cancellationToken = default, params Expression<Func<T, object>>[] includes);
    Task<IEnumerable<T>> GetPagedAsync(int page, int pageSize, bool asNoTracking = true, CancellationToken cancellationToken = default, params Expression<Func<T, object>>[] includes);
    Task<IEnumerable<T>> GetPagedAsync(Expression<Func<T, bool>> predicate, int page, int pageSize, bool asNoTracking = true, CancellationToken cancellationToken = default, params Expression<Func<T, object>>[] includes);
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, bool asNoTracking = true, CancellationToken cancellationToken = default, params Expression<Func<T, object>>[] includes);
    Task AddAsync(T entity);
    void Update(T entity);
    void Remove(T entity);
}
