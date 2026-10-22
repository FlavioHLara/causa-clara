import fs from 'fs';
const estado = JSON.parse(fs.readFileSync('../figma-prototipo-estado.json','utf8'));
const conhecidos = new Set([
  ...Object.values(estado.entities.pages),
  ...Object.values(estado.entities.components),
  ...Object.values(estado.entities.telas),
  ...Object.values(estado.entities.cor),
  ...Object.values(estado.entities.esp),
  ...Object.values(estado.entities.raio),
  ...Object.values(estado.entities.textStyles),
]);
const ALIGN_OK = new Set(['MIN','MAX','CENTER','BASELINE','SPACE_BETWEEN']);
const SIZING_MODE_OK = new Set(['FIXED','AUTO']);
const LAYOUT_SIZING_OK = new Set(['FIXED','HUG','FILL']);
let falhas = 0;
for (const arq of process.argv.slice(2)) {
  const src = fs.readFileSync(arq,'utf8');
  const prob = [];
  for (const m of src.matchAll(/(counterAxisAlignItems|primaryAxisAlignItems)\s*=\s*'([A-Z_]+)'/g))
    if (!ALIGN_OK.has(m[2])) prob.push(`${m[1]} = '${m[2]}' inválido (use MIN/MAX/CENTER/BASELINE)`);
  for (const m of src.matchAll(/(counterAxisSizingMode|primaryAxisSizingMode)\s*=\s*'([A-Z_]+)'/g))
    if (!SIZING_MODE_OK.has(m[2])) prob.push(`${m[1]} = '${m[2]}' inválido (use FIXED/AUTO)`);
  for (const m of src.matchAll(/layoutSizing(?:Horizontal|Vertical)\s*=\s*'([A-Z_]+)'/g))
    if (!LAYOUT_SIZING_OK.has(m[1])) prob.push(`layoutSizing = '${m[1]}' inválido (use FIXED/HUG/FILL)`);
  // dasharray só quebra em circle/ellipse (caso da rosca); em polyline/path funciona
  for (const m of src.matchAll(/<(circle|ellipse)[^>]*stroke-dasharray/g)) prob.push('stroke-dasharray em <' + m[1] + '>: o Figma fragmenta o traço — usar setores de arco');
  if (/figma\.notify/.test(src)) prob.push('figma.notify lança erro — remover');
  if (/loadAllPagesAsync|setPluginData|createImageAsync/.test(src)) prob.push('API não suportada pelo use_figma');
  const switches = [...src.matchAll(/setCurrentPageAsync/g)].length;
  if (switches > 1) prob.push(`setCurrentPageAsync chamado ${switches}x — permitido no máximo 1 por chamada`);
  // IDs de nós referenciados
  for (const m of src.matchAll(/getNodeByIdAsync\('([^']+)'\)/g))
    if (!conhecidos.has(m[1])) prob.push(`nó ${m[1]} não consta no registro de estado`);
  for (const m of src.matchAll(/V\('(VariableID:[^']+)'\)/g))
    if (!conhecidos.has(m[1])) prob.push(`variável ${m[1]} não consta no registro de estado`);
  for (const m of src.matchAll(/'(S:[0-9a-f]+,)'/g))
    if (!conhecidos.has(m[1])) prob.push(`estilo de texto ${m[1]} não consta no registro`);
  const unicos = [...new Set(prob)];
  console.log(`\n${arq}: ${unicos.length ? unicos.length + ' PROBLEMA(S)' : 'limpo'}`);
  unicos.forEach(p => console.log('   - ' + p));
  falhas += unicos.length;
}
console.log(falhas ? '\nRESULTADO: corrigir antes de gastar cota' : '\nRESULTADO: ambos prontos para disparar');
