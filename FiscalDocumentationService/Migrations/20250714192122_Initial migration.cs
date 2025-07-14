using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FiscalDocumentationService.Migrations
{
    /// <inheritdoc />
    public partial class Initialmigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FiscalDocuments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DocumentType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DocumentLetter = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PointOfSale = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DocumentNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CAE = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CAEExpiration = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SaleId = table.Column<int>(type: "int", nullable: false),
                    CompanyName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CompanyTaxId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CompanyAddress = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CustomerName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CustomerTaxId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CustomerAddress = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CustomerIVAType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CustomerSellCondition = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Subtotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IVATotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FiscalDocuments", x => x.Id);
                });

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
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DiscountPercent = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IVAPercent = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FiscalDocumentArticles");

            migrationBuilder.DropTable(
                name: "FiscalDocuments");
        }
    }
}
