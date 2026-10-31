# Causa Clara

**Gestão administrativa e transparência financeira para organizações da sociedade civil.**

Trabalho de Conclusão de Curso em Sistemas de Informação — PUC Minas, campus Betim.
Autor: Flávio Henrique Lara Santos · Orientadora: Juliana Santiago Teixeira

---

## O problema

Organizações do terceiro setor administram recursos de terceiros e precisam demonstrar como os aplicam. Na prática, a gestão costuma acontecer em planilhas isoladas e a prestação de contas vira esforço manual no fim do período — quando acontece. A literatura mostra o padrão: entidades divulgam missão e atividades com facilidade, mas desempenho financeiro muito menos.

A Causa Clara acopla as duas pontas. O gestor registra doações, despesas e movimentações de estoque no dia a dia; o painel público de transparência é alimentado por esses mesmos lançamentos, sem trabalho adicional de divulgação. Quem quiser conferir as contas de uma instituição não precisa de conta nem de permissão.

## O que o sistema faz

- **Vínculo institucional** — um gestor cria sua instituição ou solicita acesso a uma existente; o dono aprova ou rejeita
- **Doações** — apoiadores registram contribuições financeiras ou materiais, com opção de anonimato na divulgação
- **Despesas** — lançamento com anexo de comprovante e vínculo à doação que custeou o gasto
- **Estoque** — itens, movimentações, alerta automático de quantidade crítica e valorização patrimonial
- **Transparência pública** — painel sem autenticação com indicadores, gráficos de evolução e composição, e exportação em CSV e PDF
- **Prestação de contas** — demonstrativo consolidado do período e trilha de auditoria

## Documentação

| Documento | Conteúdo |
|---|---|
| [docs/01-requisitos.md](docs/01-requisitos.md) | 81 requisitos funcionais, 16 regras de negócio, 40 não funcionais |
| [docs/02-diagramas.html](docs/02-diagramas.html) | Casos de uso e entidade-relacionamento, com fontes PlantUML, Mermaid e DBML |
| [docs/03-prototipo.md](docs/03-prototipo.md) | Inventário de telas, tokens de design e catálogo de componentes |
| [docs/04-estudo-de-cor.html](docs/04-estudo-de-cor.html) | Contrastes calculados pela WCAG e justificativa da paleta |
| [docs/05-arquitetura.md](docs/05-arquitetura.md) | Projeto arquitetural e ordem de construção |

Protótipo de alta fidelidade no Figma: 19 telas desktop, 19 móveis mais 3 gavetas de menu, e 94 ligações de navegação. Todo perfil — visitante, apoiador, gestor sem vínculo e gestor com vínculo — tem ponto de entrada próprio no navegável.

## Stack

React · Vite · TypeScript · C# · .NET 10 · ASP.NET Core · PostgreSQL · Docker

## Como executar

Requer Docker. O ambiente completo — banco, API e cliente — sobe com um comando:

```bash
cp .env.example .env
docker compose up
```

| Serviço | Endereço |
|---|---|
| Cliente | http://localhost:5173 |
| API | http://localhost:5000 |
| Documentação interativa da API | http://localhost:5000/scalar/v1 |
| Documento OpenAPI | http://localhost:5000/openapi/v1.json |
| Caixa de entrada do ambiente local | http://localhost:8025 |

Toda mensagem que a API enviar em desenvolvimento aparece na caixa de entrada acima e **nenhuma sai para a internet**. Em produção, as mesmas variáveis `Email__*` apontam para um provedor SMTP real.

Para trabalhar sem contêiner:

```bash
dotnet tool restore          # instala o dotnet-ef fixado pelo repositório
npm run api:testar           # testes do backend
npm run web                  # cliente em modo de desenvolvimento
npm run verificar            # testes do backend e build do cliente
```

## Estado

Arquitetura decidida e documentada. Três etapas concluídas:

| Etapa | O que entrou |
|---|---|
| **1 — Fundação** | Solução nas quatro camadas, migrações, OpenAPI, ambiente conteinerizado |
| **2 — Transversais** | Isolamento por instituição, trilha de auditoria, exclusão lógica, paginação |
| **3 — Acesso** | Cadastro com dois tipos de conta, autenticação, sessão renovável, perfil e redefinição de senha por e-mail, do banco à tela |

São **147 testes**: 24 verificam a separação de camadas que o RNF32 exige, e um conjunto prova que um gestor da instituição A recebe *não encontrado* ao pedir um registro da instituição B pelo identificador — o requisito de maior risco do projeto, coberto antes de existir endpoint que dependa dele.

Os tokens de design do cliente são gerados das variáveis do Figma, com o mesmo nome no design e no código.

A próxima etapa é o vínculo institucional, que destrava tudo que é institucional.

Pendências e decisões abertas em [AGENTS.md](AGENTS.md).

## Para agentes de IA

Leia [AGENTS.md](AGENTS.md) antes de propor qualquer alteração. Ele registra as decisões já fechadas, as convenções do projeto e as armadilhas conhecidas.
