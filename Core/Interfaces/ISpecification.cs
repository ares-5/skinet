using System.Linq.Expressions;

namespace Core.Interfaces;

public interface ISpecification<T>
{
    int Take { get; }
    int Skip { get; }
    bool IsPagingEnabled { get; }
    bool IsDistinct { get; }
    
    Expression<Func<T, bool>>? Criteria { get; }
    
    Expression<Func<T, object>>? OrderBy { get; }
    
    Expression<Func<T, object>>? OrderByDescending { get; }

    IQueryable<T> ApplyCriteria(IQueryable<T> query);
}

public interface ISpecification<T, TResult> : ISpecification<T>
{
    Expression<Func<T, TResult>>? Select { get; }
}