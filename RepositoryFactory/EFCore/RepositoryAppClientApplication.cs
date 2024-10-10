using Entity.Models;
using Entity.ModelsDto;
using RapositoryAppClient;
using RepositoryAppClient.Contracts;

namespace RepositoryAppClient.EFCore
{
    public class RepositoryAppClientApplication : RepositoryAppClientBase<Application>, IRepositoryAppClientApplication
    {
        private readonly RepositoryContextAppClient _context;

        public RepositoryAppClientApplication(RepositoryContextAppClient context) : base(context) 
        {
            _context = context;
        }

        public Application GetApplication(Guid id, bool trackChanges) => GenericReadExpression(x => x.Id.Equals(id), trackChanges).SingleOrDefault();

        public IEnumerable<Application> GetPagedApplications(RequestParameters parameters, bool trackChanges)
        {
            var pagedApplications = _context.Applications.AsQueryable().Skip((parameters.PageNumber - 1) * parameters.PageSize)
                             .Take(parameters.PageSize)
                             .ToList();

            return pagedApplications;
        }
    }
}