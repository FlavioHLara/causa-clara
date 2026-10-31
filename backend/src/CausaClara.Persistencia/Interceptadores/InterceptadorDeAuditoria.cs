using CausaClara.Aplicacao.Comum;
using CausaClara.Dominio.Auditoria;
using CausaClara.Dominio.Comum;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace CausaClara.Persistencia.Interceptadores;

/// <summary>
/// Grava a trilha de auditoria (RF81) e converte exclusão em exclusão lógica (RN10),
/// inspecionando o rastreador de mudanças antes de cada gravação.
/// </summary>
/// <remarks>
/// <para>
/// Centralizar aqui é o que garante cobertura. Se cada caso de uso tivesse de
/// lembrar de auditar, bastaria um esquecimento para abrir um buraco na
/// prestação de contas — e um buraco em trilha de auditoria só aparece quando
/// alguém vai procurar o registro que deveria existir.
/// </para>
/// <para>
/// A ordem importa: o estado original é lido <em>antes</em> de converter a
/// exclusão, senão toda exclusão lógica seria registrada como alteração.
/// </para>
/// </remarks>
public sealed class InterceptadorDeAuditoria(IContextoDaRequisicao contexto) : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData dados,
        InterceptionResult<int> resultado)
    {
        Processar(dados.Context);
        return base.SavingChanges(dados, resultado);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData dados,
        InterceptionResult<int> resultado,
        CancellationToken cancelamento = default)
    {
        Processar(dados.Context);
        return base.SavingChangesAsync(dados, resultado, cancelamento);
    }

    private void Processar(DbContext? contextoDeDados)
    {
        if (contextoDeDados is null) return;

        contextoDeDados.ChangeTracker.DetectChanges();

        // A lista é materializada antes de qualquer mudança: acrescentar entradas
        // de auditoria durante a iteração modificaria a coleção sendo percorrida.
        var entradas = contextoDeDados.ChangeTracker
            .Entries()
            .Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .ToList();

        var registros = new List<RegistroDeAuditoria>();

        foreach (var entrada in entradas)
        {
            var estadoOriginal = entrada.State;

            // RN10: exclusão física vira exclusão lógica. O caso de uso pede
            // Remove ao contexto e não precisa saber desta conversão.
            if (estadoOriginal is EntityState.Deleted && entrada.Entity is IExcluivelLogicamente excluivel)
            {
                excluivel.MarcarComoExcluido();
                entrada.State = EntityState.Modified;
            }

            if (entrada.Entity is not IAuditavel) continue;

            registros.Add(RegistroDeAuditoria.Registrar(
                operacao: TraduzirOperacao(estadoOriginal),
                entidade: entrada.Metadata.ClrType.Name,
                chaveDoRegistro: ChaveDe(entrada),
                descricao: null,
                usuarioId: contexto.UsuarioId,
                instituicaoId: InstituicaoDe(entrada)));
        }

        if (registros.Count > 0) contextoDeDados.Set<RegistroDeAuditoria>().AddRange(registros);
    }

    private static OperacaoAuditada TraduzirOperacao(EntityState estado) => estado switch
    {
        EntityState.Added => OperacaoAuditada.Criacao,
        EntityState.Modified => OperacaoAuditada.Alteracao,
        EntityState.Deleted => OperacaoAuditada.Exclusao,
        _ => throw new InvalidOperationException($"Estado não auditável: {estado}.")
    };

    /// <summary>
    /// Chave primária em texto. Numa inserção o valor ainda não foi gerado pelo
    /// banco, então sai vazio e vira um marcador — a identificação do registro
    /// recém-criado fica por conta da descrição, preenchida pelo caso de uso
    /// quando ele tiver contexto para isso.
    /// </summary>
    private static string ChaveDe(EntityEntry entrada)
    {
        var chave = entrada.Properties
            .Where(p => p.Metadata.IsPrimaryKey())
            .Select(p => p.CurrentValue?.ToString())
            .Where(valor => !string.IsNullOrEmpty(valor) && valor != "0");

        var composta = string.Join("|", chave);
        return string.IsNullOrEmpty(composta) ? "(gerada na gravação)" : composta;
    }

    /// <summary>
    /// A instituição do próprio registro quando ele pertence a uma; caso
    /// contrário, a do usuário da requisição.
    /// </summary>
    private int? InstituicaoDe(EntityEntry entrada)
        => entrada.Entity is IPertenceAInstituicao pertencente
            ? pertencente.InstituicaoId
            : contexto.InstituicaoId;
}
