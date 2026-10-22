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
{ "scripts": { "tipos": "openapi-typescript http://localhost:5000/swagger/v1/swagger.json -o frontend/src/servicos/api-tipos.ts" } }
```

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
