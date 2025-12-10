using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PurchaseService.Migrations
{
    /// <inheritdoc />
    public partial class seagregoexcepciondeunicidaddecompraenpurchasedocumentinvoiceparaignorarcancelados : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PurchaseDocuments_PurchaseId",
                table: "PurchaseDocuments");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseDocuments_PurchaseId",
                table: "PurchaseDocuments",
                column: "PurchaseId",
                unique: true,
                filter: "[Type] = 1 AND [IsCanceled] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PurchaseDocuments_PurchaseId",
                table: "PurchaseDocuments");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseDocuments_PurchaseId",
                table: "PurchaseDocuments",
                column: "PurchaseId",
                unique: true,
                filter: "[Type] = 1");
        }
    }
}
