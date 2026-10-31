using Microsoft.EntityFrameworkCore;

namespace CausaClara.Aplicacao.Comum;

/// <summary>Converte uma consulta em uma página de resultados (RNF09).</summary>
public static class ExtensoesDePaginacao
{
    /// <summary>
    /// Conta o total e devolve apenas a fatia pedida, com o teto do RNF09 aplicado.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Duas idas ao banco, uma para contar e outra para buscar. É o preço de
    /// informar o total, que a interface precisa. A contagem roda sobre a mesma
    /// consulta filtrada, então respeita o filtro global por instituição.
    /// </para>
    /// <para>
    /// A consulta precisa estar ordenada antes de chegar aqui. Sem ordenação
    /// explícita, o PostgreSQL não garante a ordem entre páginas e o mesmo
    /// registro pode aparecer duas vezes, ou nenhuma, conforme o plano escolhido.
    /// </para>
    /// </remarks>
    public static async Task<PaginaResposta<T>> PaginarAsync<T>(
        this IQueryable<T> consulta,
        ParametrosListagem parametros,
        CancellationToken cancelamento = default)
    {
        ArgumentNullException.ThrowIfNull(consulta);
        ArgumentNullException.ThrowIfNull(parametros);

        var total = await consulta.CountAsync(cancelamento);
        if (total == 0) return PaginaResposta<T>.Vazia(parametros);

        var itens = await consulta
            .Skip(parametros.Salto)
            .Take(parametros.TamanhoNormalizado)
            .ToListAsync(cancelamento);

        return new PaginaResposta<T>(
            itens,
            parametros.PaginaNormalizada,
            parametros.TamanhoNormalizado,
            total);
    }
}
