using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalesService.Migrations
{
    /// <inheritdoc />
    public partial class adaptaciondesalesquoteparaadmitirconsumerfinal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TransportId",
                table: "SalesQuotes");

            migrationBuilder.AlterColumn<int>(
                name: "CustomerId",
                table: "SalesQuotes",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "CustomerIdType",
                table: "SalesQuotes",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CustomerName",
                table: "SalesQuotes",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CustomerPostalCodeId",
                table: "SalesQuotes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomerTaxId",
                table: "SalesQuotes",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsFinalConsumer",
                table: "SalesQuotes",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CustomerIdType",
                table: "SalesQuotes");

            migrationBuilder.DropColumn(
                name: "CustomerName",
                table: "SalesQuotes");

            migrationBuilder.DropColumn(
                name: "CustomerPostalCodeId",
                table: "SalesQuotes");

            migrationBuilder.DropColumn(
                name: "CustomerTaxId",
                table: "SalesQuotes");

            migrationBuilder.DropColumn(
                name: "IsFinalConsumer",
                table: "SalesQuotes");

            migrationBuilder.AlterColumn<int>(
                name: "CustomerId",
                table: "SalesQuotes",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TransportId",
                table: "SalesQuotes",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
