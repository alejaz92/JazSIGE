using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FiscalDocumentationService.Migrations
{
    /// <inheritdoc />
    public partial class seagregocampodeivaconditiontypeenfiscaldocument : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ReceiverVatConditionId",
                table: "FiscalDocuments",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReceiverVatConditionId",
                table: "FiscalDocuments");
        }
    }
}
