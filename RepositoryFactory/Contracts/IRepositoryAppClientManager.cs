using Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RepositoryAppClient.Contracts
{
    public interface IRepositoryAppClientManager
    {
        IRepositoryAppClientApplication Application { get; }
        IRepositoryAppClientCompany Company { get; }
        IRepositoryAppClientCompanyApplication CompanyApplication { get; }
        IRepositoryAppClientRole Role { get; }
        IRepositoryAppClientTenant Tenant { get; }
        IRepositoryAppClientUser User { get; }
        void Save(); //unit of work
    }
}
