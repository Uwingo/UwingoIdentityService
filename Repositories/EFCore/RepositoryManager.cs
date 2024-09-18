    using Repositories.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.EFCore
{
    public class RepositoryManager : IRepositoryManager
    {
        private readonly RepositoryContext _context;
        private readonly Lazy<IRepositoryApplication> _repositoryApplication;
        private readonly Lazy<IRepositoryCompany> _repositoryCompany;
        private readonly Lazy<IRepositoryCompanyApplication> _repository;
        private readonly Lazy<IRepositoryRole> _repositoryRole;
        private readonly Lazy<IRepositoryTenant> _repositoryTenant;
        private readonly Lazy<IRepositoryUserRole> _repositoryUserRole;
        private readonly Lazy<IRepositoryUser> _repositoryUser;
        public RepositoryManager(RepositoryContext context)
        {
            _context = context;
            _repositoryApplication = new Lazy<IRepositoryApplication>(() => new RepositoryApplication(_context));
            _repositoryCompany = new Lazy<IRepositoryCompany>(() => new RepositoryCompany(_context));
            _repository = new Lazy<IRepositoryCompanyApplication>(() => new RepositoryCompanyApplication(_context));
            _repositoryRole = new Lazy<IRepositoryRole>(() => new RepositoryRole(_context));
            _repositoryTenant = new Lazy<IRepositoryTenant>(() => new RepositoryTenant(_context));
            _repositoryUserRole = new Lazy<IRepositoryUserRole>(() => new RepositoryUserRole(_context));
            _repositoryUser = new Lazy<IRepositoryUser>(() => new RepositoryUser(_context));
        }

        public IRepositoryApplication Application => _repositoryApplication.Value;
        public IRepositoryCompany Company => _repositoryCompany.Value;
        public IRepositoryCompanyApplication CompanyApplication => _repository.Value;
        public IRepositoryRole Role => _repositoryRole.Value;
        public IRepositoryTenant Tenant => _repositoryTenant.Value;
        public IRepositoryUserRole UserRole => _repositoryUserRole.Value;
        public IRepositoryUser User => _repositoryUser.Value;

        //public void Save()
        //{
        //    _context.SaveChanges();
        //}

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
