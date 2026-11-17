using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ZelaBairro.Api.Migrations
{
    /// <inheritdoc />
    public partial class BairroTatuape : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "ZB_BAIRRO",
                columns: new[] { "Id", "Cidade", "Nome" },
                values: new object[] { 6, "São Paulo", "Tatuapé" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ZB_BAIRRO",
                keyColumn: "Id",
                keyValue: 6);
        }
    }
}
