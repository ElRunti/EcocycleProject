using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EcoCycleCore.Migrations
{
    /// <inheritdoc />
    public partial class AgregarEvidenciaPublicacion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EvidenciaUrl",
                table: "PUBLICACIONES",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EvidenciaUrl",
                table: "PUBLICACIONES");
        }
    }
}
