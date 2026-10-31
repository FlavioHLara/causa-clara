# Especificação do Protótipo de Interface

**Sistema de Gestão Administrativa e Transparência Financeira para Organizações do Terceiro Setor**
Insumo para a seção 5.4 do artigo — Interface do Sistema

---

## 1. Premissas de projeto

| Premissa | Origem |
|---|---|
| Dois enquadramentos por tela: **1440 × 1024** (desktop) e **360 × 800** (móvel) | RNF01 |
| Linguagem corrente, sem jargão contábil, no painel público | RNF02 |
| Contraste adequado e navegação por teclado entre elementos interativos | RNF07 |
| Situação crítica sinalizada por **cor + ícone + rótulo textual**, nunca só por cor | RNF08 |
| Mensagens de erro específicas, em português, sem informação técnica | RNF05, RNF06 |
| Registro de doação concluído em até **quatro interações** a partir do painel | RNF04 |
| Toda listagem com paginação e ordenação visíveis | RNF09 |
| Aviso de privacidade presente na tela de cadastro | RNF22 |

O protótipo é de **alta fidelidade**. A metodologia (seção 4.2) menciona baixa e alta fidelidade; a versão de baixa fidelidade fica registrada como o inventário de telas e o mapa de navegação deste documento, que antecedem o desenho visual.

---

## 2. Inventário de telas

### Público — sem autenticação

**T01 · Acesso**
Formulário de e-mail e senha, chamada para criar conta e link para redefinição de senha. Acesso visível ao painel público de transparência sem autenticar.
*Atende:* RF01, RF05, RF09

**T02 · Criar conta**
Seleção entre **Apoiador** e **Gestor** em dois cartões lado a lado, cada um descrevendo as permissões concedidas antes da confirmação. Abaixo, formulário com nome, e-mail, telefone opcional, senha e confirmação. Aviso de privacidade ao pé.
*Atende:* RF02, RF03, RF04, RNF22

**T03 · Instituições**
Relação pública de instituições em cartões, com campo de busca, filtro por estado, ordenação e paginação.
*Atende:* RF26

**T04 · Perfil da instituição**
Identificação, descrição, localidade, contatos e dois botões de ação: ver transparência e doar.
*Atende:* RF27

**T05 · Painel público de transparência** — *tela central do trabalho*
Quatro indicadores do período: total arrecadado, quantidade de doações, total de despesas e documentos fiscais. Saldo do período e percentual aplicado em atividades-fim. Gráfico de linha com a evolução mensal de receitas e despesas. Gráfico de rosca com a distribuição de despesas por categoria. Composição dos bens em estoque por categoria. Tabelas de doações e de despesas, com acesso ao comprovante. Filtro por período e categoria. Exportação em CSV e PDF.
*Atende:* RF63 a RF73

### Apoiador

**T06 · Meu painel**
Totais consolidados: valor total doado, quantidade de doações e instituições apoiadas. Lista das últimas contribuições e atalho para nova doação.
*Atende:* RF33

**T07 · Registrar doação**
Alternância entre doação **financeira** (valor) e **material** (item, quantidade, unidade, valor estimado). Campo de descrição e opção de anonimato na divulgação pública. Confirmação em etapa única.
*Atende:* RF28, RF29, RF30, RF31, RF34, RNF04

**T08 · Minhas doações**
Tabela com instituição, tipo, valor, item e data, com ordenação e paginação. Ação de emitir comprovante em PDF por linha.
*Atende:* RF32, RF35

### Gestor sem vínculo

**T09 · Primeiro acesso**
Duas alternativas em cartões: criar uma nova instituição ou solicitar acesso a uma existente. O primeiro caminho abre o formulário de cadastro institucional; o segundo, a busca de instituições com envio de solicitação e aviso de que o acesso depende de aprovação.
*Atende:* RF10, RF11, RF13, RF14, RF15, RF17

### Gestor vinculado

**T10 · Painel administrativo**
Indicadores do período corrente. Bloco de **itens em quantidade crítica**, ordenados pela menor quantidade, com quantidade atual, mínima e categoria. Contadores de alertas não lidos e de solicitações de vínculo pendentes.
*Atende:* RF62, RF74, RF75, RF76

**T11 · Doações recebidas**
Tabela com doador, tipo, valor e data, com filtro, ordenação e paginação. Formulário em janela modal para lançamento, com escolha entre doador cadastrado e nome livre. Ações de alterar e excluir.
*Atende:* RF36, RF37, RF38, RF40

**T12 · Despesas e comprovantes**
Tabela com documento fiscal, data de emissão, valor, categoria e comprovante. Formulário com área de anexo de arquivo e campo opcional de vínculo à doação que custeou a despesa. Filtros por período, categoria e valor.
*Atende:* RF41, RF42, RF43, RF44

**T13 · Itens de estoque**
Tabela com nome, categoria, quantidade atual, quantidade mínima, unidade e valorização. Linha em situação crítica destacada por cor, ícone e rótulo. Filtro por categoria e busca. Ações de editar, inativar e movimentar.
*Atende:* RF48, RF49, RF50, RF59, RF62

**T14 · Movimentação do item**
Registro de entrada ou saída com quantidade e observação. Exibição do saldo antes e depois. Bloqueio visível quando a saída excede o disponível. Histórico de movimentações do item com tipo, quantidade, data e gestor responsável.
*Atende:* RF52, RF53, RF54, RF55, RF57

**T15 · Alertas de estoque**
Relação dos alertas não lidos, com item, mensagem e data, e ação de marcar como lido.
*Atende:* RF60, RF61

**T16 · Membros e solicitações**
Tabela com nome, e-mail, telefone, papel, situação e datas. Solicitações pendentes no topo, com ações de aprovar e rejeitar. Ações restritas ao Gestor Dono, indisponíveis visualmente para o Funcionário.
*Atende:* RF18, RF19, RF20, RF21, RF23

**T17 · Relatórios e prestação de contas**
Seleção de período e de tipo de relatório: doações, despesas, movimentações de estoque ou demonstrativo consolidado. Resultado com a relação dos lançamentos, totais por categoria e valorização dos bens. Exportação em CSV e PDF.
*Atende:* RF58, RF77, RF78, RF79, RF80

### Telas secundárias

**T18 · Cadastro institucional** — edição dos dados da instituição, restrita ao Dono. *Atende:* RF25
**T19 · Categorias** — cadastro de categorias de receita, despesa e estoque, com bloqueio de exclusão quando em uso. *Atende:* RF45, RF46, RF47

### Lacuna encontrada na implementação

> **Faltam as duas telas do RF09.** T01 prevê o link "Esqueci minha senha", mas este inventário não tem a tela que pede o link nem a de escolher a nova senha. A lacuna só apareceu ao construir a etapa 3, quando o link de T01 precisou levar a algum lugar.
>
> As duas existem no código desde 29/10/2026, feitas a partir do requisito e dos tokens do design: uma pede o endereço e confirma com a mesma mensagem exista ou não a conta — RF09 não pode virar verificador de quem tem cadastro —, e a outra lê o segredo da URL e pede a senha nova duas vezes, avisando que as sessões abertas serão encerradas.
>
> Precisam ser desenhadas no Figma quando o protótipo for atualizado, e a contagem de telas do artigo passa de dezenove para vinte e uma.

---

## 3. Mapa de navegação

```
T01 Acesso
 ├─ T02 Criar conta ──────────────┐
 ├─ T05 Transparência (público)   │
 └─ autenticação ─────────────────┤
                                  │
       ┌──────── Apoiador ────────┤
       │  T06 Meu painel          │
       │   ├─ T07 Registrar doação│
       │   └─ T08 Minhas doações  │
       │  T03 Instituições        │
       │   └─ T04 Perfil ─ T05    │
       │                          │
       └──────── Gestor ──────────┘
            sem vínculo → T09 Primeiro acesso
                            ├─ criar  → vira Dono
                            └─ solicitar → aguarda aprovação
            com vínculo  → T10 Painel administrativo
                            ├─ T11 Doações recebidas
                            ├─ T12 Despesas
                            ├─ T13 Itens de estoque ─ T14 Movimentação
                            ├─ T15 Alertas
                            ├─ T16 Membros (somente Dono)
                            ├─ T17 Relatórios
                            └─ T18 Cadastro institucional (somente Dono)
                                 T19 Categorias (a partir do seletor de T12)
```

Navegação autenticada por barra lateral fixa, com itens variando conforme o tipo de conta e o papel de vínculo. Navegação pública por cabeçalho simples.

### Como cada perfil entra

O protótipo navegável precisa de um ponto de entrada por perfil, senão metade das telas fica inalcançável. São três:

| Entrada | Caminho | Chega em |
|---|---|---|
| Gestor com vínculo | T01, botão **Entrar** | T10 Painel administrativo |
| Gestor sem vínculo | T02, cartão **Gestor** | T09 Primeiro acesso |
| Apoiador | T02, cartão **Apoiador** ou botão **Criar conta** | T06 Meu painel |
| Visitante | T01, **Consultar transparência sem entrar** | T05 Transparência |

O botão **Entrar** leva ao painel do gestor porque o campo de e-mail de T01 traz `gestor@casasemear.org.br`: o protótipo é determinístico, e o destino precisa ser coerente com o dado exibido. Os dois cartões de tipo de conta de T02 são clicáveis e ramificam os perfis — é também o que acontece no sistema, já que a escolha do tipo decide para onde o cadastro conclui (RN01).

**T19 Categorias não tem item na barra lateral.** No protótipo ela é alcançada pelo seletor de categoria de T12. Decidir se Categorias ganha item de menu próprio ou fica como subtela de Instituição é pendência de projeto, não de protótipo.

---

## 4. Tokens de design

### Cor

| Token | Valor | Uso |
|---|---|---|
| `verde/600` | `#2F6F4F` | Cor primária: ações, barra lateral ativa, cabeçalhos |
| `verde/700` | `#1D4A35` | Estado pressionado, texto sobre fundo claro |
| `verde/100` | `#E7F1EC` | Fundo de destaque, estado selecionado |
| `tinta/900` | `#15181D` | Texto principal |
| `tinta/600` | `#4A515C` | Texto secundário |
| `tinta/400` | `#7B8392` | Texto de apoio, rótulos |
| `linha/300` | `#D7DBE2` | Bordas e divisores |
| `fundo/100` | `#F6F7F9` | Fundo da aplicação |
| `fundo/0` | `#FFFFFF` | Superfície de cartão e tabela |
| `critico/600` | `#B4231B` | Estoque crítico, erro, saída |
| `critico/100` | `#FBE9E7` | Fundo de linha crítica |
| `atencao/600` | `#B4530B` | Pendências, aguardando decisão |
| `atencao/100` | `#FDF0E3` | Fundo de aviso |
| `entrada/600` | `#1D6FA8` | Movimentação de entrada, receita |
| `entrada/100` | `#E6F1F8` | Fundo de entrada |

Os três pares semânticos — crítico, atenção e entrada — sempre acompanhados de ícone e rótulo textual, conforme RNF08.

### Tipografia

Família **Inter**, disponível no Figma e no Google Fonts.

| Estilo | Tamanho / entrelinha | Peso |
|---|---|---|
| Título de tela | 28 / 34 | 600 |
| Título de seção | 20 / 26 | 600 |
| Título de cartão | 16 / 22 | 600 |
| Corpo | 14 / 21 | 400 |
| Corpo forte | 14 / 21 | 600 |
| Apoio | 12 / 17 | 400 |
| Rótulo de tabela | 11 / 16 | 600, caixa alta, espaçamento 0.07em |
| Indicador numérico | 32 / 38 | 650, algarismos tabulares |

Valores monetários e quantidades sempre com algarismos tabulares, para alinhamento em coluna.

### Espaçamento e forma

Escala de 4 px: `4, 8, 12, 16, 24, 32, 48, 64`.
Raio de canto: `6` em campos e botões, `12` em cartões, `999` em etiquetas.
Sombra: apenas em janela modal e menu suspenso. Cartões usam borda, não sombra.
Largura da barra lateral: `248`. Largura máxima do conteúdo: `1180`.

---

## 5. Catálogo de componentes

Construir como componentes do Figma, com variantes, antes de montar as telas.

| Componente | Variantes |
|---|---|
| Botão | primário · secundário · contorno · perigo · somente ícone × normal/pressionado/desabilitado |
| Campo de texto | normal · foco · erro · desabilitado · com ícone · com prefixo monetário |
| Seleção | lista suspensa · seletor de data · seletor de período |
| Alternador de tipo | dois ou três segmentos, usado em doação financeira/material |
| Etiqueta de situação | pendente · aprovado · rejeitado · crítico · entrada · saída |
| Cartão de indicador | com e sem variação percentual · com e sem ícone |
| Tabela | cabeçalho ordenável · linha normal · linha crítica · linha selecionada · estado vazio |
| Paginação | com seletor de 20/50/100 por página |
| Janela modal | pequena · média · com formulário |
| Barra lateral | apoiador · gestor dono · gestor funcionário |
| Cabeçalho público | com e sem sessão iniciada |
| Faixa de alerta | informação · atenção · crítico |
| Área de anexo | vazia · com arquivo · enviando · erro de formato |
| Gráfico de linha | receitas e despesas por mês |
| Gráfico de rosca | despesas por categoria, com legenda lateral |
| Barra de composição | bens em estoque por categoria |
| Estado vazio | ilustração, texto e ação sugerida |

---

## 6. Dados de exemplo

Para dar verossimilhança ao protótipo e às capturas do artigo, usar uma instituição fictícia coerente:

- **Instituição:** Casa de Apoio Semear — Betim, MG
- **Período de referência:** março de 2026
- **Indicadores:** arrecadado R$ 18.450,00 · 37 doações · despesas R$ 14.820,00 · 23 documentos fiscais · saldo R$ 3.630,00 · 82% aplicado em atividades-fim
- **Categorias de despesa:** alimentação, medicamentos, manutenção, higiene e limpeza, administrativo
- **Itens em situação crítica:** arroz 5 kg (2 de 10), dipirona 500 mg (4 de 15), sabonete (8 de 20)
- **Doadores exibidos publicamente:** Maria, Anônimo, João, Anônimo, Luciana

Nomes de doadores sempre em primeiro nome ou anônimos, respeitando RF66 e RN09 já no protótipo — a captura de tela do artigo não pode contradizer o requisito.

---

## 7. Ordem de construção no Figma

1. Página **Fundamentos** — variáveis de cor, estilos de texto, escala de espaçamento
2. Página **Componentes** — catálogo da seção 5, com variantes e propriedades
3. Página **Telas · Público** — T01 a T05
4. Página **Telas · Apoiador** — T06 a T08
5. Página **Telas · Gestor** — T09 a T17
6. Página **Móvel** — versão em 360 px de cada uma das dezenove telas, M01 a M19, mais as três gavetas de menu M20 a M22
7. **Protótipo** — ligações de navegação conforme o mapa da seção 3

A versão móvel cobre as dezenove telas, uma a uma. A correspondência é direta: M*nn* é a versão em 360 px de T*nn*, com os mesmos dados de exemplo — mesma instituição fictícia, mesmo período de referência, mesmos lançamentos —, para que as capturas do artigo não se contradigam entre os dois enquadramentos.

### Padrão responsivo

As regras abaixo valem para todas as telas e explicam por que nenhuma versão móvel é a de desktop espremida:

| Elemento de desktop | Forma em 360 px |
|---|---|
| Tabela | Cartões empilhados. Cada rótulo de coluna vira um par rótulo–valor dentro do cartão, e a coluna de ações vira botões de largura cheia ao pé. |
| Barra lateral de 248 px | Barra superior fixa de 56 px, com o título da seção à esquerda e menu sanduíche à direita. O sanduíche abre uma gaveta de menu — ver abaixo. |
| Linha de filtros lado a lado | Campos empilhados, ou duas colunas de 158 px quando são apenas dois e curtos. |
| Paginação de 720 px | Bloco compacto: contagem de registros em cima, Anterior · posição · Próxima embaixo. |
| Cartões de indicador em fila de quatro | Dois por linha, em 158 px, ou um por linha quando o número é longo. |
| Linha de tabela em situação crítica | Cartão inteiro com fundo `critico/100`, borda `critico/600`, etiqueta **Crítico** e os valores em `critico/600` — cor, ícone e rótulo, conforme RNF08. |
| Estado vazio de 520 px | Bloco centrado de largura cheia, sem ilustração. |

A margem lateral é de 16 px, o que dá 328 px de conteúdo útil. Alvos de toque têm pelo menos 40 px de altura.

### As três gavetas de menu

A barra lateral do desktop não cabe em 360 px, e sem substituto o menu sanduíche não leva a lugar nenhum — metade das telas de gestor ficaria inalcançável no navegável. São três telas de menu, uma por perfil, espelhando exatamente as variantes do componente **Barra lateral** e o **Cabeçalho público**:

| Tela | Perfil | Itens |
|---|---|---|
| **M20 · Menu público** | Visitante | Instituições · Transparência · Entrar |
| **M21 · Menu do apoiador** | Apoiador | Meu painel · Instituições · Registrar doação · Minhas doações · Sair |
| **M22 · Menu do gestor** | Gestor Dono | Painel administrativo · Doações recebidas · Despesas · Estoque · Alertas · Membros · Relatórios · Instituição · Sair |

Elas não têm correspondente no desktop, onde a barra lateral está sempre visível. São o preço de 360 px, não telas novas do sistema, e por isso não recebem numeração T nem atendem a RF próprio.
