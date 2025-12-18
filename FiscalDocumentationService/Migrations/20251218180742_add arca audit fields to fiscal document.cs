using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FiscalDocumentationService.Migrations
{
    /// <inheritdoc />
    public partial class addarcaauditfieldstofiscaldocument : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ArcaCorrelationId",
                table: "FiscalDocuments",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "ArcaErrorsJson",
                table: "FiscalDocuments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ArcaLastInteractionAt",
                table: "FiscalDocuments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ArcaObservationsJson",
                table: "FiscalDocuments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ArcaRequestJson",
                table: "FiscalDocuments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ArcaResponseJson",
                table: "FiscalDocuments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ArcaStatus",
                table: "FiscalDocuments",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ArcaCorrelationId",
                table: "FiscalDocuments");

            migrationBuilder.DropColumn(
                name: "ArcaErrorsJson",
                table: "FiscalDocuments");

            migrationBuilder.DropColumn(
                name: "ArcaLastInteractionAt",
                table: "FiscalDocuments");

            migrationBuilder.DropColumn(
                name: "ArcaObservationsJson",
                table: "FiscalDocuments");

            migrationBuilder.DropColumn(
                name: "ArcaRequestJson",
                table: "FiscalDocuments");

            migrationBuilder.DropColumn(
                name: "ArcaResponseJson",
                table: "FiscalDocuments");

            migrationBuilder.DropColumn(
                name: "ArcaStatus",
                table: "FiscalDocuments");
        }
    }
}
