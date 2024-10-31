using AutoMapper;
using Entity.Models;
using Entity.ModelsDto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RapositoryAppClient;
using RepositoryAppClient.Contracts;
using Services.Contracts;
using Services.EFCore;
using System;
using System.Data;
using System.Threading.Tasks;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly IAuthenticationService _authService;
        private readonly IRoleService _roleService;
        private readonly ILogger<AuthenticationController> _logger;
        private readonly IRepositoryAppClientManager _repositoryAppClient;


        public AuthenticationController(IAuthenticationService authService, IRepositoryAppClientManager repositoryAppClient, IRoleService roleService, ILogger<AuthenticationController> logger)
        {
            _authService = authService;
            _roleService = roleService;
            _logger = logger;
            _repositoryAppClient = repositoryAppClient;
        }

        #region register
        [HttpPost("register")]
        [Authorize(Policy = "CreateUser")]
        public async Task<IActionResult> Register([FromBody] UserRegistrationDto userRegistration)
        {
            try
            {
                var result = await _authService.RegisterUser(userRegistration);

                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(error.Code, error.Description);
                    }
                    _logger.LogError("Kullanıcı kaydı başarısız.");
                    return BadRequest(ModelState);
                }

                _logger.LogError("Kullanıcı başarıyla kaydedildi.");
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError("Kullanıcı kaydedilirken bir hata oluştu: {Message}", ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }

        #endregion
        #region Login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginDto userLogin)
        {
            try
            {
                if (!await _authService.ValidateUser(userLogin))
                {
                    _logger.LogError("Kullanıcı doğrulama başarısız.");
                    return Unauthorized();
                }

                var tokenDto = await _authService.CreateToken(true);
                _logger.LogInformation("Kullanıcı başarıyla giriş yaptı.");
                return Ok(tokenDto);
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Kullanıcı giriş yaparken bir hata oluştu: {Message}", ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }
        #endregion
        #region RefreshToken
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] TokenDto tokenDto)
        {
            try
            {
                var newTokenDto = await _authService.RefreshToken(tokenDto);
                _logger.LogError("Token başarıyla yenilendi.");
                return Ok(newTokenDto);
            }
            catch (Exception ex)
            {
                _logger.LogError("Token yenilenirken bir hata oluştu: {Message}", ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }
        #endregion
        #region GetUserIdByUserName
        [HttpPost("get-user-id-by-user-name")]
        public async Task<IActionResult> GetUserIdByUserName([FromBody] string userName)
        {
            var user = await _authService.GetUserByUserName(userName);
            if (user is not null) return Ok(user.Id);
            else return NotFound();
        }
        #endregion
        #region GetApplicationIdByUserName/{username}
        [HttpGet("GetApplicationIdByUserName/{username}")]
        public async Task<IActionResult> GetApplicationIdByUserName(string username)
        {
            try
            {
                var user = await _authService.GetUserByUserName(username);
                if (user != null) return Ok(user.CompanyApplicationId);
                else return NotFound("Kullanıcı bulunamadı.");

            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        #endregion
        #region GetUsersByCompanyApplication
        [HttpGet("GetUsersByCompanyApplication")]
        public async Task<IActionResult> GetUsersByCompanyApplication(Guid companyId, Guid applicationId, int pageNumber = 1, int pageSize = 10)
        {
            var users = await _authService.GetUsersByCompanyApplication(companyId, applicationId);

            return Ok(users);
        }
        #endregion
        #region GetRolesByCompanyApplication
        [HttpGet("GetRolesByCompanyApplication")]
        public async Task<IActionResult> GetRolesByCompanyApplication(Guid companyId, Guid applicationId, int pageNumber = 1, int pageSize = 10)
        {
            var roles = await _authService.GetRolesByCompanyApplication(companyId, applicationId);

            return Ok(roles);
        }
        #endregion
        #region GetClaims/{username}
        [HttpGet("GetClaims/{username}")]
        public async Task<IActionResult> GetClaims(string userName)
        {
            var user = await _authService.GetUserByUserName(userName);
            var claim = await _authService.GetClaims(user);
            return Ok(claim);
        }
        #endregion
        #region GetAllUserClaims
        [HttpGet("GetAllUserClaims/{companyId}/{applicationId}")]
        public async Task<IActionResult> GetAllUserClaims(Guid companyId, Guid applicationId)
        {
            var caUsers = await _authService.GetAllUsersByCompanyApplicationId(companyId, applicationId);
            var caAdmin = await _authService.GetAdminId(companyId, applicationId);
            var claim = await _authService.GetUserClaimsAsync(caAdmin.Id); //Adminin yetkileri

            //var claim = await _authService.GetUserClaimsAsync("66895f4e-3ad4-48d3-9f91-43727075edbb"); //Adminin yetkileri
            return Ok(claim);
        }
        #endregion
        #region GetAllRoleClaims
        [HttpGet("GetAllRoleClaims/{companyId}/{applicationId}")]
        public async Task<IActionResult> GetAllRoleClaims(Guid companyId, Guid applicationId)
        {
            var adminRole = await _authService.GetAdminRoleId(companyId, applicationId);

            var claims = await _authService.GetRoleClaimsAsync(adminRole.Id);
            if (claims.Any())
                return Ok(claims);
            else
                return BadRequest();
        }

        #endregion
        #region GetPaginatedUsersByApplicationId/{applicationId}
        [HttpGet("GetPaginatedUsersByApplicationId/{applicationId}")]
        [Authorize(Policy = "GetAllUsers")]
        public IActionResult GetPaginatedUsersByApplicationId([FromQuery] RequestParameters parameters, bool trackChanges, Guid applicationId)
        {
            var tenantsUsers = _authService.GetPaginatedApplicationUsers(parameters, trackChanges, applicationId);
            return Ok(tenantsUsers);
        }
        #endregion
        #region GetUserById/{id}
        [HttpGet("GetUserById/{id}")]
        public async Task<IActionResult> GetUserById(string id)
        {
            try
            {
                var user = await _authService.GetUserByIdAsync(id);
                if (user == null)
                {
                    _logger.LogWarning($"User with ID {id} not found.");
                    return NotFound();
                }

                _logger.LogInformation($"User with ID {id} found.");
                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred while getting user with ID {id}: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }
        #endregion
        #region UpdateUser
        [Authorize(Policy = "EditUser")]
        [HttpPut("UpdateUser")]
        public async Task<IActionResult> UpdateUser([FromBody] UserDto updatedUser)
        {
            try
            {
                var user = await _authService.GetUserByIdAsync(updatedUser.Id);
                if (user == null)
                {
                    _logger.LogError("Kullanıcı bulunamadı.");
                    return NotFound("Kullanıcı bulunamadı.");
                }

                var result = await _authService.UpdateUser(updatedUser);

                if (result.Succeeded)
                {
                    _logger.LogInformation("Kullanıcı başarıyla güncellendi.");
                    return Ok("Kullanıcı başarıyla güncellendi.");
                }
                else
                {
                    _logger.LogError("Kullanıcı güncellenirken bir hata oluştu.");
                    return BadRequest("Kullanıcı güncellenirken bir hata oluştu.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Kullanıcı güncellenirken bir hata oluştu: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        #endregion
        #region UpdateUserProfile
        [HttpPut("UpdateUserProfile")]
        public async Task<IActionResult> UpdateUserProfile([FromBody] UserDto updatedUser)
        {
            try
            {
                var user = await _authService.GetUserByIdAsync(updatedUser.Id);
                if (user == null)
                {
                    _logger.LogError("Kullanıcı bulunamadı.");
                    return NotFound("Kullanıcı bulunamadı.");
                }

                var result = await _authService.UpdateUser(updatedUser);

                if (result.Succeeded)
                {
                    _logger.LogInformation("Kullanıcı başarıyla güncellendi.");
                    return Ok("Kullanıcı başarıyla güncellendi.");
                }
                else
                {
                    _logger.LogError("Kullanıcı güncellenirken bir hata oluştu.");
                    return BadRequest("Kullanıcı güncellenirken bir hata oluştu.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Kullanıcı güncellenirken bir hata oluştu: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        #endregion
        #region ChangePassword
        [HttpPut("ChangePassword")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto passwordDto)
        {
            try
            {
                var user = await _authService.GetUserByUserName(passwordDto.UserName);
                if (user == null)
                {
                    _logger.LogError("Kullanıcı bulunamadı.");
                    return NotFound("Kullanıcı bulunamadı.");
                }

                var result = await _authService.ChangePassword(user, passwordDto.CurrentPassword, passwordDto.NewPassword);

                if (result.Succeeded)
                {
                    _logger.LogInformation("Kullanıcı başarıyla güncellendi.");
                    return Ok("Kullanıcı başarıyla güncellendi.");
                }
                else
                {
                    _logger.LogError("Kullanıcı güncellenirken bir hata oluştu.");
                    return BadRequest("Kullanıcı güncellenirken bir hata oluştu.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Kullanıcı güncellenirken bir hata oluştu: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }
        #endregion
        #region DeleteUser
        [Authorize(Policy = "DeleteUser")]
        [HttpDelete("DeleteUser/{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            bool isDeleted = await _authService.DeleteUser(id);
            if (isDeleted) return Ok();
            else return BadRequest();
        }
        #endregion
        #region GetAllUsers
        [HttpGet("GetAllUsers")]
        [Authorize(Policy = "GetAllUsers")]
        public IActionResult GetAllUsers()
        {
            var users = _authService.GetAllUsers();
            if (users is not null) return Ok(users);
            else return BadRequest("Kullanıcıları getirme işlemi başarısız");
        }
        #endregion
        #region GetPaginatedUsers
        [HttpGet("GetPaginatedUsers")]
        [Authorize(Policy = "GetAllUsers")]
        public async Task<IActionResult> GetPaginatedUsers([FromQuery] RequestParameters parameters, bool trackChanges)
        {
            var users = await _authService.GetPaginatedUsers(parameters, trackChanges);
            if (users is not null) return Ok(users);
            else return BadRequest("Kullanıcıları getirme işlemi başarısız");
        }
        #endregion
        #region GetUserByUserName
        [HttpGet("GetUserByUserName/{userName}")]
        public async Task<IActionResult> GetUserByUserName(string userName)
        {
            User user = await _authService.GetUserByUserName(userName);
            if (user is not null) return Ok(user);
            else return BadRequest();
        }
        #endregion
        #region GetUserCount
        [HttpGet("GetUserCount")]
        [Authorize(Policy = "GetUserCount")]
        public async Task<IActionResult> GetUserCount([FromQuery] Guid companyId, [FromQuery] Guid applicationId)
        {
            try
            {
                // Eğer companyId ve applicationId geçerliyse, Kullanıcı sayısını almak için gerekli işlemleri yap
                if (!(companyId == Guid.Empty) && !(applicationId == Guid.Empty))
                {
                    // Kullanıcı sayısını, companyId ve applicationId'ye göre filtreleyerek almak için gerekli methodu çağır
                    var userCount = await _authService.GetAllUserCountByCompanyApplicationId(companyId, applicationId);
                    _logger.LogInformation("Kullanıcı sayısı çekildi: {Count}", userCount);
                    return Ok(userCount);
                }
                else return BadRequest("Kullanıcı sayısı çekilirken bir hata oluştu.");

            }
            catch (Exception ex)
            {
                _logger.LogError("Kullanıcı sayısı çekilirken bir hata oluştu: {Message}", ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }
        #endregion  
        #region AddRoleClaim
        [HttpPost("AddRoleClaim")]
        public async Task<IActionResult> AddRoleClaim(string roleId, string claimType, string claimValue)
        {
            var result = await _authService.AddRoleClaimAsync(roleId, claimType, claimValue);

            if (result.Succeeded)
                return Ok("Claim added to role");
            else
                return BadRequest("Failed to add claim to role");
        }
        #endregion
        #region AddUserClaim
        [HttpPost("AddUserClaim")]
        public async Task<IActionResult> AddUserClaim(string userId, string claimType, string claimValue)
        {
            var result = await _authService.AddUserClaimAsync(userId, claimType, claimValue);

            if (result.Succeeded)
                return Ok("Claim added to user");
            else
                return BadRequest("Failed to add claim to user");
        }
        #endregion
        #region GetUserClaimsByUserId
        [HttpGet("GetUserClaimsByUserId/{userId}")]
        public async Task<IActionResult> GetUserClaimsByUserId(string userId)
        {
            var claims = await _authService.GetUserClaimsAsync(userId);
            return Ok(claims);
        }
        #endregion
        #region GetRoleClaimsByRoleId
        [HttpGet("GetRoleClaimsByRoleId/{roleId}")]
        public async Task<IActionResult> GetRoleClaimsByRoleId(string roleId)
        {
            var claims = await _authService.GetRoleClaimsAsync(roleId);
            return Ok(claims);
        }
        #endregion
        #region GetUserClaims/{username}
        [HttpGet("GetUserClaims/{username}")]
        public async Task<IActionResult> GetUserClaims(string userName)
        {
            var user = await _authService.GetUserByUserName(userName);
            var claim = await _authService.GetUserClaimsAsync(user.Id);
            return Ok(claim);
        }
        #endregion
        #region UpdateUserClaims
        [HttpPut("UpdateUserClaims")]
        public async Task<IActionResult> UpdateUserClaims(string userId, [FromBody] List<ClaimDto> claims)
        {
            if (string.IsNullOrEmpty(userId) || claims == null)
                return BadRequest("Bilinmeyen kullanıcıID ya da yetkisi.");

            try
            {
                var result = await _authService.UpdateUserClaimsAsync(userId, claims);

                if (result.Succeeded)
                    return Ok("Claims updated successfully.");

                return BadRequest("Error updating claims.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating claims for user {userId}: {ex.Message}");
                return StatusCode(500, "Internal server error.");
            }
        }
        #endregion
        #region UpdateRoleClaims
        [HttpPut("UpdateRoleClaims")]
        public async Task<IActionResult> UpdateRoleClaims(string roleId, [FromBody] List<ClaimDto> claims)
        {
            if (string.IsNullOrEmpty(roleId) || claims == null)
                return BadRequest("Bilinmeyen roleId ya da yetkisi.");

            try
            {
                var result = await _authService.UpdateRoleClaimsAsync(roleId, claims);

                if (result.Succeeded)
                    return Ok("Claims updated successfully.");

                return BadRequest("Error updating claims.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating claims for role {roleId}: {ex.Message}");
                return StatusCode(500, "Internal server error.");
            }
        }
        #endregion
        #region SifremiUnuttum
        [HttpPost("ForgotPassword")]
        public async Task<IActionResult> ForgotPassword(EmailDto email)
        {
            try
            {
                if (string.IsNullOrEmpty(email.Email))
                    return BadRequest("E-posta adresi gerekli.");

                var result = await _authService.ForgotPasswordAsync(email.Email);
                if (!result)
                    return NotFound("Kullanıcı bulunamadı.");

                return Ok("Şifre sıfırlama e-postası gönderildi.");
            }
            catch (Exception ex)
            {

                throw ex;
            }
            
        }

        #endregion
        #region ŞifreYenileme
        [HttpPost("ResetPassword")]
        public async Task<IActionResult> ResetPassword(ResetPasswordDto resetPasswordDto)
        {
            if (!ModelState.IsValid)
                return BadRequest("Geçersiz giriş.");

            var result = await _authService.ResetPasswordAsync(resetPasswordDto);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            return Ok("Şifre başarıyla sıfırlandı.");
        }
        #endregion
    }
}