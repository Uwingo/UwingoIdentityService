using Entity.Models;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Repositories.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.EFCore
{
    public class RepositoryUserRole : RepositoryBase<UserRole>, IRepositoryUserRole
    {
        private readonly RepositoryContext _context;
        public RepositoryUserRole(RepositoryContext context) : base(context)
        {
            _context = context;
        }

        public UserRole GetUserRole(Guid userId, Guid roleId, bool trackChanges) => GenericReadExpression(x => x.RoleId.Equals(roleId) && x.UserId.Equals(userId), trackChanges).SingleOrDefault();

        public UserRole GetUserRoleByRoleId(Guid roleId, bool trackChanges) => GenericReadExpression(x => x.RoleId.Equals(roleId), trackChanges).SingleOrDefault();

        public UserRole GetUserRoleByUserId(Guid userId, bool trackChanges) => GenericReadExpression(x => x.UserId.Equals(userId), trackChanges).SingleOrDefault();

        public IEnumerable<EntityEntry<UserRole>> GetTrackedUserRoles()
        {
            return _context.ChangeTracker.Entries<UserRole>();
        }
    }
}
