using System.Reflection;
using CausaClara.Aplicacao.Comum;
using CausaClara.Dominio.Usuarios;
using CausaClara.Infraestrutura.Extensoes;
using CausaClara.Persistencia;
using NetArchTest.Rules;

namespace CausaClara.Testes.Arquitetura;

/// <summary>
/// Evidência verificável do RNF32: a separação entre apresentação, aplicação e
/// persistência é verificada pelo compilador e por estes testes, não afirmada em prosa.
/// </summary>
/// <remarks>
/// A dependência permitida é unidirecional:
/// <code>
/// Api ──► Aplicacao ──► Dominio
///  │           ▲            ▲
///  └─ (só no composition root) ─► Persistencia ──┘
/// </code>
/// A Api referencia a Persistência porque o composition root precisa registrar a
/// injeção de dependência. Essa referência é a brecha que estes testes fecham.
/// </remarks>
public sealed class TestesDeSeparacaoDeCamadas
{
    private static readonly Assembly Api = typeof(Program).Assembly;
    private static readonly Assembly Aplicacao = typeof(IContextoDeDados).Assembly;
    private static readonly Assembly Dominio = typeof(Usuario).Assembly;
    private static readonly Assembly Persistencia = typeof(ContextoDeDados).Assembly;
    private static readonly Assembly Infraestrutura = typeof(ConfiguracaoDeInfraestrutura).Assembly;

    private const string EspacoDosControllers = "CausaClara.Api.Controllers";
    private const string EfCore = "Microsoft.EntityFrameworkCore";
    private const string Npgsql = "Npgsql";
    private const string MailKit = "MailKit";
    private const string EspacoDaPersistencia = "CausaClara.Persistencia";
    private const string EspacoDaAplicacao = "CausaClara.Aplicacao";
    private const string EspacoDaInfraestrutura = "CausaClara.Infraestrutura";
    private const string AspNetCore = "Microsoft.AspNetCore";

    // ----- a fronteira que o RNF32 cobra: apresentação não toca persistência -----

    [Fact]
    public void Controllers_nao_dependem_do_EF_Core()
        => Exigir(
            Types.InAssembly(Api).That().ResideInNamespace(EspacoDosControllers)
                 .ShouldNot().HaveDependencyOn(EfCore),
            "Controller não acessa banco. Injete um caso de uso da Aplicação.");

    [Fact]
    public void Controllers_nao_dependem_do_provedor_PostgreSQL()
        => Exigir(
            Types.InAssembly(Api).That().ResideInNamespace(EspacoDosControllers)
                 .ShouldNot().HaveDependencyOn(Npgsql),
            "Controller não conhece o banco concreto.");

    [Fact]
    public void Controllers_nao_dependem_do_projeto_de_persistencia()
        => Exigir(
            Types.InAssembly(Api).That().ResideInNamespace(EspacoDosControllers)
                 .ShouldNot().HaveDependencyOn(EspacoDaPersistencia),
            "Controller não enxerga ContextoDeDados nem mapeamentos.");

    /// <summary>
    /// Forma forte da mesma garantia: o EF Core não aparece em lugar nenhum da Api,
    /// nem fora dos controllers. Só é possível porque AdicionarPersistencia()
    /// encapsula o registro, e Program.cs chama uma linha sem tipos de EF.
    /// </summary>
    [Fact]
    public void Nenhum_tipo_da_Api_depende_do_EF_Core()
        => Exigir(
            Types.InAssembly(Api).ShouldNot().HaveDependencyOn(EfCore),
            "A Api inteira deve permanecer livre de EF Core; o registro mora em AdicionarPersistencia().");

    [Fact]
    public void Nenhum_tipo_da_Api_depende_do_provedor_PostgreSQL()
        => Exigir(
            Types.InAssembly(Api).ShouldNot().HaveDependencyOn(Npgsql),
            "Trocar de banco não pode exigir mexer na Api.");

    // ----- o Domínio é o centro: não depende de nada -----

    [Fact]
    public void Dominio_nao_depende_do_EF_Core()
        => Exigir(
            Types.InAssembly(Dominio).ShouldNot().HaveDependencyOn(EfCore),
            "Invariante de negócio não carrega anotação de persistência.");

    [Fact]
    public void Dominio_nao_depende_do_AspNetCore()
        => Exigir(
            Types.InAssembly(Dominio).ShouldNot().HaveDependencyOn(AspNetCore),
            "Regra de negócio não conhece HTTP.");

    [Fact]
    public void Dominio_nao_depende_da_Aplicacao()
        => Exigir(
            Types.InAssembly(Dominio).ShouldNot().HaveDependencyOn(EspacoDaAplicacao),
            "A dependência aponta da Aplicação para o Domínio, nunca o contrário.");

    [Fact]
    public void Dominio_nao_depende_da_Persistencia()
        => Exigir(
            Types.InAssembly(Dominio).ShouldNot().HaveDependencyOn(EspacoDaPersistencia),
            "O Domínio ignora como é armazenado.");

    // ----- a Aplicação conhece as abstrações do EF, nunca o provedor nem o concreto -----

    [Fact]
    public void Aplicacao_nao_depende_do_provedor_PostgreSQL()
        => Exigir(
            Types.InAssembly(Aplicacao).ShouldNot().HaveDependencyOn(Npgsql),
            "A Aplicação usa DbSet e IDbContextTransaction, que são abstrações; o provedor é detalhe da Persistência.");

    [Fact]
    public void Aplicacao_nao_depende_da_Persistencia()
        => Exigir(
            Types.InAssembly(Aplicacao).ShouldNot().HaveDependencyOn(EspacoDaPersistencia),
            "A Aplicação conversa com IContextoDeDados, não com ContextoDeDados.");

    [Fact]
    public void Aplicacao_nao_depende_do_AspNetCore()
        => Exigir(
            Types.InAssembly(Aplicacao).ShouldNot().HaveDependencyOn(AspNetCore),
            "Caso de uso não conhece HTTP; quem traduz é o controller.");

    // ----- a Infraestrutura isola o mundo externo -----

    [Fact]
    public void Nenhum_tipo_da_Api_depende_da_biblioteca_de_email()
        => Exigir(
            Types.InAssembly(Api).ShouldNot().HaveDependencyOn(MailKit),
            "Trocar a forma de enviar e-mail não pode exigir mexer na Api; o registro mora em AdicionarInfraestrutura().");

    [Fact]
    public void A_Aplicacao_nao_depende_da_biblioteca_de_email()
        => Exigir(
            Types.InAssembly(Aplicacao).ShouldNot().HaveDependencyOn(MailKit),
            "O caso de uso conhece IServicoDeEmail; SMTP é detalhe da Infraestrutura.");

    [Fact]
    public void O_Dominio_nao_depende_da_biblioteca_de_email()
        => Exigir(
            Types.InAssembly(Dominio).ShouldNot().HaveDependencyOn(MailKit),
            "Regra de negócio não envia e-mail.");

    [Fact]
    public void A_Infraestrutura_nao_depende_da_Persistencia()
        => Exigir(
            Types.InAssembly(Infraestrutura).ShouldNot().HaveDependencyOn(EspacoDaPersistencia),
            "Adaptador externo e banco são caminhos independentes; quem os une é o caso de uso.");

    /// <summary>
    /// A Infraestrutura não toca a superfície HTTP do ASP.NET Core.
    /// </summary>
    /// <remarks>
    /// Verifica os espaços de nomes de HTTP um a um, e não o prefixo
    /// <c>Microsoft.AspNetCore</c>. O motivo é concreto: o <c>PasswordHasher</c>
    /// que atende ao RNF10 mora em <c>Microsoft.AspNetCore.Identity</c> mas é
    /// distribuído em <c>Microsoft.Extensions.Identity.Core</c>, pacote que não
    /// traz servidor, pipeline nem nada de web. Reprovar por causa do prefixo
    /// seria reprovar pelo nome, não pela dependência — e a regra que importa é
    /// "adaptador externo não conhece HTTP".
    /// </remarks>
    [Theory]
    [InlineData("Microsoft.AspNetCore.Http")]
    [InlineData("Microsoft.AspNetCore.Mvc")]
    [InlineData("Microsoft.AspNetCore.Routing")]
    [InlineData("Microsoft.AspNetCore.Builder")]
    [InlineData("Microsoft.AspNetCore.Hosting")]
    public void A_Infraestrutura_nao_depende_do_HTTP(string espacoDeHttp)
        => Exigir(
            Types.InAssembly(Infraestrutura).ShouldNot().HaveDependencyOn(espacoDeHttp),
            $"Adaptador externo não conhece HTTP, e {espacoDeHttp} é superfície de HTTP.");

    [Fact]
    public void A_Aplicacao_nao_depende_da_Infraestrutura()
        => Exigir(
            Types.InAssembly(Aplicacao).ShouldNot().HaveDependencyOn(EspacoDaInfraestrutura),
            "A dependência aponta da Infraestrutura para a Aplicação, nunca o contrário.");

    // ----- guarda contra aprovação vazia -----

    /// <summary>
    /// Os três primeiros testes filtram por namespace, e um filtro que não casa
    /// com nada passa por vacuidade. Este teste garante que há de fato controllers
    /// sendo verificados — sem ele, apagar todos os controllers deixaria a suíte verde.
    /// </summary>
    [Fact]
    public void Existe_controller_para_os_testes_de_fronteira_verificarem()
    {
        var controllers = Types.InAssembly(Api)
            .That().ResideInNamespace(EspacoDosControllers)
            .GetTypes()
            .ToList();

        Assert.True(
            controllers.Count > 0,
            $"Nenhum tipo em {EspacoDosControllers}: os testes de fronteira estariam passando por vacuidade.");
    }

    /// <summary>
    /// A Persistência é a única camada autorizada a conhecer o provedor.
    /// Se este teste falhar, o provedor foi removido do lugar certo.
    /// </summary>
    [Fact]
    public void Persistencia_e_a_unica_camada_que_conhece_o_provedor()
    {
        var conheceNpgsql = Types.InAssembly(Persistencia)
            .That().HaveDependencyOn(Npgsql)
            .GetTypes()
            .Any();

        Assert.True(conheceNpgsql, "A Persistência deveria ser quem conhece o Npgsql.");
    }

    private static void Exigir(ConditionList condicao, string porque)
    {
        var resultado = condicao.GetResult();
        var infratores = resultado.FailingTypeNames is null
            ? string.Empty
            : string.Join(", ", resultado.FailingTypeNames);

        Assert.True(resultado.IsSuccessful, $"{porque}{Environment.NewLine}Tipos infratores: {infratores}");
    }
}
