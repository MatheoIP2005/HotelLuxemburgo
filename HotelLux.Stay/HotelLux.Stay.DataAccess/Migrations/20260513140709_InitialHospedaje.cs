using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace HotelLux.Stay.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class InitialHospedaje : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "hospedaje");

            migrationBuilder.CreateTable(
                name: "estadia",
                schema: "hospedaje",
                columns: table => new
                {
                    id_estadia = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    estadia_guid = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    reserva_guid = table.Column<Guid>(type: "uuid", nullable: false),
                    reserva_habitacion_guid = table.Column<Guid>(type: "uuid", nullable: false),
                    cliente_guid = table.Column<Guid>(type: "uuid", nullable: false),
                    sucursal_guid = table.Column<Guid>(type: "uuid", nullable: false),
                    habitacion_guid = table.Column<Guid>(type: "uuid", nullable: false),
                    estado_estadia = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    checkin_utc = table.Column<DateTimeOffset>(type: "TIMESTAMPTZ", nullable: true),
                    checkout_utc = table.Column<DateTimeOffset>(type: "TIMESTAMPTZ", nullable: true),
                    observaciones_checkin = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    observaciones_checkout = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    requiere_mantenimiento = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    fecha_registro_utc = table.Column<DateTimeOffset>(type: "TIMESTAMPTZ", nullable: false),
                    creado_por_usuario = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    modificado_por_usuario = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    fecha_modificacion_utc = table.Column<DateTimeOffset>(type: "TIMESTAMPTZ", nullable: true),
                    es_eliminado = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    servicio_origen = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "stay-service")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_estadia", x => x.id_estadia);
                });

            migrationBuilder.CreateTable(
                name: "valoracion",
                schema: "hospedaje",
                columns: table => new
                {
                    id_valoracion = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    valoracion_guid = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    estadia_guid = table.Column<Guid>(type: "uuid", nullable: false),
                    sucursal_guid = table.Column<Guid>(type: "uuid", nullable: false),
                    cliente_guid = table.Column<Guid>(type: "uuid", nullable: false),
                    puntuacion_general = table.Column<decimal>(type: "numeric(4,2)", nullable: false),
                    puntuacion_limpieza = table.Column<decimal>(type: "numeric(4,2)", nullable: false),
                    puntuacion_confort = table.Column<decimal>(type: "numeric(4,2)", nullable: false),
                    puntuacion_ubicacion = table.Column<decimal>(type: "numeric(4,2)", nullable: false),
                    puntuacion_instalaciones = table.Column<decimal>(type: "numeric(4,2)", nullable: false),
                    puntuacion_personal = table.Column<decimal>(type: "numeric(4,2)", nullable: false),
                    puntuacion_calidad_precio = table.Column<decimal>(type: "numeric(4,2)", nullable: false),
                    comentario_positivo = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    comentario_negativo = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    tipo_viaje = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    fecha_publicacion_utc = table.Column<DateTimeOffset>(type: "TIMESTAMPTZ", nullable: false),
                    respuesta_hotel = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    es_eliminado = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    fecha_registro_utc = table.Column<DateTimeOffset>(type: "TIMESTAMPTZ", nullable: false),
                    creado_por_usuario = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_valoracion", x => x.id_valoracion);
                });

            migrationBuilder.CreateTable(
                name: "cargo_estadia",
                schema: "hospedaje",
                columns: table => new
                {
                    id_cargo_estadia = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    cargo_guid = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    id_estadia = table.Column<int>(type: "integer", nullable: false),
                    catalogo_guid = table.Column<Guid>(type: "uuid", nullable: true),
                    descripcion_cargo = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    cantidad = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    precio_unitario = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    subtotal = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    valor_iva = table.Column<decimal>(type: "numeric(12,2)", nullable: false, defaultValue: 0m),
                    total_cargo = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    fecha_consumo_utc = table.Column<DateTimeOffset>(type: "TIMESTAMPTZ", nullable: false),
                    estado_cargo = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false, defaultValue: "PEN"),
                    fecha_registro_utc = table.Column<DateTimeOffset>(type: "TIMESTAMPTZ", nullable: false),
                    creado_por_usuario = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    modificado_por_usuario = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    fecha_modificacion_utc = table.Column<DateTimeOffset>(type: "TIMESTAMPTZ", nullable: true),
                    modificacion_ip = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    servicio_origen = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "stay-service")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cargo_estadia", x => x.id_cargo_estadia);
                    table.ForeignKey(
                        name: "FK_cargo_estadia_estadia_id_estadia",
                        column: x => x.id_estadia,
                        principalSchema: "hospedaje",
                        principalTable: "estadia",
                        principalColumn: "id_estadia",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_cargo_estadia_cargo_guid",
                schema: "hospedaje",
                table: "cargo_estadia",
                column: "cargo_guid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_cargo_estadia_id_estadia_estado_cargo_fecha_consumo_utc",
                schema: "hospedaje",
                table: "cargo_estadia",
                columns: new[] { "id_estadia", "estado_cargo", "fecha_consumo_utc" });

            migrationBuilder.CreateIndex(
                name: "IX_estadia_estadia_guid",
                schema: "hospedaje",
                table: "estadia",
                column: "estadia_guid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_estadia_reserva_guid_estado_estadia",
                schema: "hospedaje",
                table: "estadia",
                columns: new[] { "reserva_guid", "estado_estadia" });

            migrationBuilder.CreateIndex(
                name: "IX_valoracion_cliente_guid",
                schema: "hospedaje",
                table: "valoracion",
                column: "cliente_guid");

            migrationBuilder.CreateIndex(
                name: "IX_valoracion_estadia_guid",
                schema: "hospedaje",
                table: "valoracion",
                column: "estadia_guid");

            migrationBuilder.CreateIndex(
                name: "IX_valoracion_sucursal_guid",
                schema: "hospedaje",
                table: "valoracion",
                column: "sucursal_guid");

            migrationBuilder.CreateIndex(
                name: "IX_valoracion_valoracion_guid",
                schema: "hospedaje",
                table: "valoracion",
                column: "valoracion_guid",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "cargo_estadia",
                schema: "hospedaje");

            migrationBuilder.DropTable(
                name: "valoracion",
                schema: "hospedaje");

            migrationBuilder.DropTable(
                name: "estadia",
                schema: "hospedaje");
        }
    }
}
