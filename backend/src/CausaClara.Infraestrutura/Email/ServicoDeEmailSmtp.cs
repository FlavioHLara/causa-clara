using CausaClara.Aplicacao.Comum;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Text;
using ClienteSmtp = MailKit.Net.Smtp.SmtpClient;

namespace CausaClara.Infraestrutura.Email;

/// <summary>Envia por SMTP, usando MailKit.</summary>
internal sealed class ServicoDeEmailSmtp(
    IOptions<OpcoesDeEmail> opcoes,
    ILogger<ServicoDeEmailSmtp> registrador) : IServicoDeEmail
{
    public async Task EnviarAsync(MensagemDeEmail mensagem, CancellationToken cancelamento = default)
    {
        ArgumentNullException.ThrowIfNull(mensagem);

        var configuracao = opcoes.Value;
        var destinatarioMascarado = Mascarar(mensagem.Destinatario);

        try
        {
            using var cliente = new ClienteSmtp
            {
                Timeout = configuracao.TempoLimiteEmSegundos * 1000
            };

            await cliente.ConnectAsync(
                configuracao.Servidor,
                configuracao.Porta,
                Traduzir(configuracao.Seguranca),
                cancelamento);

            if (!string.IsNullOrWhiteSpace(configuracao.Usuario))
            {
                // A verificação de inicialização já recusa usuário sem senha;
                // o fallback existe só para satisfazer a análise de nulidade.
                await cliente.AuthenticateAsync(
                    configuracao.Usuario,
                    configuracao.Senha ?? string.Empty,
                    cancelamento);
            }

            await cliente.SendAsync(Montar(mensagem, configuracao), cancelamento);
            await cliente.DisconnectAsync(quit: true, cancelamento);

            registrador.LogInformation(
                "Mensagem \"{Assunto}\" enviada para {Destinatario}.",
                mensagem.Assunto, destinatarioMascarado);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception excecao)
        {
            // RNF29: a falha vai para o log com o que permite diagnosticar, e o
            // endereço vai mascarado. RNF05: o usuário recebe mensagem genérica.
            registrador.LogError(
                excecao,
                "Falha ao enviar \"{Assunto}\" para {Destinatario} por {Servidor}:{Porta}.",
                mensagem.Assunto, destinatarioMascarado, configuracao.Servidor, configuracao.Porta);

            throw new FalhaAoEnviarEmail("Não foi possível enviar a mensagem de e-mail.", excecao);
        }
    }

    /// <summary>Monta a mensagem MIME. Separado do envio para poder ser verificado sem rede.</summary>
    internal static MimeMessage Montar(MensagemDeEmail mensagem, OpcoesDeEmail configuracao)
    {
        var mime = new MimeMessage();
        mime.From.Add(new MailboxAddress(configuracao.RemetenteNome, configuracao.RemetenteEndereco));
        mime.To.Add(new MailboxAddress(mensagem.NomeDoDestinatario ?? string.Empty, mensagem.Destinatario));
        mime.Subject = mensagem.Assunto;

        // Alternativa em texto junto do HTML: cliente que não renderiza HTML
        // precisa conseguir ler o link de redefinição de senha (RF09), e
        // mensagem só-HTML tem mais chance de ser classificada como indesejada.
        mime.Body = new MultipartAlternative
        {
            new TextPart(TextFormat.Plain) { Text = mensagem.CorpoTexto },
            new TextPart(TextFormat.Html) { Text = mensagem.CorpoHtml }
        };

        return mime;
    }

    /// <summary>
    /// <c>flavio@exemplo.com</c> vira <c>f*****@exemplo.com</c>.
    /// </summary>
    /// <remarks>
    /// Endereço de e-mail é dado pessoal (RNF19). O log precisa permitir
    /// diagnosticar um envio, não reconstruir a base de usuários — e arquivo de
    /// log costuma viver fora dos controles de acesso do banco.
    /// </remarks>
    internal static string Mascarar(string endereco)
    {
        var arroba = endereco.IndexOf('@', StringComparison.Ordinal);
        if (arroba <= 0) return "***";

        var parteLocal = endereco[..arroba];
        var dominio = endereco[arroba..];

        return parteLocal.Length == 1
            ? $"*{dominio}"
            : $"{parteLocal[0]}{new string('*', parteLocal.Length - 1)}{dominio}";
    }

    private static SecureSocketOptions Traduzir(SegurancaDoSmtp seguranca) => seguranca switch
    {
        SegurancaDoSmtp.Nenhuma => SecureSocketOptions.None,
        SegurancaDoSmtp.StartTls => SecureSocketOptions.StartTls,
        SegurancaDoSmtp.TlsImplicito => SecureSocketOptions.SslOnConnect,
        SegurancaDoSmtp.Automatica => SecureSocketOptions.Auto,
        _ => throw new ArgumentOutOfRangeException(nameof(seguranca), seguranca, "Opção de segurança desconhecida.")
    };
}
