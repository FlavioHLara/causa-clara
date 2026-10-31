using System.Net;
using System.Net.Http.Json;
using CausaClara.Aplicacao.Comum;
using Microsoft.Extensions.DependencyInjection;

namespace CausaClara.Testes.Integracao;

/// <summary>
/// Verifica que o composition root monta: a Api sobe, as rotas respondem e o
/// contêiner resolve o contrato de persistência pela interface.
/// </summary>
/// <remarks>
/// Um erro de registro de injeção de dependência só aparece em tempo de execução.
/// Sem este teste, ele apareceria no primeiro endpoint real, misturado com erros
/// de negócio. Aqui ele aparece sozinho, e o diagnóstico é imediato.
/// </remarks>
public sealed class ComposicaoDaApiTestes(ApiEmTeste api) : IClassFixture<ApiEmTeste>
{
    [Fact]
    public void O_contrato_de_persistencia_e_resolvido_pela_interface()
    {
        using var escopo = api.Services.CreateScope();

        // Resolver pela interface prova que a Aplicação nunca precisa do tipo
        // concreto; resolver sem exceção prova que AdicionarPersistencia registrou.
        var dados = escopo.ServiceProvider.GetRequiredService<IContextoDeDados>();

        Assert.NotNull(dados);
        Assert.NotNull(dados.Usuarios);
        Assert.NotNull(dados.Instituicoes);
    }

    [Fact]
    public void O_servico_de_email_e_resolvido_pela_interface()
    {
        // Resolver sem exceção prova duas coisas: AdicionarInfraestrutura
        // registrou o adaptador, e a configuração de e-mail passou pela
        // verificação de inicialização — que é o que impede descobrir, só na
        // hora de redefinir uma senha, que o servidor nunca foi configurado.
        using var escopo = api.Services.CreateScope();

        var email = escopo.ServiceProvider.GetRequiredService<IServicoDeEmail>();

        Assert.NotNull(email);
    }

    [Fact]
    public async Task A_sonda_de_vida_responde_sem_depender_do_banco()
    {
        var cliente = api.CreateClient();

        var resposta = await cliente.GetAsync("/saude/vivo");

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
    }

    [Fact]
    public async Task A_rota_de_saude_identifica_a_instancia()
    {
        var cliente = api.CreateClient();

        var resposta = await cliente.GetAsync("/api/saude");
        resposta.EnsureSuccessStatusCode();

        var corpo = await resposta.Content.ReadFromJsonAsync<RespostaDeSaude>();

        Assert.NotNull(corpo);
        Assert.Equal("Causa Clara", corpo!.Aplicacao);
        Assert.False(string.IsNullOrWhiteSpace(corpo.Ambiente));
        Assert.Equal(DateTimeKind.Utc, corpo.HorarioUtc.Kind);   // RNF28
    }

    [Theory]
    [InlineData("/api/contas/eu")]
    public async Task RNF11_rota_protegida_sem_token_responde_401(string rota)
    {
        // Prova que [Authorize] está de fato ligado no pipeline. Sem
        // UseAuthentication e UseAuthorization registrados, a rota responderia
        // 200 e o controle de acesso existiria só no atributo.
        var cliente = api.CreateClient();

        var resposta = await cliente.GetAsync(rota);

        Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
    }

    [Fact]
    public async Task RNF11_token_forjado_nao_abre_rota_protegida()
    {
        var cliente = api.CreateClient();
        cliente.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "isto.nao.e-um-token");

        var resposta = await cliente.GetAsync("/api/contas/eu");

        Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
    }

    [Fact]
    public async Task As_rotas_de_acesso_entram_no_documento_OpenAPI()
    {
        var cliente = api.CreateClient();

        var documento = await cliente.GetStringAsync("/openapi/v1.json");

        Assert.Contains("/api/contas", documento, StringComparison.Ordinal);
        Assert.Contains("/api/sessao", documento, StringComparison.Ordinal);
        Assert.Contains("/api/senha", documento, StringComparison.Ordinal);
    }

    [Fact]
    public async Task RNF31_o_documento_OpenAPI_e_publicado_em_desenvolvimento()
    {
        var cliente = api.CreateClient();

        var resposta = await cliente.GetAsync("/openapi/v1.json");

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        var documento = await resposta.Content.ReadAsStringAsync();
        Assert.Contains("/api/saude", documento, StringComparison.Ordinal);
    }

    private sealed record RespostaDeSaude(
        string Aplicacao,
        string Versao,
        string Ambiente,
        DateTime HorarioUtc);
}
