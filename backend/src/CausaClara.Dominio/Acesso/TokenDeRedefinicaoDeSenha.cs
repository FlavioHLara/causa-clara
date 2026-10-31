using CausaClara.Dominio.Comum;

namespace CausaClara.Dominio.Acesso;

/// <summary>
/// Credencial de uso único enviada por e-mail para redefinir a senha (RF09).
/// </summary>
/// <remarks>
/// Guarda apenas o resumo, como <see cref="TokenDeAtualizacao"/>. A validade é
/// curta porque o segredo viaja por e-mail, que é um canal fora do nosso
/// controle: quanto menor a janela, menor a chance de alguém que leia a caixa
/// de entrada depois conseguir usá-lo.
/// </remarks>
public sealed class TokenDeRedefinicaoDeSenha
{
    public const int TamanhoDoResumo = 64;

    private TokenDeRedefinicaoDeSenha() { }

    private TokenDeRedefinicaoDeSenha(int usuarioId, string resumo, DateTime criadoEm, DateTime expiraEm)
    {
        UsuarioId = usuarioId;
        Resumo = resumo;
        CriadoEm = criadoEm;
        ExpiraEm = expiraEm;
    }

    public long Id { get; private set; }

    public int UsuarioId { get; private set; }

    public string Resumo { get; private set; } = null!;

    public DateTime CriadoEm { get; private set; }

    public DateTime ExpiraEm { get; private set; }

    public DateTime? UsadoEm { get; private set; }

    public static TokenDeRedefinicaoDeSenha Emitir(int usuarioId, string resumo, DateTime agora, TimeSpan validade)
    {
        RegraDeNegocioViolada.Se(usuarioId <= 0, "Informe o usuário do pedido de redefinição.");
        RegraDeNegocioViolada.Se(validade <= TimeSpan.Zero, "A validade do token deve ser maior que zero.");

        var limpo = RegraDeNegocioViolada.ExigirTexto(resumo, "o resumo do token", TamanhoDoResumo);

        return new TokenDeRedefinicaoDeSenha(usuarioId, limpo, agora, agora.Add(validade));
    }

    public bool EstaValido(DateTime agora) => UsadoEm is null && agora < ExpiraEm;

    /// <summary>
    /// Consome o token. Uso único: sem isto, o mesmo link redefiniria a senha
    /// quantas vezes quisesse quem tivesse acesso ao e-mail.
    /// </summary>
    public void MarcarComoUsado(DateTime agora)
    {
        RegraDeNegocioViolada.Se(UsadoEm is not null, "Este link de redefinição já foi utilizado.");
        UsadoEm = agora;
    }
}
