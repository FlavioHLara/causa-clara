# Scripts de construção do protótipo no Figma

Arquivos temporários de build, não fazem parte do artigo. Podem ser apagados quando o protótipo estiver fechado.

Cada `.mjs` contém o corpo que vai inteiro no parâmetro `code` da ferramenta `use_figma`. A extensão é só para permitir validação local.

## Por que existem

O plano Starter do Figma limita as chamadas MCP por dia. Cada erro de sintaxe ou correção queima uma chamada. Pré-escrever e validar localmente derruba a construção de ~10 chamadas para 4.

## Ordem de execução

| Lote | Conteúdo | Posição no canvas |
|---|---|---|
| `lote-A-T10-T14.mjs` | T10 Painel administrativo, T11 Doações recebidas, T12 Despesas, T13 Itens de estoque, T14 Movimentação | página Telas · Desktop, linha y=3400 |
| `lote-B-T15-T19.mjs` | T15 Alertas, T16 Membros, T17 Relatórios, T18 Cadastro institucional, T19 Categorias | página Telas · Desktop, linha y=5100 |
| `lote-C-movel.mjs` | M01, M05, M07, M10, M13 em 360 px | página Telas · Móvel |
| `lote-D-prototipo.mjs` | ligações de navegação | roda **depois** de A e B |

O lote D localiza as telas por nome, não por ID, então não depende do que A e B devolverem.

## Validação local

```bash
node check.mjs lote-A-T10-T14.mjs
node lint.mjs lote-A-T10-T14.mjs lote-B-T15-T19.mjs lote-C-movel.mjs lote-D-prototipo.mjs
```

`check.mjs` embrulha o código num contexto async, como o Figma faz, e confirma a sintaxe — sem isso o `return` de topo acusa erro falso.

`lint.mjs` verifica as armadilhas que já custaram chamadas:

- valores inválidos em `counterAxisAlignItems` / `primaryAxisAlignItems` (aceitam MIN/MAX/CENTER/BASELINE, nunca END)
- valores inválidos em `counterAxisSizingMode` / `primaryAxisSizingMode` (FIXED/AUTO) e em `layoutSizing*` (FIXED/HUG/FILL)
- `stroke-dasharray` em `<circle>` ou `<ellipse>`, que o Figma fragmenta — em `polyline` e `path` funciona normalmente
- `figma.notify` e APIs não suportadas
- mais de um `setCurrentPageAsync` por chamada
- IDs de nós, variáveis e estilos que não constam em `../figma-prototipo-estado.json`

## Armadilha principal

`resize()` trava os dois eixos em FIXED. Todo contêiner de auto-layout precisa do helper `setW()`, que restaura o eixo que deve crescer. Sem isso o contêiner colapsa em 10 px e a tela sai praticamente vazia — foi o que aconteceu com as quatro primeiras telas, que tiveram de ser refeitas.

## Comportamento útil

Erro em qualquer ponto do script reverte a transação inteira. Scripts grandes são seguros para repetir: ou aplicam tudo, ou não aplicam nada.
