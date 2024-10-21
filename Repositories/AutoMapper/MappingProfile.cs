using AutoMapper;
using Entity.Models;
using Entity.ModelsDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.AutoMapper
{
    public class MappingProfile : Profile
    {
        public MappingProfile() 
        {
            CreateMap<Application, ApplicationDto>().ReverseMap();
            CreateMap<Company, CompanyDto>().ReverseMap();
            CreateMap<CompanyApplication, CompanyApplicationDto>().ReverseMap();
            CreateMap<Role, RoleDto>().ReverseMap();
            CreateMap<Tenant, TenantDto>().ReverseMap();
            CreateMap<UwingoUser, UwingoUserDto>().ReverseMap();
            CreateMap<UwingoUser, User>().ReverseMap();
            CreateMap<UserRegistrationDto, User>().ReverseMap();
            CreateMap<User, UserDto>().ReverseMap();
            CreateMap<UserDatabaseMatch, UserDatabaseMatchDto>().ReverseMap();
            CreateMap<RoleDatabaseMatch, RoleDatabaseMatchDto>().ReverseMap();
        }
    }
}
