// Lote A — T10 Painel administrativo, T11 Doações recebidas, T12 Despesas,
// T13 Itens de estoque, T14 Movimentação do item.
// Conteúdo deste arquivo vai inteiro no parâmetro `code` de use_figma.
// Validado localmente com `node --check` antes de gastar cota do Figma.

const page = await figma.getNodeByIdAsync('2:61');
await figma.setCurrentPageAsync(page);
for (const s of ['Regular', 'Medium', 'Semi Bold', 'Bold']) await figma.loadFontAsync({ family: 'Inter', style: s });

const V = async id => await figma.variables.getVariableByIdAsync(id);
const C = {
  prim: await V('VariableID:2:21'), primSuave: await V('VariableID:2:23'),
  txtPrin: await V('VariableID:2:24'), txtSec: await V('VariableID:2:25'), txtApoio: await V('VariableID:2:26'),
  txtAcento: await V('VariableID:2:28'), bordaPad: await V('VariableID:2:29'), bordaAce: await V('VariableID:2:30'),
  fundoApp: await V('VariableID:2:31'), superficie: await V('VariableID:2:32'),
  critico: await V('VariableID:2:33'), criticoSuave: await V('VariableID:2:34'), criticoTxt: await V('VariableID:2:35'),
  atencao: await V('VariableID:2:36'), atencaoSuave: await V('VariableID:2:37'), atencaoTxt: await V('VariableID:2:38'),
  entrada: await V('VariableID:2:39'), entradaSuave: await V('VariableID:2:40'), entradaTxt: await V('VariableID:2:41')
};
const E = {
  4: await V('VariableID:2:42'), 8: await V('VariableID:2:43'), 12: await V('VariableID:2:44'),
  16: await V('VariableID:2:45'), 24: await V('VariableID:2:46'), 32: await V('VariableID:2:47'), 48: await V('VariableID:2:48')
};
const R = { campo: await V('VariableID:2:50'), cartao: await V('VariableID:2:51') };
const TS = {
  tela: 'S:6ed5a05709624b2f472916d27e5d227d56f2a606,', secao: 'S:a070e6ba5dbcea3b3ef7f3368ae4f48ac223c067,',
  cartao: 'S:e9eafd2af8830136a2b470934cc3b1c5755dc155,', corpo: 'S:8ab5a263dfdb10f27e9ebfcb23e31400475e9ffa,',
  corpoF: 'S:4c64dce9bc4dc1a9ad4f4e783ef7686004c37dc8,', apoio: 'S:44df987ff794c67c8f62d4c9c841211f170e746a,',
  rotulo: 'S:f93f0275aebf356ca5b0602eb37f4869246648fe,', indicador: 'S:de24cb2c9175a600b0086b1da6e23b1f7ef912a5,'
};

const fv = (n, v) => { n.fills = [figma.variables.setBoundVariableForPaint({ type: 'SOLID', color: { r: 0, g: 0, b: 0 } }, 'color', v)]; };
const sv = (n, v, w) => { n.strokes = [figma.variables.setBoundVariableForPaint({ type: 'SOLID', color: { r: 0, g: 0, b: 0 } }, 'color', v)]; n.strokeWeight = w || 1; };
const pad = (n, t, r, b, l) => { n.setBoundVariable('paddingTop', t); n.setBoundVariable('paddingRight', r); n.setBoundVariable('paddingBottom', b); n.setBoundVariable('paddingLeft', l); };
const rad = (n, v) => { for (const k of ['topLeftRadius', 'topRightRadius', 'bottomLeftRadius', 'bottomRightRadius']) n.setBoundVariable(k, v); };
async function T(ch, st, cv, w) { const t = figma.createText(); if (w) { t.resize(w, 10); t.textAutoResize = 'HEIGHT'; } t.characters = ch; await t.setTextStyleIdAsync(st); if (cv) fv(t, cv); return t; }
function AL(d, n, g) { const f = figma.createAutoLayout(d, { name: n || 'Grupo' }); f.fills = []; if (g) f.setBoundVariable('itemSpacing', g); return f; }
// ARMADILHA CONHECIDA: resize() trava os dois eixos em FIXED. Sempre restaurar o eixo que deve crescer.
function setW(f, w) {
  f.resize(w, Math.max(f.height, 1));
  if (f.layoutMode === 'VERTICAL') { f.counterAxisSizingMode = 'FIXED'; f.primaryAxisSizingMode = 'AUTO'; }
  else { f.primaryAxisSizingMode = 'FIXED'; f.counterAxisSizingMode = 'AUTO'; }
}
function card(w, g) { const f = AL('VERTICAL', 'Cartão', g || E[12]); fv(f, C.superficie); sv(f, C.bordaPad, 1); rad(f, R.cartao); pad(f, E[24], E[24], E[24], E[24]); if (w) setW(f, w); return f; }

const btnSet = await figma.getNodeByIdAsync('3:18'), campoSet = await figma.getNodeByIdAsync('3:32'),
  etqSet = await figma.getNodeByIdAsync('3:51'), sbSet = await figma.getNodeByIdAsync('4:110'),
  kpiComp = await figma.getNodeByIdAsync('4:9'), pagComp = await figma.getNodeByIdAsync('4:23'),
  fxSet = await figma.getNodeByIdAsync('4:22'), anexoSet = await figma.getNodeByIdAsync('4:39');

async function btn(t, l, e) { const i = btnSet.children.find(c => c.name === `Tipo=${t}, Estado=${e || 'Normal'}`).createInstance(); const x = i.findOne(n => n.type === 'TEXT'); if (x) x.characters = l; return i; }
async function campo(r, v2, e) { const i = campoSet.children.find(c => c.name === `Estado=${e || 'Normal'}`).createInstance(); const ts = i.findAll(n => n.type === 'TEXT'); if (ts[0]) ts[0].characters = r; if (ts[1]) ts[1].characters = v2; return i; }
function etq(t) { return etqSet.children.find(c => c.name === `Tipo=${t}`).createInstance(); }
function faixa(t, msg) { const i = fxSet.children.find(c => c.name === `Tipo=${t}`).createInstance(); const x = i.findOne(n => n.type === 'TEXT'); if (x) x.characters = msg; return i; }
function anexo(e) { return anexoSet.children.find(c => c.name === `Estado=${e}`).createInstance(); }
function barra(perfil, ativo) {
  const i = sbSet.children.find(c => c.name === `Perfil=${perfil}`).createInstance();
  if (ativo) {
    for (const ni of i.findAllWithCriteria({ types: ['INSTANCE'] })) {
      const tx = ni.findOne(n => n.type === 'TEXT'); if (!tx) continue;
      try { ni.setProperties({ Estado: tx.characters === ativo ? 'Ativo' : 'Normal' }); } catch (e) { }
    }
  }
  return i;
}

// tabela: linha pode ser array simples, ou { cells, critica } para destacar
async function tabela(w, cols, linhas) {
  const t = AL('VERTICAL', 'Tabela', null); t.itemSpacing = 0; setW(t, w);
  const hd = AL('HORIZONTAL', 'Cabeçalho', null); hd.itemSpacing = 0; setW(hd, w); hd.paddingBottom = 8; hd.counterAxisAlignItems = 'CENTER';
  for (const c of cols) { const x = await T(c.r, TS.rotulo, C.txtApoio, c.w); hd.appendChild(x); x.layoutSizingHorizontal = 'FIXED'; x.resize(c.w, x.height); }
  t.appendChild(hd); hd.layoutSizingHorizontal = 'FILL';
  const sep = figma.createFrame(); sep.name = 'Linha'; sep.resize(w, 1); fv(sep, C.bordaPad); t.appendChild(sep); sep.layoutSizingHorizontal = 'FILL';
  for (const raw of linhas) {
    const l = Array.isArray(raw) ? raw : raw.cells;
    const critica = !Array.isArray(raw) && raw.critica;
    const row = AL('HORIZONTAL', 'Linha', null); row.itemSpacing = 0; setW(row, w);
    row.paddingTop = 10; row.paddingBottom = 10; row.counterAxisAlignItems = 'CENTER';
    if (critica) { fv(row, C.criticoSuave); rad(row, R.campo); row.setBoundVariable('paddingLeft', E[8]); row.setBoundVariable('paddingRight', E[8]); }
    for (let i = 0; i < cols.length; i++) {
      const cel = l[i];
      if (cel && typeof cel === 'object' && cel.etiqueta) {
        const h = AL('HORIZONTAL', 'Célula', null); setW(h, cols[i].w); h.appendChild(etq(cel.etiqueta));
        row.appendChild(h); h.layoutSizingHorizontal = 'FIXED';
      } else {
        const corTexto = critica && i === 0 ? C.criticoTxt : (i === 0 ? C.txtPrin : C.txtSec);
        const x = await T(String(cel), i === 0 ? TS.corpoF : TS.corpo, corTexto, cols[i].w);
        row.appendChild(x); x.layoutSizingHorizontal = 'FIXED'; x.resize(cols[i].w, x.height);
      }
    }
    t.appendChild(row); row.layoutSizingHorizontal = 'FILL';
    const s2 = figma.createFrame(); s2.name = 'Linha'; s2.resize(w, 1); fv(s2, C.bordaPad); s2.opacity = .6; t.appendChild(s2); s2.layoutSizingHorizontal = 'FILL';
  }
  return t;
}

function telaAut(nome, x, y, perfil, ativo) {
  const s = AL('HORIZONTAL', nome); s.name = nome;
  s.resize(1440, 1); s.primaryAxisSizingMode = 'FIXED'; s.counterAxisSizingMode = 'AUTO';
  s.itemSpacing = 0; s.clipsContent = true; fv(s, C.fundoApp); s.x = x; s.y = y; page.appendChild(s);
  const sb = barra(perfil, ativo); s.appendChild(sb); sb.layoutSizingVertical = 'FILL';
  const main = AL('VERTICAL', 'Conteúdo', E[24]); pad(main, E[32], E[32], E[48], E[32]); setW(main, 1192);
  s.appendChild(main); main.layoutSizingHorizontal = 'FILL';
  return { s, main };
}
function fechar(s) { if (s.height < 1024) { s.counterAxisSizingMode = 'FIXED'; s.resize(1440, 1024); } }
async function titulo(main, t, sub, acao) {
  const c = AL('HORIZONTAL', 'Cabeçalho', E[16]); setW(c, 1128); c.counterAxisAlignItems = 'CENTER';
  const col = AL('VERTICAL', 'Títulos', E[4]);
  const a = await T(t, TS.tela, C.txtPrin); col.appendChild(a);
  if (sub) { const b = await T(sub, TS.corpo, C.txtSec); col.appendChild(b); }
  c.appendChild(col); col.layoutSizingHorizontal = 'FILL';
  if (acao) c.appendChild(acao);
  main.appendChild(c); c.layoutSizingHorizontal = 'FILL'; return c;
}
const ids = {};

/* ===================== T10 PAINEL ADMINISTRATIVO ===================== */
{
  const { s, main } = telaAut('T10 · Painel administrativo', 0, 3400, 'Gestor Dono', 'Painel administrativo');
  await titulo(main, 'Painel administrativo', 'Casa de Apoio Semear · março de 2026');
  const k = AL('HORIZONTAL', 'Indicadores', E[16]); setW(k, 1128);
  for (const [r, v2, sb] of [['ARRECADADO NO MÊS', 'R$ 18.450,00', '37 doações'],
  ['DESPESAS NO MÊS', 'R$ 14.820,00', '23 documentos fiscais'],
  ['SALDO DO PERÍODO', 'R$ 3.630,00', 'receitas menos despesas'],
  ['VALORIZAÇÃO DO ESTOQUE', 'R$ 7.570,00', '40 itens cadastrados']]) {
    const i = kpiComp.createInstance(); const ts = i.findAll(n => n.type === 'TEXT');
    if (ts[0]) ts[0].characters = r; if (ts[1]) ts[1].characters = v2; if (ts[2]) ts[2].characters = sb;
    k.appendChild(i); i.layoutSizingHorizontal = 'FILL';
  }
  main.appendChild(k); k.layoutSizingHorizontal = 'FILL';

  const avisos = AL('HORIZONTAL', 'Pendências', E[16]); setW(avisos, 1128);
  const f1 = faixa('Atenção', '3 solicitações de vínculo pendentes de decisão'); avisos.appendChild(f1); f1.layoutSizingHorizontal = 'FILL';
  const f2 = faixa('Crítico', '5 alertas de estoque não lidos'); avisos.appendChild(f2); f2.layoutSizingHorizontal = 'FILL';
  main.appendChild(avisos); avisos.layoutSizingHorizontal = 'FILL';

  const cCrit = card(1128, E[12]);
  const ct = await T('Itens em quantidade crítica', TS.cartao, C.txtPrin); cCrit.appendChild(ct);
  const cs = await T('Ordenados pela menor quantidade disponível. A situação é indicada por cor, ícone e rótulo (RF75, RNF08).', TS.apoio, C.txtApoio, 1080);
  cCrit.appendChild(cs); cs.layoutSizingHorizontal = 'FILL';
  const tb = await tabela(1080, [{ r: 'ITEM', w: 300 }, { r: 'CATEGORIA', w: 240 }, { r: 'QTD. ATUAL', w: 150 }, { r: 'QTD. MÍNIMA', w: 150 }, { r: 'SITUAÇÃO', w: 240 }],
    [{ cells: ['Arroz 5 kg', 'Alimentos básicos', '2 un', '10 un', { etiqueta: 'Crítico' }], critica: true },
    { cells: ['Dipirona 500 mg', 'Medicamentos', '4 cx', '15 cx', { etiqueta: 'Crítico' }], critica: true },
    { cells: ['Sabonete', 'Higiene e limpeza', '8 un', '20 un', { etiqueta: 'Crítico' }], critica: true },
    ['Feijão 1 kg', 'Alimentos básicos', '18 un', '12 un', { etiqueta: 'Aprovado' }],
    ['Fralda geriátrica', 'Higiene e limpeza', '34 un', '20 un', { etiqueta: 'Aprovado' }]]);
  cCrit.appendChild(tb); tb.layoutSizingHorizontal = 'FILL';
  main.appendChild(cCrit); cCrit.layoutSizingHorizontal = 'FILL';

  const dupla = AL('HORIZONTAL', 'Resumos', E[16]); setW(dupla, 1128);
  const cDo = card(556, E[12]);
  const d1 = await T('Últimas doações recebidas', TS.cartao, C.txtPrin); cDo.appendChild(d1);
  const dTab = await tabela(508, [{ r: 'DOADOR', w: 180 }, { r: 'VALOR', w: 168 }, { r: 'DATA', w: 160 }],
    [['Maria Aparecida', 'R$ 2.000,00', '28/03/2026'], ['Doador anônimo', 'R$ 1.500,00', '26/03/2026'],
    ['João Batista', 'R$ 840,00', '24/03/2026']]);
  cDo.appendChild(dTab); dTab.layoutSizingHorizontal = 'FILL';
  dupla.appendChild(cDo); cDo.layoutSizingHorizontal = 'FILL';
  const cDe = card(556, E[12]);
  const e1 = await T('Últimas despesas', TS.cartao, C.txtPrin); cDe.appendChild(e1);
  const eTab = await tabela(508, [{ r: 'DOCUMENTO', w: 180 }, { r: 'VALOR', w: 168 }, { r: 'DATA', w: 160 }],
    [['NF 00123', 'R$ 3.240,00', '27/03/2026'], ['NF 00119', 'R$ 1.890,00', '25/03/2026'],
    ['NF 00117', 'R$ 1.450,00', '22/03/2026']]);
  cDe.appendChild(eTab); eTab.layoutSizingHorizontal = 'FILL';
  dupla.appendChild(cDe); cDe.layoutSizingHorizontal = 'FILL';
  main.appendChild(dupla); dupla.layoutSizingHorizontal = 'FILL';
  fechar(s); ids['T10'] = s.id;
}

/* ===================== T11 DOAÇÕES RECEBIDAS ===================== */
{
  const acao = await btn('Primário', 'Registrar doação recebida');
  const { s, main } = telaAut('T11 · Doações recebidas', 1540, 3400, 'Gestor Dono', 'Doações recebidas');
  await titulo(main, 'Doações recebidas', 'Inclui doações originadas fora do sistema, com doador cadastrado ou nome livre (RF36).', acao);
  const fl = AL('HORIZONTAL', 'Filtros', E[12]); setW(fl, 1128); fl.counterAxisAlignItems = 'MAX';
  const f1 = await campo('Período', '01/03/2026 a 31/03/2026'); fl.appendChild(f1); f1.layoutSizingHorizontal = 'FIXED'; f1.resize(300, f1.height);
  const f2 = await campo('Tipo', 'Todos'); fl.appendChild(f2); f2.layoutSizingHorizontal = 'FIXED'; f2.resize(180, f2.height);
  const f3 = await campo('Doador', 'Todos'); fl.appendChild(f3); f3.layoutSizingHorizontal = 'FIXED'; f3.resize(240, f3.height);
  const sp = figma.createFrame(); sp.name = 'Espaço'; sp.resize(10, 10); sp.fills = []; fl.appendChild(sp); sp.layoutSizingHorizontal = 'FILL';
  main.appendChild(fl); fl.layoutSizingHorizontal = 'FILL';
  const c = card(1128, E[12]);
  const tb = await tabela(1080, [{ r: 'DOADOR', w: 230 }, { r: 'TIPO', w: 120 }, { r: 'ITEM', w: 220 }, { r: 'VALOR', w: 160 }, { r: 'DATA', w: 150 }, { r: 'AÇÕES', w: 200 }],
    [['Maria Aparecida', 'Financeira', '—', 'R$ 2.000,00', '28/03/2026', 'Editar · Excluir'],
    ['Doador anônimo', 'Financeira', '—', 'R$ 1.500,00', '26/03/2026', 'Editar · Excluir'],
    ['João Batista', 'Material', 'Arroz 5 kg · 20 un', 'R$ 840,00', '24/03/2026', 'Editar · Excluir'],
    ['Empresa Verde Ltda.', 'Material', 'Fraldas · 60 un', 'R$ 1.260,00', '23/03/2026', 'Editar · Excluir'],
    ['Doador anônimo', 'Financeira', '—', 'R$ 600,00', '21/03/2026', 'Editar · Excluir'],
    ['Luciana Prado', 'Material', 'Sabonete · 48 un', 'R$ 450,00', '19/03/2026', 'Editar · Excluir']]);
  c.appendChild(tb); tb.layoutSizingHorizontal = 'FILL';
  const pg = pagComp.createInstance(); c.appendChild(pg); pg.layoutSizingHorizontal = 'FILL';
  const pgt = pg.findAll(n => n.type === 'TEXT')[0]; if (pgt) pgt.characters = '37 doações no período · 20 por página';
  main.appendChild(c); c.layoutSizingHorizontal = 'FILL';
  const nota = faixa('Informação', 'O nome completo do doador aparece apenas aqui, na visão interna. O painel público exibe somente o primeiro nome ou Anônimo (RN09).');
  main.appendChild(nota); nota.layoutSizingHorizontal = 'FILL';
  fechar(s); ids['T11'] = s.id;
}

/* ===================== T12 DESPESAS E COMPROVANTES ===================== */
{
  const { s, main } = telaAut('T12 · Despesas e comprovantes', 3080, 3400, 'Gestor Dono', 'Despesas');
  await titulo(main, 'Despesas e comprovantes', 'Cada despesa pode ser vinculada à doação que a custeou, permitindo rastrear a aplicação dos recursos (RF44).');
  const colunas = AL('HORIZONTAL', 'Colunas', E[16]); setW(colunas, 1128);

  const cTab = card(720, E[12]);
  const fl = AL('HORIZONTAL', 'Filtros', E[8]); setW(fl, 672); fl.counterAxisAlignItems = 'MAX';
  const f1 = await campo('Período', '01/03 a 31/03'); fl.appendChild(f1); f1.layoutSizingHorizontal = 'FILL';
  const f2 = await campo('Categoria', 'Todas'); fl.appendChild(f2); f2.layoutSizingHorizontal = 'FILL';
  cTab.appendChild(fl); fl.layoutSizingHorizontal = 'FILL';
  const tb = await tabela(672, [{ r: 'DOCUMENTO', w: 130 }, { r: 'CATEGORIA', w: 180 }, { r: 'VALOR', w: 150 }, { r: 'COMPROVANTE', w: 212 }],
    [['NF 00123', 'Alimentação', 'R$ 3.240,00', 'nota-00123.pdf'],
    ['NF 00119', 'Medicamentos', 'R$ 1.890,00', 'nota-00119.pdf'],
    ['NF 00117', 'Manutenção', 'R$ 1.450,00', 'nota-00117.jpg'],
    ['NF 00112', 'Higiene e limpeza', 'R$ 980,00', 'nota-00112.pdf'],
    ['NF 00108', 'Administrativo', 'R$ 760,00', 'nota-00108.pdf']]);
  cTab.appendChild(tb); tb.layoutSizingHorizontal = 'FILL';
  const pg = pagComp.createInstance(); cTab.appendChild(pg); pg.layoutSizingHorizontal = 'FILL';
  const pgt = pg.findAll(n => n.type === 'TEXT')[0]; if (pgt) pgt.characters = '23 despesas no período · 20 por página';
  colunas.appendChild(cTab); cTab.layoutSizingHorizontal = 'FILL';

  const cForm = card(392, E[12]);
  const ft = await T('Registrar despesa', TS.cartao, C.txtPrin); cForm.appendChild(ft);
  for (const [r, v2] of [['Número do documento fiscal', 'NF 00124'], ['Data de emissão', '29/03/2026'],
  ['Valor total', 'R$ 1.120,00'], ['Categoria', 'Alimentação'], ['Doação que custeou (opcional)', 'Maria Aparecida · 28/03']]) {
    const f = await campo(r, v2); cForm.appendChild(f); f.layoutSizingHorizontal = 'FILL';
  }
  const an = anexo('Com arquivo'); cForm.appendChild(an); an.layoutSizingHorizontal = 'FILL';
  const ac = AL('HORIZONTAL', 'Ações', E[8]); setW(ac, 344);
  const b1 = await btn('Contorno', 'Limpar'); ac.appendChild(b1); b1.layoutSizingHorizontal = 'FILL';
  const b2 = await btn('Primário', 'Salvar despesa'); ac.appendChild(b2); b2.layoutSizingHorizontal = 'FILL';
  cForm.appendChild(ac); ac.layoutSizingHorizontal = 'FILL';
  colunas.appendChild(cForm); cForm.layoutSizingHorizontal = 'FIXED';
  main.appendChild(colunas); colunas.layoutSizingHorizontal = 'FILL';
  fechar(s); ids['T12'] = s.id;
}

/* ===================== T13 ITENS DE ESTOQUE ===================== */
{
  const acao = await btn('Primário', 'Cadastrar item');
  const { s, main } = telaAut('T13 · Itens de estoque', 4620, 3400, 'Gestor Dono', 'Estoque');
  await titulo(main, 'Itens de estoque', 'A quantidade atual resulta apenas do saldo inicial e das movimentações; não é editável diretamente (RN11).', acao);
  const k = AL('HORIZONTAL', 'Indicadores', E[16]); setW(k, 1128);
  for (const [r, v2, sb] of [['ITENS CADASTRADOS', '40', 'em 4 categorias'],
  ['ITENS EM SITUAÇÃO CRÍTICA', '3', 'quantidade igual ou abaixo da mínima'],
  ['VALORIZAÇÃO TOTAL', 'R$ 7.570,00', 'quantidade atual × preço de referência']]) {
    const i = kpiComp.createInstance(); const ts = i.findAll(n => n.type === 'TEXT');
    if (ts[0]) ts[0].characters = r; if (ts[1]) ts[1].characters = v2; if (ts[2]) ts[2].characters = sb;
    k.appendChild(i); i.layoutSizingHorizontal = 'FILL';
  }
  main.appendChild(k); k.layoutSizingHorizontal = 'FILL';
  const fl = AL('HORIZONTAL', 'Filtros', E[12]); setW(fl, 1128); fl.counterAxisAlignItems = 'MAX';
  const f1 = await campo('Buscar por nome', 'Arroz'); fl.appendChild(f1); f1.layoutSizingHorizontal = 'FILL';
  const f2 = await campo('Categoria', 'Todas'); fl.appendChild(f2); f2.layoutSizingHorizontal = 'FIXED'; f2.resize(240, f2.height);
  const f3 = await campo('Situação', 'Todas'); fl.appendChild(f3); f3.layoutSizingHorizontal = 'FIXED'; f3.resize(200, f3.height);
  main.appendChild(fl); fl.layoutSizingHorizontal = 'FILL';
  const c = card(1128, E[12]);
  const tb = await tabela(1080, [{ r: 'ITEM', w: 230 }, { r: 'CATEGORIA', w: 190 }, { r: 'QTD. ATUAL', w: 120 }, { r: 'QTD. MÍNIMA', w: 120 }, { r: 'VALORIZAÇÃO', w: 150 }, { r: 'SITUAÇÃO', w: 140 }, { r: 'AÇÕES', w: 130 }],
    [{ cells: ['Arroz 5 kg', 'Alimentos básicos', '2 un', '10 un', 'R$ 56,00', { etiqueta: 'Crítico' }, 'Movimentar'], critica: true },
    { cells: ['Dipirona 500 mg', 'Medicamentos', '4 cx', '15 cx', 'R$ 48,00', { etiqueta: 'Crítico' }, 'Movimentar'], critica: true },
    { cells: ['Sabonete', 'Higiene e limpeza', '8 un', '20 un', 'R$ 24,00', { etiqueta: 'Crítico' }, 'Movimentar'], critica: true },
    ['Feijão 1 kg', 'Alimentos básicos', '18 un', '12 un', 'R$ 162,00', { etiqueta: 'Aprovado' }, 'Movimentar'],
    ['Óleo de soja 900 ml', 'Alimentos básicos', '24 un', '12 un', 'R$ 192,00', { etiqueta: 'Aprovado' }, 'Movimentar'],
    ['Fralda geriátrica', 'Higiene e limpeza', '34 un', '20 un', 'R$ 1.020,00', { etiqueta: 'Aprovado' }, 'Movimentar'],
    ['Cobertor solteiro', 'Vestuário', '15 un', '8 un', 'R$ 675,00', { etiqueta: 'Aprovado' }, 'Movimentar']]);
  c.appendChild(tb); tb.layoutSizingHorizontal = 'FILL';
  const pg = pagComp.createInstance(); c.appendChild(pg); pg.layoutSizingHorizontal = 'FILL';
  const pgt = pg.findAll(n => n.type === 'TEXT')[0]; if (pgt) pgt.characters = '40 itens · 20 por página';
  main.appendChild(c); c.layoutSizingHorizontal = 'FILL';
  fechar(s); ids['T13'] = s.id;
}

/* ===================== T14 MOVIMENTAÇÃO DO ITEM ===================== */
{
  const { s, main } = telaAut('T14 · Movimentação do item', 6160, 3400, 'Gestor Dono', 'Estoque');
  await titulo(main, 'Arroz 5 kg', 'Alimentos básicos · unidade: un · quantidade mínima de referência: 10 un');
  const alerta = faixa('Crítico', 'Este item está em situação crítica: 2 un disponíveis para uma quantidade mínima de 10 un.');
  main.appendChild(alerta); alerta.layoutSizingHorizontal = 'FILL';
  const colunas = AL('HORIZONTAL', 'Colunas', E[16]); setW(colunas, 1128);

  const cForm = card(432, E[12]);
  const ft = await T('Registrar movimentação', TS.cartao, C.txtPrin); cForm.appendChild(ft);
  const tipoL = await T('Tipo de movimentação', TS.apoio, C.txtSec); cForm.appendChild(tipoL);
  const seg = AL('HORIZONTAL', 'Alternador', null); seg.itemSpacing = 0; setW(seg, 384); rad(seg, R.campo); sv(seg, C.bordaPad, 1);
  for (const [nm, sel] of [['Entrada', true], ['Saída', false]]) {
    const op = AL('HORIZONTAL', 'Opção', null); op.counterAxisAlignItems = 'CENTER'; op.primaryAxisAlignItems = 'CENTER';
    op.paddingTop = 10; op.paddingBottom = 10; setW(op, 192);
    fv(op, sel ? C.entradaSuave : C.superficie);
    const x = await T(nm, sel ? TS.corpoF : TS.corpo, sel ? C.entradaTxt : C.txtSec); op.appendChild(x);
    seg.appendChild(op); op.layoutSizingHorizontal = 'FILL';
  }
  cForm.appendChild(seg); seg.layoutSizingHorizontal = 'FILL';
  const q = await campo('Quantidade', '30'); cForm.appendChild(q); q.layoutSizingHorizontal = 'FILL';
  const ob = await campo('Observação (opcional)', 'Doação recebida de João Batista'); cForm.appendChild(ob); ob.layoutSizingHorizontal = 'FILL';
  const saldos = AL('HORIZONTAL', 'Saldos', E[12]); setW(saldos, 384);
  for (const [r, v2, cor] of [['SALDO ATUAL', '2 un', C.txtPrin], ['APÓS A MOVIMENTAÇÃO', '32 un', C.entradaTxt]]) {
    const b = AL('VERTICAL', 'Saldo', E[4]); pad(b, E[12], E[12], E[12], E[12]); rad(b, R.campo);
    fv(b, C.superficie); sv(b, C.bordaPad, 1); setW(b, 186);
    const x1 = await T(r, TS.rotulo, C.txtApoio); b.appendChild(x1);
    const x2 = await T(v2, TS.secao, cor); b.appendChild(x2);
    saldos.appendChild(b); b.layoutSizingHorizontal = 'FILL';
  }
  cForm.appendChild(saldos); saldos.layoutSizingHorizontal = 'FILL';
  const nota = await T('Saída não pode exceder a quantidade disponível, de modo que o estoque nunca fique negativo (RN12).', TS.apoio, C.txtApoio, 384);
  cForm.appendChild(nota); nota.layoutSizingHorizontal = 'FILL';
  const b2 = await btn('Primário', 'Registrar movimentação'); cForm.appendChild(b2); b2.layoutSizingHorizontal = 'FILL';
  colunas.appendChild(cForm); cForm.layoutSizingHorizontal = 'FIXED';

  const cHist = card(680, E[12]);
  const ht = await T('Histórico de movimentações', TS.cartao, C.txtPrin); cHist.appendChild(ht);
  const hs = await T('Registros imutáveis. Correções são feitas por nova movimentação em sentido contrário (RN15).', TS.apoio, C.txtApoio, 632);
  cHist.appendChild(hs); hs.layoutSizingHorizontal = 'FILL';
  const hTab = await tabela(632, [{ r: 'TIPO', w: 110 }, { r: 'QTD.', w: 80 }, { r: 'DATA', w: 120 }, { r: 'GESTOR', w: 180 }, { r: 'OBSERVAÇÃO', w: 142 }],
    [[{ etiqueta: 'Saída' }, '8 un', '27/03/2026', 'Flávio Lara', 'Consumo semanal'],
    [{ etiqueta: 'Saída' }, '10 un', '20/03/2026', 'Flávio Lara', 'Consumo semanal'],
    [{ etiqueta: 'Entrada' }, '20 un', '18/03/2026', 'Ana Ribeiro', 'Doação de João Batista'],
    [{ etiqueta: 'Saída' }, '12 un', '13/03/2026', 'Flávio Lara', 'Consumo semanal'],
    [{ etiqueta: 'Entrada' }, '12 un', '05/03/2026', 'Ana Ribeiro', 'Compra NF 00110']]);
  cHist.appendChild(hTab); hTab.layoutSizingHorizontal = 'FILL';
  colunas.appendChild(cHist); cHist.layoutSizingHorizontal = 'FILL';
  main.appendChild(colunas); colunas.layoutSizingHorizontal = 'FILL';
  fechar(s); ids['T14'] = s.id;
}

const bounds = [];
for (const [k, id] of Object.entries(ids)) { const n = await figma.getNodeByIdAsync(id); bounds.push({ tela: k, w: n.width, h: n.height, desc: n.findAll(() => true).length }); }
return { createdNodeIds: ids, bounds };
