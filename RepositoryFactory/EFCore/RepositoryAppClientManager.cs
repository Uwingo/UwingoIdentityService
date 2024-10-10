using RapositoryAppClient;
using Repositories.Contracts;
using RepositoryAppClient.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RepositoryAppClient.EFCore
{
    public class RepositoryAppClientManager : IRepositoryAppClientManager
    {
        private readonly RepositoryContextAppClient _context;
        private readonly Lazy<IRepositoryAppClientApplication> _repositoryApplication;
        private readonly Lazy<IRepositoryAppClientCompany> _repositoryCompany;
        private readonly Lazy<IRepositoryAppClientCompanyApplication> _repository;
        private readonly Lazy<IRepositoryAppClientRole> _repositoryRole;
        private readonly Lazy<IRepositoryAppClientTenant> _repositoryTenant;
        private readonly Lazy<IRepositoryAppClientUser> _repositoryUser;
        public RepositoryAppClientManager(RepositoryContextAppClient context)
        {
            _context = context;
            _repositoryApplication = new Lazy<IRepositoryAppClientApplication>(() => new RepositoryAppClientApplication(_context));
            _repositoryCompany = new Lazy<IRepositoryAppClientCompany>(() => new RepositoryAppClientCompany(_context));
            _repository = new Lazy<IRepositoryAppClientCompanyApplication>(() => new RepositoryAppClientCompanyApplication(_context));
            _repositoryRole = new Lazy<IRepositoryAppClientRole>(() => new RepositoryAppClientRole(_context));
            _repositoryTenant = new Lazy<IRepositoryAppClientTenant>(() => new RepositoryAppClientTenant(_context));
            _repositoryUser = new Lazy<IRepositoryAppClientUser>(() => new RepositoryAppClientUser(_context));
        }

        public IRepositoryAppClientApplication Application => _repositoryApplication.Value;
        public IRepositoryAppClientCompany Company => _repositoryCompany.Value;
        public IRepositoryAppClientCompanyApplication CompanyApplication => _repository.Value;
        public IRepositoryAppClientRole Role => _repositoryRole.Value;
        public IRepositoryAppClientTenant Tenant => _repositoryTenant.Value;
        public IRepositoryAppClientUser User => _repositoryUser.Value;

        public void Save()
        {
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    _context.SaveChanges();
                    transaction.Commit(); // Başarılıysa commit et
                }
                catch (Exception ex)
                {
                    transaction.Rollback(); // Hata durumunda rollback
                    throw; // Hata fırlatılmaya devam edilir
                }
            }
        }

    }
}
