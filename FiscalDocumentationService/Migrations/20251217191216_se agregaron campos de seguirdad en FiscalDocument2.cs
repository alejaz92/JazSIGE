using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FiscalDocumentationService.Migrations
{
    /// <inheritdoc />
    public partial class seagregaroncamposdeseguirdadenFiscalDocument2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ArcaPointOfSaleUsed",
                table: "FiscalDocuments");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ArcaPointOfSaleUsed",
                table: "FiscalDocuments",
                type: "int",
                nullable: true);
        }
    }
}
