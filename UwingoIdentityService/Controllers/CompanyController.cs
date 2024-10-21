using Entity.ModelsDto;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Repositories.EFCore;
using Services.Contracts;
using Services.EFCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    
    public class CompanyController : ControllerBase
    {
        private readonly ICompanyService _companyService;
        private readonly ITenantService _tenantService;
        private readonly ILogger<CompanyController> _logger;

        public CompanyController(ICompanyService companyService, ILogger<CompanyController> logger, ITenantService tenantService)
        {
            _companyService = companyService;
            _logger = logger;
            _tenantService = tenantService;
        }

        [Authorize(Policy = "GetAllCompanies")]
        [HttpGet("GetAllCompanies")]
        public async Task<IActionResult> GetAllCompanies()
        {
            try
            {
                var companies = _companyService.GetAllCompanies();
                _logger.LogInformation("Tüm şirketler başarıyla getirildi.");
                return Ok(companies);
            }
            catch (Exception ex)
            {
                _logger.LogError("Şirketler getirilirken bir hata oluştu: {Message}", ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("GetAllCompaniesForLogin")]
        public async Task<IActionResult> GetAllCompaniesForLogin()
        {
            try
            {
                var companies = _companyService.GetAllCompaniesForLogin();
                _logger.LogInformation("Tüm şirketler başarıyla getirildi.");
                return Ok(companies);
            }
            catch (Exception ex)
            {
                _logger.LogError("Şirketler getirilirken bir hata oluştu: {Message}", ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }

        [Authorize(Policy = "GetAllCompanies")]
        [HttpGet("GetPaginatedCompanies")]
        public async Task<IActionResult> GetPaginatedCompanies([FromQuery] RequestParameters parameters)
        {
            try
            {
                var companies = _companyService.GetPaginatedCompanies(parameters, false);
                _logger.LogInformation($"{parameters.PageNumber}. sayfadaki {parameters.PageSize} şirket başarıyla getirildi.");
                return Ok(companies);
            }
            catch (Exception ex)
            {
                _logger.LogError("Şirketler getirilirken bir hata oluştu: {Message}", ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }

        [Authorize(Policy = "EditCompany")]
        [HttpGet("GetCompanyById/{id}")]
        public async Task<IActionResult> GetCompanyById(Guid id)
        {
            try
            {
                var company = _companyService.GetCompanyById(id);
                if (company == null)
                {
                    _logger.LogError("Şirket bulunamadı.");
                    return NotFound();
                }
                _logger.LogInformation("Şirket başarıyla getirildi.");
                return Ok(company);
            }
            catch (Exception ex)
            {
                _logger.LogError("Şirket getirilirken bir hata oluştu: {Message}", ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }

        [Authorize(Policy = "CreateCompany")]
        [HttpPost("CreateCompany")]
        public async Task<IActionResult> CreateCompany([FromBody] CompanyDto company)
        {

            try
            {
                company.Id = Guid.NewGuid();
                if (company == null)
                {
                    _logger.LogWarning("Geçersiz şirket nesnesi gönderildi.");
                    return BadRequest("Company object is null");
                }

                if (company.TenantId.Equals(Guid.Empty))
                {
                    company.TenantId = Guid.NewGuid();
                    TenantDto tenant = new TenantDto();
                    tenant.Name = company.Name;
                    tenant.Id = company.TenantId;

                    var createdTenant = _tenantService.CreateTenant(tenant);
                }
                
                var createdCompany = _companyService.CreateCompany(company);
                _logger.LogError("Şirket başarıyla oluşturuldu.");
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError("Şirket oluşturulurken bir hata oluştu: {Message}", ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }

        [Authorize(Policy = "EditCompany")]
        [HttpPut("UpdateCompany/{id}")]
        public async Task<IActionResult> UpdateCompany([FromBody] CompanyDto company)
        {
            try
            {
                if (company == null)
                {
                    _logger.LogWarning("Geçersiz şirket nesnesi gönderildi.");
                    return BadRequest("Company object is null");
                }

                _companyService.UpdateCompany(company);
                _logger.LogError("Şirket başarıyla güncellendi.");
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError("Şirket güncellenirken bir hata oluştu: {Message}", ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }

        [Authorize(Policy = "DeleteCompany")]
        [HttpDelete("DeleteCompany/{id}")]
        public async Task<IActionResult> DeleteCompany(Guid id)
        {
            try
            {
                //var tenantId = _companyService.GetCompanyById(id).TenantId;
                _companyService.DeleteCompany(id);
                var checkProcess = _companyService.GetCompanyById(id);
                if (checkProcess is null)
                {
                    //_tenantService.DeleteTenant(tenantId);
                    _logger.LogError("Şirket başarıyla silindi.");
                    return Ok();
                }
                else
                {
                    _logger.LogError("Şirket silinirken bir hata oluştu");
                    return BadRequest();
                }

            }
            catch (Exception ex)
            {
                _logger.LogError("Şirket silinirken bir hata oluştu: {Message}", ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete("DeleteCompanyByTenantId/{tenantId}")]
        public IActionResult DeleteCompanyByTenantId(Guid tenantId)
        {
            try
            {
                var datas = _companyService.GetAllCompanies().Where(obj => obj.TenantId == tenantId);
                if (datas is not null)
                {
                    foreach (var company in datas)
                    {
                        _companyService.DeleteCompany(company.Id);
                        _logger.LogInformation("Şirket başarıyla silindi : " + company.Name);
                    }
                    return Ok();
                }
                else
                {
                    _logger.LogError("Şirket silinirken bir hata oluştu");
                    return BadRequest();
                }

            }
            catch (Exception ex)
            {
                _logger.LogError("Şirket silinirken bir hata oluştu: {Message}", ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }

        [Authorize(Policy = "GetCompanyCount")]
        [HttpGet("GetCompanyCount")]
        public async Task<IActionResult> GetCompanyCount()
        {
            try
            {
                var companyCount = _companyService.GetAllCompanies().Count();
                _logger.LogInformation("Şirket sayısı çekildi");
                return Ok(companyCount);
            }
            catch (Exception ex)
            {
                _logger.LogError("Şirket sayısı çekilirken bir hata oluştu: {Message}", ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }

        
    }
}
