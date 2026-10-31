using CausaClara.Dominio.Comum;
using CausaClara.Dominio.Instituicoes;

namespace CausaClara.Testes.Dominio;

/// <summary>Invariantes de <see cref="Instituicao"/>: RF11 e RF25.</summary>
public sealed class InstituicaoTestes
{
    private static Instituicao InstituicaoValida(string estado = "MG")
        => Instituicao.Criar(
            nome: "Casa de Apoio Semear",
            descricao: "Acolhimento de pessoas idosas em situação de vulnerabilidade social.",
            cidade: "Betim",
            estado: estado,
            emailContato: "contato@casasemear.org.br",
            telefone: "(31) 3532-0000",
            site: "casasemear.org.br");

    [Theory]
    [InlineData("mg", "MG")]
    [InlineData(" sp ", "SP")]
    public void O_estado_e_normalizado_para_a_sigla_em_caixa_alta(string informado, string esperado)
        => Assert.Equal(esperado, InstituicaoValida(informado).Estado);

    [Theory]
    [InlineData("XX")]
    [InlineData("Minas Gerais")]
    [InlineData("M")]
    public void Unidade_da_federacao_inexistente_e_recusada(string invalido)
    {
        var excecao = Assert.Throws<RegraDeNegocioViolada>(() => InstituicaoValida(invalido));

        Assert.Contains("federação", excecao.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void RF11_todos_os_campos_enumerados_pelo_requisito_sao_obrigatorios()
    {
        // O requisito enumera os sete campos sem marcar nenhum como opcional.
        // Se essa decisão mudar, este teste é o lugar onde a mudança aparece.
        Assert.Throws<RegraDeNegocioViolada>(() => Instituicao.Criar(
            "", "descrição", "Betim", "MG", "a@b.com", "(31) 3532-0000", "site.org"));

        Assert.Throws<RegraDeNegocioViolada>(() => Instituicao.Criar(
            "Nome", "", "Betim", "MG", "a@b.com", "(31) 3532-0000", "site.org"));

        Assert.Throws<RegraDeNegocioViolada>(() => Instituicao.Criar(
            "Nome", "descrição", "", "MG", "a@b.com", "(31) 3532-0000", "site.org"));

        Assert.Throws<RegraDeNegocioViolada>(() => Instituicao.Criar(
            "Nome", "descrição", "Betim", "MG", "", "(31) 3532-0000", "site.org"));

        Assert.Throws<RegraDeNegocioViolada>(() => Instituicao.Criar(
            "Nome", "descrição", "Betim", "MG", "a@b.com", "", "site.org"));

        Assert.Throws<RegraDeNegocioViolada>(() => Instituicao.Criar(
            "Nome", "descrição", "Betim", "MG", "a@b.com", "(31) 3532-0000", ""));
    }

    [Fact]
    public void O_email_de_contato_segue_a_mesma_normalizacao_do_usuario()
    {
        var instituicao = Instituicao.Criar(
            "Instituto Raízes", "Reforço escolar.", "Contagem", "MG",
            "  Contato@Raizes.ORG.br ", "(31) 3333-0000", "raizes.org.br");

        Assert.Equal("contato@raizes.org.br", instituicao.EmailContato);
    }

    [Fact]
    public void RF25_atualizar_cadastro_troca_os_dados_e_avanca_a_data_de_atualizacao()
    {
        var instituicao = InstituicaoValida();
        var criadoEm = instituicao.CriadoEm;

        instituicao.AtualizarCadastro(
            "Casa de Apoio Semear", "Nova descrição.", "Contagem", "mg",
            "novo@casasemear.org.br", "(31) 3532-1111", "novo.casasemear.org.br");

        Assert.Equal("Contagem", instituicao.Cidade);
        Assert.Equal("MG", instituicao.Estado);
        Assert.Equal("novo@casasemear.org.br", instituicao.EmailContato);
        Assert.Equal(criadoEm, instituicao.CriadoEm);
        Assert.True(instituicao.AtualizadoEm >= criadoEm);
    }

    [Fact]
    public void RNF28_as_datas_nascem_em_UTC()
    {
        var instituicao = InstituicaoValida();

        Assert.Equal(DateTimeKind.Utc, instituicao.CriadoEm.Kind);
        Assert.Equal(DateTimeKind.Utc, instituicao.AtualizadoEm.Kind);
    }
}
