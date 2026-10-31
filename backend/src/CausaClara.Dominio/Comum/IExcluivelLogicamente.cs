namespace CausaClara.Dominio.Comum;

/// <summary>
/// Entidade cuja exclusão é lógica, nunca física (RN10).
/// </summary>
/// <remarks>
/// O lançamento some da listagem sem sumir da trilha de auditoria. Quem converte
/// a exclusão é o interceptador de <c>SaveChanges</c>: uma entrada marcada como
/// <c>Deleted</c> vira <c>Modified</c> com <see cref="Excluido"/> verdadeiro, e o
/// filtro global esconde o registro das consultas seguintes.
/// </remarks>
public interface IExcluivelLogicamente
{
    bool Excluido { get; }

    /// <summary>Quando foi excluído, em UTC (RNF28). Nulo enquanto ativo.</summary>
    DateTime? ExcluidoEm { get; }

    /// <summary>
    /// Chamado pelo interceptador de persistência, não pelos casos de uso.
    /// Casos de uso pedem <c>Remove</c> ao contexto; a conversão é transversal.
    /// </summary>
    void MarcarComoExcluido();
}
