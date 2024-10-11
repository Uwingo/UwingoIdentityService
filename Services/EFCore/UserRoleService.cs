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
    public class UserRoleService : IUserRoleService
    {
        private readonly IMapper _mapper;
        private readonly IRepositoryManager _repository;
        private readonly IRepositoryAppClientManager _repositoryAppClient;
        private readonly ILogger<UserRoleService> _logger;

        public UserRoleService(IRepositoryManager repository, IRepositoryAppClientManager repositoryAppClient, IMapper mapper, ILogger<UserRoleService> logger)
        {
            _repository = repository;
            _repositoryAppClient = repositoryAppClient;
            _mapper = mapper;
            _logger = logger;
        }

        public UserRoleDto CreateUserRole(UserRoleDto userRoleDto)
        {
            try
            {
                var mappedDto = _mapper.Map<UserRole>(userRoleDto);
                _repository.UserRole.GenericCreate(mappedDto);
                _repository.Save();
                var createdDto = _mapper.Map<UserRoleDto>(mappedDto);
                _logger.LogError("Kullanıcı rolü başarıyla oluşturuldu.");
                return createdDto;
            }
            catch (Exception ex)
            {
                _logger.LogError("Kullanıcı rolü oluşturulurken bir hata oluştu: {Message}", ex.Message);
                throw;
            }
        }

        public void DeleteUserRole(Guid userId, Guid roleId)
        {
            try
            {
                var deletedData = _repository.UserRole.GetUserRole(userId, roleId, false);

                if (deletedData is not null)
                {
                    _repository.UserRole.GenericDelete(deletedData);
                    _repository.Save();
                    _logger.LogError("Kullanıcı rolü başarıyla silindi.");
                }
                else
                {
                    _logger.LogError("Silinmek istenen kullanıcı rolü bulunamadı.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Kullanıcı rolü silinirken bir hata oluştu: {Message}", ex.Message);
                throw;
            }
        }

        public IEnumerable<UserRoleDto> GetAllUserRoles()
        {
            try
            {
                var userRoleList = _repository.UserRole.GenericRead(false);
                var userRoleDtoList = _mapper.Map<IEnumerable<UserRoleDto>>(userRoleList);
                _logger.LogError("Tüm kullanıcı rolleri başarıyla getirildi.");
                return userRoleDtoList;
            }
            catch (Exception ex)
            {
                _logger.LogError("Kullanıcı rolleri getirilirken bir hata oluştu: {Message}", ex.Message);
                throw;
            }
        }

        public UserRoleDto GetUserRole(Guid userId, Guid roleId)
        {
            try
            {
                var userRole = _repository.UserRole.GetUserRole(userId, roleId, false);
                var userRoleDto = _mapper.Map<UserRoleDto>(userRole);
                _logger.LogError("Kullanıcı rolü başarıyla getirildi.");
                return userRoleDto;
            }
            catch (Exception ex)
            {
                _logger.LogError("Kullanıcı rolü getirilirken bir hata oluştu: {Message}", ex.Message);
                throw;
            }
        }

        public void UpdateUserRole(UserRoleDto userRoleDto)
        {
            try
            {
                var updatedData = _repository.UserRole.GetUserRole(userRoleDto.UserId, userRoleDto.RoleId, false);

                if (updatedData is not null)
                {
                    var mappedData = _mapper.Map(userRoleDto, updatedData);
                    _repository.UserRole.GenericUpdate(mappedData);
                    _repository.Save();
                    _logger.LogError("Kullanıcı rolü başarıyla güncellendi.");
                }
                else
                {
                    _logger.LogError("Güncellenmek istenen kullanıcı rolü bulunamadı.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Kullanıcı rolü güncellenirken bir hata oluştu: {Message}", ex.Message);
                throw;
            }
        }

        public IEnumerable<UserRoleDto> GetUsersByRoleId(Guid roleId)
        {
            try
            {
                var userRoles = _repository.UserRole.GetUserRoleByRoleId(roleId, false);
                var userRoleDtos = _mapper.Map<IEnumerable<UserRoleDto>>(userRoles);
                _logger.LogError("Role ait kullanıcılar başarıyla getirildi.");
                return userRoleDtos;
            }
            catch (Exception ex)
            {
                _logger.LogError("Role ait kullanıcılar getirilirken bir hata oluştu: {Message}", ex.Message);
                throw;
            }
        }

        public IEnumerable<UserRoleDto> GetRolesByUserId(Guid userId)
        {
            try
            {
                var userRoles = _repository.UserRole.GetUserRoleByUserId(userId, false);
                var userRoleDtos = _mapper.Map<IEnumerable<UserRoleDto>>(userRoles);
                _logger.LogError("Kullanıcıya ait roller başarıyla getirildi.");
                return userRoleDtos;
            }
            catch (Exception ex)
            {
                _logger.LogError("Kullanıcıya ait roller getirilirken bir hata oluştu: {Message}", ex.Message);
                throw;
            }
        }
    }
}
