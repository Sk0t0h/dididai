using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DididaiApp.Core.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Socios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nombre = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Apellidos = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    Dni = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Telefono = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    Direccion = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    CodigoPostal = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    Localidad = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Pais = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    AceptaPrivacidad = table.Column<bool>(type: "INTEGER", nullable: false),
                    FechaAlta = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Socios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Colaboraciones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SocioId = table.Column<int>(type: "INTEGER", nullable: false),
                    Importe = table.Column<decimal>(type: "TEXT", precision: 10, scale: 2, nullable: false),
                    FechaInicio = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FechaFin = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Activa = table.Column<bool>(type: "INTEGER", nullable: false),
                    Discriminator = table.Column<string>(type: "TEXT", maxLength: 21, nullable: false),
                    Fecha = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Modalidad = table.Column<int>(type: "INTEGER", nullable: true),
                    Iban = table.Column<string>(type: "TEXT", maxLength: 34, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Colaboraciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Colaboraciones_Socios_SocioId",
                        column: x => x.SocioId,
                        principalTable: "Socios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Colaboraciones_SocioId",
                table: "Colaboraciones",
                column: "SocioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Colaboraciones");

            migrationBuilder.DropTable(
                name: "Socios");
        }
    }
}
