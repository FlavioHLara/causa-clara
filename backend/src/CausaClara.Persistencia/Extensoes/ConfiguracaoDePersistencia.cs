using CausaClara.Aplicacao.Comum;
using CausaClara.Persistencia.Interceptadores;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CausaClara.Persistencia.Extensoes;

/// <summary>
/// Ponto único de contato entre a Api e a Persistência.
/// </summary>
/// <remarks>
/// A Api referencia este projeto apenas para chamar
/// <see cref="AdicionarPersistencia"/> no composition root. Nenhum controller
/// enxerga EF Core, Npgsql ou <see cref="ContextoDeDados"/> — fronteira do RNF32,
/// verificada por <c>CausaClara.Testes.Arquitetura</c>.
/// </remarks>
public static class ConfiguracaoDePersistencia
{
    /// <summary>Nome da tabela de histórico de migrações (RNF35).</summary>
    public const string TabelaDeHistorico = "__historico_de_migracoes";

    /// <summary>Nome do verificador de saúde do banco, exposto em <c>/saude/pronto</c>.</summary>
    public const string VerificadorDoBanco = "banco";

    /// <summary>
    /// Registra o contexto, o contrato de dados, o interceptador de auditoria e
    /// a verificação de saúde do banco.
    /// </summary>
    /// <remarks>
    /// <b>Quem chama precisa registrar <see cref="IContextoDaRequisicao"/>.</b>
    /// Ele não tem implementação padrão aqui de propósito: resolver o usuário da
    /// requisição depende de HTTP, que é assunto da Api. Esquecer esse registro
    /// quebra na primeira resolução do contexto, e o teste de composição pega.
    /// </remarks>
    public static IServiceCollection AdicionarPersistencia(
        this IServiceCollection servicos,
        string stringDeConexao)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(stringDeConexao);

        servicos.AddScoped<InterceptadorDeAuditoria>();

        servicos.AddDbContext<ContextoDeDados>((provedor, opcoes) =>
        {
            ConfigurarContexto(opcoes, stringDeConexao);

            // Resolvido pelo provedor com escopo da requisição, e não construído
            // aqui, para que o interceptador enxergue o mesmo contexto de
            // requisição que o filtro global usa.
            opcoes.AddInterceptors(provedor.GetRequiredService<InterceptadorDeAuditoria>());
        });

        // A Aplicação depende da interface; quem resolve o concreto é só o container.
        servicos.AddScoped<IContextoDeDados>(provedor => provedor.GetRequiredService<ContextoDeDados>());

        // Registrado aqui, e não em Program.cs, para que a Api não precise
        // conhecer o tipo do contexto nem referenciar EF Core.
        servicos.AddHealthChecks().AddDbContextCheck<ContextoDeDados>(VerificadorDoBanco);

        return servicos;
    }

    /// <summary>
    /// Aplica as migrações pendentes (RNF35).
    /// </summary>
    /// <remarks>
    /// Existe aqui, e não em <c>Program.cs</c>, para que a Api não precise
    /// referenciar EF Core — fronteira do RNF32, verificada por teste. O chamador
    /// decide quando usar: no compose, a Api aplica na subida, para que
    /// <c>docker compose up</c> entregue um ambiente pronto (RNF33). Em produção
    /// com mais de uma instância, migrar na inicialização deixa de ser adequado,
    /// e o passo vira uma etapa própria do pipeline de publicação.
    /// </remarks>
    public static async Task AplicarMigracoesPendentesAsync(
        this IServiceProvider provedor,
        CancellationToken cancelamento = default)
    {
        await using var escopo = provedor.CreateAsyncScope();
        var contexto = escopo.ServiceProvider.GetRequiredService<ContextoDeDados>();
        await contexto.Database.MigrateAsync(cancelamento);
    }

    /// <summary>
    /// Configuração compartilhada entre o host da aplicação e a fábrica de
    /// tempo de projeto, para que a migração gerada corresponda ao que roda.
    /// </summary>
    internal static void ConfigurarContexto(DbContextOptionsBuilder opcoes, string stringDeConexao)
        => opcoes
            .UseNpgsql(stringDeConexao, npgsql => npgsql
                .MigrationsHistoryTable(TabelaDeHistorico)
                .MigrationsAssembly(typeof(ConfiguracaoDePersistencia).Assembly.FullName))
            // AGENTS.md seção 3: tabelas e colunas em snake_case português.
            // NomeCompleto -> nome_completo, sem HasColumnName em cada propriedade.
            .UseSnakeCaseNamingConvention();
}
