﻿namespace Data.Repositories
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Linq;
    using System.Linq.Expressions;

    using Data.Contracts;

    public class EFRepository<T> : IRepository<T>, IQueryable<T> where T : class
    {
        private readonly DbContext context;

        private readonly IDbSet<T> set;

        public EFRepository(DbContext context)
        {
            this.context = context;
            this.set = context.Set<T>();
        }

        public EFRepository()
            : this(new WorldOfWordsDbContext())
        {
        }

        public Expression Expression
        {
            get { return this.set.Expression; }
        }

        public Type ElementType
        {
            get { return this.set.ElementType; }
        }

        public IQueryProvider Provider
        {
            get { return this.set.Provider; }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return this.set.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable) this.set).GetEnumerator();
        }

        public IQueryable<T> All()
        {
            return this.set.AsQueryable();
        }

        public T Add(T entity)
        {
            var entry = this.AttachIfDetached(entity);
            entry.State = EntityState.Added;
            return entity;
        }

        public T Update(T entity)
        {
            var entry = this.AttachIfDetached(entity);
            entry.State = EntityState.Modified;
            return entity;
        }

        public T Delete(T entity)
        {
            var entry = this.AttachIfDetached(entity);
            entry.State = EntityState.Deleted;
            return entity;
        }

        public T Detach(T entity)
        {
            var entry = this.context.Entry(entity);
            entry.State = EntityState.Detached;
            return entity;
        }

        public void SaveChanges()
        {
            this.context.SaveChanges();
        }

        private DbEntityEntry AttachIfDetached(T entity)
        {
            var entry = this.context.Entry(entity);

            if (entry.State == EntityState.Detached)
            {
                this.set.Attach(entity);
            }

            return entry;
        }
    }
}