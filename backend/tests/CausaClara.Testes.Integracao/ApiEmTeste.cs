using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace CausaClara.Testes.Integracao;

/// <summary>
/// Hospeda a Api em memória para os testes.
/// </summary>
/// <remarks>
/// A conexão apontada aqui não precisa existir: registrar o <c>DbContext</c> não
/// abre conexão, e os testes desta etapa verificam apenas a composição e as rotas
/// que não tocam o banco. O teste de isolamento entre instituições (RNF12) roda
/// contra SQLite, em <c>IsolamentoPorInstituicaoTestes</c>.
/// </remarks>
public sealed class ApiEmTeste : WebApplicationFactory<Program>
{
    protected override IHost CreateHost(IHostBuilder construtor)
    {
        construtor.ConfigureHostConfiguration(configuracao =>
            configuracao.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Postgres"] =
                    "Host=localhost;Port=5432;Database=causa_clara_testes;Username=teste;Password=teste",

                // A configuração de e-mail e a do token são verificadas na subida
                // (ValidateOnStart), então precisam existir para o host levantar.
                // Nenhum teste desta etapa envia mensagem nem valida assinatura
                // contra um emissor real: os valores são só de teste.
                ["Email:Servidor"] = "localhost",
                ["Email:Porta"] = "1025",
                ["Email:Seguranca"] = "Nenhuma",
                ["Email:RemetenteEndereco"] = "nao-responda@causaclara.local",

                ["Token:Chave"] = "chave-de-teste-com-mais-de-trinta-e-dois-caracteres"
            }));

        return base.CreateHost(construtor);
    }
}
