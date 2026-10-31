using System.Reflection;
using Microsoft.AspNetCore.Mvc;

namespace CausaClara.Api.Controllers;

/// <summary>
/// Identificação da instância em execução. Serve para confirmar que a API
/// respondeu, qual versão está no ar e em qual ambiente.
/// </summary>
/// <remarks>
/// Não consulta o banco de propósito: a verificação de dependências fica em
/// <c>/saude/pronto</c>, que é a sonda de prontidão do contêiner. Aqui a resposta
/// é barata e não cria acoplamento do controller com a persistência (RNF32).
/// </remarks>
[ApiController]
[Route("api/saude")]
public sealed class SaudeController(IWebHostEnvironment ambiente) : ControllerBase
{
    /// <summary>Devolve nome, versão, ambiente e horário UTC da instância.</summary>
    [HttpGet]
    [ProducesResponseType<RespostaDeSaude>(StatusCodes.Status200OK)]
    public ActionResult<RespostaDeSaude> Consultar()
    {
        var versao = Assembly.GetExecutingAssembly()
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
            ?? "desconhecida";

        return Ok(new RespostaDeSaude(
            Aplicacao: "Causa Clara",
            Versao: versao,
            Ambiente: ambiente.EnvironmentName,
            HorarioUtc: DateTime.UtcNow));   // RNF28: persistido e trafegado em UTC
    }

    /// <summary>Identificação da instância da API.</summary>
    /// <param name="Aplicacao">Nome do produto.</param>
    /// <param name="Versao">Versão informativa do assembly.</param>
    /// <param name="Ambiente">Ambiente de execução — Development, Production.</param>
    /// <param name="HorarioUtc">Horário do servidor em UTC.</param>
    public sealed record RespostaDeSaude(
        string Aplicacao,
        string Versao,
        string Ambiente,
        DateTime HorarioUtc);
}
