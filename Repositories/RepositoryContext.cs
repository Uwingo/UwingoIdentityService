using Entity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Repositories.EFCore.Config;
using System;

namespace Repositories
{
    public class RepositoryContext : IdentityDbContext<User, Role, string, IdentityUserClaim<string>, IdentityUserRole<string>, IdentityUserLogin<string>, IdentityRoleClaim<string>, IdentityUserToken<string>>
    {
        public RepositoryContext(DbContextOptions<RepositoryContext> options) : base(options) { }

        public DbSet<Company> Companies { get; set; }
        public DbSet<Tenant> Tenants { get; set; }
        public DbSet<User> Employees { get; set; }
        public DbSet<Application> Applications { get; set; }
        public DbSet<CompanyApplication> CompanyApplications { get; set; }
        public DbSet<UserDatabaseMatch> UserDatabaseMatches { get; set; }
        public DbSet<RoleDatabaseMatch> RoleDatabaseMatches { get; set; }

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

            // UserDatabaseMatch entity configuration
            builder.Entity<UserDatabaseMatch>()
                .HasKey(udm => udm.Id);

            builder.Entity<UserDatabaseMatch>()
                .HasOne(udm => udm.CompanyApplication)
                .WithMany()
                .HasForeignKey(udm => udm.CompanyApplicationId);

            // RoleDatabaseMatch entity configuration
            builder.Entity<RoleDatabaseMatch>()
                .HasKey(rdm => rdm.Id);

            builder.Entity<RoleDatabaseMatch>()
                .HasOne(rdm => rdm.CompanyApplication)
                .WithMany()
                .HasForeignKey(rdm => rdm.CompanyApplicationId);
        }
    }
}