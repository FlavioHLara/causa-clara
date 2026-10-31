using CausaClara.Aplicacao.Comum;

namespace CausaClara.Testes.Aplicacao;

/// <summary>Normalização dos parâmetros de listagem e cálculo da página (RNF09).</summary>
public sealed class PaginacaoTestes
{
    // ----- RNF09: o teto é do servidor, não do cliente -----

    [Theory]
    [InlineData(101)]
    [InlineData(1_000)]
    [InlineData(int.MaxValue)]
    public void RNF09_tamanho_acima_de_cem_e_reduzido_ao_teto(int pedido)
    {
        var parametros = new ParametrosListagem { Tamanho = pedido };

        Assert.Equal(ParametrosListagem.TamanhoMaximo, parametros.TamanhoNormalizado);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    public void Tamanho_invalido_cai_no_padrao(int pedido)
    {
        var parametros = new ParametrosListagem { Tamanho = pedido };

        Assert.Equal(ParametrosListagem.TamanhoPadrao, parametros.TamanhoNormalizado);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Pagina_menor_que_um_vira_a_primeira(int pedida)
    {
        var parametros = new ParametrosListagem { Pagina = pedida };

        Assert.Equal(1, parametros.PaginaNormalizada);
    }

    [Theory]
    [InlineData(1, 20, 0)]
    [InlineData(2, 20, 20)]
    [InlineData(3, 50, 100)]
    public void O_salto_acompanha_pagina_e_tamanho(int pagina, int tamanho, int saltoEsperado)
    {
        var parametros = new ParametrosListagem { Pagina = pagina, Tamanho = tamanho };

        Assert.Equal(saltoEsperado, parametros.Salto);
    }

    [Fact]
    public void O_salto_usa_o_tamanho_normalizado_e_nao_o_pedido()
    {
        // Sem isto, pedir tamanho=1000 na página 2 saltaria 1000 registros
        // enquanto devolveria 100 — e a página 2 pularia 900 registros em silêncio.
        var parametros = new ParametrosListagem { Pagina = 2, Tamanho = 1_000 };

        Assert.Equal(ParametrosListagem.TamanhoMaximo, parametros.Salto);
    }

    // ----- a resposta e o que a interface precisa dela -----

    [Theory]
    [InlineData(0, 20, 0)]
    [InlineData(1, 20, 1)]
    [InlineData(20, 20, 1)]
    [InlineData(21, 20, 2)]
    [InlineData(37, 20, 2)]
    public void O_total_de_paginas_arredonda_para_cima(int total, int tamanho, int paginasEsperadas)
    {
        var pagina = new PaginaResposta<string>([], 1, tamanho, total);

        Assert.Equal(paginasEsperadas, pagina.TotalDePaginas);
    }

    [Fact]
    public void A_primeira_pagina_de_duas_tem_proxima_e_nao_tem_anterior()
    {
        var pagina = new PaginaResposta<string>(["a"], 1, 20, 37);

        Assert.True(pagina.TemProximaPagina);
        Assert.False(pagina.TemPaginaAnterior);
    }

    [Fact]
    public void A_ultima_pagina_tem_anterior_e_nao_tem_proxima()
    {
        var pagina = new PaginaResposta<string>(["a"], 2, 20, 37);

        Assert.False(pagina.TemProximaPagina);
        Assert.True(pagina.TemPaginaAnterior);
    }

    [Fact]
    public void Pagina_vazia_nao_oferece_navegacao()
    {
        var pagina = PaginaResposta<string>.Vazia(new ParametrosListagem());

        Assert.Empty(pagina.Itens);
        Assert.Equal(0, pagina.TotalDeRegistros);
        Assert.Equal(0, pagina.TotalDePaginas);
        Assert.False(pagina.TemProximaPagina);
        Assert.False(pagina.TemPaginaAnterior);
    }
}
