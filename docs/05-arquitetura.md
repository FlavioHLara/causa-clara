# Projeto Arquitetural — Causa Clara

**Sistema de Gestão Administrativa e Transparência Financeira para Organizações do Terceiro Setor**
Insumo para as seções 4.3 e 5 do artigo

---

## 1. Decisões estruturantes

Três requisitos não funcionais determinam quase toda a arquitetura. Vale enunciá-los antes porque cada decisão adiante responde a um deles:

| Requisito | O que exige | Consequência arquitetural |
|---|---|---|
| **RNF32** | Separação entre apresentação, aplicação e persistência, vedado o acesso direto ao banco pela apresentação | Quatro projetos com dependência unidirecional, verificada por teste |
| **RNF12** | Toda requisição a recurso de instituição valida o vínculo no servidor | Filtro global de consulta por instituição, não verificação manual por endpoint |
| **RNF26** | Movimentação, saldo e alerta na mesma transação | Serviço de domínio transacional com controle de concorrência otimista |

O restante é consequência ou é convencional.

---

## 2. Monorepo

```
/
├─ docs/                     artigo, requisitos, diagramas, protótipo
├─ infra/
│  ├─ docker-compose.yml     banco + api + web (RNF33)
│  └─ .env.example           segredos por variável de ambiente (RNF18)
├─ backend/
│  ├─ src/
│  │  ├─ Dominio/
│  │  ├─ Aplicacao/
│  │  ├─ Persistencia/
│  │  └─ Api/
│  ├─ tests/
│  └─ Backend.sln
├─ frontend/
│  ├─ src/
│  └─ package.json
└─ package.json              scripts de orquestração
```

Monorepo é a escolha certa aqui: um autor, um ciclo de release, contrato entre front e back que muda junto. **Não use Nx nem Turborepo** — eles resolvem grafo de dependências entre dezenas de pacotes, problema que este projeto não tem. Scripts npm na raiz bastam.

---

## 3. Backend: quatro camadas, dependência em sentido único

```
Api  ──────────►  Aplicacao  ──────────►  Dominio
 │                     ▲                      ▲
 └──── (só no          │                      │
     composition   Persistencia ──────────────┘
       root) ─────────►
```

| Projeto | Responsabilidade | Depende de |
|---|---|---|
| **Dominio** | Entidades, enums e invariantes de negócio (RN01–RN16). Nenhuma dependência de framework. | nada |
| **Aplicacao** | Casos de uso, DTOs, validação, contrato de persistência, paginação | Dominio |
| **Persistencia** | EF Core: contexto, mapeamentos, migrações, interceptadores | Aplicacao, Dominio |
| **Api** | Controllers, autenticação, autorização, middleware, OpenAPI | Aplicacao (+ Persistencia apenas para registrar a injeção de dependência) |

### Por que projetos separados e não pastas

Pastas dependem de disciplina; projetos dependem do compilador. Com `Dominio` sem referência a EF Core, é **impossível** vazar uma anotação de persistência para dentro da regra de negócio — o código não compila.

A fronteira que o RNF32 realmente cobra é `Api ↛ Persistencia`. Como a `Api` precisa referenciar `Persistencia` no *composition root* para registrar os serviços, a referência existe e um descuido poderia usar o `DbContext` direto de um controller. A proteção é um **teste de arquitetura**:

```csharp
[Fact]
public void Apresentacao_nao_deve_depender_de_EntityFrameworkCore()
{
    var resultado = Types.InAssembly(typeof(Program).Assembly)
        .That().ResideInNamespace("Api.Controllers")
        .ShouldNot().HaveDependencyOn("Microsoft.EntityFrameworkCore")
        .GetResult();

    Assert.True(resultado.IsSuccessful, string.Join(", ", resultado.FailingTypeNames ?? []));
}
```

Esse teste é a **evidência verificável do RNF32** para a banca, e é mais honesto do que afirmar a separação em prosa.

### Contrato de persistência

A `Aplicacao` não conhece o `DbContext` concreto, mas conhece uma interface:

```csharp
// Aplicacao/Contratos/IContextoDeDados.cs
public interface IContextoDeDados
{
    DbSet<Usuario> Usuarios { get; }
    DbSet<Instituicao> Instituicoes { get; }
    DbSet<MembroInstituicao> MembrosInstituicao { get; }
    DbSet<Doacao> Doacoes { get; }
    DbSet<Despesa> Despesas { get; }
    DbSet<ItemEstoque> ItensEstoque { get; }
    DbSet<MovimentacaoEstoque> MovimentacoesEstoque { get; }
    DbSet<Alerta> Alertas { get; }
    DbSet<Categoria> Categorias { get; }
    DbSet<RegistroDeAuditoria> Auditoria { get; }

    Task<int> SaveChangesAsync(CancellationToken ct = default);
    Task<IDbContextTransaction> IniciarTransacaoAsync(CancellationToken ct = default);
}
```

**Decisão consciente:** não há repositório por entidade. O `DbContext` já é *Unit of Work* e *Repository*; envolvê-lo em quinze classes repassadoras acrescenta código sem acrescentar capacidade. A `Aplicacao` referencia EF Core, o que é um acoplamento aceito e explícito. O que não se admite é a **apresentação** tocar persistência — essa é a fronteira do RNF32.

---

## 4. Isolamento entre instituições — o ponto de maior risco

RNF12 e RN05 exigem que um gestor da instituição A jamais alcance dado da instituição B, inclusive trocando um identificador na URL. Verificar isso endpoint a endpoint é frágil: basta esquecer um `if` entre quarenta métodos para abrir a falha.

A solução é estrutural. Toda entidade que pertence a uma instituição implementa um marcador:

```csharp
public interface IPertenceAInstituicao { int InstituicaoId { get; } }
```

E o contexto aplica **filtro global de consulta**:

```csharp
// Persistencia/ContextoDeDados.cs
protected override void OnModelCreating(ModelBuilder mb)
{
    mb.Entity<Doacao>().HasQueryFilter(d => d.InstituicaoId == _contexto.InstituicaoId);
    mb.Entity<Despesa>().HasQueryFilter(d => d.InstituicaoId == _contexto.InstituicaoId);
    mb.Entity<ItemEstoque>().HasQueryFilter(i => i.InstituicaoId == _contexto.InstituicaoId);
    // ...
}
```

Onde `_contexto` é um serviço com escopo de requisição, preenchido a partir das *claims* do token:

```csharp
public interface IContextoDaRequisicao
{
    int? UsuarioId { get; }
    int? InstituicaoId { get; }
    PapelMembro? Papel { get; }
    TipoUsuario? TipoUsuario { get; }
}
```

**Consequência:** um identificador forjado não retorna dado alheio, retorna *não encontrado* — porque a linha simplesmente não existe no conjunto consultável. A falha de IDOR fecha por construção, não por lembrança.

**Duas ressalvas que precisam de cuidado:**

1. O filtro global captura o serviço de contexto, não o valor. A expressão deve ler `_contexto.InstituicaoId` no momento da consulta, e o contexto precisa ter o mesmo escopo do `DbContext`.
2. A **transparência pública** (RF63–RF73) lê dados de uma instituição sem usuário autenticado. Esse caminho não pode usar o filtro. Resolva com um serviço de leitura dedicado que chama `IgnoreQueryFilters()` e filtra explicitamente pelo `instituicaoId` da rota, devolvendo apenas os campos que o RNF20 e o RNF23 autorizam. Separar o caminho público do interno em serviços distintos é o que impede que uma projeção interna vaze para o público por descuido.

---

## 5. Movimentação de estoque — a operação mais delicada

Cinco regras incidem sobre a mesma operação: RN11 (saldo derivado), RN12 (nunca negativo), RN13 (situação crítica), RN14 (alerta uma única vez), RNF26 (transação única).

A regra mora na entidade, não no controller:

```csharp
// Dominio/ItemEstoque.cs
public ResultadoMovimentacao Movimentar(TipoMovimentacao tipo, int quantidade, int gestorId, string? observacao)
{
    if (quantidade <= 0)
        throw new RegraDeNegocioViolada("A quantidade deve ser maior que zero.");

    if (tipo == TipoMovimentacao.Saida && quantidade > QtdAtual)
        throw new RegraDeNegocioViolada(
            $"Quantidade insuficiente: há {QtdAtual} {UnidadeMedida} disponíveis."); // RN12

    var anterior = QtdAtual;
    QtdAtual = tipo == TipoMovimentacao.Entrada ? QtdAtual + quantidade : QtdAtual - quantidade; // RN11
    AtualizadoEm = DateTime.UtcNow;

    // RN14: alerta só quando a quantidade CRUZA o mínimo, não a cada saída abaixo dele
    var cruzouLimite = tipo == TipoMovimentacao.Saida
                       && anterior > QtdMinima
                       && QtdAtual <= QtdMinima;

    return new ResultadoMovimentacao(anterior, QtdAtual, cruzouLimite);
}
```

O caso de uso coordena a transação:

```csharp
await using var tx = await _dados.IniciarTransacaoAsync(ct);
var resultado = item.Movimentar(dto.Tipo, dto.Quantidade, _contexto.UsuarioId!.Value, dto.Observacao);
_dados.MovimentacoesEstoque.Add(movimentacao);
if (resultado.CruzouLimite) _dados.Alertas.Add(Alerta.DeEstoqueCritico(item));
await _dados.SaveChangesAsync(ct);   // saldo + movimentação + alerta juntos (RNF26)
await tx.CommitAsync(ct);
```

### Concorrência

`QtdAtual` é campo materializado. Duas saídas simultâneas poderiam ler o mesmo saldo e gravar valores inconsistentes. O PostgreSQL oferece a coluna de sistema `xmin`, que serve de token de concorrência sem acrescentar coluna ao esquema:

```csharp
mb.Entity<ItemEstoque>().Property<uint>("Versao")
  .HasColumnName("xmin").HasColumnType("xid").ValueGeneratedOnAddOrUpdate()
  .IsConcurrencyToken();
```

A segunda gravação falha com `DbUpdateConcurrencyException`, que o caso de uso traduz em nova tentativa ou em mensagem ao gestor.

**Alternativa descartada:** calcular `SUM(movimentações)` a cada leitura dispensaria o token e honraria RN11 literalmente, mas tornaria a listagem de estoque (RF49) uma agregação por item em toda consulta. O campo materializado mais transação mais token é o equilíbrio correto, e a verdade continua sendo o histórico — o campo é cache, não fonte.

---

## 6. Trilha de auditoria por interceptador

RF81 exige registrar criação, alteração e exclusão de lançamentos financeiros e movimentações. Espalhar essa gravação pelos casos de uso garante esquecimento. Um interceptador de `SaveChanges` centraliza:

```csharp
public sealed class InterceptadorDeAuditoria(IContextoDaRequisicao ctx) : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData e, InterceptionResult<int> r, CancellationToken ct = default)
    {
        foreach (var entrada in e.Context!.ChangeTracker.Entries<IAuditavel>())
        {
            if (entrada.State is not (EntityState.Added or EntityState.Modified or EntityState.Deleted))
                continue;
            e.Context.Set<RegistroDeAuditoria>().Add(RegistroDeAuditoria.De(entrada, ctx));
        }
        return base.SavingChangesAsync(e, r, ct);
    }
}
```

O mesmo interceptador trata a **exclusão lógica** do RN10: entradas `Deleted` de tipos `IExcluivelLogicamente` viram `Modified` com `Excluido = true`, e um filtro global esconde as excluídas. O lançamento some da listagem sem sumir da trilha.

---

## 7. Persistência

| Decisão | Razão |
|---|---|
| `numeric(14,2)` no banco, `decimal` em C# | RNF24: ponto flutuante binário erra centavo em soma |
| `timestamptz`, sempre UTC | RNF28; conversão para fuso local só na apresentação |
| Enums gravados como texto + `CHECK` | Legibilidade no banco e integridade sem depender da aplicação |
| Chaves estrangeiras explícitas | RNF27 |
| **EF Core Migrations** | RNF35 exige versionamento incremental |
| `nome` de instituição com índice único | RF13 |
| `(usuario_id, instituicao_id)` único em membros | RF17 |
| Índice em `(instituicao_id, data)` nas tabelas de lançamento | Toda consulta filtra por instituição e período |

**Mudança em relação ao projeto anterior:** o OngConecta mantém o esquema num `init.sql` escrito à mão, o que não atende ao RNF35. Migrações do EF Core passam a ser a fonte da verdade do esquema, e o `init.sql` deixa de existir.

### Paginação

O par `ParametrosListagem` / `PaginaResposta<T>` já existente é bom e atende ao RNF09. Ele sobe para a `Aplicacao`, com o teto de cem registros validado no servidor — o cliente não decide o limite.

---

## 8. Frontend

```
frontend/src/
├─ app/
│  ├─ main.tsx
│  ├─ rotas.tsx            rotas + guardas por tipo de conta e papel
│  └─ provedores.tsx
├─ modulos/                organizado por funcionalidade, não por tipo de arquivo
│  ├─ acesso/              RF01–RF09
│  ├─ vinculo/             RF10–RF24
│  ├─ instituicoes/        RF25–RF27
│  ├─ doacoes/             RF28–RF40
│  ├─ despesas/            RF41–RF46
│  ├─ estoque/             RF47–RF62
│  ├─ transparencia/       RF63–RF73
│  └─ relatorios/          RF74–RF81
├─ ui/                     design system: Botao, Campo, Tabela, Etiqueta, Paginacao...
├─ servicos/
│  ├─ cliente.ts           axios + interceptadores de token e de erro
│  └─ api-tipos.ts         GERADO do OpenAPI, não editar à mão
├─ estilos/
│  ├─ tokens.css           GERADO das variáveis do Figma
│  └─ base.css
└─ hooks/
```

Cada módulo guarda `paginas/`, `componentes/`, `api.ts` e `tipos.ts`. Agrupar por funcionalidade mantém junto o que muda junto; agrupar por tipo de arquivo espalha uma alteração de doação por seis pastas.

**Guardas de rota** espelham o RNF11 para experiência de uso, mas não são a trava. A autorização real é do servidor — a interface só evita oferecer o que será recusado.

**Formulários:** `react-hook-form` com `zod`. Os esquemas `zod` expressam as mesmas regras do domínio (RN06, por exemplo: financeira exige valor maior que zero; material exige item ou descrição). A validação do cliente é conveniência; o servidor revalida tudo, conforme RNF15.

**Estado de servidor:** recomendo **TanStack Query**. Dezenove telas com tabelas paginadas, filtros e invalidação após gravação significam que, sem ela, você reescreve cache, estado de carregamento, tratamento de erro e refetch à mão em cada tela — e vai errar em algumas. É uma dependência a defender, e a defesa é objetiva. Se preferir manter a pilha mínima do projeto anterior, o caminho é concentrar essa lógica num único hook de listagem, como o `useListagem` que já existe, e aceitar que cache entre telas não vai existir.

---

## 9. Dois pontos de continuidade com o que já está pronto

### Tokens de design

As variáveis do Figma já foram criadas com *code syntax* no padrão `var(--cor-primaria)`. Isso não foi decorativo: elas geram diretamente o `estilos/tokens.css`, e o nome do token é o mesmo no design e no código. Quando a cor primária mudar no Figma, muda em um lugar e desce para o CSS.

A API REST de variáveis do Figma é restrita ao plano Enterprise, mas o plano atual permite ler as variáveis locais pelo servidor MCP e emitir o CSS por script — é um gerador de dez linhas.

### Contrato entre front e back

O RNF31 já obriga a expor OpenAPI. Aproveite: gere os tipos TypeScript a partir do documento, com `openapi-typescript`, para `servicos/api-tipos.ts`. Um script na raiz:

```json
{ "scripts": { "tipos": "openapi-typescript http://localhost:5000/openapi/v1.json -o frontend/src/servicos/api-tipos.ts" } }
```

> O caminho é `/openapi/v1.json`, e não `/swagger/...`: o .NET 10 gera o documento OpenAPI nativamente e o Swashbuckle saiu do template. Ver seção 16.

Assim um campo renomeado no DTO do backend quebra a compilação do frontend, em vez de quebrar em produção. É o maior ganho prático do monorepo neste projeto.

---

## 10. Infraestrutura

`docker-compose` com três serviços — banco, api, web — subindo com um comando, conforme RNF33. Segredos por variável de ambiente, nunca no repositório (RNF18).

Para publicação, o critério do RNF40 é custo nulo ou reduzido. Render com PostgreSQL gratuito atende e já é conhecido do projeto anterior. Vale saber que instâncias gratuitas hibernam após inatividade, o que provoca alguns segundos na primeira requisição — irrelevante para avaliação, mas convém mencionar no artigo se houver medição de tempo de resposta.

---

## 11. Testes

A lista final de requisitos não traz exigência de testes automatizados. Ainda assim, três conjuntos pequenos valem o esforço porque **produzem evidência** para a banca:

1. **Teste de arquitetura** — a apresentação não depende de EF Core. Evidência do RNF32.
2. **Testes de unidade do domínio** — RN06, RN11, RN12, RN14 e RN16. São funções puras, rodam em milissegundos, e cada teste nomeia a regra que verifica.
3. **Um teste de integração de isolamento** — usuário vinculado à instituição A consulta recurso da instituição B e recebe *não encontrado*. É o teste mais valioso do projeto: cobre o RNF12, que é o requisito de maior risco.

Não persiga cobertura alta. Esses três conjuntos custam pouco e sustentam afirmações do artigo.

---

## 12. O que deliberadamente não entra

| Descartado | Por quê |
|---|---|
| Microsserviços | Um domínio, um autor, um banco. Acrescentaria rede e orquestração sem resolver problema existente. |
| CQRS, MediatR, *event sourcing* | A complexidade se paga quando leitura e escrita escalam separadamente. Aqui, não. |
| Repositório por entidade | O `DbContext` já cumpre *Unit of Work* e *Repository*. |
| Nx, Turborepo | Resolvem grafos de dezenas de pacotes. |
| Cache distribuído, fila de mensagens | Não há operação assíncrona no escopo. |

Arquitetura também se defende pelo que foi recusado. Vale uma frase sobre isso na seção de desenvolvimento do artigo.

---

## 13. Ponto de partida

**Decisão: base nova, sem herdar código do projeto interdisciplinar.** O custo é refazer a modelagem de domínio e o front que já existiam; o ganho é uma estrutura que nasce nas quatro camadas, sem dívida acumulada e sem qualquer discussão sobre reaproveitamento na defesa — a cláusula do regulamento que permite reuso (Art. 3º §2º) trata de textos e projetos de pesquisa, não de código-fonte.

O projeto anterior permanece disponível como **referência de consulta**, não como origem. Olhar como um problema foi resolvido antes é legítimo e não é reaproveitamento.

### Nome: Causa Clara

| Uso | Forma |
|---|---|
| Exibição, interface, artigo | **Causa Clara** |
| *Namespace* raiz e solução | `CausaClara` |
| Repositório, compose, pastas | `causa-clara` |

> **Causa Clara** — gestão administrativa e transparência financeira para organizações da sociedade civil

"Causa" é a palavra do setor: uma organização não se descreve pela categoria jurídica, e sim pela causa que abraça. "Clara" entrega transparência sem usar a palavra "transparência", gasta no campo. Juntas produzem duas leituras que se sustentam — uma causa cujas contas são claras, e uma causa que é legítima —, e a segunda é exatamente o que a literatura do referencial atribui à transparência: confiança como patrimônio, em Slomski, e transparência como relação com *stakeholders*, em Dethier.

Nove candidatos foram testados antes. O registro do que foi descartado, e por quê, está em `AGENTS.md` para não ser reaberto.

A marca já foi aplicada ao protótipo no Figma em 21/10/2026 — nove textos trocados, incluindo as três variantes da barra lateral e o cabeçalho público. A instituição fictícia dos dados de exemplo continua sendo *Casa de Apoio Semear*: é dado de amostra, não marca, e deve permanecer.

---

## 15. Estrutura do repositório

### Raiz

```
causa-clara/
├─ backend/
├─ frontend/
├─ docs/
├─ docker-compose.yml
├─ .env.example
├─ .editorconfig
├─ .gitignore
├─ package.json
└─ README.md
```

Sem pasta `infra/`: ficaria com um arquivo só, e pasta quase vazia atrapalha mais do que organiza.

O `docker-compose.yml` fica na **raiz** de propósito. O RNF33 pede que o ambiente suba "com um único comando" — na raiz, é `docker compose up`. Dentro de `infra/`, viraria `docker compose -f infra/docker-compose.yml up`, que já não é um comando único. Os `Dockerfile` ficam junto do que cada um constrói, para o contexto de build sair correto.

### Backend

```
backend/
├─ CausaClara.sln
├─ Dockerfile
├─ src/
│  ├─ CausaClara.Dominio/
│  │  ├─ Comum/            marcadores, exceção de regra de negócio
│  │  ├─ Usuarios/
│  │  ├─ Instituicoes/     instituição e vínculo
│  │  ├─ Doacoes/
│  │  ├─ Despesas/
│  │  ├─ Estoque/          item, movimentação, alerta
│  │  └─ Auditoria/
│  ├─ CausaClara.Aplicacao/
│  │  ├─ Comum/            paginação, contexto da requisição, contrato de dados
│  │  ├─ Usuarios/
│  │  ├─ Instituicoes/
│  │  ├─ Doacoes/
│  │  ├─ Despesas/
│  │  ├─ Estoque/
│  │  ├─ Transparencia/    leitura pública, caminho separado
│  │  └─ Relatorios/
│  ├─ CausaClara.Persistencia/
│  │  ├─ ContextoDeDados.cs
│  │  ├─ Mapeamentos/      um por entidade
│  │  ├─ Interceptadores/  auditoria e exclusão lógica
│  │  ├─ Migrations/       geradas pelo EF
│  │  └─ Extensoes/        AddPersistencia(), ponto único de contato da Api
│  └─ CausaClara.Api/
│     ├─ Program.cs
│     ├─ Controllers/
│     ├─ Autenticacao/
│     └─ Middleware/
└─ tests/
   ├─ CausaClara.Testes.Arquitetura/
   ├─ CausaClara.Testes.Dominio/
   └─ CausaClara.Testes.Integracao/
```

### Frontend

```
frontend/src/
├─ app/            bootstrap, rotas, guardas, provedores
├─ modulos/
│  ├─ acesso/  vinculo/  instituicoes/  doacoes/
│  ├─ despesas/  estoque/  transparencia/  relatorios/
├─ ui/             design system: Botao, Campo, Tabela, Etiqueta, Paginacao
├─ servicos/       cliente http e tipos gerados do OpenAPI
├─ estilos/        tokens.css (do Figma) e base.css
└─ hooks/
```

Cada módulo guarda `paginas/`, `componentes/`, `api.ts` e `tipos.ts`.

### Quatro decisões que dão essa forma

**1. A árvore organiza por módulo de negócio, não por tipo técnico.** Uma pasta `Controllers/ Services/ Repositories/` não diz nada sobre o sistema — poderia ser e-commerce ou hospital. `Doacoes/ Despesas/ Estoque/` diz o que o software faz antes de abrir qualquer arquivo. O efeito prático: alterar a regra de doação material mexe em `Doacoes/` nas três camadas e só; na organização por tipo, a mesma alteração se espalha por quatro pastas distantes.

**2. Os módulos se repetem nas camadas, e isso é o custo, não um erro.** `Doacoes` aparece em Domínio, Aplicação e Persistência. A alternativa — fatia vertical, uma pasta por funcionalidade com tudo dentro — dissolveria a fronteira que o RNF32 exige. Entre repetir nome de pasta e perder a verificação de camada, repetir é o preço menor.

**3. Transparência é módulo próprio na Aplicação.** A leitura pública ignora o filtro por instituição e devolve apenas o que RNF20 e RNF23 autorizam. Se morasse dentro de `Doacoes/`, cedo ou tarde alguém reaproveitaria um DTO interno num endpoint público e vazaria nome completo ou e-mail. Separar no diretório torna o reaproveitamento indevido um ato deliberado, não um descuido.

**4. Os módulos do front espelham os do back, com os mesmos nomes.** Um vocabulário só nos dois lados, e as faixas de RF caem na mesma pasta em ambos.

---

## 14. Ordem de construção

Greenfield costuma falhar na sequência, não na estrutura. Duas armadilhas específicas deste projeto:

> **Os mecanismos transversais vêm antes dos casos de uso, não depois.** Acrescentar o filtro global de instituição e o interceptador de auditoria quando já existem quarenta endpoints significa revisar os quarenta e torcer para não ter esquecido nenhum. Construídos na etapa 2, todo endpoint nasce coberto.

> **Construa em fatias verticais, não em camadas horizontais.** Fazer "todo o backend e depois todo o frontend" deixa meses sem nada demonstrável e empurra a integração para o fim, que é quando o prazo aperta. Cada módulo sai do banco à tela antes do próximo começar.

| # | Etapa | Entrega | RF |
|---|---|---|---|
| 1 | **Fundação** | Solução com quatro projetos, compose subindo banco/api/web, migração inicial, OpenAPI, teste de arquitetura | — |
| 2 | **Transversais** | Contexto da requisição, filtro global por instituição, interceptador de auditoria, exclusão lógica, paginação | RNF12, RNF32, RF81, RN10 |
| 3 | **Acesso** | Cadastro com dois tipos de conta, autenticação, sessão | RF01–RF09 |
| 4 | **Vínculo institucional** | Criar instituição, solicitar acesso, aprovar e rejeitar, papéis | RF10–RF24 |
| 5 | **Cadastro e consulta pública** | Dados da instituição, listagem pública, página pública | RF25–RF27 |
| 6 | **Doações** | Registro pelo apoiador, lançamento pelo gestor, histórico, anonimato | RF28–RF40 |
| 7 | **Despesas** | Lançamento, anexo de comprovante, vínculo com a doação que custeou, categorias | RF41–RF46 |
| 8 | **Estoque** | Itens, movimentação transacional, alertas, valorização | RF47–RF62 |
| 9 | **Transparência pública** | Painel sem autenticação, gráficos, exportação | RF63–RF73 |
| 10 | **Painéis e prestação de contas** | Painel do gestor, relatórios, demonstrativo | RF74–RF81 |

A etapa 4 vem antes de tudo que é institucional porque todo lançamento depende de existir um vínculo aprovado — sem ela, não há contexto de instituição para o filtro global resolver.

A etapa 8 é a mais longa e a mais arriscada: concentra cinco regras de negócio sobre a mesma operação. Convém reservar folga e escrever os testes de unidade do domínio junto, não depois.

---

## 16. Divergências entre este projeto e a implementação

Registradas na etapa 1 (29/10/2026). Nenhuma altera decisão estruturante; todas são ajustes de detalhe que a ferramenta ou a convenção do repositório impuseram. Mantidas aqui para que este documento continue descrevendo o que existe.

| Onde | Este documento diz | O que foi implementado | Por quê |
|---|---|---|---|
| Arquivo de solução | `CausaClara.sln` | `CausaClara.slnx` | É o padrão do SDK .NET 10. Formato XML sem GUID, legível e que não gera conflito de merge. Manter o padrão do SDK fixado pelo RNF38 é a escolha de menor risco. |
| `IContextoDeDados` | `SaveChangesAsync`, `IniciarTransacaoAsync` | `SalvarAlteracoesAsync`, `IniciarTransacaoAsync` | A seção 3 do AGENTS.md exige português em método, sem exceção. `ContextoDeDados` faz a ponte para o `SaveChangesAsync` do EF numa linha. |
| Documentação da API | `/swagger/v1/swagger.json` | `/openapi/v1.json` e interface em `/scalar/v1` | O .NET 10 traz geração de OpenAPI nativa; o Swashbuckle deixou de vir no template. A interface interativa que o RNF31 pede fica por conta do Scalar, que lê o documento nativo. **O script de geração de tipos da seção 9 precisa apontar para o novo caminho.** |
| Raiz do backend | — | `Directory.Build.props` e `Directory.Packages.props` | Alvo, análise e versões de pacote num lugar só, em vez de repetidos nos sete `.csproj`. `TreatWarningsAsErrors` ligado. |
| Raiz do repositório | — | `.config/dotnet-tools.json` | `dotnet-ef` fixado como ferramenta local, para que a versão da ferramenta seja versionada junto com o código. Depois de clonar: `dotnet tool restore`. |
| Nomes no banco | `snake_case` por mapeamento | `EFCore.NamingConventions` | Traduz `NomeCompleto` em `nome_completo` por convenção. A alternativa seria uma centena de `HasColumnName`, que é superfície de erro de digitação sem ganho. |
| Migração inicial | — | Apenas `usuarios` e `instituicoes` | O modelo de dados definitivo ainda é pendência aberta. Gerar o esquema inteiro agora seria exatamente a migração errada que custa caro depois. Cada etapa acrescenta suas tabelas. |
| Aplicação de migrações | — | `AplicarMigracoesPendentesAsync()` chamada por configuração | Para que `docker compose up` entregue banco e esquema prontos (RNF33). Mora na Persistência, não em `Program.cs`, senão o EF Core vazaria para a Api e o teste de arquitetura reprovaria. |
| Locale do PostgreSQL | — | Sem `POSTGRES_INITDB_ARGS` | Fixar `pt_BR.UTF-8` no cluster depende de a imagem ter a locale gerada, o que não vale para a variante alpine nem para todo provedor. A ordenação em português importa em poucas colunas e será resolvida por collation ICU por coluna. |

### Decisão: os comprovantes ficam no PostgreSQL, em `bytea`

Lacuna encontrada na revisão de 29/10/2026 e fechada aqui. O projeto arquitetural não dizia onde o arquivo do RF42 é guardado, e essa omissão ameaçava o RF67 — que promete acesso público ao comprovante — de um jeito silencioso.

**O que elimina a opção óbvia.** Gravar no sistema de arquivos do contêiner não funciona: em tier gratuito o disco é efêmero, e o comprovante some na próxima publicação. O sistema continuaria subindo, a despesa continuaria listada, e só o link do comprovante quebraria. Como a prestação de contas é o objetivo do trabalho, comprovante que desaparece não é defeito de infraestrutura, é perda da tese.

**As duas alternativas reais.**

| | `bytea` no PostgreSQL | Objeto externo (R2, Supabase, B2) |
|---|---|---|
| Custo (RNF40) | Nenhum. Já existe banco. | Nenhum no papel, mais um tier gratuito para depender. |
| Atomicidade (RNF25) | Despesa e comprovante na mesma transação. | Duas escritas sem transação comum: ou sobra arquivo órfão, ou sobra linha apontando para arquivo que não existe. Exige rotina de reconciliação. |
| Integridade (RNF27) | Chave estrangeira de verdade. | Referência por texto, sem garantia do banco. |
| Backup | Um só, o do banco. | Dois, que precisam estar consistentes entre si. |
| Segredos (RNF18) | Nenhum a mais. | Chave de acesso do provedor. |
| `docker compose up` (RNF33) | Sem contêiner novo. | Exige um MinIO local para o ambiente ser fiel. |
| Crescimento | Infla o banco e seus backups. | Não infla. |

**Decisão: `bytea`.** O que torna isso seguro é o próprio RNF17: o limite de cinco megabytes por arquivo cria um teto previsível. O TOAST do PostgreSQL lida bem com valores desse tamanho, e o tier gratuito de 1 GB comporta alguns milhares de comprovantes — muito além do que o trabalho precisa demonstrar e do que uma organização pequena acumula em anos. Em troca, elimina-se uma classe inteira de falhas: arquivo órfão, referência quebrada, backup inconsistente e um provedor a mais que pode mudar de termos antes da defesa.

**Três condições que fazem parte da decisão:**

1. **Tabela separada.** Os bytes ficam em `comprovantes`, com relação de um para um com `despesas`, e não numa coluna de `despesas`. Assim nenhuma consulta de listagem carrega o conteúdo por engano — o erro clássico dessa abordagem é um `SELECT *` que arrasta megabytes para montar uma tabela de cinco linhas.
2. **Tipo verificado pelo conteúdo.** RNF17 exige verificar o tipo real, então a validação lê os bytes iniciais do arquivo. Extensão e cabeçalho enviados pelo cliente são declaração do cliente, não prova.
3. **A Aplicação fala com uma interface.** O caso de uso depende de `IArmazenamentoDeComprovantes`, não do banco. Se um dia o volume justificar objeto externo, troca-se a implementação na Persistência e nada mais. A decisão é reversível por construção — e é por isso que ela pode ser tomada agora, sem travar o projeto.

> A interface e a tabela **não** foram criadas nesta etapa. Criá-las agora seria abstração especulativa: elas nascem na etapa 7, junto com RF41 a RF44, que é quando existe despesa para anexar comprovante. O que está fechado é a decisão, não o código.

### Divergências e descobertas da etapa 3

| Onde | O que foi implementado | Por quê |
|---|---|---|
| Serialização de enum | Enum viaja como texto na API: `"Gestor"`, não `1` | O padrão do `System.Text.Json` é numérico, o que obrigaria o cliente a conhecer a ordem dos membros. Reordenar o enum mudaria o significado dos dados em trânsito **sem quebrar compilação nenhuma**. Texto também alinha a API ao banco, onde o enum já é texto com `CHECK`, e deixa o documento OpenAPI legível. Encontrado exercitando a API de verdade: o cadastro recusava `"Gestor"` com 400. |
| `TiposDeClaim` | Subiu da Api para a Aplicação | Dois lados precisam combinar — quem emite o token, na Infraestrutura, e quem o lê, na Api. Com a constante duplicada, divergir era questão de tempo. |
| Tolerância de relógio na validação | 30 segundos, em vez dos 5 minutos padrão | O padrão da biblioteca estenderia na prática o teto de validade do RNF13. |
| Projeto de teste | `CausaClara.Testes.Integracao` ganhou o fluxo de acesso sobre SQLite | Cadastro, sessão e redefinição são exercitados com as implementações **reais** de resumo de senha e de segredo; só relógio, e-mail e emissor de token são substituídos. |

### Divergências e descobertas da etapa 2

| Onde | Este documento diz | O que foi implementado | Por quê |
|---|---|---|---|
| Filtro global | `HasQueryFilter` escrito por entidade em `OnModelCreating` | Convenção que varre o modelo e aplica aos marcadores | Lista escrita à mão tem o mesmo defeito do `if` por endpoint: funciona até alguém esquecer uma linha. |
| Filtro global | `d.InstituicaoId == _contexto.InstituicaoId`, com `_contexto` sendo o serviço | A expressão referencia `InstituicaoDaRequisicao`, **membro do `DbContext`** | Ver abaixo — a forma do documento não funciona. |
| Exclusão lógica | Filtro próprio | Composto com o de instituição por `&&`, numa expressão só | Dois `HasQueryFilter` na mesma entidade substituem em vez de somar, e perder o isolamento em silêncio é o pior defeito possível aqui. |
| Projetos de teste | Três | Quatro: entrou `CausaClara.Testes.Aplicacao` | A Aplicação passou a ter lógica própria para testar — a normalização de paginação do RNF09. Pôr isso em "Integração" descreveria errado o que o teste é. |
| Teste de isolamento | "Entra quando houver entidade de instituição" | Já existe, com entidade de fixação em SQLite | O requisito de maior risco não precisa esperar a etapa 4 para ter teste. O que é específico do PostgreSQL continua verificado no compose. |

#### A armadilha do filtro global, que o projeto previa sem saber a forma exata

A seção 4 avisa que "o filtro deve ler o contexto no momento da consulta". O aviso está certo, mas a forma proposta — a expressão referenciar o serviço `IContextoDaRequisicao` — **não produz esse efeito**. O EF Core só transforma em parâmetro da consulta o acesso a um membro de instância do próprio `DbContext`; qualquer outro objeto capturado é tratado como subexpressão constante e avaliado **uma única vez**, na primeira compilação daquela consulta.

Na prática: a primeira requisição que compilasse a consulta congelaria sua instituição — ou, como aconteceu aqui, o `null` do momento em que o cenário foi montado — para todas as requisições seguintes. O sistema continuaria subindo e respondendo. As listagens é que viriam erradas, com dado de outra instituição ou com nada.

A correção é o contexto expor a propriedade e a convenção referenciá-la:

```csharp
internal int? InstituicaoDaRequisicao => contextoDaRequisicao.InstituicaoId;
```

O defeito foi encontrado pelo teste escrito justamente para a ressalva da seção 4, antes de qualquer entidade real depender do filtro. É o argumento prático a favor de construir os transversais na etapa 2, e não depois de quarenta endpoints.

### Decisão: e-mail por SMTP, com captura local no ambiente de desenvolvimento

Segunda lacuna da revisão de 29/10/2026, fechada antes da etapa 3. RF09 e RF24 dependem de enviar e-mail — redefinição de senha e aviso de decisão sobre vínculo —, e não havia serviço de e-mail em nenhum documento nem no compose.

**SMTP, e não a API HTTP de um provedor.** Os dois funcionam e custam zero no tier gratuito. A diferença é o que acontece quando o provedor muda de termos, de preço ou simplesmente sai do ar: com SMTP, trocar de fornecedor é trocar variáveis de ambiente; com a API de um fornecedor, é reescrever o adaptador. Num trabalho que precisa continuar funcionando até a defesa e depois dela, portabilidade vale mais do que os recursos extras que uma API proprietária oferece — nenhum deles é exigido por requisito.

**MailKit, e não `System.Net.Mail.SmtpClient`.** A própria Microsoft desaconselha o `SmtpClient` para código novo, por não acompanhar TLS e autenticação modernos, e aponta o MailKit como substituto. É mantido pela .NET Foundation.

**Captura local no compose, em vez de enviar de verdade em desenvolvimento.** Um contêiner de captura fala SMTP como um servidor real, mostra a caixa de entrada no navegador e não entrega nada para fora. Isso torna RF09 e RF24 demonstráveis sem cadastrar provedor, sem credencial no ambiente de desenvolvimento e sem risco de disparar mensagem para endereço de pessoa real durante um teste. Em produção o serviço não sobe: as mesmas variáveis `Email__*` apontam para o provedor.

**Onde o código mora: projeto novo `CausaClara.Infraestrutura`.** O documento previa quatro projetos, e enviar e-mail não cabe em nenhum deles — não é persistência, não é regra de negócio, e pôr um cliente SMTP na Api misturaria HTTP com adaptador externo. O projeto segue o mesmo padrão da Persistência: a Api o referencia só no composition root, para chamar `AdicionarInfraestrutura()`, e testes de arquitetura garantem que nem a Api nem a Aplicação nem o Domínio dependem de MailKit. A geração de PDF das etapas 9 e 10 é o próximo morador.

**Três detalhes que fazem parte da decisão:**

1. **A configuração é verificada na inicialização**, não no primeiro envio. Descobrir que o servidor nunca foi configurado no momento em que alguém pede redefinição de senha é tarde demais: o usuário fica esperando uma mensagem que nunca sai. As mensagens de erro nomeiam a variável de ambiente a corrigir.
2. **O envio acontece depois da gravação, nunca dentro da transação.** Servidor de e-mail fora do ar não pode desfazer a aprovação de um vínculo que já foi decidida. O contrato lança exceção e documenta, por requisito, quem deve propagar e quem deve apenas registrar — no RF09, em particular, a resposta ao usuário é sempre a mesma, tenha o endereço existido ou não, para não revelar quais e-mails estão cadastrados.
3. **O endereço vai mascarado para o log** (`f*****@exemplo.com`). Endereço de e-mail é dado pessoal (RNF19), e arquivo de log costuma viver fora dos controles de acesso do banco. O log precisa permitir diagnosticar um envio, não reconstruir a base de usuários.

> Os modelos de mensagem de RF09 e RF24 **não** foram escritos nesta etapa: eles nascem com os casos de uso que os enviam, nas etapas 3 e 4.

### Decisão: desenho de autenticação e autorização

Pendência aberta desde o projeto arquitetural, fechada na etapa 3.

**Resumo da senha: PBKDF2-HMAC-SHA512, 100 mil iterações, sal de 128 bits** (RNF10), pelo `PasswordHasher` do ASP.NET Core. Três razões para preferi-lo ao bcrypt e a implementações próprias: o formato é versionado, e a conferência avisa quando um resumo foi produzido com parâmetros antigos — o que permite **aumentar o custo do algoritmo com o tempo sem obrigar ninguém a trocar de senha**, já que o resumo é refeito no acesso seguinte; não há o truncamento em 72 bytes do bcrypt, no qual senha longa vira senha curta em silêncio; e vem num pacote pequeno da Microsoft, sem arrastar o Identity inteiro.

**Credencial de acesso: JWT com HMAC-SHA256, uma hora de validade.** Assinatura simétrica porque há um único emissor e um único verificador, o mesmo processo — par de chaves serve quando terceiros precisam validar sem poder emitir, o que não acontece aqui.

**O que entra no token:** identificador do usuário e tipo de conta, que são fatos imutáveis (RN01). O vínculo institucional também viaja nele, e isso tem uma consequência que precisa estar escrita: **revogar um vínculo só tem efeito pleno quando o token em uso expira**. Duas medidas limitam a janela — a vida curta da credencial, e o fato de que revogar um vínculo revoga também as sessões do usuário, impedindo a renovação. A alternativa, resolver o vínculo no banco a cada requisição, foi descartada porque o `DbContext` depende do contexto da requisição para montar o filtro global: fazer o contexto consultar o banco criaria uma dependência circular.

**Sessão: token de renovação opaco, guardado só como resumo, de uso único e rotativo.** Oito horas de validade absoluta, que é o teto do RNF13 — renovar emite credencial nova **dentro** do prazo original, nunca o estica. Isso dá três propriedades: encerrar sessão (RF08) é efetivo no servidor, e não apenas o cliente esquecendo o token; uma cópia vazada deixa de servir assim que o dono legítimo renova; e trocar a senha derruba as sessões abertas, que é o que alguém espera ao trocar a senha por suspeita de acesso indevido.

**Gestor sem vínculo** autentica normalmente e recebe token sem a *claim* de instituição. O filtro global não casa com nada, então ele enxerga zero lançamentos — a falha fecha. É também o estado que leva à tela de primeiro acesso (RF10), na etapa 4.

**Três decisões sobre o que não se revela:**

1. **A recusa de acesso não diz se o e-mail existe.** A mensagem é a mesma para endereço desconhecido e senha errada. Mais: quando o e-mail não existe, o servidor confere a senha contra um resumo de referência, para que o tempo de resposta não entregue a diferença.
2. **O pedido de redefinição responde a mesma coisa sempre**, tenha a conta existido ou não, e falha de envio também não vaza — fica no log. A rota é aberta; responder "e-mail não encontrado" a transformaria num verificador de quem tem conta.
3. **O endereço vai mascarado para o log** (RNF19), e o link de redefinição vale trinta minutos, de uso único, porque o segredo viaja por e-mail — canal fora do nosso controle.

> O vínculo institucional ainda não é resolvido: a entidade que o define nasce na etapa 4. Até lá o token sai sem a *claim* de instituição, e o ponto onde ela entra está marcado no código.

### O teste de arquitetura ficou mais forte do que o previsto

A seção 3 propõe verificar que `Api.Controllers` não depende de EF Core. Como `AdicionarPersistencia()` encapsula todo o registro, foi possível afirmar algo mais forte e igualmente verdadeiro: **nenhum tipo da Api inteira depende de EF Core ou de Npgsql**, nem fora dos controllers. São catorze testes, incluindo uma guarda contra aprovação por vacuidade — sem ela, apagar todos os controllers deixaria os testes de fronteira verdes por não terem o que verificar.
