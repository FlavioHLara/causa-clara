using CausaClara.Aplicacao.Acesso;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CausaClara.Api.Controllers;

/// <summary>Abertura, renovação e encerramento de sessão (RF05, RF06, RF08).</summary>
[ApiController]
[Route("api/sessao")]
public sealed class SessaoController(ServicoDeAutenticacao autenticacao) : ControllerBase
{
    /// <summary>Autentica por e-mail e senha e abre a sessão (RF05).</summary>
    [HttpPost]
    [AllowAnonymous]
    [ProducesResponseType<SessaoAberta>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SessaoAberta>> Abrir(
        [FromBody] Credenciais credenciais,
        CancellationToken cancelamento)
        => Ok(await autenticacao.AutenticarAsync(credenciais, cancelamento));

    /// <summary>
    /// Renova a credencial enquanto a sessão estiver ativa (RF06).
    /// </summary>
    /// <remarks>
    /// O token de renovação é de uso único: cada chamada devolve um novo e
    /// invalida o anterior. Renovar não estica o teto de oito horas do RNF13 —
    /// passado esse prazo, o usuário autentica de novo.
    /// </remarks>
    [HttpPost("renovacao")]
    [AllowAnonymous]
    [ProducesResponseType<SessaoAberta>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SessaoAberta>> Renovar(
        [FromBody] PedidoDeRenovacao pedido,
        CancellationToken cancelamento)
        => Ok(await autenticacao.RenovarAsync(pedido.TokenDeAtualizacao, cancelamento));

    /// <summary>Encerra a sessão, revogando o token de renovação no servidor (RF08).</summary>
    /// <remarks>
    /// Devolve 204 mesmo quando o token já não valia: sair não pode falhar, e
    /// distinguir os casos revelaria se um token existe.
    /// </remarks>
    [HttpDelete]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Encerrar(
        [FromBody] PedidoDeRenovacao pedido,
        CancellationToken cancelamento)
    {
        await autenticacao.EncerrarAsync(pedido.TokenDeAtualizacao, cancelamento);
        return NoContent();
    }

    /// <summary>Corpo das operações que recebem o token de renovação.</summary>
    public sealed record PedidoDeRenovacao(string TokenDeAtualizacao);
}
