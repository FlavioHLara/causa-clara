using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CausaClara.Persistencia.Migrations
{
    /// <inheritdoc />
    public partial class TrilhaDeAuditoria : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "auditoria",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    operacao = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    entidade = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    chave_do_registro = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    descricao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    usuario_id = table.Column<int>(type: "integer", nullable: true),
                    instituicao_id = table.Column<int>(type: "integer", nullable: true),
                    ocorrido_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_auditoria", x => x.id);
                    table.CheckConstraint("ck_auditoria_operacao", "operacao IN ('Criacao', 'Alteracao', 'Exclusao')");
                    table.ForeignKey(
                        name: "fk_auditoria_instituicoes_instituicao_id",
                        column: x => x.instituicao_id,
                        principalTable: "instituicoes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_auditoria_usuarios_usuario_id",
                        column: x => x.usuario_id,
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "ix_auditoria_instituicao_ocorrido_em",
                table: "auditoria",
                columns: new[] { "instituicao_id", "ocorrido_em" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "ix_auditoria_usuario_id",
                table: "auditoria",
                column: "usuario_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "auditoria");
        }
    }
}
