import fs from "node:fs";
import path from "node:path";

const root = path.resolve(import.meta.dirname, "..");
const gmlPath = path.join(root, "chs-tools", "all-code", "CodeEntries", "gml_GlobalScript_scrLoadInternalText.gml");
// 旧 meta-zh-cache.json 含机器初稿，只保留作历史参考，禁止写入活动构建。
const cachePath = path.join(root, "chs-tools", "translations", "meta-human-zh.json");
const outputPath = path.join(root, "chs-tools", "staging", "JAPANESE", "m_Text.json");

const lines = fs.readFileSync(gmlPath, "utf8").split(/\r?\n/).slice(0, 5923);
const meta = {};
const assignment = /global\.TEXT_META(?:\.([A-Za-z0-9_]+)|\[\$\s*"([^"]+)"\])\s*=\s*("(?:\\.|[^"\\])*");/;

for (const line of lines) {
  const match = line.match(assignment);
  if (!match) continue;
  const key = match[1] ?? match[2];
  meta[key] = JSON.parse(match[3]);
}

if (Object.keys(meta).length < 2500) {
  throw new Error(`英文元数据提取数量异常：${Object.keys(meta).length}`);
}

const cache = fs.existsSync(cachePath) ? JSON.parse(fs.readFileSync(cachePath, "utf8")) : {};
for (const [key, value] of Object.entries(cache)) {
  if (key in meta) meta[key] = value;
}

fs.mkdirSync(path.dirname(outputPath), { recursive: true });
fs.writeFileSync(outputPath, `${JSON.stringify(meta, null, 2)}\r\n`, "utf8");
console.log(`已生成 ${Object.keys(meta).length} 条外置元数据，其中中文缓存 ${Object.keys(cache).length} 条：${outputPath}`);
