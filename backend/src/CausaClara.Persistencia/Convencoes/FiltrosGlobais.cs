using System.Linq.Expressions;
using CausaClara.Dominio.Comum;
using Microsoft.EntityFrameworkCore;

namespace CausaClara.Persistencia.Convencoes;

/// <summary>
/// Aplica o isolamento por instituição (RNF12) e a exclusão lógica (RN10) a toda
/// entidade que declara os marcadores, sem listar entidade por entidade.
/// </summary>
/// <remarks>
/// <para>
/// Esta é a peça que fecha a falha de IDOR por construção. Uma lista escrita à
/// mão em <c>OnModelCreating</c> teria o mesmo defeito do <c>if</c> por endpoint:
/// funciona até alguém acrescentar a trigésima entidade e esquecer a linha. Aqui,
/// implementar <see cref="IPertenceAInstituicao"/> já basta.
/// </para>
/// <para>
/// <b>Por que uma expressão só, com <c>&amp;&amp;</c>.</b> Lançamentos precisam
/// dos dois filtros ao mesmo tempo. Chamar <c>HasQueryFilter</c> duas vezes para
/// a mesma entidade substitui o filtro anterior em vez de somar, e o resultado
/// seria perder silenciosamente o isolamento — o pior defeito possível aqui.
/// Compor as condições numa única expressão não depende de versão do EF Core nem
/// de filtro nomeado, e não tem como regredir.
/// </para>
/// <para>
/// <b>Por que a instituição é lida do próprio <c>DbContext</c>.</b> O EF Core só
/// reavalia um filtro a cada consulta quando ele referencia um membro de
/// instância do contexto; qualquer outro objeto capturado é tratado como
/// subexpressão constante e <em>avaliado uma única vez</em>, na primeira
/// compilação da consulta. Referenciar o serviço de contexto da requisição
/// diretamente parecia funcionar e congelava a primeira instituição que
/// passasse por aqui — ou, pior, um nulo — para todas as requisições seguintes.
/// É a ressalva da seção 4 do projeto arquitetural, e o teste
/// <c>O_filtro_le_a_instituicao_no_momento_da_consulta</c> existe para que ela
/// não volte.
/// </para>
/// </remarks>
internal static class FiltrosGlobais
{
    /// <summary>
    /// Nome da propriedade que o <c>DbContext</c> precisa expor para que o filtro
    /// por instituição seja reavaliado a cada consulta.
    /// </summary>
    public const string PropriedadeDeInstituicao = "InstituicaoDaRequisicao";

    public static void Aplicar(ModelBuilder construtor, DbContext contextoDoBanco)
    {
        ArgumentNullException.ThrowIfNull(construtor);
        ArgumentNullException.ThrowIfNull(contextoDoBanco);

        foreach (var tipo in construtor.Model.GetEntityTypes())
        {
            // Tipos pertencentes (owned) são configurados pelo dono; aplicar
            // filtro neles faz o EF reclamar em vez de proteger.
            if (tipo.IsOwned()) continue;

            var classe = tipo.ClrType;
            var entidade = Expression.Parameter(classe, "entidade");
            Expression? condicao = null;

            if (typeof(IPertenceAInstituicao).IsAssignableFrom(classe))
            {
                condicao = ComparaInstituicao(entidade, contextoDoBanco);
            }

            if (typeof(IExcluivelLogicamente).IsAssignableFrom(classe))
            {
                var naoExcluido = Expression.Not(
                    Expression.Property(entidade, nameof(IExcluivelLogicamente.Excluido)));

                condicao = condicao is null ? naoExcluido : Expression.AndAlso(condicao, naoExcluido);
            }

            if (condicao is null) continue;

            construtor.Entity(classe).HasQueryFilter(Expression.Lambda(condicao, entidade));
        }
    }

    /// <summary>
    /// <c>entidade.InstituicaoId == contexto.InstituicaoDaRequisicao</c>, com a
    /// conversão para anulável que permite comparar o inteiro da entidade com o
    /// anulável do contexto. Sem usuário autenticado o lado direito é nulo, a
    /// comparação é falsa para toda linha, e a consulta não devolve nada — a
    /// falha fecha em vez de abrir.
    /// </summary>
    private static Expression ComparaInstituicao(ParameterExpression entidade, DbContext contextoDoBanco)
    {
        var tipoDoContexto = contextoDoBanco.GetType();

        var propriedade = tipoDoContexto.GetProperty(
            PropriedadeDeInstituicao,
            System.Reflection.BindingFlags.Instance
            | System.Reflection.BindingFlags.Public
            | System.Reflection.BindingFlags.NonPublic)
            ?? throw new InvalidOperationException(
                $"{tipoDoContexto.Name} precisa expor a propriedade '{PropriedadeDeInstituicao}' do tipo int? " +
                "para que o filtro global por instituição (RNF12) seja reavaliado a cada consulta.");

        var daEntidade = Expression.Convert(
            Expression.Property(entidade, nameof(IPertenceAInstituicao.InstituicaoId)),
            typeof(int?));

        // A constante é o próprio DbContext: é esta forma que o EF Core reconhece
        // e transforma em parâmetro da consulta, em vez de avaliar uma vez só.
        var doContexto = Expression.Property(
            Expression.Constant(contextoDoBanco, tipoDoContexto),
            propriedade);

        return Expression.Equal(daEntidade, doContexto);
    }
}
