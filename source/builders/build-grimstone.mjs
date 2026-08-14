import fs from "node:fs";
import path from "node:path";
import { grimstoneIsApproved } from "./grimstone-review-policy.mjs";

const root = path.resolve(import.meta.dirname, "..");
const sourcePath = path.join(root, "ext", "ENGLISH", "12_Text.json");
const cachePath = path.join(root, "chs-tools", "translations", "grimstone-zh-cache.json");
const outputPath = path.join(root, "chs-tools", "staging", "JAPANESE", "12_Text.json");

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
const cache = fs.existsSync(cachePath) ? JSON.parse(fs.readFileSync(cachePath, "utf8")) : {};

let approvedCount = 0;
for (const [key, value] of Object.entries(cache)) {
  if (!(key in source)) throw new Error(`Grimstone 缓存包含未知键：${key}`);
  if (/_(?:lim|wl|wc)$/.test(key)) throw new Error(`Grimstone 缓存不应修改布局键：${key}`);
  if (grimstoneIsApproved(key)) {
    source[key] = value;
    approvedCount++;
  }
}

encodeOfficialStyle(outputPath, source);
console.log(`已生成 Grimstone 中文暂存：${Object.keys(source).length} 个键，已写入人工认可中文 ${approvedCount} 条，待人工重译 ${Object.keys(cache).length - approvedCount} 条保持英文。`);
