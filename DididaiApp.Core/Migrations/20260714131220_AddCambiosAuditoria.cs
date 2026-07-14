using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DididaiApp.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddCambiosAuditoria : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Cambios",
                table: "RegistrosAuditoria",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Cambios",
                table: "RegistrosAuditoria");
        }
    }
}
