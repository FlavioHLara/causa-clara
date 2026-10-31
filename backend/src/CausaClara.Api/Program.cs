using System.Text.Json.Serialization;
using CausaClara.Api.Autenticacao;
using CausaClara.Api.Middleware;
using CausaClara.Aplicacao.Acesso;
using CausaClara.Aplicacao.Comum;
using CausaClara.Aplicacao.Extensoes;
using CausaClara.Infraestrutura.Extensoes;
using CausaClara.Infraestrutura.Seguranca;
using CausaClara.Persistencia.Extensoes;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;

var construtor = WebApplication.CreateBuilder(args);

// RNF18: a conexão vem da configuração do ambiente, nunca do repositório.
// No compose, chega como ConnectionStrings__Postgres.
var conexao = construtor.Configuration.GetConnectionString("Postgres");
if (string.IsNullOrWhiteSpace(conexao))
{
    throw new InvalidOperationException(
        "String de conexão ausente. Defina ConnectionStrings__Postgres no ambiente. " +
        "Para desenvolvimento local, copie .env.example para .env e use 'docker compose up'.");
}

// Quem está requisitando, lido das claims. Precisa ser registrado antes de
// qualquer resolução do contexto de dados: o filtro global por instituição
// (RNF12) e o interceptador de auditoria (RF81) dependem dele.
construtor.Services.AddHttpContextAccessor();
construtor.Services.AddScoped<IContextoDaRequisicao, ContextoDaRequisicao>();

// Único ponto em que a Api toca a Persistência: o composition root (RNF32).
construtor.Services.AdicionarPersistencia(conexao);

// Adaptadores externos: e-mail, resumo de senha e emissão de token.
construtor.Services.AdicionarInfraestrutura(construtor.Configuration);

// Casos de uso. Os prazos de sessão chegam resolvidos, da configuração.
construtor.Services.AdicionarAplicacao(
    construtor.Configuration.GetSection("Acesso").Get<OpcoesDeAcesso>() ?? new OpcoesDeAcesso());

// RNF11 e RNF13: credencial assinada, validada no servidor a cada requisição.
var opcoesDeToken = construtor.Configuration.GetSection(OpcoesDeToken.Secao).Get<OpcoesDeToken>()
                    ?? new OpcoesDeToken();

construtor.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opcoes => opcoes.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = opcoesDeToken.Emissor,
        ValidateAudience = true,
        ValidAudience = opcoesDeToken.Audiencia,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = opcoesDeToken.ChaveDeAssinatura(),
        ValidateLifetime = true,

        // O padrão da biblioteca é cinco minutos de tolerância, o que estenderia
        // na prática o teto de validade do RNF13. Trinta segundos cobrem
        // diferença de relógio entre máquinas sem afrouxar o prazo.
        ClockSkew = TimeSpan.FromSeconds(30)
    });

construtor.Services.AddAuthorization();

construtor.Services
    .AddControllers()
    .AddJsonOptions(opcoes =>
        // Enum viaja como texto: "Gestor", não 1. O padrão do System.Text.Json
        // é numérico, o que obrigaria o cliente a conhecer a ordem dos membros —
        // e reordenar o enum mudaria o significado dos dados em trânsito sem
        // quebrar compilação nenhuma. Também alinha a API ao banco, onde o enum
        // já é texto com restrição CHECK, e deixa o documento OpenAPI legível.
        opcoes.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// RNF05: erro devolvido em português, no formato ProblemDetails, sem rastreamento
// de pilha nem detalhe técnico. A ordem importa — o primeiro tratador que
// responder encerra a cadeia.
construtor.Services.AddProblemDetails();
construtor.Services.AddExceptionHandler<TratadorDeRegraDeNegocio>();
construtor.Services.AddExceptionHandler<TratadorDeFalhaInesperada>();

// RNF31: documento OpenAPI. A interface interativa entra só em desenvolvimento.
construtor.Services.AddOpenApi();

var aplicacao = construtor.Build();

// RNF33 e RNF35: no compose, a Api aplica as migrações pendentes ao subir, para
// que um único comando entregue banco e esquema prontos. Fora dali o padrão é
// não aplicar — migrar na inicialização não é adequado com várias instâncias.
if (construtor.Configuration.GetValue<bool>("AplicarMigracoesNaInicializacao"))
{
    await aplicacao.Services.AplicarMigracoesPendentesAsync();
}

aplicacao.UseExceptionHandler();

if (aplicacao.Environment.IsDevelopment())
{
    aplicacao.MapOpenApi();                 // documento em /openapi/v1.json
    aplicacao.MapScalarApiReference();       // interface interativa em /scalar/v1
}
else
{
    // RNF14. Em produção o TLS termina na plataforma de hospedagem; este
    // redirecionamento cobre o caso de a aplicação ser alcançada em texto claro.
    aplicacao.UseHttpsRedirection();
}

// A ordem importa: autenticar identifica quem é, autorizar decide o que pode.
aplicacao.UseAuthentication();
aplicacao.UseAuthorization();

aplicacao.MapControllers();

// Sondas de contêiner, separadas de propósito: "vivo" responde enquanto o
// processo estiver de pé, "pronto" só depois que o banco aceitar conexão.
// Juntas, elas evitam que o orquestrador mate um processo saudável que ainda
// está esperando o banco subir.
aplicacao.MapHealthChecks("/saude/vivo", new() { Predicate = _ => false });
aplicacao.MapHealthChecks("/saude/pronto");

aplicacao.Run();

/// <summary>
/// Exposto para que <c>WebApplicationFactory</c> possa hospedar a Api nos
/// testes de integração. Sem isto, o <c>Program</c> gerado é interno.
/// </summary>
public partial class Program;
