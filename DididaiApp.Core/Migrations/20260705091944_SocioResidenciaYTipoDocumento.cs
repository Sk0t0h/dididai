using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DididaiApp.Core.Migrations
{
    /// <inheritdoc />
    public partial class SocioResidenciaYTipoDocumento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Se sustituye el antiguo Pais (texto libre) por PaisResidencia (ISO 3166-1
            // alpha-2) y se añade TipoDocumento (enum: DNI/NIE/Pasaporte/Otro), que es
            // lo que decide la validación del documento (no el país). Es drop+add: los
            // valores antiguos eran texto libre no mapeable. En producción la tabla está
            // vacía y en local solo había socios de prueba desechables.
            migrationBuilder.DropColumn(
                name: "Pais",
                table: "Socios");

            migrationBuilder.AddColumn<string>(
                name: "PaisResidencia",
                table: "Socios",
                type: "TEXT",
                maxLength: 2,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "TipoDocumento",
                table: "Socios",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaisResidencia",
                table: "Socios");

            migrationBuilder.DropColumn(
                name: "TipoDocumento",
                table: "Socios");

            migrationBuilder.AddColumn<string>(
                name: "Pais",
                table: "Socios",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }
    }
}
