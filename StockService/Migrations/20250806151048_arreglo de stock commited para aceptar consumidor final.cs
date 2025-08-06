using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StockService.Migrations
{
    /// <inheritdoc />
    public partial class arreglodestockcommitedparaaceptarconsumidorfinal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "CustomerId",
                table: "CommitedStockEntries",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<bool>(
                name: "IsFinalConsumer",
                table: "CommitedStockEntries",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsFinalConsumer",
                table: "CommitedStockEntries");

            migrationBuilder.AlterColumn<int>(
                name: "CustomerId",
                table: "CommitedStockEntries",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}
