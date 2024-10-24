﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace RepositoryAppClient.Contracts
{
    public interface IRepositoryAppClientBase<T>
    {
        void GenericCreate(T entity);
        IEnumerable<T> GenericRead(bool trackChanges);
        void GenericUpdate(T entity);
        void GenericDelete(T entity);
        IQueryable<T> GenericReadExpression(Expression<Func<T, bool>> expression, bool trackChanges);
    }
}
