using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FUParkingModel.Migrations
{
    /// <inheritdoc />
    public partial class V13 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageUrl",
                schema: "dbo",
                table: "Vehicle");

            migrationBuilder.AddColumn<DateTime>(
                name: "EXPDate",
                schema: "dbo",
                table: "Wallet",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BodyImage",
                schema: "dbo",
                table: "Vehicle",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HeadImage",
                schema: "dbo",
                table: "Vehicle",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PlateImage",
                schema: "dbo",
                table: "Vehicle",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ExtraCoin",
                schema: "dbo",
                table: "Package",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FreeCoinEXP",
                schema: "dbo",
                table: "Package",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                schema: "dbo",
                table: "Gate",
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
                name: "EXPDate",
                schema: "dbo",
                table: "Wallet");

            migrationBuilder.DropColumn(
                name: "BodyImage",
                schema: "dbo",
                table: "Vehicle");

            migrationBuilder.DropColumn(
                name: "HeadImage",
                schema: "dbo",
                table: "Vehicle");

            migrationBuilder.DropColumn(
                name: "PlateImage",
                schema: "dbo",
                table: "Vehicle");

            migrationBuilder.DropColumn(
                name: "ExtraCoin",
                schema: "dbo",
                table: "Package");

            migrationBuilder.DropColumn(
                name: "FreeCoinEXP",
                schema: "dbo",
                table: "Package");

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                schema: "dbo",
                table: "Vehicle",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                schema: "dbo",
                table: "Gate",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }
    }
}
