using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CatalogService.Migrations
{
    /// <inheritdoc />
    public partial class migration313 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DeliveryAddress",
                table: "Customers",
                newName: "DeliverAddress");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DeliverAddress",
                table: "Customers",
                newName: "DeliveryAddress");
        }
    }
}
