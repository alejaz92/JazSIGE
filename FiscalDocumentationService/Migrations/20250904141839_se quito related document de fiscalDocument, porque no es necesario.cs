using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FiscalDocumentationService.Migrations
{
    /// <inheritdoc />
    public partial class sequitorelateddocumentdefiscalDocumentporquenoesnecesario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RelatedFiscalDocumentId",
                table: "FiscalDocuments");

            migrationBuilder.AlterColumn<int>(
                name: "SalesOrderId",
                table: "FiscalDocuments",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "SalesOrderId",
                table: "FiscalDocuments",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "RelatedFiscalDocumentId",
                table: "FiscalDocuments",
                type: "int",
                nullable: true);
        }
    }
}
