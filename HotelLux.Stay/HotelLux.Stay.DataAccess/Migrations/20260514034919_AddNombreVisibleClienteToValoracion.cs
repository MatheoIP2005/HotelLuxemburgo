using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HotelLux.Stay.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddNombreVisibleClienteToValoracion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "nombre_visible_cliente",
                schema: "hospedaje",
                table: "valoracion",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "nombre_visible_cliente",
                schema: "hospedaje",
                table: "valoracion");
        }
    }
}
