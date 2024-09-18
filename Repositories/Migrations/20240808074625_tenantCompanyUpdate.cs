using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repositories.Migrations
{
    /// <inheritdoc />
    public partial class tenantCompanyUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Companies_TenantId",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "Tenants");

            migrationBuilder.CreateIndex(
                name: "IX_Companies_TenantId",
                table: "Companies",
                column: "TenantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Companies_TenantId",
                table: "Companies");

            migrationBuilder.AddColumn<Guid>(
                name: "CompanyId",
                table: "Tenants",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Companies_TenantId",
                table: "Companies",
                column: "TenantId",
                unique: true);
        }
    }
}
