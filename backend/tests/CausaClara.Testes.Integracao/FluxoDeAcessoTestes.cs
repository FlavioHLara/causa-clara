using CausaClara.Aplicacao.Acesso;
using CausaClara.Aplicacao.Comum;
using CausaClara.Dominio.Comum;
using CausaClara.Dominio.Usuarios;
using CausaClara.Testes.Integracao.Apoio;
using Microsoft.EntityFrameworkCore;

namespace CausaClara.Testes.Integracao;

/// <summary>
/// Cadastro, autenticação, sessão e redefinição de senha, de ponta a ponta sobre
/// um banco real (RF02 a RF09).
/// </summary>
/// <remarks>
/// Roda com as implementações verdadeiras de resumo de senha e de segredo. Só o
/// relógio, o envio de e-mail e a emissão do token são substituídos — o relógio
/// para exercitar prazos sem esperar, e os outros dois porque o que está sob
/// teste aqui é o fluxo, não o formato da credencial nem o transporte SMTP.
/// </remarks>
public sealed class FluxoDeAcessoTestes
{
    // ----- RF02 a RF04: cadastro -----

    [Fact]
    public async Task RF03_a_conta_e_criada_com_os_dados_informados()
    {
        using var cenario = new CenarioDeAcesso();

        var perfil = await cenario.Cadastro.CadastrarAsync(CenarioDeAcesso.ContaValida());

        Assert.Equal("flavio@exemplo.com", perfil.Email);
        Assert.Equal(TipoUsuario.Apoiador, perfil.Tipo);
        Assert.True(perfil.Id > 0);
    }

    [Fact]
    public async Task RNF10_a_senha_nunca_e_guardada_em_claro()
    {
        using var cenario = new CenarioDeAcesso();
        const string senha = "senha-bem-comprida";

        await cenario.Cadastro.CadastrarAsync(CenarioDeAcesso.ContaValida(senha: senha));

        var guardado = await cenario.Contexto.Usuarios.AsNoTracking().SingleAsync();

        Assert.NotEqual(senha, guardado.SenhaHash);
        Assert.DoesNotContain(senha, guardado.SenhaHash, StringComparison.Ordinal);
        Assert.Equal(VerificacaoDeSenha.Valida, cenario.Hash.Verificar(guardado.SenhaHash, senha));
    }

    [Fact]
    public async Task RNF10_duas_contas_com_a_mesma_senha_tem_resumos_diferentes()
    {
        // É o efeito do sal individual: sem ele, resumos iguais entregariam
        // quais contas compartilham senha.
        using var cenario = new CenarioDeAcesso();

        await cenario.Cadastro.CadastrarAsync(CenarioDeAcesso.ContaValida("um@exemplo.com"));
        await cenario.Cadastro.CadastrarAsync(CenarioDeAcesso.ContaValida("dois@exemplo.com"));

        var resumos = await cenario.Contexto.Usuarios.AsNoTracking().Select(u => u.SenhaHash).ToListAsync();

        Assert.Equal(2, resumos.Distinct(StringComparer.Ordinal).Count());
    }

    [Fact]
    public async Task RF04_o_segundo_cadastro_com_o_mesmo_email_e_recusado()
    {
        using var cenario = new CenarioDeAcesso();
        await cenario.Cadastro.CadastrarAsync(CenarioDeAcesso.ContaValida());

        var excecao = await Assert.ThrowsAsync<RegraDeNegocioViolada>(
            () => cenario.Cadastro.CadastrarAsync(CenarioDeAcesso.ContaValida("FLAVIO@Exemplo.com")));

        Assert.Contains("já existe", excecao.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task A_confirmacao_de_senha_precisa_coincidir()
    {
        using var cenario = new CenarioDeAcesso();
        var conta = CenarioDeAcesso.ContaValida() with { ConfirmacaoDeSenha = "outra-coisa" };

        await Assert.ThrowsAsync<RegraDeNegocioViolada>(() => cenario.Cadastro.CadastrarAsync(conta));
    }

    [Fact]
    public async Task RNF15_senha_curta_e_recusada_no_servidor()
    {
        using var cenario = new CenarioDeAcesso();
        var conta = CenarioDeAcesso.ContaValida(senha: "curta");

        var excecao = await Assert.ThrowsAsync<RegraDeNegocioViolada>(
            () => cenario.Cadastro.CadastrarAsync(conta));

        Assert.Contains("caracteres", excecao.Message, StringComparison.OrdinalIgnoreCase);
    }

    // ----- RF05: autenticação -----

    [Fact]
    public async Task RF05_credenciais_corretas_abrem_a_sessao()
    {
        using var cenario = new CenarioDeAcesso();
        await cenario.Cadastro.CadastrarAsync(CenarioDeAcesso.ContaValida());

        var sessao = await cenario.Autenticacao.AutenticarAsync(
            new Credenciais("flavio@exemplo.com", "senha-bem-comprida"));

        Assert.False(string.IsNullOrWhiteSpace(sessao.TokenDeAcesso));
        Assert.False(string.IsNullOrWhiteSpace(sessao.TokenDeAtualizacao));
        Assert.Equal("flavio@exemplo.com", sessao.Perfil.Email);
    }

    [Theory]
    [InlineData("flavio@exemplo.com", "senha-errada-mas-comprida")]
    [InlineData("naoexiste@exemplo.com", "senha-bem-comprida")]
    public async Task RF05_a_recusa_nao_diz_se_o_email_existe(string email, string senha)
    {
        // A mesma mensagem nos dois casos. Distinguir transformaria a tela de
        // acesso num verificador de quais endereços têm conta.
        using var cenario = new CenarioDeAcesso();
        await cenario.Cadastro.CadastrarAsync(CenarioDeAcesso.ContaValida());

        var excecao = await Assert.ThrowsAsync<RegraDeNegocioViolada>(
            () => cenario.Autenticacao.AutenticarAsync(new Credenciais(email, senha)));

        Assert.Equal("E-mail ou senha incorretos.", excecao.Message);
    }

    [Fact]
    public async Task O_segredo_de_sessao_nao_fica_guardado_no_banco()
    {
        using var cenario = new CenarioDeAcesso();
        await cenario.Cadastro.CadastrarAsync(CenarioDeAcesso.ContaValida());

        var sessao = await cenario.Autenticacao.AutenticarAsync(
            new Credenciais("flavio@exemplo.com", "senha-bem-comprida"));

        var guardado = await cenario.Contexto.TokensDeAtualizacao.AsNoTracking().SingleAsync();

        Assert.NotEqual(sessao.TokenDeAtualizacao, guardado.Resumo);
        Assert.Equal(cenario.Protetor.Resumir(sessao.TokenDeAtualizacao), guardado.Resumo);
    }

    // ----- RF06 e RF08: renovação e encerramento -----

    [Fact]
    public async Task RF06_renovar_emite_credencial_nova_e_invalida_a_anterior()
    {
        using var cenario = new CenarioDeAcesso();
        await cenario.Cadastro.CadastrarAsync(CenarioDeAcesso.ContaValida());
        var primeira = await cenario.Autenticacao.AutenticarAsync(
            new Credenciais("flavio@exemplo.com", "senha-bem-comprida"));

        cenario.Relogio.Avancar(TimeSpan.FromMinutes(30));
        var segunda = await cenario.Autenticacao.RenovarAsync(primeira.TokenDeAtualizacao);

        Assert.NotEqual(primeira.TokenDeAtualizacao, segunda.TokenDeAtualizacao);

        // Rotação: o token usado não serve mais, então uma cópia vazada perde
        // o valor assim que o dono legítimo renova.
        await Assert.ThrowsAsync<RegraDeNegocioViolada>(
            () => cenario.Autenticacao.RenovarAsync(primeira.TokenDeAtualizacao));
    }

    [Fact]
    public async Task RNF13_a_sessao_nao_passa_de_oito_horas_mesmo_renovando()
    {
        using var cenario = new CenarioDeAcesso();
        await cenario.Cadastro.CadastrarAsync(CenarioDeAcesso.ContaValida());
        var sessao = await cenario.Autenticacao.AutenticarAsync(
            new Credenciais("flavio@exemplo.com", "senha-bem-comprida"));

        // Renova de hora em hora até o limite: o prazo original é preservado,
        // renovar não estica o teto.
        for (var hora = 0; hora < 7; hora++)
        {
            cenario.Relogio.Avancar(TimeSpan.FromHours(1));
            sessao = await cenario.Autenticacao.RenovarAsync(sessao.TokenDeAtualizacao);
        }

        cenario.Relogio.Avancar(TimeSpan.FromHours(1));

        var excecao = await Assert.ThrowsAsync<RegraDeNegocioViolada>(
            () => cenario.Autenticacao.RenovarAsync(sessao.TokenDeAtualizacao));

        Assert.Contains("expirada", excecao.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task RF08_encerrar_impede_a_renovacao()
    {
        using var cenario = new CenarioDeAcesso();
        await cenario.Cadastro.CadastrarAsync(CenarioDeAcesso.ContaValida());
        var sessao = await cenario.Autenticacao.AutenticarAsync(
            new Credenciais("flavio@exemplo.com", "senha-bem-comprida"));

        await cenario.Autenticacao.EncerrarAsync(sessao.TokenDeAtualizacao);

        await Assert.ThrowsAsync<RegraDeNegocioViolada>(
            () => cenario.Autenticacao.RenovarAsync(sessao.TokenDeAtualizacao));
    }

    [Fact]
    public async Task RF08_encerrar_sessao_inexistente_nao_falha()
    {
        // Sair não pode falhar, e distinguir os casos revelaria se um token existe.
        using var cenario = new CenarioDeAcesso();

        await cenario.Autenticacao.EncerrarAsync("segredo-que-nunca-existiu");
        await cenario.Autenticacao.EncerrarAsync(null);
    }

    // ----- RF09: redefinição de senha -----

    [Fact]
    public async Task RF09_o_pedido_envia_o_link_para_quem_tem_conta()
    {
        using var cenario = new CenarioDeAcesso();
        await cenario.Cadastro.CadastrarAsync(CenarioDeAcesso.ContaValida());

        await cenario.Redefinicao.SolicitarAsync(new PedidoDeRedefinicao("flavio@exemplo.com"));

        var mensagem = Assert.Single(cenario.Email.Enviadas);
        Assert.Equal("flavio@exemplo.com", mensagem.Destinatario);
        Assert.Contains("token=", mensagem.CorpoTexto, StringComparison.Ordinal);
        Assert.Contains("token=", mensagem.CorpoHtml, StringComparison.Ordinal);
    }

    [Fact]
    public async Task RF09_o_pedido_para_email_sem_conta_nao_envia_nada_e_nao_falha()
    {
        // Responder diferente aqui transformaria a rota, que é aberta, num
        // verificador de quais endereços têm conta.
        using var cenario = new CenarioDeAcesso();

        await cenario.Redefinicao.SolicitarAsync(new PedidoDeRedefinicao("ninguem@exemplo.com"));

        Assert.Empty(cenario.Email.Enviadas);
    }

    [Fact]
    public async Task RF09_o_link_troca_a_senha_e_derruba_as_sessoes_abertas()
    {
        using var cenario = new CenarioDeAcesso();
        await cenario.Cadastro.CadastrarAsync(CenarioDeAcesso.ContaValida());
        var sessao = await cenario.Autenticacao.AutenticarAsync(
            new Credenciais("flavio@exemplo.com", "senha-bem-comprida"));

        await cenario.Redefinicao.SolicitarAsync(new PedidoDeRedefinicao("flavio@exemplo.com"));
        var segredo = ExtrairToken(cenario.Email.Enviadas[0].CorpoTexto);

        await cenario.Redefinicao.RedefinirAsync(new NovaSenha(segredo, "nova-senha-comprida", "nova-senha-comprida"));

        // A senha nova vale.
        var nova = await cenario.Autenticacao.AutenticarAsync(
            new Credenciais("flavio@exemplo.com", "nova-senha-comprida"));
        Assert.False(string.IsNullOrWhiteSpace(nova.TokenDeAcesso));

        // A antiga, não.
        await Assert.ThrowsAsync<RegraDeNegocioViolada>(
            () => cenario.Autenticacao.AutenticarAsync(
                new Credenciais("flavio@exemplo.com", "senha-bem-comprida")));

        // E quem trocou a senha por suspeita de acesso indevido espera que a
        // sessão que estava aberta caia junto.
        await Assert.ThrowsAsync<RegraDeNegocioViolada>(
            () => cenario.Autenticacao.RenovarAsync(sessao.TokenDeAtualizacao));
    }

    [Fact]
    public async Task RF09_o_link_expirado_e_recusado()
    {
        using var cenario = new CenarioDeAcesso();
        await cenario.Cadastro.CadastrarAsync(CenarioDeAcesso.ContaValida());
        await cenario.Redefinicao.SolicitarAsync(new PedidoDeRedefinicao("flavio@exemplo.com"));
        var segredo = ExtrairToken(cenario.Email.Enviadas[0].CorpoTexto);

        cenario.Relogio.Avancar(cenario.Opcoes.ValidadeDoLinkDeRedefinicao + TimeSpan.FromMinutes(1));

        var excecao = await Assert.ThrowsAsync<RegraDeNegocioViolada>(
            () => cenario.Redefinicao.RedefinirAsync(
                new NovaSenha(segredo, "nova-senha-comprida", "nova-senha-comprida")));

        Assert.Contains("expirado", excecao.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task RF09_o_mesmo_link_nao_serve_duas_vezes()
    {
        using var cenario = new CenarioDeAcesso();
        await cenario.Cadastro.CadastrarAsync(CenarioDeAcesso.ContaValida());
        await cenario.Redefinicao.SolicitarAsync(new PedidoDeRedefinicao("flavio@exemplo.com"));
        var segredo = ExtrairToken(cenario.Email.Enviadas[0].CorpoTexto);

        await cenario.Redefinicao.RedefinirAsync(new NovaSenha(segredo, "primeira-nova-senha", "primeira-nova-senha"));

        await Assert.ThrowsAsync<RegraDeNegocioViolada>(
            () => cenario.Redefinicao.RedefinirAsync(
                new NovaSenha(segredo, "segunda-nova-senha", "segunda-nova-senha")));
    }

    private static string ExtrairToken(string corpo)
    {
        var inicio = corpo.IndexOf("token=", StringComparison.Ordinal) + "token=".Length;
        var fim = corpo.IndexOfAny([' ', '\r', '\n'], inicio);
        return Uri.UnescapeDataString(fim < 0 ? corpo[inicio..] : corpo[inicio..fim]);
    }
}
