using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AccountingService.Migrations
{
    /// <inheritdoc />
    public partial class seagregarondatosdelchequeenlospaymentlines : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "CheckIsThirdParty",
                schema: "ledger",
                table: "ReceiptPayments",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CheckIssueDate",
                schema: "ledger",
                table: "ReceiptPayments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CheckIssuerBankCode",
                schema: "ledger",
                table: "ReceiptPayments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CheckIssuerName",
                schema: "ledger",
                table: "ReceiptPayments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CheckIssuerTaxId",
                schema: "ledger",
                table: "ReceiptPayments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CheckNumber",
                schema: "ledger",
                table: "ReceiptPayments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CheckPaymentDate",
                schema: "ledger",
                table: "ReceiptPayments",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CheckIsThirdParty",
                schema: "ledger",
                table: "ReceiptPayments");

            migrationBuilder.DropColumn(
                name: "CheckIssueDate",
                schema: "ledger",
                table: "ReceiptPayments");

            migrationBuilder.DropColumn(
                name: "CheckIssuerBankCode",
                schema: "ledger",
                table: "ReceiptPayments");

            migrationBuilder.DropColumn(
                name: "CheckIssuerName",
                schema: "ledger",
                table: "ReceiptPayments");

            migrationBuilder.DropColumn(
                name: "CheckIssuerTaxId",
                schema: "ledger",
                table: "ReceiptPayments");

            migrationBuilder.DropColumn(
                name: "CheckNumber",
                schema: "ledger",
                table: "ReceiptPayments");

            migrationBuilder.DropColumn(
                name: "CheckPaymentDate",
                schema: "ledger",
                table: "ReceiptPayments");
        }
    }
}
