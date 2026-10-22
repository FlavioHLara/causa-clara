// Lote B — T15 Alertas, T16 Membros e solicitações, T17 Relatórios,
// T18 Cadastro institucional, T19 Categorias.
// Mesmo preâmbulo do lote A: nenhum estado persiste entre chamadas de use_figma.

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
function setW(f, w) {
  f.resize(w, Math.max(f.height, 1));
  if (f.layoutMode === 'VERTICAL') { f.counterAxisSizingMode = 'FIXED'; f.primaryAxisSizingMode = 'AUTO'; }
  else { f.primaryAxisSizingMode = 'FIXED'; f.counterAxisSizingMode = 'AUTO'; }
}
function card(w, g) { const f = AL('VERTICAL', 'Cartão', g || E[12]); fv(f, C.superficie); sv(f, C.bordaPad, 1); rad(f, R.cartao); pad(f, E[24], E[24], E[24], E[24]); if (w) setW(f, w); return f; }

const btnSet = await figma.getNodeByIdAsync('3:18'), campoSet = await figma.getNodeByIdAsync('3:32'),
  etqSet = await figma.getNodeByIdAsync('3:51'), sbSet = await figma.getNodeByIdAsync('4:110'),
  kpiComp = await figma.getNodeByIdAsync('4:9'), pagComp = await figma.getNodeByIdAsync('4:23'),
  fxSet = await figma.getNodeByIdAsync('4:22'), vazioComp = await figma.getNodeByIdAsync('4:40');

async function btn(t, l, e) { const i = btnSet.children.find(c => c.name === `Tipo=${t}, Estado=${e || 'Normal'}`).createInstance(); const x = i.findOne(n => n.type === 'TEXT'); if (x) x.characters = l; return i; }
async function campo(r, v2, e) { const i = campoSet.children.find(c => c.name === `Estado=${e || 'Normal'}`).createInstance(); const ts = i.findAll(n => n.type === 'TEXT'); if (ts[0]) ts[0].characters = r; if (ts[1]) ts[1].characters = v2; return i; }
function etq(t) { return etqSet.children.find(c => c.name === `Tipo=${t}`).createInstance(); }
function faixa(t, msg) { const i = fxSet.children.find(c => c.name === `Tipo=${t}`).createInstance(); const x = i.findOne(n => n.type === 'TEXT'); if (x) x.characters = msg; return i; }
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
async function tabela(w, cols, linhas) {
  const t = AL('VERTICAL', 'Tabela', null); t.itemSpacing = 0; setW(t, w);
  const hd = AL('HORIZONTAL', 'Cabeçalho', null); hd.itemSpacing = 0; setW(hd, w); hd.paddingBottom = 8; hd.counterAxisAlignItems = 'CENTER';
  for (const c of cols) { const x = await T(c.r, TS.rotulo, C.txtApoio, c.w); hd.appendChild(x); x.layoutSizingHorizontal = 'FIXED'; x.resize(c.w, x.height); }
  t.appendChild(hd); hd.layoutSizingHorizontal = 'FILL';
  const sep = figma.createFrame(); sep.name = 'Linha'; sep.resize(w, 1); fv(sep, C.bordaPad); t.appendChild(sep); sep.layoutSizingHorizontal = 'FILL';
  for (const raw of linhas) {
    const l = Array.isArray(raw) ? raw : raw.cells;
    const destaque = !Array.isArray(raw) && raw.destaque;
    const row = AL('HORIZONTAL', 'Linha', null); row.itemSpacing = 0; setW(row, w);
    row.paddingTop = 10; row.paddingBottom = 10; row.counterAxisAlignItems = 'CENTER';
    if (destaque) { fv(row, destaque === 'atencao' ? C.atencaoSuave : C.criticoSuave); rad(row, R.campo); row.setBoundVariable('paddingLeft', E[8]); row.setBoundVariable('paddingRight', E[8]); }
    for (let i = 0; i < cols.length; i++) {
      const cel = l[i];
      if (cel && typeof cel === 'object' && cel.etiqueta) {
        const h = AL('HORIZONTAL', 'Célula', null); setW(h, cols[i].w); h.appendChild(etq(cel.etiqueta));
        row.appendChild(h); h.layoutSizingHorizontal = 'FIXED';
      } else if (cel && typeof cel === 'object' && cel.acoes) {
        const h = AL('HORIZONTAL', 'Ações', E[8]); setW(h, cols[i].w);
        for (const [tp, lb] of cel.acoes) { const b = await btn(tp, lb); h.appendChild(b); }
        row.appendChild(h); h.layoutSizingHorizontal = 'FIXED';
      } else {
        const x = await T(String(cel), i === 0 ? TS.corpoF : TS.corpo, i === 0 ? C.txtPrin : C.txtSec, cols[i].w);
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
async function segmentos(w, opcoes) {
  const seg = AL('HORIZONTAL', 'Alternador', null); seg.itemSpacing = 0; setW(seg, w); rad(seg, R.campo); sv(seg, C.bordaPad, 1);
  const larg = Math.floor(w / opcoes.length);
  for (const [nm, sel] of opcoes) {
    const op = AL('HORIZONTAL', 'Opção', null); op.counterAxisAlignItems = 'CENTER'; op.primaryAxisAlignItems = 'CENTER';
    op.paddingTop = 10; op.paddingBottom = 10; setW(op, larg);
    fv(op, sel ? C.primSuave : C.superficie);
    const x = await T(nm, sel ? TS.corpoF : TS.corpo, sel ? C.txtAcento : C.txtSec); op.appendChild(x);
    seg.appendChild(op); op.layoutSizingHorizontal = 'FILL';
  }
  return seg;
}
const ids = {};

/* ===================== T15 ALERTAS DE ESTOQUE ===================== */
{
  const { s, main } = telaAut('T15 · Alertas de estoque', 0, 5100, 'Gestor Dono', 'Alertas');
  await titulo(main, 'Alertas de estoque', 'O alerta é gerado uma única vez, quando a quantidade cruza a mínima de referência, e não se repete a cada movimentação (RN14).');
  const k = AL('HORIZONTAL', 'Indicadores', E[16]); setW(k, 1128);
  for (const [r, v2, sb] of [['ALERTAS NÃO LIDOS', '5', 'desde 12/03/2026'],
  ['ITENS EM SITUAÇÃO CRÍTICA', '3', 'quantidade igual ou abaixo da mínima'],
  ['ALERTA MAIS ANTIGO', '7 dias', 'Sabonete · 12/03/2026']]) {
    const i = kpiComp.createInstance(); const ts = i.findAll(n => n.type === 'TEXT');
    if (ts[0]) ts[0].characters = r; if (ts[1]) ts[1].characters = v2; if (ts[2]) ts[2].characters = sb;
    k.appendChild(i); i.layoutSizingHorizontal = 'FILL';
  }
  main.appendChild(k); k.layoutSizingHorizontal = 'FILL';
  const c = card(1128, E[12]);
  const ct = await T('Não lidos', TS.cartao, C.txtPrin); c.appendChild(ct);
  const ALERTAS = [
    ['Arroz 5 kg', 'Estoque crítico: Arroz 5 kg está com apenas 2 un.', '27/03/2026'],
    ['Dipirona 500 mg', 'Estoque crítico: Dipirona 500 mg está com apenas 4 cx.', '25/03/2026'],
    ['Leite em pó 400 g', 'Estoque crítico: Leite em pó 400 g está com apenas 6 un.', '21/03/2026'],
    ['Papel higiênico', 'Estoque crítico: Papel higiênico está com apenas 9 un.', '16/03/2026'],
    ['Sabonete', 'Estoque crítico: Sabonete está com apenas 8 un.', '12/03/2026']
  ];
  for (const [item, msg, data] of ALERTAS) {
    const lin = AL('HORIZONTAL', 'Alerta', E[16]); lin.counterAxisAlignItems = 'CENTER';
    pad(lin, E[12], E[16], E[12], E[16]); rad(lin, R.campo); fv(lin, C.criticoSuave); sv(lin, C.critico, 1); setW(lin, 1080);
    const ic = figma.createEllipse(); ic.resize(10, 10); fv(ic, C.criticoTxt); lin.appendChild(ic);
    const col = AL('VERTICAL', 'Texto', E[4]);
    const x1 = await T(msg, TS.corpoF, C.criticoTxt); col.appendChild(x1);
    const x2 = await T(item + ' · gerado em ' + data, TS.apoio, C.criticoTxt); col.appendChild(x2);
    lin.appendChild(col); col.layoutSizingHorizontal = 'FILL';
    const b1 = await btn('Contorno', 'Ver item'); lin.appendChild(b1);
    const b2 = await btn('Secundário', 'Marcar como lido'); lin.appendChild(b2);
    c.appendChild(lin); lin.layoutSizingHorizontal = 'FILL';
  }
  main.appendChild(c); c.layoutSizingHorizontal = 'FILL';
  fechar(s); ids['T15'] = s.id;
}

/* ===================== T16 MEMBROS E SOLICITAÇÕES ===================== */
{
  const { s, main } = telaAut('T16 · Membros e solicitações', 1540, 5100, 'Gestor Dono', 'Membros');
  await titulo(main, 'Membros e solicitações', 'Aprovar ou rejeitar solicitações de vínculo é atribuição exclusiva do Gestor Dono (RF20).');
  const aviso = faixa('Atenção', '3 solicitações aguardam sua decisão. Enquanto pendentes, o solicitante não acessa nenhum dado da instituição (RN04).');
  main.appendChild(aviso); aviso.layoutSizingHorizontal = 'FILL';

  const cPend = card(1128, E[12]);
  const pt = await T('Solicitações pendentes', TS.cartao, C.txtPrin); cPend.appendChild(pt);
  const pTab = await tabela(1080, [{ r: 'SOLICITANTE', w: 230 }, { r: 'E-MAIL', w: 260 }, { r: 'TELEFONE', w: 160 }, { r: 'SOLICITADO EM', w: 160 }, { r: 'DECISÃO', w: 270 }],
    [{ cells: ['Ana Ribeiro', 'ana.ribeiro@exemplo.com', '(31) 98888-1020', '26/03/2026', { acoes: [['Primário', 'Aprovar'], ['Perigo', 'Rejeitar']] }], destaque: 'atencao' },
    { cells: ['Carlos Menezes', 'carlos.m@exemplo.com', '(31) 97777-3040', '24/03/2026', { acoes: [['Primário', 'Aprovar'], ['Perigo', 'Rejeitar']] }], destaque: 'atencao' },
    { cells: ['Patrícia Lopes', 'patricia.l@exemplo.com', '(31) 96666-5060', '20/03/2026', { acoes: [['Primário', 'Aprovar'], ['Perigo', 'Rejeitar']] }], destaque: 'atencao' }]);
  cPend.appendChild(pTab); pTab.layoutSizingHorizontal = 'FILL';
  main.appendChild(cPend); cPend.layoutSizingHorizontal = 'FILL';

  const cMem = card(1128, E[12]);
  const mt = await T('Membros vinculados', TS.cartao, C.txtPrin); cMem.appendChild(mt);
  const mTab = await tabela(1080, [{ r: 'MEMBRO', w: 230 }, { r: 'E-MAIL', w: 260 }, { r: 'PAPEL', w: 160 }, { r: 'SITUAÇÃO', w: 150 }, { r: 'RESPONDIDO EM', w: 150 }, { r: 'AÇÕES', w: 130 }],
    [['Flávio Lara', 'flavio@casasemear.org.br', 'Dono', { etiqueta: 'Aprovado' }, '12/03/2024', '—'],
    ['Marcos Vinícius', 'marcos.v@exemplo.com', 'Funcionário', { etiqueta: 'Aprovado' }, '08/01/2026', 'Revogar'],
    ['Juliana Costa', 'juliana.c@exemplo.com', 'Funcionário', { etiqueta: 'Aprovado' }, '15/11/2025', 'Revogar'],
    ['Rafael Dias', 'rafael.d@exemplo.com', 'Funcionário', { etiqueta: 'Rejeitado' }, '02/02/2026', '—']]);
  cMem.appendChild(mTab); mTab.layoutSizingHorizontal = 'FILL';
  const nota = await T('O vínculo do Gestor Dono não pode ser alterado nem removido (RF21). A revogação de um Funcionário preserva o histórico dos lançamentos que ele registrou (RF23).', TS.apoio, C.txtApoio, 1080);
  cMem.appendChild(nota); nota.layoutSizingHorizontal = 'FILL';
  main.appendChild(cMem); cMem.layoutSizingHorizontal = 'FILL';
  fechar(s); ids['T16'] = s.id;
}

/* ===================== T17 RELATÓRIOS E PRESTAÇÃO DE CONTAS ===================== */
{
  const { s, main } = telaAut('T17 · Relatórios e prestação de contas', 3080, 5100, 'Gestor Dono', 'Relatórios');
  const exp = AL('HORIZONTAL', 'Exportar', E[8]);
  const x1 = await btn('Contorno', 'Exportar CSV'); exp.appendChild(x1);
  const x2 = await btn('Primário', 'Exportar PDF'); exp.appendChild(x2);
  await titulo(main, 'Prestação de contas', 'Demonstrativo consolidado do período, com receitas, despesas, saldo, totais por categoria e valorização dos bens (RF79).', exp);

  const fl = AL('HORIZONTAL', 'Filtros', E[12]); setW(fl, 1128); fl.counterAxisAlignItems = 'MAX';
  const f1 = await campo('Período', '01/03/2026 a 31/03/2026'); fl.appendChild(f1); f1.layoutSizingHorizontal = 'FIXED'; f1.resize(300, f1.height);
  const seg = await segmentos(640, [['Demonstrativo', true], ['Doações', false], ['Despesas', false], ['Estoque', false]]);
  fl.appendChild(seg); seg.layoutSizingHorizontal = 'FILL';
  main.appendChild(fl); fl.layoutSizingHorizontal = 'FILL';

  const k = AL('HORIZONTAL', 'Totais', E[16]); setW(k, 1128);
  for (const [r, v2, sb] of [['RECEITAS DO PERÍODO', 'R$ 18.450,00', '37 doações'],
  ['DESPESAS DO PERÍODO', 'R$ 14.820,00', '23 documentos fiscais'],
  ['SALDO DO PERÍODO', 'R$ 3.630,00', 'receitas menos despesas'],
  ['BENS EM ESTOQUE', 'R$ 7.570,00', 'valorização ao fim do período']]) {
    const i = kpiComp.createInstance(); const ts = i.findAll(n => n.type === 'TEXT');
    if (ts[0]) ts[0].characters = r; if (ts[1]) ts[1].characters = v2; if (ts[2]) ts[2].characters = sb;
    k.appendChild(i); i.layoutSizingHorizontal = 'FILL';
  }
  main.appendChild(k); k.layoutSizingHorizontal = 'FILL';

  const dupla = AL('HORIZONTAL', 'Por categoria', E[16]); setW(dupla, 1128);
  const cR = card(556, E[12]);
  const r1 = await T('Receitas por tipo', TS.cartao, C.txtPrin); cR.appendChild(r1);
  const rTab = await tabela(508, [{ r: 'TIPO', w: 220 }, { r: 'LANÇAMENTOS', w: 140 }, { r: 'TOTAL', w: 148 }],
    [['Financeira', '26', 'R$ 14.900,00'], ['Material', '11', 'R$ 3.550,00'], ['Total geral', '37', 'R$ 18.450,00']]);
  cR.appendChild(rTab); rTab.layoutSizingHorizontal = 'FILL';
  dupla.appendChild(cR); cR.layoutSizingHorizontal = 'FILL';
  const cD = card(556, E[12]);
  const d1 = await T('Despesas por categoria', TS.cartao, C.txtPrin); cD.appendChild(d1);
  const dTab = await tabela(508, [{ r: 'CATEGORIA', w: 220 }, { r: 'LANÇAMENTOS', w: 140 }, { r: 'TOTAL', w: 148 }],
    [['Alimentação', '8', 'R$ 5.632,00'], ['Medicamentos', '5', 'R$ 3.260,00'],
    ['Manutenção', '4', 'R$ 2.519,00'], ['Higiene e limpeza', '4', 'R$ 1.927,00'],
    ['Administrativo', '2', 'R$ 1.482,00'], ['Total geral', '23', 'R$ 14.820,00']]);
  cD.appendChild(dTab); dTab.layoutSizingHorizontal = 'FILL';
  dupla.appendChild(cD); cD.layoutSizingHorizontal = 'FILL';
  main.appendChild(dupla); dupla.layoutSizingHorizontal = 'FILL';

  const cAud = card(1128, E[12]);
  const a1 = await T('Trilha de auditoria do período', TS.cartao, C.txtPrin); cAud.appendChild(a1);
  const a2 = await T('Toda criação, alteração e exclusão de lançamento financeiro ou movimentação de estoque fica registrada com autor, data e operação (RF81).', TS.apoio, C.txtApoio, 1080);
  cAud.appendChild(a2); a2.layoutSizingHorizontal = 'FILL';
  const aTab = await tabela(1080, [{ r: 'DATA E HORA', w: 180 }, { r: 'AUTOR', w: 200 }, { r: 'OPERAÇÃO', w: 160 }, { r: 'ENTIDADE', w: 240 }, { r: 'REGISTRO', w: 300 }],
    [['29/03/2026 14:12', 'Flávio Lara', 'Criação', 'Despesa', 'NF 00124 · R$ 1.120,00'],
    ['28/03/2026 09:40', 'Ana Ribeiro', 'Criação', 'Doação', 'Maria Aparecida · R$ 2.000,00'],
    ['27/03/2026 17:05', 'Flávio Lara', 'Criação', 'Movimentação de estoque', 'Arroz 5 kg · saída de 8 un'],
    ['26/03/2026 11:22', 'Flávio Lara', 'Alteração', 'Despesa', 'NF 00119 · valor corrigido']]);
  cAud.appendChild(aTab); aTab.layoutSizingHorizontal = 'FILL';
  main.appendChild(cAud); cAud.layoutSizingHorizontal = 'FILL';
  fechar(s); ids['T17'] = s.id;
}

/* ===================== T18 CADASTRO INSTITUCIONAL ===================== */
{
  const { s, main } = telaAut('T18 · Cadastro institucional', 4620, 5100, 'Gestor Dono', 'Instituição');
  await titulo(main, 'Cadastro institucional', 'Edição restrita ao Gestor Dono. Estes dados alimentam a página pública da instituição (RF25, RF27).');
  const colunas = AL('HORIZONTAL', 'Colunas', E[16]); setW(colunas, 1128);

  const cForm = card(720, E[16]);
  const n1 = await campo('Nome da instituição', 'Casa de Apoio Semear'); cForm.appendChild(n1); n1.layoutSizingHorizontal = 'FILL';
  const d1 = await campo('Descrição', 'Acolhimento de pessoas idosas em situação de vulnerabilidade social.'); cForm.appendChild(d1); d1.layoutSizingHorizontal = 'FILL';
  const g1 = AL('HORIZONTAL', 'Grade', E[16]); setW(g1, 672);
  const c1 = await campo('Cidade', 'Betim'); g1.appendChild(c1); c1.layoutSizingHorizontal = 'FILL';
  const c2 = await campo('Estado', 'MG'); g1.appendChild(c2); c2.layoutSizingHorizontal = 'FIXED'; c2.resize(160, c2.height);
  cForm.appendChild(g1); g1.layoutSizingHorizontal = 'FILL';
  const g2 = AL('HORIZONTAL', 'Grade', E[16]); setW(g2, 672);
  const c3 = await campo('E-mail de contato', 'contato@casasemear.org.br'); g2.appendChild(c3); c3.layoutSizingHorizontal = 'FILL';
  const c4 = await campo('Telefone', '(31) 3532-0000'); g2.appendChild(c4); c4.layoutSizingHorizontal = 'FILL';
  cForm.appendChild(g2); g2.layoutSizingHorizontal = 'FILL';
  const st = await campo('Site', 'casasemear.org.br'); cForm.appendChild(st); st.layoutSizingHorizontal = 'FILL';
  const nf = await campo('Nome já utilizado por outra instituição', 'Casa de Apoio Semear', 'Erro'); cForm.appendChild(nf); nf.layoutSizingHorizontal = 'FILL';
  const ac = AL('HORIZONTAL', 'Ações', E[8]); setW(ac, 672); ac.primaryAxisAlignItems = 'MAX';
  const b1 = await btn('Contorno', 'Descartar alterações'); ac.appendChild(b1);
  const b2 = await btn('Primário', 'Salvar alterações'); ac.appendChild(b2);
  cForm.appendChild(ac); ac.layoutSizingHorizontal = 'FILL';
  colunas.appendChild(cForm); cForm.layoutSizingHorizontal = 'FILL';

  const cPrev = card(392, E[12]);
  const pt = await T('Como aparece ao público', TS.cartao, C.txtPrin); cPrev.appendChild(pt);
  const pn = await T('Casa de Apoio Semear', TS.secao, C.txtPrin, 344); cPrev.appendChild(pn); pn.layoutSizingHorizontal = 'FILL';
  const pl = await T('Betim, MG', TS.apoio, C.txtApoio); cPrev.appendChild(pl);
  const pd = await T('Acolhimento de pessoas idosas em situação de vulnerabilidade social.', TS.corpo, C.txtSec, 344);
  cPrev.appendChild(pd); pd.layoutSizingHorizontal = 'FILL';
  const pb = await btn('Secundário', 'Ver página pública'); cPrev.appendChild(pb); pb.layoutSizingHorizontal = 'FILL';
  const pn2 = await T('O nome da instituição é único no sistema; nomes duplicados são recusados (RF13).', TS.apoio, C.txtApoio, 344);
  cPrev.appendChild(pn2); pn2.layoutSizingHorizontal = 'FILL';
  colunas.appendChild(cPrev); cPrev.layoutSizingHorizontal = 'FIXED';
  main.appendChild(colunas); colunas.layoutSizingHorizontal = 'FILL';
  fechar(s); ids['T18'] = s.id;
}

/* ===================== T19 CATEGORIAS ===================== */
{
  const acao = await btn('Primário', 'Nova categoria');
  const { s, main } = telaAut('T19 · Categorias', 6160, 5100, 'Gestor Dono', 'Estoque');
  await titulo(main, 'Categorias', 'Uma única entidade classifica receitas, despesas e itens de estoque (RF45, RF47).', acao);
  const seg = await segmentos(600, [['Despesa', true], ['Receita', false], ['Estoque', false]]);
  main.appendChild(seg); seg.layoutSizingHorizontal = 'FIXED';
  const c = card(1128, E[12]);
  const tb = await tabela(1080, [{ r: 'CATEGORIA', w: 300 }, { r: 'TIPO', w: 160 }, { r: 'ORIGEM', w: 200 }, { r: 'LANÇAMENTOS', w: 180 }, { r: 'AÇÕES', w: 240 }],
    [['Alimentação', 'Despesa', 'Padrão do sistema', '8 no período', 'Editar'],
    ['Medicamentos', 'Despesa', 'Padrão do sistema', '5 no período', 'Editar'],
    ['Manutenção', 'Despesa', 'Padrão do sistema', '4 no período', 'Editar'],
    ['Higiene e limpeza', 'Despesa', 'Padrão do sistema', '4 no período', 'Editar'],
    ['Administrativo', 'Despesa', 'Padrão do sistema', '2 no período', 'Editar'],
    ['Transporte de doações', 'Despesa', 'Própria da instituição', 'Sem lançamentos', 'Editar · Excluir']]);
  c.appendChild(tb); tb.layoutSizingHorizontal = 'FILL';
  const nota = faixa('Informação', 'Categoria utilizada por algum lançamento não pode ser excluída; apenas as sem uso exibem a ação Excluir (RF46).');
  c.appendChild(nota); nota.layoutSizingHorizontal = 'FILL';
  main.appendChild(c); c.layoutSizingHorizontal = 'FILL';
  const vz = vazioComp.createInstance();
  const vzt = vz.findAll(n => n.type === 'TEXT');
  if (vzt[0]) vzt[0].characters = 'Nenhuma categoria de receita cadastrada';
  if (vzt[1]) vzt[1].characters = 'Crie a primeira categoria para classificar as doações recebidas.';
  main.appendChild(vz); vz.layoutSizingHorizontal = 'FIXED';
  fechar(s); ids['T19'] = s.id;
}

const bounds = [];
for (const [k, id] of Object.entries(ids)) { const n = await figma.getNodeByIdAsync(id); bounds.push({ tela: k, w: n.width, h: n.height, desc: n.findAll(() => true).length }); }
return { createdNodeIds: ids, bounds };
