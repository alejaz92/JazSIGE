using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AccountingService.Migrations
{
    /// <inheritdoc />
    public partial class version20deaccountingservice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Documents_FiscalDocumentId",
                schema: "ledger",
                table: "Documents");

            migrationBuilder.DropIndex(
                name: "IX_Documents_PartyType_PartyId_Status_Kind",
                schema: "ledger",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "FiscalDocumentId",
                schema: "ledger",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "FiscalDocumentNumber",
                schema: "ledger",
                table: "Documents");

            migrationBuilder.AddColumn<string>(
                name: "DisplayNumber",
                schema: "ledger",
                table: "Documents",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReceiptId",
                schema: "ledger",
                table: "Documents",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "SourceDocumentId",
                schema: "ledger",
                table: "Documents",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<byte>(
                name: "SourceKind",
                schema: "ledger",
                table: "Documents",
                type: "tinyint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Documents_Party_Status_Kind_Date",
                schema: "ledger",
                table: "Documents",
                columns: new[] { "PartyType", "PartyId", "Status", "Kind", "DocumentDate" });

            migrationBuilder.CreateIndex(
                name: "UX_Documents_ReceiptId",
                schema: "ledger",
                table: "Documents",
                column: "ReceiptId",
                unique: true,
                filter: "[ReceiptId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UX_Documents_SourceKind_SourceId",
                schema: "ledger",
                table: "Documents",
                columns: new[] { "SourceKind", "SourceDocumentId" },
                unique: true,
                filter: "[SourceKind] IS NOT NULL AND [SourceDocumentId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Documents_Receipts_ReceiptId",
                schema: "ledger",
                table: "Documents",
                column: "ReceiptId",
                principalSchema: "ledger",
                principalTable: "Receipts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Documents_Receipts_ReceiptId",
                schema: "ledger",
                table: "Documents");

            migrationBuilder.DropIndex(
                name: "IX_Documents_Party_Status_Kind_Date",
                schema: "ledger",
                table: "Documents");

            migrationBuilder.DropIndex(
                name: "UX_Documents_ReceiptId",
                schema: "ledger",
                table: "Documents");

            migrationBuilder.DropIndex(
                name: "UX_Documents_SourceKind_SourceId",
                schema: "ledger",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "DisplayNumber",
                schema: "ledger",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "ReceiptId",
                schema: "ledger",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "SourceDocumentId",
                schema: "ledger",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "SourceKind",
                schema: "ledger",
                table: "Documents");

            migrationBuilder.AddColumn<int>(
                name: "FiscalDocumentId",
                schema: "ledger",
                table: "Documents",
                type: "int",
                maxLength: 50,
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "FiscalDocumentNumber",
                schema: "ledger",
                table: "Documents",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_FiscalDocumentId",
                schema: "ledger",
                table: "Documents",
                column: "FiscalDocumentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Documents_PartyType_PartyId_Status_Kind",
                schema: "ledger",
                table: "Documents",
                columns: new[] { "PartyType", "PartyId", "Status", "Kind" });
        }
    }
}
