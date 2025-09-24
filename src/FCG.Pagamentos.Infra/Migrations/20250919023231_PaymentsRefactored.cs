using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FCG.Pagamentos.Infra.Migrations
{
    /// <inheritdoc />
    public partial class PaymentsRefactored : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PAYMENTS_JOGOID",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "JogoId",
                table: "Payments");

            migrationBuilder.RenameColumn(
                name: "ItemId",
                table: "PaymentItens",
                newName: "JogoId");

            migrationBuilder.RenameIndex(
                name: "IX_PaymentItems_ItemId",
                table: "PaymentItens",
                newName: "IX_PaymentItems_JogoId");

            migrationBuilder.CreateIndex(
                name: "IX_PAYMENTS_USERID",
                table: "Payments",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PAYMENTS_USERID",
                table: "Payments");

            migrationBuilder.RenameColumn(
                name: "JogoId",
                table: "PaymentItens",
                newName: "ItemId");

            migrationBuilder.RenameIndex(
                name: "IX_PaymentItems_JogoId",
                table: "PaymentItens",
                newName: "IX_PaymentItems_ItemId");

            migrationBuilder.AddColumn<Guid>(
                name: "JogoId",
                table: "Payments",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_PAYMENTS_JOGOID",
                table: "Payments",
                column: "JogoId");
        }
    }
}
