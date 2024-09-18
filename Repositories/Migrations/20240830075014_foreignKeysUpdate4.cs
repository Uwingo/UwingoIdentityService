using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Repositories.Migrations
{
    /// <inheritdoc />
    public partial class foreignKeysUpdate4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_RolePermissions",
                table: "RolePermissions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CompanyApplications_1",
                table: "CompanyApplications");

            

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "RolePermissions",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_RolePermissions",
                table: "RolePermissions",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CompanyApplications",
                table: "CompanyApplications",
                column: "Id");


        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_RolePermissions",
                table: "RolePermissions");


            migrationBuilder.DropPrimaryKey(
                name: "PK_CompanyApplications",
                table: "CompanyApplications");


           

            migrationBuilder.DropColumn(
                name: "Id",
                table: "RolePermissions");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RolePermissions",
                table: "RolePermissions",
                columns: new[] { "RoleId", "PermissionId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_CompanyApplications",
                table: "CompanyApplications",
                columns: new[] { "CompanyId", "ApplicationId" });

          
        }
    }
}
