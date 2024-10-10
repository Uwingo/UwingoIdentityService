using Entity.Models;
using Entity.ModelsDto;
using RepositoryAppClient.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RepositoryAppClient.Contracts
{
    public interface IRepositoryAppClientTenant : IRepositoryAppClientBase<Tenant>
    {
        Tenant GetTenantByTenantId(Guid tenantId, bool trackChanges);
        public IEnumerable<Tenant> GetPagedTenants(RequestParameters parameters, bool trackChanges);
    }
}
