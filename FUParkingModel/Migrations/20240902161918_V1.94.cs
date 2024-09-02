using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FUParkingModel.Migrations
{
    /// <inheritdoc />
    public partial class V194 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FCMToken",
                schema: "dbo",
                table: "Customer",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FCMToken",
                schema: "dbo",
                table: "Customer");
        }
    }
}
