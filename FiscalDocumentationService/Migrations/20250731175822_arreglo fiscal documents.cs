using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FiscalDocumentationService.Migrations
{
    /// <inheritdoc />
    public partial class arreglofiscaldocuments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CustomerCUIT",
                table: "FiscalDocuments");

            migrationBuilder.DropColumn(
                name: "CustomerIVAType",
                table: "FiscalDocuments");

            migrationBuilder.DropColumn(
                name: "CustomerName",
                table: "FiscalDocuments");

            migrationBuilder.RenameColumn(
                name: "VAT",
                table: "FiscalDocumentItems",
                newName: "VATBase");

            migrationBuilder.AddColumn<long>(
                name: "BuyerDocumentNumber",
                table: "FiscalDocuments",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<int>(
                name: "BuyerDocumentType",
                table: "FiscalDocuments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "ExemptAmount",
                table: "FiscalDocuments",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<long>(
                name: "InvoiceFrom",
                table: "FiscalDocuments",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "InvoiceTo",
                table: "FiscalDocuments",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<int>(
                name: "InvoiceType",
                table: "FiscalDocuments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "NonTaxableAmount",
                table: "FiscalDocuments",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "OtherTaxesAmount",
                table: "FiscalDocuments",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "PointOfSale",
                table: "FiscalDocuments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "VATAmount",
                table: "FiscalDocumentItems",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "VATId",
                table: "FiscalDocumentItems",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BuyerDocumentNumber",
                table: "FiscalDocuments");

            migrationBuilder.DropColumn(
                name: "BuyerDocumentType",
                table: "FiscalDocuments");

            migrationBuilder.DropColumn(
                name: "ExemptAmount",
                table: "FiscalDocuments");

            migrationBuilder.DropColumn(
                name: "InvoiceFrom",
                table: "FiscalDocuments");

            migrationBuilder.DropColumn(
                name: "InvoiceTo",
                table: "FiscalDocuments");

            migrationBuilder.DropColumn(
                name: "InvoiceType",
                table: "FiscalDocuments");

            migrationBuilder.DropColumn(
                name: "NonTaxableAmount",
                table: "FiscalDocuments");

            migrationBuilder.DropColumn(
                name: "OtherTaxesAmount",
                table: "FiscalDocuments");

            migrationBuilder.DropColumn(
                name: "PointOfSale",
                table: "FiscalDocuments");

            migrationBuilder.DropColumn(
                name: "VATAmount",
                table: "FiscalDocumentItems");

            migrationBuilder.DropColumn(
                name: "VATId",
                table: "FiscalDocumentItems");

            migrationBuilder.RenameColumn(
                name: "VATBase",
                table: "FiscalDocumentItems",
                newName: "VAT");

            migrationBuilder.AddColumn<string>(
                name: "CustomerCUIT",
                table: "FiscalDocuments",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CustomerIVAType",
                table: "FiscalDocuments",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CustomerName",
                table: "FiscalDocuments",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
