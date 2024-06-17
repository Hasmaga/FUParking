using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FUParkingModel.Migrations
{
    /// <inheritdoc />
    public partial class v17 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Camera",
                schema: "dbo");

            migrationBuilder.DropColumn(
                name: "Description",
                schema: "dbo",
                table: "Deposit");

            migrationBuilder.DropColumn(
                name: "Phone",
                schema: "dbo",
                table: "Customer");

            migrationBuilder.RenameColumn(
                name: "Name",
                schema: "dbo",
                table: "Deposit",
                newName: "AppTranId");

            migrationBuilder.AddColumn<string>(
                name: "WalletType",
                schema: "dbo",
                table: "Wallet",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<int>(
                name: "Price",
                schema: "dbo",
                table: "Package",
                type: "int",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,0)");

            migrationBuilder.AddColumn<int>(
                name: "Amount",
                schema: "dbo",
                table: "Deposit",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "CustomerId",
                schema: "dbo",
                table: "Deposit",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "PaymentMethodId",
                schema: "dbo",
                table: "Deposit",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Deposit_CustomerId",
                schema: "dbo",
                table: "Deposit",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Deposit_PaymentMethodId",
                schema: "dbo",
                table: "Deposit",
                column: "PaymentMethodId");

            migrationBuilder.AddForeignKey(
                name: "FK_Deposit_Customer_CustomerId",
                schema: "dbo",
                table: "Deposit",
                column: "CustomerId",
                principalSchema: "dbo",
                principalTable: "Customer",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Deposit_PaymentMethod_PaymentMethodId",
                schema: "dbo",
                table: "Deposit",
                column: "PaymentMethodId",
                principalSchema: "dbo",
                principalTable: "PaymentMethod",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Deposit_Customer_CustomerId",
                schema: "dbo",
                table: "Deposit");

            migrationBuilder.DropForeignKey(
                name: "FK_Deposit_PaymentMethod_PaymentMethodId",
                schema: "dbo",
                table: "Deposit");

            migrationBuilder.DropIndex(
                name: "IX_Deposit_CustomerId",
                schema: "dbo",
                table: "Deposit");

            migrationBuilder.DropIndex(
                name: "IX_Deposit_PaymentMethodId",
                schema: "dbo",
                table: "Deposit");

            migrationBuilder.DropColumn(
                name: "WalletType",
                schema: "dbo",
                table: "Wallet");

            migrationBuilder.DropColumn(
                name: "Amount",
                schema: "dbo",
                table: "Deposit");

            migrationBuilder.DropColumn(
                name: "CustomerId",
                schema: "dbo",
                table: "Deposit");

            migrationBuilder.DropColumn(
                name: "PaymentMethodId",
                schema: "dbo",
                table: "Deposit");

            migrationBuilder.RenameColumn(
                name: "AppTranId",
                schema: "dbo",
                table: "Deposit",
                newName: "Name");

            migrationBuilder.AlterColumn<decimal>(
                name: "Price",
                schema: "dbo",
                table: "Package",
                type: "decimal(18,0)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                schema: "dbo",
                table: "Deposit",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Phone",
                schema: "dbo",
                table: "Customer",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Camera",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Camera", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Camera_Gate_GateId",
                        column: x => x.GateId,
                        principalSchema: "dbo",
                        principalTable: "Gate",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Camera_GateId",
                schema: "dbo",
                table: "Camera",
                column: "GateId");
        }
    }
}
