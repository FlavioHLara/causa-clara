using CausaClara.Dominio.Comum;
using CausaClara.Dominio.Usuarios;

namespace CausaClara.Aplicacao.Comum;

/// <summary>
/// Uma mensagem a enviar. Não conhece SMTP, provedor nem formato de transporte.
/// </summary>
/// <param name="Destinatario">Endereço do destinatário, já normalizado.</param>
/// <param name="NomeDoDestinatario">Nome para o cabeçalho, opcional.</param>
/// <param name="Assunto">Assunto, em português (RNF06).</param>
/// <param name="CorpoHtml">Corpo em HTML.</param>
/// <param name="CorpoTexto">
/// Alternativa em texto puro. Sempre preenchida: cliente que não renderiza HTML
/// ainda precisa conseguir ler o link de redefinição de senha (RF09), e mensagem
/// só-HTML tem mais chance de ser classificada como indesejada.
/// </param>
public sealed record MensagemDeEmail(
    string Destinatario,
    string? NomeDoDestinatario,
    string Assunto,
    string CorpoHtml,
    string CorpoTexto)
{
    public const int TamanhoMaximoAssunto = 200;

    /// <summary>Valida e normaliza, para que endereço inválido falhe aqui e não no servidor de saída.</summary>
    public static MensagemDeEmail Criar(
        string destinatario,
        string? nomeDoDestinatario,
        string assunto,
        string corpoHtml,
        string corpoTexto)
        => new(
            Usuario.NormalizarEmail(destinatario),
            RegraDeNegocioViolada.TextoOpcional(nomeDoDestinatario, "o nome do destinatário", Usuario.TamanhoMaximoNome),
            RegraDeNegocioViolada.ExigirTexto(assunto, "o assunto da mensagem", TamanhoMaximoAssunto),
            RegraDeNegocioViolada.ExigirTexto(corpoHtml, "o corpo da mensagem", int.MaxValue),
            RegraDeNegocioViolada.ExigirTexto(corpoTexto, "o corpo em texto da mensagem", int.MaxValue));
}
