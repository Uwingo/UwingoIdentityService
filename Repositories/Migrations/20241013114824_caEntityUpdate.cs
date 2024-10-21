using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Repositories.Migrations
{
    /// <inheritdoc />
    public partial class caEntityUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {


            migrationBuilder.DropColumn(
                name: "DbConnection",
                table: "Applications");

            migrationBuilder.AddColumn<string>(
                name: "DbConnection",
                table: "CompanyApplications",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {


            migrationBuilder.DropColumn(
                name: "DbConnection",
                table: "CompanyApplications");

            migrationBuilder.AddColumn<string>(
                name: "DbConnection",
                table: "Applications",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");


        }
    }
}
