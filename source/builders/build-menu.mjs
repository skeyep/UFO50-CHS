import fs from "node:fs";
import path from "node:path";
import { project } from "./project-paths.mjs";

const source = path.join(project.englishDir, "0_Text.json");
const outputIndex = process.argv.indexOf("--output");
const target = outputIndex >= 0
  ? path.resolve(process.argv[outputIndex + 1])
  : path.join(project.outputDir, "0_Text.json");

function decode(file) {
  const raw = Buffer.from(fs.readFileSync(file, "ascii").trim(), "base64").toString("utf8");
  return JSON.parse(raw.replace(/,\s*}\s*$/, "\n}"));
}

function encodeOfficialStyle(file, object) {
  const lines = Object.entries(object).map(([key, value]) => `${JSON.stringify(key)}:\t${JSON.stringify(value)},`);
  const raw = `{\r\n${lines.join("\r\n")}\r\n}\r\n`;
  fs.mkdirSync(path.dirname(file), { recursive: true });
  fs.writeFileSync(file, Buffer.from(raw, "utf8").toString("base64"), "ascii");
}

const text = decode(source);
const zh = JSON.parse(fs.readFileSync(path.join(project.translationsDir, "menu-human-zh.json"), "utf8"));

for (const [key, value] of Object.entries(zh)) {
  if (!(key in text)) throw new Error(`缺少本地化键：${key}`);
  if (typeof value !== "string" || /_(?:lim|wl|wc)$/.test(key)) throw new Error(`人工译文菜单字段无效：${key}`);
  text[key] = value;
}

encodeOfficialStyle(target, text);
console.log(`已写入 ${Object.keys(zh).length} 条 UFO 50 通用菜单中文：${target}`);
