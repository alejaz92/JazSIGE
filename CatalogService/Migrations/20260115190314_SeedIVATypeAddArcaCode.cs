using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CatalogService.Migrations
{
    /// <inheritdoc />
    public partial class SeedIVATypeAddArcaCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "IVATypes",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.InsertData(
                table: "IVATypes",
                columns: new[] { "Id", "ArcaCode", "CreatedAt", "Description", "IsActive", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, 1, new DateTime(2026, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "IVA Responsable Inscripto", true, new DateTime(2026, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 2, 4, new DateTime(2026, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "IVA Sujeto Exento", true, new DateTime(2026, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 3, 5, new DateTime(2026, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Consumidor Final", true, new DateTime(2026, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 4, 6, new DateTime(2026, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Responsable Monotributo", true, new DateTime(2026, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 5, 7, new DateTime(2026, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Sujeto No Categorizado", true, new DateTime(2026, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 6, 8, new DateTime(2026, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Proveedor del Exterior", true, new DateTime(2026, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 7, 9, new DateTime(2026, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Cliente del Exterior", true, new DateTime(2026, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 8, 10, new DateTime(2026, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "IVA Liberado – Ley 19.640", true, new DateTime(2026, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 9, 13, new DateTime(2026, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Monotributista Social", true, new DateTime(2026, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 10, 15, new DateTime(2026, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "IVA No Alcanzado", true, new DateTime(2026, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 11, 16, new DateTime(2026, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Monotributo Trabajador Independiente Promovido", true, new DateTime(2026, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.CreateIndex(
                name: "IX_IVATypes_ArcaCode",
                table: "IVATypes",
                column: "ArcaCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IVATypes_Description",
                table: "IVATypes",
                column: "Description",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_IVATypes_ArcaCode",
                table: "IVATypes");

            migrationBuilder.DropIndex(
                name: "IX_IVATypes_Description",
                table: "IVATypes");

            migrationBuilder.DeleteData(
                table: "IVATypes",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "IVATypes",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "IVATypes",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "IVATypes",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "IVATypes",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "IVATypes",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "IVATypes",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "IVATypes",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "IVATypes",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "IVATypes",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "IVATypes",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "IVATypes",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);
        }
    }
}
