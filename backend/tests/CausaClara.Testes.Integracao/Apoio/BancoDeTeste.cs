using CausaClara.Aplicacao.Comum;
using CausaClara.Dominio.Acesso;
using CausaClara.Dominio.Auditoria;
using CausaClara.Dominio.Comum;
using CausaClara.Dominio.Instituicoes;
using CausaClara.Dominio.Usuarios;
using CausaClara.Persistencia.Convencoes;
using CausaClara.Persistencia.Interceptadores;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace CausaClara.Testes.Integracao.Apoio;

/// <summary>Contexto da requisição mutável, para o teste trocar de usuário entre consultas.</summary>
internal sealed class ContextoDaRequisicaoFalso : IContextoDaRequisicao
{
    public int? UsuarioId { get; set; }
    public int? InstituicaoId { get; set; }
    public TipoUsuario? TipoDeConta { get; set; }
    public bool Autenticado => UsuarioId is not null;
}

/// <summary>
/// Entidade só de teste que declara os três marcadores transversais.
/// </summary>
/// <remarks>
/// Existe porque na etapa 2 ainda não há entidade de negócio que pertença a uma
/// instituição — a primeira nasce na etapa 4. O que está sob teste é a
/// convenção, e testar convenção com uma entidade de fixação é o jeito correto:
/// inventar uma entidade de negócio fora de ordem seria pior.
/// </remarks>
internal sealed class LancamentoDeTeste : IPertenceAInstituicao, IExcluivelLogicamente, IAuditavel
{
    public int Id { get; set; }
    public int InstituicaoId { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public bool Excluido { get; private set; }
    public DateTime? ExcluidoEm { get; private set; }

    public void MarcarComoExcluido()
    {
        Excluido = true;
        ExcluidoEm = DateTime.UtcNow;
    }
}

/// <summary>
/// Contexto de teste que aplica exatamente a mesma convenção de filtros da
/// produção e cumpre o mesmo contrato que os casos de uso enxergam.
/// </summary>
/// <remarks>
/// Implementa <see cref="IContextoDeDados"/> para que os serviços de aplicação
/// rodem aqui sem adaptação. Os mapeamentos são mínimos e por convenção: os de
/// produção usam tipos do PostgreSQL — <c>timestamptz</c>, identidade por
/// coluna — que o SQLite recusa.
/// </remarks>
internal sealed class ContextoDeTeste(
    DbContextOptions<ContextoDeTeste> opcoes,
    IContextoDaRequisicao contextoDaRequisicao) : DbContext(opcoes), IContextoDeDados
{
    public DbSet<LancamentoDeTeste> Lancamentos => Set<LancamentoDeTeste>();

    public DbSet<RegistroDeAuditoria> Auditoria => Set<RegistroDeAuditoria>();

    public DbSet<Usuario> Usuarios => Set<Usuario>();

    public DbSet<Instituicao> Instituicoes => Set<Instituicao>();

    public DbSet<TokenDeAtualizacao> TokensDeAtualizacao => Set<TokenDeAtualizacao>();

    public DbSet<TokenDeRedefinicaoDeSenha> TokensDeRedefinicao => Set<TokenDeRedefinicaoDeSenha>();

    /// <summary>Mesmo contrato que a convenção exige do contexto de produção.</summary>
    internal int? InstituicaoDaRequisicao => contextoDaRequisicao.InstituicaoId;

    public Task<int> SalvarAlteracoesAsync(CancellationToken cancelamento = default)
        => SaveChangesAsync(cancelamento);

    public Task<IDbContextTransaction> IniciarTransacaoAsync(CancellationToken cancelamento = default)
        => Database.BeginTransactionAsync(cancelamento);

    protected override void OnModelCreating(ModelBuilder construtor)
    {
        construtor.Entity<LancamentoDeTeste>().HasKey(l => l.Id);

        construtor.Entity<RegistroDeAuditoria>(entrada =>
        {
            entrada.HasKey(r => r.Id);
            entrada.Property(r => r.Operacao).HasConversion<string>();
        });

        construtor.Entity<Usuario>(entrada =>
        {
            entrada.HasKey(u => u.Id);
            entrada.Property(u => u.Tipo).HasConversion<string>();
            entrada.HasIndex(u => u.Email).IsUnique();
        });

        construtor.Entity<Instituicao>().HasKey(i => i.Id);
        construtor.Entity<TokenDeAtualizacao>().HasKey(t => t.Id);
        construtor.Entity<TokenDeRedefinicaoDeSenha>().HasKey(t => t.Id);

        FiltrosGlobais.Aplicar(construtor, this);
    }
}

/// <summary>
/// Banco SQLite em memória, com os mesmos filtros globais e o mesmo
/// interceptador de auditoria que a aplicação usa.
/// </summary>
/// <remarks>
/// SQLite executa SQL de verdade, então o filtro é exercitado como consulta, e
/// não simulado. O que é específico do PostgreSQL — <c>xmin</c>, <c>timestamptz</c>,
/// restrições CHECK — continua sendo verificado no ambiente do compose.
/// A conexão fica aberta durante toda a vida do objeto porque um banco SQLite
/// em memória deixa de existir quando sua última conexão fecha.
/// </remarks>
internal sealed class BancoDeTeste : IDisposable
{
    private readonly SqliteConnection _conexao;

    public BancoDeTeste()
    {
        _conexao = new SqliteConnection("Filename=:memory:");
        _conexao.Open();

        using var inicial = Abrir();
        inicial.Database.EnsureCreated();
    }

    public ContextoDaRequisicaoFalso Requisicao { get; } = new();

    public ContextoDeTeste Abrir()
    {
        var opcoes = new DbContextOptionsBuilder<ContextoDeTeste>()
            .UseSqlite(_conexao)
            .AddInterceptors(new InterceptadorDeAuditoria(Requisicao))
            .Options;

        return new ContextoDeTeste(opcoes, Requisicao);
    }

    /// <summary>Insere lançamentos sem contexto de usuário, para montar o cenário.</summary>
    public async Task<(int PrimeiroDaUm, int PrimeiroDaDois)> SemearDuasInstituicoesAsync()
    {
        Requisicao.InstituicaoId = null;
        Requisicao.UsuarioId = null;

        await using var contexto = Abrir();

        var daUm = new LancamentoDeTeste { InstituicaoId = 1, Descricao = "Doação da instituição 1" };
        var daDois = new LancamentoDeTeste { InstituicaoId = 2, Descricao = "Doação da instituição 2" };

        contexto.Lancamentos.AddRange(daUm, daDois);
        await contexto.SaveChangesAsync();

        return (daUm.Id, daDois.Id);
    }

    public void Dispose() => _conexao.Dispose();
}
