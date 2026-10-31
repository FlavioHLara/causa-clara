using System.Globalization;
using System.Security.Claims;
using CausaClara.Aplicacao.Acesso;
using CausaClara.Aplicacao.Comum;
using CausaClara.Dominio.Usuarios;

namespace CausaClara.Api.Autenticacao;

/// <summary>
/// Lê das <em>claims</em> do token quem está fazendo a requisição.
/// </summary>
/// <remarks>
/// <para>
/// Mora na Api porque resolver o usuário depende de HTTP, e nem a Aplicação nem
/// a Persistência conhecem HTTP. As duas conversam com
/// <see cref="IContextoDaRequisicao"/>.
/// </para>
/// <para>
/// Leitura tolerante de propósito: <em>claim</em> ausente ou malformada vira
/// nulo, nunca exceção. Com instituição nula o filtro global não casa com
/// nenhuma linha, de modo que o pior caso é não ver nada — e não ver dado alheio.
/// Recusar a requisição é atribuição da autorização, na etapa 3, não desta classe.
/// </para>
/// </remarks>
public sealed class ContextoDaRequisicao(IHttpContextAccessor acessor) : IContextoDaRequisicao
{
    private ClaimsPrincipal? Usuario => acessor.HttpContext?.User;

    public bool Autenticado => Usuario?.Identity?.IsAuthenticated ?? false;

    public int? UsuarioId => InteiroDaClaim(TiposDeClaim.UsuarioId);

    public int? InstituicaoId => InteiroDaClaim(TiposDeClaim.InstituicaoId);

    public TipoUsuario? TipoDeConta
        => Enum.TryParse<TipoUsuario>(Usuario?.FindFirstValue(TiposDeClaim.TipoDeConta), out var tipo)
            && Enum.IsDefined(tipo)
                ? tipo
                : null;

    private int? InteiroDaClaim(string nome)
        => int.TryParse(
            Usuario?.FindFirstValue(nome),
            NumberStyles.Integer,
            CultureInfo.InvariantCulture,
            out var valor)
            ? valor
            : null;
}
