import fs from "node:fs";
import path from "node:path";

const root = path.resolve(import.meta.dirname, "..");
const gmlPath = path.join(root, "chs-tools", "all-code", "CodeEntries", "gml_GlobalScript_scrLoadInternalText.gml");
const translationPath = path.join(root, "chs-tools", "translations", "game-51-human-zh.json");
const reviewDir = path.join(root, "chs-review");
const source = fs.readFileSync(gmlPath, "utf8").split(/\r?\n/);
const assignment = /global\.TEXT_META(?:\.([A-Za-z0-9_]+)|\[\$\s*"([^"]+)"\])\s*=\s*("(?:\\.|[^"\\])*");/;

function extract(start, end) {
  const result = {};
  for (const line of source.slice(start, end)) {
    const match = line.match(assignment);
    if (match) result[match[1] ?? match[2]] = JSON.parse(match[3]);
  }
  return result;
}

// Language blocks in scrLoadInternalText.gml are stable in this game build.
const english = extract(2, 5923);
const japanese = extract(35528, source.length);
const chinese = fs.existsSync(translationPath)
  ? JSON.parse(fs.readFileSync(translationPath, "utf8"))
  : {};
const keys = Object.keys(english).filter(key => key.startsWith("game_51_") && !/_(?:lim|wl|wc)$/.test(key));

if (keys.length !== 547) throw new Error(`第 51 款正文键数量异常：${keys.length}`);
for (const key of Object.keys(chinese)) {
  if (!keys.includes(key)) throw new Error(`第 51 款中文包含未知键：${key}`);
}

const rows = keys.map((key, index) => ({
  index: index + 1,
  key,
  status: key in chinese ? "已翻译" : "待翻译",
  en: english[key],
  ja: japanese[key],
  zh: chinese[key] ?? ""
}));

const json = {
  generatedAt: new Date().toISOString(),
  total: rows.length,
  translated: rows.filter(row => row.status === "已翻译").length,
  rows
};
const text = rows.flatMap(row => [
  `# ${row.index} [${row.status}] ${row.key}`,
  `EN: ${row.en}`,
  `JA: ${row.ja}`,
  `ZH: ${row.zh || "（待翻译）"}`,
  ""
]).join("\n");

fs.mkdirSync(reviewDir, { recursive: true });
fs.writeFileSync(path.join(reviewDir, "game51-review.json"), `${JSON.stringify(json, null, 2)}\n`, "utf8");
fs.writeFileSync(path.join(reviewDir, "game51-review.txt"), text, "utf8");
console.log(`已生成第 51 款对照稿：${json.translated}/${json.total}。`);

