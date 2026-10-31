using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CausaClara.Persistencia.Migrations
{
    /// <inheritdoc />
    public partial class ModuloDeAcesso : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tokens_de_atualizacao",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    usuario_id = table.Column<int>(type: "integer", nullable: false),
                    resumo = table.Column<string>(type: "character(64)", fixedLength: true, maxLength: 64, nullable: false),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    expira_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    revogado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tokens_de_atualizacao", x => x.id);
                    table.ForeignKey(
                        name: "fk_tokens_de_atualizacao_usuarios_usuario_id",
                        column: x => x.usuario_id,
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tokens_de_redefinicao",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    usuario_id = table.Column<int>(type: "integer", nullable: false),
                    resumo = table.Column<string>(type: "character(64)", fixedLength: true, maxLength: 64, nullable: false),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    expira_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    usado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tokens_de_redefinicao", x => x.id);
                    table.ForeignKey(
                        name: "fk_tokens_de_redefinicao_usuarios_usuario_id",
                        column: x => x.usuario_id,
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_tokens_de_atualizacao_usuario",
                table: "tokens_de_atualizacao",
                columns: new[] { "usuario_id", "revogado_em" });

            migrationBuilder.CreateIndex(
                name: "ux_tokens_de_atualizacao_resumo",
                table: "tokens_de_atualizacao",
                column: "resumo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_tokens_de_redefinicao_usuario_id",
                table: "tokens_de_redefinicao",
                column: "usuario_id");

            migrationBuilder.CreateIndex(
                name: "ux_tokens_de_redefinicao_resumo",
                table: "tokens_de_redefinicao",
                column: "resumo",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tokens_de_atualizacao");

            migrationBuilder.DropTable(
                name: "tokens_de_redefinicao");
        }
    }
}
