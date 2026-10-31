using CausaClara.Dominio.Instituicoes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CausaClara.Persistencia.Mapeamentos;

/// <summary>Mapeamento da tabela <c>instituicoes</c>.</summary>
public sealed class MapeamentoDeInstituicao : IEntityTypeConfiguration<Instituicao>
{
    public void Configure(EntityTypeBuilder<Instituicao> construtor)
    {
        construtor.ToTable("instituicoes");

        construtor.HasKey(i => i.Id);

        construtor.Property(i => i.Nome)
            .IsRequired()
            .HasMaxLength(Instituicao.TamanhoMaximoNome);

        construtor.Property(i => i.Descricao)
            .IsRequired()
            .HasMaxLength(Instituicao.TamanhoMaximoDescricao);

        construtor.Property(i => i.Cidade)
            .IsRequired()
            .HasMaxLength(Instituicao.TamanhoMaximoCidade);

        construtor.Property(i => i.Estado)
            .IsRequired()
            .HasMaxLength(Instituicao.TamanhoUf)
            .IsFixedLength();

        construtor.Property(i => i.EmailContato)
            .IsRequired()
            .HasMaxLength(Instituicao.TamanhoMaximoEmail);

        construtor.Property(i => i.Telefone)
            .IsRequired()
            .HasMaxLength(Instituicao.TamanhoMaximoTelefone);

        construtor.Property(i => i.Site)
            .IsRequired()
            .HasMaxLength(Instituicao.TamanhoMaximoSite);

        construtor.Property(i => i.CriadoEm)
            .IsRequired()
            .HasColumnType("timestamp with time zone");   // RNF28

        construtor.Property(i => i.AtualizadoEm)
            .IsRequired()
            .HasColumnType("timestamp with time zone");   // RNF28

        // RF13: nome de instituição é único no sistema.
        construtor.HasIndex(i => i.Nome)
            .IsUnique()
            .HasDatabaseName("ux_instituicoes_nome");

        // RF26 lista as instituições publicamente com busca e ordenação por
        // localidade; o índice acompanha a consulta que a tela faz.
        construtor.HasIndex(i => new { i.Estado, i.Cidade })
            .HasDatabaseName("ix_instituicoes_estado_cidade");
    }
}
