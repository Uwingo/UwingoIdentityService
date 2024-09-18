using Microsoft.EntityFrameworkCore;
using Repositories.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.EFCore
{
    public class RepositoryBase<T> : IRepositoryBase<T> where T : class
    {
        private readonly RepositoryContext _context;
        public RepositoryBase(RepositoryContext context)
        {
            _context = context;
        }
        public void GenericCreate(T entity) => _context.Set<T>().Add(entity);

        public void GenericDelete(T entity) => _context.Set<T>().Remove(entity);

        public IEnumerable<T> GenericRead(bool trackChanges) => _context.Set<T>().AsNoTracking();

        public IQueryable<T> GenericReadExpression(Expression<Func<T, bool>> expression, bool trackChanges) =>
            !trackChanges ? _context.Set<T>().Where(expression).AsNoTracking() 
            : _context.Set<T>().Where(expression);

        //public void GenericUpdate(T entity) => _context.Set<T>().Update(entity);

        public void GenericUpdate(T entity)
        {
            var trackedEntity = _context.Set<T>().Local.FirstOrDefault(e => _context.Entry(e).Property("Id").CurrentValue.Equals(_context.Entry(entity).Property("Id").CurrentValue));

            if (trackedEntity != null)
            {
                // Eğer nesne zaten izleniyorsa, mevcut değerlerle güncelle
                _context.Entry(trackedEntity).CurrentValues.SetValues(entity);
            }
            else
            {
                // Eğer izlenmiyorsa, izleme olmadan güncelle
                _context.Set<T>().Update(entity);
            }
        }
        //public void GenericUpdate(T entity)
        //{
        //    try
        //    {
        //        // Local'deki entity'nin var olup olmadığını kontrol ediyoruz
        //        var trackedEntity = _context.Set<T>().Local.FirstOrDefault(e =>
        //            _context.Entry(e).Property("Id").CurrentValue.Equals(_context.Entry(entity).Property("Id").CurrentValue));

        //        if (trackedEntity != null)
        //        {
        //            // Eğer izleniyorsa, mevcut değerlerle güncelliyoruz
        //            _context.Entry(trackedEntity).CurrentValues.SetValues(entity);
        //        }
        //        else
        //        {
        //            // İzlenmiyorsa, entity'yi güncelliyoruz
        //            _context.Set<T>().Update(entity);
        //        }
        //    }
        //    catch (DbUpdateConcurrencyException ex)
        //    {
        //        // Concurrency hatasını yakalayıp, logluyoruz
        //        throw new DbUpdateConcurrencyException("Güncelleme sırasında bir çakışma oluştu.", ex);
        //    }
        //}

    }
}
