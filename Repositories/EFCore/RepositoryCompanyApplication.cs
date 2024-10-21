using Entity.Models;
using Entity.ModelsDto;
using Repositories.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.EFCore
{
    public class RepositoryCompanyApplication : RepositoryBase<CompanyApplication>, IRepositoryCompanyApplication
    {
        private readonly RepositoryContext _context;
        public RepositoryCompanyApplication(RepositoryContext context) : base(context)
        {
            _context = context;
        }

        public CompanyApplication GetCompanyApplication(Guid id, bool trackChanges) 
            => GenericReadExpression(ca => ca.Id.Equals(id), trackChanges).SingleOrDefault();

        public CompanyApplication GetCompanyApplicationByApplicationAndCompanyId(Guid companyId, Guid applicationId, bool trackChanges)
            => GenericReadExpression(ca => ca.CompanyId.Equals(companyId) && ca.ApplicationId.Equals(applicationId), trackChanges).SingleOrDefault();

        public IEnumerable<CompanyApplication> GetCompanyApplicationByApplicationId(Guid applicationId, bool trackChanges) 
            => GenericReadExpression(x => x.ApplicationId.Equals(applicationId), trackChanges);
        public IEnumerable<CompanyApplication> GetCompanyApplicationByCompanyId(Guid companyId, bool trackChanges) 
            => GenericReadExpression(x => x.CompanyId.Equals(companyId), trackChanges);

        public IEnumerable<CompanyApplication> GetPagedCompanyApplications(RequestParameters parameters, bool trackChanges)
        {
            var pagedCompanyApplications = _context.CompanyApplications.AsQueryable().Skip((parameters.PageNumber - 1) * parameters.PageSize)
                             .Take(parameters.PageSize)
                             .ToList();

            return pagedCompanyApplications;
        }
    }
}
