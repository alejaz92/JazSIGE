using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PurchaseService.Migrations
{
    /// <inheritdoc />
    public partial class agregadacolumnaStockUpdated : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "StockUpdated",
                table: "Purchases",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StockUpdated",
                table: "Purchases");
        }
    }
}
