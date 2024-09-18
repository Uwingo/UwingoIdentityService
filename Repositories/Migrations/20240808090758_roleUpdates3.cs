using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Repositories.Migrations
{
    /// <inheritdoc />
    public partial class roleUpdates3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Description", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "07434bdc-8ce9-450f-ac5c-e53308022a28", null, "Kiracı Admini", "TenantAdmin", "TENANTADMIN" },
                    { "93997af7-441d-41ab-bee9-5ca5dc42100d", null, "Uygulama Kullanıcısı", "User", "USER" },
                    { "9970bb6b-2a25-4380-b695-c523b9c0476f", null, "Yönetici (Uwingo) Admin", "Admin", "ADMIN" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "07434bdc-8ce9-450f-ac5c-e53308022a28");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "93997af7-441d-41ab-bee9-5ca5dc42100d");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "9970bb6b-2a25-4380-b695-c523b9c0476f");
        }
    }
}
