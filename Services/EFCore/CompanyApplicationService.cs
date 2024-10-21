using AutoMapper;
using Entity.Models;
using Entity.ModelsDto;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Repositories.Contracts;
using RepositoryAppClient.Contracts;
using Services.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.EFCore
{
    public class CompanyApplicationService : ICompanyApplicationService
    {
        private readonly IRepositoryManager _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<CompanyApplicationService> _logger;

        public CompanyApplicationService(IRepositoryManager repository, IMapper mapper, ILogger<CompanyApplicationService> logger)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
        }

        public CompanyApplicationDto CreateCompanyApplication(CompanyApplicationDto companyApplicationDto)
        {
            try
            {
                companyApplicationDto.Id = Guid.NewGuid();
                var mappedDto = _mapper.Map<CompanyApplication>(companyApplicationDto);
                _repository.CompanyApplication.GenericCreate(mappedDto);
                _repository.Save();
                var createdDto = _mapper.Map<CompanyApplicationDto>(mappedDto);
                _logger.LogError("Şirket uygulaması başarıyla oluşturuldu.");
                return createdDto;
            }
            catch (Exception ex)
            {
                _logger.LogError("Şirket uygulaması oluşturulurken bir hata oluştu: {Message}", ex.Message);
                throw;
            }
        }
        public void DeleteCompanyApplication(Guid id)
        {
            try
            {
                var deletedData = _repository.CompanyApplication.GetCompanyApplication(id, false);

                if (deletedData is not null)
                {
                    _repository.CompanyApplication.GenericDelete(deletedData);
                    _repository.Save();
                    _logger.LogError("Şirket uygulaması başarıyla silindi.");
                }
                else
                {
                    _logger.LogError("Silinmek istenen şirket uygulaması bulunamadı.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Şirket uygulaması silinirken bir hata oluştu: {Message}", ex.Message);
                throw;
            }
        }

        public IEnumerable<CompanyApplicationDto> GetAllCompanyApplications()
        {
            try
            {
                var companyApplicationList = _repository.CompanyApplication.GenericRead(false);
                var companyApplicationDtoList = _mapper.Map<IEnumerable<CompanyApplicationDto>>(companyApplicationList);
                _logger.LogError("Tüm şirket uygulamaları başarıyla getirildi.");
                return companyApplicationDtoList;
            }
            catch (Exception ex)
            {
                _logger.LogError("Şirket uygulamaları getirilirken bir hata oluştu: {Message}", ex.Message);
                throw;
            }
        }

        public IEnumerable<CompanyApplicationDto> GetPaginatedCompanyApplication(RequestParameters parameters, bool trackChanges)
        {
            var companyApplications = _repository.CompanyApplication.GetPagedCompanyApplications(parameters, trackChanges);
            var companyApplicationDto = _mapper.Map<IEnumerable<CompanyApplicationDto>>(companyApplications);

            return companyApplicationDto;
        }

        public CompanyApplicationDto GetCompanyApplication(Guid id)
        {
            try
            {
                var companyApplication = _repository.CompanyApplication.GetCompanyApplication(id, false);
                var companyApplicationDto = _mapper.Map<CompanyApplicationDto>(companyApplication);
                _logger.LogError("Şirket uygulaması başarıyla getirildi.");
                return companyApplicationDto;
            }
            catch (Exception ex)
            {
                _logger.LogError("Şirket uygulaması getirilirken bir hata oluştu: {Message}", ex.Message);
                throw;
            }
        }

        public void UpdateCompanyApplication(CompanyApplicationDto companyApplicationDto)
        {
            try
            {
                var company = _repository.Company.GetCompany(companyApplicationDto.CompanyId, false);
                var application = _repository.Application.GetApplication(companyApplicationDto.ApplicationId, false);
                // Önce veritabanından ilgili kaydı çekiyoruz
                var updatedData = _repository.CompanyApplication.GetCompanyApplication(companyApplicationDto.Id, true);

                if (updatedData is not null)
                {
                    updatedData.Application = application;
                    updatedData.Company = company;
                    // DTO'dan veritabanı modeline veri haritalıyoruz
                    _mapper.Map(companyApplicationDto, updatedData);

                    // Veritabanında güncelleme yapıyoruz
                    _repository.CompanyApplication.GenericUpdate(updatedData);

                    // Değişiklikleri kaydediyoruz
                    _repository.Save();

                    _logger.LogInformation("Şirket uygulaması başarıyla güncellendi.");
                }
                else
                {
                    _logger.LogWarning("Güncellenmek istenen şirket uygulaması bulunamadı.");
                }
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError("Concurrency hatası: {Message}", ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError("Şirket uygulaması güncellenirken bir hata oluştu: {Message}", ex.Message);
                throw;
            }
        }


        public IEnumerable<CompanyApplicationDto> GetApplicationsByCompanyId(Guid companyId)
        {
            try
            {
                var companyApplications = _repository.CompanyApplication.GetCompanyApplicationByCompanyId(companyId, false);
                var companyApplicationDtos = _mapper.Map<IEnumerable<CompanyApplicationDto>>(companyApplications);
                _logger.LogError("Şirketin uygulamaları başarıyla getirildi.");
                return companyApplicationDtos;
            }
            catch (Exception ex)
            {
                _logger.LogError("Şirketin uygulamaları getirilirken bir hata oluştu: {Message}", ex.Message);
                throw;
            }
        }

        public IEnumerable<CompanyApplicationDto> GetCompaniesByApplicationId(Guid applicationId)
        {
            try
            {
                var companyApplications = _repository.CompanyApplication.GetCompanyApplicationByApplicationId(applicationId, false);
                var companyApplicationDtos = _mapper.Map<IEnumerable<CompanyApplicationDto>>(companyApplications);
                _logger.LogError("Uygulamayı kullanan şirketler başarıyla getirildi.");
                return companyApplicationDtos;
            }
            catch (Exception ex)
            {
                _logger.LogError("Uygulamayı kullanan şirketler getirilirken bir hata oluştu: {Message}", ex.Message);
                throw;
            }
        }

        
    }
}
