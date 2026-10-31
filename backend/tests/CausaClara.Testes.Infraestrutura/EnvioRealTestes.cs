using CausaClara.Aplicacao.Comum;
using CausaClara.Infraestrutura.Email;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace CausaClara.Testes.Infraestrutura;

/// <summary>
/// Envio de verdade contra a captura de mensagens do ambiente local.
/// </summary>
/// <remarks>
/// <para>
/// Os outros testes verificam a montagem da mensagem sem rede. Este fecha o
/// único trecho que eles não alcançam: se o adaptador realmente fala SMTP com um
/// servidor. Por depender do ambiente do compose, roda apenas quando
/// <c>CAUSACLARA_SMTP_DE_TESTE</c> aponta para o servidor — ou seja, nunca por
/// acidente numa execução comum de <c>dotnet test</c>.
/// </para>
/// <para>
/// Para rodar: <c>docker compose up -d</c>, depois
/// <c>$env:CAUSACLARA_SMTP_DE_TESTE="localhost:1025"; dotnet test</c>.
/// </para>
/// </remarks>
public sealed class EnvioRealTestes
{
    [FatoComServidorSmtp]
    public async Task O_adaptador_entrega_a_mensagem_ao_servidor_SMTP()
    {
        var destino = Environment.GetEnvironmentVariable(FatoComServidorSmtpAttribute.Variavel)!;
        var partes = destino.Split(':');
        var opcoes = Options.Create(new OpcoesDeEmail
        {
            Servidor = partes[0],
            Porta = int.Parse(partes[1]),
            Seguranca = SegurancaDoSmtp.Nenhuma,
            RemetenteEndereco = "nao-responda@causaclara.local",
            RemetenteNome = "Causa Clara"
        });

        var servico = new ServicoDeEmailSmtp(opcoes, NullLogger<ServicoDeEmailSmtp>.Instance);

        var mensagem = MensagemDeEmail.Criar(
            destinatario: "flavio@exemplo.com",
            nomeDoDestinatario: "Flávio Henrique",
            assunto: "Verificação de envio",
            corpoHtml: "<p>Mensagem de verificação do adaptador SMTP.</p>",
            corpoTexto: "Mensagem de verificação do adaptador SMTP.");

        // Sem exceção, a mensagem atravessou conexão, diálogo SMTP e entrega.
        await servico.EnviarAsync(mensagem);
    }
}
