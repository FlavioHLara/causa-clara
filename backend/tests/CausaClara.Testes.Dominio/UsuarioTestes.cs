using System.Reflection;
using CausaClara.Dominio.Comum;
using CausaClara.Dominio.Usuarios;

namespace CausaClara.Testes.Dominio;

/// <summary>Invariantes de <see cref="Usuario"/>: RN01, RF03 e RF04.</summary>
public sealed class UsuarioTestes
{
    private static Usuario UsuarioValido(
        string email = "flavio@exemplo.com",
        TipoUsuario tipo = TipoUsuario.Apoiador)
        => Usuario.Criar("Flávio Henrique Lara Santos", email, "$2a$11$resumo", "(31) 99999-0000", tipo);

    // ----- RN01: o tipo de conta é imutável -----

    [Fact]
    public void RN01_a_propriedade_Tipo_nao_tem_atribuidor_publico()
    {
        var tipo = typeof(Usuario).GetProperty(nameof(Usuario.Tipo));

        Assert.NotNull(tipo);
        Assert.False(
            tipo!.SetMethod?.IsPublic ?? false,
            "RN01: expor atribuidor público em Tipo permitiria trocar o tipo de conta depois do cadastro.");
    }

    [Fact]
    public void RN01_nenhum_metodo_publico_recebe_TipoUsuario_exceto_a_fabrica()
    {
        var suspeitos = typeof(Usuario)
            .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
            .Where(m => m.Name != nameof(Usuario.Criar))
            .Where(m => m.GetParameters().Any(p => p.ParameterType == typeof(TipoUsuario)))
            .Select(m => m.Name)
            .ToList();

        Assert.True(
            suspeitos.Count == 0,
            $"RN01: só a fábrica Criar pode receber TipoUsuario. Encontrado em: {string.Join(", ", suspeitos)}.");
    }

    [Fact]
    public void RN01_alterar_dados_cadastrais_preserva_o_tipo_de_conta()
    {
        var usuario = UsuarioValido(tipo: TipoUsuario.Gestor);

        usuario.AlterarDadosCadastrais("Outro Nome", "(31) 98888-1111");

        Assert.Equal(TipoUsuario.Gestor, usuario.Tipo);
    }

    [Fact]
    public void RN01_tipo_de_conta_fora_do_enum_e_recusado()
    {
        var excecao = Assert.Throws<RegraDeNegocioViolada>(
            () => Usuario.Criar("Nome", "a@b.com", "resumo", null, (TipoUsuario)99));

        Assert.Contains("tipo de conta", excecao.Message, StringComparison.OrdinalIgnoreCase);
    }

    // ----- RF04: um e-mail, um usuário -----

    [Theory]
    [InlineData("Flavio@Exemplo.COM", "flavio@exemplo.com")]
    [InlineData("  flavio@exemplo.com  ", "flavio@exemplo.com")]
    public void RF04_o_email_e_normalizado_para_que_a_unicidade_valha(string informado, string esperado)
    {
        var usuario = UsuarioValido(informado);

        Assert.Equal(esperado, usuario.Email);
    }

    [Theory]
    [InlineData("sem-arroba")]
    [InlineData("@semparte.local")]
    [InlineData("sem-dominio@")]
    [InlineData("dois@@arrobas.com")]
    [InlineData("com espaco@exemplo.com")]
    public void Email_sem_parte_local_ou_dominio_e_recusado(string invalido)
        => Assert.Throws<RegraDeNegocioViolada>(() => UsuarioValido(invalido));

    // ----- RF03: nome obrigatório, telefone opcional -----

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void RF03_o_nome_completo_e_obrigatorio(string? nome)
    {
        var excecao = Assert.Throws<RegraDeNegocioViolada>(
            () => Usuario.Criar(nome!, "a@b.com", "resumo", null, TipoUsuario.Apoiador));

        Assert.Contains("nome completo", excecao.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void RF03_telefone_ausente_vira_nulo_e_nao_string_vazia(string? telefone)
    {
        var usuario = Usuario.Criar("Nome", "a@b.com", "resumo", telefone, TipoUsuario.Apoiador);

        Assert.Null(usuario.Telefone);
    }

    [Fact]
    public void RF07_alterar_dados_cadastrais_muda_nome_e_telefone_e_preserva_o_email()
    {
        var usuario = UsuarioValido();
        var emailOriginal = usuario.Email;

        usuario.AlterarDadosCadastrais("  Nome Atualizado  ", "  (31) 97777-2222  ");

        Assert.Equal("Nome Atualizado", usuario.NomeCompleto);
        Assert.Equal("(31) 97777-2222", usuario.Telefone);
        Assert.Equal(emailOriginal, usuario.Email);
    }

    [Fact]
    public void Texto_acima_do_tamanho_maximo_e_recusado()
    {
        var nomeLongo = new string('a', Usuario.TamanhoMaximoNome + 1);

        var excecao = Assert.Throws<RegraDeNegocioViolada>(
            () => Usuario.Criar(nomeLongo, "a@b.com", "resumo", null, TipoUsuario.Apoiador));

        Assert.Contains(Usuario.TamanhoMaximoNome.ToString(), excecao.Message, StringComparison.Ordinal);
    }

    // ----- RNF28: tempo em UTC -----

    [Fact]
    public void RNF28_as_datas_nascem_em_UTC()
    {
        var usuario = UsuarioValido();

        Assert.Equal(DateTimeKind.Utc, usuario.CriadoEm.Kind);
        Assert.Equal(DateTimeKind.Utc, usuario.AtualizadoEm.Kind);
    }

    [Fact]
    public void RNF10_o_dominio_guarda_o_resumo_da_senha_e_nunca_a_senha_em_claro()
    {
        var propriedades = typeof(Usuario).GetProperties().Select(p => p.Name).ToList();

        Assert.Contains(nameof(Usuario.SenhaHash), propriedades);
        Assert.DoesNotContain(propriedades, p => p.Equals("Senha", StringComparison.OrdinalIgnoreCase));
    }
}
