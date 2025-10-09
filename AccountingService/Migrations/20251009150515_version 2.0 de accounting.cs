using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AccountingService.Migrations
{
    /// <inheritdoc />
    public partial class version20deaccounting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Allocations",
                schema: "ledger");

            migrationBuilder.DropTable(
                name: "NumberingSequences",
                schema: "ledger");

            migrationBuilder.DropTable(
                name: "Documents",
                schema: "ledger");

            migrationBuilder.DropIndex(
                name: "IX_Receipts_PartyType_PartyId_Date",
                schema: "ledger",
                table: "Receipts");

            migrationBuilder.DropColumn(
                name: "Currency",
                schema: "ledger",
                table: "Receipts");

            migrationBuilder.DropColumn(
                name: "FxRate",
                schema: "ledger",
                table: "Receipts");

            migrationBuilder.DropColumn(
                name: "IsVoided",
                schema: "ledger",
                table: "Receipts");

            migrationBuilder.DropColumn(
                name: "PartyId",
                schema: "ledger",
                table: "Receipts");

            migrationBuilder.DropColumn(
                name: "PartyType",
                schema: "ledger",
                table: "Receipts");

            migrationBuilder.DropColumn(
                name: "TotalBase",
                schema: "ledger",
                table: "Receipts");

            migrationBuilder.DropColumn(
                name: "TotalOriginal",
                schema: "ledger",
                table: "Receipts");

            migrationBuilder.DropColumn(
                name: "VoidedAt",
                schema: "ledger",
                table: "Receipts");

            migrationBuilder.DropColumn(
                name: "AmountBase",
                schema: "ledger",
                table: "ReceiptPayments");

            migrationBuilder.DropColumn(
                name: "CheckIssueDate",
                schema: "ledger",
                table: "ReceiptPayments");

            migrationBuilder.DropColumn(
                name: "CheckIssuerBankCode",
                schema: "ledger",
                table: "ReceiptPayments");

            migrationBuilder.RenameTable(
                name: "Receipts",
                schema: "ledger",
                newName: "Receipts");

            migrationBuilder.RenameTable(
                name: "ReceiptPayments",
                schema: "ledger",
                newName: "ReceiptPayments");

            migrationBuilder.RenameColumn(
                name: "Date",
                table: "Receipts",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "CheckPaymentDate",
                table: "ReceiptPayments",
                newName: "CheckDueDate");

            migrationBuilder.RenameColumn(
                name: "CheckIssuerTaxId",
                table: "ReceiptPayments",
                newName: "CheckIssuer");

            migrationBuilder.RenameColumn(
                name: "CheckIssuerName",
                table: "ReceiptPayments",
                newName: "CheckBankCode");

            migrationBuilder.RenameColumn(
                name: "CheckIsThirdParty",
                table: "ReceiptPayments",
                newName: "IsThirdPartyCheck");

            migrationBuilder.AlterColumn<string>(
                name: "Number",
                table: "Receipts",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "Receipts",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Receipts",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AlterColumn<string>(
                name: "TransactionReference",
                table: "ReceiptPayments",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "ReceiptPayments",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Method",
                table: "ReceiptPayments",
                type: "int",
                nullable: false,
                oldClrType: typeof(byte),
                oldType: "tinyint");

            migrationBuilder.AddColumn<decimal>(
                name: "AmountARS",
                table: "ReceiptPayments",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "ReceiptPayments",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "FxRate",
                table: "ReceiptPayments",
                type: "decimal(18,6)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "ReceiptPayments",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "ReceiptPayments",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateTable(
                name: "AllocationBatches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TargetDocumentId = table.Column<int>(type: "int", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AllocationBatches", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LedgerDocuments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PartyType = table.Column<int>(type: "int", nullable: false),
                    PartyId = table.Column<int>(type: "int", nullable: false),
                    Kind = table.Column<int>(type: "int", nullable: false),
                    ExternalRefId = table.Column<int>(type: "int", nullable: false),
                    ExternalRefNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DocumentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    FxRate = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    AmountOriginal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AmountARS = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PendingARS = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LedgerDocuments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ReceiptAllocations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReceiptId = table.Column<int>(type: "int", nullable: false),
                    TargetDocumentId = table.Column<int>(type: "int", nullable: false),
                    AppliedARS = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReceiptAllocations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReceiptAllocations_Receipts_ReceiptId",
                        column: x => x.ReceiptId,
                        principalTable: "Receipts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AllocationItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AllocationBatchId = table.Column<int>(type: "int", nullable: false),
                    SourceDocumentId = table.Column<int>(type: "int", nullable: false),
                    AppliedARS = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AllocationItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AllocationItems_AllocationBatches_AllocationBatchId",
                        column: x => x.AllocationBatchId,
                        principalTable: "AllocationBatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AllocationBatches_TargetDocumentId",
                table: "AllocationBatches",
                column: "TargetDocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_AllocationItems_AllocationBatchId",
                table: "AllocationItems",
                column: "AllocationBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_LedgerDocuments_Kind_ExternalRefId",
                table: "LedgerDocuments",
                columns: new[] { "Kind", "ExternalRefId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LedgerDocuments_Kind_Status",
                table: "LedgerDocuments",
                columns: new[] { "Kind", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_LedgerDocuments_PartyType_PartyId_DocumentDate",
                table: "LedgerDocuments",
                columns: new[] { "PartyType", "PartyId", "DocumentDate" });

            migrationBuilder.CreateIndex(
                name: "IX_ReceiptAllocations_ReceiptId",
                table: "ReceiptAllocations",
                column: "ReceiptId");

            migrationBuilder.CreateIndex(
                name: "IX_ReceiptAllocations_TargetDocumentId",
                table: "ReceiptAllocations",
                column: "TargetDocumentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AllocationItems");

            migrationBuilder.DropTable(
                name: "LedgerDocuments");

            migrationBuilder.DropTable(
                name: "ReceiptAllocations");

            migrationBuilder.DropTable(
                name: "AllocationBatches");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Receipts");

            migrationBuilder.DropColumn(
                name: "AmountARS",
                table: "ReceiptPayments");

            migrationBuilder.DropColumn(
                name: "Currency",
                table: "ReceiptPayments");

            migrationBuilder.DropColumn(
                name: "FxRate",
                table: "ReceiptPayments");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "ReceiptPayments");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "ReceiptPayments");

            migrationBuilder.EnsureSchema(
                name: "ledger");

            migrationBuilder.RenameTable(
                name: "Receipts",
                newName: "Receipts",
                newSchema: "ledger");

            migrationBuilder.RenameTable(
                name: "ReceiptPayments",
                newName: "ReceiptPayments",
                newSchema: "ledger");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                schema: "ledger",
                table: "Receipts",
                newName: "Date");

            migrationBuilder.RenameColumn(
                name: "IsThirdPartyCheck",
                schema: "ledger",
                table: "ReceiptPayments",
                newName: "CheckIsThirdParty");

            migrationBuilder.RenameColumn(
                name: "CheckIssuer",
                schema: "ledger",
                table: "ReceiptPayments",
                newName: "CheckIssuerTaxId");

            migrationBuilder.RenameColumn(
                name: "CheckDueDate",
                schema: "ledger",
                table: "ReceiptPayments",
                newName: "CheckPaymentDate");

            migrationBuilder.RenameColumn(
                name: "CheckBankCode",
                schema: "ledger",
                table: "ReceiptPayments",
                newName: "CheckIssuerName");

            migrationBuilder.AlterColumn<string>(
                name: "Number",
                schema: "ledger",
                table: "Receipts",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                schema: "ledger",
                table: "Receipts",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Currency",
                schema: "ledger",
                table: "Receipts",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "FxRate",
                schema: "ledger",
                table: "Receipts",
                type: "decimal(18,6)",
                precision: 18,
                scale: 6,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "IsVoided",
                schema: "ledger",
                table: "Receipts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "PartyId",
                schema: "ledger",
                table: "Receipts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<byte>(
                name: "PartyType",
                schema: "ledger",
                table: "Receipts",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalBase",
                schema: "ledger",
                table: "Receipts",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalOriginal",
                schema: "ledger",
                table: "Receipts",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "VoidedAt",
                schema: "ledger",
                table: "Receipts",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TransactionReference",
                schema: "ledger",
                table: "ReceiptPayments",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                schema: "ledger",
                table: "ReceiptPayments",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<byte>(
                name: "Method",
                schema: "ledger",
                table: "ReceiptPayments",
                type: "tinyint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<decimal>(
                name: "AmountBase",
                schema: "ledger",
                table: "ReceiptPayments",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "CheckIssueDate",
                schema: "ledger",
                table: "ReceiptPayments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CheckIssuerBankCode",
                schema: "ledger",
                table: "ReceiptPayments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Documents",
                schema: "ledger",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReceiptId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    DisplayNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DocumentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FxRate = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    Kind = table.Column<byte>(type: "tinyint", nullable: false),
                    PartyId = table.Column<int>(type: "int", nullable: false),
                    PartyType = table.Column<byte>(type: "tinyint", nullable: false),
                    SourceDocumentId = table.Column<long>(type: "bigint", nullable: true),
                    SourceKind = table.Column<byte>(type: "tinyint", nullable: true),
                    Status = table.Column<byte>(type: "tinyint", nullable: false),
                    TotalBase = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalOriginal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    VoidedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Documents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Documents_Receipts_ReceiptId",
                        column: x => x.ReceiptId,
                        principalSchema: "ledger",
                        principalTable: "Receipts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "NumberingSequences",
                schema: "ledger",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NextNumber = table.Column<int>(type: "int", nullable: false),
                    Scope = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NumberingSequences", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Allocations",
                schema: "ledger",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreditDocumentId = table.Column<int>(type: "int", nullable: true),
                    DebitDocumentId = table.Column<int>(type: "int", nullable: false),
                    ReceiptId = table.Column<int>(type: "int", nullable: true),
                    AmountBase = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    Source = table.Column<byte>(type: "tinyint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Allocations", x => x.Id);
                    table.CheckConstraint("CK_Allocations_Amount_Positive", "[AmountBase] > 0");
                    table.CheckConstraint("CK_Allocations_SourceShape", "(\r\n                            (Source = 1 AND ReceiptId IS NOT NULL AND CreditDocumentId IS NULL) OR\r\n                            (Source = 2 AND CreditDocumentId IS NOT NULL AND ReceiptId IS NULL)\r\n                          )");
                    table.ForeignKey(
                        name: "FK_Allocations_Documents_CreditDocumentId",
                        column: x => x.CreditDocumentId,
                        principalSchema: "ledger",
                        principalTable: "Documents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Allocations_Documents_DebitDocumentId",
                        column: x => x.DebitDocumentId,
                        principalSchema: "ledger",
                        principalTable: "Documents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Allocations_Receipts_ReceiptId",
                        column: x => x.ReceiptId,
                        principalSchema: "ledger",
                        principalTable: "Receipts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Receipts_PartyType_PartyId_Date",
                schema: "ledger",
                table: "Receipts",
                columns: new[] { "PartyType", "PartyId", "Date" });

            migrationBuilder.CreateIndex(
                name: "IX_Allocations_CreditDocumentId",
                schema: "ledger",
                table: "Allocations",
                column: "CreditDocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_Allocations_DebitDocumentId",
                schema: "ledger",
                table: "Allocations",
                column: "DebitDocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_Allocations_ReceiptId",
                schema: "ledger",
                table: "Allocations",
                column: "ReceiptId");

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
        }
    }
}
