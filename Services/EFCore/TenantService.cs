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
    public class TenantService : ITenantService
    {
        private readonly IMapper _mapper;
        private readonly IRepositoryManager _repository;
        private readonly IRepositoryAppClientManager _repositoryAppClient;
        private readonly ILogger<TenantService> _logger;

        public TenantService(IRepositoryManager repository, IRepositoryAppClientManager repositoryAppClient, IMapper mapper, ILogger<TenantService> logger)
        {
            _repository = repository;
            _repositoryAppClient = repositoryAppClient;
            _mapper = mapper;
            _logger = logger;
        }

        public TenantDto CreateTenant(TenantDto tenantDto)
        {
            try
            {
                var mappedDto = _mapper.Map<Tenant>(tenantDto);
                _repositoryAppClient.Tenant.GenericCreate(mappedDto);
                _repositoryAppClient.Save();
                var createdDto = _mapper.Map<TenantDto>(mappedDto);
                _logger.LogError("Kiracı başarıyla oluşturuldu.");
                return createdDto;
            }
            catch (Exception ex)
            {
                _logger.LogError("Kiracı oluşturulurken bir hata oluştu: {Message}", ex.Message);
                throw;
            }
        }

        public void DeleteTenant(Guid id)
        {
            try
            {
                var deletedData = _repositoryAppClient.Tenant.GetTenantByTenantId(id, false);

                if (deletedData is not null)
                {
                    _repositoryAppClient.Tenant.GenericDelete(deletedData);
                    _repositoryAppClient.Save();
                    _logger.LogError("Kiracı başarıyla silindi.");
                }
                else
                {
                    _logger.LogError("Silinmek istenen kiracı bulunamadı.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Kiracı silinirken bir hata oluştu: {Message}", ex.Message);
                throw;
            }
        }

        public IEnumerable<TenantDto> GetAllTenants()
        {
            try
            {
                var tenantList = _repositoryAppClient.Tenant.GenericRead(false);
                var tenantDtoList = _mapper.Map<IEnumerable<TenantDto>>(tenantList);
                _logger.LogError("Tüm kiracılar başarıyla getirildi.");
                return tenantDtoList;
            }
            catch (Exception ex)
            {
                _logger.LogError("Kiracılar getirilirken bir hata oluştu: {Message}", ex.Message);
                throw;
            }
        }

        public IEnumerable<TenantDto> GetPaginatedTenants(RequestParameters parameters, bool trackChanges)
        {
            try
            {
                var tenants = _repositoryAppClient.Tenant.GetPagedTenants(parameters, trackChanges);
                var tenantsDto = _mapper.Map<IEnumerable<TenantDto>>(tenants);
                _logger.LogError("Tüm kiracılar pagination ile başarıyla getirildi.");
                return tenantsDto;
            }
            catch (Exception ex)
            {
                _logger.LogError("Kiracılar getirilirken bir hata oluştu: {Message}", ex.Message);
                throw;
            }
        }

        public TenantDto GetTenantById(Guid id)
        {
            try
            {
                var tenant = _repositoryAppClient.Tenant.GetTenantByTenantId(id, false);
                var tenantDto = _mapper.Map<TenantDto>(tenant);
                _logger.LogError("Kiracı tenantId ile başarıyla getirildi.");
                return tenantDto;
            }
            catch (Exception ex)
            {
                _logger.LogError("Kiracı tenantId ile getirilirken bir hata oluştu: {Message}", ex.Message);
                throw;
            }
        }

        public void UpdateTenant(TenantDto tenantDto)
        {
            try
            {
                var updatedData = _repositoryAppClient.Tenant.GetTenantByTenantId(tenantDto.Id, false);

                if (updatedData is not null)
                {
                    var mappedData = _mapper.Map(tenantDto, updatedData);
                    _repositoryAppClient.Tenant.GenericUpdate(mappedData);
                    _repositoryAppClient.Save();
                    _logger.LogError("Kiracı başarıyla güncellendi.");
                }
                else
                {
                    _logger.LogError("Güncellenmek istenen kiracı bulunamadı.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Kiracı güncellenirken bir hata oluştu: {Message}", ex.Message);
                throw;
            }
        }


    }
}