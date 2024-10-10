using Entity.Models;
using Entity.ModelsDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace RepositoryAppClient.Contracts
{
    public interface IRepositoryAppClientUser : IRepositoryAppClientBase<User>
    {
        Task<List<Claim>> GetAllClaimsAsync();
        public IEnumerable<User> GetPagedUsers(RequestParameters parameters, bool trackChanges);
    }
}
