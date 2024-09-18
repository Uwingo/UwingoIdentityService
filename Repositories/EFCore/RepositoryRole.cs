using Entity.Models;
using Entity.ModelsDto;
using Repositories.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.EFCore
{
    public class RepositoryRole : RepositoryBase<Role>, IRepositoryRole
    {
        private readonly RepositoryContext _context;
        public RepositoryRole(RepositoryContext context) : base(context)
        {
            _context = context;
        }

        public IEnumerable<Role> GetPagedRoles(RequestParameters parameters, bool trackChanges)
        {
            var pagedRoles = _context.Roles.AsQueryable().Skip((parameters.PageNumber - 1) * parameters.PageSize)
                             .Take(parameters.PageSize)
                             .ToList();

            return pagedRoles;
        }

        public Role GetRole(string roleId, bool trackChanges) => GenericReadExpression(x => x.Id.Equals(roleId), trackChanges).SingleOrDefault();
    }
}
