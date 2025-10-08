using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AccountingService.Migrations
{
    /// <inheritdoc />
    public partial class arreglosminimosenaccounting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddCheckConstraint(
                name: "CK_Allocations_Amount_Positive",
                schema: "ledger",
                table: "Allocations",
                sql: "[AmountBase] > 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Allocations_Amount_Positive",
                schema: "ledger",
                table: "Allocations");
        }
    }
}
