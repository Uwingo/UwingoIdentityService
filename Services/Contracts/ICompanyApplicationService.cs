using Entity.Models;
using Entity.ModelsDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Contracts
{
    public interface ICompanyApplicationService
    {
        IEnumerable<CompanyApplicationDto> GetAllCompanyApplications();
        CompanyApplicationDto GetCompanyApplication(Guid id);
        IEnumerable<CompanyApplicationDto> GetApplicationsByCompanyId(Guid companyId);
        IEnumerable<CompanyApplicationDto> GetCompaniesByApplicationId(Guid applicationId);
        IEnumerable<CompanyApplicationDto> GetPaginatedCompanyApplication(RequestParameters parameters, bool trackChanges);
        CompanyApplicationDto CreateCompanyApplication(CompanyApplicationDto companyApplicationDto);
        void UpdateCompanyApplication(CompanyApplicationDto companyApplicationDto);
        void DeleteCompanyApplication(Guid id);
    }
}
