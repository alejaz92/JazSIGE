using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FiscalDocumentationService.Migrations
{
    /// <inheritdoc />
    public partial class AddReferencedInvoiceFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "ReferencedInvoiceNumber",
                table: "FiscalDocuments",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReferencedInvoiceType",
                table: "FiscalDocuments",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReferencedPointOfSale",
                table: "FiscalDocuments",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReferencedInvoiceNumber",
                table: "FiscalDocuments");

            migrationBuilder.DropColumn(
                name: "ReferencedInvoiceType",
                table: "FiscalDocuments");

            migrationBuilder.DropColumn(
                name: "ReferencedPointOfSale",
                table: "FiscalDocuments");
        }
    }
}
