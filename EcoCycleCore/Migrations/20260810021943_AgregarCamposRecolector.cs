using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EcoCycleCore.Migrations
{
    /// <inheritdoc />
    public partial class AgregarCamposRecolector : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NotaRecolector",
                table: "PUBLICACIONES",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "EntregaCorrecta",
                table: "PUBLICACIONES",
                type: "bit",
                nullable: true);

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "NotaRecolector", table: "PUBLICACIONES");
            migrationBuilder.DropColumn(name: "EntregaCorrecta", table: "PUBLICACIONES");
        }
    }
}
