using CausaClara.Dominio.Comum;

namespace CausaClara.Dominio.Usuarios;

/// <summary>
/// Conta de acesso ao sistema (RF03). O tipo é escolhido no cadastro e nunca muda (RN01).
/// </summary>
public sealed class Usuario
{
    public const int TamanhoMaximoNome = 150;
    public const int TamanhoMaximoEmail = 254;      // limite de endereço de e-mail na RFC 5321
    public const int TamanhoMaximoTelefone = 20;
    public const int TamanhoMaximoSenhaHash = 255;  // cabe bcrypt, scrypt ou argon2id com sal

    /// <summary>Construtor do EF Core. Não usar no código de negócio.</summary>
    private Usuario() { }

    private Usuario(string nomeCompleto, string email, string senhaHash, string? telefone, TipoUsuario tipo)
    {
        NomeCompleto = nomeCompleto;
        Email = email;
        SenhaHash = senhaHash;
        Telefone = telefone;
        Tipo = tipo;
        CriadoEm = DateTime.UtcNow;
        AtualizadoEm = CriadoEm;
    }

    public int Id { get; private set; }

    public string NomeCompleto { get; private set; } = null!;

    /// <summary>Identificador de acesso, sempre normalizado. Único no sistema (RF04).</summary>
    public string Email { get; private set; } = null!;

    /// <summary>
    /// Resumo criptográfico da senha, com sal individual (RNF10).
    /// O domínio nunca vê a senha em claro: quem deriva o resumo é a camada de Aplicação.
    /// </summary>
    public string SenhaHash { get; private set; } = null!;

    /// <summary>Opcional por RF03.</summary>
    public string? Telefone { get; private set; }

    /// <summary>
    /// RN01: sem método que altere este valor. A imutabilidade é estrutural,
    /// não depende de ninguém lembrar de não mexer.
    /// </summary>
    public TipoUsuario Tipo { get; private set; }

    /// <summary>Sempre UTC (RNF28).</summary>
    public DateTime CriadoEm { get; private set; }

    /// <summary>Sempre UTC (RNF28).</summary>
    public DateTime AtualizadoEm { get; private set; }

    public static Usuario Criar(
        string nomeCompleto,
        string email,
        string senhaHash,
        string? telefone,
        TipoUsuario tipo)
    {
        var nome = RegraDeNegocioViolada.ExigirTexto(nomeCompleto, "o nome completo", TamanhoMaximoNome);
        var endereco = NormalizarEmail(email);
        var resumo = RegraDeNegocioViolada.ExigirTexto(senhaHash, "a senha", TamanhoMaximoSenhaHash);
        var fone = RegraDeNegocioViolada.TextoOpcional(telefone, "o telefone", TamanhoMaximoTelefone);

        RegraDeNegocioViolada.Se(
            !Enum.IsDefined(tipo),
            "Escolha um tipo de conta válido: Apoiador ou Gestor.");

        return new Usuario(nome, endereco, resumo, fone, tipo);
    }

    /// <summary>
    /// RF07: o usuário atualiza nome e telefone. Tipo e e-mail não entram —
    /// o tipo por RN01, o e-mail porque é a credencial de acesso e sua troca
    /// é um caso de uso próprio, com reverificação.
    /// </summary>
    public void AlterarDadosCadastrais(string nomeCompleto, string? telefone)
    {
        NomeCompleto = RegraDeNegocioViolada.ExigirTexto(nomeCompleto, "o nome completo", TamanhoMaximoNome);
        Telefone = RegraDeNegocioViolada.TextoOpcional(telefone, "o telefone", TamanhoMaximoTelefone);
        AtualizadoEm = DateTime.UtcNow;
    }

    /// <summary>Substitui o resumo da senha. Quem deriva o resumo é a Aplicação (RNF10).</summary>
    public void DefinirSenha(string senhaHash)
    {
        SenhaHash = RegraDeNegocioViolada.ExigirTexto(senhaHash, "a senha", TamanhoMaximoSenhaHash);
        AtualizadoEm = DateTime.UtcNow;
    }

    /// <summary>
    /// Caixa baixa e sem espaços nas pontas, para que a unicidade do RF04 valha
    /// de fato: "Flavio@Exemplo.com" e "flavio@exemplo.com" são a mesma conta.
    /// </summary>
    /// <remarks>
    /// A verificação aqui é de invariante — o endereço precisa ter parte local e
    /// domínio —, não de conformidade com a RFC 5322. Validação de formato completa
    /// é responsabilidade do validador do DTO na Aplicação (RNF15).
    /// </remarks>
    public static string NormalizarEmail(string? email)
    {
        var limpo = RegraDeNegocioViolada.ExigirTexto(email, "o e-mail", TamanhoMaximoEmail).ToLowerInvariant();

        var arroba = limpo.IndexOf('@', StringComparison.Ordinal);
        var formatoValido = arroba > 0
                            && arroba == limpo.LastIndexOf('@')
                            && arroba < limpo.Length - 1
                            && !limpo.Contains(' ', StringComparison.Ordinal);

        RegraDeNegocioViolada.Se(!formatoValido, "Informe um e-mail válido.");
        return limpo;
    }
}
