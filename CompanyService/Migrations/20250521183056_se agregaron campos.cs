using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CompanyService.Migrations
{
    /// <inheritdoc />
    public partial class seagregaroncampos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "taxId",
                table: "CompanyInfo",
                newName: "TaxId");

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "CompanyInfo",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Country",
                table: "CompanyInfo",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "IVATypeId",
                table: "CompanyInfo",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "PostalCode",
                table: "CompanyInfo",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Province",
                table: "CompanyInfo",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "City",
                table: "CompanyInfo");

            migrationBuilder.DropColumn(
                name: "Country",
                table: "CompanyInfo");

            migrationBuilder.DropColumn(
                name: "IVATypeId",
                table: "CompanyInfo");

            migrationBuilder.DropColumn(
                name: "PostalCode",
                table: "CompanyInfo");

            migrationBuilder.DropColumn(
                name: "Province",
                table: "CompanyInfo");

            migrationBuilder.RenameColumn(
                name: "TaxId",
                table: "CompanyInfo",
                newName: "taxId");
        }
    }
}
