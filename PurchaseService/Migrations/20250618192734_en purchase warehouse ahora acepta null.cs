using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PurchaseService.Migrations
{
    /// <inheritdoc />
    public partial class enpurchasewarehouseahoraaceptanull : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StockUpdated",
                table: "Purchases");

            migrationBuilder.AlterColumn<int>(
                name: "WarehouseId",
                table: "Purchases",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "WarehouseId",
                table: "Purchases",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "StockUpdated",
                table: "Purchases",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
