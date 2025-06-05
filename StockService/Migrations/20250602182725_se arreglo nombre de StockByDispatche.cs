using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StockService.Migrations
{
    /// <inheritdoc />
    public partial class searreglonombredeStockByDispatche : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_stockByDispatches",
                table: "stockByDispatches");

            migrationBuilder.RenameTable(
                name: "stockByDispatches",
                newName: "StockByDispatches");

            migrationBuilder.AddPrimaryKey(
                name: "PK_StockByDispatches",
                table: "StockByDispatches",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_StockByDispatches",
                table: "StockByDispatches");

            migrationBuilder.RenameTable(
                name: "StockByDispatches",
                newName: "stockByDispatches");

            migrationBuilder.AddPrimaryKey(
                name: "PK_stockByDispatches",
                table: "stockByDispatches",
                column: "Id");
        }
    }
}
