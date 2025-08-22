using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FiscalDocumentationService.Migrations
{
    /// <inheritdoc />
    public partial class seagregaroncamposparaarmarelqrdeafip : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "FiscalDocuments",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "ExchangeRate",
                table: "FiscalDocuments",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "IssuerTaxId",
                table: "FiscalDocuments",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Currency",
                table: "FiscalDocuments");

            migrationBuilder.DropColumn(
                name: "ExchangeRate",
                table: "FiscalDocuments");

            migrationBuilder.DropColumn(
                name: "IssuerTaxId",
                table: "FiscalDocuments");
        }
    }
}
