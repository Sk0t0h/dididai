using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DididaiApp.Core.Migrations
{
    /// <inheritdoc />
    public partial class SolicitudColaboracionId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ColaboracionId",
                table: "SolicitudesColaboracion",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesColaboracion_ColaboracionId",
                table: "SolicitudesColaboracion",
                column: "ColaboracionId");

            migrationBuilder.AddForeignKey(
                name: "FK_SolicitudesColaboracion_Colaboraciones_ColaboracionId",
                table: "SolicitudesColaboracion",
                column: "ColaboracionId",
                principalTable: "Colaboraciones",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SolicitudesColaboracion_Colaboraciones_ColaboracionId",
                table: "SolicitudesColaboracion");

            migrationBuilder.DropIndex(
                name: "IX_SolicitudesColaboracion_ColaboracionId",
                table: "SolicitudesColaboracion");

            migrationBuilder.DropColumn(
                name: "ColaboracionId",
                table: "SolicitudesColaboracion");
        }
    }
}
