using Entity.ModelsDto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Services.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UwingoIdentityService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApplicationController : ControllerBase
    {
        private readonly IApplicationService _applicationService;
        private readonly ICompanyApplicationService _companyApplicationService;
        private readonly IAuthenticationService _authenticationService;
        private readonly ICompanyService _companyService;
        private readonly ILogger<ApplicationController> _logger;

        public ApplicationController(IApplicationService applicationService, IAuthenticationService authenticationService, ICompanyApplicationService companyApplicationService, ICompanyService companyService, ILogger<ApplicationController> logger)
        {
            _applicationService = applicationService;
            _companyApplicationService = companyApplicationService;
            _authenticationService = authenticationService;
            _companyService = companyService;
            _logger = logger;
        }

        [Authorize(Policy = "GetAllApplications")]
        [HttpGet("GetAllApplications")]
        public async Task<IActionResult> GetAllApplications()
        {
            try
            {
                var applications = _applicationService.GetAllApplication();
                _logger.LogError("Tüm uygulamalar başarıyla getirildi.");
                return Ok(applications);
            }
            catch (Exception ex)
            {
                _logger.LogError("Uygulamalar getirilirken bir hata oluştu: {Message}", ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }

        [Authorize(Policy = "GetAllApplications")]
        [HttpGet("GetPaginatedApplications")]
        public async Task<IActionResult> GetPaginatedApplications([FromQuery] RequestParameters parameters)
        {
            try
            {
                var applications = _applicationService.GetPaginatedApplication(parameters, false);
                _logger.LogInformation($"{parameters.PageNumber}. sayfadaki {parameters.PageSize} uygulama başarıyla getirildi.");
                return Ok(applications);
            }
            catch (Exception ex)
            {
                _logger.LogError("Uygulamalar getirilirken bir hata oluştu: {Message}", ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("GetAllApplicationsForLogin")]
        public async Task<IActionResult> GetAllApplicationsForLogin()
        {
            try
            {
                var applications = _applicationService.GetAllApplicationForLogin();
                _logger.LogError("Tüm uygulamalar başarıyla getirildi.");
                return Ok(applications);
            }
            catch (Exception ex)
            {
                _logger.LogError("Uygulamalar getirilirken bir hata oluştu: {Message}", ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }
        

        [HttpGet("GetApplicationById/{id}")]
        public async Task<IActionResult> GetApplicationById(Guid id)
        {
            try
            {
                var application = _applicationService.GetByIdApplication(id);
                if (application is null)
                {
                    _logger.LogError("Uygulama bulunamadı.");
                    return NotFound();
                }
                else
                {
                    _logger.LogError("Uygulama başarıyla getirildi.");
                    return Ok(application);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Uygulama getirilirken bir hata oluştu: {Message}", ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }

        [Authorize(Policy = "CreateApplication")]
        [HttpPost("CreateApplication")]
        public async Task<IActionResult> CreateApplication([FromBody] ApplicationDto application)
        {
            application.Id = Guid.NewGuid();
            try
            {
                if (application == null)
                {
                    _logger.LogWarning("Geçersiz uygulama nesnesi gönderildi.");
                    return BadRequest("Application object is null");
                }

                var createdApplication = _applicationService.CreateApplication(application);
                if (createdApplication is not null)
                {
                    _logger.LogError("Uygulama başarıyla oluşturuldu.");
                    return Ok();
                }
                else
                {
                    _logger.LogError("Uygulama oluşturulamadı.");
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Uygulama oluşturulurken bir hata oluştu: {Message}", ex.Message);
                return StatusCode(500, "Catch Bloğuna Düştü");
            }
        }

        [Authorize(Policy = "EditApplication")]
        [HttpPut("UpdateApplication/{id}")]
        public async Task<IActionResult> UpdateApplication(Guid id, [FromBody] ApplicationDto application)
        {
            try
            {
                if (application == null)
                {
                    _logger.LogWarning("Geçersiz uygulama nesnesi gönderildi.");
                    return BadRequest("Application object is null");
                }

                _applicationService.UpdateApplication(application);
                _logger.LogError("Uygulama başarıyla güncellendi.");
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError("Uygulama güncellenirken bir hata oluştu: {Message}", ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }

        [Authorize(Policy = "DeleteApplication")]
        [HttpDelete("DeleteApplication/{id}")]
        public async Task<IActionResult> DeleteApplication(Guid id)
        {
            try
            {
                _applicationService.DeleteApplication(id);
                var checkProcess = _applicationService.GetByIdApplication(id);
                if (checkProcess is null)
                {
                    _logger.LogInformation("Uygulama başarıyla silindi.");
                    return Ok();
                }
                else
                {
                    _logger.LogError("Uygulama silinemedi.");
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Uygulama silinirken bir hata oluştu: {Message}", ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }

        [Authorize(Policy = "GetApplicationCount")]
        [HttpGet("GetApplicationCount")]
        public async Task<IActionResult> GetApplicationCount()
        {
            try
            {
                var applicationCount = _applicationService.GetAllApplication().Count();
                _logger.LogInformation("Uygulama sayısı çekildi");
                return Ok(applicationCount);
            }
            catch (Exception ex)
            {
                _logger.LogError("Uygulama sayısı çekilirken bir hata oluştu: {Message}", ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("GetApplicationByUserName/{userName}")]
        public async Task<IActionResult> GetApplicationByUserName(string userName)
        {
            try
            {
                List<ApplicationDto> applications = new List<ApplicationDto>();
                var user = await _authenticationService.GetUserByUserName(userName);

                var caId = user.CompanyApplicationId;

                Guid companyId = _companyApplicationService.GetCompanyApplication(caId).CompanyId;

                var company = _companyService.GetAllCompanies().Where(c => c.Id == companyId).FirstOrDefault();
                var companyApplications = _companyApplicationService.GetApplicationsByCompanyId(company.Id);

                foreach (var ca in companyApplications)
                {
                    var application = _applicationService.GetAllApplication().Where(c => c.Id == ca.ApplicationId).FirstOrDefault();
                    applications.Add(application);
                }
                _logger.LogInformation("Kullanıcı adıyla şirketin tüm uygulamaları çekildi");
                if (applications.Any()) return Ok(applications);
                else return BadRequest();
            }
            catch (Exception ex)
            {
                _logger.LogError("Kullanıcı adıyla şirketin tüm uygulamaları çekilirken bir hata oluştu: {Message}", ex.Message);
                return StatusCode(500, "Internal server error");
            }
            
        }
    }
}
