using CausaClara.Dominio.Auditoria;
using CausaClara.Testes.Integracao.Apoio;
using Microsoft.EntityFrameworkCore;

namespace CausaClara.Testes.Integracao;

/// <summary>
/// RF81 e RN10: toda criação, alteração e exclusão entra na trilha, e exclusão
/// de lançamento é lógica.
/// </summary>
public sealed class AuditoriaEExclusaoLogicaTestes
{
    [Fact]
    public async Task RF81_criar_registra_a_operacao_com_autor_e_instituicao()
    {
        using var banco = new BancoDeTeste();
        banco.Requisicao.UsuarioId = 77;
        banco.Requisicao.InstituicaoId = 1;

        await using (var contexto = banco.Abrir())
        {
            contexto.Lancamentos.Add(new LancamentoDeTeste { InstituicaoId = 1, Descricao = "Doação" });
            await contexto.SaveChangesAsync();
        }

        await using var leitura = banco.Abrir();
        var entrada = Assert.Single(await leitura.Auditoria.ToListAsync());

        Assert.Equal(OperacaoAuditada.Criacao, entrada.Operacao);
        Assert.Equal(nameof(LancamentoDeTeste), entrada.Entidade);
        Assert.Equal(77, entrada.UsuarioId);
        Assert.Equal(1, entrada.InstituicaoId);
        Assert.NotEqual(default, entrada.OcorridoEm);

        // O tipo do instante (UTC) é verificado em CausaClara.Testes.Dominio, no
        // objeto recém-criado. Aqui não dá: o provedor SQLite guarda data como
        // texto sem fuso e devolve Unspecified, o que mediria o provedor de teste
        // em vez do código. No PostgreSQL a coluna é timestamptz (RNF28).
    }

    [Fact]
    public async Task RF81_alterar_registra_como_alteracao()
    {
        using var banco = new BancoDeTeste();
        banco.Requisicao.UsuarioId = 77;
        banco.Requisicao.InstituicaoId = 1;

        int id;
        await using (var criacao = banco.Abrir())
        {
            var lancamento = new LancamentoDeTeste { InstituicaoId = 1, Descricao = "Antes" };
            criacao.Lancamentos.Add(lancamento);
            await criacao.SaveChangesAsync();
            id = lancamento.Id;
        }

        await using (var alteracao = banco.Abrir())
        {
            var lancamento = await alteracao.Lancamentos.SingleAsync(l => l.Id == id);
            lancamento.Descricao = "Depois";
            await alteracao.SaveChangesAsync();
        }

        await using var leitura = banco.Abrir();
        var operacoes = await leitura.Auditoria.OrderBy(a => a.Id).Select(a => a.Operacao).ToListAsync();

        Assert.Equal([OperacaoAuditada.Criacao, OperacaoAuditada.Alteracao], operacoes);
    }

    [Fact]
    public async Task RN10_excluir_registra_como_exclusao_e_nao_como_alteracao()
    {
        // O interceptador converte Deleted em Modified para fazer a exclusão
        // lógica. Se ele lesse o estado DEPOIS de converter, toda exclusão
        // apareceria na trilha como alteração, e o demonstrativo de prestação de
        // contas mentiria sobre o que aconteceu.
        using var banco = new BancoDeTeste();
        banco.Requisicao.UsuarioId = 77;
        banco.Requisicao.InstituicaoId = 1;

        int id;
        await using (var criacao = banco.Abrir())
        {
            var lancamento = new LancamentoDeTeste { InstituicaoId = 1, Descricao = "Some depois" };
            criacao.Lancamentos.Add(lancamento);
            await criacao.SaveChangesAsync();
            id = lancamento.Id;
        }

        await using (var exclusao = banco.Abrir())
        {
            var lancamento = await exclusao.Lancamentos.SingleAsync(l => l.Id == id);
            exclusao.Lancamentos.Remove(lancamento);
            await exclusao.SaveChangesAsync();
        }

        await using var leitura = banco.Abrir();
        var ultima = await leitura.Auditoria.OrderByDescending(a => a.Id).FirstAsync();

        Assert.Equal(OperacaoAuditada.Exclusao, ultima.Operacao);
    }

    [Fact]
    public async Task RN10_o_registro_excluido_some_da_listagem_e_permanece_no_banco()
    {
        using var banco = new BancoDeTeste();
        banco.Requisicao.UsuarioId = 77;
        banco.Requisicao.InstituicaoId = 1;

        int id;
        await using (var criacao = banco.Abrir())
        {
            var lancamento = new LancamentoDeTeste { InstituicaoId = 1, Descricao = "Some da lista" };
            criacao.Lancamentos.Add(lancamento);
            await criacao.SaveChangesAsync();
            id = lancamento.Id;
        }

        await using (var exclusao = banco.Abrir())
        {
            exclusao.Lancamentos.Remove(await exclusao.Lancamentos.SingleAsync(l => l.Id == id));
            await exclusao.SaveChangesAsync();
        }

        await using var leitura = banco.Abrir();

        Assert.Empty(await leitura.Lancamentos.ToListAsync());

        var preservado = await leitura.Lancamentos.IgnoreQueryFilters().SingleAsync(l => l.Id == id);
        Assert.True(preservado.Excluido);
        Assert.NotNull(preservado.ExcluidoEm);
    }

    [Fact]
    public async Task Os_dois_filtros_valem_ao_mesmo_tempo_na_mesma_entidade()
    {
        // A armadilha que esta convenção evita: chamar HasQueryFilter duas vezes
        // SUBSTITUI o filtro anterior em vez de somar. Se isso acontecesse, o
        // isolamento por instituição seria perdido silenciosamente — e o sistema
        // continuaria parecendo funcionar.
        using var banco = new BancoDeTeste();
        banco.Requisicao.UsuarioId = 77;
        banco.Requisicao.InstituicaoId = 1;

        await using (var preparo = banco.Abrir())
        {
            var daMinhaExcluido = new LancamentoDeTeste { InstituicaoId = 1, Descricao = "Minha, excluída" };
            var daMinhaAtivo = new LancamentoDeTeste { InstituicaoId = 1, Descricao = "Minha, ativa" };
            var deOutra = new LancamentoDeTeste { InstituicaoId = 2, Descricao = "De outra instituição" };

            preparo.Lancamentos.AddRange(daMinhaExcluido, daMinhaAtivo, deOutra);
            await preparo.SaveChangesAsync();

            preparo.Lancamentos.Remove(daMinhaExcluido);
            await preparo.SaveChangesAsync();
        }

        await using var leitura = banco.Abrir();
        var visiveis = await leitura.Lancamentos.ToListAsync();

        // Sobra exatamente uma: a da minha instituição que não foi excluída.
        var unico = Assert.Single(visiveis);
        Assert.Equal("Minha, ativa", unico.Descricao);
    }

    [Fact]
    public async Task A_propria_trilha_nao_e_auditada()
    {
        // Auditar a auditoria entraria em recursão e dobraria a tabela a cada
        // gravação. RegistroDeAuditoria não implementa IAuditavel, e este teste
        // é o que impede alguém de "corrigir" isso por engano.
        using var banco = new BancoDeTeste();
        banco.Requisicao.UsuarioId = 77;
        banco.Requisicao.InstituicaoId = 1;

        await using (var contexto = banco.Abrir())
        {
            contexto.Lancamentos.Add(new LancamentoDeTeste { InstituicaoId = 1, Descricao = "Única" });
            await contexto.SaveChangesAsync();
        }

        await using var leitura = banco.Abrir();

        Assert.Single(await leitura.Auditoria.ToListAsync());
    }
}
