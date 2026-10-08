// Build a private EN/JA/CHS review catalog with call sites and resumable status.
import fs from 'node:fs';
import path from 'node:path';
import crypto from 'node:crypto';
import { project, readMetaLanguage } from '../source/builders/project-paths.mjs';

const args = process.argv.slice(2);
const option = name => args[args.indexOf(name) + 1];
if (!args.includes('--output')) throw new Error('--output is required');
const output = path.resolve(option('--output'));
const codeDir = args.includes('--code-dir') ? path.resolve(option('--code-dir')) : path.join(project.referenceRoot, 'chs-tools/all-code/CodeEntries');
if (output.startsWith(project.repoRoot + path.sep)) throw new Error('Review contains original game text; save outside the public repository.');
const decode = p => JSON.parse(Buffer.from(fs.readFileSync(p, 'ascii').trim(), 'base64').toString('utf8').replace(/,\s*}\s*$/, '}'));
const metaEN = readMetaLanguage(project.metaGml, 'ENGLISH');
const metaJA = readMetaLanguage(project.metaGml, 'JAPANESE');
const code = fs.readdirSync(codeDir).filter(n => n.endsWith('.gml')).map(n => ({ name: n, lines: fs.readFileSync(path.join(codeDir, n), 'utf8').split(/\r?\n/) }));
const literalSites = new Map();
for (const entry of code) entry.lines.forEach((line, index) => {
  for (const match of line.matchAll(/"([a-z][a-z0-9_]*)"/g)) {
    const sites = literalSites.get(match[1]) ?? [];
    sites.push({ file: entry.name, line: index + 1, context: entry.lines.slice(Math.max(0, index - 2), index + 3).join('\n') });
    literalSites.set(match[1], sites);
  }
});
const old = fs.existsSync(output) ? new Map(JSON.parse(fs.readFileSync(output, 'utf8')).rows.map(r => [r.id, r])) : new Map();
const rows = [];
for (const file of fs.readdirSync(project.translationsDir).filter(n => n.endsWith('.json') && n !== 'menu-reviewed-keys.json').sort()) {
  const game = file === 'menu-human-zh.json' ? 0 : file === 'grimstone-zh-cache.json' ? 12 : file === 'meta-human-zh.json' ? 'meta' : Number(file.match(/game-(\d+)-/)[1]);
  const en = game === 'meta' || game === 51 ? metaEN : decode(path.join(project.englishDir, `${game}_Text.json`));
  const ja = game === 'meta' || game === 51 ? metaJA : decode(path.join(project.japaneseDir, `${game}_Text.json`));
  const zh = JSON.parse(fs.readFileSync(path.join(project.translationsDir, file), 'utf8'));
  for (const [key, chinese] of Object.entries(zh)) {
    if (!(key in en)) throw new Error(`Unknown source key: ${file}/${key}`);
    const id = `${game}/${key}`;
    const fingerprint = crypto.createHash('sha256').update(JSON.stringify([en[key], ja[key] ?? '', chinese])).digest('hex');
    const prior = old.get(id);
    const sites = (literalSites.get(key) ?? []).filter(s => game === 'meta' || game === 0 || game === 51 || s.file.includes(`scr${String(game).padStart(2, '0')}_`) || s.file.includes(`o${String(game).padStart(2, '0')}_`));
    rows.push({ id, game, file, key, english: en[key], japanese: ja[key] ?? '', chinese, fingerprint,
      title: metaEN[`game_title_${game}`] ?? metaEN[`game_internal_name_${game}`] ?? String(game),
      callSites: sites, contextStatus: sites.length ? 'literal-sites-found' : 'dynamic-or-internal-lookup-needed',
      review: prior?.fingerprint === fingerprint ? prior.review : { status: 'pending', notes: '', evidence: [] } });
  }
}
fs.mkdirSync(path.dirname(output), { recursive: true });
fs.writeFileSync(output, JSON.stringify({ generatedAt: new Date().toISOString(), rows }, null, 2) + '\n');
const counts = {};
for (const r of rows) counts[r.review.status] = (counts[r.review.status] ?? 0) + 1;
console.log(JSON.stringify({ rows: rows.length, counts, output }));
