using Entity.Models;
using Entity.ModelsDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Contracts
{
    public interface IRepositoryCompany : IRepositoryBase<Company>
    {
        Company GetCompany(Guid id, bool trackChanges);
        public IEnumerable<Company> GetPagedCompanies(RequestParameters parameters, bool trackChanges);
    }
}
