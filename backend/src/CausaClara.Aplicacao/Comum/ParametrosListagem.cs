namespace CausaClara.Aplicacao.Comum;

/// <summary>
/// Página e tamanho pedidos pelo cliente, normalizados no servidor (RNF09).
/// </summary>
/// <remarks>
/// O cliente pede; o servidor decide. Valores fora da faixa são corrigidos em
/// vez de recusados, porque paginação é detalhe de transporte e não vale
/// devolver erro por causa dela — mas o teto de cem é inegociável, senão
/// bastaria pedir <c>tamanho=1000000</c> para obter a coleção inteira, que é
/// justamente o que o RNF09 veda.
/// </remarks>
public sealed record ParametrosListagem
{
    /// <summary>Teto do RNF09. O cliente não decide o limite.</summary>
    public const int TamanhoMaximo = 100;

    public const int TamanhoPadrao = 20;

    public int Pagina { get; init; } = 1;

    public int Tamanho { get; init; } = TamanhoPadrao;

    /// <summary>Página efetiva: nunca menor que um.</summary>
    public int PaginaNormalizada => Pagina < 1 ? 1 : Pagina;

    /// <summary>Tamanho efetivo: entre um e <see cref="TamanhoMaximo"/>.</summary>
    public int TamanhoNormalizado => Tamanho switch
    {
        < 1 => TamanhoPadrao,
        > TamanhoMaximo => TamanhoMaximo,
        _ => Tamanho
    };

    /// <summary>Quantos registros pular para chegar nesta página.</summary>
    public int Salto => (PaginaNormalizada - 1) * TamanhoNormalizado;
}
