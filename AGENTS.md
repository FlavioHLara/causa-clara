# AGENTS.md — Causa Clara

Contexto para agentes de IA (Claude Code, Copilot, Cursor) trabalharem neste repositório com acurácia e sem retrabalho. **Leia este arquivo inteiro antes de propor qualquer coisa.** Ele responde a maior parte das perguntas de roteamento e registra decisões já fechadas, para que não sejam reabertas.

Não tome atalhos. Não faça concessões. Escolha a perfeição.
O desafio de resolver problemas de forma elegante é, por si só, a recompensa. O destino, sem a jornada, não tem valor.

Diante de um problema difícil:

1- Pare. Pergunte: qual é a maneira CORRETA de fazer isso?

2- Se a maneira correta for difícil, esse é justamente o ponto. Faça.

3- Se não for possível fazer da maneira correta, admita. Não finja.

---

## 1. O que é o projeto

**Causa Clara** — sistema de gestão administrativa e transparência financeira para organizações da sociedade civil.

Trabalho de Conclusão de Curso em Sistemas de Informação, PUC Minas campus Betim. Autor: Flávio Henrique Lara Santos. Orientadora: Juliana Santiago Teixeira.

O sistema resolve dois problemas acoplados do terceiro setor: a gestão administrativa e financeira interna, e a prestação de contas pública. Um gestor lança doações, despesas e movimentações de estoque; qualquer cidadão consulta o painel público de transparência daquela instituição, sem autenticação.

> **Causa Clara** — gestão administrativa e transparência financeira para organizações da sociedade civil

| Uso | Forma |
|---|---|
| Exibição, interface, artigo | **Causa Clara** |
| *Namespace* raiz e solução | `CausaClara` |
| Repositório, compose, pastas | `causa-clara` |

---

## 2. Estado atual

**Fase: arquitetura decidida, código ainda não iniciado.**

| Etapa | Situação |
|---|---|
| Requisitos | Fechados — RF01–RF81, RN01–RN16, RNF01–RNF40 |
| Diagramas de caso de uso e ER | Prontos |
| Protótipo de alta fidelidade no Figma | Pronto e navegável — 19 telas desktop, 19 móveis mais 3 gavetas de menu, 94 ligações, sem tela órfã |
| Projeto arquitetural | Decidido e documentado |
| Código — etapa 1, Fundação | **Concluída** — solução, quatro camadas, migração inicial, OpenAPI, compose verificado |
| Código — etapa 2, Transversais | **Concluída** — filtro global, auditoria, exclusão lógica, paginação |
| Código — etapa 3, Acesso | **Concluída** — servidor e as telas T01 e T02, verificadas no navegador contra a API real |
| Código — etapa 4 em diante | Não iniciado |

A próxima ação é a **etapa 4, Vínculo institucional** (RF10–RF24). Ela é o que destrava tudo que é institucional: sem vínculo aprovado não há contexto de instituição para o filtro global resolver.

### O que a etapa 1 entregou

- `backend/` com `CausaClara.slnx` e sete projetos: Dominio, Aplicacao, Persistencia, Api, mais Testes.Arquitetura, Testes.Dominio e Testes.Integracao
- `Directory.Build.props` e `Directory.Packages.props`: alvo, análise e versões de pacote centralizados, com `TreatWarningsAsErrors`
- Domínio de `Usuario` e `Instituicao` — só o que a migração inicial exige; nada de abstração especulativa
- `IContextoDeDados` na Aplicação; `ContextoDeDados` e mapeamentos na Persistência
- Migração `Inicial` gerada: `usuarios` e `instituicoes` em snake_case português, `timestamptz`, CHECK no enum, índices únicos de RF04 e RF13
- Api com OpenAPI, Scalar em desenvolvimento, tratadores de exceção em português e sondas `/saude/vivo` e `/saude/pronto`
- `frontend/` em React 19 + Vite 8 + TypeScript (RNF37)
- `docker-compose.yml` na raiz com banco, api e web (RNF33), verificado de ponta a ponta

### O que a etapa 2 entregou

- `IContextoDaRequisicao` lido das *claims*, implementado na Api (`Autenticacao/ContextoDaRequisicao.cs`)
- **Filtro global por convenção** (`Persistencia/Convencoes/FiltrosGlobais.cs`): varre o modelo e aplica isolamento por instituição e exclusão lógica numa expressão única, composta com `&&`
- **`InterceptadorDeAuditoria`**: grava a trilha do RF81 e converte exclusão em exclusão lógica (RN10), lendo o estado original antes de converter
- `RegistroDeAuditoria` e a tabela `auditoria`, com autor anulável para não brigar com o RNF21
- `ParametrosListagem` e `PaginaResposta<T>` com o teto de cem do RNF09 aplicado no servidor
- Testes de domínio, de aplicação e de integração cobrindo isolamento, auditoria e exclusão lógica

O teste de isolamento — gestor da instituição A pedindo registro da instituição B pelo identificador e recebendo *não encontrado* — roda contra SQLite em memória, com SQL de verdade, e não espera a etapa 4.

### O que a etapa 3 entregou, no servidor

Módulo `Acesso` nas quatro camadas, cobrindo RF02 a RF09:

| Rota | RF | Observação |
|---|---|---|
| `POST /api/contas` | RF02, RF03, RF04 | Aberta. Tipo de conta escolhido aqui não muda (RN01) |
| `GET /api/contas/eu` | RF07 | Exige token |
| `PUT /api/contas/eu` | RF07 | Nome e telefone; tipo não é parâmetro |
| `POST /api/sessao` | RF05 | Aberta |
| `POST /api/sessao/renovacao` | RF06 | Token de uso único, rotativo |
| `DELETE /api/sessao` | RF08 | Devolve 204 mesmo com token já inválido |
| `POST /api/senha/redefinicao` | RF09 | Devolve 202 sempre, exista ou não a conta |
| `PUT /api/senha` | RF09 | Derruba as sessões abertas do usuário |

**Pontos que não são óbvios e têm teste:**

- O resumo da senha é **refeito automaticamente** quando foi gerado com parâmetros antigos, no acesso seguinte. Permite endurecer o algoritmo sem forçar troca de senha.
- A recusa de acesso usa **a mesma mensagem** para e-mail inexistente e senha errada, e confere a senha contra um resumo de referência quando o e-mail não existe, para o tempo de resposta não entregar a diferença.
- Renovar **não estica** o teto de oito horas do RNF13: o token novo herda o prazo do original.
- Trocar a senha **revoga as sessões abertas**.

### O que a etapa 3 entregou, no cliente

`frontend/` deixou de ser scaffold. Estrutura conforme a seção 8 de `docs/05-arquitetura.md`:

- `estilos/tokens.css` — **gerado das variáveis do Figma**, com os nomes do *code syntax* (`var(--cor-primaria)`). Não edite valores à mão: ajuste no Figma e regenere
- `servicos/cliente.ts` — axios com interceptador que renova a sessão no 401 e repete a requisição. A renovação é serializada: três 401 simultâneos disparariam três renovações, e como o token é de uso único, duas falhariam e derrubariam quem estava usando o sistema
- `ui/` — Botão, Campo e Faixa, com foco visível, `aria-describedby` no erro e aviso por cor **mais** símbolo e texto (RNF07, RNF08)
- `modulos/acesso/` — T01, T02 e as duas telas do RF09, com validação `zod` espelhando as regras do servidor
- Rotas para telas ainda não construídas mostram aviso honesto, em vez de link quebrado

> ⚠️ **As duas telas do RF09 não existem no protótipo.** T01 prevê o link "Esqueci minha senha", mas o inventário de telas não tem a tela que ele abre nem a de escolher a nova senha. Foram construídas a partir do requisito e dos tokens do design. Quando o protótipo for atualizado ao final, elas precisam entrar — e a contagem de telas do artigo muda.

**Pilha adotada:** react-router, axios, react-hook-form e zod — a documentada na arquitetura. TanStack Query **não** entrou: ela se paga com tabelas paginadas e invalidação após gravação, que chegam na etapa 6. Entra lá, com justificativa, ou não entra.

> ⚠️ **A sessão fica em `localStorage`**, o que significa que um script injetado na página a alcança. As defesas que existem são do servidor: token de acesso de uma hora, token de renovação de uso único e revogação de verdade ao sair. Trocar por cookie `HttpOnly` é decisão de servidor — está na seção 12.

### Envio de e-mail, decidido antes da etapa 3

- `IServicoDeEmail` e `MensagemDeEmail` na Aplicação; adaptador SMTP em `CausaClara.Infraestrutura`
- Configuração só por `Email__*`, verificada na **inicialização** da API, não no primeiro envio
- Contêiner de captura no compose: caixa de entrada em http://localhost:8025, nada sai para a internet
- **147 testes passando** e 1 saltado: o envio real contra servidor SMTP roda só com `CAUSACLARA_SMTP_DE_TESTE` definida, com o compose de pé

```bash
docker compose up -d
$env:CAUSACLARA_SMTP_DE_TESTE = "localhost:1025"; dotnet test backend/CausaClara.slnx
```

### Comandos do dia a dia

```bash
dotnet tool restore              # depois de clonar, para ter o dotnet-ef
cp .env.example .env
docker compose up                # banco + api + web (RNF33)
npm run verificar                # testes do backend e build do frontend
```

---

## 3. Regra que mais economiza idas e voltas: tudo em português

**Sempre**, sem exceção:

- Classes, métodos, variáveis, pastas, arquivos, rotas, *props* — `Doacao`, `RegistrarMovimentacao`, `PainelGestor`, `modulos/estoque/`
- Tabelas e colunas em `snake_case` português — `itens_estoque`, `qtd_atual`, `criado_em`
- Mensagens ao usuário em português, no formato `{ mensagem = "..." }`
- Comentários e *commits* em português (*commits* no estilo Conventional: `feat:`, `fix:`, `docs:`)

Não "anglicize" nada. O vocabulário do domínio é português — doação, prestação de contas, vínculo, apoiador — e traduzir perde precisão.

---

## 4. Stack

| Camada | Tecnologia | Requisito |
|---|---|---|
| Front | React + Vite + TypeScript | RNF37 |
| Back | C# / .NET 10, ASP.NET Core | RNF38 |
| Banco | PostgreSQL | RNF39 |
| Infra local | Docker Compose, um comando | RNF33 |
| Publicação | Tier gratuito (Render ou similar) | RNF40 |

Ambiente verificado nesta máquina: .NET SDK 10.0.201, Node 22.16, npm 10.9, Docker 29.1 com Compose v5, Git 2.49.

**Não sugira trocar de stack nem acrescentar dependência pesada** sem justificativa forte. As escolhas são restrições formais do trabalho (RNF37–RNF40), não preferências.

---

## 5. Mapa do repositório

```
causa-clara/
├─ AGENTS.md              este arquivo
├─ README.md
├─ docs/                  artefatos do TCC — fonte da verdade conceitual
│  ├─ 01-requisitos.md    RF01–RF81, RN01–RN16, RNF01–RNF40
│  ├─ 02-diagramas.html   casos de uso e ER, com PlantUML/Mermaid/DBML embutidos
│  ├─ 03-prototipo.md     inventário de telas, tokens, componentes
│  ├─ 04-estudo-de-cor.html   contrastes WCAG calculados, justificativa da paleta
│  ├─ 05-arquitetura.md   PROJETO ARQUITETURAL — leia antes de codar
│  └─ recursos/
│     ├─ diagramas.drawio
│     ├─ figma-estado.json      IDs de tudo que existe no Figma
│     └─ figma-scripts/         scripts de construção do protótipo + linter
├─ backend/
│  ├─ CausaClara.slnx           solução (.slnx é o padrão do SDK 10)
│  ├─ Directory.Build.props     alvo, análise, TreatWarningsAsErrors
│  ├─ Directory.Packages.props  versões de pacote centralizadas
│  ├─ Dockerfile
│  ├─ src/
│  │  ├─ CausaClara.Dominio/       zero dependências, por construção
│  │  ├─ CausaClara.Aplicacao/     casos de uso e IContextoDeDados
│  │  ├─ CausaClara.Persistencia/  EF Core, mapeamentos, Migrations/
│  │  ├─ CausaClara.Infraestrutura/ adaptadores externos: e-mail, depois PDF
│  │  └─ CausaClara.Api/           controllers, OpenAPI, tratadores de erro
│  └─ tests/
│     ├─ CausaClara.Testes.Arquitetura/    evidência do RNF32
│     ├─ CausaClara.Testes.Dominio/        invariantes RN
│     ├─ CausaClara.Testes.Aplicacao/      paginação e casos de uso
│     ├─ CausaClara.Testes.Infraestrutura/ montagem de e-mail e configuração
│     └─ CausaClara.Testes.Integracao/     composição, rotas, isolamento e auditoria
├─ frontend/              React 19 + Vite 8 + TypeScript (RNF37)
├─ .config/
│  └─ dotnet-tools.json   dotnet-ef fixado; rode `dotnet tool restore`
├─ docker-compose.yml     banco + api + web, um comando (RNF33)
├─ .env.example           modelo de segredos (RNF18)
├─ .editorconfig
└─ package.json           scripts de orquestração
```

### Onde achar o quê, sem fazer busca

| Pergunta | Arquivo |
|---|---|
| O que o sistema deve fazer | `docs/01-requisitos.md` |
| Por que a arquitetura é assim | `docs/05-arquitetura.md` |
| Como as telas são e o que cada uma atende | `docs/03-prototipo.md` |
| Quais entidades existem e como se relacionam | `docs/02-diagramas.html`, seções 5 e 6 |
| Cores, tipografia, espaçamento | `docs/03-prototipo.md` seção 4, e `docs/04-estudo-de-cor.html` |
| IDs de variáveis e componentes do Figma | `docs/recursos/figma-estado.json` |

---

## 6. Arquitetura — decisões fechadas

Detalhe completo em `docs/05-arquitetura.md`. Resumo do que não se discute mais:

### Quatro camadas, dependência em sentido único

```
Api ──► Aplicacao ──► Dominio
             ▲            ▲
       Persistencia ──────┘
```

- **Dominio** — entidades, enums, invariantes (RN01–RN16). **Zero dependências**, nem EF Core, nem ASP.NET.
- **Aplicacao** — casos de uso, DTOs, contrato de persistência (`IContextoDeDados`), validação.
- **Persistencia** — EF Core: contexto, mapeamentos, migrações, interceptadores.
- **Api** — controllers, autenticação, middleware. Referencia Persistencia **apenas** no *composition root*, para registrar DI.

Projetos separados, não pastas: pasta depende de disciplina, projeto depende do compilador.

### Três mecanismos transversais — construir ANTES dos casos de uso

Esta é a armadilha de sequência mais cara do projeto. Acrescentar esses mecanismos depois de existirem quarenta endpoints significa revisar os quarenta e torcer para não ter esquecido nenhum.

**1. Isolamento por instituição (RNF12, RN05).** Filtro global de consulta no EF Core sobre toda entidade que implemente `IPertenceAInstituicao`, alimentado por um `IContextoDaRequisicao` resolvido das *claims* do token. Consequência: identificador forjado devolve *não encontrado*, não dado alheio. A falha de IDOR fecha por construção, não por lembrar de um `if`.

> **Construído na etapa 2**, por convenção que varre o modelo: `Persistencia/Convencoes/FiltrosGlobais.cs`. Basta a entidade implementar o marcador — não há lista para manter.

> ⚠️ **Armadilha do EF Core que custou caro e tem teste de regressão.** O filtro só é reavaliado a cada consulta se a expressão referenciar um **membro de instância do `DbContext`**. Referenciar o serviço `IContextoDaRequisicao` diretamente compila, roda e **congela a primeira instituição que passar pelo modelo** — ou um nulo — para todas as requisições seguintes, porque o EF trata o objeto capturado como subexpressão constante e a avalia uma única vez. Por isso o contexto expõe `InstituicaoDaRequisicao` e a convenção referencia essa propriedade. O teste `O_filtro_le_a_instituicao_no_momento_da_consulta` existe para impedir que isso volte.

> A segunda ressalva segue valendo: o caminho público de transparência **não usa o filtro** — ele chama `IgnoreQueryFilters()` e filtra pelo `instituicaoId` da rota, em serviço separado, projetando só o que RNF20 e RNF23 autorizam.

**2. Transação do estoque (RNF26, RN11–RN14).** Movimentação, atualização de saldo e eventual alerta numa única transação. A regra mora na entidade `ItemEstoque.Movimentar()`, não no controller. Concorrência por token otimista usando a coluna de sistema `xmin` do PostgreSQL — evita saldo inconsistente em saídas simultâneas sem acrescentar coluna ao esquema.

**3. Trilha de auditoria (RF81, RN10).** `SaveChangesInterceptor` que inspeciona o `ChangeTracker` e grava em `auditoria`. O mesmo interceptador converte `Deleted` em exclusão lógica para tipos `IExcluivelLogicamente`.

### Sem repositório por entidade

O `DbContext` já é *Unit of Work* e *Repository*. A Aplicação conversa com a interface `IContextoDeDados`, que expõe os `DbSet` e o `SaveChangesAsync`. Quinze classes repassadoras acrescentariam código sem acrescentar capacidade. A fronteira que importa é **apresentação ↛ persistência**, e ela se mantém.

### Persistência

| Decisão | Razão |
|---|---|
| `numeric(14,2)` no banco, `decimal` em C# | RNF24 — ponto flutuante binário erra centavo em soma |
| `timestamptz`, sempre UTC | RNF28 — conversão de fuso só na apresentação |
| Enums como texto + `CHECK` | Legibilidade no banco e integridade independente da aplicação |
| **EF Core Migrations** | RNF35 exige versionamento incremental. **Não** usar `init.sql` escrito à mão |
| Índice em `(instituicao_id, data)` nos lançamentos | Toda consulta filtra por instituição e período |

### Teste de arquitetura

Há um teste com NetArchTest afirmando que `Api.Controllers` não depende de `Microsoft.EntityFrameworkCore`, `Npgsql` nem de `CausaClara.Persistencia`. Ele é a **evidência verificável do RNF32** — mais honesto do que afirmar a separação em prosa.

> Já foi validado: com os seis testes passando, bastou plantar um controller injetando `DbContext` para o teste reprovar, e removê-lo para voltar ao verde. O teste pega de verdade.

---

## 7. Ordem de construção

Detalhada na seção 14 de `docs/05-arquitetura.md`.

| # | Etapa | RF |
|---|---|---|
| 1 | Fundação — solução, compose, migração inicial, OpenAPI, teste de arquitetura | — |
| 2 | **Transversais** — contexto, filtro global, auditoria, exclusão lógica, paginação | RNF12, RNF32, RF81 |
| 3 | Acesso — cadastro com dois tipos de conta, autenticação | RF01–RF09 |
| 4 | Vínculo institucional | RF10–RF24 |
| 5 | Cadastro e consulta pública | RF25–RF27 |
| 6 | Doações | RF28–RF40 |
| 7 | Despesas | RF41–RF46 |
| 8 | Estoque — **a mais longa e arriscada** | RF47–RF62 |
| 9 | Transparência pública | RF63–RF73 |
| 10 | Painéis e prestação de contas | RF74–RF81 |

**Construa em fatias verticais**, do banco à tela, um módulo por vez. "Todo o backend e depois todo o frontend" deixa meses sem nada demonstrável e empurra a integração para quando o prazo aperta.

---

## 8. Decisões de produto já fechadas — não reabrir

**Nome.** "Causa Clara" foi escolhido depois de nove candidatos testados e descartados por colisão: *GT* e *TG* (Grupo de Trabalho; NBC TG é norma contábil do próprio setor), *GAT* e *3S* (vizinhança com marcas de apostas), *GTF* e *TPC* (empresas brasileiras ativas), *Ágora Social* e *Social Lumen* (domínio ou marca ocupados), *Stoa* (insurtech e rede da USP). Conclusão da busca: **sigla curta de duas ou três letras no vocabulário administrativo brasileiro está saturada**. Não proponha siglas novas.

**Doação é declaratória.** O sistema registra e publica a doação informada, **sem intermediar transferência de recursos**. Não há *gateway* de pagamento no escopo. A intermediação exigiria conformidade com normas do setor de pagamentos e deslocaria o foco da transparência para a transação.

**Fora de escopo:** processamento de pagamentos, gestão de vagas de voluntariado e candidaturas, emissão de documentos fiscais, integração com sistemas contábeis externos, bens imóveis e depreciação, aplicativo móvel nativo.

**Gestor Dono generaliza Gestor Funcionário**, sem ator abstrato intermediário. O Dono faz tudo que o Funcionário faz e acrescenta quatro casos de uso. UC16 (criar instituição) e UC17 (solicitar acesso) pertencem ao ator que o executor **passa a ser** ao concluí-los; a restrição de não ter vínculo é pré-condição, não ator separado.

**Paleta roxa, opção B.** `roxo/300 #8B86E8` é a cor de ação, com **texto escuro** (5,65:1). Laranja foi descartado por colidir com a cor semântica de atenção `#B4530B`, usada em pendência e despesas. Branco sobre o roxo claro dá 3,15:1 e reprova no AA. Contrastes calculados em `docs/04-estudo-de-cor.html`.

---

## 9. Protótipo no Figma

- **Arquivo:** `stQ02xCuTeByDjDGdAT574` — https://www.figma.com/design/stQ02xCuTeByDjDGdAT574
- **Plano:** Professional, assento Full. 200 chamadas MCP/dia, páginas ilimitadas.
- **Estado completo com todos os IDs:** `docs/recursos/figma-estado.json` — ler antes de mexer, evita recriar o que já existe.
- **Páginas:** Capa · Componentes · Telas · Desktop · Telas · Móvel

A marca **Causa Clara** já está aplicada (21/10/2026). *Casa de Apoio Semear* permanece como instituição fictícia nos dados de exemplo — é dado de amostra, não marca, e **não deve ser trocada**.

### O protótipo está fechado para esta fase

O protótipo cumpre o que precisava cumprir: 19 telas desktop, 19 móveis, 3 gavetas de menu, 94 ligações, nenhuma tela órfã, todo perfil com entrada própria no navegável. **Não abra frente de retrabalho nele agora.**

Questões de usabilidade que sobraram — o item de menu de Categorias, a profundidade da navegação móvel, o que merece atalho — **serão decididas durante a implementação, com a tela funcionando**, e aplicadas ao Figma ao final, de uma vez. O estático dá menos informação que o sistema rodando; adiantar essas decisões no protótipo seria palpite caro.

Vale para o agente: **não proponha ajustes de interface ao protótipo** nem trate o que está em "Revisitar na implementação" (seção 12) como pendência bloqueante. Se algo da interface incomodar enquanto o código anda, anote na seção 12 e siga.

> `figma.root.name` não é gravável: a API responde "Setting the document name is currently not supported". Renomear o arquivo só pela interface do Figma.

### Armadilhas da API do Figma que já custaram chamadas

1. **`resize()` trava os dois eixos em FIXED.** Sempre restaurar `primaryAxisSizingMode='AUTO'` (vertical) ou `counterAxisSizingMode='AUTO'` (horizontal) depois. Sem isso o contêiner colapsa em 10px e a tela sai praticamente vazia.
2. **`counterAxisAlignItems`** aceita MIN/MAX/CENTER/BASELINE. `END` quebra.
3. **`createNodeFromSvg` ignora `stroke-dasharray` em `<circle>`** — rosca precisa de setores anulares por caminho de arco. Em `polyline` e `path` funciona normalmente.
4. Erro em qualquer ponto do script **reverte a transação inteira**. Scripts grandes são seguros para repetir: ou aplicam tudo, ou nada.
5. Conferir cada lote por captura de tela. Os defeitos 1 e 3 só apareceram assim.

Os scripts validados e um *linter* para essas armadilhas estão em `docs/recursos/figma-scripts/`. Pré-validar localmente derrubou a construção de ~10 chamadas para 4.

---

## 10. Contexto acadêmico

- O artigo é a fonte autoritativa dos requisitos. **A numeração em `docs/01-requisitos.md` precisa espelhar a do artigo**: RF01–RF81, RN01–RN16, RNF01–RNF40. O documento de arquitetura cita esses números.
- O regulamento (Art. 3º §2º) permite reaproveitar **textos e projetos de pesquisa** de disciplinas anteriores, exigindo declaração após a Conclusão. **Não** trata de código-fonte.
- Decidiu-se **começar o código do zero**, sem herdar do projeto interdisciplinar anterior (OngConecta), para evitar discussão sobre reaproveitamento e para nascer já nas quatro camadas. O projeto anterior pode ser consultado como referência, nunca copiado.
- Seções do artigo ainda a escrever: **5.4 Interface do Sistema**, **6 Resultados**, **7 Considerações Finais**.
- O RNF03 (escala SUS ≥ 68) pode ser avaliado **com o protótipo navegável**, sem esperar a implementação.

---

## 11. O que não fazer

| Evitar | Por quê |
|---|---|
| Microsserviços | Um domínio, um autor, um banco. Acrescenta rede e orquestração sem resolver problema existente. |
| CQRS, MediatR, *event sourcing* | Se paga quando leitura e escrita escalam separadamente. Aqui, não. |
| Repositório por entidade | O `DbContext` já cumpre *Unit of Work* e *Repository*. |
| Nx, Turborepo | Resolvem grafos de dezenas de pacotes. Há um pacote JS. |
| Cache distribuído, fila de mensagens | Não há operação assíncrona no escopo. |
| `init.sql` escrito à mão | RNF35 exige migrações incrementais. |
| Verificar instituição endpoint a endpoint | Use o filtro global. Quarenta `if` é quarenta chances de esquecer um. |
| Entidade de domínio em assinatura de controller | Controller trafega DTO. Expor entidade vaza modelo e abre *mass assignment*. |
| Perseguir cobertura alta de testes | Três conjuntos pequenos bastam: arquitetura, regras de domínio, e um de isolamento. |

---

## 12. Pendências abertas

### Lacunas que a revisão arquitetural de 29/10/2026 encontrou

Estas não estavam no projeto arquitetural e **precisam de decisão antes das etapas que indicam**. Nenhuma bloqueia a etapa 2.

- [x] ~~**Onde ficam os comprovantes (RF42, RNF17).**~~ **Fechada em 29/10/2026: `bytea` no PostgreSQL**, em tabela `comprovantes` separada de `despesas`, com o tipo verificado pelos bytes iniciais e a Aplicação falando com `IArmazenamentoDeComprovantes`. Razão curta: disco de contêiner é efêmero e quebraria o RF67 em silêncio; objeto externo não tem transação comum com a despesa, o que produz arquivo órfão ou referência quebrada. O teto de 5 MB do RNF17 é o que torna o `bytea` seguro. Justificativa completa na seção 16 de `docs/05-arquitetura.md`. A interface e a tabela nascem na etapa 7, não antes.
- [x] ~~**Entrega de e-mail (RF09, RF24).**~~ **Fechada em 29/10/2026: SMTP via MailKit**, configurado só por variáveis `Email__*`, com contêiner de captura no ambiente local e provedor real em produção pelas mesmas variáveis. O adaptador mora no projeto novo `CausaClara.Infraestrutura`; nem a Api, nem a Aplicação, nem o Domínio dependem de MailKit, e há teste de arquitetura para cada um. SMTP em vez da API HTTP de um fornecedor porque trocar de provedor passa a ser trocar variáveis, não reescrever código. Justificativa completa na seção 16 de `docs/05-arquitetura.md`. Os modelos de mensagem nascem com os casos de uso, nas etapas 3 e 4.
- [ ] **Direito à exclusão (RNF21) × trilha de auditoria (RN10, RF81) — decidir antes da etapa 3.** O titular pode pedir exclusão dos dados pessoais, preservados os lançamentos em forma anonimizada. A trilha de auditoria registra o autor de cada operação. Anonimizar um gestor que assinou cem lançamentos exige decidir o que fica: chave estrangeira anulada com nome congelado no registro, ou marca de anonimização no usuário. É decisão de modelo de dados, e migração errada aqui custa caro.
- [ ] **Percentual aplicado em atividades-fim (RF73) — decidir antes da etapa 9.** O painel público promete esse percentual, o que exige classificar cada categoria de despesa como atividade-fim ou atividade-meio. `Categoria` precisa desse atributo, e ele não está em nenhum documento.
- [x] ~~**Dois filtros globais na mesma entidade.**~~ **Fechada na etapa 2:** a convenção compõe as condições numa expressão única com `&&`, o que não depende de versão do EF Core nem de filtro nomeado e não tem como regredir para "o segundo filtro substitui o primeiro". Coberto pelo teste `Os_dois_filtros_valem_ao_mesmo_tempo_na_mesma_entidade`.
- [ ] **RF56 cruza a fronteira de módulos.** Converter doação material em entrada de estoque toca `Doacoes` e `Estoque`. Decidir de qual módulo é o caso de uso antes da etapa 8, para não duplicar regra.

### Pendências anteriores

- [ ] Modelo de dados definitivo — campo a campo, com tipos, nulabilidade, índices e *constraints*. A etapa 1 fechou `usuarios` e `instituicoes`; o resto segue aberto. É o artefato que mais economiza retrabalho; migração errada custa caro depois.
- [ ] Confirmar a obrigatoriedade dos campos de RF11. A etapa 1 adotou a regra **opcional somente onde um requisito diz opcional**, então site e telefone da instituição nasceram obrigatórios. Exigir site de uma organização pequena é restritivo; mudar isso é mudar o requisito, não o código.
- [ ] Contrato da API — endpoints × RF × regra de autorização.
- [x] ~~Desenho de autenticação e autorização.~~ **Fechado na etapa 3.** Senha por PBKDF2-HMAC-SHA512 com sal individual e refazimento automático do resumo quando os parâmetros envelhecem (RNF10). Acesso por JWT HMAC-SHA256 de uma hora, carregando usuário e tipo de conta. Sessão por token opaco de uso único e rotativo, guardado só como resumo, com oito horas de teto absoluto (RNF13) — renovar não estica o prazo. Gestor sem vínculo recebe token sem a *claim* de instituição e enxerga zero lançamentos. Justificativa completa, inclusive o que **não** se revela nas mensagens de erro, na seção 16 de `docs/05-arquitetura.md`.
- [ ] **Onde guardar a sessão no navegador.** Hoje é `localStorage`, exposto a script injetado. A alternativa é a API emitir e ler cookie `HttpOnly` com `SameSite`, o que muda o contrato dos endpoints de sessão e traz proteção contra CSRF para a conta. Decidir antes de o sistema ir ao ar, não antes da etapa 4.
- [ ] **O papel de vínculo ainda não entra no token** — `TiposDeClaim` tem usuário, tipo de conta e instituição; Dono e Funcionário entram na etapa 4, junto da entidade que os define.
- [ ] Renomear o arquivo no Figma pela interface (a API não permite).
- [ ] Decidir se haverá CI (`.github/workflows`) rodando build e testes. Não é exigência de nenhum RNF, mas dá evidência para o artigo.

### Revisitar na implementação — não bloqueiam o código

Decisão de 22/10/2026: estes pontos se avaliam melhor com a aplicação rodando, e os ajustes voltam ao Figma de uma vez, ao final. Ver seção 9.

- [ ] **O protótipo não tem as telas do RF09.** T01 prevê o link "Esqueci minha senha" e o inventário não tem nem a tela que pede o link nem a de escolher a nova senha. As duas já existem no código, construídas a partir do requisito; falta desenhá-las no Figma e acertar a contagem de telas do artigo.
- [ ] **Categorias sem item de menu.** T19 atende RF45–RF47 mas não está na barra lateral; no protótipo é alcançada pelo seletor de categoria de T12. Escolher entre item próprio no menu ou aba dentro de T18 Cadastro institucional.
- [ ] **Profundidade da navegação móvel.** As gavetas M20–M22 resolvem o alcance, mas custam um toque a mais em toda troca de seção. Ver se alguma seção merece atalho fixo.
- [ ] **`docs/recursos/figma-scripts/lint.mjs` não roda:** lê `../figma-prototipo-estado.json`, nome do repositório anterior; aqui o arquivo é `figma-estado.json`. O conjunto de IDs conhecidos também não inclui `telasMovel`, o que daria falso positivo nas telas móveis. Duas linhas, sem pressa — o protótipo está fechado.

---

## 13. Antes de propor mudança

- [ ] Nomes em **português** — arquivo, classe, método, rota, coluna.
- [ ] Mensagens de retorno em português, no formato `{ mensagem = "..." }` (RNF05).
- [ ] A camada certa: regra de negócio no Domínio, orquestração na Aplicação, EF na Persistência, HTTP na Api.
- [ ] Entidade de instituição implementa `IPertenceAInstituicao`? Então o filtro global já cuida — não escreva verificação manual.
- [ ] Valor monetário é `decimal`, data é UTC.
- [ ] Endpoint que muda estado tem autorização por tipo de conta **e** papel de vínculo (RNF11).
- [ ] Listagem é paginada no servidor, teto de cem (RNF09).
- [ ] Se afeta requisito documentado, `docs/01-requisitos.md` foi conferido.
- [ ] `dotnet build` e `dotnet test` passam, incluindo o teste de arquitetura.

---

## 14. Notas de ambiente

`dotnet remove <projeto> package <pacote>` falhou nesta máquina com "não foi possível encontrar nenhum projeto", mesmo com o caminho do `.csproj`. Editar o `.csproj` direto funciona. Possivelmente relacionado ao acento no caminho do usuário (`C:\Users\Flávio\`).

**`npm create vite` ignora `--template` quando recebe `--yes`**, e entrega o modelo *vanilla* sem React — o que violaria o RNF37 sem erro nenhum. A forma que funciona é `npx --yes create-vite@latest frontend --template react-ts`. Conferir se `react` está em `dependencies` antes de seguir.

**`launchSettings.json` carrega os valores de desenvolvimento**, para que `npm run api` funcione sem preparo: ele aponta para o banco e para a captura de e-mail que o compose publica, com a porta 5000, a mesma do compose e a que o proxy do Vite procura. O arquivo **não vai para a publicação**, então nada ali chega a produção — mas também não ponha valor real nele, e **não escreva comentário**: `dotnet run` não aceita JSON comentado.

**A porta 5432 costuma estar ocupada nesta máquina** por um PostgreSQL em WSL. Por isso o compose publica o banco em `5433` por padrão; dentro da rede do compose a API continua falando na 5432. Se `docker compose up` falhar com "port is already allocated", é isso.

**O `backend/Dockerfile` copia `src/` inteiro antes de publicar, de propósito.** A versão anterior listava cada `.csproj` para reaproveitar a camada de restauração; acrescentar um projeto à solução passou a quebrar a imagem com `NETSDK1004: Assets file not found`, erro que não aponta para a causa e que só aparece na hora de subir o ambiente. Lista que precisa ser lembrada é lista que será esquecida — não reintroduza.

**A API avisa que guarda chaves de proteção de dados num diretório não persistido.** É esperado e inofensivo hoje: nada no projeto usa o *Data Protection* do ASP.NET Core — a credencial de acesso é JWT assinado com a chave de `Token__Chave`, e o token de sessão é segredo opaco. O aviso passa a importar se alguma coisa vier a usar *antiforgery* ou autenticação por *cookie*; até lá, montar volume para chaves que ninguém usa seria ritual.

**Na primeira subida com banco vazio, a API registra uma linha `fail:` do EF Core** dizendo que `__historico_de_migracoes` não existe. É esperado: o EF consulta a tabela de histórico antes de criá-la. Só acontece com banco virgem e não indica problema.

**O log do contêiner do banco avisa `no usable system locales were found`.** Também é esperado: a imagem alpine não traz locales geradas, e o compose não força nenhuma de propósito — ver a seção 16 de `docs/05-arquitetura.md`.
