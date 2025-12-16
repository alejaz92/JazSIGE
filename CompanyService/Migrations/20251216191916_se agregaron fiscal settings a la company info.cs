using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CompanyService.Migrations
{
    /// <inheritdoc />
    public partial class seagregaronfiscalsettingsalacompanyinfo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ArcaEnabled",
                table: "CompanyInfo",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "ArcaEnvironment",
                table: "CompanyInfo",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ArcaInvoiceTypesEnabled",
                table: "CompanyInfo",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "ArcaPointOfSale",
                table: "CompanyInfo",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ArcaEnabled",
                table: "CompanyInfo");

            migrationBuilder.DropColumn(
                name: "ArcaEnvironment",
                table: "CompanyInfo");

            migrationBuilder.DropColumn(
                name: "ArcaInvoiceTypesEnabled",
                table: "CompanyInfo");

            migrationBuilder.DropColumn(
                name: "ArcaPointOfSale",
                table: "CompanyInfo");
        }
    }
}
