using CausaClara.Aplicacao.Comum;
using CausaClara.Dominio.Comum;
using CausaClara.Dominio.Usuarios;
using Microsoft.EntityFrameworkCore;

namespace CausaClara.Aplicacao.Acesso;

/// <summary>Cadastro de usuário (RF02, RF03, RF04).</summary>
public sealed class ServicoDeCadastro(
    IContextoDeDados dados,
    IHashDeSenha hashDeSenha,
    OpcoesDeAcesso opcoes)
{
    public async Task<PerfilDoUsuario> CadastrarAsync(NovaConta nova, CancellationToken cancelamento = default)
    {
        ArgumentNullException.ThrowIfNull(nova);

        RegraDeNegocioViolada.Se(
            !string.Equals(nova.Senha, nova.ConfirmacaoDeSenha, StringComparison.Ordinal),
            "A senha e a confirmação não coincidem.");

        ConferirSenha(nova.Senha, opcoes.TamanhoMinimoDaSenha);

        var email = Usuario.NormalizarEmail(nova.Email);

        // RF04: a verificação prévia dá a mensagem boa; o índice único do banco
        // é a garantia de fato, para o caso de dois cadastros simultâneos.
        var jaExiste = await dados.Usuarios.AnyAsync(u => u.Email == email, cancelamento);
        RegraDeNegocioViolada.Se(jaExiste, "Já existe uma conta cadastrada com este e-mail.");

        var usuario = Usuario.Criar(
            nova.NomeCompleto,
            email,
            hashDeSenha.Gerar(nova.Senha),   // RNF10
            nova.Telefone,
            nova.Tipo);

        dados.Usuarios.Add(usuario);

        try
        {
            await dados.SalvarAlteracoesAsync(cancelamento);
        }
        catch (DbUpdateException excecao) when (EhViolacaoDeEmailDuplicado(excecao))
        {
            // Perdeu a corrida para outro cadastro com o mesmo e-mail.
            // A mensagem é a mesma da verificação prévia (RNF05).
            throw new RegraDeNegocioViolada("Já existe uma conta cadastrada com este e-mail.");
        }

        return ConversorDePerfil.De(usuario, instituicaoId: null);
    }

    /// <summary>Exigências de senha, aplicadas no servidor (RNF15).</summary>
    internal static void ConferirSenha(string? senha, int tamanhoMinimo)
    {
        RegraDeNegocioViolada.Se(
            string.IsNullOrWhiteSpace(senha),
            "Informe a senha.");

        RegraDeNegocioViolada.Se(
            senha!.Length < tamanhoMinimo,
            $"A senha deve ter pelo menos {tamanhoMinimo} caracteres.");
    }

    private static bool EhViolacaoDeEmailDuplicado(DbUpdateException excecao)
        => excecao.InnerException?.Message.Contains("ux_usuarios_email", StringComparison.OrdinalIgnoreCase) ?? false;
}

/// <summary>Conversão de entidade para o que o cliente enxerga.</summary>
/// <remarks>
/// Existe para que nenhum caminho devolva a entidade direto: expor
/// <see cref="Usuario"/> vazaria o resumo da senha e abriria mass assignment.
/// </remarks>
internal static class ConversorDePerfil
{
    public static PerfilDoUsuario De(Usuario usuario, int? instituicaoId)
        => new(
            usuario.Id,
            usuario.NomeCompleto,
            usuario.Email,
            usuario.Telefone,
            usuario.Tipo,
            instituicaoId);
}
