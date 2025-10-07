using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AccountingService.Migrations
{
    /// <inheritdoc />
    public partial class cambiosenallocation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "ReceiptId",
                schema: "ledger",
                table: "Allocations",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "CreditDocumentId",
                schema: "ledger",
                table: "Allocations",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<byte>(
                name: "Source",
                schema: "ledger",
                table: "Allocations",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.CreateIndex(
                name: "IX_Allocations_CreditDocumentId",
                schema: "ledger",
                table: "Allocations",
                column: "CreditDocumentId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Allocations_SourceShape",
                schema: "ledger",
                table: "Allocations",
                sql: "(\r\n                            (Source = 1 AND ReceiptId IS NOT NULL AND CreditDocumentId IS NULL) OR\r\n                            (Source = 2 AND CreditDocumentId IS NOT NULL AND ReceiptId IS NULL)\r\n                          )");

            migrationBuilder.AddForeignKey(
                name: "FK_Allocations_Documents_CreditDocumentId",
                schema: "ledger",
                table: "Allocations",
                column: "CreditDocumentId",
                principalSchema: "ledger",
                principalTable: "Documents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Allocations_Documents_CreditDocumentId",
                schema: "ledger",
                table: "Allocations");

            migrationBuilder.DropIndex(
                name: "IX_Allocations_CreditDocumentId",
                schema: "ledger",
                table: "Allocations");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Allocations_SourceShape",
                schema: "ledger",
                table: "Allocations");

            migrationBuilder.DropColumn(
                name: "CreditDocumentId",
                schema: "ledger",
                table: "Allocations");

            migrationBuilder.DropColumn(
                name: "Source",
                schema: "ledger",
                table: "Allocations");

            migrationBuilder.AlterColumn<int>(
                name: "ReceiptId",
                schema: "ledger",
                table: "Allocations",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}
