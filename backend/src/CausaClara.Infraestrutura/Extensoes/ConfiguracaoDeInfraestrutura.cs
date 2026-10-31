using CausaClara.Aplicacao.Acesso;
using CausaClara.Aplicacao.Comum;
using CausaClara.Infraestrutura.Email;
using CausaClara.Infraestrutura.Seguranca;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CausaClara.Infraestrutura.Extensoes;

/// <summary>
/// Ponto único de contato entre a Api e a Infraestrutura.
/// </summary>
/// <remarks>
/// Mesmo padrão de <c>AdicionarPersistencia</c>: a Api chama uma linha no
/// composition root e não conhece MailKit, SMTP nem nenhum provedor. Trocar a
/// forma de enviar e-mail não toca a Api nem a Aplicação.
/// </remarks>
public static class ConfiguracaoDeInfraestrutura
{
    public static IServiceCollection AdicionarInfraestrutura(
        this IServiceCollection servicos,
        IConfiguration configuracao)
    {
        ArgumentNullException.ThrowIfNull(configuracao);

        servicos
            .AddOptions<OpcoesDeEmail>()
            .Bind(configuracao.GetSection(OpcoesDeEmail.Secao))
            .Validate(
                opcoes => !opcoes.Problemas().Any(),
                "Configuração de e-mail inválida. Verifique as variáveis Email__* do ambiente.")
            // Falha na subida, não no primeiro envio: descobrir que o servidor
            // de e-mail não estava configurado quando alguém pede redefinição de
            // senha é tarde demais (RF09).
            .ValidateOnStart();

        servicos.AddScoped<IServicoDeEmail, ServicoDeEmailSmtp>();

        servicos
            .AddOptions<OpcoesDeToken>()
            .Bind(configuracao.GetSection(OpcoesDeToken.Secao))
            .Validate(
                opcoes => !opcoes.Problemas().Any(),
                "Configuração do token de acesso inválida. Verifique as variáveis Token__* do ambiente.")
            .ValidateOnStart();

        // Sem estado e baratos de construir: uma instância serve a tudo.
        servicos.AddSingleton<IHashDeSenha, HashDeSenhaPbkdf2>();
        servicos.AddSingleton<IProtetorDeSegredos, ProtetorDeSegredos>();
        servicos.AddSingleton<IGeradorDeTokenDeAcesso, GeradorDeTokenJwt>();

        // Relógio injetável: expiração de sessão e de link de redefinição só é
        // testável se o tempo puder ser controlado pelo teste.
        servicos.TryAddSingleton(TimeProvider.System);

        return servicos;
    }
}
