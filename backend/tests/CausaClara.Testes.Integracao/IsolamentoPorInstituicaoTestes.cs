using CausaClara.Testes.Integracao.Apoio;
using Microsoft.EntityFrameworkCore;

namespace CausaClara.Testes.Integracao;

/// <summary>
/// RNF12 e RN05: um gestor da instituição A jamais alcança dado da instituição B,
/// nem trocando o identificador na rota.
/// </summary>
/// <remarks>
/// É o teste mais valioso do projeto, porque cobre o requisito de maior risco.
/// Ele exercita a convenção de filtros globais contra um banco de verdade, com
/// SQL de verdade — não verifica metadados do modelo, que passariam mesmo se o
/// filtro estivesse mal montado.
/// </remarks>
public sealed class IsolamentoPorInstituicaoTestes
{
    [Fact]
    public async Task O_gestor_so_enxerga_lancamentos_da_propria_instituicao()
    {
        using var banco = new BancoDeTeste();
        await banco.SemearDuasInstituicoesAsync();

        banco.Requisicao.UsuarioId = 10;
        banco.Requisicao.InstituicaoId = 1;

        await using var contexto = banco.Abrir();
        var visiveis = await contexto.Lancamentos.ToListAsync();

        Assert.Single(visiveis);
        Assert.Equal(1, visiveis[0].InstituicaoId);
    }

    [Fact]
    public async Task Identificador_forjado_devolve_nao_encontrado_e_nao_dado_alheio()
    {
        using var banco = new BancoDeTeste();
        var (_, idDaInstituicaoDois) = await banco.SemearDuasInstituicoesAsync();

        banco.Requisicao.UsuarioId = 10;
        banco.Requisicao.InstituicaoId = 1;

        await using var contexto = banco.Abrir();

        // O gestor da instituição 1 pede, pelo identificador, um lançamento da
        // instituição 2 — exatamente a manipulação que o RNF12 cita.
        var alheio = await contexto.Lancamentos
            .FirstOrDefaultAsync(l => l.Id == idDaInstituicaoDois);

        Assert.Null(alheio);
    }

    [Fact]
    public async Task O_cenario_tem_dados_das_duas_instituicoes()
    {
        // Guarda contra falso positivo: sem este teste, os anteriores passariam
        // igualmente se a tabela estivesse vazia por qualquer motivo.
        using var banco = new BancoDeTeste();
        await banco.SemearDuasInstituicoesAsync();

        await using var contexto = banco.Abrir();
        var todos = await contexto.Lancamentos.IgnoreQueryFilters().ToListAsync();

        Assert.Equal(2, todos.Count);
        Assert.Contains(todos, l => l.InstituicaoId == 1);
        Assert.Contains(todos, l => l.InstituicaoId == 2);
    }

    [Fact]
    public async Task Sem_usuario_autenticado_nenhum_lancamento_e_visivel()
    {
        using var banco = new BancoDeTeste();
        await banco.SemearDuasInstituicoesAsync();

        banco.Requisicao.UsuarioId = null;
        banco.Requisicao.InstituicaoId = null;

        await using var contexto = banco.Abrir();
        var visiveis = await contexto.Lancamentos.ToListAsync();

        // A falha fecha: instituição nula não casa com nenhuma linha. O caminho
        // público de transparência não passa por aqui — ele usa serviço próprio,
        // com IgnoreQueryFilters e filtro explícito pela rota.
        Assert.Empty(visiveis);
    }

    [Fact]
    public async Task O_filtro_le_a_instituicao_no_momento_da_consulta()
    {
        // A ressalva da seção 4 do projeto arquitetural: se a expressão capturasse
        // o VALOR em vez do serviço, a primeira instituição que passasse pelo
        // modelo ficaria congelada para todas as requisições seguintes — e o
        // vazamento seria invisível até alguém com outra instituição reclamar.
        using var banco = new BancoDeTeste();
        await banco.SemearDuasInstituicoesAsync();

        await using var contexto = banco.Abrir();

        banco.Requisicao.InstituicaoId = 1;
        var daPrimeira = await contexto.Lancamentos.AsNoTracking().ToListAsync();

        banco.Requisicao.InstituicaoId = 2;
        var daSegunda = await contexto.Lancamentos.AsNoTracking().ToListAsync();

        Assert.Equal(1, Assert.Single(daPrimeira).InstituicaoId);
        Assert.Equal(2, Assert.Single(daSegunda).InstituicaoId);
    }
}
