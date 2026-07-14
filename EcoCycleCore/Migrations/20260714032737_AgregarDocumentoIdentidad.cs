using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EcoCycleCore.Migrations
{
    /// <inheritdoc />
    public partial class AgregarDocumentoIdentidad : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DocumentoIdentidad",
                table: "USUARIOS",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DocumentoIdentidad",
                table: "USUARIOS");
        }
    }
}
