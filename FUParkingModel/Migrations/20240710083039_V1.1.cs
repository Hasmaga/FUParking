using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FUParkingModel.Migrations
{
    /// <inheritdoc />
    public partial class V11 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "PaymentMethodId",
                schema: "dbo",
                table: "Session",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Session_PaymentMethodId",
                schema: "dbo",
                table: "Session",
                column: "PaymentMethodId");

            migrationBuilder.AddForeignKey(
                name: "FK_Session_PaymentMethod_PaymentMethodId",
                schema: "dbo",
                table: "Session",
                column: "PaymentMethodId",
                principalSchema: "dbo",
                principalTable: "PaymentMethod",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Session_PaymentMethod_PaymentMethodId",
                schema: "dbo",
                table: "Session");

            migrationBuilder.DropIndex(
                name: "IX_Session_PaymentMethodId",
                schema: "dbo",
                table: "Session");

            migrationBuilder.DropColumn(
                name: "PaymentMethodId",
                schema: "dbo",
                table: "Session");
        }
    }
}
