using CausaClara.Dominio.Comum;

namespace CausaClara.Dominio.Instituicoes;

/// <summary>
/// Organização da sociedade civil cadastrada no sistema (RF11), cujos dados
/// alimentam a página pública e o painel de transparência (RF25, RF27).
/// </summary>
/// <remarks>
/// Obrigatoriedade dos campos: RF11 enumera nome, descrição, cidade, estado,
/// e-mail de contato, telefone e site sem marcar nenhum como opcional, e por isso
/// todos nascem obrigatórios. A regra adotada em todo o domínio é <b>opcional
/// somente onde um requisito diz que é opcional</b> — como RF03 faz com o telefone
/// do usuário. Exigir site de uma organização pequena é restritivo, e isso está
/// registrado como ponto a confirmar; mudar exige mudar o requisito, não o código.
/// </remarks>
public sealed class Instituicao
{
    public const int TamanhoMaximoNome = 150;
    public const int TamanhoMaximoDescricao = 2000;
    public const int TamanhoMaximoCidade = 100;
    public const int TamanhoUf = 2;
    public const int TamanhoMaximoEmail = 254;
    public const int TamanhoMaximoTelefone = 20;
    public const int TamanhoMaximoSite = 255;

    /// <summary>Unidades da federação, para validar o estado sem ir ao banco.</summary>
    private static readonly HashSet<string> UnidadesFederativas = new(StringComparer.Ordinal)
    {
        "AC", "AL", "AP", "AM", "BA", "CE", "DF", "ES", "GO", "MA", "MT", "MS", "MG",
        "PA", "PB", "PR", "PE", "PI", "RJ", "RN", "RS", "RO", "RR", "SC", "SP", "SE", "TO"
    };

    /// <summary>Construtor do EF Core. Não usar no código de negócio.</summary>
    private Instituicao() { }

    private Instituicao(
        string nome, string descricao, string cidade, string estado,
        string emailContato, string telefone, string site)
    {
        Nome = nome;
        Descricao = descricao;
        Cidade = cidade;
        Estado = estado;
        EmailContato = emailContato;
        Telefone = telefone;
        Site = site;
        CriadoEm = DateTime.UtcNow;
        AtualizadoEm = CriadoEm;
    }

    public int Id { get; private set; }

    /// <summary>Único no sistema (RF13). A garantia final é o índice único no banco.</summary>
    public string Nome { get; private set; } = null!;

    public string Descricao { get; private set; } = null!;

    public string Cidade { get; private set; } = null!;

    /// <summary>Sigla da unidade federativa, em caixa alta.</summary>
    public string Estado { get; private set; } = null!;

    public string EmailContato { get; private set; } = null!;

    public string Telefone { get; private set; } = null!;

    public string Site { get; private set; } = null!;

    /// <summary>Sempre UTC (RNF28).</summary>
    public DateTime CriadoEm { get; private set; }

    /// <summary>Sempre UTC (RNF28).</summary>
    public DateTime AtualizadoEm { get; private set; }

    public static Instituicao Criar(
        string nome, string descricao, string cidade, string estado,
        string emailContato, string telefone, string site)
    {
        var dados = Validar(nome, descricao, cidade, estado, emailContato, telefone, site);
        return new Instituicao(
            dados.Nome, dados.Descricao, dados.Cidade, dados.Estado,
            dados.EmailContato, dados.Telefone, dados.Site);
    }

    /// <summary>RF25: edição do cadastro, restrita ao Gestor Dono pela camada de Aplicação.</summary>
    public void AtualizarCadastro(
        string nome, string descricao, string cidade, string estado,
        string emailContato, string telefone, string site)
    {
        var dados = Validar(nome, descricao, cidade, estado, emailContato, telefone, site);
        Nome = dados.Nome;
        Descricao = dados.Descricao;
        Cidade = dados.Cidade;
        Estado = dados.Estado;
        EmailContato = dados.EmailContato;
        Telefone = dados.Telefone;
        Site = dados.Site;
        AtualizadoEm = DateTime.UtcNow;
    }

    private static DadosInstitucionais Validar(
        string nome, string descricao, string cidade, string estado,
        string emailContato, string telefone, string site)
    {
        // A sigla não passa por ExigirTexto: um estado escrito por extenso cairia
        // na mensagem de tamanho máximo, que não diz ao usuário o que fazer.
        // A pertinência ao conjunto cobre vazio, sigla inexistente e nome por extenso,
        // sempre com a mesma orientação (RNF05).
        var uf = (estado ?? string.Empty).Trim().ToUpperInvariant();
        RegraDeNegocioViolada.Se(uf.Length == 0, "Informe o estado.");
        RegraDeNegocioViolada.Se(
            !UnidadesFederativas.Contains(uf),
            "Informe uma unidade da federação válida, com duas letras. Exemplo: MG.");

        return new DadosInstitucionais(
            RegraDeNegocioViolada.ExigirTexto(nome, "o nome da instituição", TamanhoMaximoNome),
            RegraDeNegocioViolada.ExigirTexto(descricao, "a descrição", TamanhoMaximoDescricao),
            RegraDeNegocioViolada.ExigirTexto(cidade, "a cidade", TamanhoMaximoCidade),
            uf,
            // Reaproveita a normalização do usuário: a regra de e-mail é a mesma.
            Usuarios.Usuario.NormalizarEmail(emailContato),
            RegraDeNegocioViolada.ExigirTexto(telefone, "o telefone", TamanhoMaximoTelefone),
            RegraDeNegocioViolada.ExigirTexto(site, "o site", TamanhoMaximoSite));
    }

    private sealed record DadosInstitucionais(
        string Nome, string Descricao, string Cidade, string Estado,
        string EmailContato, string Telefone, string Site);
}
