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

Protótipo de alta fidelidade no Figma: 19 telas desktop, 5 móveis e 34 ligações de navegação.

## Stack

React · Vite · TypeScript · C# · .NET 10 · ASP.NET Core · PostgreSQL · Docker

## Estado

Arquitetura decidida. Implementação ainda não iniciada — ver pendências em [AGENTS.md](AGENTS.md).

## Para agentes de IA

Leia [AGENTS.md](AGENTS.md) antes de propor qualquer alteração. Ele registra as decisões já fechadas, as convenções do projeto e as armadilhas conhecidas.
