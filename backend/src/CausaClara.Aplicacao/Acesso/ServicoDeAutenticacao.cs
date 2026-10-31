using CausaClara.Aplicacao.Comum;
using CausaClara.Dominio.Acesso;
using CausaClara.Dominio.Comum;
using CausaClara.Dominio.Usuarios;
using Microsoft.EntityFrameworkCore;

namespace CausaClara.Aplicacao.Acesso;

/// <summary>Abertura, renovação e encerramento de sessão (RF05, RF06, RF08).</summary>
public sealed class ServicoDeAutenticacao(
    IContextoDeDados dados,
    IHashDeSenha hashDeSenha,
    IProtetorDeSegredos protetor,
    IGeradorDeTokenDeAcesso gerador,
    OpcoesDeAcesso opcoes,
    TimeProvider relogio)
{
    /// <summary>
    /// Resumo de uma senha qualquer, usado quando o e-mail não existe.
    /// </summary>
    /// <remarks>
    /// Sem isto, a resposta sairia na hora para e-mail inexistente e só depois
    /// do cálculo do resumo para e-mail existente. A diferença de tempo diria
    /// quais endereços estão cadastrados — a mesma informação que a mensagem de
    /// erro genérica esconde.
    /// </remarks>
    private readonly Lazy<string> _resumoDeReferencia =
        new(() => hashDeSenha.Gerar("senha-inexistente-para-equalizar-o-tempo"));

    public async Task<SessaoAberta> AutenticarAsync(Credenciais credenciais, CancellationToken cancelamento = default)
    {
        ArgumentNullException.ThrowIfNull(credenciais);

        var email = NormalizarSemRevelar(credenciais.Email);

        var usuario = email is null
            ? null
            : await dados.Usuarios.FirstOrDefaultAsync(u => u.Email == email, cancelamento);

        var resumo = usuario?.SenhaHash ?? _resumoDeReferencia.Value;
        var verificacao = hashDeSenha.Verificar(resumo, credenciais.Senha ?? string.Empty);

        // Mensagem única para e-mail inexistente e senha errada: dizer qual dos
        // dois falhou entregaria a lista de quem tem conta.
        RegraDeNegocioViolada.Se(
            usuario is null || verificacao is VerificacaoDeSenha.Invalida,
            "E-mail ou senha incorretos.");

        if (verificacao is VerificacaoDeSenha.ValidaComResumoDesatualizado)
        {
            // Aumenta o custo do resumo sem obrigar ninguém a trocar de senha.
            usuario!.DefinirSenha(hashDeSenha.Gerar(credenciais.Senha!));
        }

        return await AbrirSessaoAsync(usuario!, cancelamento);
    }

    /// <summary>
    /// RF06: renova enquanto a sessão estiver ativa. O token usado é revogado e
    /// outro é emitido — rotação, para que uma cópia vazada deixe de servir
    /// assim que o dono legítimo renovar.
    /// </summary>
    public async Task<SessaoAberta> RenovarAsync(string tokenDeAtualizacao, CancellationToken cancelamento = default)
    {
        var agora = relogio.GetUtcNow().UtcDateTime;
        var resumo = protetor.Resumir(ExigirToken(tokenDeAtualizacao));

        var token = await dados.TokensDeAtualizacao
            .FirstOrDefaultAsync(t => t.Resumo == resumo, cancelamento);

        RegraDeNegocioViolada.Se(
            token is null || !token.EstaAtivo(agora),
            "Sessão expirada. Entre novamente.");

        var usuario = await dados.Usuarios.FirstOrDefaultAsync(u => u.Id == token!.UsuarioId, cancelamento);
        RegraDeNegocioViolada.Se(usuario is null, "Sessão expirada. Entre novamente.");

        token!.Revogar(agora);

        // A sessão mantém o prazo original: renovar não estica o teto de oito
        // horas do RNF13, só emite credencial nova dentro dele.
        return await AbrirSessaoAsync(usuario!, cancelamento, token.ExpiraEm);
    }

    /// <summary>RF08: encerra a sessão revogando o token de renovação no servidor.</summary>
    /// <remarks>
    /// Silencioso de propósito: encerrar sessão com token já revogado, expirado
    /// ou inexistente devolve sucesso. Sair não pode falhar, e distinguir os
    /// casos revelaria se um token existe.
    /// </remarks>
    public async Task EncerrarAsync(string? tokenDeAtualizacao, CancellationToken cancelamento = default)
    {
        if (string.IsNullOrWhiteSpace(tokenDeAtualizacao)) return;

        var resumo = protetor.Resumir(tokenDeAtualizacao);
        var token = await dados.TokensDeAtualizacao.FirstOrDefaultAsync(t => t.Resumo == resumo, cancelamento);
        if (token is null) return;

        token.Revogar(relogio.GetUtcNow().UtcDateTime);
        await dados.SalvarAlteracoesAsync(cancelamento);
    }

    /// <summary>Revoga toda sessão ativa de um usuário.</summary>
    /// <remarks>
    /// Usado ao redefinir a senha (RF09): quem trocou a senha porque suspeita de
    /// acesso indevido espera que as sessões abertas caiam. Na etapa 4 serve
    /// também à revogação de vínculo (RF23).
    /// </remarks>
    public async Task RevogarSessoesAsync(int usuarioId, CancellationToken cancelamento = default)
    {
        var agora = relogio.GetUtcNow().UtcDateTime;

        var ativos = await dados.TokensDeAtualizacao
            .Where(t => t.UsuarioId == usuarioId && t.RevogadoEm == null)
            .ToListAsync(cancelamento);

        foreach (var token in ativos) token.Revogar(agora);
    }

    private async Task<SessaoAberta> AbrirSessaoAsync(
        Usuario usuario,
        CancellationToken cancelamento,
        DateTime? fimDaSessao = null)
    {
        var agora = relogio.GetUtcNow().UtcDateTime;

        // O vínculo institucional entra aqui na etapa 4, quando a entidade de
        // vínculo existir. Até lá nenhuma conta tem instituição resolvida.
        int? instituicaoId = null;

        var acesso = gerador.Emitir(usuario, instituicaoId);

        var (segredo, resumo) = protetor.Gerar();
        var validade = fimDaSessao is null
            ? opcoes.ValidadeDaSessao
            : fimDaSessao.Value - agora;

        var atualizacao = TokenDeAtualizacao.Emitir(usuario.Id, resumo, agora, validade);
        dados.TokensDeAtualizacao.Add(atualizacao);

        await dados.SalvarAlteracoesAsync(cancelamento);

        return new SessaoAberta(
            acesso.Token,
            acesso.ExpiraEm,
            segredo,
            ConversorDePerfil.De(usuario, instituicaoId));
    }

    private static string ExigirToken(string? token)
    {
        RegraDeNegocioViolada.Se(string.IsNullOrWhiteSpace(token), "Sessão expirada. Entre novamente.");
        return token!;
    }

    /// <summary>
    /// Normaliza sem lançar: e-mail malformado na tela de acesso é "incorretos",
    /// não um erro de validação diferente que distinguiria o caso.
    /// </summary>
    private static string? NormalizarSemRevelar(string? email)
    {
        try
        {
            return Usuario.NormalizarEmail(email);
        }
        catch (RegraDeNegocioViolada)
        {
            return null;
        }
    }
}
