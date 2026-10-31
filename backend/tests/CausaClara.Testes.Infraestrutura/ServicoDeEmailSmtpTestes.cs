using CausaClara.Aplicacao.Comum;
using CausaClara.Infraestrutura.Email;
using MimeKit;
using MimeKit.Text;

namespace CausaClara.Testes.Infraestrutura;

/// <summary>
/// Montagem da mensagem e mascaramento do destinatário no log.
/// </summary>
/// <remarks>
/// O que se verifica aqui é o código do projeto, não o MailKit: que a mensagem
/// sai com remetente, destinatário, assunto e as duas alternativas de corpo, e
/// que o log não reconstrói endereços. Falar SMTP é responsabilidade da
/// biblioteca, e testar isso seria testar terceiro.
/// </remarks>
public sealed class ServicoDeEmailSmtpTestes
{
    private static readonly OpcoesDeEmail Configuracao = new()
    {
        Servidor = "email",
        Porta = 1025,
        Seguranca = SegurancaDoSmtp.Nenhuma,
        RemetenteEndereco = "nao-responda@causaclara.local",
        RemetenteNome = "Causa Clara"
    };

    private static MensagemDeEmail Mensagem() => MensagemDeEmail.Criar(
        destinatario: "Flavio@Exemplo.COM",
        nomeDoDestinatario: "Flávio Henrique",
        assunto: "Redefinição de senha",
        corpoHtml: "<p>Use o link para redefinir sua senha.</p>",
        corpoTexto: "Use o link para redefinir sua senha.");

    [Fact]
    public void A_mensagem_sai_com_o_remetente_configurado()
    {
        var mime = ServicoDeEmailSmtp.Montar(Mensagem(), Configuracao);

        var remetente = Assert.IsType<MailboxAddress>(Assert.Single(mime.From));
        Assert.Equal("nao-responda@causaclara.local", remetente.Address);
        Assert.Equal("Causa Clara", remetente.Name);
    }

    [Fact]
    public void O_destinatario_vai_normalizado_em_caixa_baixa()
    {
        var mime = ServicoDeEmailSmtp.Montar(Mensagem(), Configuracao);

        var destinatario = Assert.IsType<MailboxAddress>(Assert.Single(mime.To));
        Assert.Equal("flavio@exemplo.com", destinatario.Address);
        Assert.Equal("Flávio Henrique", destinatario.Name);
    }

    [Fact]
    public void O_assunto_atravessa_sem_alteracao()
        => Assert.Equal("Redefinição de senha", ServicoDeEmailSmtp.Montar(Mensagem(), Configuracao).Subject);

    [Fact]
    public void RF09_a_mensagem_leva_texto_puro_alem_do_HTML()
    {
        // Sem a alternativa em texto, quem lê e-mail sem HTML não enxerga o link
        // de redefinição de senha — e a mensagem ainda tem mais chance de cair
        // como indesejada.
        var mime = ServicoDeEmailSmtp.Montar(Mensagem(), Configuracao);

        var alternativas = Assert.IsType<MultipartAlternative>(mime.Body);
        Assert.Equal(2, alternativas.Count);

        var texto = Assert.IsType<TextPart>(alternativas[0]);
        var html = Assert.IsType<TextPart>(alternativas[1]);

        Assert.True(texto.IsPlain, "A primeira alternativa deve ser o texto puro.");
        Assert.True(html.IsHtml);
        Assert.Contains("redefinir sua senha", texto.Text, StringComparison.Ordinal);
    }

    // ----- RNF19: o log não reconstrói a base de endereços -----

    [Theory]
    [InlineData("flavio@exemplo.com", "f*****@exemplo.com")]
    [InlineData("a@b.com", "*@b.com")]
    [InlineData("contato@casasemear.org.br", "c******@casasemear.org.br")]
    public void O_endereco_e_mascarado_preservando_o_dominio(string endereco, string esperado)
        => Assert.Equal(esperado, ServicoDeEmailSmtp.Mascarar(endereco));

    [Theory]
    [InlineData("sem-arroba")]
    [InlineData("")]
    [InlineData("@so-dominio.com")]
    public void Endereco_sem_parte_local_nao_vaza_nada(string estranho)
        => Assert.Equal("***", ServicoDeEmailSmtp.Mascarar(estranho));
}
