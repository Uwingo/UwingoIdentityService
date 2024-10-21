using Entity.ModelsDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Contracts
{
    public interface IRoleService
    {
        IEnumerable<RoleDto> GetAllRoles();
        IEnumerable<RoleDto> GetPaginatedRoles(RequestParameters parameters, bool trackChanges);
        Task<RoleDto> GetRoleById(string id);
        Task<RoleDto> CreateRole(RoleDto roleDto);
        Task UpdateRole(RoleDto roleDto);
        Task DeleteRole(string id);
    }
}
