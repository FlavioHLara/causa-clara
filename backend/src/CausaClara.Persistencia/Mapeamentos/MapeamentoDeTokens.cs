using CausaClara.Dominio.Acesso;
using CausaClara.Dominio.Usuarios;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CausaClara.Persistencia.Mapeamentos;

/// <summary>Mapeamento da tabela <c>tokens_de_atualizacao</c> (RF06, RF08).</summary>
public sealed class MapeamentoDeTokenDeAtualizacao : IEntityTypeConfiguration<TokenDeAtualizacao>
{
    public void Configure(EntityTypeBuilder<TokenDeAtualizacao> construtor)
    {
        construtor.ToTable("tokens_de_atualizacao");

        construtor.HasKey(t => t.Id);
        construtor.Property(t => t.Id).UseIdentityByDefaultColumn();

        construtor.Property(t => t.Resumo)
            .IsRequired()
            .HasMaxLength(TokenDeAtualizacao.TamanhoDoResumo)
            .IsFixedLength();

        construtor.Property(t => t.CriadoEm).IsRequired().HasColumnType("timestamp with time zone");
        construtor.Property(t => t.ExpiraEm).IsRequired().HasColumnType("timestamp with time zone");
        construtor.Property(t => t.RevogadoEm).HasColumnType("timestamp with time zone");

        // RNF27. Cascade: tokens de sessão não têm valor histórico — sumir com o
        // usuário é o comportamento certo, ao contrário da trilha de auditoria.
        construtor.HasOne<Usuario>()
            .WithMany()
            .HasForeignKey(t => t.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);

        // Toda renovação procura por este resumo: é a consulta mais frequente
        // do módulo de acesso. Único porque dois segredos distintos não podem
        // produzir o mesmo resumo sem que algo esteja errado.
        construtor.HasIndex(t => t.Resumo)
            .IsUnique()
            .HasDatabaseName("ux_tokens_de_atualizacao_resumo");

        // Revogar todas as sessões de um usuário (RF09, e RF23 na etapa 4).
        construtor.HasIndex(t => new { t.UsuarioId, t.RevogadoEm })
            .HasDatabaseName("ix_tokens_de_atualizacao_usuario");
    }
}

/// <summary>Mapeamento da tabela <c>tokens_de_redefinicao</c> (RF09).</summary>
public sealed class MapeamentoDeTokenDeRedefinicao : IEntityTypeConfiguration<TokenDeRedefinicaoDeSenha>
{
    public void Configure(EntityTypeBuilder<TokenDeRedefinicaoDeSenha> construtor)
    {
        construtor.ToTable("tokens_de_redefinicao");

        construtor.HasKey(t => t.Id);
        construtor.Property(t => t.Id).UseIdentityByDefaultColumn();

        construtor.Property(t => t.Resumo)
            .IsRequired()
            .HasMaxLength(TokenDeRedefinicaoDeSenha.TamanhoDoResumo)
            .IsFixedLength();

        construtor.Property(t => t.CriadoEm).IsRequired().HasColumnType("timestamp with time zone");
        construtor.Property(t => t.ExpiraEm).IsRequired().HasColumnType("timestamp with time zone");
        construtor.Property(t => t.UsadoEm).HasColumnType("timestamp with time zone");

        construtor.HasOne<Usuario>()
            .WithMany()
            .HasForeignKey(t => t.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);

        construtor.HasIndex(t => t.Resumo)
            .IsUnique()
            .HasDatabaseName("ux_tokens_de_redefinicao_resumo");
    }
}
