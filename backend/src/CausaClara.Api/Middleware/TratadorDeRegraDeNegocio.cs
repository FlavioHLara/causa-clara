using CausaClara.Dominio.Comum;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace CausaClara.Api.Middleware;

/// <summary>
/// Converte <see cref="RegraDeNegocioViolada"/> em 400 com a mensagem do domínio.
/// </summary>
/// <remarks>
/// A mensagem da exceção já é escrita para o usuário final, em português e sem
/// informação técnica (RNF05), então ela atravessa sem reescrita. Violar regra de
/// negócio é resultado esperado de entrada inválida, não falha do sistema — por
/// isso o registro é em nível de informação, não de erro (RNF29).
/// </remarks>
public sealed class TratadorDeRegraDeNegocio(
    IProblemDetailsService servicoDeProblema,
    ILogger<TratadorDeRegraDeNegocio> registrador) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext contexto,
        Exception excecao,
        CancellationToken cancelamento)
    {
        if (excecao is not RegraDeNegocioViolada regra) return false;

        registrador.LogInformation(
            "Regra de negócio recusou {Metodo} {Caminho}: {Mensagem}",
            contexto.Request.Method, contexto.Request.Path, regra.Message);

        contexto.Response.StatusCode = StatusCodes.Status400BadRequest;

        return await servicoDeProblema.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = contexto,
            Exception = regra,
            ProblemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Não foi possível concluir a operação",
                Detail = regra.Message
            }
        });
    }
}
