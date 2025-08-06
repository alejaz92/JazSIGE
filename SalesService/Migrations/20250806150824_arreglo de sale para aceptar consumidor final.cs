using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalesService.Migrations
{
    /// <inheritdoc />
    public partial class arreglodesaleparaaceptarconsumidorfinal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "CustomerId",
                table: "Sales",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "CustomerIdType",
                table: "Sales",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CustomerName",
                table: "Sales",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "CustomerPostalCodeId",
                table: "Sales",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomerTaxId",
                table: "Sales",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsFinalConsumer",
                table: "Sales",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CustomerIdType",
                table: "Sales");

            migrationBuilder.DropColumn(
                name: "CustomerName",
                table: "Sales");

            migrationBuilder.DropColumn(
                name: "CustomerPostalCodeId",
                table: "Sales");

            migrationBuilder.DropColumn(
                name: "CustomerTaxId",
                table: "Sales");

            migrationBuilder.DropColumn(
                name: "IsFinalConsumer",
                table: "Sales");

            migrationBuilder.AlterColumn<int>(
                name: "CustomerId",
                table: "Sales",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}
