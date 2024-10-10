using Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RepositoryAppClient.Contracts
{
    public interface IDbContextAppClient<RepositoryContext>
    {
        RepositoryContext CreateDbContext(string connectionString);
    }
}
