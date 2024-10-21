using Entity.Models;
using Repositories.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.EFCore
{
    public class RepositoryUserDbMatch : RepositoryBase<UserDatabaseMatch>, IRepositoryUserDbMatch
    {
        private readonly RepositoryContext _context;

        public RepositoryUserDbMatch(RepositoryContext context) : base(context)
        {
            _context = context;
        }
        public UserDatabaseMatch GetUserDbMatch(string userId, bool trackChanges) => GenericReadExpression(x => x.UserId.Equals(userId), trackChanges).SingleOrDefault();

        public Guid GetUsersCAIdByUserName(string userName, bool trackChanges) => GenericReadExpression(x => x.UserName.Equals(userName), trackChanges).SingleOrDefault().CompanyApplicationId;

        public Guid GetUsersCompanyApplicationId(string userId, bool trackChanges) => GenericReadExpression(x => x.UserId.Equals(userId), trackChanges).SingleOrDefault().CompanyApplicationId;
    }
}
