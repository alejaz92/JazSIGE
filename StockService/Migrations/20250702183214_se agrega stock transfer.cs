using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StockService.Migrations
{
    /// <inheritdoc />
    public partial class seagregastocktransfer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "StockTransferId",
                table: "StockMovements",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "StockTransfers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OriginWarehouseId = table.Column<int>(type: "int", nullable: false),
                    DestinationWarehouseId = table.Column<int>(type: "int", nullable: false),
                    TransportId = table.Column<int>(type: "int", nullable: true),
                    NumberOfPackages = table.Column<int>(type: "int", nullable: false),
                    DeclaredValue = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Observations = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockTransfers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StockTransfer_Articles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StockTransferId = table.Column<int>(type: "int", nullable: false),
                    ArticleId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockTransfer_Articles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StockTransfer_Articles_StockTransfers_StockTransferId",
                        column: x => x.StockTransferId,
                        principalTable: "StockTransfers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StockMovements_StockTransferId",
                table: "StockMovements",
                column: "StockTransferId");

            migrationBuilder.CreateIndex(
                name: "IX_StockTransfer_Articles_StockTransferId",
                table: "StockTransfer_Articles",
                column: "StockTransferId");

            migrationBuilder.AddForeignKey(
                name: "FK_StockMovements_StockTransfers_StockTransferId",
                table: "StockMovements",
                column: "StockTransferId",
                principalTable: "StockTransfers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StockMovements_StockTransfers_StockTransferId",
                table: "StockMovements");

            migrationBuilder.DropTable(
                name: "StockTransfer_Articles");

            migrationBuilder.DropTable(
                name: "StockTransfers");

            migrationBuilder.DropIndex(
                name: "IX_StockMovements_StockTransferId",
                table: "StockMovements");

            migrationBuilder.DropColumn(
                name: "StockTransferId",
                table: "StockMovements");
        }
    }
}
