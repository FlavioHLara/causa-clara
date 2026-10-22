// Lote C — versões móveis em 360 px na página "Telas · Móvel" (2:62).
// Cinco telas que cobrem os dois fluxos de entrada, o painel público, o registro
// de doação e o controle de estoque — suficiente para demonstrar o RNF01.
// Padrão responsivo: tabelas viram cartões empilhados, não tabelas espremidas.

const page = await figma.getNodeByIdAsync('2:62');
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
  16: await V('VariableID:2:45'), 24: await V('VariableID:2:46'), 32: await V('VariableID:2:47')
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
function card(w, g) { const f = AL('VERTICAL', 'Cartão', g || E[12]); fv(f, C.superficie); sv(f, C.bordaPad, 1); rad(f, R.cartao); pad(f, E[16], E[16], E[16], E[16]); if (w) setW(f, w); return f; }

const btnSet = await figma.getNodeByIdAsync('3:18'), campoSet = await figma.getNodeByIdAsync('3:32'),
  etqSet = await figma.getNodeByIdAsync('3:51'), fxSet = await figma.getNodeByIdAsync('4:22');
async function btn(t, l, e) { const i = btnSet.children.find(c => c.name === `Tipo=${t}, Estado=${e || 'Normal'}`).createInstance(); const x = i.findOne(n => n.type === 'TEXT'); if (x) x.characters = l; return i; }
async function campo(r, v2) { const i = campoSet.children.find(c => c.name === 'Estado=Normal').createInstance(); const ts = i.findAll(n => n.type === 'TEXT'); if (ts[0]) ts[0].characters = r; if (ts[1]) ts[1].characters = v2; return i; }
function etq(t) { return etqSet.children.find(c => c.name === `Tipo=${t}`).createInstance(); }
function faixa(t, msg) { const i = fxSet.children.find(c => c.name === `Tipo=${t}`).createInstance(); const x = i.findOne(n => n.type === 'TEXT'); if (x) x.characters = msg; return i; }

const LARG = 360, INT = 328; // largura da tela e do conteúdo com 16 px de margem (RNF01)
function telaM(nome, x) {
  const s = AL('VERTICAL', nome); s.name = nome;
  s.resize(LARG, 1); s.counterAxisSizingMode = 'FIXED'; s.primaryAxisSizingMode = 'AUTO';
  s.itemSpacing = 0; s.clipsContent = true; fv(s, C.fundoApp); s.x = x; s.y = 0; page.appendChild(s);
  return s;
}
function fechar(s) { if (s.height < 800) { s.primaryAxisSizingMode = 'FIXED'; s.resize(LARG, 800); } }
// barra superior do app em tela estreita: marca à esquerda, menu sanduíche à direita
async function topo(s, titulo2) {
  const t = AL('HORIZONTAL', 'Barra superior', E[12]); t.counterAxisAlignItems = 'CENTER';
  pad(t, E[16], E[16], E[16], E[16]); fv(t, C.superficie); sv(t, C.bordaPad, 1); setW(t, LARG);
  const m = await T(titulo2 || 'Semear', TS.cartao, C.txtAcento); t.appendChild(m); m.layoutSizingHorizontal = 'FILL';
  const menu = AL('VERTICAL', 'Menu', null); menu.itemSpacing = 4;
  for (let i = 0; i < 3; i++) { const b = figma.createRectangle(); b.resize(18, 2); b.cornerRadius = 1; fv(b, C.txtSec); menu.appendChild(b); }
  t.appendChild(menu);
  s.appendChild(t); t.layoutSizingHorizontal = 'FILL';
  return t;
}
function corpoM(s) {
  const c = AL('VERTICAL', 'Corpo', E[16]); pad(c, E[16], E[16], E[32], E[16]); setW(c, LARG);
  s.appendChild(c); c.layoutSizingHorizontal = 'FILL'; return c;
}
// indicador compacto: rótulo acima, valor grande abaixo
async function kpiM(rotulo, valor, apoio) {
  const k = card(INT, E[4]);
  const a = await T(rotulo, TS.rotulo, C.txtApoio); k.appendChild(a); a.layoutSizingHorizontal = 'FILL';
  const b = await T(valor, TS.indicador, C.txtPrin); k.appendChild(b); b.layoutSizingHorizontal = 'FILL';
  if (apoio) { const c = await T(apoio, TS.apoio, C.txtSec); k.appendChild(c); c.layoutSizingHorizontal = 'FILL'; }
  return k;
}
// linha de dado em cartão, substituindo coluna de tabela
async function dado(pai, rotulo, valor, cor) {
  const l = AL('HORIZONTAL', 'Dado', E[8]); l.counterAxisAlignItems = 'CENTER'; setW(l, INT - 32);
  const a = await T(rotulo, TS.apoio, C.txtApoio); l.appendChild(a); a.layoutSizingHorizontal = 'FILL';
  const b = await T(valor, TS.corpoF, cor || C.txtPrin); l.appendChild(b);
  pai.appendChild(l); l.layoutSizingHorizontal = 'FILL'; return l;
}
const ids = {};

/* ===================== M01 ACESSO ===================== */
{
  const s = telaM('M01 · Acesso (360 px)', 0);
  const c = corpoM(s);
  const esp = figma.createFrame(); esp.name = 'Espaço'; esp.resize(INT, 48); esp.fills = []; c.appendChild(esp); esp.layoutSizingHorizontal = 'FILL';
  const cab = AL('VERTICAL', 'Marca', E[4]); cab.counterAxisAlignItems = 'CENTER'; setW(cab, INT);
  const m = await T('Semear', TS.tela, C.txtAcento); cab.appendChild(m);
  const sub = await T('Gestão e transparência para o terceiro setor', TS.apoio, C.txtSec, INT);
  cab.appendChild(sub); sub.layoutSizingHorizontal = 'FILL';
  c.appendChild(cab); cab.layoutSizingHorizontal = 'FILL';
  const cc = card(INT, E[12]);
  for (const [r, v2] of [['E-mail', 'gestor@casasemear.org.br'], ['Senha', '••••••••']]) {
    const f = await campo(r, v2); cc.appendChild(f); f.layoutSizingHorizontal = 'FILL';
  }
  const b1 = await btn('Primário', 'Entrar'); cc.appendChild(b1); b1.layoutSizingHorizontal = 'FILL';
  const lk = await T('Esqueci minha senha', TS.apoio, C.txtAcento); cc.appendChild(lk); lk.layoutSizingHorizontal = 'FILL';
  const div = figma.createFrame(); div.name = 'Divisor'; div.resize(INT - 32, 1); fv(div, C.bordaPad); cc.appendChild(div); div.layoutSizingHorizontal = 'FILL';
  const b2 = await btn('Contorno', 'Criar conta'); cc.appendChild(b2); b2.layoutSizingHorizontal = 'FILL';
  c.appendChild(cc); cc.layoutSizingHorizontal = 'FILL';
  const pub = AL('VERTICAL', 'Acesso público', E[4]); pub.counterAxisAlignItems = 'CENTER'; setW(pub, INT);
  const p1 = await T('Consultar transparência sem entrar', TS.corpoF, C.txtAcento); pub.appendChild(p1);
  c.appendChild(pub); pub.layoutSizingHorizontal = 'FILL';
  fechar(s); ids['M01'] = s.id;
}

/* ===================== M05 TRANSPARÊNCIA ===================== */
{
  const s = telaM('M05 · Transparência (360 px)', 460);
  await topo(s);
  const c = corpoM(s);
  const cab = AL('VERTICAL', 'Títulos', E[4]); setW(cab, INT);
  const t1 = await T('Transparência', TS.tela, C.txtPrin); cab.appendChild(t1);
  const t2 = await T('Casa de Apoio Semear · março de 2026', TS.corpo, C.txtSec, INT); cab.appendChild(t2); t2.layoutSizingHorizontal = 'FILL';
  c.appendChild(cab); cab.layoutSizingHorizontal = 'FILL';
  // indicadores empilhados em duas colunas de meia largura
  const g = AL('HORIZONTAL', 'Indicadores', E[12]); setW(g, INT);
  for (const [r, v2] of [['ARRECADADO', 'R$ 18.450'], ['DESPESAS', 'R$ 14.820']]) {
    const k = card(158, E[4]);
    const a = await T(r, TS.rotulo, C.txtApoio); k.appendChild(a); a.layoutSizingHorizontal = 'FILL';
    const b = await T(v2, TS.secao, C.txtPrin); k.appendChild(b); b.layoutSizingHorizontal = 'FILL';
    g.appendChild(k); k.layoutSizingHorizontal = 'FILL';
  }
  c.appendChild(g); g.layoutSizingHorizontal = 'FILL';
  const saldo = await kpiM('SALDO DO PERÍODO', 'R$ 3.630,00', '82% aplicado em atividades-fim');
  c.appendChild(saldo); saldo.layoutSizingHorizontal = 'FILL';
  // gráfico reduzido para a largura estreita
  const cg = card(INT, E[8]);
  const gt = await T('Receitas e despesas por mês', TS.cartao, C.txtPrin); cg.appendChild(gt);
  const svgM = `<svg width="296" height="150" viewBox="0 0 296 150" xmlns="http://www.w3.org/2000/svg">
<g stroke="#D7DBE2" stroke-width="1"><line x1="4" y1="10" x2="292" y2="10"/><line x1="4" y1="55" x2="292" y2="55"/><line x1="4" y1="100" x2="292" y2="100"/></g>
<polyline points="12,72 68,56 124,88 180,61 236,44 288,30" fill="none" stroke="#2F6F4F" stroke-width="2.5" stroke-linejoin="round" stroke-linecap="round"/>
<polyline points="12,86 68,78 124,94 180,74 236,66 288,56" fill="none" stroke="#B4530B" stroke-width="2.5" stroke-dasharray="5 4" stroke-linejoin="round" stroke-linecap="round"/>
<g font-family="Inter" font-size="9" fill="#7B8392"><text x="4" y="124">out</text><text x="60" y="124">nov</text><text x="116" y="124">dez</text><text x="172" y="124">jan</text><text x="228" y="124">fev</text><text x="276" y="124">mar</text></g></svg>`;
  const gv = figma.createNodeFromSvg(svgM); gv.name = 'Gráfico mensal'; gv.resize(296, 150);
  cg.appendChild(gv);
  c.appendChild(cg); cg.layoutSizingHorizontal = 'FILL';
  // doações como cartões empilhados, não tabela
  const cd = card(INT, E[12]);
  const dt = await T('Doações recebidas', TS.cartao, C.txtPrin); cd.appendChild(dt);
  for (const [nome, tipo, valor, data] of [['Maria', 'Financeira', 'R$ 2.000,00', '28/03'], ['Anônimo', 'Financeira', 'R$ 1.500,00', '26/03'], ['João', 'Material', 'R$ 840,00', '24/03']]) {
    const it = AL('VERTICAL', 'Doação', E[4]); pad(it, E[12], E[12], E[12], E[12]);
    rad(it, R.campo); sv(it, C.bordaPad, 1); setW(it, INT - 32);
    const l1 = AL('HORIZONTAL', 'Linha', E[8]); l1.counterAxisAlignItems = 'CENTER'; setW(l1, INT - 56);
    const n1 = await T(nome, TS.corpoF, C.txtPrin); l1.appendChild(n1); n1.layoutSizingHorizontal = 'FILL';
    const n2 = await T(valor, TS.corpoF, C.txtAcento); l1.appendChild(n2);
    it.appendChild(l1); l1.layoutSizingHorizontal = 'FILL';
    const n3 = await T(tipo + ' · ' + data, TS.apoio, C.txtApoio); it.appendChild(n3);
    cd.appendChild(it); it.layoutSizingHorizontal = 'FILL';
  }
  const nota = await T('Apenas o primeiro nome é divulgado, ou Anônimo (RF66).', TS.apoio, C.txtApoio, INT - 32);
  cd.appendChild(nota); nota.layoutSizingHorizontal = 'FILL';
  c.appendChild(cd); cd.layoutSizingHorizontal = 'FILL';
  fechar(s); ids['M05'] = s.id;
}

/* ===================== M07 REGISTRAR DOAÇÃO ===================== */
{
  const s = telaM('M07 · Registrar doação (360 px)', 920);
  await topo(s, 'Registrar doação');
  const c = corpoM(s);
  const cc = card(INT, E[12]);
  const inst = await campo('Instituição', 'Casa de Apoio Semear'); cc.appendChild(inst); inst.layoutSizingHorizontal = 'FILL';
  const tl = await T('Tipo de doação', TS.apoio, C.txtSec); cc.appendChild(tl);
  const seg = AL('HORIZONTAL', 'Alternador', null); seg.itemSpacing = 0; setW(seg, INT - 32); rad(seg, R.campo); sv(seg, C.bordaPad, 1);
  for (const [nm, sel] of [['Financeira', true], ['Material', false]]) {
    const op = AL('HORIZONTAL', 'Opção', null); op.counterAxisAlignItems = 'CENTER'; op.primaryAxisAlignItems = 'CENTER';
    op.paddingTop = 10; op.paddingBottom = 10; setW(op, (INT - 32) / 2);
    fv(op, sel ? C.primSuave : C.superficie);
    const x = await T(nm, sel ? TS.corpoF : TS.corpo, sel ? C.txtAcento : C.txtSec); op.appendChild(x);
    seg.appendChild(op); op.layoutSizingHorizontal = 'FILL';
  }
  cc.appendChild(seg); seg.layoutSizingHorizontal = 'FILL';
  const vl = await campo('Valor da doação', 'R$ 2.000,00'); cc.appendChild(vl); vl.layoutSizingHorizontal = 'FILL';
  const ds = await campo('Descrição (opcional)', 'Contribuição mensal'); cc.appendChild(ds); ds.layoutSizingHorizontal = 'FILL';
  const an = AL('HORIZONTAL', 'Anonimato', E[8]); an.counterAxisAlignItems = 'CENTER';
  pad(an, E[12], E[12], E[12], E[12]); rad(an, R.campo); fv(an, C.entradaSuave); setW(an, INT - 32);
  const cb = figma.createRectangle(); cb.resize(18, 18); cb.cornerRadius = 4; fv(cb, C.prim); an.appendChild(cb);
  const at = await T('Divulgar como Anônimo', TS.corpoF, C.entradaTxt); an.appendChild(at); at.layoutSizingHorizontal = 'FILL';
  cc.appendChild(an); an.layoutSizingHorizontal = 'FILL';
  const b2 = await btn('Primário', 'Confirmar doação'); cc.appendChild(b2); b2.layoutSizingHorizontal = 'FILL';
  c.appendChild(cc); cc.layoutSizingHorizontal = 'FILL';
  const nt = faixa('Informação', 'Registro declaratório: o sistema publica a doação informada, sem intermediar a transferência.');
  c.appendChild(nt); nt.layoutSizingHorizontal = 'FILL';
  fechar(s); ids['M07'] = s.id;
}

/* ===================== M10 PAINEL ADMINISTRATIVO ===================== */
{
  const s = telaM('M10 · Painel administrativo (360 px)', 1380);
  await topo(s, 'Painel');
  const c = corpoM(s);
  const cab = AL('VERTICAL', 'Títulos', E[4]); setW(cab, INT);
  const t1 = await T('Painel administrativo', TS.tela, C.txtPrin); cab.appendChild(t1);
  const t2 = await T('Casa de Apoio Semear · março de 2026', TS.apoio, C.txtApoio); cab.appendChild(t2);
  c.appendChild(cab); cab.layoutSizingHorizontal = 'FILL';
  const al = faixa('Crítico', '5 alertas de estoque não lidos'); c.appendChild(al); al.layoutSizingHorizontal = 'FILL';
  const g = AL('HORIZONTAL', 'Indicadores', E[12]); setW(g, INT);
  for (const [r, v2] of [['ARRECADADO', 'R$ 18.450'], ['DESPESAS', 'R$ 14.820']]) {
    const k = card(158, E[4]);
    const a = await T(r, TS.rotulo, C.txtApoio); k.appendChild(a); a.layoutSizingHorizontal = 'FILL';
    const b = await T(v2, TS.secao, C.txtPrin); k.appendChild(b); b.layoutSizingHorizontal = 'FILL';
    g.appendChild(k); k.layoutSizingHorizontal = 'FILL';
  }
  c.appendChild(g); g.layoutSizingHorizontal = 'FILL';
  const cc = card(INT, E[12]);
  const ct = await T('Itens em quantidade crítica', TS.cartao, C.txtPrin); cc.appendChild(ct);
  for (const [item, cat, atual, min] of [['Arroz 5 kg', 'Alimentos básicos', '2 un', '10 un'],
  ['Dipirona 500 mg', 'Medicamentos', '4 cx', '15 cx'], ['Sabonete', 'Higiene e limpeza', '8 un', '20 un']]) {
    const it = AL('VERTICAL', 'Item', E[8]); pad(it, E[12], E[12], E[12], E[12]);
    rad(it, R.campo); fv(it, C.criticoSuave); sv(it, C.critico, 1); setW(it, INT - 32);
    const l1 = AL('HORIZONTAL', 'Linha', E[8]); l1.counterAxisAlignItems = 'CENTER'; setW(l1, INT - 56);
    const n1 = await T(item, TS.corpoF, C.criticoTxt); l1.appendChild(n1); n1.layoutSizingHorizontal = 'FILL';
    l1.appendChild(etq('Crítico'));
    it.appendChild(l1); l1.layoutSizingHorizontal = 'FILL';
    const n2 = await T(cat, TS.apoio, C.criticoTxt); it.appendChild(n2);
    await dado(it, 'Quantidade atual', atual, C.criticoTxt);
    await dado(it, 'Quantidade mínima', min, C.criticoTxt);
    cc.appendChild(it); it.layoutSizingHorizontal = 'FILL';
  }
  c.appendChild(cc); cc.layoutSizingHorizontal = 'FILL';
  fechar(s); ids['M10'] = s.id;
}

/* ===================== M13 ITENS DE ESTOQUE ===================== */
{
  const s = telaM('M13 · Itens de estoque (360 px)', 1840);
  await topo(s, 'Estoque');
  const c = corpoM(s);
  const cab = AL('VERTICAL', 'Títulos', E[4]); setW(cab, INT);
  const t1 = await T('Itens de estoque', TS.tela, C.txtPrin); cab.appendChild(t1);
  const t2 = await T('40 itens · 3 em situação crítica', TS.apoio, C.txtApoio); cab.appendChild(t2);
  c.appendChild(cab); cab.layoutSizingHorizontal = 'FILL';
  const bs = await campo('Buscar por nome', 'Arroz'); c.appendChild(bs); bs.layoutSizingHorizontal = 'FILL';
  const LISTA = [
    ['Arroz 5 kg', 'Alimentos básicos', '2 un', '10 un', 'R$ 56,00', true],
    ['Dipirona 500 mg', 'Medicamentos', '4 cx', '15 cx', 'R$ 48,00', true],
    ['Feijão 1 kg', 'Alimentos básicos', '18 un', '12 un', 'R$ 162,00', false],
    ['Fralda geriátrica', 'Higiene e limpeza', '34 un', '20 un', 'R$ 1.020,00', false]
  ];
  for (const [item, cat, atual, min, val, critico] of LISTA) {
    const it = card(INT, E[8]);
    if (critico) { fv(it, C.criticoSuave); sv(it, C.critico, 1); }
    const l1 = AL('HORIZONTAL', 'Linha', E[8]); l1.counterAxisAlignItems = 'CENTER'; setW(l1, INT - 32);
    const n1 = await T(item, TS.cartao, critico ? C.criticoTxt : C.txtPrin); l1.appendChild(n1); n1.layoutSizingHorizontal = 'FILL';
    l1.appendChild(etq(critico ? 'Crítico' : 'Aprovado'));
    it.appendChild(l1); l1.layoutSizingHorizontal = 'FILL';
    const n2 = await T(cat, TS.apoio, C.txtApoio); it.appendChild(n2);
    await dado(it, 'Quantidade atual', atual, critico ? C.criticoTxt : C.txtPrin);
    await dado(it, 'Quantidade mínima', min);
    await dado(it, 'Valorização', val);
    const b = await btn(critico ? 'Primário' : 'Contorno', 'Movimentar'); it.appendChild(b); b.layoutSizingHorizontal = 'FILL';
    c.appendChild(it); it.layoutSizingHorizontal = 'FILL';
  }
  fechar(s); ids['M13'] = s.id;
}

const bounds = [];
for (const [k, id] of Object.entries(ids)) { const n = await figma.getNodeByIdAsync(id); bounds.push({ tela: k, w: n.width, h: n.height, desc: n.findAll(() => true).length }); }
return { createdNodeIds: ids, bounds };
