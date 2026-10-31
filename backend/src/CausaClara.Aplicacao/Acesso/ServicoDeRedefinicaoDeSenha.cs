using CausaClara.Aplicacao.Comum;
using CausaClara.Dominio.Acesso;
using CausaClara.Dominio.Comum;
using CausaClara.Dominio.Usuarios;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CausaClara.Aplicacao.Acesso;

/// <summary>Redefinição de senha por link enviado ao e-mail (RF09).</summary>
public sealed class ServicoDeRedefinicaoDeSenha(
    IContextoDeDados dados,
    IHashDeSenha hashDeSenha,
    IProtetorDeSegredos protetor,
    IServicoDeEmail email,
    ServicoDeAutenticacao autenticacao,
    OpcoesDeAcesso opcoes,
    TimeProvider relogio,
    ILogger<ServicoDeRedefinicaoDeSenha> registrador)
{
    /// <summary>
    /// Registra o pedido e envia o link, se a conta existir.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Devolve sucesso sempre</b>, exista ou não a conta. Responder "e-mail
    /// não encontrado" transformaria esta rota num verificador de quais
    /// endereços têm conta — e ela é aberta, sem autenticação.
    /// </para>
    /// <para>
    /// Falha de envio também não vaza: é registrada no log e a resposta
    /// continua a mesma. O usuário legítimo tenta de novo; quem está sondando
    /// não aprende nada.
    /// </para>
    /// </remarks>
    public async Task SolicitarAsync(PedidoDeRedefinicao pedido, CancellationToken cancelamento = default)
    {
        ArgumentNullException.ThrowIfNull(pedido);

        string endereco;
        try
        {
            endereco = Usuario.NormalizarEmail(pedido.Email);
        }
        catch (RegraDeNegocioViolada)
        {
            return;   // e-mail malformado: mesma resposta silenciosa
        }

        var usuario = await dados.Usuarios.FirstOrDefaultAsync(u => u.Email == endereco, cancelamento);
        if (usuario is null)
        {
            registrador.LogInformation("Pedido de redefinição para endereço sem conta. Nenhuma mensagem enviada.");
            return;
        }

        var agora = relogio.GetUtcNow().UtcDateTime;
        var (segredo, resumo) = protetor.Gerar();

        dados.TokensDeRedefinicao.Add(TokenDeRedefinicaoDeSenha.Emitir(
            usuario.Id, resumo, agora, opcoes.ValidadeDoLinkDeRedefinicao));

        await dados.SalvarAlteracoesAsync(cancelamento);

        try
        {
            await email.EnviarAsync(MontarMensagem(usuario, segredo), cancelamento);
        }
        catch (FalhaAoEnviarEmail excecao)
        {
            // O token ficou gravado; só a entrega falhou. Não desfaz nada e não
            // conta ao cliente, pelo mesmo motivo da resposta silenciosa.
            registrador.LogError(excecao, "Falha ao entregar o link de redefinição de senha.");
        }
    }

    /// <summary>RF09: troca a senha e derruba as sessões abertas.</summary>
    public async Task RedefinirAsync(NovaSenha nova, CancellationToken cancelamento = default)
    {
        ArgumentNullException.ThrowIfNull(nova);

        RegraDeNegocioViolada.Se(
            !string.Equals(nova.Senha, nova.ConfirmacaoDeSenha, StringComparison.Ordinal),
            "A senha e a confirmação não coincidem.");

        ServicoDeCadastro.ConferirSenha(nova.Senha, opcoes.TamanhoMinimoDaSenha);

        RegraDeNegocioViolada.Se(
            string.IsNullOrWhiteSpace(nova.Token),
            "Link de redefinição inválido ou expirado. Peça um novo.");

        var agora = relogio.GetUtcNow().UtcDateTime;
        var resumo = protetor.Resumir(nova.Token);

        var token = await dados.TokensDeRedefinicao.FirstOrDefaultAsync(t => t.Resumo == resumo, cancelamento);

        // Mensagem única para token inexistente, expirado e já usado: os três
        // levam à mesma ação do usuário, que é pedir outro link.
        RegraDeNegocioViolada.Se(
            token is null || !token.EstaValido(agora),
            "Link de redefinição inválido ou expirado. Peça um novo.");

        var usuario = await dados.Usuarios.FirstOrDefaultAsync(u => u.Id == token!.UsuarioId, cancelamento);
        RegraDeNegocioViolada.Se(usuario is null, "Link de redefinição inválido ou expirado. Peça um novo.");

        token!.MarcarComoUsado(agora);
        usuario!.DefinirSenha(hashDeSenha.Gerar(nova.Senha));

        // Quem troca a senha por suspeita de acesso indevido espera que as
        // sessões abertas caiam junto.
        await autenticacao.RevogarSessoesAsync(usuario.Id, cancelamento);

        await dados.SalvarAlteracoesAsync(cancelamento);
    }

    private MensagemDeEmail MontarMensagem(Usuario usuario, string segredo)
    {
        var link = opcoes.MontarLinkDeRedefinicao(segredo);
        var minutos = (int)opcoes.ValidadeDoLinkDeRedefinicao.TotalMinutes;
        var primeiroNome = usuario.NomeCompleto.Split(' ')[0];

        var texto =
            $"""
             Olá, {primeiroNome}.

             Recebemos um pedido para redefinir a senha da sua conta na Causa Clara.
             Abra o endereço abaixo para escolher uma nova senha. O link vale por {minutos} minutos.

             {link}

             Se não foi você quem pediu, ignore esta mensagem: sua senha continua a mesma.

             Causa Clara
             """;

        var html =
            $"""
             <p>Olá, {primeiroNome}.</p>
             <p>Recebemos um pedido para redefinir a senha da sua conta na <strong>Causa Clara</strong>.</p>
             <p><a href="{link}">Escolher uma nova senha</a></p>
             <p>O link vale por {minutos} minutos.</p>
             <p>Se não foi você quem pediu, ignore esta mensagem: sua senha continua a mesma.</p>
             <p>Causa Clara</p>
             """;

        return MensagemDeEmail.Criar(
            usuario.Email,
            usuario.NomeCompleto,
            "Redefinição de senha",
            html,
            texto);
    }
}
