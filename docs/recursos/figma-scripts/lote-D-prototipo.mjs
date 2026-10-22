// Lote D — ligações de protótipo, conforme o mapa de navegação da seção 3 de
// 03-prototipo-especificacao.md.
//
// Localiza as telas por NOME, não por ID, justamente para rodar depois dos lotes
// A e B sem depender dos IDs que eles geram.

const page = await figma.getNodeByIdAsync('2:61');
await figma.setCurrentPageAsync(page);

const telas = {};
for (const c of page.children) {
  const m = c.name.match(/^(T\d\d)/);
  if (m) telas[m[1]] = c;
}

// [tela de origem, texto do elemento clicável, tela de destino]
const LIGACOES = [
  ['T01', 'Criar conta', 'T02'],
  ['T01', 'Consultar transparência sem entrar', 'T05'],
  ['T01', 'Entrar', 'T06'],
  ['T02', 'Voltar', 'T01'],
  ['T02', 'Criar conta', 'T01'],
  ['T03', 'Ver transparência', 'T05'],
  ['T03', 'Doar', 'T07'],
  ['T03', 'Instituições', 'T03'],
  ['T04', 'Ver painel de transparência', 'T05'],
  ['T04', 'Registrar doação', 'T07'],
  ['T05', 'Instituições', 'T03'],
  ['T06', 'Registrar nova doação', 'T07'],
  ['T06', 'Minhas doações', 'T08'],
  ['T06', 'Instituições', 'T03'],
  ['T07', 'Cancelar', 'T06'],
  ['T07', 'Confirmar doação', 'T08'],
  ['T08', 'Meu painel', 'T06'],
  ['T09', 'Criar instituição', 'T18'],
  ['T09', 'Solicitar acesso', 'T09'],
  ['T10', 'Estoque', 'T13'],
  ['T10', 'Alertas', 'T15'],
  ['T10', 'Membros', 'T16'],
  ['T10', 'Doações recebidas', 'T11'],
  ['T10', 'Despesas', 'T12'],
  ['T10', 'Relatórios', 'T17'],
  ['T10', 'Instituição', 'T18'],
  ['T11', 'Painel administrativo', 'T10'],
  ['T12', 'Painel administrativo', 'T10'],
  ['T13', 'Movimentar', 'T14'],
  ['T13', 'Painel administrativo', 'T10'],
  ['T14', 'Estoque', 'T13'],
  ['T15', 'Ver item', 'T14'],
  ['T15', 'Painel administrativo', 'T10'],
  ['T16', 'Painel administrativo', 'T10'],
  ['T17', 'Painel administrativo', 'T10'],
  ['T18', 'Ver página pública', 'T04'],
  ['T19', 'Despesas', 'T12']
];

function alvoClicavel(telaNode, texto) {
  // preferir instância de botão ou item de navegação cujo texto bate
  const instancias = telaNode.findAllWithCriteria({ types: ['INSTANCE'] });
  for (const i of instancias) {
    const t = i.findOne(n => n.type === 'TEXT');
    if (t && t.characters.trim() === texto) return i;
  }
  // senão, o próprio nó de texto
  const txt = telaNode.findAll(n => n.type === 'TEXT' && n.characters.trim() === texto);
  return txt.length ? txt[0] : null;
}

const feitas = [], faltando = [];
for (const [de, rotulo, para] of LIGACOES) {
  const origem = telas[de], destino = telas[para];
  if (!origem || !destino) { faltando.push(`${de}→${para}: tela ausente`); continue; }
  const no = alvoClicavel(origem, rotulo);
  if (!no) { faltando.push(`${de}→${para}: não achei "${rotulo}"`); continue; }
  const anteriores = no.reactions ? no.reactions.slice() : [];
  const jaTem = anteriores.some(r => r.action && r.action.destinationId === destino.id);
  if (jaTem) { feitas.push(`${de}→${para} (já existia)`); continue; }
  await no.setReactionsAsync([...anteriores, {
    trigger: { type: 'ON_CLICK' },
    actions: [{
      type: 'NODE',
      destinationId: destino.id,
      navigation: 'NAVIGATE',
      transition: { type: 'SMART_ANIMATE', easing: { type: 'EASE_OUT' }, duration: 0.3 },
      preserveScrollPosition: false
    }]
  }]);
  feitas.push(`${de}→${para} via "${rotulo}"`);
}

return {
  telasEncontradas: Object.keys(telas).sort(),
  ligacoesCriadas: feitas.length,
  detalhe: feitas,
  naoResolvidas: faltando
};
