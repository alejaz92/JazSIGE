using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalesService.Migrations
{
    /// <inheritdoc />
    public partial class creationofsalesQuoteobjects : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SalesQuotes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    SellerId = table.Column<int>(type: "int", nullable: false),
                    TransportId = table.Column<int>(type: "int", nullable: true),
                    PriceListId = table.Column<int>(type: "int", nullable: false),
                    ExchangeRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SubtotalUSD = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    IVAAmountUSD = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    TotalUSD = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Observations = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesQuotes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SalesQuoteArticles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SalesQuoteId = table.Column<int>(type: "int", nullable: false),
                    ArticleId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UnitPriceUSD = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    DiscountPercent = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IVA = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalUSD = table.Column<decimal>(type: "decimal(18,4)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesQuoteArticles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SalesQuoteArticles_SalesQuotes_SalesQuoteId",
                        column: x => x.SalesQuoteId,
                        principalTable: "SalesQuotes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SalesQuoteArticles_SalesQuoteId",
                table: "SalesQuoteArticles",
                column: "SalesQuoteId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SalesQuoteArticles");

            migrationBuilder.DropTable(
                name: "SalesQuotes");
        }
    }
}
