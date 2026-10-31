namespace CausaClara.Testes.Infraestrutura;

/// <summary>
/// Teste que só roda quando há um servidor SMTP de verdade para falar.
/// </summary>
/// <remarks>
/// <para>
/// O xUnit 2 decide o salto na descoberta, então a condição é lida da variável
/// de ambiente antes da execução. Um teste saltado aparece como "Ignorado" no
/// relatório — fica visível, em vez de sumir.
/// </para>
/// <para>
/// Para habilitar, com o ambiente do compose de pé:
/// <code>
/// $env:CAUSACLARA_SMTP_DE_TESTE = "localhost:1025"
/// dotnet test
/// </code>
/// </para>
/// </remarks>
public sealed class FatoComServidorSmtpAttribute : FactAttribute
{
    public const string Variavel = "CAUSACLARA_SMTP_DE_TESTE";

    public FatoComServidorSmtpAttribute()
    {
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(Variavel)))
        {
            Skip = $"Defina {Variavel}=servidor:porta, com o ambiente do compose de pé, para exercitar o envio real.";
        }
    }
}
