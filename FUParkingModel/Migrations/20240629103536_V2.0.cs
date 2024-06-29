using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FUParkingModel.Migrations
{
    /// <inheritdoc />
    public partial class V20 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WalletStatus",
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
                name: "StatusPayment",
                schema: "dbo",
                table: "Payment");

            migrationBuilder.DropColumn(
                name: "WPFCode",
                schema: "dbo",
                table: "Gate");

            migrationBuilder.AddColumn<Guid>(
                name: "CreateById",
                schema: "dbo",
                table: "VehicleType",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LastModifyById",
                schema: "dbo",
                table: "VehicleType",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifyDate",
                schema: "dbo",
                table: "VehicleType",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "StatusVehicle",
                schema: "dbo",
                table: "Vehicle",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PlateNumber",
                schema: "dbo",
                table: "Vehicle",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PlateImage",
                schema: "dbo",
                table: "Vehicle",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LastModifyById",
                schema: "dbo",
                table: "Vehicle",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifyDate",
                schema: "dbo",
                table: "Vehicle",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "StaffId",
                schema: "dbo",
                table: "Vehicle",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PasswordSalt",
                schema: "dbo",
                table: "User",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PasswordHash",
                schema: "dbo",
                table: "User",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FullName",
                schema: "dbo",
                table: "User",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                schema: "dbo",
                table: "User",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreateById",
                schema: "dbo",
                table: "User",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LastModifyById",
                schema: "dbo",
                table: "User",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifyDate",
                schema: "dbo",
                table: "User",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TransactionStatus",
                schema: "dbo",
                table: "Transaction",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TransactionDescription",
                schema: "dbo",
                table: "Transaction",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ImageOutUrl",
                schema: "dbo",
                table: "Session",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ImageInUrl",
                schema: "dbo",
                table: "Session",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CustomerId",
                schema: "dbo",
                table: "Session",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreateById",
                schema: "dbo",
                table: "Role",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LastModifyById",
                schema: "dbo",
                table: "Role",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifyDate",
                schema: "dbo",
                table: "Role",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "StatusPriceTable",
                schema: "dbo",
                table: "PriceTable",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreateById",
                schema: "dbo",
                table: "PriceTable",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LastModifyById",
                schema: "dbo",
                table: "PriceTable",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifyDate",
                schema: "dbo",
                table: "PriceTable",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "MinPrice",
                schema: "dbo",
                table: "PriceItem",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "MaxPrice",
                schema: "dbo",
                table: "PriceItem",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<Guid>(
                name: "CreateById",
                schema: "dbo",
                table: "PriceItem",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LastModifyById",
                schema: "dbo",
                table: "PriceItem",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifyDate",
                schema: "dbo",
                table: "PriceItem",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                schema: "dbo",
                table: "PaymentMethod",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreateById",
                schema: "dbo",
                table: "PaymentMethod",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LastModifyById",
                schema: "dbo",
                table: "PaymentMethod",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifyDate",
                schema: "dbo",
                table: "PaymentMethod",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "StatusParkingArea",
                schema: "dbo",
                table: "ParkingArea",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Mode",
                schema: "dbo",
                table: "ParkingArea",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreateById",
                schema: "dbo",
                table: "ParkingArea",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LastModifyById",
                schema: "dbo",
                table: "ParkingArea",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifyDate",
                schema: "dbo",
                table: "ParkingArea",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreateById",
                schema: "dbo",
                table: "Package",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LastModifyById",
                schema: "dbo",
                table: "Package",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifyDate",
                schema: "dbo",
                table: "Package",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                schema: "dbo",
                table: "GateType",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Descriptipn",
                schema: "dbo",
                table: "GateType",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreateById",
                schema: "dbo",
                table: "GateType",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LastModifyById",
                schema: "dbo",
                table: "GateType",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifyDate",
                schema: "dbo",
                table: "GateType",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "StatusGate",
                schema: "dbo",
                table: "Gate",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreateById",
                schema: "dbo",
                table: "Gate",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LastModifyById",
                schema: "dbo",
                table: "Gate",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifyDate",
                schema: "dbo",
                table: "Gate",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                schema: "dbo",
                table: "Feedback",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                schema: "dbo",
                table: "CustomerType",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                schema: "dbo",
                table: "CustomerType",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreateById",
                schema: "dbo",
                table: "CustomerType",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LastModifyById",
                schema: "dbo",
                table: "CustomerType",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifyDate",
                schema: "dbo",
                table: "CustomerType",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FullName",
                schema: "dbo",
                table: "Customer",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                schema: "dbo",
                table: "Customer",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreateById",
                schema: "dbo",
                table: "Customer",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LastModifyById",
                schema: "dbo",
                table: "Customer",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifyDate",
                schema: "dbo",
                table: "Customer",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreateById",
                schema: "dbo",
                table: "Card",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LastModifyById",
                schema: "dbo",
                table: "Card",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifyDate",
                schema: "dbo",
                table: "Card",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_VehicleType_CreateById",
                schema: "dbo",
                table: "VehicleType",
                column: "CreateById");

            migrationBuilder.CreateIndex(
                name: "IX_VehicleType_LastModifyById",
                schema: "dbo",
                table: "VehicleType",
                column: "LastModifyById");

            migrationBuilder.CreateIndex(
                name: "IX_Vehicle_LastModifyById",
                schema: "dbo",
                table: "Vehicle",
                column: "LastModifyById");

            migrationBuilder.CreateIndex(
                name: "IX_Vehicle_StaffId",
                schema: "dbo",
                table: "Vehicle",
                column: "StaffId");

            migrationBuilder.CreateIndex(
                name: "IX_User_LastModifyById",
                schema: "dbo",
                table: "User",
                column: "LastModifyById",
                unique: true,
                filter: "[LastModifyById] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Session_CustomerId",
                schema: "dbo",
                table: "Session",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Role_CreateById",
                schema: "dbo",
                table: "Role",
                column: "CreateById");

            migrationBuilder.CreateIndex(
                name: "IX_Role_LastModifyById",
                schema: "dbo",
                table: "Role",
                column: "LastModifyById");

            migrationBuilder.CreateIndex(
                name: "IX_PriceTable_CreateById",
                schema: "dbo",
                table: "PriceTable",
                column: "CreateById");

            migrationBuilder.CreateIndex(
                name: "IX_PriceTable_LastModifyById",
                schema: "dbo",
                table: "PriceTable",
                column: "LastModifyById");

            migrationBuilder.CreateIndex(
                name: "IX_PriceItem_CreateById",
                schema: "dbo",
                table: "PriceItem",
                column: "CreateById");

            migrationBuilder.CreateIndex(
                name: "IX_PriceItem_LastModifyById",
                schema: "dbo",
                table: "PriceItem",
                column: "LastModifyById");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentMethod_CreateById",
                schema: "dbo",
                table: "PaymentMethod",
                column: "CreateById");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentMethod_LastModifyById",
                schema: "dbo",
                table: "PaymentMethod",
                column: "LastModifyById");

            migrationBuilder.CreateIndex(
                name: "IX_ParkingArea_CreateById",
                schema: "dbo",
                table: "ParkingArea",
                column: "CreateById");

            migrationBuilder.CreateIndex(
                name: "IX_ParkingArea_LastModifyById",
                schema: "dbo",
                table: "ParkingArea",
                column: "LastModifyById");

            migrationBuilder.CreateIndex(
                name: "IX_Package_CreateById",
                schema: "dbo",
                table: "Package",
                column: "CreateById");

            migrationBuilder.CreateIndex(
                name: "IX_Package_LastModifyById",
                schema: "dbo",
                table: "Package",
                column: "LastModifyById");

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

            migrationBuilder.CreateIndex(
                name: "IX_Gate_CreateById",
                schema: "dbo",
                table: "Gate",
                column: "CreateById");

            migrationBuilder.CreateIndex(
                name: "IX_Gate_LastModifyById",
                schema: "dbo",
                table: "Gate",
                column: "LastModifyById");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerType_CreateById",
                schema: "dbo",
                table: "CustomerType",
                column: "CreateById");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerType_LastModifyById",
                schema: "dbo",
                table: "CustomerType",
                column: "LastModifyById");

            migrationBuilder.CreateIndex(
                name: "IX_Customer_CreateById",
                schema: "dbo",
                table: "Customer",
                column: "CreateById");

            migrationBuilder.CreateIndex(
                name: "IX_Customer_LastModifyById",
                schema: "dbo",
                table: "Customer",
                column: "LastModifyById");

            migrationBuilder.CreateIndex(
                name: "IX_Card_CreateById",
                schema: "dbo",
                table: "Card",
                column: "CreateById");

            migrationBuilder.CreateIndex(
                name: "IX_Card_LastModifyById",
                schema: "dbo",
                table: "Card",
                column: "LastModifyById");

            migrationBuilder.AddForeignKey(
                name: "FK_Card_User_CreateById",
                schema: "dbo",
                table: "Card",
                column: "CreateById",
                principalSchema: "dbo",
                principalTable: "User",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Card_User_LastModifyById",
                schema: "dbo",
                table: "Card",
                column: "LastModifyById",
                principalSchema: "dbo",
                principalTable: "User",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Customer_User_CreateById",
                schema: "dbo",
                table: "Customer",
                column: "CreateById",
                principalSchema: "dbo",
                principalTable: "User",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Customer_User_LastModifyById",
                schema: "dbo",
                table: "Customer",
                column: "LastModifyById",
                principalSchema: "dbo",
                principalTable: "User",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CustomerType_User_CreateById",
                schema: "dbo",
                table: "CustomerType",
                column: "CreateById",
                principalSchema: "dbo",
                principalTable: "User",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CustomerType_User_LastModifyById",
                schema: "dbo",
                table: "CustomerType",
                column: "LastModifyById",
                principalSchema: "dbo",
                principalTable: "User",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Gate_User_CreateById",
                schema: "dbo",
                table: "Gate",
                column: "CreateById",
                principalSchema: "dbo",
                principalTable: "User",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Gate_User_LastModifyById",
                schema: "dbo",
                table: "Gate",
                column: "LastModifyById",
                principalSchema: "dbo",
                principalTable: "User",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_GateType_User_CreateById",
                schema: "dbo",
                table: "GateType",
                column: "CreateById",
                principalSchema: "dbo",
                principalTable: "User",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_GateType_User_LastModifyById",
                schema: "dbo",
                table: "GateType",
                column: "LastModifyById",
                principalSchema: "dbo",
                principalTable: "User",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Package_User_CreateById",
                schema: "dbo",
                table: "Package",
                column: "CreateById",
                principalSchema: "dbo",
                principalTable: "User",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Package_User_LastModifyById",
                schema: "dbo",
                table: "Package",
                column: "LastModifyById",
                principalSchema: "dbo",
                principalTable: "User",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ParkingArea_User_CreateById",
                schema: "dbo",
                table: "ParkingArea",
                column: "CreateById",
                principalSchema: "dbo",
                principalTable: "User",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ParkingArea_User_LastModifyById",
                schema: "dbo",
                table: "ParkingArea",
                column: "LastModifyById",
                principalSchema: "dbo",
                principalTable: "User",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentMethod_User_CreateById",
                schema: "dbo",
                table: "PaymentMethod",
                column: "CreateById",
                principalSchema: "dbo",
                principalTable: "User",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentMethod_User_LastModifyById",
                schema: "dbo",
                table: "PaymentMethod",
                column: "LastModifyById",
                principalSchema: "dbo",
                principalTable: "User",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PriceItem_User_CreateById",
                schema: "dbo",
                table: "PriceItem",
                column: "CreateById",
                principalSchema: "dbo",
                principalTable: "User",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PriceItem_User_LastModifyById",
                schema: "dbo",
                table: "PriceItem",
                column: "LastModifyById",
                principalSchema: "dbo",
                principalTable: "User",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PriceTable_User_CreateById",
                schema: "dbo",
                table: "PriceTable",
                column: "CreateById",
                principalSchema: "dbo",
                principalTable: "User",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PriceTable_User_LastModifyById",
                schema: "dbo",
                table: "PriceTable",
                column: "LastModifyById",
                principalSchema: "dbo",
                principalTable: "User",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Role_User_CreateById",
                schema: "dbo",
                table: "Role",
                column: "CreateById",
                principalSchema: "dbo",
                principalTable: "User",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Role_User_LastModifyById",
                schema: "dbo",
                table: "Role",
                column: "LastModifyById",
                principalSchema: "dbo",
                principalTable: "User",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Session_Customer_CustomerId",
                schema: "dbo",
                table: "Session",
                column: "CustomerId",
                principalSchema: "dbo",
                principalTable: "Customer",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_User_User_LastModifyById",
                schema: "dbo",
                table: "User",
                column: "LastModifyById",
                principalSchema: "dbo",
                principalTable: "User",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Vehicle_User_LastModifyById",
                schema: "dbo",
                table: "Vehicle",
                column: "LastModifyById",
                principalSchema: "dbo",
                principalTable: "User",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Vehicle_User_StaffId",
                schema: "dbo",
                table: "Vehicle",
                column: "StaffId",
                principalSchema: "dbo",
                principalTable: "User",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_VehicleType_User_CreateById",
                schema: "dbo",
                table: "VehicleType",
                column: "CreateById",
                principalSchema: "dbo",
                principalTable: "User",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_VehicleType_User_LastModifyById",
                schema: "dbo",
                table: "VehicleType",
                column: "LastModifyById",
                principalSchema: "dbo",
                principalTable: "User",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Card_User_CreateById",
                schema: "dbo",
                table: "Card");

            migrationBuilder.DropForeignKey(
                name: "FK_Card_User_LastModifyById",
                schema: "dbo",
                table: "Card");

            migrationBuilder.DropForeignKey(
                name: "FK_Customer_User_CreateById",
                schema: "dbo",
                table: "Customer");

            migrationBuilder.DropForeignKey(
                name: "FK_Customer_User_LastModifyById",
                schema: "dbo",
                table: "Customer");

            migrationBuilder.DropForeignKey(
                name: "FK_CustomerType_User_CreateById",
                schema: "dbo",
                table: "CustomerType");

            migrationBuilder.DropForeignKey(
                name: "FK_CustomerType_User_LastModifyById",
                schema: "dbo",
                table: "CustomerType");

            migrationBuilder.DropForeignKey(
                name: "FK_Gate_User_CreateById",
                schema: "dbo",
                table: "Gate");

            migrationBuilder.DropForeignKey(
                name: "FK_Gate_User_LastModifyById",
                schema: "dbo",
                table: "Gate");

            migrationBuilder.DropForeignKey(
                name: "FK_GateType_User_CreateById",
                schema: "dbo",
                table: "GateType");

            migrationBuilder.DropForeignKey(
                name: "FK_GateType_User_LastModifyById",
                schema: "dbo",
                table: "GateType");

            migrationBuilder.DropForeignKey(
                name: "FK_Package_User_CreateById",
                schema: "dbo",
                table: "Package");

            migrationBuilder.DropForeignKey(
                name: "FK_Package_User_LastModifyById",
                schema: "dbo",
                table: "Package");

            migrationBuilder.DropForeignKey(
                name: "FK_ParkingArea_User_CreateById",
                schema: "dbo",
                table: "ParkingArea");

            migrationBuilder.DropForeignKey(
                name: "FK_ParkingArea_User_LastModifyById",
                schema: "dbo",
                table: "ParkingArea");

            migrationBuilder.DropForeignKey(
                name: "FK_PaymentMethod_User_CreateById",
                schema: "dbo",
                table: "PaymentMethod");

            migrationBuilder.DropForeignKey(
                name: "FK_PaymentMethod_User_LastModifyById",
                schema: "dbo",
                table: "PaymentMethod");

            migrationBuilder.DropForeignKey(
                name: "FK_PriceItem_User_CreateById",
                schema: "dbo",
                table: "PriceItem");

            migrationBuilder.DropForeignKey(
                name: "FK_PriceItem_User_LastModifyById",
                schema: "dbo",
                table: "PriceItem");

            migrationBuilder.DropForeignKey(
                name: "FK_PriceTable_User_CreateById",
                schema: "dbo",
                table: "PriceTable");

            migrationBuilder.DropForeignKey(
                name: "FK_PriceTable_User_LastModifyById",
                schema: "dbo",
                table: "PriceTable");

            migrationBuilder.DropForeignKey(
                name: "FK_Role_User_CreateById",
                schema: "dbo",
                table: "Role");

            migrationBuilder.DropForeignKey(
                name: "FK_Role_User_LastModifyById",
                schema: "dbo",
                table: "Role");

            migrationBuilder.DropForeignKey(
                name: "FK_Session_Customer_CustomerId",
                schema: "dbo",
                table: "Session");

            migrationBuilder.DropForeignKey(
                name: "FK_User_User_LastModifyById",
                schema: "dbo",
                table: "User");

            migrationBuilder.DropForeignKey(
                name: "FK_Vehicle_User_LastModifyById",
                schema: "dbo",
                table: "Vehicle");

            migrationBuilder.DropForeignKey(
                name: "FK_Vehicle_User_StaffId",
                schema: "dbo",
                table: "Vehicle");

            migrationBuilder.DropForeignKey(
                name: "FK_VehicleType_User_CreateById",
                schema: "dbo",
                table: "VehicleType");

            migrationBuilder.DropForeignKey(
                name: "FK_VehicleType_User_LastModifyById",
                schema: "dbo",
                table: "VehicleType");

            migrationBuilder.DropIndex(
                name: "IX_VehicleType_CreateById",
                schema: "dbo",
                table: "VehicleType");

            migrationBuilder.DropIndex(
                name: "IX_VehicleType_LastModifyById",
                schema: "dbo",
                table: "VehicleType");

            migrationBuilder.DropIndex(
                name: "IX_Vehicle_LastModifyById",
                schema: "dbo",
                table: "Vehicle");

            migrationBuilder.DropIndex(
                name: "IX_Vehicle_StaffId",
                schema: "dbo",
                table: "Vehicle");

            migrationBuilder.DropIndex(
                name: "IX_User_LastModifyById",
                schema: "dbo",
                table: "User");

            migrationBuilder.DropIndex(
                name: "IX_Session_CustomerId",
                schema: "dbo",
                table: "Session");

            migrationBuilder.DropIndex(
                name: "IX_Role_CreateById",
                schema: "dbo",
                table: "Role");

            migrationBuilder.DropIndex(
                name: "IX_Role_LastModifyById",
                schema: "dbo",
                table: "Role");

            migrationBuilder.DropIndex(
                name: "IX_PriceTable_CreateById",
                schema: "dbo",
                table: "PriceTable");

            migrationBuilder.DropIndex(
                name: "IX_PriceTable_LastModifyById",
                schema: "dbo",
                table: "PriceTable");

            migrationBuilder.DropIndex(
                name: "IX_PriceItem_CreateById",
                schema: "dbo",
                table: "PriceItem");

            migrationBuilder.DropIndex(
                name: "IX_PriceItem_LastModifyById",
                schema: "dbo",
                table: "PriceItem");

            migrationBuilder.DropIndex(
                name: "IX_PaymentMethod_CreateById",
                schema: "dbo",
                table: "PaymentMethod");

            migrationBuilder.DropIndex(
                name: "IX_PaymentMethod_LastModifyById",
                schema: "dbo",
                table: "PaymentMethod");

            migrationBuilder.DropIndex(
                name: "IX_ParkingArea_CreateById",
                schema: "dbo",
                table: "ParkingArea");

            migrationBuilder.DropIndex(
                name: "IX_ParkingArea_LastModifyById",
                schema: "dbo",
                table: "ParkingArea");

            migrationBuilder.DropIndex(
                name: "IX_Package_CreateById",
                schema: "dbo",
                table: "Package");

            migrationBuilder.DropIndex(
                name: "IX_Package_LastModifyById",
                schema: "dbo",
                table: "Package");

            migrationBuilder.DropIndex(
                name: "IX_GateType_CreateById",
                schema: "dbo",
                table: "GateType");

            migrationBuilder.DropIndex(
                name: "IX_GateType_LastModifyById",
                schema: "dbo",
                table: "GateType");

            migrationBuilder.DropIndex(
                name: "IX_Gate_CreateById",
                schema: "dbo",
                table: "Gate");

            migrationBuilder.DropIndex(
                name: "IX_Gate_LastModifyById",
                schema: "dbo",
                table: "Gate");

            migrationBuilder.DropIndex(
                name: "IX_CustomerType_CreateById",
                schema: "dbo",
                table: "CustomerType");

            migrationBuilder.DropIndex(
                name: "IX_CustomerType_LastModifyById",
                schema: "dbo",
                table: "CustomerType");

            migrationBuilder.DropIndex(
                name: "IX_Customer_CreateById",
                schema: "dbo",
                table: "Customer");

            migrationBuilder.DropIndex(
                name: "IX_Customer_LastModifyById",
                schema: "dbo",
                table: "Customer");

            migrationBuilder.DropIndex(
                name: "IX_Card_CreateById",
                schema: "dbo",
                table: "Card");

            migrationBuilder.DropIndex(
                name: "IX_Card_LastModifyById",
                schema: "dbo",
                table: "Card");

            migrationBuilder.DropColumn(
                name: "CreateById",
                schema: "dbo",
                table: "VehicleType");

            migrationBuilder.DropColumn(
                name: "LastModifyById",
                schema: "dbo",
                table: "VehicleType");

            migrationBuilder.DropColumn(
                name: "LastModifyDate",
                schema: "dbo",
                table: "VehicleType");

            migrationBuilder.DropColumn(
                name: "LastModifyById",
                schema: "dbo",
                table: "Vehicle");

            migrationBuilder.DropColumn(
                name: "LastModifyDate",
                schema: "dbo",
                table: "Vehicle");

            migrationBuilder.DropColumn(
                name: "StaffId",
                schema: "dbo",
                table: "Vehicle");

            migrationBuilder.DropColumn(
                name: "CreateById",
                schema: "dbo",
                table: "User");

            migrationBuilder.DropColumn(
                name: "LastModifyById",
                schema: "dbo",
                table: "User");

            migrationBuilder.DropColumn(
                name: "LastModifyDate",
                schema: "dbo",
                table: "User");

            migrationBuilder.DropColumn(
                name: "CustomerId",
                schema: "dbo",
                table: "Session");

            migrationBuilder.DropColumn(
                name: "CreateById",
                schema: "dbo",
                table: "Role");

            migrationBuilder.DropColumn(
                name: "LastModifyById",
                schema: "dbo",
                table: "Role");

            migrationBuilder.DropColumn(
                name: "LastModifyDate",
                schema: "dbo",
                table: "Role");

            migrationBuilder.DropColumn(
                name: "CreateById",
                schema: "dbo",
                table: "PriceTable");

            migrationBuilder.DropColumn(
                name: "LastModifyById",
                schema: "dbo",
                table: "PriceTable");

            migrationBuilder.DropColumn(
                name: "LastModifyDate",
                schema: "dbo",
                table: "PriceTable");

            migrationBuilder.DropColumn(
                name: "CreateById",
                schema: "dbo",
                table: "PriceItem");

            migrationBuilder.DropColumn(
                name: "LastModifyById",
                schema: "dbo",
                table: "PriceItem");

            migrationBuilder.DropColumn(
                name: "LastModifyDate",
                schema: "dbo",
                table: "PriceItem");

            migrationBuilder.DropColumn(
                name: "CreateById",
                schema: "dbo",
                table: "PaymentMethod");

            migrationBuilder.DropColumn(
                name: "LastModifyById",
                schema: "dbo",
                table: "PaymentMethod");

            migrationBuilder.DropColumn(
                name: "LastModifyDate",
                schema: "dbo",
                table: "PaymentMethod");

            migrationBuilder.DropColumn(
                name: "CreateById",
                schema: "dbo",
                table: "ParkingArea");

            migrationBuilder.DropColumn(
                name: "LastModifyById",
                schema: "dbo",
                table: "ParkingArea");

            migrationBuilder.DropColumn(
                name: "LastModifyDate",
                schema: "dbo",
                table: "ParkingArea");

            migrationBuilder.DropColumn(
                name: "CreateById",
                schema: "dbo",
                table: "Package");

            migrationBuilder.DropColumn(
                name: "LastModifyById",
                schema: "dbo",
                table: "Package");

            migrationBuilder.DropColumn(
                name: "LastModifyDate",
                schema: "dbo",
                table: "Package");

            migrationBuilder.DropColumn(
                name: "CreateById",
                schema: "dbo",
                table: "GateType");

            migrationBuilder.DropColumn(
                name: "LastModifyById",
                schema: "dbo",
                table: "GateType");

            migrationBuilder.DropColumn(
                name: "LastModifyDate",
                schema: "dbo",
                table: "GateType");

            migrationBuilder.DropColumn(
                name: "CreateById",
                schema: "dbo",
                table: "Gate");

            migrationBuilder.DropColumn(
                name: "LastModifyById",
                schema: "dbo",
                table: "Gate");

            migrationBuilder.DropColumn(
                name: "LastModifyDate",
                schema: "dbo",
                table: "Gate");

            migrationBuilder.DropColumn(
                name: "CreateById",
                schema: "dbo",
                table: "CustomerType");

            migrationBuilder.DropColumn(
                name: "LastModifyById",
                schema: "dbo",
                table: "CustomerType");

            migrationBuilder.DropColumn(
                name: "LastModifyDate",
                schema: "dbo",
                table: "CustomerType");

            migrationBuilder.DropColumn(
                name: "CreateById",
                schema: "dbo",
                table: "Customer");

            migrationBuilder.DropColumn(
                name: "LastModifyById",
                schema: "dbo",
                table: "Customer");

            migrationBuilder.DropColumn(
                name: "LastModifyDate",
                schema: "dbo",
                table: "Customer");

            migrationBuilder.DropColumn(
                name: "CreateById",
                schema: "dbo",
                table: "Card");

            migrationBuilder.DropColumn(
                name: "LastModifyById",
                schema: "dbo",
                table: "Card");

            migrationBuilder.DropColumn(
                name: "LastModifyDate",
                schema: "dbo",
                table: "Card");

            migrationBuilder.AddColumn<string>(
                name: "WalletStatus",
                schema: "dbo",
                table: "Wallet",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "StatusVehicle",
                schema: "dbo",
                table: "Vehicle",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "PlateNumber",
                schema: "dbo",
                table: "Vehicle",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "PlateImage",
                schema: "dbo",
                table: "Vehicle",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

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

            migrationBuilder.AlterColumn<string>(
                name: "PasswordSalt",
                schema: "dbo",
                table: "User",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "PasswordHash",
                schema: "dbo",
                table: "User",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "FullName",
                schema: "dbo",
                table: "User",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                schema: "dbo",
                table: "User",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "TransactionStatus",
                schema: "dbo",
                table: "Transaction",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "TransactionDescription",
                schema: "dbo",
                table: "Transaction",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "ImageOutUrl",
                schema: "dbo",
                table: "Session",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "ImageInUrl",
                schema: "dbo",
                table: "Session",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "StatusPriceTable",
                schema: "dbo",
                table: "PriceTable",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<int>(
                name: "MinPrice",
                schema: "dbo",
                table: "PriceItem",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "MaxPrice",
                schema: "dbo",
                table: "PriceItem",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                schema: "dbo",
                table: "PaymentMethod",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "StatusPayment",
                schema: "dbo",
                table: "Payment",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "StatusParkingArea",
                schema: "dbo",
                table: "ParkingArea",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Mode",
                schema: "dbo",
                table: "ParkingArea",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                schema: "dbo",
                table: "GateType",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Descriptipn",
                schema: "dbo",
                table: "GateType",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "StatusGate",
                schema: "dbo",
                table: "Gate",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<Guid>(
                name: "WPFCode",
                schema: "dbo",
                table: "Gate",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                schema: "dbo",
                table: "Feedback",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                schema: "dbo",
                table: "CustomerType",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                schema: "dbo",
                table: "CustomerType",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "FullName",
                schema: "dbo",
                table: "Customer",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                schema: "dbo",
                table: "Customer",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }
    }
}
