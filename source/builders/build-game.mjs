import fs from "node:fs";
import path from "node:path";

const id = process.argv[2];
if (!/^\d+$/.test(id ?? "")) throw new Error("用法：node chs-tools/build-game.mjs <游戏内部 ID>");

const root = path.resolve(import.meta.dirname, "..");
const sourcePath = path.join(root, "ext", "ENGLISH", `${id}_Text.json`);
const cachePath = path.join(root, "chs-tools", "translations", `game-${id}-human-zh.json`);
const outputPath = path.join(root, "chs-tools", "staging", "JAPANESE", `${id}_Text.json`);

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

const source = decode(sourcePath);
const human = JSON.parse(fs.readFileSync(cachePath, "utf8"));
for (const [key, value] of Object.entries(human)) {
  if (!(key in source)) throw new Error(`人工译文包含未知键：${key}`);
  if (/_(?:lim|wl|wc)$/.test(key)) throw new Error(`人工译文不得修改布局键：${key}`);
  if (typeof value !== "string") throw new Error(`人工译文必须是字符串：${key}`);
  source[key] = value;
}

encodeOfficialStyle(outputPath, source);
console.log(`已生成游戏 ${id} 中文：${Object.keys(source).length} 个键，人工译文 ${Object.keys(human).length} 条。`);
