using System;
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
                name: "CreateById",
                schema: "dbo",
                table: "Session",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LastModifyById",
                schema: "dbo",
                table: "Session",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifyDate",
                schema: "dbo",
                table: "Session",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                schema: "dbo",
                table: "Session",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Session_CreateById",
                schema: "dbo",
                table: "Session",
                column: "CreateById");

            migrationBuilder.CreateIndex(
                name: "IX_Session_LastModifyById",
                schema: "dbo",
                table: "Session",
                column: "LastModifyById");

            migrationBuilder.AddForeignKey(
                name: "FK_Session_User_CreateById",
                schema: "dbo",
                table: "Session",
                column: "CreateById",
                principalSchema: "dbo",
                principalTable: "User",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Session_User_LastModifyById",
                schema: "dbo",
                table: "Session",
                column: "LastModifyById",
                principalSchema: "dbo",
                principalTable: "User",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Session_User_CreateById",
                schema: "dbo",
                table: "Session");

            migrationBuilder.DropForeignKey(
                name: "FK_Session_User_LastModifyById",
                schema: "dbo",
                table: "Session");

            migrationBuilder.DropIndex(
                name: "IX_Session_CreateById",
                schema: "dbo",
                table: "Session");

            migrationBuilder.DropIndex(
                name: "IX_Session_LastModifyById",
                schema: "dbo",
                table: "Session");

            migrationBuilder.DropColumn(
                name: "CreateById",
                schema: "dbo",
                table: "Session");

            migrationBuilder.DropColumn(
                name: "LastModifyById",
                schema: "dbo",
                table: "Session");

            migrationBuilder.DropColumn(
                name: "LastModifyDate",
                schema: "dbo",
                table: "Session");

            migrationBuilder.DropColumn(
                name: "Status",
                schema: "dbo",
                table: "Session");
        }
    }
}
