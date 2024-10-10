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
    public class RepositoryAppClientCompany : RepositoryAppClientBase<Company>, IRepositoryAppClientCompany
    {
        private readonly RepositoryContextAppClient _context;
        public RepositoryAppClientCompany(RepositoryContextAppClient context) : base(context)
        {
            _context = context;
        }

        public Company GetCompany(Guid id, bool trackChanges) => GenericReadExpression(x => x.Id.Equals(id), trackChanges).SingleOrDefault();

        public IEnumerable<Company> GetPagedCompanies(RequestParameters parameters, bool trackChanges)
        {
            var pagedCompanies = _context.Companies.AsQueryable().Skip((parameters.PageNumber - 1) * parameters.PageSize)
                             .Take(parameters.PageSize)
                             .ToList();

            return pagedCompanies;
        }
    }
}
