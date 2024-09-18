using Entity.ModelsDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Contracts
{
    public interface ITenantService
    {
        IEnumerable<TenantDto> GetAllTenants();
        IEnumerable<TenantDto> GetPaginatedTenants(RequestParameters parameters, bool trackChanges);
        TenantDto GetTenantById(Guid id);
        TenantDto CreateTenant(TenantDto tenantDto);
        void UpdateTenant(TenantDto tenantDto);
        void DeleteTenant(Guid id);
    }
}
