using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DididaiApp.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddPeriodicidadGasto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "FechaFin",
                table: "Gastos",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Periodicidad",
                table: "Gastos",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FechaFin",
                table: "Gastos");

            migrationBuilder.DropColumn(
                name: "Periodicidad",
                table: "Gastos");
        }
    }
}
