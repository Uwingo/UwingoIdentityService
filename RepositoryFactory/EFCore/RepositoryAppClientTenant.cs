using Entity.Models;
using Entity.ModelsDto;
using RapositoryAppClient;
using RepositoryAppClient.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RepositoryAppClient.EFCore
{
    public class RepositoryAppClientTenant : RepositoryAppClientBase<Tenant>, IRepositoryAppClientTenant
    {
        private readonly RepositoryContextAppClient _context;
        public RepositoryAppClientTenant(RepositoryContextAppClient context) : base(context)
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
