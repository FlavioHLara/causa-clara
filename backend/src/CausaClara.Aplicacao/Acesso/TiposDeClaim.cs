namespace CausaClara.Aplicacao.Acesso;

/// <summary>
/// Nomes das <em>claims</em> que o token carrega.
/// </summary>
/// <remarks>
/// <para>
/// Mora na Aplicação, e não na Api, porque dois lados precisam combinar: a
/// Infraestrutura, que emite o token, e a Api, que o lê. Com a constante num só
/// lugar, não há como os dois divergirem.
/// </para>
/// <para>
/// Um erro de digitação aqui não quebraria a compilação: o contexto da
/// requisição viria vazio, o filtro global não casaria com nada, e toda
/// listagem apareceria vazia sem erro nenhum. Nomes curtos porque cada byte de
/// <em>claim</em> viaja em todo cabeçalho de requisição.
/// </para>
/// </remarks>
public static class TiposDeClaim
{
    public const string UsuarioId = "uid";

    /// <summary>Instituição do vínculo aprovado. Ausente para gestor sem vínculo e para apoiador.</summary>
    public const string InstituicaoId = "ins";

    /// <summary>Apoiador ou Gestor (RN01).</summary>
    public const string TipoDeConta = "tipo";
}
