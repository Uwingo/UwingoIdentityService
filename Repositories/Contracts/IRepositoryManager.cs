using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Contracts
{
    public interface IRepositoryManager
    {
        IRepositoryApplication Application { get; }
        IRepositoryCompany Company { get; }
        IRepositoryCompanyApplication CompanyApplication { get; }
        IRepositoryRole Role { get; }
        IRepositoryTenant Tenant { get; }
        IRepositoryUserRole UserRole { get; }
        IRepositoryUser User { get; }
        void Save(); //unit of work
    }
}
