namespace CausaClara.Dominio.Comum;

/// <summary>
/// Entidade cujas criações, alterações e exclusões entram na trilha de auditoria (RF81).
/// </summary>
/// <remarks>
/// <para>
/// Marcador sem membros: a entidade não precisa fazer nada, basta declarar.
/// Quem grava é o interceptador de <c>SaveChanges</c>, que inspeciona o
/// <c>ChangeTracker</c>. Espalhar a gravação pelos casos de uso garantiria que
/// alguém esquecesse de um.
/// </para>
/// <para>
/// RF81 nomeia lançamentos financeiros e movimentações de estoque. Cadastro de
/// usuário e de instituição ficam de fora porque o requisito não os inclui, e
/// auditar tudo encheria a tabela de ruído sem servir à prestação de contas.
/// </para>
/// </remarks>
public interface IAuditavel;
