using CausaClara.Dominio.Usuarios;

namespace CausaClara.Aplicacao.Comum;

/// <summary>
/// Quem está fazendo a requisição, resolvido a partir das <em>claims</em> do token.
/// </summary>
/// <remarks>
/// <para>
/// É o que alimenta o filtro global por instituição (RNF12) e o autor das
/// entradas da trilha de auditoria (RF81). O escopo é o da requisição, igual ao
/// do <c>DbContext</c> — isso não é detalhe: se os escopos divergissem, o filtro
/// leria o contexto de outra requisição.
/// </para>
/// <para>
/// Todas as propriedades são anuláveis porque o caminho público de transparência
/// (RF63 a RF73) roda sem usuário autenticado. Quando <see cref="InstituicaoId"/>
/// é nulo, o filtro global não casa com nada — a falha é fechada, não aberta.
/// </para>
/// <para>
/// O papel de vínculo (Dono ou Funcionário) entra aqui na etapa 4, junto com a
/// entidade de vínculo que o define. Declará-lo agora seria tipo especulativo.
/// </para>
/// </remarks>
public interface IContextoDaRequisicao
{
    int? UsuarioId { get; }

    /// <summary>Instituição do vínculo aprovado do usuário, se houver.</summary>
    int? InstituicaoId { get; }

    TipoUsuario? TipoDeConta { get; }

    /// <summary>Verdadeiro quando há usuário autenticado na requisição.</summary>
    bool Autenticado { get; }
}
