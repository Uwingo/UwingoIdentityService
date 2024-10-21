using Entity.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Contracts
{
    public interface IRepositoryRoleDbMatch : IRepositoryBase<RoleDatabaseMatch>
    {
        RoleDatabaseMatch GetRoleDbMatch(string roleId, bool trackChanges);
        Guid GetRolesCompanyApplicationId(string roleId, bool trackChanges);
    }
}
