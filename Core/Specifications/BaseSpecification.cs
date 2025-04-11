using System;
using System.Dynamic;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using Core.Interfaces;

namespace Core.Specifications;

public class BaseSpecification<T> : ISpecification<T>
{
    private readonly Expression<Func<T, bool>> _criteria;
    public Expression<Func<T, bool>> Criteria => _criteria;

    public Expression<Func<T, object>>? OrderBy {get; private set;}

    public Expression<Func<T, object>>? OrderByDescending {get; private set;}

    public bool IsDistinct {get; private set;}

    public int Take {get; private set;}

    public int Skip {get; private set;}

    public bool IsPageEnabled {get; private set;}

    public List<Expression<Func<T, object>>> Includes {get;} = [];

    public List<string> IncludeStrings {get;} = [];

    public BaseSpecification(Expression<Func<T, bool>> criteria)
    {
        this._criteria = criteria;
    }
    protected void AddInclude(Expression<Func<T, object>> includeExpression)
    {
        Includes.Add(includeExpression);// Eager loading first level
    }
    protected void AddInclude(string includeString){
        IncludeStrings.Add(includeString); // For ThenInclude in Eager loading
    }
    protected void AddOrderBy(Expression<Func<T,object>> orderByExpression)
    {
        OrderBy = orderByExpression;
    }
    protected void AddOrderByDescending(Expression<Func<T,object>> orderByDescExpression)
    {
        OrderByDescending = orderByDescExpression;
    }
    protected void ApplyDistinct()
    {
        IsDistinct = true;
    }
    protected void ApplyPaging(int skip, int take)
    {
        Skip = skip;
        Take = take;
        IsPageEnabled = true;
    }

    public IQueryable<T> ApplyCriteria(IQueryable<T> query)
    {
        if(Criteria!= null)
        {
            query = query.Where(Criteria);
        }
        return query;
    }
}
public class BaseSpecification<T, TResult> : BaseSpecification<T>, ISpecification<T, TResult>
{
    protected BaseSpecification() : base(null!){}
    public BaseSpecification(Expression<Func<T, bool>> criteria) : base(criteria)
    {
    }

    public Expression<Func<T, TResult>>? Select {get; private set;}
    protected void AddSelect(Expression<Func<T, TResult>> selectExpression)
    {
        Select = selectExpression;
    }
}
