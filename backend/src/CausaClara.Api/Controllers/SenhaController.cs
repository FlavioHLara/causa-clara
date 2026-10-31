using CausaClara.Aplicacao.Acesso;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CausaClara.Api.Controllers;

/// <summary>Redefinição de senha por link enviado ao e-mail (RF09).</summary>
[ApiController]
[Route("api/senha")]
public sealed class SenhaController(ServicoDeRedefinicaoDeSenha redefinicao) : ControllerBase
{
    /// <summary>Pede o link de redefinição.</summary>
    /// <remarks>
    /// Responde 202 sempre, exista ou não a conta. Dizer "e-mail não
    /// encontrado" transformaria esta rota, que é aberta, num verificador de
    /// quais endereços têm conta cadastrada.
    /// </remarks>
    [HttpPost("redefinicao")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    public async Task<IActionResult> Solicitar(
        [FromBody] PedidoDeRedefinicao pedido,
        CancellationToken cancelamento)
    {
        await redefinicao.SolicitarAsync(pedido, cancelamento);

        return Accepted(new
        {
            mensagem = "Se houver uma conta com este e-mail, enviaremos o link de redefinição."
        });
    }

    /// <summary>Troca a senha usando o segredo recebido por e-mail.</summary>
    /// <remarks>Derruba as sessões abertas do usuário: trocar a senha encerra o que estava em uso.</remarks>
    [HttpPut]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Redefinir(
        [FromBody] NovaSenha nova,
        CancellationToken cancelamento)
    {
        await redefinicao.RedefinirAsync(nova, cancelamento);
        return NoContent();
    }
}
