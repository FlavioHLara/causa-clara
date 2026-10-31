using CausaClara.Dominio.Usuarios;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CausaClara.Persistencia.Mapeamentos;

/// <summary>Mapeamento da tabela <c>usuarios</c>.</summary>
/// <remarks>
/// Os nomes de coluna saem da convenção snake_case aplicada em
/// <see cref="Extensoes.ConfiguracaoDePersistencia"/>: <c>NomeCompleto</c>
/// vira <c>nome_completo</c>. Aqui só entra o que a convenção não decide.
/// </remarks>
public sealed class MapeamentoDeUsuario : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> construtor)
    {
        construtor.ToTable("usuarios", tabela => tabela.HasCheckConstraint(
            "ck_usuarios_tipo",
            // RNF27 e decisão de persistência: enum como texto com CHECK, para que a
            // integridade não dependa da aplicação. Acrescentar membro ao enum
            // TipoUsuario exige migração que altere esta restrição.
            "tipo IN ('Apoiador', 'Gestor')"));

        construtor.HasKey(u => u.Id);

        construtor.Property(u => u.NomeCompleto)
            .IsRequired()
            .HasMaxLength(Usuario.TamanhoMaximoNome);

        construtor.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(Usuario.TamanhoMaximoEmail);

        construtor.Property(u => u.SenhaHash)
            .IsRequired()
            .HasMaxLength(Usuario.TamanhoMaximoSenhaHash);

        construtor.Property(u => u.Telefone)
            .HasMaxLength(Usuario.TamanhoMaximoTelefone);

        construtor.Property(u => u.Tipo)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        construtor.Property(u => u.CriadoEm)
            .IsRequired()
            .HasColumnType("timestamp with time zone");   // RNF28

        construtor.Property(u => u.AtualizadoEm)
            .IsRequired()
            .HasColumnType("timestamp with time zone");   // RNF28

        // RF04: um e-mail, um usuário. A garantia é do banco, não de uma consulta
        // prévia na aplicação, que teria janela de corrida entre a checagem e o insert.
        construtor.HasIndex(u => u.Email)
            .IsUnique()
            .HasDatabaseName("ux_usuarios_email");
    }
}
