using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace POSSUM.Data
{
    public interface IGenericRepository<T>  where T : class
    {
        IQueryable<T> AsQueryable();

        IEnumerable<T> GetAll();
        IEnumerable<T> Where(Expression<Func<T, bool>> predicate);
        IEnumerable<T> Find(Expression<Func<T, bool>> predicate);
        T Single(Expression<Func<T, bool>> predicate);
        T SingleOrDefault(Expression<Func<T, bool>> predicate);
        T First(Expression<Func<T, bool>> predicate);
        T FirstOrDefault(Expression<Func<T, bool>> predicate);
        T FirstOrDefault();
        T First();
        T GetById(object id);
        long Max(Expression<Func<T, long>> predicate);
        int Count();
        bool Any();
        bool Any(Expression<Func<T, bool>> predicate);
        void Add(T entity);
        void AddOrUpdate(T entity);
        void Delete(T entity);
        void Attach(T entity);

        //Custom Methods
        IList<T> List(List<Expression<Func<T, bool>>> conditions, Func<T, object> orderBy,
          OrderByDirecton direction, int page, int pageSize, out int records);
    }
}
