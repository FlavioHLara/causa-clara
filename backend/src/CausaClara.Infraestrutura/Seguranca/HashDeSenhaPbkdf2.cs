using CausaClara.Aplicacao.Comum;
using CausaClara.Dominio.Usuarios;
using Microsoft.AspNetCore.Identity;

namespace CausaClara.Infraestrutura.Seguranca;

/// <summary>
/// Resumo de senha com PBKDF2-HMAC-SHA512, sal individual de 128 bits e
/// 100.000 iterações (RNF10).
/// </summary>
/// <remarks>
/// <para>
/// Usa o <see cref="PasswordHasher{TUser}"/> do ASP.NET Core em vez de uma
/// implementação própria ou de um pacote de terceiros. Três razões:
/// </para>
/// <list type="number">
/// <item><description>
/// O formato é versionado. O resumo guarda qual versão o gerou, e a conferência
/// avisa quando foi produzido por parâmetros antigos — o que permite aumentar o
/// custo do algoritmo com o tempo <b>sem</b> obrigar ninguém a trocar de senha.
/// </description></item>
/// <item><description>
/// Não tem o truncamento em 72 bytes que o bcrypt impõe, no qual senha longa
/// vira senha curta em silêncio.
/// </description></item>
/// <item><description>
/// É mantido pela Microsoft e vem num pacote pequeno, sem arrastar o Identity
/// inteiro.
/// </description></item>
/// </list>
/// </remarks>
internal sealed class HashDeSenhaPbkdf2 : IHashDeSenha
{
    private readonly PasswordHasher<Usuario> _hasher = new();

    public string Gerar(string senha)
    {
        ArgumentException.ThrowIfNullOrEmpty(senha);

        // O primeiro argumento participaria do resumo em implementações que
        // misturam dados do usuário; esta não mistura, e passar null deixa
        // explícito que o resumo depende só da senha e do sal gerado.
        return _hasher.HashPassword(null!, senha);
    }

    public VerificacaoDeSenha Verificar(string resumoArmazenado, string senhaInformada)
    {
        if (string.IsNullOrEmpty(resumoArmazenado) || string.IsNullOrEmpty(senhaInformada))
        {
            return VerificacaoDeSenha.Invalida;
        }

        // Resumo corrompido ou em formato desconhecido não derruba o acesso de
        // todo mundo: vira senha inválida, que é o efeito correto.
        PasswordVerificationResult resultado;
        try
        {
            resultado = _hasher.VerifyHashedPassword(null!, resumoArmazenado, senhaInformada);
        }
        catch (FormatException)
        {
            return VerificacaoDeSenha.Invalida;
        }

        return resultado switch
        {
            PasswordVerificationResult.Success => VerificacaoDeSenha.Valida,
            PasswordVerificationResult.SuccessRehashNeeded => VerificacaoDeSenha.ValidaComResumoDesatualizado,
            _ => VerificacaoDeSenha.Invalida
        };
    }
}
