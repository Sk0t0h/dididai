using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DididaiApp.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddGasto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Gastos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Concepto = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Importe = table.Column<decimal>(type: "TEXT", precision: 10, scale: 2, nullable: false),
                    Fecha = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Categoria = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Gastos", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Gastos");
        }
    }
}
