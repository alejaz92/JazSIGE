using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FiscalDocumentationService.Migrations
{
    /// <inheritdoc />
    public partial class migracioninicialfiscaldocumentation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FiscalDocumentArticles");

            migrationBuilder.DropColumn(
                name: "CompanyAddress",
                table: "FiscalDocuments");

            migrationBuilder.DropColumn(
                name: "CompanyName",
                table: "FiscalDocuments");

            migrationBuilder.DropColumn(
                name: "CompanyTaxId",
                table: "FiscalDocuments");

            migrationBuilder.DropColumn(
                name: "CustomerAddress",
                table: "FiscalDocuments");

            migrationBuilder.DropColumn(
                name: "CustomerSellCondition",
                table: "FiscalDocuments");

            migrationBuilder.DropColumn(
                name: "CustomerTaxId",
                table: "FiscalDocuments");

            migrationBuilder.DropColumn(
                name: "DocumentLetter",
                table: "FiscalDocuments");

            migrationBuilder.DropColumn(
                name: "DocumentType",
                table: "FiscalDocuments");

            migrationBuilder.RenameColumn(
                name: "Total",
                table: "FiscalDocuments",
                newName: "VATAmount");

            migrationBuilder.RenameColumn(
                name: "Subtotal",
                table: "FiscalDocuments",
                newName: "TotalAmount");

            migrationBuilder.RenameColumn(
                name: "SaleId",
                table: "FiscalDocuments",
                newName: "Type");

            migrationBuilder.RenameColumn(
                name: "PointOfSale",
                table: "FiscalDocuments",
                newName: "CustomerCUIT");

            migrationBuilder.RenameColumn(
                name: "IVATotal",
                table: "FiscalDocuments",
                newName: "NetAmount");

            migrationBuilder.AddColumn<int>(
                name: "SalesOrderId",
                table: "FiscalDocuments",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "FiscalDocumentItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    VAT = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FiscalDocumentId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FiscalDocumentItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FiscalDocumentItems_FiscalDocuments_FiscalDocumentId",
                        column: x => x.FiscalDocumentId,
                        principalTable: "FiscalDocuments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FiscalDocumentItems_FiscalDocumentId",
                table: "FiscalDocumentItems",
                column: "FiscalDocumentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FiscalDocumentItems");

            migrationBuilder.DropColumn(
                name: "SalesOrderId",
                table: "FiscalDocuments");

            migrationBuilder.RenameColumn(
                name: "VATAmount",
                table: "FiscalDocuments",
                newName: "Total");

            migrationBuilder.RenameColumn(
                name: "Type",
                table: "FiscalDocuments",
                newName: "SaleId");

            migrationBuilder.RenameColumn(
                name: "TotalAmount",
                table: "FiscalDocuments",
                newName: "Subtotal");

            migrationBuilder.RenameColumn(
                name: "NetAmount",
                table: "FiscalDocuments",
                newName: "IVATotal");

            migrationBuilder.RenameColumn(
                name: "CustomerCUIT",
                table: "FiscalDocuments",
                newName: "PointOfSale");

            migrationBuilder.AddColumn<string>(
                name: "CompanyAddress",
                table: "FiscalDocuments",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CompanyName",
                table: "FiscalDocuments",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CompanyTaxId",
                table: "FiscalDocuments",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CustomerAddress",
                table: "FiscalDocuments",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CustomerSellCondition",
                table: "FiscalDocuments",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CustomerTaxId",
                table: "FiscalDocuments",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DocumentLetter",
                table: "FiscalDocuments",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DocumentType",
                table: "FiscalDocuments",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "FiscalDocumentArticles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FiscalDocumentId = table.Column<int>(type: "int", nullable: false),
                    ArticleId = table.Column<int>(type: "int", nullable: false),
                    ArticleName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ArticleSKU = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DiscountPercent = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IVAPercent = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FiscalDocumentArticles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FiscalDocumentArticles_FiscalDocuments_FiscalDocumentId",
                        column: x => x.FiscalDocumentId,
                        principalTable: "FiscalDocuments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FiscalDocumentArticles_FiscalDocumentId",
                table: "FiscalDocumentArticles",
                column: "FiscalDocumentId");
        }
    }
}
