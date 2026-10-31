using CausaClara.Aplicacao.Acesso;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CausaClara.Api.Controllers;

/// <summary>Cadastro e dados cadastrais do usuário (RF02, RF03, RF04, RF07).</summary>
[ApiController]
[Route("api/contas")]
public sealed class ContasController(
    ServicoDeCadastro cadastro,
    ServicoDePerfil perfil) : ControllerBase
{
    /// <summary>Cria uma conta de Apoiador ou de Gestor.</summary>
    /// <remarks>
    /// Aberto, sem autenticação: é por aqui que alguém passa a ter conta.
    /// O tipo escolhido aqui não muda depois (RN01).
    /// </remarks>
    [HttpPost]
    [AllowAnonymous]
    [ProducesResponseType<PerfilDoUsuario>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PerfilDoUsuario>> Cadastrar(
        [FromBody] NovaConta nova,
        CancellationToken cancelamento)
    {
        var criado = await cadastro.CadastrarAsync(nova, cancelamento);
        return CreatedAtAction(nameof(ObterPerfil), null, criado);
    }

    /// <summary>Dados cadastrais de quem está autenticado (RF07).</summary>
    [HttpGet("eu")]
    [Authorize]
    [ProducesResponseType<PerfilDoUsuario>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PerfilDoUsuario>> ObterPerfil(CancellationToken cancelamento)
        => Ok(await perfil.ObterAsync(cancelamento));

    /// <summary>Atualiza nome e telefone. Tipo de conta não é alterável (RN01).</summary>
    [HttpPut("eu")]
    [Authorize]
    [ProducesResponseType<PerfilDoUsuario>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PerfilDoUsuario>> AtualizarPerfil(
        [FromBody] AtualizacaoDePerfil atualizacao,
        CancellationToken cancelamento)
        => Ok(await perfil.AtualizarAsync(atualizacao, cancelamento));
}
