using CausaClara.Aplicacao.Comum;
using CausaClara.Dominio.Comum;
using Microsoft.EntityFrameworkCore;

namespace CausaClara.Aplicacao.Acesso;

/// <summary>Consulta e alteração dos dados cadastrais do próprio usuário (RF07).</summary>
public sealed class ServicoDePerfil(IContextoDeDados dados, IContextoDaRequisicao contexto)
{
    public async Task<PerfilDoUsuario> ObterAsync(CancellationToken cancelamento = default)
    {
        var usuario = await ExigirUsuarioAutenticadoAsync(cancelamento);
        return ConversorDePerfil.De(usuario, contexto.InstituicaoId);
    }

    /// <summary>
    /// RF07: altera nome e telefone. O tipo de conta não é parâmetro — por RN01
    /// ele não muda, e a entidade não expõe caminho para isso.
    /// </summary>
    public async Task<PerfilDoUsuario> AtualizarAsync(
        AtualizacaoDePerfil atualizacao,
        CancellationToken cancelamento = default)
    {
        ArgumentNullException.ThrowIfNull(atualizacao);

        var usuario = await ExigirUsuarioAutenticadoAsync(cancelamento);
        usuario.AlterarDadosCadastrais(atualizacao.NomeCompleto, atualizacao.Telefone);

        await dados.SalvarAlteracoesAsync(cancelamento);

        return ConversorDePerfil.De(usuario, contexto.InstituicaoId);
    }

    private async Task<Dominio.Usuarios.Usuario> ExigirUsuarioAutenticadoAsync(CancellationToken cancelamento)
    {
        var id = contexto.UsuarioId;
        RegraDeNegocioViolada.Se(id is null, "Sessão expirada. Entre novamente.");

        var usuario = await dados.Usuarios.FirstOrDefaultAsync(u => u.Id == id, cancelamento);

        // Token válido de usuário que não existe mais: trata como sessão
        // encerrada, não como erro do sistema.
        RegraDeNegocioViolada.Se(usuario is null, "Sessão expirada. Entre novamente.");

        return usuario!;
    }
}
