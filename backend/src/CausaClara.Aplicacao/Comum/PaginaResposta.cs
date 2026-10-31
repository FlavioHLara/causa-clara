namespace CausaClara.Aplicacao.Comum;

/// <summary>
/// Uma página de resultados e o que o cliente precisa para montar a paginação.
/// </summary>
/// <remarks>
/// O total de registros vem junto porque o componente de paginação do protótipo
/// mostra "37 doações no período" e precisa saber se existe próxima página. Sem
/// o total, o cliente teria de adivinhar pedindo uma página a mais.
/// </remarks>
public sealed record PaginaResposta<T>(
    IReadOnlyList<T> Itens,
    int Pagina,
    int Tamanho,
    int TotalDeRegistros)
{
    public int TotalDePaginas => TotalDeRegistros == 0
        ? 0
        : (int)Math.Ceiling(TotalDeRegistros / (double)Tamanho);

    public bool TemPaginaAnterior => Pagina > 1;

    public bool TemProximaPagina => Pagina < TotalDePaginas;

    /// <summary>Página vazia, para quando não há o que listar.</summary>
    public static PaginaResposta<T> Vazia(ParametrosListagem parametros)
        => new([], parametros.PaginaNormalizada, parametros.TamanhoNormalizado, 0);
}
