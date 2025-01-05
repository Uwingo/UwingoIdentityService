using Entity.Models;
using Entity.ModelsDto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Services.Contracts;
using Services.EFCore;
using System;
using System.Threading.Tasks;

namespace Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompanyApplicationController : ControllerBase
    {
        private readonly ICompanyApplicationService _companyApplicationService;
        private readonly IApplicationService _applicationService;
        private readonly IAuthenticationService _authenticationService;
        private readonly ILogger<CompanyApplicationController> _logger;

        public CompanyApplicationController(ICompanyApplicationService companyApplicationService, IAuthenticationService authenticationService, IApplicationService applicationService, ILogger<CompanyApplicationController> logger)
        {
            _companyApplicationService = companyApplicationService;
            _applicationService = applicationService;
            _authenticationService = authenticationService;
            _logger = logger;
        }

        [Authorize(Policy = "GetAllCompanyApplications")]
        [HttpGet("GetAllCompanyApplications")]
        public async Task<IActionResult> GetAllCompanyApplications()
        {
            try
            {
                var companyApplications = _companyApplicationService.GetAllCompanyApplications();
                _logger.LogInformation("Tüm şirket uygulamaları başarıyla getirildi.");
                return Ok(companyApplications);
            }
            catch (Exception ex)
            {
                _logger.LogError("Şirket uygulamaları getirilirken bir hata oluştu: {Message}", ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }

        [Authorize(Policy = "GetAllCompanyApplications")]
        [HttpGet("GetPaginatedCompanyApplications")]
        public async Task<IActionResult> GetPaginatedCompanyApplications([FromQuery] RequestParameters parameters)
        {
            try
            {
                var companyApplications = _companyApplicationService.GetPaginatedCompanyApplication(parameters, false);
                _logger.LogInformation($"{parameters.PageNumber}. sayfadaki {parameters.PageSize} şirket uygulaması başarıyla getirildi.");
                return Ok(companyApplications);
            }
            catch (Exception ex)
            {
                _logger.LogError("Şirket uygulamaları getirilirken bir hata oluştu: {Message}", ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("GetCompanyApplication/{id}")]
        public async Task<IActionResult> GetCompanyApplicationById(Guid id)
        {
            try
            {
                var companyApplication = _companyApplicationService.GetCompanyApplication(id);
                if (companyApplication == null)
                {
                    _logger.LogWarning("Şirket uygulaması bulunamadı.");
                    return NotFound();
                }
                _logger.LogInformation("Şirket uygulaması başarıyla getirildi.");
                return Ok(companyApplication);
            }
            catch (Exception ex)
            {
                _logger.LogError("Şirket uygulaması getirilirken bir hata oluştu: {Message}", ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }

        [Authorize(Policy = "CreateCompanyApplication")]
        [HttpPost("CreateCompanyApplication")]
        public async Task<IActionResult> CreateCompanyApplication([FromBody] CompanyApplicationDto companyApplication)
        {
            try
            {
                if (companyApplication == null)
                {
                    _logger.LogWarning("Geçersiz şirket uygulaması nesnesi gönderildi.");
                    return BadRequest("CompanyApplication object is null");
                }

                var createdCompanyApplication = _companyApplicationService.CreateCompanyApplication(companyApplication);
                if (createdCompanyApplication != null)
                {
                    _logger.LogInformation("Şirket uygulaması başarıyla oluşturuldu.");
                    return Ok(createdCompanyApplication);
                }
                else
                {
                    _logger.LogError("Şirket uygulaması eklenirken bir hata oluştu");
                    return BadRequest("Şirket uygulaması eklenemedi");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Şirket uygulaması eklenirken bir hata oluştu: {Message}", ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }

        [Authorize(Policy = "EditCompanyApplication")]
        [HttpPut("UpdateCompanyApplication")]
        public async Task<IActionResult> UpdateCompanyApplication([FromBody] CompanyApplicationDto companyApplication)
        {
            try
            {
                if (companyApplication == null)
                {
                    _logger.LogWarning("Geçersiz şirket uygulaması nesnesi gönderildi.");
                    return BadRequest("CompanyApplication object is null");
                }

                _companyApplicationService.UpdateCompanyApplication(companyApplication);
                _logger.LogInformation("Şirket uygulaması başarıyla güncellendi.");
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError("Şirket uygulaması güncellenirken bir hata oluştu: {Message}", ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }

        [Authorize(Policy = "DeleteCompanyApplication")]
        [HttpDelete("DeleteCompanyApplication/{id}")]
        public async Task<IActionResult> DeleteCompanyApplication(Guid id)
        {
            try
            {
                _companyApplicationService.DeleteCompanyApplication(id);
                var checkProcess = _companyApplicationService.GetCompanyApplication(id);
                if (checkProcess == null)
                {
                    _logger.LogInformation("Şirket uygulaması başarıyla silindi.");
                    return Ok();
                }
                else
                {
                    _logger.LogError("Şirket uygulaması silinirken bir hata oluştu");
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Şirket uygulaması silinirken bir hata oluştu: {Message}", ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }

        [Authorize(Policy = "GetCompanyApplicationCount")]
        [HttpGet("GetCompanyApplicationCount")]
        public async Task<IActionResult> GetCompanyApplicationCount()
        {
            try
            {
                var caCount = _companyApplicationService.GetAllCompanyApplications().Count();
                _logger.LogInformation("Şirket uygulaması sayısı başarıyla getirildi.");
                return Ok(caCount);
            }
            catch (Exception ex)
            {
                _logger.LogError("Şirket uygulaması sayısı çekilirken bir hata oluştu: {Message}", ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("GetApplicationsByCompanyId/{companyId}")]
        public IActionResult GetApplicationsByCompanyId(Guid companyId)
        {
            try
            {
                // CompanyId'ye ait tüm uygulamaları çek
                var companyApplications = _companyApplicationService.GetApplicationsByCompanyId(companyId);
                List<ApplicationDto> applications = new List<ApplicationDto>();

                foreach (var ca in companyApplications)
                {
                    var application = _applicationService.GetByIdApplication(ca.ApplicationId);
                    applications.Add(application);
                }

                if (applications == null || !applications.Any()) return NotFound("Bu şirkete ait aktif uygulama bulunamadı.");          
                return Ok(applications);
            }
            catch (Exception ex)
            {
                // Hata durumunda loglama ve 500 hatası dönme
                return StatusCode(500, $"Uygulamalar getirilirken bir hata oluştu: {ex.Message}");
            }
        }

        [HttpGet("GetCompanyApplicationIdByUserName/{userName}")]
        public async Task<IActionResult> GetCompanyApplicationIdByUserName(string userName)
        {
            User user = await _authenticationService.GetUserByUserName(userName);
            Guid caId = user.CompanyApplicationId;

            return Ok(caId);
        }

        [HttpGet("GetCompanyApplicationsByUserName/{userName}")]
        public async Task<IActionResult> GetCompanyApplicationsByUserName(string userName)
        {
            User user = await _authenticationService.GetUserByUserName(userName);
            var usersCompanyId = _companyApplicationService.GetAllCompanyApplications().Where(ca => ca.Id == user.CompanyApplicationId).FirstOrDefault().CompanyId;
            var companyApplications = _companyApplicationService.GetApplicationsByCompanyId(usersCompanyId);

            return Ok(companyApplications);
        }
    }
}
