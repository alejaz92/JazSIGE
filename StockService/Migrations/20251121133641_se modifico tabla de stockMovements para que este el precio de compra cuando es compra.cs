using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StockService.Migrations
{
    /// <inheritdoc />
    public partial class semodificotabladestockMovementsparaqueesteelpreciodecompracuandoescompra : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "AvgUnitCost",
                table: "StockMovements",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "LastUnitCost",
                table: "StockMovements",
                type: "decimal(18,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvgUnitCost",
                table: "StockMovements");

            migrationBuilder.DropColumn(
                name: "LastUnitCost",
                table: "StockMovements");
        }
    }
}
