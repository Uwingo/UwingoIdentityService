using AutoMapper;
using Entity.Models;
using Entity.ModelsDto;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using RapositoryAppClient;
using Repositories;
using Repositories.Contracts;
using Services.Contracts;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Services.EFCore
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthenticationService> _logger;
        private User _user;
        private readonly IRepositoryManager _repo;
        private readonly IEmailService _emailService;

        public AuthenticationService(UserManager<User> userManager, RoleManager<Role> roleManager,IMapper mapper, IConfiguration configuration, ILogger<AuthenticationService> logger, IRepositoryManager repo, IEmailService emailService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _mapper = mapper;
            _configuration = configuration;
            _logger = logger;
            _repo = repo;
            _emailService = emailService;
        }

        public async Task<IdentityResult> RegisterUser(UserRegistrationDto userRegistrationDto)
        {
            try
            {
                var role = await _roleManager.FindByIdAsync(userRegistrationDto.RoleId);
   
                var user = _mapper.Map<User>(userRegistrationDto);
                user.RefreshToken = GenerateRefreshToken();
                user.RefreshTokenExpiryTime = DateTime.Now.AddDays(7);

                var result = await _userManager.CreateAsync(user, userRegistrationDto.Password);

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, role.Name);
                    _logger.LogError("Kullanıcı başarıyla kaydedildi ve role atandı.");
                }
                else _logger.LogError("Kullanıcı kaydı başarısız.");
                

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError("Kullanıcı kaydedilirken bir hata oluştu: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<bool> ValidateUser(UserLoginDto userLoginDto)
        {
            try
            {
                // Kullanıcıyı bul
                _user = await _userManager.FindByNameAsync(userLoginDto.UserName);
                var result = _user != null && await _userManager.CheckPasswordAsync(_user, userLoginDto.Password);

                // Eğer kullanıcı doğrulanırsa devam et
                if (result)
                {
                    // Kullanıcıya bağlı CompanyApplicationId'yi al
                    Guid companyApplicationId = _user.CompanyApplicationId;

                    // CompanyApplicationId'ye bağlı ApplicationId'yi al
                    Guid applicationId = _repo.CompanyApplication.GetCompanyApplication(companyApplicationId, false).ApplicationId;

                    // ApplicationId'ye bağlı connection string'i al
                    string dbString = _repo.Application.GetApplication(applicationId, false).DbConnection;

                    // Yeni bir bağlam oluşturun (RepositoryContextAppClient)
                    var newContext = await ChangeDatabase(dbString);

                    // Diğer servislerde de yeni bağlamı kullanmak istiyorsanız, o servislere bu bağlamı geçirmeniz gerekecek
                    _logger.LogInformation("Kullanıcı başarıyla doğrulandı ve veritabanı bağlantısı sağlandı.");
                }
                else
                {
                    _logger.LogError("Kullanıcı doğrulama başarısız.");
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError("Kullanıcı doğrulanırken bir hata oluştu: {Message}", ex.Message);
                throw;
            }
        }

        private async Task<RepositoryContextAppClient> ChangeDatabase(string dbString)
        {
            var optionsBuilder = new DbContextOptionsBuilder<RepositoryContextAppClient>();
            optionsBuilder.UseSqlServer(dbString);
            var newContext = new RepositoryContextAppClient(optionsBuilder.Options);
            return newContext;
        }

        public async Task<TokenDto> CreateToken(bool populateExp)
        {
            var signingCredentials = GetSigninCredentials();
            var claims = await GetClaims(_user);
            var tokenOptions = GenerateTokenOptions(signingCredentials, claims);

            var refreshToken = GenerateRefreshToken();
            _user.RefreshToken = refreshToken;

            if (populateExp) _user.RefreshTokenExpiryTime = DateTime.Now.AddDays(7);

            await _userManager.UpdateAsync(_user); // Veritabanındaki RefreshToken'ı güncelle

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.WriteToken(tokenOptions);

            _logger.LogError("Token başarıyla oluşturuldu.");
            return new TokenDto
            {
                AccessToken = token,
                RefreshToken = refreshToken
            };
        }

        private JwtSecurityToken GenerateTokenOptions(SigningCredentials signingCredentials, IEnumerable<Claim> claims)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");

            var tokenOptions = new JwtSecurityToken(
                issuer: jwtSettings.GetSection("ValidateIssue").Value,
                audience: jwtSettings.GetSection("ValidateAudience").Value,
                expires: DateTime.Now.AddMinutes(Convert.ToDouble(jwtSettings.GetSection("Expire").Value)),
                claims: claims,
                signingCredentials: signingCredentials);

            return tokenOptions;
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        public async Task<List<Claim>> GetClaims(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim("uid", user.Id)
            };

            var roles = await _userManager.GetRolesAsync(user);
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
                var usersRole = await _roleManager.FindByNameAsync(role);
                if (usersRole is not null)
                {
                    var roleClaims = await GetRoleClaimsAsync(usersRole.Id);
                    foreach (var roleClaim in roleClaims)
                    {
                        claims.Add(new Claim(roleClaim.Type, roleClaim.Value));
                    }
                }
            }

            var userClaims = await GetUserClaimsAsync(user.Id);

            foreach (var userClaim in userClaims)
            {
                claims.Add(new Claim(userClaim.Type, userClaim.Value));
            }

            return claims;
        }

        private SigningCredentials GetSigninCredentials()
        {
            var key = Encoding.UTF8.GetBytes(_configuration["JwtSettings:SecretKey"]);
            var secret = new SymmetricSecurityKey(key);

            return new SigningCredentials(secret, SecurityAlgorithms.HmacSha256);
        }

        public async Task<TokenDto> RefreshToken(TokenDto tokenDto)
        {
            var principal = GetPrincipalFromExpiredToken(tokenDto.AccessToken);
            var user = await _userManager.FindByNameAsync(principal.Identity.Name);

            if (user == null || user.RefreshToken != tokenDto.RefreshToken || user.RefreshTokenExpiryTime <= DateTime.Now)
            {
                _logger.LogWarning("Geçersiz refresh token.");
                throw new SecurityTokenException("Invalid refresh token");
            }
            _user = user;

            var newToken = await CreateToken(false);
            user.RefreshToken = newToken.RefreshToken;
            await _userManager.UpdateAsync(user);

            _logger.LogError("Refresh token başarıyla yenilendi.");
            return newToken;
        }

        private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"];

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings["ValidateIssue"],
                ValidAudience = jwtSettings["ValidateAudience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                ClockSkew = TimeSpan.FromMinutes(Convert.ToDouble(jwtSettings.GetSection("Expire").Value))
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);

            if (!(securityToken is JwtSecurityToken jwtSecurityToken) || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                _logger.LogWarning("Geçersiz token.");
                throw new SecurityTokenException("Invalid token");
            }

            return principal;
        }

        public async Task<User> GetUserByUserName(string userName)
        {
            var user = await _userManager.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.UserName == userName);

            if (user is null)
                return null;

            // Kullanıcının rollerini string olarak al
            var roleNames = await _userManager.GetRolesAsync(user);

            foreach (var roleName in roleNames)
            {
                var role = await _roleManager.FindByNameAsync(roleName);
                if (role != null && !user.UserRoles.Any(ur => ur.RoleId == role.Id))
                {
                    user.UserRoles.Add(new UserRole
                    {
                        RoleId = role.Id,
                        UserId = user.Id
                    });
                }
            }

            return user;
        }


        public async Task<IEnumerable<User>> GetAllUsersByApplicationId(Guid companyApplicationId)
        {
            return _userManager.Users.Where(u => u.CompanyApplicationId == companyApplicationId);

        }

        public async Task<UserDto> GetUserByIdAsync(string id)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    _logger.LogWarning($"User with ID {id} not found.");
                    return null;
                }

                var userDto = _mapper.Map<UserDto>(user);
                return userDto;
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred while getting user with ID {id}: {ex.Message}");
                throw;
            }
        }

        public async Task<IdentityResult> UpdateUser(UserDto userDto)
        {
            // Kullanıcıyı izlenmeden alıyoruz
            var existingUser = await _userManager.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userDto.Id.ToString());

            if (existingUser == null)
            {
                return IdentityResult.Failed(new IdentityError { Description = "Kullanıcı bulunamadı." });
            }

            // DTO'yu mevcut kullanıcıya map ediyoruz
            var mappedEntity = _mapper.Map(userDto, existingUser);

            _repo.User.GenericUpdate(mappedEntity);
            _repo.Save();
            return IdentityResult.Success;
        }

        public async Task<bool> DeleteUser(string userId)
        {
            var user = await _repo.User.GenericReadExpression(u => u.Id == userId, false).FirstOrDefaultAsync();
            if (user is null) return false;
            _repo.User.GenericDelete(user);
            _repo.Save();
            return true;
        }

        /***************************************** USER VE ROLE CLAIMS ***************************************-*/

        public async Task<IdentityResult> AddRoleClaimAsync(string roleId, string claimType, string claimValue)
        {
            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null)
            {
                return IdentityResult.Failed(new IdentityError { Description = "Role not found" });
            }

            var claim = new Claim(claimType, claimValue);
            return await _roleManager.AddClaimAsync(role, claim);
        }

        public async Task<IdentityResult> AddUserClaimAsync(string userId, string claimType, string claimValue)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return IdentityResult.Failed(new IdentityError { Description = "User not found" });
            }

            var claim = new Claim(claimType, claimValue);
            return await _userManager.AddClaimAsync(user, claim);
        }

        public async Task<IEnumerable<Claim>> GetUserClaimsAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return null;

            return await _userManager.GetClaimsAsync(user);
        }

        public async Task<IEnumerable<Claim>> GetRoleClaimsAsync(string roleId)
        {
            var role = await _roleManager.FindByIdAsync(roleId); //1.veritabanına gittiği için null geliyo.
            if (role == null) return null;

            return await _roleManager.GetClaimsAsync(role);
        }

        public async Task<IdentityResult> RemoveUserClaimAsync(string userId, string claimType, string claimValue)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return IdentityResult.Failed(new IdentityError { Description = "User not found" });
            }

            var claim = new Claim(claimType, claimValue);
            return await _userManager.RemoveClaimAsync(user, claim);
        }

        public async Task<IdentityResult> RemoveRoleClaimAsync(string roleId, string claimType, string claimValue)
        {
            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null)
            {
                return IdentityResult.Failed(new IdentityError { Description = "Role not found" });
            }

            var claim = new Claim(claimType, claimValue);
            return await _roleManager.RemoveClaimAsync(role, claim);
        }

        public int GetUserCount()
        {
            int userCount = _repo.User.GenericRead(false).Count();
            return userCount;
        }

        public List<User> GetAllUsers()
        {
            var users = _userManager
                        .Users
                        .Include(u => u.UserRoles)
                        .ThenInclude(ur => ur.Role)
                        .ToList();
            return users;
        }

        public List<UserDto> GetPaginatedUsers(RequestParameters parameters, bool trackChanges)
        {
            var users = _repo.User.GetPagedUsers(parameters, trackChanges);
            var usersDto = _mapper.Map<List<UserDto>>(users);

            return usersDto;
        }
        public List<UserDto> GetPaginatedApplicationUsers(RequestParameters parameters, bool trackChanges, Guid companyApplicationId)
        {
            var users = _repo.User.GetPagedUsers(parameters, trackChanges).Where(u=> u.CompanyApplicationId == companyApplicationId);
            var usersDto = _mapper.Map<List<UserDto>>(users);

            return usersDto;
        }
        public async Task<List<Claim>> GetAllClaims()
        {
            var userClaims = await _repo.User.GetAllClaimsAsync();
            return userClaims;
        }

        public async Task<IdentityResult> UpdateUserClaimsAsync(string userId, List<ClaimDto> newClaims)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user is null) return IdentityResult.Failed(new IdentityError { Description = "Kullanıcı Bulunamadı" });

            var existingClaims = await GetUserClaimsAsync(user.Id);

            // Compare claims by type and value
            var claimsToRemove = existingClaims
                .Where(ec => !newClaims.Any(nc => nc.Type == ec.Type && nc.Value == ec.Value))
                .ToList();

            var claimsToAdd = newClaims
                .Where(nc => !existingClaims.Any(ec => ec.Type == nc.Type && ec.Value == nc.Value))
                .ToList();

            // Remove old claims
            foreach (var claim in claimsToRemove)
            {
                var result = await _userManager.RemoveClaimAsync(user, claim);
                if (!result.Succeeded) return result;
            }

            // Add new claims
            foreach (var claim in claimsToAdd)
            {
                var newClaim = new Claim(claim.Type, claim.Value);
                var result = await _userManager.AddClaimAsync(user, newClaim);
                if (!result.Succeeded) return result;
            }

            return IdentityResult.Success;
        }

        public async Task<IdentityResult> UpdateRoleClaimsAsync(string roleId, List<ClaimDto> newClaims)
        {
            var role = await _roleManager.FindByIdAsync(roleId);
            if (role is null) return IdentityResult.Failed(new IdentityError { Description = "Rol Bulunamadı" });

            var existingClaims = await GetRoleClaimsAsync(role.Id);
            // Compare claims by type and value
            var claimsToRemove = existingClaims
                .Where(ec => !newClaims.Any(nc => nc.Type == ec.Type && nc.Value == ec.Value))
                .ToList();

            var claimsToAdd = newClaims
                .Where(nc => !existingClaims.Any(ec => ec.Type == nc.Type && ec.Value == nc.Value))
                .ToList();

            // Remove old claims
            foreach (var claim in claimsToRemove)
            {
                var result = await _roleManager.RemoveClaimAsync(role, claim);
                if (!result.Succeeded) return result;
            }

            // Add new claims
            foreach (var claim in claimsToAdd)
            {
                var newClaim = new Claim(claim.Type, claim.Value);
                var result = await _roleManager.AddClaimAsync(role, newClaim);
                if (!result.Succeeded) return result;
            }

            return IdentityResult.Success;
        }

        public async Task<bool> ForgotPasswordAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                _logger.LogWarning("Kullanıcı bulunamadı.");
                return false;
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var encodedToken = Convert.ToBase64String(Encoding.UTF8.GetBytes(token));
            var resetUrl = $"https://localhost:7197/Authentication/ResetPassword?token={encodedToken}&email={user.Email}";

            var subject = "Şifre Sıfırlama Talebi";
            var message = $"Lütfen <a href='{resetUrl}'>buraya tıklayarak</a> şifrenizi sıfırlayın.";

            await _emailService.SendEmailAsync(user.Email, subject, message);

            return true;
        }

        public async Task<IdentityResult> ResetPasswordAsync(ResetPasswordDto resetPasswordDto)
        {
            var user = await _userManager.FindByEmailAsync(resetPasswordDto.Email);
            if (user == null)
            {
                _logger.LogWarning("Kullanıcı bulunamadı.");
                return IdentityResult.Failed(new IdentityError { Description = "Kullanıcı bulunamadı." });
            }

            var decodedToken = Encoding.UTF8.GetString(Convert.FromBase64String(resetPasswordDto.Token));

            var resetPassResult = await _userManager.ResetPasswordAsync(user, decodedToken, resetPasswordDto.NewPassword);
            if (!resetPassResult.Succeeded)
            {
                foreach (var error in resetPassResult.Errors)
                {
                    _logger.LogError($"Şifre sıfırlama hatası: {error.Description}");
                }
                return resetPassResult;
            }

            return IdentityResult.Success;
        }

        public async Task<IdentityResult> ChangePassword(User user, string currentPassword,string newPassword)
        {
            var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
            if (result.Succeeded) return IdentityResult.Success;
            else return IdentityResult.Failed();
        }
    }
}
