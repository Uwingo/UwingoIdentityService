using Entity.Models;
using Entity.ModelsDto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Services.Contracts;
using Services.EFCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoleController : ControllerBase
    {
        private readonly IRoleService _roleService;
        private readonly ILogger<RoleController> _logger;

        public RoleController(IRoleService roleService, ILogger<RoleController> logger)
        {
            _roleService = roleService;
            _logger = logger;
        }

        [Authorize(Policy = "GetAllRoles")]
        [HttpGet("GetAllRoles")]
        public async Task<IActionResult> GetAllRoles()
        {
            try
            {
                var roles = _roleService.GetAllRoles();
                _logger.LogInformation("Tüm roller başarıyla getirildi.");
                return Ok(roles);
            }
            catch (Exception ex)
            {
                _logger.LogError("Roller getirilirken bir hata oluştu: {Message}", ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }

        [Authorize(Policy = "GetAllRoles")]
        [HttpGet("GetPaginatedRoles")]
        public async Task<IActionResult> GetPaginatedRoles([FromQuery] RequestParameters parameters)
        {
            try
            {
                var roles = _roleService.GetPaginatedRoles(parameters, false);
                _logger.LogInformation($"{parameters.PageNumber}. sayfadaki {parameters.PageSize} rol başarıyla getirildi.");
                return Ok(roles);
            }
            catch (Exception ex)
            {
                _logger.LogError("Roller getirilirken bir hata oluştu: {Message}", ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }

        [Authorize(Policy = "EditRole")]
        [HttpGet("GetRoleById/{id}")]
        public async Task<IActionResult> GetRoleById(string id)
        {
            try
            {
                var role = await _roleService.GetRoleById(id);
                if (role == null)
                {
                    _logger.LogWarning("Rol bulunamadı.");
                    return NotFound();
                }
                _logger.LogInformation("Rol başarıyla getirildi.");
                return Ok(role);
            }
            catch (Exception ex)
            {
                _logger.LogError("Rol getirilirken bir hata oluştu: {Message}", ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }

        [Authorize(Policy = "CreateRole")]
        [HttpPost("CreateRole")]
        public async Task<IActionResult> CreateRole([FromBody] RoleDto role)
        {
            role.Id = Guid.NewGuid().ToString();
            try
            {
                if (role == null)
                {
                    _logger.LogWarning("Geçersiz rol nesnesi gönderildi.");
                    return BadRequest("Role object is null");
                }

                var createdRole = _roleService.CreateRole(role);
                var checkProcess = _roleService.GetRoleById(role.Id);
                if (checkProcess is not null)
                {
                    _logger.LogInformation("Rol başarıyla oluşturuldu.");
                    return Ok();
                }
               else
                {
                    _logger.LogError("Rol oluşturulurken bir hata oluştu");
                    return BadRequest();
                }
                
            }
            catch (Exception ex)
            {
                _logger.LogError("Rol oluşturulurken bir hata oluştu: {Message}", ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }

        [Authorize(Policy = "EditRole")]
        [HttpPut("UpdateRole/{id}")]
        public async Task<IActionResult> UpdateRole(string id, [FromBody] RoleDto role)
        {
            try
            {
                if (role == null)
                {
                    _logger.LogWarning("Geçersiz rol nesnesi gönderildi.");
                    return BadRequest("Role object is null");
                }

                _roleService.UpdateRole(role);
                _logger.LogInformation("Rol başarıyla güncellendi.");
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError("Rol güncellenirken bir hata oluştu: {Message}", ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }

        [Authorize(Policy = "DeleteRole")]
        [HttpDelete("DeleteRole/{id}")]
        public async Task<IActionResult> DeleteRole(string id)
        {
            try
            {
                _roleService.DeleteRole(id);
                var checkProcess = _roleService.GetRoleById(id);
                if (checkProcess is null)
                {
                    _logger.LogInformation("Rol başarıyla silindi.");
                    return Ok();
                }
                else
                {
                    _logger.LogError("Rol silinirken bir hata oluştu");
                    return BadRequest();
                }       
            }
            catch (Exception ex)
            {
                _logger.LogError("Rol silinirken bir hata oluştu: {Message}", ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }

        [Authorize(Policy = "GetRoleCount")]
        [HttpGet("GetRoleCount")]
        public async Task<IActionResult> GetRoleCount()
        {
            try
            {
                var roleCount = _roleService.GetAllRoles().Count();
                _logger.LogInformation("Rol sayısı çekildi");
                return Ok(roleCount);
            }
            catch (Exception ex)
            {
                _logger.LogError("Rol sayısı çekilirken bir hata oluştu: {Message}", ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
