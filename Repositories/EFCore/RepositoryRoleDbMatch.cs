using Entity.Models;
using Repositories.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.EFCore
{
    public class RepositoryRoleDbMatch : RepositoryBase<RoleDatabaseMatch>, IRepositoryRoleDbMatch
    {
        private readonly RepositoryContext _context;
        public RepositoryRoleDbMatch(RepositoryContext context) : base(context)
        {
            _context = context;
        }
        public RoleDatabaseMatch GetRoleDbMatch(string roleId, bool trackChanges) => GenericReadExpression(x => x.RoleId.Equals(roleId), trackChanges).SingleOrDefault();
        public Guid GetRolesCompanyApplicationId(string roleId, bool trackChanges) => GenericReadExpression(x => x.RoleId.Equals(roleId), trackChanges).SingleOrDefault().CompanyApplicationId;
    }
}