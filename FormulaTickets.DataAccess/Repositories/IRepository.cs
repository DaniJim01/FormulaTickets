using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace FormulaTickets.DataAccess.Repositories
{
    /// <summary>
    /// Operaciones CRUD comunes. Los repositories NO llaman SaveChanges:
    /// eso lo hace IUnitOfWork.Complete(), para poder agrupar varias
    /// operaciones en una sola transacción.
    /// </summary>
    public interface IRepository<T> where T : class
    {
        IEnumerable<T> GetAll();
        T GetById(int id);
        IEnumerable<T> Find(Expression<Func<T, bool>> predicate);
        bool Any(Expression<Func<T, bool>> predicate);
        int Count();

        void Add(T entity);
        void Update(T entity);
        void Remove(T entity);
        void RemoveById(int id);
    }
}
