namespace CausaClara.Aplicacao.Acesso;

/// <summary>
/// Prazos e endereços que os casos de uso de acesso precisam conhecer.
/// </summary>
/// <remarks>
/// Classe simples, preenchida pelo composition root. A Aplicação não lê
/// configuração nem conhece variáveis de ambiente — ela recebe os valores já
/// resolvidos.
/// </remarks>
public sealed class OpcoesDeAcesso
{
    /// <summary>
    /// Vida do token de renovação, e portanto da sessão inteira.
    /// </summary>
    /// <remarks>
    /// Oito horas é o teto do RNF13. Passado esse prazo não há renovação
    /// possível: o usuário autentica de novo.
    /// </remarks>
    public TimeSpan ValidadeDaSessao { get; set; } = TimeSpan.FromHours(8);

    /// <summary>
    /// Vida do link de redefinição de senha. Curta porque o segredo viaja por
    /// e-mail, canal fora do nosso controle.
    /// </summary>
    public TimeSpan ValidadeDoLinkDeRedefinicao { get; set; } = TimeSpan.FromMinutes(30);

    /// <summary>Tamanho mínimo da senha.</summary>
    /// <remarks>
    /// Nenhum requisito fixa um mínimo; oito caracteres é a escolha feita aqui,
    /// alinhada à recomendação corrente de não exigir composição complicada e
    /// sim comprimento. Está neste lugar para ser fácil de mudar.
    /// </remarks>
    public int TamanhoMinimoDaSenha { get; set; } = 8;

    /// <summary>
    /// Endereço do cliente, usado para montar o link de redefinição (RF09).
    /// </summary>
    public string UrlBaseDoCliente { get; set; } = "http://localhost:5173";

    /// <summary>Caminho da tela que recebe o token de redefinição.</summary>
    public string CaminhoDeRedefinicao { get; set; } = "/redefinir-senha";

    public string MontarLinkDeRedefinicao(string segredo)
        => $"{UrlBaseDoCliente.TrimEnd('/')}{CaminhoDeRedefinicao}?token={Uri.EscapeDataString(segredo)}";
}
