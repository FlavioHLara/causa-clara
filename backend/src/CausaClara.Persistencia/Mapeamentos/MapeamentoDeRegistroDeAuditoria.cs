using CausaClara.Dominio.Auditoria;
using CausaClara.Dominio.Instituicoes;
using CausaClara.Dominio.Usuarios;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CausaClara.Persistencia.Mapeamentos;

/// <summary>Mapeamento da tabela <c>auditoria</c> (RF81).</summary>
public sealed class MapeamentoDeRegistroDeAuditoria : IEntityTypeConfiguration<RegistroDeAuditoria>
{
    public void Configure(EntityTypeBuilder<RegistroDeAuditoria> construtor)
    {
        construtor.ToTable("auditoria", tabela => tabela.HasCheckConstraint(
            "ck_auditoria_operacao",
            "operacao IN ('Criacao', 'Alteracao', 'Exclusao')"));

        construtor.HasKey(r => r.Id);

        // bigint: a trilha cresce com cada lançamento e nunca é podada.
        construtor.Property(r => r.Id).UseIdentityByDefaultColumn();

        construtor.Property(r => r.Operacao)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        construtor.Property(r => r.Entidade)
            .IsRequired()
            .HasMaxLength(RegistroDeAuditoria.TamanhoMaximoEntidade);

        construtor.Property(r => r.ChaveDoRegistro)
            .IsRequired()
            .HasMaxLength(RegistroDeAuditoria.TamanhoMaximoChave);

        construtor.Property(r => r.Descricao)
            .HasMaxLength(RegistroDeAuditoria.TamanhoMaximoDescricao);

        construtor.Property(r => r.OcorridoEm)
            .IsRequired()
            .HasColumnType("timestamp with time zone");   // RNF28

        // RNF27: integridade referencial declarada no banco.
        //
        // OnDelete(SetNull) no autor serve ao RNF21: anonimizar o titular
        // esvazia a referência sem apagar a entrada da trilha. Restrict apagaria
        // a possibilidade de excluir o usuário; Cascade apagaria a trilha junto,
        // que é exatamente o que a prestação de contas não pode perder.
        construtor.HasOne<Usuario>()
            .WithMany()
            .HasForeignKey(r => r.UsuarioId)
            .OnDelete(DeleteBehavior.SetNull);

        construtor.HasOne<Instituicao>()
            .WithMany()
            .HasForeignKey(r => r.InstituicaoId)
            .OnDelete(DeleteBehavior.Restrict);

        // T17 lista a trilha por instituição e período, da mais recente para a
        // mais antiga; o índice acompanha exatamente essa consulta.
        construtor.HasIndex(r => new { r.InstituicaoId, r.OcorridoEm })
            .HasDatabaseName("ix_auditoria_instituicao_ocorrido_em")
            .IsDescending(false, true);
    }
}
