using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Linq.Expressions;
using POSSUM.Model;

namespace POSSUM.Data
{ 
    public class GenericRepository<T> : IGenericRepository<T>
                                       where T : BaseEntity 
    {
        private readonly ApplicationDbContext context;
        private IDbSet<T> _dbSet;

        public GenericRepository(ApplicationDbContext context)
        {
            this.context = context;
            _dbSet = context.Set<T>();
        }

        #region IGenericRepository<T> implementation

        public virtual IQueryable<T> AsQueryable()
        {
            return _dbSet.AsQueryable();
        }

        public virtual IEnumerable<T> GetAll()
        {
            return _dbSet;
        }

        public int Count()
        {
            return _dbSet.Count();
        }

        public IEnumerable<T> Find(Expression<Func<T, bool>> predicate)
        {
            return _dbSet.Where(predicate);
        }

        public virtual IEnumerable<T> Where(Expression<Func<T, bool>> predicate)
        {
            return _dbSet.Where(predicate);
        }

        public T Single(Expression<Func<T, bool>> predicate)
        {
            return _dbSet.Single(predicate);
        }

        public T SingleOrDefault(Expression<Func<T, bool>> predicate)
        {
            return _dbSet.SingleOrDefault(predicate);
        }

        public T First(Expression<Func<T, bool>> predicate)
        {
            return _dbSet.First(predicate);
        }

        public T FirstOrDefault(Expression<Func<T, bool>> predicate)
        {
            return _dbSet.FirstOrDefault(predicate);
        }

        public void Save()
        {
            context.SaveChanges();
        }

        public T First()
        {
            return _dbSet.First();
        }

        public T FirstOrDefault()
        {
            return _dbSet.FirstOrDefault();
        }

        public bool Any()
        {
            return _dbSet.Any();
        }

        public bool Any(Expression<Func<T, bool>> predicate)
        {
            return _dbSet.Any(predicate);
        }

        public long Max(Expression<Func<T, long>> predicate)
        {
            return _dbSet.Max(predicate);
        }

        public T GetById(object id)
        {
            return _dbSet.Find(id);
        }

        public  virtual void Add(T entity)
        {
            _dbSet.Add(entity);
        }

        public void AddOrUpdate(T entity)
        {
            _dbSet.AddOrUpdate(entity);
        }

        public void Delete(T entity)
        {
            _dbSet.Remove(entity);
        }

        public void Attach(T entity)
        {
            _dbSet.Attach(entity);          
        }

        // Customer Methods
        public virtual IList<T> List(List<Expression<Func<T, bool>>> conditions, Func<T, object> orderBy,
          OrderByDirecton direction, int page, int pageSize, out int records)
        {
            IQueryable<T> select = _dbSet.AsQueryable();
            IQueryable<T> count = _dbSet.AsQueryable();
            if (conditions != null && conditions.Any())
            {
                foreach (var condition in conditions)
                {
                    select = select.Where(condition);
                    count = count.Where(condition);
                }
            }
            records = count.Count();
            var result = new List<T>();
            if (orderBy != null)
            {
                if (direction != OrderByDirecton.ASC)
                {
                    result = (pageSize >= records
                       ? @select.OrderByDescending(orderBy).ToList()
                       : @select.OrderByDescending(orderBy)
                           .Skip(page * pageSize)
                           .Take(pageSize)
                           .ToList());
                }
                else
                {
                    result = (pageSize >= records
                       ? @select.OrderBy(orderBy).ToList()
                       : @select.OrderBy(orderBy).Skip(page * pageSize).Take(pageSize).ToList());
                }
            }
            return result.Any() ? result : @select.ToList();
        }

        #endregion IGenericRepository<T> implementation


        




    }
}