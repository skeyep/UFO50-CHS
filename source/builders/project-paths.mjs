import fs from "node:fs";
import path from "node:path";

const options = new Map();
export const positionalArgs = [];
const pathOptions = new Set(["--reference-root", "--output-root", "--review-root", "--meta-gml", "--output"]);
for (let i = 2; i < process.argv.length; i++) {
  const arg = process.argv[i];
  if (pathOptions.has(arg)) {
    const value = process.argv[++i];
    if (!value || value.startsWith("--")) throw new Error(`${arg} 需要路径参数。`);
    options.set(arg, path.resolve(value));
  } else if (!arg.startsWith("--")) positionalArgs.push(arg);
}

const repoRoot = path.resolve(import.meta.dirname, "../..");
const referenceRoot = options.get("--reference-root") ?? path.resolve(process.env.UFO50_CHS_REFERENCE_ROOT ?? path.join(repoRoot, "private"));
export const project = {
  repoRoot,
  referenceRoot,
  englishDir: path.join(referenceRoot, "ext", "ENGLISH"),
  japaneseDir: path.join(referenceRoot, "reference", "JAPANESE-original"),
  translationsDir: path.join(repoRoot, "source", "translations"),
  outputDir: options.get("--output-root") ?? path.join(repoRoot, "dist", "text", "JAPANESE"),
  reviewDir: options.get("--review-root") ?? path.join(repoRoot, "dist", "review"),
  metaGml: options.get("--meta-gml") ?? path.join(referenceRoot, "chs-tools", "all-code", "CodeEntries", "gml_GlobalScript_scrLoadInternalText.gml"),
};

export function readMetaLanguage(file, language) {
  const branch = /^\s*if\s*\(global\.language\s*==\s*global\.LANG_([A-Z_]+)\)/;
  const assignment = /global\.TEXT_META(?:\.([A-Za-z0-9_]+)|\[\$\s*"([^"]+)"\])\s*=\s*("(?:\\.|[^"\\])*");/;
  const values = {};
  let active = false;
  for (const line of fs.readFileSync(file, "utf8").split(/\r?\n/)) {
    const matchBranch = line.match(branch);
    if (matchBranch) active = matchBranch[1] === language;
    if (!active) continue;
    const match = line.match(assignment);
    if (!match) continue;
    const key = match[1] ?? match[2];
    if (key in values) throw new Error(`${language} 元数据出现重复键：${key}`);
    values[key] = JSON.parse(match[3]);
  }
  if (Object.keys(values).length < 2500) throw new Error(`${language} 元数据语言分支提取失败。`);
  return values;
}
