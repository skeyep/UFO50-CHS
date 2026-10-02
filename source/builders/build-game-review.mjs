import fs from "node:fs";
import path from "node:path";
import { project, positionalArgs } from "./project-paths.mjs";

const id = positionalArgs[0];
if (!id || !/^\d+$/.test(id)) {
  console.error("用法：node source/builders/build-game-review.mjs <游戏ID> [输出名]");
  process.exit(1);
}

const root = project.repoRoot;
const outputName = positionalArgs[1] || `game-${id}-review`;

function decodeExternal(relativePath) {
  const wrapped = fs.readFileSync(path.join(project.referenceRoot, relativePath), "ascii").trim();
  const json = Buffer.from(wrapped, "base64")
    .toString("utf8")
    .replace(/,\s*}\s*$/, "}");
  return JSON.parse(json);
}

const english = decodeExternal(`ext/ENGLISH/${id}_Text.json`);
const japanese = decodeExternal(`reference/JAPANESE-original/${id}_Text.json`);
const humanPath = path.join(project.translationsDir, `game-${id}-human-zh.json`);
const chinese = JSON.parse(fs.readFileSync(humanPath, "utf8"));

const rows = Object.entries(english).filter(
  ([key, value]) =>
    typeof value === "string" &&
    value.trim() &&
    !/_(?:lim|wl|wc)$/.test(key),
);

const missing = rows.filter(([key]) => !(key in chinese));
const unknown = Object.keys(chinese).filter((key) => !(key in english));
if (missing.length || unknown.length) {
  console.error(`人工译文不完整：缺少 ${missing.length}，未知键 ${unknown.length}`);
  process.exit(1);
}

function indent(value) {
  return String(value).replace(/\r?\n/g, "\n    ");
}

const lines = [
  `UFO 50 子游戏 ${id} 人工译文对照台账`,
  `实际文本：${rows.length} 条`,
  "来源：英文原文 / 官方日文参考 / 简体中文人工译文",
  "",
];

rows.forEach(([key, value], index) => {
  lines.push(
    `[${String(index + 1).padStart(4, "0")}] ${key}`,
    `EN  ${indent(value)}`,
    `JA  ${indent(japanese[key] ?? "")}`,
    `ZH  ${indent(chinese[key])}`,
    "",
  );
});

const outputDir = project.reviewDir;
fs.mkdirSync(outputDir, { recursive: true });
const outputPath = path.join(outputDir, `${outputName}.txt`);
fs.writeFileSync(outputPath, `\uFEFF${lines.join("\n")}`, "utf8");
console.log(`已生成 ${path.relative(root, outputPath)}：${rows.length} 条。`);
