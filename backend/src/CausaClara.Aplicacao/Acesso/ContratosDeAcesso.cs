using CausaClara.Dominio.Usuarios;

namespace CausaClara.Aplicacao.Acesso;

/// <summary>Dados do cadastro (RF02, RF03).</summary>
public sealed record NovaConta(
    string NomeCompleto,
    string Email,
    string? Telefone,
    string Senha,
    string ConfirmacaoDeSenha,
    TipoUsuario Tipo);

/// <summary>Credenciais de acesso (RF05).</summary>
public sealed record Credenciais(string Email, string Senha);

/// <summary>Como o usuário é devolvido ao cliente. Nunca inclui o resumo da senha.</summary>
public sealed record PerfilDoUsuario(
    int Id,
    string NomeCompleto,
    string Email,
    string? Telefone,
    TipoUsuario Tipo,
    int? InstituicaoId);

/// <summary>Sessão recém-aberta ou renovada (RF05, RF06).</summary>
/// <param name="TokenDeAcesso">Credencial assinada, de vida curta (RNF13).</param>
/// <param name="ExpiraEm">Quando o token de acesso deixa de valer, em UTC (RNF28).</param>
/// <param name="TokenDeAtualizacao">Segredo de uso único para renovar a sessão.</param>
public sealed record SessaoAberta(
    string TokenDeAcesso,
    DateTime ExpiraEm,
    string TokenDeAtualizacao,
    PerfilDoUsuario Perfil);

/// <summary>Alteração de dados cadastrais (RF07). Tipo de conta não entra, por RN01.</summary>
public sealed record AtualizacaoDePerfil(string NomeCompleto, string? Telefone);

/// <summary>Pedido de redefinição de senha (RF09).</summary>
public sealed record PedidoDeRedefinicao(string Email);

/// <summary>Troca efetiva da senha, com o segredo recebido por e-mail (RF09).</summary>
public sealed record NovaSenha(string Token, string Senha, string ConfirmacaoDeSenha);

/// <summary>Token de acesso emitido, com o instante em que deixa de valer.</summary>
public sealed record TokenDeAcessoEmitido(string Token, DateTime ExpiraEm);

/// <summary>
/// Emissão da credencial assinada de acesso (RNF13).
/// </summary>
/// <remarks>
/// A Aplicação pede um token para um usuário; o formato — JWT, algoritmo de
/// assinatura, chave — é detalhe da Infraestrutura.
/// </remarks>
public interface IGeradorDeTokenDeAcesso
{
    TokenDeAcessoEmitido Emitir(Usuario usuario, int? instituicaoId);
}
