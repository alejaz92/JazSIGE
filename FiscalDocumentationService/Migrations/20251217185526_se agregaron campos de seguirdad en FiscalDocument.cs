using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FiscalDocumentationService.Migrations
{
    /// <inheritdoc />
    public partial class seagregaroncamposdeseguirdadenFiscalDocument : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ArcaEnvironment",
                table: "FiscalDocuments",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "ArcaPointOfSaleUsed",
                table: "FiscalDocuments",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmissionProvider",
                table: "FiscalDocuments",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ArcaEnvironment",
                table: "FiscalDocuments");

            migrationBuilder.DropColumn(
                name: "ArcaPointOfSaleUsed",
                table: "FiscalDocuments");

            migrationBuilder.DropColumn(
                name: "EmissionProvider",
                table: "FiscalDocuments");
        }
    }
}
