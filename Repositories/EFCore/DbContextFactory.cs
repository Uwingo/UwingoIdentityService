using Microsoft.EntityFrameworkCore;
using Repositories.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.EFCore
{
    public class DbContextFactory : IDbContextFactory
    {
        private readonly DbContextOptions<RepositoryContext> _options;

        public DbContextFactory(DbContextOptions<RepositoryContext> options)
        {
            _options = options;
        }
        public RepositoryContext CreateDbContext(string connectionString)
        {
            var optionsBuilder = new DbContextOptionsBuilder<RepositoryContext>();
            optionsBuilder.UseSqlServer(connectionString);

            return new RepositoryContext(optionsBuilder.Options);
        }
    }
}
