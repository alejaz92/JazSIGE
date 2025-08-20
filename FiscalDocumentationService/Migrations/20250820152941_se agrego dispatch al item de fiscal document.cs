using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FiscalDocumentationService.Migrations
{
    /// <inheritdoc />
    public partial class seagregodispatchalitemdefiscaldocument : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DispatchCode",
                table: "FiscalDocumentItems",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DispatchCode",
                table: "FiscalDocumentItems");
        }
    }
}
