using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;

namespace FormulaTickets.DataAccess.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly FormulaTicketsDBEntities Context;
        protected readonly DbSet<T> DbSet;

        public Repository(FormulaTicketsDBEntities context)
        {
            if (context == null) throw new ArgumentNullException("context");
            Context = context;
            DbSet = context.Set<T>();
        }

        public virtual IEnumerable<T> GetAll()
        {
            return DbSet.AsNoTracking().ToList();
        }

        public virtual T GetById(int id)
        {
            return DbSet.Find(id);
        }

        public virtual IEnumerable<T> Find(Expression<Func<T, bool>> predicate)
        {
            return DbSet.AsNoTracking().Where(predicate).ToList();
        }

        public virtual bool Any(Expression<Func<T, bool>> predicate)
        {
            return DbSet.Any(predicate);
        }

        public virtual int Count()
        {
            return DbSet.Count();
        }

        public virtual void Add(T entity)
        {
            DbSet.Add(entity);
        }

        /// <summary>
        /// Marca la entidad (normalmente llegada desde un formulario, desconectada)
        /// como modificada. Si ya hay otra instancia con el mismo id rastreada en este
        /// contexto, EF lanzará excepción: por eso las validaciones previas deben usar AsNoTracking.
        /// </summary>
        public virtual void Update(T entity)
        {
            Context.Entry(entity).State = EntityState.Modified;
        }

        public virtual void Remove(T entity)
        {
            if (Context.Entry(entity).State == EntityState.Detached)
                DbSet.Attach(entity);
            DbSet.Remove(entity);
        }

        public virtual void RemoveById(int id)
        {
            var entity = DbSet.Find(id);
            if (entity != null) DbSet.Remove(entity);
        }
    }
}
