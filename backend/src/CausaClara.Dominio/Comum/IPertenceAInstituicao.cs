namespace CausaClara.Dominio.Comum;

/// <summary>
/// Entidade cujos registros pertencem a uma única instituição (RN05).
/// </summary>
/// <remarks>
/// <para>
/// Implementar este marcador é o que faz a entidade ser coberta pelo filtro
/// global de consulta por instituição (RNF12). Não escreva verificação manual de
/// vínculo em caso de uso nem em controller: o filtro já cuida, e um
/// identificador forjado devolve <em>não encontrado</em> em vez de dado alheio.
/// </para>
/// <para>
/// A propriedade é somente leitura de propósito. Trocar a instituição de um
/// lançamento não é operação do domínio — seria mover dinheiro de uma
/// organização para outra sem rastro.
/// </para>
/// </remarks>
public interface IPertenceAInstituicao
{
    int InstituicaoId { get; }
}
