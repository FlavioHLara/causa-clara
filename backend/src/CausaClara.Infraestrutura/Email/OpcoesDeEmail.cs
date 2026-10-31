namespace CausaClara.Infraestrutura.Email;

/// <summary>Como a sigla de segurança do SMTP é negociada.</summary>
/// <remarks>
/// Enum próprio em vez do tipo do MailKit: a configuração do projeto não deveria
/// mudar de forma porque trocamos de biblioteca de envio.
/// </remarks>
public enum SegurancaDoSmtp
{
    /// <summary>Sem criptografia. Só para a captura de mensagens do ambiente local.</summary>
    Nenhuma,

    /// <summary>Conecta em texto claro e eleva para TLS. É o que quase todo provedor usa, na porta 587.</summary>
    StartTls,

    /// <summary>TLS desde o primeiro byte, normalmente na porta 465.</summary>
    TlsImplicito,

    /// <summary>Deixa a biblioteca escolher pelo que o servidor anunciar.</summary>
    Automatica
}

/// <summary>
/// Configuração de envio de e-mail (RF09, RF24), vinda do ambiente (RNF18).
/// </summary>
/// <remarks>
/// Tudo por variável de ambiente, nada no repositório. Trocar de provedor é
/// trocar estes valores — por isso SMTP, e não a API HTTP de um fornecedor:
/// SMTP é o denominador comum entre todos eles, e um tier gratuito que mude de
/// termos não vira reescrita de código.
/// </remarks>
public sealed class OpcoesDeEmail
{
    /// <summary>Seção na configuração: <c>Email</c>, ou <c>Email__...</c> no ambiente.</summary>
    public const string Secao = "Email";

    /// <summary>Endereço que aparece como remetente.</summary>
    public string RemetenteEndereco { get; set; } = string.Empty;

    public string RemetenteNome { get; set; } = "Causa Clara";

    public string Servidor { get; set; } = string.Empty;

    public int Porta { get; set; } = 587;

    /// <summary>Vazio quando o servidor não exige autenticação, como a captura local.</summary>
    public string? Usuario { get; set; }

    public string? Senha { get; set; }

    public SegurancaDoSmtp Seguranca { get; set; } = SegurancaDoSmtp.StartTls;

    /// <summary>Segundos antes de desistir da conexão.</summary>
    public int TempoLimiteEmSegundos { get; set; } = 15;

    /// <summary>
    /// Razões pelas quais esta configuração não serve, em português.
    /// </summary>
    /// <remarks>
    /// Verificado na inicialização, e não no primeiro envio. Descobrir que o
    /// servidor de e-mail não estava configurado no momento em que alguém pede
    /// redefinição de senha é tarde demais.
    /// </remarks>
    public IEnumerable<string> Problemas()
    {
        if (string.IsNullOrWhiteSpace(Servidor))
            yield return $"{Secao}__Servidor não foi informado.";

        if (Porta is < 1 or > 65535)
            yield return $"{Secao}__Porta deve estar entre 1 e 65535; veio {Porta}.";

        if (string.IsNullOrWhiteSpace(RemetenteEndereco))
            yield return $"{Secao}__RemetenteEndereco não foi informado.";
        else if (!RemetenteEndereco.Contains('@', StringComparison.Ordinal))
            yield return $"{Secao}__RemetenteEndereco não parece um endereço de e-mail.";

        if (!string.IsNullOrWhiteSpace(Usuario) && string.IsNullOrWhiteSpace(Senha))
            yield return $"{Secao}__Senha é obrigatória quando {Secao}__Usuario é informado.";

        if (TempoLimiteEmSegundos < 1)
            yield return $"{Secao}__TempoLimiteEmSegundos deve ser maior que zero.";
    }
}
