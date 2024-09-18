using Entity.Models;
using Entity.ModelsDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Contracts
{
    public interface IRepositoryRole : IRepositoryBase<Role>
    {
        Role GetRole(string roleId, bool trackChanges);
        public IEnumerable<Role> GetPagedRoles(RequestParameters parameters, bool trackChanges);
    }
}
