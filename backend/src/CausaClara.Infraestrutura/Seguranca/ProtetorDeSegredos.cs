using System.Buffers.Text;
using System.Security.Cryptography;
using CausaClara.Aplicacao.Comum;

namespace CausaClara.Infraestrutura.Seguranca;

/// <summary>
/// Gera segredos opacos de 256 bits e os resume com SHA-256.
/// </summary>
/// <remarks>
/// <para>
/// O segredo vai para o cliente; só o resumo é persistido. Vazamento do banco
/// não permite renovar sessão nem redefinir senha de ninguém.
/// </para>
/// <para>
/// SHA-256 simples, e não PBKDF2 como na senha: o segredo tem 256 bits de
/// entropia vindos de <see cref="RandomNumberGenerator"/>, então não há espaço
/// de busca que torne força bruta viável, e o resumo precisa ser barato porque
/// é calculado a cada renovação. Função adaptativa existe para compensar a
/// baixa entropia de senhas escolhidas por pessoas — aqui não há essa fraqueza.
/// </para>
/// <para>
/// A codificação é base64url para o segredo, de modo que ele viaje em URL de
/// link de redefinição (RF09) sem precisar de escape.
/// </para>
/// </remarks>
internal sealed class ProtetorDeSegredos : IProtetorDeSegredos
{
    private const int BytesDeEntropia = 32;   // 256 bits

    public (string Segredo, string Resumo) Gerar()
    {
        var bytes = RandomNumberGenerator.GetBytes(BytesDeEntropia);
        var segredo = Base64Url.EncodeToString(bytes);
        return (segredo, Resumir(segredo));
    }

    public string Resumir(string segredo)
    {
        ArgumentException.ThrowIfNullOrEmpty(segredo);

        var bytes = System.Text.Encoding.UTF8.GetBytes(segredo);
        return Convert.ToHexStringLower(SHA256.HashData(bytes));
    }
}
