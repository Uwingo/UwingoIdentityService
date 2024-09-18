using Entity.Models;
using Entity.ModelsDto;
using Microsoft.EntityFrameworkCore;
using Repositories.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.EFCore
{
    public class RepositoryTenant : RepositoryBase<Tenant>, IRepositoryTenant
    {
        private readonly RepositoryContext _context;
        public RepositoryTenant(RepositoryContext context) : base(context)
        {
            _context = context;
        }

        public IEnumerable<Tenant> GetPagedTenants(RequestParameters parameters, bool trackChanges)
        {
            var pagedTenants = _context.Tenants.AsQueryable().Skip((parameters.PageNumber - 1) * parameters.PageSize)
                             .Take(parameters.PageSize)
                             .ToList();

            return pagedTenants;
        }

        public Tenant GetTenantByTenantId(Guid tenantId, bool trackChanges) => GenericReadExpression(x => x.Id.Equals(tenantId), trackChanges).SingleOrDefault();


    }
}
