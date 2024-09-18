using Entity.ModelsDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Contracts
{
    public interface IUserRoleService
    {
        IEnumerable<UserRoleDto> GetAllUserRoles();
        UserRoleDto GetUserRole(Guid userId, Guid roleId);
        IEnumerable<UserRoleDto> GetUsersByRoleId(Guid roleId);
        IEnumerable<UserRoleDto> GetRolesByUserId(Guid userId);
        UserRoleDto CreateUserRole(UserRoleDto userRoleDto);
        void UpdateUserRole(UserRoleDto userRoleDto);
        void DeleteUserRole(Guid userId, Guid roleId);
    }
}
