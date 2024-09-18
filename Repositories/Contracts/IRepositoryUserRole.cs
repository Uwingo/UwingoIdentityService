using Entity.Models;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Contracts
{
    public interface IRepositoryUserRole : IRepositoryBase<UserRole>
    {
        UserRole GetUserRole(Guid userId, Guid roleId, bool trackChanges);
        UserRole GetUserRoleByUserId(Guid userId, bool trackChanges);
        UserRole GetUserRoleByRoleId(Guid roleId, bool trackChanges); 
        IEnumerable<EntityEntry<UserRole>> GetTrackedUserRoles();
    }
}
