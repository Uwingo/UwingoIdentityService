using Entity.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Contracts
{
    public interface IRepositoryUserDbMatch : IRepositoryBase<UserDatabaseMatch>
    {
        UserDatabaseMatch GetUserDbMatch(string userId, bool trackChanges);
        Guid GetUsersCompanyApplicationId(string userId, bool trackChanges);
        Guid GetUsersCAIdByUserName(string userName, bool trackChanges);

    }
}
