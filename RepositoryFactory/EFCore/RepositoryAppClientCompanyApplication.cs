using Entity.Models;
using Entity.ModelsDto;
using RapositoryAppClient;
using RepositoryAppClient.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RepositoryAppClient.EFCore
{
    public class RepositoryAppClientCompanyApplication : RepositoryAppClientBase<CompanyApplication>, IRepositoryAppClientCompanyApplication
    {
        private readonly RepositoryContextAppClient _context;
        public RepositoryAppClientCompanyApplication(RepositoryContextAppClient context) : base(context)
        {
            _context = context;
        }

        public CompanyApplication GetCompanyApplication(Guid id, bool trackChanges) 
            => GenericReadExpression(ca => ca.Id.Equals(id), trackChanges).SingleOrDefault();
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
