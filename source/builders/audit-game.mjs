import fs from "node:fs";
import path from "node:path";

const id = process.argv[2];
if (!/^\d+$/.test(id ?? "")) throw new Error("用法：node chs-tools/audit-game.mjs <游戏内部 ID>");
const active = process.argv.includes("--active");
const root = path.resolve(import.meta.dirname, "..");

function decode(file) {
  const raw = Buffer.from(fs.readFileSync(file, "ascii").trim(), "base64").toString("utf8");
  return JSON.parse(raw.replace(/,\s*}\s*$/, "\n}"));
}

function signature(value) {
  return (String(value).match(/\*+|@+|\^+|\[[12UDLR]/g) ?? []).sort().join("|");
}

const english = decode(path.join(root, "ext", "ENGLISH", `${id}_Text.json`));
const output = decode(active
  ? path.join(root, "ext", "JAPANESE", `${id}_Text.json`)
  : path.join(root, "chs-tools", "staging", "JAPANESE", `${id}_Text.json`));
const human = JSON.parse(fs.readFileSync(path.join(root, "chs-tools", "translations", `game-${id}-human-zh.json`), "utf8"));
const actual = Object.entries(english).filter(([key, value]) => typeof value === "string" && value.trim() && !/_(?:lim|wl|wc)$/.test(key));
const missingHuman = actual.filter(([key]) => !(key in human)).map(([key]) => key);
const unknownHuman = Object.keys(human).filter(key => !(key in english));
const keyDiff = Object.keys(english).filter(key => !(key in output)).concat(Object.keys(output).filter(key => !(key in english)));
const layoutDiff = Object.keys(english).filter(key => /_(?:lim|wl|wc)$/.test(key) && output[key] !== english[key]);
const controlDiff = Object.keys(english).filter(key => signature(output[key]) !== signature(english[key]));
const missingOutput = Object.keys(human).filter(key => output[key] !== human[key]);
for (const [name, values] of Object.entries({ missingHuman, unknownHuman, keyDiff, layoutDiff, controlDiff, missingOutput })) {
  if (values.length) throw new Error(`${name}：${values.slice(0, 30).join(", ")}`);
}
console.log(JSON.stringify({ id, target: active ? "active" : "staging", keys: Object.keys(english).length, actual: actual.length, human: Object.keys(human).length, controlDiff: 0, layoutDiff: 0 }));
