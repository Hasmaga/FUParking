using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FUParkingModel.Migrations
{
    /// <inheritdoc />
    public partial class V10 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "dbo");

            migrationBuilder.CreateTable(
                name: "Card",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PlateNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CardNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreateById = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModifyById = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModifyDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Card", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Customer",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CustomerTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StatusCustomer = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreateById = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModifyById = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModifyDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customer", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Wallet",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Balance = table.Column<int>(type: "int", nullable: false),
                    WalletType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EXPDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Wallet", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Wallet_Customer_CustomerId",
                        column: x => x.CustomerId,
                        principalSchema: "dbo",
                        principalTable: "Customer",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CustomerType",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreateById = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModifyById = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModifyDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Deposit",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PaymentMethodId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PackageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Amount = table.Column<int>(type: "int", nullable: false),
                    AppTranId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Deposit", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Deposit_Customer_CustomerId",
                        column: x => x.CustomerId,
                        principalSchema: "dbo",
                        principalTable: "Customer",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Feedback",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ParkingAreaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Feedback", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Feedback_Customer_CustomerId",
                        column: x => x.CustomerId,
                        principalSchema: "dbo",
                        principalTable: "Customer",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Gate",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ParkingAreaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GateTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StatusGate = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreateById = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModifyById = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModifyDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Gate", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Session",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CardId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GateInId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GateOutId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PlateNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ImageInUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ImageOutUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TimeIn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TimeOut = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Mode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Session", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Session_Card_CardId",
                        column: x => x.CardId,
                        principalSchema: "dbo",
                        principalTable: "Card",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Session_Customer_CustomerId",
                        column: x => x.CustomerId,
                        principalSchema: "dbo",
                        principalTable: "Customer",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Session_Gate_GateInId",
                        column: x => x.GateInId,
                        principalSchema: "dbo",
                        principalTable: "Gate",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Session_Gate_GateOutId",
                        column: x => x.GateOutId,
                        principalSchema: "dbo",
                        principalTable: "Gate",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "GateType",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Descriptipn = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreateById = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModifyById = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModifyDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GateType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Package",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CoinAmount = table.Column<int>(type: "int", nullable: false),
                    ExtraCoin = table.Column<int>(type: "int", nullable: true),
                    EXPPackage = table.Column<int>(type: "int", nullable: true),
                    Price = table.Column<int>(type: "int", nullable: false),
                    PackageStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreateById = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModifyById = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModifyDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Package", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ParkingArea",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MaxCapacity = table.Column<int>(type: "int", nullable: false),
                    Block = table.Column<int>(type: "int", nullable: false),
                    Mode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StatusParkingArea = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreateById = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModifyById = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModifyDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParkingArea", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Payment",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PaymentMethodId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TotalPrice = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Payment_Session_SessionId",
                        column: x => x.SessionId,
                        principalSchema: "dbo",
                        principalTable: "Session",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Transaction",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WalletId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PaymentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DepositId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Amount = table.Column<int>(type: "int", nullable: false),
                    TransactionDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TransactionStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transaction", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Transaction_Deposit_DepositId",
                        column: x => x.DepositId,
                        principalSchema: "dbo",
                        principalTable: "Deposit",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Transaction_Payment_PaymentId",
                        column: x => x.PaymentId,
                        principalSchema: "dbo",
                        principalTable: "Payment",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Transaction_Wallet_WalletId",
                        column: x => x.WalletId,
                        principalSchema: "dbo",
                        principalTable: "Wallet",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PaymentMethod",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreateById = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModifyById = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModifyDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentMethod", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PriceItem",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PriceTableId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApplyFromHour = table.Column<TimeOnly>(type: "time", nullable: false),
                    ApplyToHour = table.Column<TimeOnly>(type: "time", nullable: false),
                    MaxPrice = table.Column<int>(type: "int", nullable: true),
                    MinPrice = table.Column<int>(type: "int", nullable: true),
                    CreateById = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModifyById = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModifyDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PriceItem", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PriceTable",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VehicleTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ApplyFromDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ApplyToDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StatusPriceTable = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreateById = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModifyById = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModifyDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PriceTable", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Role",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreateById = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModifyById = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModifyDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Role", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "User",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PasswordSalt = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StatusUser = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreateById = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModifyById = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModifyDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.Id);
                    table.ForeignKey(
                        name: "FK_User_Role_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "dbo",
                        principalTable: "Role",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_User_User_LastModifyById",
                        column: x => x.LastModifyById,
                        principalSchema: "dbo",
                        principalTable: "User",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "VehicleType",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreateById = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModifyById = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModifyDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VehicleType", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VehicleType_User_CreateById",
                        column: x => x.CreateById,
                        principalSchema: "dbo",
                        principalTable: "User",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_VehicleType_User_LastModifyById",
                        column: x => x.LastModifyById,
                        principalSchema: "dbo",
                        principalTable: "User",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Vehicle",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PlateNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VehicleTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PlateImage = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StatusVehicle = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StaffId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModifyById = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModifyDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vehicle", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Vehicle_Customer_CustomerId",
                        column: x => x.CustomerId,
                        principalSchema: "dbo",
                        principalTable: "Customer",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Vehicle_User_LastModifyById",
                        column: x => x.LastModifyById,
                        principalSchema: "dbo",
                        principalTable: "User",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Vehicle_User_StaffId",
                        column: x => x.StaffId,
                        principalSchema: "dbo",
                        principalTable: "User",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Vehicle_VehicleType_VehicleTypeId",
                        column: x => x.VehicleTypeId,
                        principalSchema: "dbo",
                        principalTable: "VehicleType",
                        principalColumn: "Id");
                });

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

            migrationBuilder.CreateIndex(
                name: "IX_Customer_CreateById",
                schema: "dbo",
                table: "Customer",
                column: "CreateById");

            migrationBuilder.CreateIndex(
                name: "IX_Customer_CustomerTypeId",
                schema: "dbo",
                table: "Customer",
                column: "CustomerTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Customer_LastModifyById",
                schema: "dbo",
                table: "Customer",
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
                name: "IX_Deposit_CustomerId",
                schema: "dbo",
                table: "Deposit",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Deposit_PackageId",
                schema: "dbo",
                table: "Deposit",
                column: "PackageId");

            migrationBuilder.CreateIndex(
                name: "IX_Deposit_PaymentMethodId",
                schema: "dbo",
                table: "Deposit",
                column: "PaymentMethodId");

            migrationBuilder.CreateIndex(
                name: "IX_Feedback_CustomerId",
                schema: "dbo",
                table: "Feedback",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Feedback_ParkingAreaId",
                schema: "dbo",
                table: "Feedback",
                column: "ParkingAreaId");

            migrationBuilder.CreateIndex(
                name: "IX_Feedback_SessionId",
                schema: "dbo",
                table: "Feedback",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_Gate_CreateById",
                schema: "dbo",
                table: "Gate",
                column: "CreateById");

            migrationBuilder.CreateIndex(
                name: "IX_Gate_GateTypeId",
                schema: "dbo",
                table: "Gate",
                column: "GateTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Gate_LastModifyById",
                schema: "dbo",
                table: "Gate",
                column: "LastModifyById");

            migrationBuilder.CreateIndex(
                name: "IX_Gate_ParkingAreaId",
                schema: "dbo",
                table: "Gate",
                column: "ParkingAreaId");

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
                name: "IX_Payment_PaymentMethodId",
                schema: "dbo",
                table: "Payment",
                column: "PaymentMethodId");

            migrationBuilder.CreateIndex(
                name: "IX_Payment_SessionId",
                schema: "dbo",
                table: "Payment",
                column: "SessionId");

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
                name: "IX_PriceItem_PriceTableId",
                schema: "dbo",
                table: "PriceItem",
                column: "PriceTableId");

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
                name: "IX_PriceTable_VehicleTypeId",
                schema: "dbo",
                table: "PriceTable",
                column: "VehicleTypeId");

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
                name: "IX_Session_CardId",
                schema: "dbo",
                table: "Session",
                column: "CardId");

            migrationBuilder.CreateIndex(
                name: "IX_Session_CustomerId",
                schema: "dbo",
                table: "Session",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Session_GateInId",
                schema: "dbo",
                table: "Session",
                column: "GateInId");

            migrationBuilder.CreateIndex(
                name: "IX_Session_GateOutId",
                schema: "dbo",
                table: "Session",
                column: "GateOutId");

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_DepositId",
                schema: "dbo",
                table: "Transaction",
                column: "DepositId");

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_PaymentId",
                schema: "dbo",
                table: "Transaction",
                column: "PaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_WalletId",
                schema: "dbo",
                table: "Transaction",
                column: "WalletId");

            migrationBuilder.CreateIndex(
                name: "IX_User_LastModifyById",
                schema: "dbo",
                table: "User",
                column: "LastModifyById",
                unique: true,
                filter: "[LastModifyById] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_User_RoleId",
                schema: "dbo",
                table: "User",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Vehicle_CustomerId",
                schema: "dbo",
                table: "Vehicle",
                column: "CustomerId");

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
                name: "IX_Vehicle_VehicleTypeId",
                schema: "dbo",
                table: "Vehicle",
                column: "VehicleTypeId");

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
                name: "IX_Wallet_CustomerId",
                schema: "dbo",
                table: "Wallet",
                column: "CustomerId");

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
                name: "FK_Customer_CustomerType_CustomerTypeId",
                schema: "dbo",
                table: "Customer",
                column: "CustomerTypeId",
                principalSchema: "dbo",
                principalTable: "CustomerType",
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
                name: "FK_Deposit_Package_PackageId",
                schema: "dbo",
                table: "Deposit",
                column: "PackageId",
                principalSchema: "dbo",
                principalTable: "Package",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Deposit_PaymentMethod_PaymentMethodId",
                schema: "dbo",
                table: "Deposit",
                column: "PaymentMethodId",
                principalSchema: "dbo",
                principalTable: "PaymentMethod",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Feedback_ParkingArea_ParkingAreaId",
                schema: "dbo",
                table: "Feedback",
                column: "ParkingAreaId",
                principalSchema: "dbo",
                principalTable: "ParkingArea",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Feedback_Session_SessionId",
                schema: "dbo",
                table: "Feedback",
                column: "SessionId",
                principalSchema: "dbo",
                principalTable: "Session",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Gate_GateType_GateTypeId",
                schema: "dbo",
                table: "Gate",
                column: "GateTypeId",
                principalSchema: "dbo",
                principalTable: "GateType",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Gate_ParkingArea_ParkingAreaId",
                schema: "dbo",
                table: "Gate",
                column: "ParkingAreaId",
                principalSchema: "dbo",
                principalTable: "ParkingArea",
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
                name: "FK_Payment_PaymentMethod_PaymentMethodId",
                schema: "dbo",
                table: "Payment",
                column: "PaymentMethodId",
                principalSchema: "dbo",
                principalTable: "PaymentMethod",
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
                name: "FK_PriceItem_PriceTable_PriceTableId",
                schema: "dbo",
                table: "PriceItem",
                column: "PriceTableId",
                principalSchema: "dbo",
                principalTable: "PriceTable",
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
                name: "FK_PriceTable_VehicleType_VehicleTypeId",
                schema: "dbo",
                table: "PriceTable",
                column: "VehicleTypeId",
                principalSchema: "dbo",
                principalTable: "VehicleType",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Role_User_CreateById",
                schema: "dbo",
                table: "Role");

            migrationBuilder.DropForeignKey(
                name: "FK_Role_User_LastModifyById",
                schema: "dbo",
                table: "Role");

            migrationBuilder.DropTable(
                name: "Feedback",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "PriceItem",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Transaction",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Vehicle",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "PriceTable",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Deposit",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Payment",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Wallet",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "VehicleType",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Package",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "PaymentMethod",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Session",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Card",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Customer",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Gate",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "CustomerType",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "GateType",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "ParkingArea",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "User",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Role",
                schema: "dbo");
        }
    }
}
