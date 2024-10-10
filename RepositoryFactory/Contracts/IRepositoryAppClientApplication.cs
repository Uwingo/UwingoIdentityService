using Entity.Models;
using Entity.ModelsDto;
using RepositoryAppClient.EFCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RepositoryAppClient.Contracts
{
    public interface IRepositoryAppClientApplication : IRepositoryAppClientBase<Application>
    {
        Application GetApplication(Guid id, bool trackChanges);
        public IEnumerable<Application> GetPagedApplications(RequestParameters parameters, bool trackChanges);
    }
}
