using Entity.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Repositories;
using RepositoryAppClient.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Repositories.EFCore.Config;

namespace RapositoryAppClient
{
    public class RepositoryContextAppClient : IdentityDbContext<User, Role, string, IdentityUserClaim<string>, UserRole, IdentityUserLogin<string>, IdentityRoleClaim<string>, IdentityUserToken<string>>
    {
        public RepositoryContextAppClient(DbContextOptions<RepositoryContextAppClient> options) : base(options) { }


        public DbSet<Company> Companies { get; set; }
        public DbSet<Application> Applications { get; set; }
        public DbSet<Tenant> Tenants { get; set; }
        public DbSet<User> Employees { get; set; }
        public DbSet<CompanyApplication> CompanyApplications { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.ApplyConfiguration(new RoleConfiguration());

            // Tenant - Company ilişkisi
            builder.Entity<Company>()
                .HasOne(c => c.Tenant)
                .WithMany()
                .HasForeignKey(c => c.TenantId)
                .OnDelete(DeleteBehavior.Cascade);


            // Company - Application Many-to-Many ilişkisi
            builder.Entity<CompanyApplication>()
                .HasKey(ca => ca.Id);

            builder.Entity<CompanyApplication>()
                .HasOne(ca => ca.Company)
                .WithMany(c => c.CompanyApplications)
                .HasForeignKey(ca => ca.CompanyId);

            builder.Entity<CompanyApplication>()
                .HasOne(ca => ca.Application)
                .WithMany(a => a.CompanyApplications)
                .HasForeignKey(ca => ca.ApplicationId);

            // IdentityUserRole ilişkisi
            builder.Entity<UserRole>(userRole =>
            {
                userRole.HasKey(ur => new { ur.UserId, ur.RoleId });

                userRole.HasOne(ur => ur.User)
                    .WithMany(u => u.UserRoles)
                    .HasForeignKey(ur => ur.UserId)
                    .IsRequired();

                userRole.HasOne(ur => ur.Role)
                    .WithMany(r => r.UserRoles)
                    .HasForeignKey(ur => ur.RoleId)
                    .IsRequired();
            });
        }
    }
}
