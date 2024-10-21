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
        private readonly Lazy<IRepositoryAppClientRole> _repositoryRole;
        private readonly Lazy<IRepositoryAppClientUser> _repositoryUser;
        public RepositoryAppClientManager(RepositoryContextAppClient context)
        {
            _context = context;
            _repositoryRole = new Lazy<IRepositoryAppClientRole>(() => new RepositoryAppClientRole(_context));
            _repositoryUser = new Lazy<IRepositoryAppClientUser>(() => new RepositoryAppClientUser(_context));
        }

        public IRepositoryAppClientRole Role => _repositoryRole.Value;
        public IRepositoryAppClientUser User => _repositoryUser.Value;
        //public IRepositoryAppClientUserRole UserRole => _repositoryUserRole.Value;

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
        public RepositoryContextAppClient GetContext()
        {
            return _context;
        }
    }
}
