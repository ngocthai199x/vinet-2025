using System;
using System.Collections.Concurrent;
using Core.Entities;
using Core.Interfaces;

namespace Infrastructure.Data;

public class UnitOfWork(StoreContext context) : IUnitOfWork
{
    private readonly ConcurrentDictionary<string, object> _reporsitories = new();
    public async Task<bool> Complete()
    {
        return await context.SaveChangesAsync() > 0;
    }
    public IGenericRepository<TEntity> Repository<TEntity>() where TEntity : BaseEntity
    {
        var type = typeof(TEntity).Name;
        return (IGenericRepository<TEntity>)_reporsitories.GetOrAdd(type, t =>{
            var repositoryType = typeof(GenericRepository<>).MakeGenericType(typeof(TEntity));
            return Activator.CreateInstance(repositoryType, context) ?? throw new InvalidOperationException($"Could not create a instance for {t}");
        });
    }
    public void Dispose()
    {
        context.Dispose();
    }
}
