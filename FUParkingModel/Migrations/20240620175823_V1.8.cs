using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FUParkingModel.Migrations
{
    /// <inheritdoc />
    public partial class V18 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PasswordHash",
                schema: "dbo",
                table: "Customer",
                newName: "Avarta");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Avarta",
                schema: "dbo",
                table: "Customer",
                newName: "PasswordHash");
        }
    }
}
