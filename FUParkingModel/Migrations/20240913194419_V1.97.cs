using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FUParkingModel.Migrations
{
    /// <inheritdoc />
    public partial class V197 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "UserTopUpId",
                schema: "dbo",
                table: "Transaction",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_UserTopUpId",
                schema: "dbo",
                table: "Transaction",
                column: "UserTopUpId");

            migrationBuilder.AddForeignKey(
                name: "FK_Transaction_User_UserTopUpId",
                schema: "dbo",
                table: "Transaction",
                column: "UserTopUpId",
                principalSchema: "dbo",
                principalTable: "User",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transaction_User_UserTopUpId",
                schema: "dbo",
                table: "Transaction");

            migrationBuilder.DropIndex(
                name: "IX_Transaction_UserTopUpId",
                schema: "dbo",
                table: "Transaction");

            migrationBuilder.DropColumn(
                name: "UserTopUpId",
                schema: "dbo",
                table: "Transaction");
        }
    }
}
