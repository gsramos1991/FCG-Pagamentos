using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FCG.Pagamentos.Infra.Migrations
{
    /// <inheritdoc />
    public partial class PaymentsTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    PaymentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    JogoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Currency = table.Column<string>(type: "VARCHAR(3)", nullable: false),
                    StatusPayment = table.Column<string>(type: "VARCHAR(20)", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "DATETIME", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.PaymentId);
                });

            migrationBuilder.CreateTable(
                name: "PaymentEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PaymentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EventType = table.Column<string>(type: "VARCHAR(100)", maxLength: 100, nullable: false),
                    PayLoad = table.Column<string>(type: "NVARCHAR(MAX)", nullable: false),
                    Version = table.Column<int>(type: "INT", nullable: false),
                    EventDate = table.Column<DateTime>(type: "DATETIME", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentEvents_Payments_PaymentId",
                        column: x => x.PaymentId,
                        principalTable: "Payments",
                        principalColumn: "PaymentId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PaymentItens",
                columns: table => new
                {
                    PaymentItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PaymentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentItens", x => x.PaymentItemId);
                    table.ForeignKey(
                        name: "FK_PaymentItens_Payments_PaymentId",
                        column: x => x.PaymentId,
                        principalTable: "Payments",
                        principalColumn: "PaymentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "UX_PaymentEvents_PaymentId_Version",
                table: "PaymentEvents",
                columns: new[] { "PaymentId", "Version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentItems_ItemId",
                table: "PaymentItens",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentItems_PaymentId",
                table: "PaymentItens",
                column: "PaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_PAYMENTS_JOGOID",
                table: "Payments",
                column: "JogoId");

            migrationBuilder.CreateIndex(
                name: "IX_PAYMENTS_STATUSPAYMENT",
                table: "Payments",
                column: "StatusPayment");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PaymentEvents");

            migrationBuilder.DropTable(
                name: "PaymentItens");

            migrationBuilder.DropTable(
                name: "Payments");
        }
    }
}
