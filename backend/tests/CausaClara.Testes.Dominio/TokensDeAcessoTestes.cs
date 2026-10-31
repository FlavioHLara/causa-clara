using CausaClara.Dominio.Acesso;
using CausaClara.Dominio.Comum;

namespace CausaClara.Testes.Dominio;

/// <summary>Validade e consumo das credenciais de sessão e de redefinição (RF06, RF08, RF09).</summary>
public sealed class TokensDeAcessoTestes
{
    private static readonly DateTime Agora = new(2026, 10, 29, 12, 0, 0, DateTimeKind.Utc);
    private const string Resumo = "0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef";

    // ----- token de renovação de sessão -----

    [Fact]
    public void Um_token_recem_emitido_esta_ativo()
    {
        var token = TokenDeAtualizacao.Emitir(1, Resumo, Agora, TimeSpan.FromHours(8));

        Assert.True(token.EstaAtivo(Agora));
        Assert.True(token.EstaAtivo(Agora.AddHours(7)));
    }

    [Fact]
    public void RNF13_depois_do_prazo_o_token_deixa_de_servir()
    {
        var token = TokenDeAtualizacao.Emitir(1, Resumo, Agora, TimeSpan.FromHours(8));

        Assert.False(token.EstaAtivo(Agora.AddHours(8)));
        Assert.False(token.EstaAtivo(Agora.AddHours(9)));
    }

    [Fact]
    public void RF08_revogar_encerra_a_sessao_imediatamente()
    {
        var token = TokenDeAtualizacao.Emitir(1, Resumo, Agora, TimeSpan.FromHours(8));

        token.Revogar(Agora.AddMinutes(10));

        Assert.False(token.EstaAtivo(Agora.AddMinutes(11)));
    }

    [Fact]
    public void Revogar_duas_vezes_preserva_o_instante_da_primeira()
    {
        // Sair não pode falhar, e a primeira revogação é a que conta: é ela que
        // marca quando a sessão deixou de valer.
        var token = TokenDeAtualizacao.Emitir(1, Resumo, Agora, TimeSpan.FromHours(8));
        var primeira = Agora.AddMinutes(5);

        token.Revogar(primeira);
        token.Revogar(Agora.AddMinutes(30));

        Assert.Equal(primeira, token.RevogadoEm);
    }

    [Fact]
    public void O_token_guarda_apenas_o_resumo_do_segredo()
    {
        // Se houvesse propriedade com o segredo, uma cópia do banco permitiria
        // renovar a sessão de qualquer pessoa.
        var propriedades = typeof(TokenDeAtualizacao).GetProperties().Select(p => p.Name).ToList();

        Assert.Contains(nameof(TokenDeAtualizacao.Resumo), propriedades);
        Assert.DoesNotContain(propriedades, p => p.Contains("Segredo", StringComparison.OrdinalIgnoreCase));
    }

    // ----- token de redefinição de senha -----

    [Fact]
    public void RF09_o_link_de_redefinicao_vale_ate_expirar()
    {
        var token = TokenDeRedefinicaoDeSenha.Emitir(1, Resumo, Agora, TimeSpan.FromMinutes(30));

        Assert.True(token.EstaValido(Agora.AddMinutes(29)));
        Assert.False(token.EstaValido(Agora.AddMinutes(30)));
    }

    [Fact]
    public void RF09_o_link_de_redefinicao_e_de_uso_unico()
    {
        // Sem isto, quem tivesse acesso à caixa de entrada redefiniria a senha
        // quantas vezes quisesse, inclusive depois de a pessoa recuperar a conta.
        var token = TokenDeRedefinicaoDeSenha.Emitir(1, Resumo, Agora, TimeSpan.FromMinutes(30));

        token.MarcarComoUsado(Agora.AddMinutes(1));

        Assert.False(token.EstaValido(Agora.AddMinutes(2)));
        Assert.Throws<RegraDeNegocioViolada>(() => token.MarcarComoUsado(Agora.AddMinutes(3)));
    }

    [Fact]
    public void Validade_nao_positiva_e_recusada()
    {
        Assert.Throws<RegraDeNegocioViolada>(
            () => TokenDeAtualizacao.Emitir(1, Resumo, Agora, TimeSpan.Zero));

        Assert.Throws<RegraDeNegocioViolada>(
            () => TokenDeRedefinicaoDeSenha.Emitir(1, Resumo, Agora, TimeSpan.FromMinutes(-5)));
    }

    [Fact]
    public void Token_sem_usuario_e_recusado()
        => Assert.Throws<RegraDeNegocioViolada>(
            () => TokenDeAtualizacao.Emitir(0, Resumo, Agora, TimeSpan.FromHours(1)));
}
