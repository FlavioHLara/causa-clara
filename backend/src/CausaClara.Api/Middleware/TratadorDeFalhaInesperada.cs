using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace CausaClara.Api.Middleware;

/// <summary>
/// Rede de segurança: qualquer exceção não tratada vira 500 com mensagem genérica.
/// </summary>
/// <remarks>
/// RNF05 proíbe expor informação técnica ao usuário, e RNF29 exige registrar a
/// falha com data, operação e usuário, sem dados sensíveis. O detalhe fica no log
/// e o cliente recebe apenas o identificador da requisição, que permite localizar
/// a ocorrência no log sem revelar nada sobre a implementação.
/// </remarks>
public sealed class TratadorDeFalhaInesperada(
    IProblemDetailsService servicoDeProblema,
    ILogger<TratadorDeFalhaInesperada> registrador) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext contexto,
        Exception excecao,
        CancellationToken cancelamento)
    {
        var identificador = contexto.TraceIdentifier;

        registrador.LogError(
            excecao,
            "Falha não tratada em {Metodo} {Caminho}. Usuário {Usuario}. Requisição {Identificador}.",
            contexto.Request.Method,
            contexto.Request.Path,
            contexto.User.Identity?.Name ?? "anônimo",
            identificador);

        contexto.Response.StatusCode = StatusCodes.Status500InternalServerError;

        return await servicoDeProblema.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = contexto,
            Exception = excecao,
            ProblemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Erro inesperado",
                Detail = "Não foi possível concluir a operação. Tente novamente em instantes.",
                Extensions = { ["identificadorDaRequisicao"] = identificador }
            }
        });
    }
}
