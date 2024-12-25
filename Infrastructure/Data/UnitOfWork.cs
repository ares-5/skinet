using System.Collections.Concurrent;
using Core.Entities;
using Core.Interfaces;

namespace Infrastructure.Data;

public class UnitOfWork(StoreContext context) : IUnitOfWork
{
    private readonly StoreContext context = context;
    private readonly ConcurrentDictionary<string, object> repositories = [];

    public IRepository<TEntity> Repository<TEntity>() where TEntity : BaseEntity
    {
        var type = typeof(TEntity).Name;
        return (IRepository<TEntity>)repositories.GetOrAdd(type, t =>
        {
            var repositoryType = typeof(Repository<>).MakeGenericType(typeof(TEntity));
            return Activator.CreateInstance(repositoryType, context) 
                   ?? throw new InvalidOperationException($"Could not create repository instance for {t}");
        });
    }

    public async Task<bool> CompleteAsync()
    {
        return await context.SaveChangesAsync() > 0;
    }
    
    public void Dispose()
    {
        context.Dispose();
    }
}