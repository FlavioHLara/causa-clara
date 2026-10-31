using CausaClara.Aplicacao.Acesso;
using Microsoft.Extensions.DependencyInjection;

namespace CausaClara.Aplicacao.Extensoes;

/// <summary>Registra os casos de uso. Ponto único de contato da Api com a Aplicação.</summary>
public static class ConfiguracaoDeAplicacao
{
    public static IServiceCollection AdicionarAplicacao(
        this IServiceCollection servicos,
        OpcoesDeAcesso opcoesDeAcesso)
    {
        ArgumentNullException.ThrowIfNull(opcoesDeAcesso);

        // Objeto simples e imutável na prática: a Aplicação recebe os prazos já
        // resolvidos, sem conhecer configuração nem variáveis de ambiente.
        servicos.AddSingleton(opcoesDeAcesso);

        servicos.AddScoped<ServicoDeCadastro>();
        servicos.AddScoped<ServicoDeAutenticacao>();
        servicos.AddScoped<ServicoDePerfil>();
        servicos.AddScoped<ServicoDeRedefinicaoDeSenha>();

        return servicos;
    }
}
