using Entity.Models;
using Entity.ModelsDto;
using Microsoft.EntityFrameworkCore;
using Repositories.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.EFCore
{
    public class RepositoryUser : RepositoryBase<User>, IRepositoryUser
    {
        private readonly RepositoryContext _context;
        public RepositoryUser(RepositoryContext context) : base(context)
        {
            _context = context;
        }

        public async Task<List<Claim>> GetAllClaimsAsync()
        {
            var userClaims = await _context.UserClaims 
                .Select(uc => new Claim(uc.ClaimType, uc.ClaimValue))
                .ToListAsync();

            return userClaims;
        }

        public IEnumerable<User> GetPagedUsers(RequestParameters parameters, bool trackChanges)
        {
            var pagedUsers = _context.Users.AsQueryable().Skip((parameters.PageNumber - 1) * parameters.PageSize)
                             .Take(parameters.PageSize)
                             .ToList();

            return pagedUsers;
        }
    }
}

