using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace HotelLux.Audit.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class InitialAuditoria : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "auditoria");

            migrationBuilder.CreateTable(
                name: "evento_auditoria",
                schema: "auditoria",
                columns: table => new
                {
                    id_auditoria = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    auditoria_guid = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    tabla_afectada = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    operacion = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    entidad_guid = table.Column<Guid>(type: "uuid", nullable: true),
                    id_registro_afectado = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    datos_anteriores = table.Column<string>(type: "jsonb", nullable: true),
                    datos_nuevos = table.Column<string>(type: "jsonb", nullable: true),
                    usuario_ejecutor = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    usuario_guid = table.Column<Guid>(type: "uuid", nullable: true),
                    ip_origen = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    servicio_origen = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    fecha_evento_utc = table.Column<DateTimeOffset>(type: "TIMESTAMPTZ", nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_evento_auditoria", x => x.id_auditoria);
                });

            migrationBuilder.CreateIndex(
                name: "IX_evento_auditoria_entidad_guid",
                schema: "auditoria",
                table: "evento_auditoria",
                column: "entidad_guid");

            migrationBuilder.CreateIndex(
                name: "IX_evento_auditoria_fecha_evento_utc",
                schema: "auditoria",
                table: "evento_auditoria",
                column: "fecha_evento_utc");

            migrationBuilder.CreateIndex(
                name: "IX_evento_auditoria_servicio_origen_fecha_evento_utc",
                schema: "auditoria",
                table: "evento_auditoria",
                columns: new[] { "servicio_origen", "fecha_evento_utc" });

            migrationBuilder.CreateIndex(
                name: "IX_evento_auditoria_tabla_afectada_fecha_evento_utc",
                schema: "auditoria",
                table: "evento_auditoria",
                columns: new[] { "tabla_afectada", "fecha_evento_utc" });

            migrationBuilder.CreateIndex(
                name: "IX_evento_auditoria_usuario_ejecutor_fecha_evento_utc",
                schema: "auditoria",
                table: "evento_auditoria",
                columns: new[] { "usuario_ejecutor", "fecha_evento_utc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "evento_auditoria",
                schema: "auditoria");
        }
    }
}
