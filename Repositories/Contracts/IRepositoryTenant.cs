using Entity.Models;
using Entity.ModelsDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Contracts
{
    public interface IRepositoryTenant : IRepositoryBase<Tenant>
    {
        Tenant GetTenantByTenantId(Guid tenantId, bool trackChanges);
        public IEnumerable<Tenant> GetPagedTenants(RequestParameters parameters, bool trackChanges);
    }
}
