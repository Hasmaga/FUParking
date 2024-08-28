using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FUParkingModel.Migrations
{
    /// <inheritdoc />
    public partial class V192 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Session_VehicleType_VehicleTypeId",
                schema: "dbo",
                table: "Session");

            migrationBuilder.AlterColumn<Guid>(
                name: "CustomerId",
                schema: "dbo",
                table: "Vehicle",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "VehicleTypeId",
                schema: "dbo",
                table: "Session",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddForeignKey(
                name: "FK_Session_VehicleType_VehicleTypeId",
                schema: "dbo",
                table: "Session",
                column: "VehicleTypeId",
                principalSchema: "dbo",
                principalTable: "VehicleType",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Session_VehicleType_VehicleTypeId",
                schema: "dbo",
                table: "Session");

            migrationBuilder.AlterColumn<Guid>(
                name: "CustomerId",
                schema: "dbo",
                table: "Vehicle",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "VehicleTypeId",
                schema: "dbo",
                table: "Session",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Session_VehicleType_VehicleTypeId",
                schema: "dbo",
                table: "Session",
                column: "VehicleTypeId",
                principalSchema: "dbo",
                principalTable: "VehicleType",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
