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
    public class TenantController : ControllerBase
    {
        private readonly ITenantService _tenantService;
        private readonly ICompanyService _companyService;
        private readonly ILogger<TenantController> _logger;

        public TenantController(ITenantService tenantService, ICompanyService companyService, ILogger<TenantController> logger)
        {
            _tenantService = tenantService;
            _companyService = companyService;
            _logger = logger;
        }

        [Authorize(Policy = "GetAllTenants")]
        [HttpGet("GetAllTenants")]
        public async Task<IActionResult> GetAllTenants()
        {
            try
            {
                var tenants = _tenantService.GetAllTenants();
                _logger.LogError("Tüm kiracılar başarıyla getirildi.");
                return Ok(tenants);
            }
            catch (Exception ex)
            {
                _logger.LogError("Kiracılar getirilirken bir hata oluştu: {Message}", ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }

        [Authorize(Policy = "GetAllTenants")]
        [HttpGet("GetPaginatedTenants")]
        public async Task<IActionResult> GetPaginatedTenants([FromQuery] RequestParameters parameters)
        {
            try
            {
                var tenants = _tenantService.GetPaginatedTenants(parameters, false);
                _logger.LogInformation($"{parameters.PageNumber}. sayfadaki {parameters.PageSize} kiracı başarıyla getirildi.");
                return Ok(tenants);
            }
            catch (Exception ex)
            {
                _logger.LogError("Kiracılar getirilirken bir hata oluştu: {Message}", ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }

        [Authorize(Policy = "EditTenant")]
        [HttpGet("GetTenantById/{id}")]
        public async Task<IActionResult> GetTenantById(Guid id)
        {
            try
            {
                var tenant = _tenantService.GetTenantById(id);
                if (tenant == null)
                {
                    _logger.LogError("Kiracı bulunamadı.");
                    return NotFound();
                }
                else
                {
                    _logger.LogError("Kiracı başarıyla getirildi.");
                    return Ok(tenant);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Kiracı getirilirken bir hata oluştu: {Message}", ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }

        [Authorize(Policy = "CreateTenant")]
        [HttpPost("CreateTenant")]
        public async Task<IActionResult> CreateTenant([FromBody] TenantDto tenant)
        {
            tenant.Id = Guid.NewGuid();
            try
            {
                if (tenant == null)
                {
                    _logger.LogWarning("Geçersiz kiracı nesnesi gönderildi.");
                    return BadRequest("Tenant object is null");
                }

                var createdTenant = _tenantService.CreateTenant(tenant);
                if (createdTenant is not null)
                {
                    //CompanyDto company = new CompanyDto();
                    //company.Address = adress;
                    //company.TenantId = tenant.Id;
                    //company.Id = Guid.NewGuid();
                    //company.Name = tenant.Name;

                    //var createdCompany = _companyService.CreateCompany(company);
                    _logger.LogError("Kiracı başarıyla oluşturuldu.");
                    return Ok(createdTenant.Id);
                }
                else
                {
                    _logger.LogError("Kiracı oluşturulamadı.");
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Kiracı oluşturulurken bir hata oluştu: {Message}", ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }

        [Authorize(Policy = "EditTenant")]
        [HttpPut("UpdateTenant/{id}")]
        public async Task<IActionResult> UpdateTenant(Guid id, [FromBody] TenantDto tenant)
        {
            try
            {
                if (tenant == null)
                {
                    _logger.LogWarning("Geçersiz kiracı nesnesi gönderildi.");
                    return BadRequest("Tenant object is null");
                }

                _tenantService.UpdateTenant(tenant);
                _logger.LogError("Kiracı başarıyla güncellendi.");
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError("Kiracı güncellenirken bir hata oluştu: {Message}", ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }

        [Authorize(Policy = "DeleteTenant")]
        [HttpDelete("DeleteTenant/{id}")]
        public async Task<IActionResult> DeleteTenant(Guid id)
        {
            try
            {
                _tenantService.DeleteTenant(id);
                var checkProcess = _tenantService.GetTenantById(id);
                if (checkProcess is null)
                {
                    _logger.LogInformation("Kiracı başarıyla silindi.");
                    return Ok();
                }
                else
                {
                    _logger.LogError("Kiracı silinemedi.");
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Kiracı silinirken bir hata oluştu: {Message}", ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }

        [Authorize(Policy = "GetTenantCount")]
        [HttpGet("GetTenantCount")]
        public async Task<IActionResult> GetTenantCount()
        {
            try
            {
                var rolePermCount = _tenantService.GetAllTenants().Count();
                _logger.LogInformation("Kiracı sayısı çekildi");
                return Ok(rolePermCount);
            }
            catch (Exception ex)
            {
                _logger.LogError("Kiracı sayısı çekilirken bir hata oluştu: {Message}", ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
