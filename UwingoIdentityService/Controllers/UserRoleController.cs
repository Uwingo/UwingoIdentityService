using Entity.ModelsDto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Services.Contracts;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = "UserRoleController")]
    public class UserRoleController : ControllerBase
    {
        private readonly IUserRoleService _userRoleService;
        private readonly ILogger<UserRoleController> _logger;

        public UserRoleController(IUserRoleService userRoleService, ILogger<UserRoleController> logger)
        {
            _userRoleService = userRoleService;
            _logger = logger;
        }

        [Authorize(Policy = "GetAllUserRoles")]
        [HttpGet]
        public async Task<IActionResult> GetAllUserRoles()
        {
            try
            {
                var userRoles = _userRoleService.GetAllUserRoles();
                _logger.LogError("Tüm kullanıcı rolleri başarıyla getirildi.");
                return Ok(userRoles);
            }
            catch (Exception ex)
            {
                _logger.LogError("Kullanıcı rolleri getirilirken bir hata oluştu: {Message}", ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }

        [Authorize(Policy = "EditUserRole")]
        [HttpGet("{userId}/{roleId}", Name = "UserRoleById")]
        public async Task<IActionResult> GetUserRole(Guid userId, Guid roleId)
        {
            try
            {
                var userRole = _userRoleService.GetUserRole(userId, roleId);
                if (userRole == null)
                {
                    _logger.LogError("Kullanıcı rolü bulunamadı.");
                    return NotFound();
                }
                _logger.LogError("Kullanıcı rolü başarıyla getirildi.");
                return Ok(userRole);
            }
            catch (Exception ex)
            {
                _logger.LogError("Kullanıcı rolü getirilirken bir hata oluştu: {Message}", ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }

        [Authorize(Policy = "CreateUserRole")]
        [HttpPost]
        public async Task<IActionResult> CreateUserRole([FromBody] UserRoleDto userRole)
        {
            try
            {
                if (userRole == null)
                {
                    _logger.LogWarning("Geçersiz kullanıcı rolü nesnesi gönderildi.");
                    return BadRequest("UserRole object is null");
                }

                var createdUserRole = _userRoleService.CreateUserRole(userRole);
                _logger.LogError("Kullanıcı rolü başarıyla oluşturuldu.");
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError("Kullanıcı rolü oluşturulurken bir hata oluştu: {Message}", ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }

        [Authorize(Policy = "EditUserRole")]
        [HttpPut("{userId}/{roleId}")]
        public async Task<IActionResult> UpdateUserRole(Guid userId, Guid roleId, [FromBody] UserRoleDto userRole)
        {
            try
            {
                if (userRole == null)
                {
                    _logger.LogWarning("Geçersiz kullanıcı rolü nesnesi gönderildi.");
                    return BadRequest("UserRole object is null");
                }

                _userRoleService.UpdateUserRole(userRole);
                _logger.LogError("Kullanıcı rolü başarıyla güncellendi.");
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError("Kullanıcı rolü güncellenirken bir hata oluştu: {Message}", ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }

        [Authorize(Policy = "DeleteUserRole")]
        [HttpDelete("{userId}/{roleId}")]
        public async Task<IActionResult> DeleteUserRole(Guid userId, Guid roleId)
        {
            try
            {
                _userRoleService.DeleteUserRole(userId, roleId);
                _logger.LogError("Kullanıcı rolü başarıyla silindi.");
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError("Kullanıcı rolü silinirken bir hata oluştu: {Message}", ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
