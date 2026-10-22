import fs from 'fs';
const alvo = process.argv[2];
const src = fs.readFileSync(alvo, 'utf8');
// o Figma embrulha o code em um contexto async; replicamos para validar sintaxe
const envolto = `export default async function(){\n${src}\n}`;
fs.writeFileSync(alvo + '.check.mjs', envolto);
try {
  await import('node:url').then(async () => {
    const { pathToFileURL } = await import('node:url');
    // import dinâmico só valida sintaxe porque a função não é executada
    await import(pathToFileURL(alvo + '.check.mjs').href);
  });
  console.log('SINTAXE OK —', alvo);
  console.log('bytes:', Buffer.byteLength(src, 'utf8'), '(limite do parâmetro code: 50000)');
} catch (e) {
  console.log('ERRO DE SINTAXE —', alvo);
  console.log(e.message);
  process.exitCode = 1;
} finally {
  fs.unlinkSync(alvo + '.check.mjs');
}
