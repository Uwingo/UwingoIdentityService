using Entity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.EFCore.Config
{
    public class RoleConfiguration : IEntityTypeConfiguration<Role>
    {
        public void Configure(EntityTypeBuilder<Role> builder)
        {
            builder.HasData(
                new Role
                {
                    Name = "User",
                    NormalizedName = "USER",
                    Description = "Uygulama Kullanıcısı"
                },
                new Role
                {
                    Name = "Admin",
                    NormalizedName = "ADMIN",
                    Description = "Yönetici (Uwingo) Admin"
                },
                new Role
                {
                    Name = "TenantAdmin",
                    NormalizedName = "TENANTADMIN",
                    Description = "Kiracı Admini"
                });
        }
    }
}
