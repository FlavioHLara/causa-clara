namespace CausaClara.Dominio.Auditoria;

/// <summary>Operação registrada na trilha de auditoria (RF81).</summary>
/// <remarks>Persistido como texto, com restrição CHECK no banco.</remarks>
public enum OperacaoAuditada
{
    Criacao,
    Alteracao,

    /// <summary>
    /// Exclusão lógica (RN10). O registro sai da listagem e permanece no banco,
    /// então esta operação marca o momento em que ele deixou de ser visível.
    /// </summary>
    Exclusao
}
