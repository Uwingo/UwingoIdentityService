using AutoMapper;
using Entity.Models;
using Entity.ModelsDto;
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
    public class CompanyService : ICompanyService
    {
        private readonly IMapper _mapper;
        private readonly IRepositoryManager _repository;
        private readonly IRepositoryAppClientManager _repositoryAppClient;
        private readonly ILogger<CompanyService> _logger;

        public CompanyService(IRepositoryManager repository, IRepositoryAppClientManager repositoryAppClient, IMapper mapper, ILogger<CompanyService> logger)
        {
            _repository = repository;
            _repositoryAppClient = repositoryAppClient;
            _mapper = mapper;
            _logger = logger;
        }

        public CompanyDto CreateCompany(CompanyDto companyDto)
        {
            try
            {
                var mappedDto = _mapper.Map<Company>(companyDto);
                _repositoryAppClient.Company.GenericCreate(mappedDto);
                _repositoryAppClient.Save();
                var createdDto = _mapper.Map<CompanyDto>(mappedDto);
                _logger.LogError("Şirket başarıyla oluşturuldu.");
                return createdDto;
            }
            catch (Exception ex)
            {
                _logger.LogError("Şirket oluşturulurken bir hata oluştu: {Message}", ex.Message);
                throw;
            }
        }

        public void DeleteCompany(Guid id)
        {
            try
            {
                var deletedData = _repositoryAppClient.Company.GetCompany(id, false);

                if (deletedData is not null)
                {
                    _repositoryAppClient.Company.GenericDelete(deletedData);
                    _repositoryAppClient.Save();
                    _logger.LogError("Şirket başarıyla silindi.");
                }
                else
                {
                    _logger.LogError("Silinmek istenen şirket bulunamadı.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Şirket silinirken bir hata oluştu: {Message}", ex.Message);
                throw;
            }
        }

        public IEnumerable<CompanyDto> GetAllCompanies()
        {
            try
            {
                var companyList = _repositoryAppClient.Company.GenericRead(false);
                var companyDtoList = _mapper.Map<IEnumerable<CompanyDto>>(companyList);
                _logger.LogError("Tüm şirketler başarıyla getirildi.");
                return companyDtoList;
            }
            catch (Exception ex)
            {
                _logger.LogError("Şirketler getirilirken bir hata oluştu: {Message}", ex.Message);
                throw;
            }
        }

        public IEnumerable<CompanyDto> GetPaginatedCompanies(RequestParameters parameters, bool trackChanges)
        {
            var companies = _repositoryAppClient.Company.GetPagedCompanies(parameters, trackChanges);
            var companiesDto = _mapper.Map<IEnumerable<CompanyDto>>(companies);

            return companiesDto;
        }

        public CompanyDto GetCompanyById(Guid id)
        {
            try
            {
                var company = _repositoryAppClient.Company.GetCompany(id, false);
                var companyDto = _mapper.Map<CompanyDto>(company);
                _logger.LogError("Şirket başarıyla getirildi.");
                return companyDto;
            }
            catch (Exception ex)
            {
                _logger.LogError("Şirket getirilirken bir hata oluştu: {Message}", ex.Message);
                throw;
            }
        }

        public void UpdateCompany(CompanyDto companyDto)
        {
            try
            {
                var updatedData = _repositoryAppClient.Company.GetCompany(companyDto.Id, false);

                if (updatedData is not null)
                {
                    var mappedData = _mapper.Map(companyDto, updatedData);
                    _repositoryAppClient.Company.GenericUpdate(mappedData);
                    _repositoryAppClient.Save();
                    _logger.LogError("Şirket başarıyla güncellendi.");
                }
                else
                {
                    _logger.LogError("Güncellenmek istenen şirket bulunamadı.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Şirket güncellenirken bir hata oluştu: {Message}", ex.Message);
                throw;
            }
        }
    }
}
