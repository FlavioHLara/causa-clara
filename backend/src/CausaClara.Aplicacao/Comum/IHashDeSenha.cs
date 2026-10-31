namespace CausaClara.Aplicacao.Comum;

/// <summary>Resultado da conferência de uma senha.</summary>
public enum VerificacaoDeSenha
{
    /// <summary>Não confere.</summary>
    Invalida,

    /// <summary>Confere.</summary>
    Valida,

    /// <summary>
    /// Confere, mas o resumo foi gerado com parâmetros antigos e deve ser
    /// refeito. Permite aumentar o custo do algoritmo com o tempo sem forçar
    /// ninguém a trocar de senha: o resumo é atualizado no próximo acesso.
    /// </summary>
    ValidaComResumoDesatualizado
}

/// <summary>
/// Derivação e conferência do resumo da senha (RNF10).
/// </summary>
/// <remarks>
/// A Aplicação nunca guarda senha em claro nem escolhe algoritmo: ela pede o
/// resumo e confere. O algoritmo e seus parâmetros são detalhe da Infraestrutura
/// e podem mudar sem tocar em caso de uso.
/// </remarks>
public interface IHashDeSenha
{
    /// <summary>Deriva o resumo, com sal individual, conforme RNF10.</summary>
    string Gerar(string senha);

    VerificacaoDeSenha Verificar(string resumoArmazenado, string senhaInformada);
}

/// <summary>
/// Geração e conferência de segredos opacos — tokens de sessão e de
/// redefinição de senha.
/// </summary>
/// <remarks>
/// O segredo vai para o cliente; só o resumo é persistido. Resumo simples e
/// rápido, não adaptativo como o da senha: o segredo tem entropia de 256 bits
/// gerada por fonte criptográfica, então não há o que adivinhar por força bruta,
/// e o resumo precisa ser barato para ser calculado a cada renovação.
/// </remarks>
public interface IProtetorDeSegredos
{
    /// <summary>Gera um segredo novo e o resumo correspondente.</summary>
    (string Segredo, string Resumo) Gerar();

    /// <summary>Resumo de um segredo recebido, para procurar no banco.</summary>
    string Resumir(string segredo);
}
