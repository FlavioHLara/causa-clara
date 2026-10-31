using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace CausaClara.Infraestrutura.Seguranca;

/// <summary>
/// Configuração da credencial assinada de acesso (RNF13), vinda do ambiente (RNF18).
/// </summary>
public sealed class OpcoesDeToken
{
    /// <summary>Seção na configuração: <c>Token</c>, ou <c>Token__...</c> no ambiente.</summary>
    public const string Secao = "Token";

    /// <summary>Tamanho mínimo da chave para HMAC-SHA256, em caracteres.</summary>
    /// <remarks>
    /// O algoritmo exige chave de pelo menos 256 bits. Chave curta faz a
    /// biblioteca recusar a assinatura na primeira emissão — melhor recusar na
    /// subida, com mensagem que diz o que fazer.
    /// </remarks>
    public const int TamanhoMinimoDaChave = 32;

    /// <summary>Quem emite o token. Conferido na validação.</summary>
    public string Emissor { get; set; } = "causa-clara";

    /// <summary>Para quem o token vale. Conferido na validação.</summary>
    public string Audiencia { get; set; } = "causa-clara";

    /// <summary>Chave de assinatura. Segredo: nunca entra no repositório.</summary>
    public string Chave { get; set; } = string.Empty;

    /// <summary>Minutos de vida do token de acesso. O RNF13 fixa o teto em oito horas.</summary>
    public int ValidadeEmMinutos { get; set; } = 60;

    /// <summary>
    /// Chave de assinatura, usada tanto para emitir quanto para validar.
    /// </summary>
    /// <remarks>
    /// Exposta aqui, e não no emissor, porque a Api precisa dela para configurar
    /// a validação — e o emissor é detalhe interno da Infraestrutura.
    /// </remarks>
    public SymmetricSecurityKey ChaveDeAssinatura() => new(Encoding.UTF8.GetBytes(Chave));

    public IEnumerable<string> Problemas()
    {
        if (string.IsNullOrWhiteSpace(Chave))
            yield return $"{Secao}__Chave não foi informada.";
        else if (Chave.Length < TamanhoMinimoDaChave)
            yield return $"{Secao}__Chave deve ter pelo menos {TamanhoMinimoDaChave} caracteres.";

        if (string.IsNullOrWhiteSpace(Emissor))
            yield return $"{Secao}__Emissor não foi informado.";

        if (string.IsNullOrWhiteSpace(Audiencia))
            yield return $"{Secao}__Audiencia não foi informada.";

        if (ValidadeEmMinutos < 1)
            yield return $"{Secao}__ValidadeEmMinutos deve ser maior que zero.";

        // RNF13: "validade máxima de oito horas".
        if (ValidadeEmMinutos > 480)
            yield return $"{Secao}__ValidadeEmMinutos não pode passar de 480, que são as oito horas do RNF13.";
    }
}
