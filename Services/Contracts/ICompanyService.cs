using Entity.ModelsDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Contracts
{
    public interface ICompanyService
    {
        IEnumerable<CompanyDto> GetAllCompanies();
        IEnumerable<CompanyDto> GetAllCompaniesForLogin();
        IEnumerable<CompanyDto> GetPaginatedCompanies(RequestParameters parameters, bool trackChanges);
        CompanyDto GetCompanyById(Guid id);
        CompanyDto CreateCompany(CompanyDto companyDto);
        void UpdateCompany(CompanyDto companyDto);
        void DeleteCompany(Guid id);
    }
}
