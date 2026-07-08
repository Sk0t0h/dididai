using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DididaiApp.Core.Migrations
{
    /// <inheritdoc />
    public partial class RediseñoFlujoSolicitudes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SocioId",
                table: "SolicitudesColaboracion",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Localidad",
                table: "Socios",
                type: "TEXT",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Direccion",
                table: "Socios",
                type: "TEXT",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "CodigoPostal",
                table: "Socios",
                type: "TEXT",
                maxLength: 10,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 10);

            migrationBuilder.CreateTable(
                name: "AccionesSolicitud",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SolicitudId = table.Column<int>(type: "INTEGER", nullable: false),
                    Tipo = table.Column<int>(type: "INTEGER", nullable: false),
                    Nota = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    Usuario = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    Fecha = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccionesSolicitud", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccionesSolicitud_SolicitudesColaboracion_SolicitudId",
                        column: x => x.SolicitudId,
                        principalTable: "SolicitudesColaboracion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesColaboracion_SocioId",
                table: "SolicitudesColaboracion",
                column: "SocioId");

            migrationBuilder.CreateIndex(
                name: "IX_AccionesSolicitud_SolicitudId",
                table: "AccionesSolicitud",
                column: "SolicitudId");

            migrationBuilder.AddForeignKey(
                name: "FK_SolicitudesColaboracion_Socios_SocioId",
                table: "SolicitudesColaboracion",
                column: "SocioId",
                principalTable: "Socios",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SolicitudesColaboracion_Socios_SocioId",
                table: "SolicitudesColaboracion");

            migrationBuilder.DropTable(
                name: "AccionesSolicitud");

            migrationBuilder.DropIndex(
                name: "IX_SolicitudesColaboracion_SocioId",
                table: "SolicitudesColaboracion");

            migrationBuilder.DropColumn(
                name: "SocioId",
                table: "SolicitudesColaboracion");

            migrationBuilder.AlterColumn<string>(
                name: "Localidad",
                table: "Socios",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Direccion",
                table: "Socios",
                type: "TEXT",
                maxLength: 200,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CodigoPostal",
                table: "Socios",
                type: "TEXT",
                maxLength: 10,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 10,
                oldNullable: true);
        }
    }
}
