import fs from "node:fs";
import path from "node:path";
import { project, readMetaLanguage } from "./project-paths.mjs";

const gmlPath = project.metaGml;
const translationPath = path.join(project.translationsDir, "game-51-human-zh.json");
const reviewDir = project.reviewDir;
const english = readMetaLanguage(gmlPath, "ENGLISH");
const japanese = readMetaLanguage(gmlPath, "JAPANESE");
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

