namespace CausaClara.Dominio.Comum;

/// <summary>
/// Violação de uma invariante de negócio (RN01 a RN16).
/// </summary>
/// <remarks>
/// A mensagem é escrita para o usuário final, em português e sem informação
/// técnica, conforme RNF05 — o tratador de exceções da Api a devolve como está.
/// Erro de programação continua sendo <see cref="ArgumentException"/> e afins;
/// esta exceção é só para regra de negócio.
/// </remarks>
public sealed class RegraDeNegocioViolada(string mensagem) : Exception(mensagem)
{
    /// <summary>Lança quando <paramref name="condicao"/> for verdadeira.</summary>
    public static void Se(bool condicao, string mensagem)
    {
        if (condicao) throw new RegraDeNegocioViolada(mensagem);
    }

    /// <summary>Exige texto não vazio e dentro do tamanho máximo.</summary>
    /// <param name="campo">
    /// Nome do campo com artigo, como <c>"o nome completo"</c>, para que as
    /// mensagens saiam em português corrente (RNF05).
    /// </param>
    public static string ExigirTexto(string? valor, string campo, int tamanhoMaximo)
    {
        var limpo = valor?.Trim();
        Se(string.IsNullOrEmpty(limpo), $"Informe {campo}.");
        Se(limpo!.Length > tamanhoMaximo, MensagemDeTamanho(campo, tamanhoMaximo));
        return limpo;
    }

    /// <summary>Normaliza texto opcional: vazio vira nulo, e o tamanho é verificado.</summary>
    public static string? TextoOpcional(string? valor, string campo, int tamanhoMaximo)
    {
        var limpo = valor?.Trim();
        if (string.IsNullOrEmpty(limpo)) return null;
        Se(limpo.Length > tamanhoMaximo, MensagemDeTamanho(campo, tamanhoMaximo));
        return limpo;
    }

    /// <summary>
    /// "o nome completo" vira "O nome completo deve ter no máximo 150 caracteres."
    /// Prefixar com "O campo " produziria "O campo o nome completo", com artigo repetido.
    /// </summary>
    private static string MensagemDeTamanho(string campo, int tamanhoMaximo)
        => $"{char.ToUpperInvariant(campo[0])}{campo[1..]} deve ter no máximo {tamanhoMaximo} caracteres.";
}
