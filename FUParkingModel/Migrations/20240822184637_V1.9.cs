using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FUParkingModel.Migrations
{
    /// <inheritdoc />
    public partial class V19 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImageInBodyUrl",
                schema: "dbo",
                table: "Session",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ImageOutBodyUrl",
                schema: "dbo",
                table: "Session",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageInBodyUrl",
                schema: "dbo",
                table: "Session");

            migrationBuilder.DropColumn(
                name: "ImageOutBodyUrl",
                schema: "dbo",
                table: "Session");
        }
    }
}
