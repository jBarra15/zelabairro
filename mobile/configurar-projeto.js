/**
 * Rode UMA vez, depois de copiar estes arquivos para dentro do projeto Expo:
 *   node configurar-projeto.js
 * Ajusta package.json (authors, script typecheck) e app.json (nome e textos de permissão).
 */
const fs = require('fs');
const path = require('path');

const pkgPath = path.join(__dirname, 'package.json');
const pkg = JSON.parse(fs.readFileSync(pkgPath, 'utf8'));
pkg.name = 'zelabairro-mobile';
pkg.authors = [{ name: 'Joel NOME COMPLETO', rm: 'RM560796' }];
pkg.scripts = { ...pkg.scripts, typecheck: 'tsc --noEmit' };
fs.writeFileSync(pkgPath, JSON.stringify(pkg, null, 2) + '\n');

const appPath = path.join(__dirname, 'app.json');
const app = JSON.parse(fs.readFileSync(appPath, 'utf8'));
app.expo.name = 'ZelaBairro';
app.expo.slug = 'zelabairro';
const nomePlugin = (p) => (Array.isArray(p) ? p[0] : p);
const plugins = (app.expo.plugins || []).filter((p) => !['expo-image-picker', 'expo-location'].includes(nomePlugin(p)));
plugins.push(['expo-image-picker', { cameraPermission: 'O ZelaBairro usa a câmera para você fotografar o problema do bairro.' }]);
plugins.push(['expo-location', { locationWhenInUsePermission: 'O ZelaBairro usa sua localização para marcar onde está o problema.' }]);
app.expo.plugins = plugins;
fs.writeFileSync(appPath, JSON.stringify(app, null, 2) + '\n');

console.log('OK: package.json e app.json configurados. Troque "NOME COMPLETO" no package.json.');
