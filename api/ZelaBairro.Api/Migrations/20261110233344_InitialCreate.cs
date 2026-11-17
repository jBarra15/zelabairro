using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ZelaBairro.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ZB_BAIRRO",
                columns: table => new
                {
                    Id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    Nome = table.Column<string>(type: "NVARCHAR2(80)", maxLength: 80, nullable: false),
                    Cidade = table.Column<string>(type: "NVARCHAR2(80)", maxLength: 80, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ZB_BAIRRO", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ZB_CATEGORIA",
                columns: table => new
                {
                    Id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    Nome = table.Column<string>(type: "NVARCHAR2(60)", maxLength: 60, nullable: false),
                    Descricao = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: false),
                    PrazoAtendimentoDias = table.Column<int>(type: "NUMBER(10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ZB_CATEGORIA", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ZB_MORADOR",
                columns: table => new
                {
                    Id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    Nome = table.Column<string>(type: "NVARCHAR2(80)", maxLength: 80, nullable: false),
                    Email = table.Column<string>(type: "NVARCHAR2(120)", maxLength: 120, nullable: false),
                    BairroId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ZB_MORADOR", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ZB_MORADOR_ZB_BAIRRO_BairroId",
                        column: x => x.BairroId,
                        principalTable: "ZB_BAIRRO",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ZB_OCORRENCIA",
                columns: table => new
                {
                    Id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    Titulo = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: false),
                    Descricao = table.Column<string>(type: "NVARCHAR2(500)", maxLength: 500, nullable: false),
                    CategoriaId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    BairroId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    MoradorId = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    Latitude = table.Column<double>(type: "BINARY_DOUBLE", nullable: false),
                    Longitude = table.Column<double>(type: "BINARY_DOUBLE", nullable: false),
                    Endereco = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: true),
                    FotoArquivo = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: true),
                    Status = table.Column<string>(type: "NVARCHAR2(20)", maxLength: 20, nullable: false),
                    Prioridade = table.Column<string>(type: "NVARCHAR2(20)", maxLength: 20, nullable: false),
                    Origem = table.Column<string>(type: "NVARCHAR2(20)", maxLength: 20, nullable: false),
                    Confirmacoes = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    CriadaEm = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    AtualizadaEm = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    ResolvidaEm = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ZB_OCORRENCIA", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ZB_OCORRENCIA_ZB_BAIRRO_BairroId",
                        column: x => x.BairroId,
                        principalTable: "ZB_BAIRRO",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ZB_OCORRENCIA_ZB_CATEGORIA_CategoriaId",
                        column: x => x.CategoriaId,
                        principalTable: "ZB_CATEGORIA",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ZB_OCORRENCIA_ZB_MORADOR_MoradorId",
                        column: x => x.MoradorId,
                        principalTable: "ZB_MORADOR",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.InsertData(
                table: "ZB_BAIRRO",
                columns: new[] { "Id", "Cidade", "Nome" },
                values: new object[,]
                {
                    { 1, "São Paulo", "Bela Vista" },
                    { 2, "São Paulo", "Consolação" },
                    { 3, "São Paulo", "Liberdade" },
                    { 4, "São Paulo", "Paraíso" },
                    { 5, "São Paulo", "Vila Mariana" }
                });

            migrationBuilder.InsertData(
                table: "ZB_CATEGORIA",
                columns: new[] { "Id", "Descricao", "Nome", "PrazoAtendimentoDias" },
                values: new object[,]
                {
                    { 1, "Entulho, lixo acumulado, lixeira cheia", "Lixo e descarte irregular", 3 },
                    { 2, "Poste apagado ou piscando", "Iluminação pública", 5 },
                    { 3, "Buracos e afundamentos no asfalto", "Buraco na via", 10 },
                    { 4, "Calçada quebrada, sem rampa ou obstruída", "Calçada danificada", 15 },
                    { 5, "Galho caindo, árvore com risco de queda", "Árvore e poda", 7 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ZB_CATEGORIA_Nome",
                table: "ZB_CATEGORIA",
                column: "Nome",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ZB_MORADOR_BairroId",
                table: "ZB_MORADOR",
                column: "BairroId");

            migrationBuilder.CreateIndex(
                name: "IX_ZB_MORADOR_Email",
                table: "ZB_MORADOR",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ZB_OCORRENCIA_BairroId",
                table: "ZB_OCORRENCIA",
                column: "BairroId");

            migrationBuilder.CreateIndex(
                name: "IX_ZB_OCORRENCIA_CategoriaId",
                table: "ZB_OCORRENCIA",
                column: "CategoriaId");

            migrationBuilder.CreateIndex(
                name: "IX_ZB_OCORRENCIA_MoradorId",
                table: "ZB_OCORRENCIA",
                column: "MoradorId");

            migrationBuilder.CreateIndex(
                name: "IX_ZB_OCORRENCIA_Status",
                table: "ZB_OCORRENCIA",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ZB_OCORRENCIA");

            migrationBuilder.DropTable(
                name: "ZB_CATEGORIA");

            migrationBuilder.DropTable(
                name: "ZB_MORADOR");

            migrationBuilder.DropTable(
                name: "ZB_BAIRRO");
        }
    }
}
