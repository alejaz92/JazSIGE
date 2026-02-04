using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PurchaseService.Migrations
{
    /// <inheritdoc />
    public partial class purchaseIdnullableenDispatch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Dispatches_Purchases_PurchaseId",
                table: "Dispatches");

            migrationBuilder.DropIndex(
                name: "IX_Dispatches_PurchaseId",
                table: "Dispatches");

            migrationBuilder.AlterColumn<int>(
                name: "PurchaseId",
                table: "Dispatches",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateIndex(
                name: "IX_Dispatches_PurchaseId",
                table: "Dispatches",
                column: "PurchaseId",
                unique: true,
                filter: "[PurchaseId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Dispatches_Purchases_PurchaseId",
                table: "Dispatches",
                column: "PurchaseId",
                principalTable: "Purchases",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Dispatches_Purchases_PurchaseId",
                table: "Dispatches");

            migrationBuilder.DropIndex(
                name: "IX_Dispatches_PurchaseId",
                table: "Dispatches");

            migrationBuilder.AlterColumn<int>(
                name: "PurchaseId",
                table: "Dispatches",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Dispatches_PurchaseId",
                table: "Dispatches",
                column: "PurchaseId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Dispatches_Purchases_PurchaseId",
                table: "Dispatches",
                column: "PurchaseId",
                principalTable: "Purchases",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
