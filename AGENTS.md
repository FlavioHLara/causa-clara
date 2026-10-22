# AGENTS.md — Causa Clara

Contexto para agentes de IA (Claude Code, Copilot, Cursor) trabalharem neste repositório com acurácia e sem retrabalho. **Leia este arquivo inteiro antes de propor qualquer coisa.** Ele responde a maior parte das perguntas de roteamento e registra decisões já fechadas, para que não sejam reabertas.

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
| Protótipo de alta fidelidade no Figma | Pronto — 19 telas desktop, 5 móveis, 34 ligações |
| Projeto arquitetural | Decidido e documentado |
| Código | **Não iniciado** |

Não há código neste repositório ainda. A próxima ação é a etapa 1 da ordem de construção (seção 14 de `docs/05-arquitetura.md`).

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
├─ backend/               (a criar)
└─ frontend/              (a criar)
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

> Duas ressalvas: o filtro deve ler o contexto **no momento da consulta** (mesmo escopo do DbContext); e o caminho público de transparência **não usa o filtro** — ele chama `IgnoreQueryFilters()` e filtra pelo `instituicaoId` da rota, em serviço separado, projetando só o que RNF20 e RNF23 autorizam.

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

- [ ] Modelo de dados definitivo — campo a campo, com tipos, nulabilidade, índices e *constraints*. É o artefato que mais economiza retrabalho; migração errada custa caro depois.
- [ ] Contrato da API — endpoints × RF × regra de autorização.
- [ ] Desenho de autenticação e autorização — o que entra no token, como o papel é resolvido, o que acontece com gestor sem vínculo.
- [ ] Renomear o arquivo no Figma pela interface (a API não permite).
- [ ] Decidir se haverá CI (`.github/workflows`) rodando build e testes. Não é exigência de nenhum RNF, mas dá evidência para o artigo.

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

## 14. Nota de ambiente

`dotnet remove <projeto> package <pacote>` falhou nesta máquina com "não foi possível encontrar nenhum projeto", mesmo com o caminho do `.csproj`. Editar o `.csproj` direto funciona. Possivelmente relacionado ao acento no caminho do usuário (`C:\Users\Flávio\`).
