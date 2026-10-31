using System.Globalization;
using System.Security.Claims;
using System.Text;
using CausaClara.Aplicacao.Acesso;
using CausaClara.Dominio.Usuarios;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace CausaClara.Infraestrutura.Seguranca;

/// <summary>
/// Emite a credencial assinada de acesso, em JWT com HMAC-SHA256 (RNF13).
/// </summary>
/// <remarks>
/// <para>
/// Assinatura simétrica, e não par de chaves: há um único emissor e um único
/// verificador, o mesmo processo. Chave assimétrica serve quando terceiros
/// precisam validar sem poder emitir, o que não acontece aqui — seria
/// complexidade sem destinatário.
/// </para>
/// <para>
/// <b>O que entra no token, e por quê isso importa.</b> O identificador do
/// usuário e o tipo de conta são fatos imutáveis (RN01). O vínculo
/// institucional, não: ele pode ser aprovado ou revogado a qualquer momento.
/// Como ele viaja dentro da credencial, revogar um vínculo só tem efeito pleno
/// quando o token em uso expira — por isso a vida curta configurada em
/// <see cref="OpcoesDeToken.ValidadeEmMinutos"/>, e por isso revogar um vínculo
/// também revoga as sessões do usuário, impedindo a renovação.
/// </para>
/// </remarks>
internal sealed class GeradorDeTokenJwt(IOptions<OpcoesDeToken> opcoes, TimeProvider relogio)
    : IGeradorDeTokenDeAcesso
{
    private readonly JsonWebTokenHandler _manipulador = new();

    public TokenDeAcessoEmitido Emitir(Usuario usuario, int? instituicaoId)
    {
        ArgumentNullException.ThrowIfNull(usuario);

        var configuracao = opcoes.Value;
        var agora = relogio.GetUtcNow().UtcDateTime;
        var expiraEm = agora.AddMinutes(configuracao.ValidadeEmMinutos);

        var identidade = new ClaimsIdentity(
        [
            new Claim(JwtRegisteredClaimNames.Sub, usuario.Id.ToString(CultureInfo.InvariantCulture)),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
            new Claim(TiposDeClaim.UsuarioId, usuario.Id.ToString(CultureInfo.InvariantCulture)),
            new Claim(TiposDeClaim.TipoDeConta, usuario.Tipo.ToString())
        ]);

        if (instituicaoId is not null)
        {
            identidade.AddClaim(new Claim(
                TiposDeClaim.InstituicaoId,
                instituicaoId.Value.ToString(CultureInfo.InvariantCulture)));
        }

        var descritor = new SecurityTokenDescriptor
        {
            Issuer = configuracao.Emissor,
            Audience = configuracao.Audiencia,
            Subject = identidade,
            IssuedAt = agora,
            NotBefore = agora,
            Expires = expiraEm,
            SigningCredentials = new SigningCredentials(
                configuracao.ChaveDeAssinatura(),
                SecurityAlgorithms.HmacSha256)
        };

        return new TokenDeAcessoEmitido(_manipulador.CreateToken(descritor), expiraEm);
    }
}
