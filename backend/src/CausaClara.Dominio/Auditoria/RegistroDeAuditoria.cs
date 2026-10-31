using CausaClara.Dominio.Comum;

namespace CausaClara.Dominio.Auditoria;

/// <summary>
/// Entrada imutável da trilha de auditoria: quem fez o quê, quando e sobre qual
/// registro (RF81).
/// </summary>
/// <remarks>
/// <para>
/// Não há método que altere uma entrada depois de criada, e a entidade não
/// implementa <see cref="IExcluivelLogicamente"/> nem <see cref="IAuditavel"/>:
/// trilha que se edita não é trilha, e auditar a auditoria entraria em recursão.
/// </para>
/// <para>
/// <b>Sobre o RNF21.</b> O autor é guardado como referência anulável, e não como
/// nome copiado. Quando o titular pedir exclusão dos dados pessoais, anular a
/// referência basta para anonimizar a trilha sem apagá-la — que é exatamente o
/// que o requisito pede. Copiar o nome para cá criaria uma segunda cópia do dado
/// pessoal, fora do alcance do pedido de exclusão. A decisão completa sobre o
/// RNF21 continua aberta; este modelo apenas não trabalha contra ela.
/// </para>
/// </remarks>
public sealed class RegistroDeAuditoria
{
    public const int TamanhoMaximoEntidade = 100;
    public const int TamanhoMaximoChave = 50;
    public const int TamanhoMaximoDescricao = 500;

    /// <summary>Construtor do EF Core. Não usar no código de negócio.</summary>
    private RegistroDeAuditoria() { }

    private RegistroDeAuditoria(
        OperacaoAuditada operacao,
        string entidade,
        string chaveDoRegistro,
        string? descricao,
        int? usuarioId,
        int? instituicaoId)
    {
        Operacao = operacao;
        Entidade = entidade;
        ChaveDoRegistro = chaveDoRegistro;
        Descricao = descricao;
        UsuarioId = usuarioId;
        InstituicaoId = instituicaoId;
        OcorridoEm = DateTime.UtcNow;
    }

    public long Id { get; private set; }

    public OperacaoAuditada Operacao { get; private set; }

    /// <summary>Nome do tipo de domínio afetado, como <c>Doacao</c> ou <c>MovimentacaoEstoque</c>.</summary>
    public string Entidade { get; private set; } = null!;

    /// <summary>Chave primária do registro afetado, em texto, porque nem toda entidade usa o mesmo tipo de chave.</summary>
    public string ChaveDoRegistro { get; private set; } = null!;

    /// <summary>Resumo legível da operação, para a coluna "Registro" do demonstrativo (T17).</summary>
    public string? Descricao { get; private set; }

    /// <summary>
    /// Autor da operação. Nulo quando a operação não tem usuário autenticado ou
    /// quando o autor foi anonimizado a pedido do titular (RNF21).
    /// </summary>
    public int? UsuarioId { get; private set; }

    /// <summary>
    /// Instituição a que o dado afetado pertencia. Nulo para operações que
    /// acontecem fora de um vínculo institucional.
    /// </summary>
    public int? InstituicaoId { get; private set; }

    /// <summary>Sempre UTC (RNF28).</summary>
    public DateTime OcorridoEm { get; private set; }

    /// <summary>
    /// Monta a entrada a partir de dados já extraídos da camada de persistência.
    /// </summary>
    /// <remarks>
    /// Recebe tipos primitivos de propósito: o Domínio não conhece
    /// <c>EntityEntry</c> nem nada do EF Core. Quem traduz o rastreador de
    /// mudanças para estes argumentos é o interceptador, na Persistência.
    /// </remarks>
    public static RegistroDeAuditoria Registrar(
        OperacaoAuditada operacao,
        string entidade,
        string chaveDoRegistro,
        string? descricao = null,
        int? usuarioId = null,
        int? instituicaoId = null)
    {
        var nomeDaEntidade = RegraDeNegocioViolada.ExigirTexto(entidade, "a entidade auditada", TamanhoMaximoEntidade);
        var chave = RegraDeNegocioViolada.ExigirTexto(chaveDoRegistro, "a chave do registro auditado", TamanhoMaximoChave);
        var resumo = RegraDeNegocioViolada.TextoOpcional(descricao, "a descrição da auditoria", TamanhoMaximoDescricao);

        return new RegistroDeAuditoria(operacao, nomeDaEntidade, chave, resumo, usuarioId, instituicaoId);
    }
}
