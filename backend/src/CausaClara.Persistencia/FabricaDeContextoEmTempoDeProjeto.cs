using CausaClara.Aplicacao.Comum;
using CausaClara.Dominio.Usuarios;
using CausaClara.Persistencia.Extensoes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CausaClara.Persistencia;

/// <summary>
/// Permite que <c>dotnet ef migrations</c> construa o contexto sem subir a Api.
/// </summary>
/// <remarks>
/// <para>
/// Sem esta fábrica, a ferramenta precisaria do projeto da Api como
/// <c>--startup-project</c> e da configuração completa só para gerar uma migração.
/// Com ela, o comando é <c>dotnet ef migrations add Nome -p src/CausaClara.Persistencia</c>.
/// </para>
/// <para>
/// A conexão usada aqui é exclusivamente de tempo de projeto: gerar e inspecionar
/// migrações não abre conexão com o banco. O valor vem da variável de ambiente
/// <c>CAUSACLARA_CONEXAO</c> quando presente, e cai num padrão local caso contrário.
/// Nenhum segredo entra no repositório (RNF18).
/// </para>
/// </remarks>
public sealed class FabricaDeContextoEmTempoDeProjeto : IDesignTimeDbContextFactory<ContextoDeDados>
{
    private const string ConexaoLocalPadrao =
        "Host=localhost;Port=5432;Database=causa_clara;Username=causa_clara;Password=desenvolvimento";

    public ContextoDeDados CreateDbContext(string[] args)
    {
        var conexao = Environment.GetEnvironmentVariable("CAUSACLARA_CONEXAO");
        if (string.IsNullOrWhiteSpace(conexao)) conexao = ConexaoLocalPadrao;

        var opcoes = new DbContextOptionsBuilder<ContextoDeDados>();
        ConfiguracaoDePersistencia.ConfigurarContexto(opcoes, conexao);

        return new ContextoDeDados(opcoes.Options, new SemRequisicao());
    }

    /// <summary>
    /// Contexto de requisição vazio, para tempo de projeto.
    /// </summary>
    /// <remarks>
    /// Gerar migração não tem usuário nem instituição. Os filtros globais são
    /// montados assim mesmo — o que entra na migração é a estrutura das tabelas,
    /// e filtro de consulta não vira DDL.
    /// </remarks>
    private sealed class SemRequisicao : IContextoDaRequisicao
    {
        public int? UsuarioId => null;
        public int? InstituicaoId => null;
        public TipoUsuario? TipoDeConta => null;
        public bool Autenticado => false;
    }
}
