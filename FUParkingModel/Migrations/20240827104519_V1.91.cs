using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FUParkingModel.Migrations
{
    /// <inheritdoc />
    public partial class V191 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_User_LastModifyById",
                schema: "dbo",
                table: "User");

            migrationBuilder.CreateIndex(
                name: "IX_User_CreateById",
                schema: "dbo",
                table: "User",
                column: "CreateById");

            migrationBuilder.CreateIndex(
                name: "IX_User_LastModifyById",
                schema: "dbo",
                table: "User",
                column: "LastModifyById");

            migrationBuilder.AddForeignKey(
                name: "FK_User_User_CreateById",
                schema: "dbo",
                table: "User",
                column: "CreateById",
                principalSchema: "dbo",
                principalTable: "User",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_User_User_CreateById",
                schema: "dbo",
                table: "User");

            migrationBuilder.DropIndex(
                name: "IX_User_CreateById",
                schema: "dbo",
                table: "User");

            migrationBuilder.DropIndex(
                name: "IX_User_LastModifyById",
                schema: "dbo",
                table: "User");

            migrationBuilder.CreateIndex(
                name: "IX_User_LastModifyById",
                schema: "dbo",
                table: "User",
                column: "LastModifyById",
                unique: true,
                filter: "[LastModifyById] IS NOT NULL");
        }
    }
}
