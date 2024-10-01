using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Repositories.Migrations
{
    /// <inheritdoc />
    public partial class userEntityUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {


            migrationBuilder.AddColumn<Guid>(
                name: "CompanyApplicationId",
                table: "AspNetUsers",
                type: "uniqueidentifier",
                nullable: false);

            migrationBuilder.CreateIndex(
               name: "IX_AspNetUsers_CompanyApplicationId",
               table: "AspNetUsers",
               column: "CompanyApplicationId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_CompanyApplications_CompanyApplicationId",
                table: "AspNetUsers",
                column: "CompanyApplicationId",
                principalTable: "CompanyApplications",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // CompanyApplicationId'yi kaldırın
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_CompanyApplications_CompanyApplicationId",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_CompanyApplicationId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "CompanyApplicationId",
                table: "AspNetUsers");
        }
    }
}
