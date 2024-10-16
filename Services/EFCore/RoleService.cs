﻿using AutoMapper;
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
    public class RoleService : IRoleService
    {
        private readonly IMapper _mapper;
        private readonly IRepositoryAppClientManager _repositoryAppClient;
        private readonly IRepositoryManager _repository;
        private readonly ILogger<RoleService> _logger;

        public RoleService(IRepositoryManager repository, IRepositoryAppClientManager repositoryAppClient, IMapper mapper, ILogger<RoleService> logger)
        {
            _repository = repository;
            _repositoryAppClient = repositoryAppClient;
            _mapper = mapper;
            _logger = logger;
        }

        public RoleDto CreateRole(RoleDto roleDto)
        {
            try
            {
                var mappedDto = _mapper.Map<Role>(roleDto);
                mappedDto.NormalizedName = mappedDto.Name.ToUpperInvariant();
                _repositoryAppClient.Role.GenericCreate(mappedDto);
                _repositoryAppClient.Save();
                var createdDto = _mapper.Map<RoleDto>(mappedDto);
                _logger.LogError("Rol başarıyla oluşturuldu.");
                return createdDto;
            }
            catch (Exception ex)
            {
                _logger.LogError("Rol oluşturulurken bir hata oluştu: {Message}", ex.Message);
                throw;
            }
        }

        public void DeleteRole(string id)
        {
            try
            {
                var deletedData = _repositoryAppClient.Role.GetRole(id, false);

                if (deletedData is not null)
                {
                    _repositoryAppClient.Role.GenericDelete(deletedData);
                    _repositoryAppClient.Save();
                    _logger.LogError("Rol başarıyla silindi.");
                }
                else
                {
                    _logger.LogError("Silinmek istenen rol bulunamadı.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Rol silinirken bir hata oluştu: {Message}", ex.Message);
                throw;
            }
        }

        public IEnumerable<RoleDto> GetAllRoles()
        {
            try
            {
                var roleList = _repositoryAppClient.Role.GenericRead(false);
                var roleDtoList = _mapper.Map<IEnumerable<RoleDto>>(roleList);
                _logger.LogError("Tüm roller başarıyla getirildi.");
                return roleDtoList;
            }
            catch (Exception ex)
            {
                _logger.LogError("Roller getirilirken bir hata oluştu: {Message}", ex.Message);
                throw;
            }
        }

        public IEnumerable<RoleDto> GetPaginatedRoles(RequestParameters parameters, bool trackChanges)
        {
            try
            {
                var roles = _repositoryAppClient.Role.GetPagedRoles(parameters, trackChanges);
                var roleDto = _mapper.Map<IEnumerable<RoleDto>>(roles);
                _logger.LogError("Tüm roller paginetion ile başarıyla getirildi.");
                return roleDto;
            }
            catch (Exception ex)
            {
                _logger.LogError("Roller getirilirken bir hata oluştu: {Message}", ex.Message);
                throw;
            }
        }
        public RoleDto GetRoleById(string id)
        {
            try
            {
                var role = _repositoryAppClient.Role.GetRole(id, false);
                var roleDto = _mapper.Map<RoleDto>(role);
                _logger.LogError("Rol başarıyla getirildi.");
                return roleDto;
            }
            catch (Exception ex)
            {
                _logger.LogError("Rol getirilirken bir hata oluştu: {Message}", ex.Message);
                throw;
            }
        }

        public void UpdateRole(RoleDto roleDto)
        {
            try
            {
                var updatedData = _repositoryAppClient.Role.GetRole(roleDto.Id, false);

                if (updatedData is not null)
                {
                    var mappedData = _mapper.Map(roleDto, updatedData);
                    mappedData.NormalizedName = mappedData.Name.ToUpperInvariant();
                    _repositoryAppClient.Role.GenericUpdate(mappedData);
                    _repositoryAppClient.Save();
                    _logger.LogError("Rol başarıyla güncellendi.");
                }
                else
                {
                    _logger.LogError("Güncellenmek istenen rol bulunamadı.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Rol güncellenirken bir hata oluştu: {Message}", ex.Message);
                throw;
            }
        }
    }
}
