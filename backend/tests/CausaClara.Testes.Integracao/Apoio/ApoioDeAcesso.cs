using CausaClara.Aplicacao.Acesso;
using CausaClara.Aplicacao.Comum;
using CausaClara.Dominio.Usuarios;
using CausaClara.Infraestrutura.Seguranca;
using Microsoft.Extensions.Logging.Abstractions;

namespace CausaClara.Testes.Integracao.Apoio;

/// <summary>Relógio controlado pelo teste, para exercitar prazos sem esperar.</summary>
internal sealed class RelogioDeTeste(DateTimeOffset inicio) : TimeProvider
{
    private DateTimeOffset _agora = inicio;

    public override DateTimeOffset GetUtcNow() => _agora;

    public void Avancar(TimeSpan quanto) => _agora = _agora.Add(quanto);
}

/// <summary>Guarda as mensagens em vez de enviá-las, para o teste inspecionar.</summary>
internal sealed class EmailDeTeste : IServicoDeEmail
{
    public List<MensagemDeEmail> Enviadas { get; } = [];

    public Task EnviarAsync(MensagemDeEmail mensagem, CancellationToken cancelamento = default)
    {
        Enviadas.Add(mensagem);
        return Task.CompletedTask;
    }
}

/// <summary>Emissor de token previsível, para o teste não depender de assinatura.</summary>
/// <remarks>
/// O conteúdo do token é verificado onde ele é gerado de verdade; aqui o que
/// está sob teste é o fluxo de sessão, não o formato da credencial.
/// </remarks>
internal sealed class GeradorDeTokenDeTeste(TimeProvider relogio) : IGeradorDeTokenDeAcesso
{
    public int Emissoes { get; private set; }

    public TokenDeAcessoEmitido Emitir(Usuario usuario, int? instituicaoId)
    {
        Emissoes++;
        return new TokenDeAcessoEmitido(
            $"token-de-acesso-{usuario.Id}-{Emissoes}",
            relogio.GetUtcNow().UtcDateTime.AddHours(1));
    }
}

/// <summary>Monta os serviços de acesso sobre o banco de teste.</summary>
internal sealed class CenarioDeAcesso : IDisposable
{
    private readonly BancoDeTeste _banco = new();

    public CenarioDeAcesso()
    {
        Relogio = new RelogioDeTeste(new DateTimeOffset(2026, 10, 29, 12, 0, 0, TimeSpan.Zero));
        Email = new EmailDeTeste();
        Opcoes = new OpcoesDeAcesso();

        Contexto = _banco.Abrir();

        // Implementações reais de resumo de senha e de segredo: são justamente
        // as peças cujo comportamento o fluxo depende.
        Hash = new HashDeSenhaPbkdf2();
        Protetor = new ProtetorDeSegredos();

        Cadastro = new ServicoDeCadastro(Contexto, Hash, Opcoes);
        Autenticacao = new ServicoDeAutenticacao(
            Contexto, Hash, Protetor, new GeradorDeTokenDeTeste(Relogio), Opcoes, Relogio);
        Redefinicao = new ServicoDeRedefinicaoDeSenha(
            Contexto, Hash, Protetor, Email, Autenticacao, Opcoes, Relogio,
            NullLogger<ServicoDeRedefinicaoDeSenha>.Instance);
    }

    public ContextoDeTeste Contexto { get; }
    public RelogioDeTeste Relogio { get; }
    public EmailDeTeste Email { get; }
    public OpcoesDeAcesso Opcoes { get; }
    public IHashDeSenha Hash { get; }
    public IProtetorDeSegredos Protetor { get; }
    public ServicoDeCadastro Cadastro { get; }
    public ServicoDeAutenticacao Autenticacao { get; }
    public ServicoDeRedefinicaoDeSenha Redefinicao { get; }

    public static NovaConta ContaValida(
        string email = "flavio@exemplo.com",
        string senha = "senha-bem-comprida",
        TipoUsuario tipo = TipoUsuario.Apoiador)
        => new("Flávio Henrique Lara Santos", email, "(31) 99999-0000", senha, senha, tipo);

    public void Dispose()
    {
        Contexto.Dispose();
        _banco.Dispose();
    }
}
