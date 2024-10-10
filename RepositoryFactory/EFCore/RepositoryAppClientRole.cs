using Entity.Models;
using Entity.ModelsDto;
using RapositoryAppClient;
using RepositoryAppClient.Contracts;
using RepositoryAppClient.EFCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RepositoryAppClient.EFCore
{
    public class RepositoryAppClientRole : RepositoryAppClientBase<Role>, IRepositoryAppClientRole
    {
        private readonly RepositoryContextAppClient _context;
        public RepositoryAppClientRole(RepositoryContextAppClient context) : base(context)
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
