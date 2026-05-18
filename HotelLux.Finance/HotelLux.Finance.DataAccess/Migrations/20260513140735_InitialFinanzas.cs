using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace HotelLux.Finance.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class InitialFinanzas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "finanzas");

            migrationBuilder.CreateTable(
                name: "factura",
                schema: "finanzas",
                columns: table => new
                {
                    id_factura = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    factura_guid = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    cliente_guid = table.Column<Guid>(type: "uuid", nullable: false),
                    reserva_guid = table.Column<Guid>(type: "uuid", nullable: false),
                    sucursal_guid = table.Column<Guid>(type: "uuid", nullable: false),
                    numero_factura = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    tipo_factura = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "RESERVA"),
                    fecha_emision = table.Column<DateTimeOffset>(type: "TIMESTAMPTZ", nullable: false),
                    subtotal = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    valor_iva = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    descuento_total = table.Column<decimal>(type: "numeric(12,2)", nullable: false, defaultValue: 0m),
                    total = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    saldo_pendiente = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    moneda = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, defaultValue: "USD"),
                    observaciones_factura = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    origen_canal_factura = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    estado = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false, defaultValue: "EMI"),
                    fecha_inhabilitacion_utc = table.Column<DateTimeOffset>(type: "TIMESTAMPTZ", nullable: true),
                    es_eliminado = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    creado_por_usuario = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    fecha_registro_utc = table.Column<DateTimeOffset>(type: "TIMESTAMPTZ", nullable: false),
                    modificado_por_usuario = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    fecha_modificacion_utc = table.Column<DateTimeOffset>(type: "TIMESTAMPTZ", nullable: true),
                    modificacion_ip = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    servicio_origen = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "finance-service"),
                    motivo_inhabilitacion = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_factura", x => x.id_factura);
                });

            migrationBuilder.CreateTable(
                name: "factura_detalle",
                schema: "finanzas",
                columns: table => new
                {
                    id_factura_detalle = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    factura_detalle_guid = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    id_factura = table.Column<int>(type: "integer", nullable: false),
                    tipo_item = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    referencia_tipo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    referencia_guid = table.Column<Guid>(type: "uuid", nullable: true),
                    descripcion_item = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    cantidad = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    precio_unitario = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    subtotal_linea = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    valor_iva_linea = table.Column<decimal>(type: "numeric(12,2)", nullable: false, defaultValue: 0m),
                    descuento_linea = table.Column<decimal>(type: "numeric(12,2)", nullable: false, defaultValue: 0m),
                    total_linea = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    fecha_registro_utc = table.Column<DateTimeOffset>(type: "TIMESTAMPTZ", nullable: false),
                    creado_por_usuario = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_factura_detalle", x => x.id_factura_detalle);
                    table.ForeignKey(
                        name: "FK_factura_detalle_factura_id_factura",
                        column: x => x.id_factura,
                        principalSchema: "finanzas",
                        principalTable: "factura",
                        principalColumn: "id_factura",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "pago",
                schema: "finanzas",
                columns: table => new
                {
                    id_pago = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    pago_guid = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    id_factura = table.Column<int>(type: "integer", nullable: false),
                    reserva_guid = table.Column<Guid>(type: "uuid", nullable: false),
                    monto = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    metodo_pago = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    es_pago_electronico = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    proveedor_pasarela = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    transaccion_externa = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    codigo_autorizacion = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    referencia = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    estado_pago = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false, defaultValue: "PEN"),
                    fecha_pago_utc = table.Column<DateTimeOffset>(type: "TIMESTAMPTZ", nullable: false),
                    moneda = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, defaultValue: "USD"),
                    tipo_cambio = table.Column<decimal>(type: "numeric(10,4)", nullable: false, defaultValue: 1m),
                    respuesta_pasarela = table.Column<string>(type: "text", nullable: true),
                    creado_por_usuario = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    fecha_registro_utc = table.Column<DateTimeOffset>(type: "TIMESTAMPTZ", nullable: false),
                    modificado_por_usuario = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    fecha_modificacion_utc = table.Column<DateTimeOffset>(type: "TIMESTAMPTZ", nullable: true),
                    modificacion_ip = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    servicio_origen = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "finance-service")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pago", x => x.id_pago);
                    table.ForeignKey(
                        name: "FK_pago_factura_id_factura",
                        column: x => x.id_factura,
                        principalSchema: "finanzas",
                        principalTable: "factura",
                        principalColumn: "id_factura",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_factura_cliente_guid",
                schema: "finanzas",
                table: "factura",
                column: "cliente_guid");

            migrationBuilder.CreateIndex(
                name: "IX_factura_factura_guid",
                schema: "finanzas",
                table: "factura",
                column: "factura_guid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_factura_numero_factura",
                schema: "finanzas",
                table: "factura",
                column: "numero_factura",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_factura_reserva_guid_estado_fecha_emision",
                schema: "finanzas",
                table: "factura",
                columns: new[] { "reserva_guid", "estado", "fecha_emision" });

            migrationBuilder.CreateIndex(
                name: "IX_factura_sucursal_guid",
                schema: "finanzas",
                table: "factura",
                column: "sucursal_guid");

            migrationBuilder.CreateIndex(
                name: "IX_factura_detalle_factura_detalle_guid",
                schema: "finanzas",
                table: "factura_detalle",
                column: "factura_detalle_guid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_factura_detalle_id_factura",
                schema: "finanzas",
                table: "factura_detalle",
                column: "id_factura");

            migrationBuilder.CreateIndex(
                name: "IX_pago_id_factura_estado_pago_fecha_pago_utc",
                schema: "finanzas",
                table: "pago",
                columns: new[] { "id_factura", "estado_pago", "fecha_pago_utc" });

            migrationBuilder.CreateIndex(
                name: "IX_pago_pago_guid",
                schema: "finanzas",
                table: "pago",
                column: "pago_guid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_pago_reserva_guid",
                schema: "finanzas",
                table: "pago",
                column: "reserva_guid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "factura_detalle",
                schema: "finanzas");

            migrationBuilder.DropTable(
                name: "pago",
                schema: "finanzas");

            migrationBuilder.DropTable(
                name: "factura",
                schema: "finanzas");
        }
    }
}
