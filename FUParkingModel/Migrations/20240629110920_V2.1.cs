using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FUParkingModel.Migrations
{
    /// <inheritdoc />
    public partial class V21 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "SessionId",
                schema: "dbo",
                table: "Feedback",
                type: "uniqueidentifier",
                nullable: false);

            migrationBuilder.CreateIndex(
                name: "IX_Feedback_SessionId",
                schema: "dbo",
                table: "Feedback",
                column: "SessionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Feedback_Session_SessionId",
                schema: "dbo",
                table: "Feedback",
                column: "SessionId",
                principalSchema: "dbo",
                principalTable: "Session",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Feedback_Session_SessionId",
                schema: "dbo",
                table: "Feedback");

            migrationBuilder.DropIndex(
                name: "IX_Feedback_SessionId",
                schema: "dbo",
                table: "Feedback");

            migrationBuilder.DropColumn(
                name: "SessionId",
                schema: "dbo",
                table: "Feedback");
        }
    }
}
