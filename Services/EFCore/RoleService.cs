using AutoMapper;
using Entity.Models;
using Entity.ModelsDto;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using RapositoryAppClient;
using Repositories.Contracts;
using RepositoryAppClient.Contracts;
using Services.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;

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

        public async Task<RoleDto> CreateRole(RoleDto roleDto)
        {
            try
            {
                var mappedDto = _mapper.Map<Role>(roleDto);
                mappedDto.NormalizedName = mappedDto.Name.ToUpperInvariant();



                // Dinamik veritabanı kontrolü
                CompanyApplication companyApplication = _repository.CompanyApplication.GetCompanyApplicationByApplicationAndCompanyId(roleDto.CompanyId, roleDto.ApplicationId, false);
                string dbConnection = companyApplication.DbConnection;

                RoleDatabaseMatch roleDatabaseMatch = new RoleDatabaseMatch()
                {
                    CompanyApplicationId = companyApplication.Id,
                    Id = Guid.NewGuid(),
                    RoleId = mappedDto.Id,
                };

                _repository.RoleDbMatch.GenericCreate(roleDatabaseMatch);
                _repository.Save();
                var context = await ChangeDatabase(dbConnection);
                var roleManager = CreateRoleManager(context);

                await roleManager.CreateAsync(mappedDto);


                var createdDto = _mapper.Map<RoleDto>(mappedDto);

               
                _logger.LogInformation("Rol başarıyla oluşturuldu.");
                return createdDto;
            }
            catch (Exception ex)
            {
                _logger.LogError("Rol oluşturulurken bir hata oluştu: {Message}", ex.Message);
                throw;
            }
        }

        public async Task DeleteRole(string id)
        {
            try
            {
                // İlk veritabanında rolü bul ve sil
                var deletedData = _repository.Role.GetRole(id, false);

                if (deletedData is not null)
                {
                    _repository.Role.GenericDelete(deletedData);
                    _repository.Save();
                    _logger.LogInformation("Rol başarıyla silindi.");
                    return;
                }

                // Eğer rol ilk veritabanında bulunamazsa, dinamik veritabanına geç
                Guid companyApplicationId = _repository.RoleDbMatch.GetRolesCompanyApplicationId(id, false);
                string dbString = _repository.CompanyApplication.GetCompanyApplication(companyApplicationId, false).DbConnection;

                var context = await ChangeDatabase(dbString);
                var roleManager = CreateRoleManager(context);

                // Dinamik veritabanında rolü bul
                var dynamicRole = await roleManager.FindByIdAsync(id);
                if (dynamicRole != null)
                {
                    await roleManager.DeleteAsync(dynamicRole); // Asenkron sil
                    _logger.LogInformation("Dinamik veritabanından rol başarıyla silindi.");
                }
                else
                {
                    _logger.LogWarning("Silinmek istenen rol dinamik veritabanında bulunamadı.");
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
                var roleList = _repository.Role.GenericRead(false);
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

        public async Task<int> GetAllRolesByCompanyApplicationId(Guid companyId, Guid applicationId)
        {
            string dbString = _repository.CompanyApplication.GetCompanyApplicationByApplicationAndCompanyId(companyId, applicationId, false).DbConnection;

            var context = await ChangeDatabase(dbString);
            var roleManager = CreateRoleManager(context);

            var roleCount = roleManager.Roles.Count();

            return roleCount;
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
        public async Task<RoleDto> GetRoleById(string id)
        {
            try
            {
                // İlk veritabanında rolü bul
                var role = _repository.Role.GetRole(id, false);
                if (role != null)
                {
                    var roleDto = _mapper.Map<RoleDto>(role);
                    _logger.LogInformation("Rol başarıyla getirildi.");
                    return roleDto;
                }

                // Eğer rol ilk veritabanında bulunamazsa, dinamik veritabanına geç
                Guid companyApplicationId = _repository.RoleDbMatch.GetRolesCompanyApplicationId(id, false);
                string dbString = _repository.CompanyApplication.GetCompanyApplication(companyApplicationId, false).DbConnection;

                var context = await ChangeDatabase(dbString);
                var roleManager = CreateRoleManager(context);

                // Dinamik veritabanında rolü bul
                var dynamicRole = await roleManager.FindByIdAsync(id);
                if (dynamicRole != null)
                {
                    var dynamicRoleDto = _mapper.Map<RoleDto>(dynamicRole);
                    _logger.LogInformation("Dinamik veritabanından rol başarıyla getirildi.");
                    return dynamicRoleDto;
                }

                // Eğer hala rol bulunamazsa
                _logger.LogWarning("Rol bulunamadı: {RoleId}", id);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError("Rol getirilirken bir hata oluştu: {Message}", ex.Message);
                throw;
            }
        }


        public async Task UpdateRole(RoleDto roleDto)
        {
            try
            {
                Guid caId = _repository.RoleDbMatch.GetRolesCompanyApplicationId(roleDto.Id, false);
                string dbConnection = _repository.CompanyApplication.GetCompanyApplication(caId, false).DbConnection;
                var context = await ChangeDatabase(dbConnection);
                var roleManager = CreateRoleManager(context);

                var updatedData = await roleManager.FindByIdAsync(roleDto.Id);

                if (updatedData is not null)
                {
                    var mappedData = _mapper.Map(roleDto, updatedData);
                    mappedData.NormalizedName = mappedData.Name.ToUpperInvariant();
                    context.Roles.Update(mappedData);
                    await context.SaveChangesAsync();
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

        private async Task<RepositoryContextAppClient> ChangeDatabase(string dbString)
        {
            var optionsBuilder = new DbContextOptionsBuilder<RepositoryContextAppClient>();
            optionsBuilder.UseSqlServer(dbString);
            var newContext = new RepositoryContextAppClient(optionsBuilder.Options);
            return newContext;
        }

        private RoleManager<Role> CreateRoleManager(RepositoryContextAppClient contextAppClient)
        {
            // RoleStore nesnesini oluşturun
            var roleStore = new RoleStore<Role, RepositoryContextAppClient, string>(contextAppClient);

            // ILoggerFactory kullanarak doğru türde bir logger oluştur
            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            var roleLogger = loggerFactory.CreateLogger<RoleManager<Role>>();

            // RoleManager nesnesini oluşturun ve geri döndürün
            var roleManagerAppClient = new RoleManager<Role>(
                roleStore,
                new IRoleValidator<Role>[0],
                new UpperInvariantLookupNormalizer(),
                new IdentityErrorDescriber(),
                roleLogger
            );

            return roleManagerAppClient;
        }
    }
}
