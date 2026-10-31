using CausaClara.Infraestrutura.Email;

namespace CausaClara.Testes.Infraestrutura;

/// <summary>
/// Verificação da configuração de e-mail, que roda na subida da API.
/// </summary>
/// <remarks>
/// Descobrir que o servidor de e-mail não estava configurado no momento em que
/// alguém pede redefinição de senha é tarde demais — o usuário fica esperando
/// uma mensagem que nunca sai. Por isso a validação é de inicialização.
/// </remarks>
public sealed class OpcoesDeEmailTestes
{
    private static OpcoesDeEmail Validas() => new()
    {
        Servidor = "smtp.exemplo.com",
        Porta = 587,
        RemetenteEndereco = "nao-responda@causaclara.org.br",
        Seguranca = SegurancaDoSmtp.StartTls
    };

    [Fact]
    public void Configuracao_completa_nao_aponta_problema()
        => Assert.Empty(Validas().Problemas());

    [Fact]
    public void Servidor_ausente_e_recusado()
    {
        var opcoes = Validas();
        opcoes.Servidor = "   ";

        Assert.Contains(opcoes.Problemas(), p => p.Contains("Servidor", StringComparison.Ordinal));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(70_000)]
    public void Porta_fora_da_faixa_e_recusada(int porta)
    {
        var opcoes = Validas();
        opcoes.Porta = porta;

        Assert.Contains(opcoes.Problemas(), p => p.Contains("Porta", StringComparison.Ordinal));
    }

    [Fact]
    public void Remetente_sem_arroba_e_recusado()
    {
        var opcoes = Validas();
        opcoes.RemetenteEndereco = "nao-responda";

        Assert.Contains(opcoes.Problemas(), p => p.Contains("RemetenteEndereco", StringComparison.Ordinal));
    }

    [Fact]
    public void Usuario_sem_senha_e_recusado()
    {
        // Metade das credenciais costuma ser pior que nenhuma: o servidor recusa
        // a autenticação e a mensagem some sem explicação clara.
        var opcoes = Validas();
        opcoes.Usuario = "api";
        opcoes.Senha = null;

        Assert.Contains(opcoes.Problemas(), p => p.Contains("Senha", StringComparison.Ordinal));
    }

    [Fact]
    public void Servidor_sem_autenticacao_e_valido()
    {
        // É o caso da captura de mensagens do ambiente local.
        var opcoes = Validas();
        opcoes.Servidor = "email";
        opcoes.Porta = 1025;
        opcoes.Seguranca = SegurancaDoSmtp.Nenhuma;

        Assert.Empty(opcoes.Problemas());
    }

    [Fact]
    public void As_mensagens_de_problema_nomeiam_a_variavel_de_ambiente()
    {
        // Quem está configurando o ambiente precisa saber qual variável mexer,
        // não apenas que "a configuração está inválida".
        var opcoes = new OpcoesDeEmail();

        Assert.All(opcoes.Problemas(), p => Assert.StartsWith("Email__", p, StringComparison.Ordinal));
    }
}
