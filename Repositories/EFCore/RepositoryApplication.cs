using Entity.Models;
using Entity.ModelsDto;
using Repositories.Contracts;

namespace Repositories.EFCore
{
    public class RepositoryApplication : RepositoryBase<Application>, IRepositoryApplication
    {
        private readonly RepositoryContext _context;

        public RepositoryApplication(RepositoryContext context) : base(context) 
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