using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StockService.Migrations
{
    /// <inheritdoc />
    public partial class ArregloCommitedStockEntry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsProcessed",
                table: "CommitedStockEntries");

            migrationBuilder.AddColumn<decimal>(
                name: "Delivered",
                table: "CommitedStockEntries",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Delivered",
                table: "CommitedStockEntries");

            migrationBuilder.AddColumn<bool>(
                name: "IsProcessed",
                table: "CommitedStockEntries",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
