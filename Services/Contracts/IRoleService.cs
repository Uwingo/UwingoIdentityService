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
        RoleDto GetRoleById(string id);
        RoleDto CreateRole(RoleDto roleDto);
        void UpdateRole(RoleDto roleDto);
        void DeleteRole(string id);
    }
}
