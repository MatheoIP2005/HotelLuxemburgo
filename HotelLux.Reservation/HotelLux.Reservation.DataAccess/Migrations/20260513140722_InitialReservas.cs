using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace HotelLux.Reservation.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class InitialReservas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "reservas");

            migrationBuilder.CreateTable(
                name: "cliente",
                schema: "reservas",
                columns: table => new
                {
                    id_cliente = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    cliente_guid = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    tipo_identificacion = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    numero_identificacion = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    nombres = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    apellidos = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: true),
                    razon_social = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    correo = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    telefono = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    direccion = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    estado = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false, defaultValue: "ACT"),
                    es_eliminado = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    creado_por_usuario = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    fecha_registro_utc = table.Column<DateTimeOffset>(type: "TIMESTAMPTZ", nullable: false),
                    modificado_por_usuario = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    fecha_modificacion_utc = table.Column<DateTimeOffset>(type: "TIMESTAMPTZ", nullable: true),
                    modificacion_ip = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    fecha_inhabilitacion_utc = table.Column<DateTimeOffset>(type: "TIMESTAMPTZ", nullable: true),
                    motivo_inhabilitacion = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    servicio_origen = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "reservation-service")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cliente", x => x.id_cliente);
                });

            migrationBuilder.CreateTable(
                name: "reserva",
                schema: "reservas",
                columns: table => new
                {
                    id_reserva = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    reserva_guid = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    codigo_reserva = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    id_cliente = table.Column<int>(type: "integer", nullable: false),
                    sucursal_guid = table.Column<Guid>(type: "uuid", nullable: false),
                    fecha_reserva_utc = table.Column<DateTimeOffset>(type: "TIMESTAMPTZ", nullable: false),
                    fecha_inicio = table.Column<DateOnly>(type: "DATE", nullable: false),
                    fecha_fin = table.Column<DateOnly>(type: "DATE", nullable: false),
                    subtotal_reserva = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    valor_iva = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    total_reserva = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    descuento_aplicado = table.Column<decimal>(type: "numeric(12,2)", nullable: false, defaultValue: 0m),
                    saldo_pendiente = table.Column<decimal>(type: "numeric(12,2)", nullable: false, defaultValue: 0m),
                    origen_canal_reserva = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    estado_reserva = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false, defaultValue: "PEN"),
                    fecha_confirmacion_utc = table.Column<DateTimeOffset>(type: "TIMESTAMPTZ", nullable: true),
                    fecha_cancelacion_utc = table.Column<DateTimeOffset>(type: "TIMESTAMPTZ", nullable: true),
                    motivo_cancelacion = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    observaciones = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    es_walkin = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    es_eliminado = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    fecha_inhabilitacion_utc = table.Column<DateTimeOffset>(type: "TIMESTAMPTZ", nullable: true),
                    motivo_inhabilitacion = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    fecha_registro_utc = table.Column<DateTimeOffset>(type: "TIMESTAMPTZ", nullable: false),
                    creado_por_usuario = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    creado_desde_ip = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    modificado_por_usuario = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    fecha_modificacion_utc = table.Column<DateTimeOffset>(type: "TIMESTAMPTZ", nullable: true),
                    modificacion_ip = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    servicio_origen = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "reservation-service")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reserva", x => x.id_reserva);
                    table.ForeignKey(
                        name: "FK_reserva_cliente_id_cliente",
                        column: x => x.id_cliente,
                        principalSchema: "reservas",
                        principalTable: "cliente",
                        principalColumn: "id_cliente",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "reserva_habitacion",
                schema: "reservas",
                columns: table => new
                {
                    id_reserva_habitacion = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    reserva_habitacion_guid = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    id_reserva = table.Column<int>(type: "integer", nullable: false),
                    habitacion_guid = table.Column<Guid>(type: "uuid", nullable: false),
                    tarifa_guid = table.Column<Guid>(type: "uuid", nullable: true),
                    fecha_inicio = table.Column<DateOnly>(type: "DATE", nullable: false),
                    fecha_fin = table.Column<DateOnly>(type: "DATE", nullable: false),
                    num_adultos = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    num_ninos = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    precio_noche_aplicado = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    subtotal_linea = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    valor_iva_linea = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    descuento_linea = table.Column<decimal>(type: "numeric(12,2)", nullable: false, defaultValue: 0m),
                    total_linea = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    estado_detalle = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false, defaultValue: "PEN"),
                    fecha_registro_utc = table.Column<DateTimeOffset>(type: "TIMESTAMPTZ", nullable: false),
                    creado_por_usuario = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    modificado_por_usuario = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    fecha_modificacion_utc = table.Column<DateTimeOffset>(type: "TIMESTAMPTZ", nullable: true),
                    modificacion_ip = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    servicio_origen = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "reservation-service")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reserva_habitacion", x => x.id_reserva_habitacion);
                    table.ForeignKey(
                        name: "FK_reserva_habitacion_reserva_id_reserva",
                        column: x => x.id_reserva,
                        principalSchema: "reservas",
                        principalTable: "reserva",
                        principalColumn: "id_reserva",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_cliente_cliente_guid",
                schema: "reservas",
                table: "cliente",
                column: "cliente_guid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_cliente_correo",
                schema: "reservas",
                table: "cliente",
                column: "correo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_cliente_numero_identificacion",
                schema: "reservas",
                table: "cliente",
                column: "numero_identificacion",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_cliente_tipo_identificacion_numero_identificacion_correo",
                schema: "reservas",
                table: "cliente",
                columns: new[] { "tipo_identificacion", "numero_identificacion", "correo" });

            migrationBuilder.CreateIndex(
                name: "IX_reserva_codigo_reserva",
                schema: "reservas",
                table: "reserva",
                column: "codigo_reserva",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_reserva_id_cliente_estado_reserva",
                schema: "reservas",
                table: "reserva",
                columns: new[] { "id_cliente", "estado_reserva" });

            migrationBuilder.CreateIndex(
                name: "IX_reserva_reserva_guid",
                schema: "reservas",
                table: "reserva",
                column: "reserva_guid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_reserva_sucursal_guid_fecha_inicio_fecha_fin",
                schema: "reservas",
                table: "reserva",
                columns: new[] { "sucursal_guid", "fecha_inicio", "fecha_fin" });

            migrationBuilder.CreateIndex(
                name: "IX_reserva_habitacion_id_reserva_habitacion_guid_fecha_inicio",
                schema: "reservas",
                table: "reserva_habitacion",
                columns: new[] { "id_reserva", "habitacion_guid", "fecha_inicio" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_reserva_habitacion_reserva_habitacion_guid",
                schema: "reservas",
                table: "reserva_habitacion",
                column: "reserva_habitacion_guid",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "reserva_habitacion",
                schema: "reservas");

            migrationBuilder.DropTable(
                name: "reserva",
                schema: "reservas");

            migrationBuilder.DropTable(
                name: "cliente",
                schema: "reservas");
        }
    }
}
