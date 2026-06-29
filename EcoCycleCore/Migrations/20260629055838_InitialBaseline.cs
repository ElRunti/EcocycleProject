using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EcoCycleCore.Migrations
{
    /// <inheritdoc />
    public partial class InitialBaseline : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Vacío intencionalmente: las tablas ya existen en la base de datos.
            // Esta migración solo marca el punto de partida (baseline) en el historial de EF Core.
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Vacío intencionalmente, ver comentario en Up().
        }
    }
}