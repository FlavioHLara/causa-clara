using System.Reflection;
using CausaClara.Aplicacao.Comum;
using CausaClara.Dominio.Acesso;
using CausaClara.Dominio.Auditoria;
using CausaClara.Dominio.Instituicoes;
using CausaClara.Dominio.Usuarios;
using CausaClara.Persistencia.Convencoes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace CausaClara.Persistencia;

/// <summary>
/// Contexto do EF Core. Implementa <see cref="IContextoDeDados"/>, que é o que
/// a camada de Aplicação conhece — ela nunca depende desta classe concreta.
/// </summary>
/// <param name="contextoDaRequisicao">
/// Quem está requisitando. Alimenta o filtro global por instituição (RNF12) e
/// precisa ter o mesmo escopo deste contexto: ambos são registrados por
/// requisição, de modo que o filtro leia a instituição certa.
/// </param>
public sealed class ContextoDeDados(
    DbContextOptions<ContextoDeDados> opcoes,
    IContextoDaRequisicao contextoDaRequisicao)
    : DbContext(opcoes), IContextoDeDados
{
    public DbSet<Usuario> Usuarios => Set<Usuario>();

    public DbSet<Instituicao> Instituicoes => Set<Instituicao>();

    public DbSet<RegistroDeAuditoria> Auditoria => Set<RegistroDeAuditoria>();

    public DbSet<TokenDeAtualizacao> TokensDeAtualizacao => Set<TokenDeAtualizacao>();

    public DbSet<TokenDeRedefinicaoDeSenha> TokensDeRedefinicao => Set<TokenDeRedefinicaoDeSenha>();

    public Task<int> SalvarAlteracoesAsync(CancellationToken cancelamento = default)
        => SaveChangesAsync(cancelamento);

    public Task<IDbContextTransaction> IniciarTransacaoAsync(CancellationToken cancelamento = default)
        => Database.BeginTransactionAsync(cancelamento);

    /// <summary>
    /// Instituição do usuário da requisição, lida pelo filtro global.
    /// </summary>
    /// <remarks>
    /// Existe como membro do contexto, e não como acesso direto ao serviço, por
    /// uma razão específica do EF Core: só um membro de instância do
    /// <c>DbContext</c> é transformado em parâmetro da consulta e reavaliado a
    /// cada execução. Ver <see cref="FiltrosGlobais"/>.
    /// </remarks>
    internal int? InstituicaoDaRequisicao => contextoDaRequisicao.InstituicaoId;

    protected override void OnModelCreating(ModelBuilder construtor)
    {
        // Um arquivo de mapeamento por entidade, em Mapeamentos/.
        construtor.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        // Depois dos mapeamentos: a convenção precisa enxergar todas as
        // entidades já registradas no modelo para decidir em quais aplicar.
        FiltrosGlobais.Aplicar(construtor, this);

        base.OnModelCreating(construtor);
    }
}
