using CausaClara.Dominio.Acesso;
using CausaClara.Dominio.Auditoria;
using CausaClara.Dominio.Instituicoes;
using CausaClara.Dominio.Usuarios;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace CausaClara.Aplicacao.Comum;

/// <summary>
/// Contrato de persistência que a camada de Aplicação enxerga (RNF32).
/// </summary>
/// <remarks>
/// <para>
/// Não há repositório por entidade, por decisão registrada em
/// <c>docs/05-arquitetura.md</c> seção 3: o <c>DbContext</c> já cumpre os papéis
/// de Unit of Work e de Repository, e envolvê-lo em classes repassadoras
/// acrescentaria código sem acrescentar capacidade.
/// </para>
/// <para>
/// O conjunto de <c>DbSet</c> cresce a cada etapa da ordem de construção
/// (seção 14 do mesmo documento). Hoje contém apenas o que a etapa 1 criou.
/// </para>
/// </remarks>
public interface IContextoDeDados
{
    DbSet<Usuario> Usuarios { get; }

    DbSet<Instituicao> Instituicoes { get; }

    /// <summary>Tokens de renovação de sessão (RF06, RF08).</summary>
    DbSet<TokenDeAtualizacao> TokensDeAtualizacao { get; }

    /// <summary>Tokens de redefinição de senha (RF09).</summary>
    DbSet<TokenDeRedefinicaoDeSenha> TokensDeRedefinicao { get; }

    /// <summary>
    /// Trilha de auditoria (RF81). Somente leitura na prática: quem escreve é o
    /// interceptador de persistência, não os casos de uso.
    /// </summary>
    DbSet<RegistroDeAuditoria> Auditoria { get; }

    /// <summary>Confirma as alterações rastreadas.</summary>
    Task<int> SalvarAlteracoesAsync(CancellationToken cancelamento = default);

    /// <summary>
    /// Abre transação explícita, para as operações que alteram mais de um
    /// registro e precisam de atomicidade (RNF25, RNF26).
    /// </summary>
    Task<IDbContextTransaction> IniciarTransacaoAsync(CancellationToken cancelamento = default);
}
