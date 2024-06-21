using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FUParkingModel.Migrations
{
    /// <inheritdoc />
    public partial class V14 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FreeCoinEXP",
                schema: "dbo",
                table: "Package");

            migrationBuilder.AddColumn<int>(
                name: "EXPPackage",
                schema: "dbo",
                table: "Package",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CardNumber",
                schema: "dbo",
                table: "Card",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EXPPackage",
                schema: "dbo",
                table: "Package");

            migrationBuilder.AddColumn<DateTime>(
                name: "FreeCoinEXP",
                schema: "dbo",
                table: "Package",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CardNumber",
                schema: "dbo",
                table: "Card",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }
    }
}
