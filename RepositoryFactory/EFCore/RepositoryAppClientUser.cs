using Entity.Models;
using Entity.ModelsDto;
using Microsoft.EntityFrameworkCore;
using RapositoryAppClient;
using Repositories;
using RepositoryAppClient.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace RepositoryAppClient.EFCore
{
    public class RepositoryAppClientUser : RepositoryAppClientBase<UwingoUser>, IRepositoryAppClientUser
    {
        private readonly RepositoryContextAppClient _context;
        public RepositoryAppClientUser(RepositoryContextAppClient context) : base(context)
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

        public IEnumerable<UwingoUser> GetPagedUsers(RequestParameters parameters, bool trackChanges)
        {
            var pagedUsers = _context.Users.AsQueryable().Skip((parameters.PageNumber - 1) * parameters.PageSize)
                             .Take(parameters.PageSize)
                             .ToList();

            return pagedUsers;
        }
    }
}

