using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EcoCycleCore.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CUPONES",
                columns: table => new
                {
                    cupon_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    tienda_nombre = table.Column<string>(type: "varchar(150)", unicode: false, maxLength: 150, nullable: false),
                    desc_beneficio = table.Column<string>(type: "text", nullable: false),
                    cost_puntos = table.Column<int>(type: "int", nullable: false),
                    stock = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CUPONES", x => x.cupon_id);
                });

            migrationBuilder.CreateTable(
                name: "MATERIALES",
                columns: table => new
                {
                    material_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nombre_material = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    puntos_por_kilo = table.Column<decimal>(type: "decimal(10,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MATERIALES", x => x.material_id);
                });

            migrationBuilder.CreateTable(
                name: "USUARIOS",
                columns: table => new
                {
                    usuario_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nombre = table.Column<string>(type: "varchar(150)", unicode: false, maxLength: 150, nullable: false),
                    correo = table.Column<string>(type: "varchar(150)", unicode: false, maxLength: 150, nullable: false),
                    contrasena_hash = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false),
                    telefono = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    direccion = table.Column<string>(type: "text", nullable: true),
                    tipo_usuario = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    puntos_acumulacion = table.Column<int>(type: "int", nullable: true, defaultValue: 0),
                    fecha_registro = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    DocumentoIdentidad = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_USUARIOS", x => x.usuario_id);
                });

            migrationBuilder.CreateTable(
                name: "CANJES",
                columns: table => new
                {
                    canje_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    cupon_id = table.Column<int>(type: "int", nullable: false),
                    usuario_id = table.Column<int>(type: "int", nullable: false),
                    codigo_digital = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    fecha_canje = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CANJES", x => x.canje_id);
                    table.ForeignKey(
                        name: "FK_CANJES_CUPONES",
                        column: x => x.cupon_id,
                        principalTable: "CUPONES",
                        principalColumn: "cupon_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CANJES_USUARIOS",
                        column: x => x.usuario_id,
                        principalTable: "USUARIOS",
                        principalColumn: "usuario_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PUBLICACIONES",
                columns: table => new
                {
                    publicaciones_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    usuario_id = table.Column<int>(type: "int", nullable: false),
                    material_id = table.Column<int>(type: "int", nullable: false),
                    descripcion = table.Column<string>(type: "text", nullable: true),
                    peso_cantidad = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    ubicacion = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true),
                    url_imagen = table.Column<string>(type: "varchar(2083)", unicode: false, maxLength: 2083, nullable: true),
                    estado = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    fecha_publicacion = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    recolector_id = table.Column<int>(type: "int", nullable: true),
                    fecha_aceptacion = table.Column<DateTime>(type: "datetime", nullable: true),
                    fecha_entrega = table.Column<DateTime>(type: "datetime", nullable: true),
                    fecha_confirmacion = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PUBLICACIONES", x => x.publicaciones_id);
                    table.ForeignKey(
                        name: "FK_PUBLICACIONES_MATERIALES",
                        column: x => x.material_id,
                        principalTable: "MATERIALES",
                        principalColumn: "material_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PUBLICACIONES_RECOLECTOR",
                        column: x => x.recolector_id,
                        principalTable: "USUARIOS",
                        principalColumn: "usuario_id");
                    table.ForeignKey(
                        name: "FK_PUBLICACIONES_USUARIOS",
                        column: x => x.usuario_id,
                        principalTable: "USUARIOS",
                        principalColumn: "usuario_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ENTREGAS",
                columns: table => new
                {
                    entrega_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    publicacion_id = table.Column<int>(type: "int", nullable: false),
                    ciudadano_id = table.Column<int>(type: "int", nullable: false),
                    centro_id = table.Column<int>(type: "int", nullable: false),
                    peso_real = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    puntos_otorgados = table.Column<int>(type: "int", nullable: false),
                    fecha_entrega = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ENTREGAS", x => x.entrega_id);
                    table.ForeignKey(
                        name: "FK_ENTREGAS_PUBLICACIONES",
                        column: x => x.publicacion_id,
                        principalTable: "PUBLICACIONES",
                        principalColumn: "publicaciones_id");
                    table.ForeignKey(
                        name: "FK_ENTREGAS_USUARIOS",
                        column: x => x.ciudadano_id,
                        principalTable: "USUARIOS",
                        principalColumn: "usuario_id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_CANJES_cupon_id",
                table: "CANJES",
                column: "cupon_id");

            migrationBuilder.CreateIndex(
                name: "IX_CANJES_usuario_id",
                table: "CANJES",
                column: "usuario_id");

            migrationBuilder.CreateIndex(
                name: "UQ__CANJES__ECD7D41A82363E0E",
                table: "CANJES",
                column: "codigo_digital",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ENTREGAS_ciudadano_id",
                table: "ENTREGAS",
                column: "ciudadano_id");

            migrationBuilder.CreateIndex(
                name: "IX_ENTREGAS_publicacion_id",
                table: "ENTREGAS",
                column: "publicacion_id");

            migrationBuilder.CreateIndex(
                name: "IX_PUBLICACIONES_material_id",
                table: "PUBLICACIONES",
                column: "material_id");

            migrationBuilder.CreateIndex(
                name: "IX_PUBLICACIONES_recolector_id",
                table: "PUBLICACIONES",
                column: "recolector_id");

            migrationBuilder.CreateIndex(
                name: "IX_PUBLICACIONES_usuario_id",
                table: "PUBLICACIONES",
                column: "usuario_id");

            migrationBuilder.CreateIndex(
                name: "UQ__USUARIOS__2A586E0B3A764D46",
                table: "USUARIOS",
                column: "correo",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CANJES");

            migrationBuilder.DropTable(
                name: "ENTREGAS");

            migrationBuilder.DropTable(
                name: "CUPONES");

            migrationBuilder.DropTable(
                name: "PUBLICACIONES");

            migrationBuilder.DropTable(
                name: "MATERIALES");

            migrationBuilder.DropTable(
                name: "USUARIOS");
        }
    }
}
