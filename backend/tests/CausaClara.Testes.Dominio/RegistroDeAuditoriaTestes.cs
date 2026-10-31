using CausaClara.Dominio.Auditoria;
using CausaClara.Dominio.Comum;

namespace CausaClara.Testes.Dominio;

/// <summary>Invariantes da trilha de auditoria (RF81, RN10).</summary>
public sealed class RegistroDeAuditoriaTestes
{
    private static RegistroDeAuditoria Entrada(
        OperacaoAuditada operacao = OperacaoAuditada.Criacao,
        string entidade = "Doacao",
        string chave = "42")
        => RegistroDeAuditoria.Registrar(operacao, entidade, chave, "NF 00124 · R$ 1.120,00", 7, 1);

    [Fact]
    public void RNF28_o_instante_da_operacao_nasce_em_UTC()
        => Assert.Equal(DateTimeKind.Utc, Entrada().OcorridoEm.Kind);

    [Fact]
    public void A_entrada_guarda_autor_instituicao_e_operacao()
    {
        var entrada = Entrada(OperacaoAuditada.Exclusao, "MovimentacaoEstoque", "9");

        Assert.Equal(OperacaoAuditada.Exclusao, entrada.Operacao);
        Assert.Equal("MovimentacaoEstoque", entrada.Entidade);
        Assert.Equal("9", entrada.ChaveDoRegistro);
        Assert.Equal(7, entrada.UsuarioId);
        Assert.Equal(1, entrada.InstituicaoId);
    }

    [Fact]
    public void Sem_usuario_e_sem_instituicao_a_entrada_continua_valida()
    {
        // Operações anteriores a qualquer vínculo — cadastro de usuário, por
        // exemplo — precisam poder ser registradas.
        var entrada = RegistroDeAuditoria.Registrar(OperacaoAuditada.Criacao, "Usuario", "1");

        Assert.Null(entrada.UsuarioId);
        Assert.Null(entrada.InstituicaoId);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Entidade_e_chave_sao_obrigatorias(string vazio)
    {
        Assert.Throws<RegraDeNegocioViolada>(
            () => RegistroDeAuditoria.Registrar(OperacaoAuditada.Criacao, vazio, "1"));

        Assert.Throws<RegraDeNegocioViolada>(
            () => RegistroDeAuditoria.Registrar(OperacaoAuditada.Criacao, "Doacao", vazio));
    }

    [Fact]
    public void RN10_a_trilha_nao_e_excluivel_nem_auditavel()
    {
        // Trilha que se apaga não é trilha; auditar a auditoria entraria em
        // recursão. Os dois defeitos seriam introduzidos por uma linha só —
        // acrescentar um marcador à declaração da classe.
        Assert.False(typeof(IExcluivelLogicamente).IsAssignableFrom(typeof(RegistroDeAuditoria)));
        Assert.False(typeof(IAuditavel).IsAssignableFrom(typeof(RegistroDeAuditoria)));
    }

    [Fact]
    public void A_entrada_nao_expoe_nenhum_atribuidor_publico()
    {
        var atribuidores = typeof(RegistroDeAuditoria)
            .GetProperties()
            .Where(p => p.SetMethod?.IsPublic ?? false)
            .Select(p => p.Name)
            .ToList();

        Assert.True(
            atribuidores.Count == 0,
            $"Entrada de auditoria é imutável. Atribuidores públicos: {string.Join(", ", atribuidores)}.");
    }
}
