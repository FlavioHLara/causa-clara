using CausaClara.Dominio.Comum;

namespace CausaClara.Dominio.Acesso;

/// <summary>
/// Credencial de renovação de sessão (RF06, RF08, RNF13).
/// </summary>
/// <remarks>
/// <para>
/// O que fica guardado é o <b>resumo</b> do segredo, nunca o segredo. Quem
/// obtiver uma cópia do banco não consegue renovar a sessão de ninguém — o mesmo
/// raciocínio do RNF10 para senhas, aplicado à credencial de sessão.
/// </para>
/// <para>
/// É de uso único e rotativo: renovar invalida o token usado e emite outro. Se
/// um token já consumido reaparecer, é sinal de cópia vazada, e a cadeia inteira
/// pode ser revogada.
/// </para>
/// <para>
/// O instante vem por parâmetro em vez de <c>DateTime.UtcNow</c>: expiração que
/// só pode ser testada esperando o relógio não é testada.
/// </para>
/// </remarks>
public sealed class TokenDeAtualizacao
{
    public const int TamanhoDoResumo = 64;   // SHA-256 em hexadecimal

    private TokenDeAtualizacao() { }

    private TokenDeAtualizacao(int usuarioId, string resumo, DateTime criadoEm, DateTime expiraEm)
    {
        UsuarioId = usuarioId;
        Resumo = resumo;
        CriadoEm = criadoEm;
        ExpiraEm = expiraEm;
    }

    public long Id { get; private set; }

    public int UsuarioId { get; private set; }

    /// <summary>Resumo do segredo. O segredo em si só existe no cliente.</summary>
    public string Resumo { get; private set; } = null!;

    public DateTime CriadoEm { get; private set; }

    public DateTime ExpiraEm { get; private set; }

    /// <summary>Preenchido ao encerrar a sessão (RF08) ou ao renovar.</summary>
    public DateTime? RevogadoEm { get; private set; }

    public static TokenDeAtualizacao Emitir(int usuarioId, string resumo, DateTime agora, TimeSpan validade)
    {
        RegraDeNegocioViolada.Se(usuarioId <= 0, "Informe o usuário do token de sessão.");
        RegraDeNegocioViolada.Se(validade <= TimeSpan.Zero, "A validade do token deve ser maior que zero.");

        var limpo = RegraDeNegocioViolada.ExigirTexto(resumo, "o resumo do token", TamanhoDoResumo);

        return new TokenDeAtualizacao(usuarioId, limpo, agora, agora.Add(validade));
    }

    /// <summary>Serve para renovar: não foi revogado e ainda não expirou.</summary>
    public bool EstaAtivo(DateTime agora) => RevogadoEm is null && agora < ExpiraEm;

    /// <summary>
    /// Encerra este token. Idempotente de propósito: encerrar sessão duas vezes
    /// não é erro, e a primeira revogação é a que vale.
    /// </summary>
    public void Revogar(DateTime agora) => RevogadoEm ??= agora;
}
