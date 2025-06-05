using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalesService.Migrations
{
    /// <inheritdoc />
    public partial class seagregoSalesOrder_ArticleDispatch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "salesOrder_ArticleDispatches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SalesOrderArticleId = table.Column<int>(type: "int", nullable: false),
                    DispatchId = table.Column<int>(type: "int", nullable: true),
                    Quantity = table.Column<decimal>(type: "decimal(18,4)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_salesOrder_ArticleDispatches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_salesOrder_ArticleDispatches_SalesOrder_Articles_SalesOrderArticleId",
                        column: x => x.SalesOrderArticleId,
                        principalTable: "SalesOrder_Articles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_salesOrder_ArticleDispatches_SalesOrderArticleId",
                table: "salesOrder_ArticleDispatches",
                column: "SalesOrderArticleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "salesOrder_ArticleDispatches");
        }
    }
}
