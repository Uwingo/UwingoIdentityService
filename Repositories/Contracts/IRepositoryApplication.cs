using Entity.Models;
using Entity.ModelsDto;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Contracts
{
    public interface IRepositoryApplication : IRepositoryBase<Application>
    {
        Application GetApplication(Guid id, bool trackChanges);
        public IEnumerable<Application> GetPagedApplications(RequestParameters parameters, bool trackChanges);
    }
}
