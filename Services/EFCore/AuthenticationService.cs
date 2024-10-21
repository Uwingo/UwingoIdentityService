using AutoMapper;
using Entity.Models;
using Entity.ModelsDto;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using RapositoryAppClient;
using Repositories;
using Repositories.Contracts;
using RepositoryAppClient.Contracts;
using RepositoryAppClient.EFCore;
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
        private UwingoUser _uwingoUser;
        private readonly IRepositoryManager _repo;
        private readonly IRepositoryAppClientManager _repositoryAppClient;
        private readonly IEmailService _emailService;

        public AuthenticationService(UserManager<User> userManager, IRepositoryAppClientManager repositoryAppClient, RoleManager<Role> roleManager, IMapper mapper, IConfiguration configuration, ILogger<AuthenticationService> logger, IRepositoryManager repo, IEmailService emailService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _mapper = mapper;
            _configuration = configuration;
            _logger = logger;
            _repo = repo;
            _repositoryAppClient = repositoryAppClient;
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

                IdentityResult result;

                var connectionString = _repo.CompanyApplication.GenericRead(false).Where(ca => ca.Id == userRegistrationDto.CompanyApplicationId).FirstOrDefault().DbConnection;
                var newContext = await ChangeDatabase(connectionString);

                var userManagerAppClient = CreateUserManager(newContext);

                UwingoUser user2 = new UwingoUser();
                user2.Email = user.Email;
                user2.PhoneNumber = user.PhoneNumber;
                user2.FirstName = user.FirstName;
                user2.LastName = user.LastName;
                user2.UserName = user.UserName;

                result = await userManagerAppClient.CreateAsync(user2, userRegistrationDto.Password);
                if (result.Succeeded)

                {
                    List<string> roles = new List<string>();
                    roles.Add(role.Name);

                    var result3 = await userManagerAppClient.AddToRoleAsync(user2, role.Name);
                    UserDatabaseMatch userDbMatch = new UserDatabaseMatch
                    {
                        CompanyApplicationId = userRegistrationDto.CompanyApplicationId,
                        UserId = user2.Id,
                        UserName = user2.UserName
                    };
                    _repo.UserDbMatch.GenericCreate(userDbMatch);
                    _logger.LogInformation("Kullanıcı (Admin olmayan) başarıyla kaydedildi ve role atandı.");
                }
                else
                {
                    _logger.LogError("Kullanıcı (Admin olmayan) kaydı başarısız.");
                }
                //}

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError("Kullanıcı kaydedilirken bir hata oluştu: {Message}", ex.Message);
                throw;
            }
        }

        private UserManager<UwingoUser> CreateUserManager(RepositoryContextAppClient contextAppClient)
        {
            // UserStore nesnesini oluşturun
            var userStore = new UserStore<UwingoUser, Role, RepositoryContextAppClient, string>(contextAppClient);

            // Identity ayarlarını oluşturun
            var options = Options.Create(new IdentityOptions());

            // ILoggerFactory kullanarak doğru türde bir logger oluştur
            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            var userLogger = loggerFactory.CreateLogger<UserManager<UwingoUser>>();

            // UserManager nesnesini oluşturun ve geri döndürün
            var userManagerAppClient = new UserManager<UwingoUser>(
                userStore,
                options,
                new PasswordHasher<UwingoUser>(),
                new IUserValidator<UwingoUser>[0],
                new IPasswordValidator<UwingoUser>[0],
                new UpperInvariantLookupNormalizer(),
                new IdentityErrorDescriber(),
                null,
                userLogger
            );

            return userManagerAppClient;
        }

        private RoleManager<Role> CreateRoleManager(RepositoryContextAppClient contextAppClient)
        {
            // RoleStore nesnesini oluşturun
            var roleStore = new RoleStore<Role, RepositoryContextAppClient, string>(contextAppClient);

            // ILoggerFactory kullanarak doğru türde bir logger oluştur
            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            var roleLogger = loggerFactory.CreateLogger<RoleManager<Role>>();

            // RoleManager nesnesini oluşturun ve geri döndürün
            var roleManagerAppClient = new RoleManager<Role>(
                roleStore,
                new IRoleValidator<Role>[0],
                new UpperInvariantLookupNormalizer(),
                new IdentityErrorDescriber(),
                roleLogger
            );

            return roleManagerAppClient;
        }

        public async Task<bool> ValidateUser(UserLoginDto userLoginDto)
        {
            try
            {
                var companyApplication = _repo.CompanyApplication.GetCompanyApplicationByApplicationAndCompanyId(userLoginDto.CompanyId, userLoginDto.ApplicationId, false);

                var dbString = companyApplication.DbConnection;

                var newContext = await ChangeDatabase(dbString);

                var userManagerAppClient = CreateUserManager(newContext);
                // Kullanıcıyı bul
                _user = await _userManager.FindByNameAsync(userLoginDto.UserName);
                var result = _user != null && await _userManager.CheckPasswordAsync(_user, userLoginDto.Password);

                if (_user is null)
                {
                    _uwingoUser = await userManagerAppClient.FindByNameAsync(userLoginDto.UserName);
                    result = _uwingoUser != null && await userManagerAppClient.CheckPasswordAsync(_uwingoUser, userLoginDto.Password);
                }

                // Eğer kullanıcı doğrulanırsa devam et
                if (result) _logger.LogInformation("Kullanıcı başarıyla doğrulandı ve veritabanı bağlantısı sağlandı.");
                else _logger.LogError("Kullanıcı doğrulama başarısız.");

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
            List<Claim> claims;

            // Kullanıcıyı belirleyip uygun UserManager kullanın
            if (_user is null)
            {
                var mappedUser = _mapper.Map<User>(_uwingoUser);
                claims = await GetClaims(mappedUser);

                var companyApplicationId = _repo.UserDbMatch.GetUsersCompanyApplicationId(_uwingoUser.Id, false);
                var dbString = _repo.CompanyApplication.GetCompanyApplication(companyApplicationId, false).DbConnection;

                var context = await ChangeDatabase(dbString);
                var userManagerAppClient = CreateUserManager(context);

                // RefreshToken ve RefreshTokenExpiryTime güncellemesi
                _uwingoUser.RefreshToken = GenerateRefreshToken();
                if (populateExp)
                    _uwingoUser.RefreshTokenExpiryTime = DateTime.Now.AddDays(7);

                //var result = context.Users.Update(_uwingoUser);
                var result = await userManagerAppClient.UpdateAsync(_uwingoUser);
            }
            else
            {
                claims = await GetClaims(_user);

                // RefreshToken ve RefreshTokenExpiryTime güncellemesi
                _user.RefreshToken = GenerateRefreshToken();
                if (populateExp)
                    _user.RefreshTokenExpiryTime = DateTime.Now.AddDays(7);

                await _userManager.UpdateAsync(_user);
            }

            // JWT oluşturulması
            var tokenOptions = GenerateTokenOptions(signingCredentials, claims);
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.WriteToken(tokenOptions);

            _logger.LogError("Token başarıyla oluşturuldu.");
            return new TokenDto
            {
                AccessToken = token,
                RefreshToken = _user?.RefreshToken ?? _uwingoUser?.RefreshToken
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
            // Kullanıcıya ait temel claim'leri ekle
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim("uid", user.Id)
            };

            // Kullanıcının companyApplicationId'sini bulmak için veritabanını kontrol edin
            var companyApplicationId = _repo.UserDbMatch.GetUsersCompanyApplicationId(user.Id, false);
            var dbString = _repo.CompanyApplication.GetCompanyApplication(companyApplicationId, false).DbConnection;

            // Dinamik olarak veritabanını değiştir
            var contextAppClient = await ChangeDatabase(dbString);
            var userManagerAppClient = CreateUserManager(contextAppClient);
            var roleManagerAppClient = CreateRoleManager(contextAppClient);

            // UwingoUser oluştur
            var uwingoUser = await userManagerAppClient.FindByIdAsync(user.Id);
            if (uwingoUser is null)
            {
                throw new InvalidOperationException("Kullanıcı bulunamadı.");
            }

            // Rolleri alın
            var roles = await userManagerAppClient.GetRolesAsync(uwingoUser);

            // Her role için ilgili claim'leri ekleyin
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
                var usersRole = await roleManagerAppClient.FindByNameAsync(role);

                if (usersRole is not null)
                {
                    var roleClaims = await GetRoleClaimsAsync(usersRole.Id);
                    foreach (var roleClaim in roleClaims)
                    {
                        claims.Add(new Claim(roleClaim.Type, roleClaim.Value));
                    }
                }
            }

            // Kullanıcıya ait claim'leri alın
            var userClaims = await userManagerAppClient.GetClaimsAsync(uwingoUser);

            foreach (var userClaim in userClaims)
            {
                claims.Add(new Claim(userClaim.Type, userClaim.Value));
            }

            return claims;
        }



        public async Task<List<UwingoUserDto>> GetUsersByCompanyApplication(Guid companyId, Guid applicationId)
        {
            var dbString = _repo.CompanyApplication.GetCompanyApplicationByApplicationAndCompanyId(companyId, applicationId, false).DbConnection;
            var context = await ChangeDatabase(dbString);
            var userManager = CreateUserManager(context);

            var users = userManager.Users.ToList();
            var usersDto = _mapper.Map<List<UwingoUserDto>>(users);

            return usersDto;
        }

        public async Task<List<RoleDto>> GetRolesByCompanyApplication(Guid companyId, Guid applicationId)
        {
            var dbString = _repo.CompanyApplication.GetCompanyApplicationByApplicationAndCompanyId(companyId, applicationId, false).DbConnection;
            var context = await ChangeDatabase(dbString);
            var roleManager = CreateRoleManager(context);

            var roles = roleManager.Roles.ToList();
            var roleList = _mapper.Map<List<RoleDto>>(roles);

            return roleList;
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
            string dbString = "";
            TokenDto newToken;

            if (user == null || user.RefreshToken != tokenDto.RefreshToken || user.RefreshTokenExpiryTime <= DateTime.Now)
            {
                Guid companyApplicationId;
                // Kullanıcı ana veritabanında bulunamadıysa alt veritabanında arama yapılır
                if (user is null) companyApplicationId = _repo.UserDbMatch.GetUsersCAIdByUserName(principal.Identity.Name, false);
                else companyApplicationId = _repo.UserDbMatch.GetUsersCompanyApplicationId(user.Id, false);

                dbString = _repo.CompanyApplication.GetCompanyApplication(companyApplicationId, false).DbConnection;

                var context = await ChangeDatabase(dbString);
                var userManager = CreateUserManager(context);

                _uwingoUser = await userManager.FindByNameAsync(principal.Identity.Name);
                if (_uwingoUser == null || _uwingoUser.RefreshToken != tokenDto.RefreshToken || _uwingoUser.RefreshTokenExpiryTime <= DateTime.Now)
                {
                    _logger.LogWarning("Geçersiz refresh token.");
                    throw new SecurityTokenException("Invalid refresh token");
                }

                newToken = await CreateToken(false);

                // Token bilgilerini `user` nesnesinde güncelleyin
                _uwingoUser.RefreshToken = newToken.RefreshToken;
                _uwingoUser.RefreshTokenExpiryTime = DateTime.Now.AddDays(7);

                await userManager.UpdateAsync(_uwingoUser);
                _logger.LogError("Refresh token başarıyla yenilendi.");
                return newToken;
            }
            _user = user;
            newToken = await CreateToken(false);

            // Token bilgilerini `user` nesnesinde güncelleyin
            user.RefreshToken = newToken.RefreshToken;
            user.RefreshTokenExpiryTime = DateTime.Now.AddDays(7);

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
            // İlk olarak, birinci veritabanında kullanıcıyı kontrol et
            var user = await _userManager.Users
                .FirstOrDefaultAsync(u => u.UserName == userName);

            // Eğer kullanıcı birinci veritabanında bulunamazsa, veritabanı bilgisini al
            if (user is null)
            {

                // Kullanıcıya ait companyApplicationId'sini bul
                var companyApplicationId = _repo.UserDbMatch.GetUsersCAIdByUserName(userName, false);
                var dbString = _repo.CompanyApplication.GetCompanyApplication(companyApplicationId, false).DbConnection;

                // Dinamik olarak veritabanını değiştir
                var context = await ChangeDatabase(dbString);
                var userManager2 = CreateUserManager(context);

                // UwingoUser'ı bul
                var uwingoUser = await userManager2.Users.FirstOrDefaultAsync(u => u.UserName == userName);
                if (uwingoUser != null)
                {
                    user = _mapper.Map<User>(uwingoUser);
                }

            }

            return user;
        }



        public async Task<IEnumerable<User>> GetAllUsersByApplicationId(Guid companyApplicationId)
        {
            var users = _userManager.Users.Where(u => u.CompanyApplicationId == companyApplicationId);
            if (users is null)
            {
                string dbString = _repo.CompanyApplication.GetCompanyApplication(companyApplicationId, false).DbConnection;
                var context = await ChangeDatabase(dbString);
                var userManager2 = CreateUserManager(context);
                users = _userManager.Users.Where(u => u.CompanyApplicationId == companyApplicationId);
            }
            return users;
        }

        public async Task<UserDto> GetUserByIdAsync(string id)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user is null)
                {
                    Guid companyApplicationId = _repo.UserDbMatch.GetUsersCompanyApplicationId(id, false);
                    string dbString = _repo.CompanyApplication.GetCompanyApplication(companyApplicationId, false).DbConnection;
                    var context = await ChangeDatabase(dbString);
                    var userManager = CreateUserManager(context);

                    var uwingoUser = await userManager.FindByIdAsync(id);
                    user = _mapper.Map<User>(uwingoUser);
                    if (user is null)
                    {
                        _logger.LogWarning($"User with ID {id} not found.");
                        return null;
                    }
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
            try
            {
                // Kullanıcıyı mevcut UserManager'dan izlenmeden alıyoruz
                var existingUser = await _userManager.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userDto.Id.ToString());

                // Eğer kullanıcı mevcut değilse, başka bir veritabanına ait olabilir
                if (existingUser == null)
                {
                    Guid companyApplicationId = _repo.UserDbMatch.GetUsersCompanyApplicationId(userDto.Id, false);
                    string dbString = _repo.CompanyApplication.GetCompanyApplication(companyApplicationId, false).DbConnection;

                    // Dinamik olarak veritabanını değiştir ve ilgili UserManager oluştur
                    var context = await ChangeDatabase(dbString);
                    var userManager = CreateUserManager(context);

                    // İlgili veritabanından kullanıcıyı al
                    var existingUwingoUser = await userManager.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userDto.Id.ToString());

                    if (existingUwingoUser == null)
                    {
                        return IdentityResult.Failed(new IdentityError { Description = "Kullanıcı bulunamadı." });
                    }

                    // Kullanıcıyı DTO ile güncelle ve kaydet
                    _mapper.Map(userDto, existingUwingoUser);
                    context.Users.Update(existingUwingoUser);
                    await context.SaveChangesAsync();
                    return IdentityResult.Success;
                }

                // DTO'yu mevcut kullanıcıya map ediyoruz
                _mapper.Map(userDto, existingUser);
                _repo.User.GenericUpdate(existingUser);
                _repo.Save();
                return IdentityResult.Success;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Kullanıcı güncellenirken bir hata oluştu: {ex.Message}");
                throw;
            }
        }

        //public async Task<bool> DeleteUser(string userId)
        //{
        //    var user = await _repo.User.GenericReadExpression(u => u.Id == userId, false).FirstOrDefaultAsync();
        //    if (user is null) return false;
        //    _repo.User.GenericDelete(user);
        //    _repo.Save();
        //    return true;
        //}

        public async Task<bool> DeleteUser(string userId)
        {
            // Öncelikle birinci veritabanında kullanıcıyı kontrol et
            var user = await _repo.User.GenericReadExpression(u => u.Id == userId, false).FirstOrDefaultAsync();

            if (user is not null)
            {
                // Eğer kullanıcı bulunduysa, silme işlemini yap
                _repo.User.GenericDelete(user);
                _repo.Save(); // Asenkron kaydetme
                return true;
            }

            // Eğer kullanıcı birinci veritabanında bulunamadıysa, ikinci veritabanını kontrol et
            Guid companyApplicationId = _repo.UserDbMatch.GetUsersCompanyApplicationId(userId, false);
            string dbString = _repo.CompanyApplication.GetCompanyApplication(companyApplicationId, false).DbConnection;

            // Dinamik olarak veritabanını değiştir ve ilgili UserManager oluştur
            var context = await ChangeDatabase(dbString);
            var userManager = CreateUserManager(context);

            // İkinci veritabanında kullanıcıyı kontrol et
            var uwingoUser = await userManager.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (uwingoUser is not null)
            {
                // Kullanıcı bulundu, silme işlemini yap
                await userManager.DeleteAsync(uwingoUser); // Asenkron silme
                return true;
            }

            // Kullanıcı her iki veritabanında bulunamadıysa
            return false;
        }


        /***************************************** USER VE ROLE CLAIMS ***************************************-*/

        public async Task<IdentityResult> AddRoleClaimAsync(string roleId, string claimType, string claimValue)
        {
            var role = await _roleManager.FindByIdAsync(roleId);
            var claim = new Claim(claimType, claimValue);

            if (role == null)
            {
                Guid companyApplicationId = _repo.RoleDbMatch.GetRolesCompanyApplicationId(roleId, false);
                string dbString = _repo.CompanyApplication.GetCompanyApplication(companyApplicationId, false).DbConnection;

                var context = await ChangeDatabase(dbString);
                var roleManager = CreateRoleManager(context);

                role = await roleManager.FindByIdAsync(roleId);

                if (role is null) return IdentityResult.Failed(new IdentityError { Description = "Role not found" });

                return await roleManager.AddClaimAsync(role, claim);
            }

            return await _roleManager.AddClaimAsync(role, claim);
        }

        public async Task<IdentityResult> AddUserClaimAsync(string userId, string claimType, string claimValue)
        {
            // İlk veritabanında kullanıcıyı kontrol et
            var user = await _userManager.FindByIdAsync(userId);
            var claim = new Claim(claimType, claimValue);

            if (user == null)
            {
                // Kullanıcı birinci veritabanında bulunamadı, dinamik veritabanına geç
                Guid companyApplicationId = _repo.UserDbMatch.GetUsersCompanyApplicationId(userId, false);
                string dbString = _repo.CompanyApplication.GetCompanyApplication(companyApplicationId, false).DbConnection;

                var context = await ChangeDatabase(dbString);
                var userManager = CreateUserManager(context);

                // UwingoUser'ı bul
                var uwingoUser = await userManager.FindByIdAsync(userId);
                if (uwingoUser == null)
                {
                    return IdentityResult.Failed(new IdentityError { Description = "User not found" });
                }
                // UwingoUser'dan User'a dönüşüm yap
                return await userManager.AddClaimAsync(uwingoUser, claim);
            }

            return await _userManager.AddClaimAsync(user, claim);
        }



        public async Task<IEnumerable<Claim>> GetUserClaimsAsync(string userId)
        {
            IEnumerable<Claim> userClaims;

            // İlk veritabanında kullanıcıyı ara
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                // Kullanıcının companyApplicationId'sini al
                Guid companyApplicationId = _repo.UserDbMatch.GetUsersCompanyApplicationId(userId, false);

                // companyApplicationId üzerinden gerekli veritabanı bağlantı dizgesini al
                string dbString = _repo.CompanyApplication.GetCompanyApplication(companyApplicationId, false).DbConnection;

                if (string.IsNullOrEmpty(dbString))
                {
                    throw new InvalidOperationException("Veritabanı bağlantı dizgesi bulunamadı.");
                }

                // Dinamik olarak veritabanını değiştir
                var context = await ChangeDatabase(dbString);
                var userManager = CreateUserManager(context);

                // İkinci veritabanında kullanıcıyı ara
                var uwingoUser = await userManager.FindByIdAsync(userId);
                if (uwingoUser is not null)
                {
                    // İkinci veritabanında kullanıcı bulundu, claim'leri al
                    userClaims = await userManager.GetClaimsAsync(uwingoUser);
                    return userClaims;
                }
                else
                {
                    throw new InvalidOperationException("Kullanıcı bulunamadı.");
                }
            }

            // İlk veritabanında kullanıcı bulundu, claim'leri al
            userClaims = await _userManager.GetClaimsAsync(user);

            return userClaims;
        }

        //public async Task<IEnumerable<Claim>> GetRoleClaimsAsync(string roleId)
        //{
        //    var role = await _roleManager.FindByIdAsync(roleId); //1.veritabanına gittiği için null geliyo.
        //    if (role == null) return null;

        //    return await _roleManager.GetClaimsAsync(role);
        //}
        public async Task<IEnumerable<Claim>> GetRoleClaimsAsync(string roleId)
        {
            // İlk olarak birinci veritabanında rolü kontrol et
            var role = await _roleManager.FindByIdAsync(roleId);

            // Eğer rol birinci veritabanında bulunamadıysa, dinamik veritabanını kontrol et
            if (role == null)
            {
                // Rol bulunamadı, dinamik veritabanını kullanmak için gerekli bilgileri al
                Guid companyApplicationId = _repo.RoleDbMatch.GetRolesCompanyApplicationId(roleId, false);
                string dbString = _repo.CompanyApplication.GetCompanyApplication(companyApplicationId, false).DbConnection;

                // Dinamik olarak veritabanını değiştir
                var context = await ChangeDatabase(dbString);
                var roleManagerAppClient = CreateRoleManager(context); // Dinamik RoleManager oluştur

                // Rolü dinamik veritabanında bul
                role = await roleManagerAppClient.FindByIdAsync(roleId);
                if (role == null) return null; // Eğer rol bulunamazsa null döndür
                else return await roleManagerAppClient.GetClaimsAsync(role);
            }

            // Rol bulunduğunda claim'leri getir
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
                        //.Include(u => u.UserRoles)
                        //.ThenInclude(ur => ur.Role)
                        .ToList();
            return users;
        }

        public async Task<List<UwingoUser>> GetAllUsersByCompanyApplicationId(Guid companyId, Guid applicationId)
        {
            var dbString = _repo.CompanyApplication.GetCompanyApplicationByApplicationAndCompanyId(companyId, applicationId, false).DbConnection;
            var context = await ChangeDatabase(dbString);
            var userManager = CreateUserManager(context);

            var users = userManager
                        .Users
                        .ToList();

            return users;
        }

        public async Task<UwingoUser> GetAdminId(Guid companyId, Guid applicationId)
        {
            var dbString = _repo.CompanyApplication.GetCompanyApplicationByApplicationAndCompanyId(companyId, applicationId, false).DbConnection;
            var context = await ChangeDatabase(dbString);
            var userManager = CreateUserManager(context);

            var users = await userManager.GetUsersInRoleAsync("Admin");
            var admin = users.FirstOrDefault();

            return admin;
        }

        public async Task<Role> GetAdminRoleId(Guid companyId, Guid applicationId)
        {
            var dbString = _repo.CompanyApplication.GetCompanyApplicationByApplicationAndCompanyId(companyId, applicationId, false).DbConnection;
            var context = await ChangeDatabase(dbString);
            var roleManager = CreateRoleManager(context);

            var roles = await roleManager.FindByNameAsync("Admin");

            return roles;
        }

        //public List<UserDto> GetPaginatedUsers(RequestParameters parameters, bool trackChanges)
        //{
        //    var users = _repo.User.GetPagedUsers(parameters, trackChanges);
        //    var usersDto = _mapper.Map<List<UserDto>>(users);

        //    return usersDto;
        //}

        public async Task<List<UserDto>> GetPaginatedUsers(RequestParameters parameters, bool trackChanges)
        {
            // İlk olarak ana veritabanındaki kullanıcıları çekiyoruz
            var users = _repo.User.GetPagedUsers(parameters, trackChanges).ToList();

            // CompanyApplication tablosundaki farklı connectionString'leri alıyoruz
            //var companyApplications = _repo.CompanyApplication.GenericRead(false);
            //var connectionStrings = companyApplications.Select(ca => ca.DbConnection).Distinct();

            //foreach (var connectionString in connectionStrings)
            //{
            //    if (users.Count >= parameters.PageSize) break;
            //    // Yeni bağlam oluşturup, her bir veritabanına bağlanarak kullanıcıları çekiyoruz
            //    var newContext = await ChangeDatabase(connectionString);
            //    var repositoryAppClientManager = new RepositoryAppClientManager(newContext);
            //    var additionalUsers = repositoryAppClientManager.User.GetPagedUsers(parameters, trackChanges).ToList();
            //    foreach (var additionalUser in additionalUsers)
            //    {
            //        if (users.Count >= parameters.PageSize) break;
            //        users.Add(additionalUser);
            //    }
            //}

            // Kullanıcıları DTO'ya dönüştürüyoruz
            var usersDto = _mapper.Map<List<UserDto>>(users);

            return usersDto;
        }

        public List<UserDto> GetPaginatedApplicationUsers(RequestParameters parameters, bool trackChanges, Guid companyApplicationId)
        {
            var users = _repo.User.GetPagedUsers(parameters, trackChanges).Where(u => u.CompanyApplicationId == companyApplicationId);
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
            // İlk veritabanında kullanıcıyı kontrol et
            var user = await _userManager.FindByIdAsync(userId);

            if (user is null)
            {
                // Kullanıcı birinci veritabanında bulunamadı, dinamik veritabanına geç
                Guid companyApplicationId = _repo.UserDbMatch.GetUsersCompanyApplicationId(userId, false);
                string dbString = _repo.CompanyApplication.GetCompanyApplication(companyApplicationId, false).DbConnection;

                var context = await ChangeDatabase(dbString);
                var uwingoUserManager = CreateUserManager(context); // UwingoUser için UserManager

                // UwingoUser'ı bul
                var uwingoUser = await uwingoUserManager.FindByIdAsync(userId);
                if (uwingoUser is null)
                {
                    return IdentityResult.Failed(new IdentityError { Description = "Kullanıcı Bulunamadı" });
                }

                // UwingoUser ile çalışacağız
                var existingClaims = await GetUserClaimsAsync(uwingoUser.Id);

                // Compare claims by type and value
                var claimsToRemove = existingClaims
                    .Where(ec => !newClaims.Any(nc => nc.Type == ec.Type && nc.Value == ec.Value))
                    .ToList();

                var claimsToAdd = newClaims
                    .Where(nc => !existingClaims.Any(ec => ec.Type == nc.Type && nc.Value == nc.Value))
                    .ToList();

                // Remove old claims
                foreach (var claim in claimsToRemove)
                {
                    var result = await uwingoUserManager.RemoveClaimAsync(uwingoUser, claim);
                    if (!result.Succeeded) return result;
                }

                // Add new claims
                foreach (var claim in claimsToAdd)
                {
                    var newClaim = new Claim(claim.Type, claim.Value);
                    var result = await uwingoUserManager.AddClaimAsync(uwingoUser, newClaim);
                    if (!result.Succeeded) return result;
                }
            }
            else
            {
                // Eğer kullanıcı birinci veritabanında bulunduysa
                var existingClaims = await GetUserClaimsAsync(user.Id);

                // Compare claims by type and value
                var claimsToRemove = existingClaims
                    .Where(ec => !newClaims.Any(nc => nc.Type == ec.Type && nc.Value == ec.Value))
                    .ToList();

                var claimsToAdd = newClaims
                    .Where(nc => !existingClaims.Any(ec => ec.Type == nc.Type && nc.Value == nc.Value))
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
            }

            return IdentityResult.Success;
        }

        public async Task<IdentityResult> UpdateRoleClaimsAsync(string roleId, List<ClaimDto> newClaims)
        {
            // İlk veritabanında rolü kontrol et
            var role = await _roleManager.FindByIdAsync(roleId);

            if (role is null)
            {
                // Rol birinci veritabanında bulunamadı, dinamik veritabanına geç
                Guid companyApplicationId = _repo.RoleDbMatch.GetRolesCompanyApplicationId(roleId, false);
                string dbString = _repo.CompanyApplication.GetCompanyApplication(companyApplicationId, false).DbConnection;

                var context = await ChangeDatabase(dbString);
                var roleManager = CreateRoleManager(context); // Dinamik veritabanı için yeni RoleManager

                // Dinamik veritabanında rolü bul
                role = await roleManager.FindByIdAsync(roleId);
                if (role is null) return IdentityResult.Failed(new IdentityError { Description = "Rol Bulunamadı" });

                // Varsayılan veritabanında veya dinamik veritabanında rolü bulduktan sonra claim'leri güncelleyebiliriz
                var existingClaims = await GetRoleClaimsAsync(role.Id);

                // Compare claims by type and value
                var claimsToRemove = existingClaims
                    .Where(ec => !newClaims.Any(nc => nc.Type == ec.Type && nc.Value == ec.Value))
                    .ToList();

                var claimsToAdd = newClaims
                    .Where(nc => !existingClaims.Any(ec => ec.Type == nc.Type && nc.Value == nc.Value))
                    .ToList();

                // Remove old claims
                foreach (var claim in claimsToRemove)
                {
                    var result = await roleManager.RemoveClaimAsync(role, claim); // Eğer rol birinci veritabanındaysa
                    if (!result.Succeeded) return result;
                }

                // Add new claims
                foreach (var claim in claimsToAdd)
                {
                    var newClaim = new Claim(claim.Type, claim.Value);
                    var result = await roleManager.AddClaimAsync(role, newClaim); // Eğer rol birinci veritabanındaysa
                    if (!result.Succeeded) return result;
                }
            }
            else
            {
                // Varsayılan veritabanında veya dinamik veritabanında rolü bulduktan sonra claim'leri güncelleyebiliriz
                var existingClaims = await GetRoleClaimsAsync(role.Id);

                // Compare claims by type and value
                var claimsToRemove = existingClaims
                    .Where(ec => !newClaims.Any(nc => nc.Type == ec.Type && nc.Value == ec.Value))
                    .ToList();

                var claimsToAdd = newClaims
                    .Where(nc => !existingClaims.Any(ec => ec.Type == nc.Type && nc.Value == nc.Value))
                    .ToList();

                // Remove old claims
                foreach (var claim in claimsToRemove)
                {
                    var result = await _roleManager.RemoveClaimAsync(role, claim); // Eğer rol birinci veritabanındaysa
                    if (!result.Succeeded) return result;
                }

                // Add new claims
                foreach (var claim in claimsToAdd)
                {
                    var newClaim = new Claim(claim.Type, claim.Value);
                    var result = await _roleManager.AddClaimAsync(role, newClaim); // Eğer rol birinci veritabanındaysa
                    if (!result.Succeeded) return result;
                }
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

        public async Task<IdentityResult> ChangePassword(User user, string currentPassword, string newPassword)
        {
            var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
            if (result.Succeeded) return IdentityResult.Success;
            else return IdentityResult.Failed();
        }
    }
}
