import fs from "node:fs";
import path from "node:path";
import { menuStatus } from "./menu-review-policy.mjs";

const root = path.resolve(import.meta.dirname, "..");
const englishPath = path.join(root, "ext", "ENGLISH", "0_Text.json");
const outputPath = path.join(root, "chs-tools", "staging", "JAPANESE", "0_Text.json");

function decode(file) {
  const raw = Buffer.from(fs.readFileSync(file, "ascii").trim(), "base64").toString("utf8");
  return JSON.parse(raw.replace(/,\s*}\s*$/, "\n}"));
}

function controlSignature(value) {
  return (String(value).match(/\*+|@+|\^+|\[[12UDLR]/g) ?? []).sort().join("|");
}

const english = decode(englishPath);
const output = decode(outputPath);
const englishKeys = Object.keys(english);
const outputKeys = Object.keys(output);
const missing = englishKeys.filter(key => !(key in output));
const extra = outputKeys.filter(key => !(key in english));
const layoutDiff = englishKeys.filter(key => /_(?:lim|wl|wc)$/.test(key) && output[key] !== english[key]);
const controlDiff = englishKeys.filter(key => controlSignature(output[key]) !== controlSignature(english[key]));
const actual = Object.entries(english).filter(([key, value]) => typeof value === "string" && value.trim() && !/_(?:lim|wl|wc)$/.test(key));
const pending = actual.filter(([key]) => menuStatus(key) !== "人工复审").map(([key]) => key);
const unchanged = actual.filter(([key, value]) => output[key] === value).map(([key]) => key);

for (const [name, values] of Object.entries({ missing, extra, layoutDiff, controlDiff, pending })) {
  if (values.length) throw new Error(`${name}：${values.slice(0, 30).join(", ")}`);
}

console.log(JSON.stringify({
  englishKeys: englishKeys.length,
  outputKeys: outputKeys.length,
  actual: actual.length,
  reviewed: actual.length - pending.length,
  changed: actual.length - unchanged.length,
  unchanged,
  controlDiff: controlDiff.length,
  layoutDiff: layoutDiff.length
}, null, 2));
