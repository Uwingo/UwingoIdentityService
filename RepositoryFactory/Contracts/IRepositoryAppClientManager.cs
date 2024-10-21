using RapositoryAppClient;
using Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RepositoryAppClient.Contracts
{
    public interface IRepositoryAppClientManager
    {
        IRepositoryAppClientRole Role { get; }
        IRepositoryAppClientUser User { get; }
        void Save(); //unit of work
        RepositoryContextAppClient GetContext();
    }
}
