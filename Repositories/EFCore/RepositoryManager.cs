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
        private readonly Lazy<IRepositoryUserDbMatch> _repositoryUserDbMatch;
        private readonly Lazy<IRepositoryRoleDbMatch> _repositoryRoleDbMatch;
        private readonly Lazy<IRepositoryUser> _repositoryUser;
        public RepositoryManager(RepositoryContext context)
        {
            _context = context;
            _repositoryApplication = new Lazy<IRepositoryApplication>(() => new RepositoryApplication(_context));
            _repositoryCompany = new Lazy<IRepositoryCompany>(() => new RepositoryCompany(_context));
            _repository = new Lazy<IRepositoryCompanyApplication>(() => new RepositoryCompanyApplication(_context));
            _repositoryRole = new Lazy<IRepositoryRole>(() => new RepositoryRole(_context));
            _repositoryTenant = new Lazy<IRepositoryTenant>(() => new RepositoryTenant(_context));
            _repositoryUserDbMatch = new Lazy<IRepositoryUserDbMatch>(() => new RepositoryUserDbMatch(_context));
            _repositoryRoleDbMatch = new Lazy<IRepositoryRoleDbMatch>(() => new RepositoryRoleDbMatch(_context));
            _repositoryUser = new Lazy<IRepositoryUser>(() => new RepositoryUser(_context));
        }

        public IRepositoryApplication Application => _repositoryApplication.Value;
        public IRepositoryCompany Company => _repositoryCompany.Value;
        public IRepositoryCompanyApplication CompanyApplication => _repository.Value;
        public IRepositoryRole Role => _repositoryRole.Value;
        public IRepositoryTenant Tenant => _repositoryTenant.Value;
        public IRepositoryUserDbMatch UserDbMatch => _repositoryUserDbMatch.Value;
        public IRepositoryRoleDbMatch RoleDbMatch => _repositoryRoleDbMatch.Value;
        public IRepositoryUser User => _repositoryUser.Value;

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
