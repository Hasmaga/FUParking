using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FUParkingModel.Migrations
{
    /// <inheritdoc />
    public partial class V196 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Gate_GateType_GateTypeId",
                schema: "dbo",
                table: "Gate");

            migrationBuilder.DropTable(
                name: "GateType",
                schema: "dbo");

            migrationBuilder.DropIndex(
                name: "IX_Gate_GateTypeId",
                schema: "dbo",
                table: "Gate");

            migrationBuilder.DropColumn(
                name: "GateTypeId",
                schema: "dbo",
                table: "Gate");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "GateTypeId",
                schema: "dbo",
                table: "Gate",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "GateType",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreateById = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModifyById = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Descriptipn = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastModifyDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GateType", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GateType_User_CreateById",
                        column: x => x.CreateById,
                        principalSchema: "dbo",
                        principalTable: "User",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_GateType_User_LastModifyById",
                        column: x => x.LastModifyById,
                        principalSchema: "dbo",
                        principalTable: "User",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Gate_GateTypeId",
                schema: "dbo",
                table: "Gate",
                column: "GateTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_GateType_CreateById",
                schema: "dbo",
                table: "GateType",
                column: "CreateById");

            migrationBuilder.CreateIndex(
                name: "IX_GateType_LastModifyById",
                schema: "dbo",
                table: "GateType",
                column: "LastModifyById");

            migrationBuilder.AddForeignKey(
                name: "FK_Gate_GateType_GateTypeId",
                schema: "dbo",
                table: "Gate",
                column: "GateTypeId",
                principalSchema: "dbo",
                principalTable: "GateType",
                principalColumn: "Id");
        }
    }
}
