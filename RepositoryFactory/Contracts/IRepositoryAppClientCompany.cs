using Entity.Models;
using Entity.ModelsDto;
using RepositoryAppClient.Contracts;
using RepositoryAppClient.EFCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RepositoryAppClient.Contracts
{
    public interface IRepositoryAppClientCompany : IRepositoryAppClientBase<Company>
    {
        Company GetCompany(Guid id, bool trackChanges);
        public IEnumerable<Company> GetPagedCompanies(RequestParameters parameters, bool trackChanges);
    }
}
