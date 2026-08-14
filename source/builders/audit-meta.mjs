import fs from "node:fs";
import path from "node:path";

const root = path.resolve(import.meta.dirname, "..");
const gmlPath = path.join(root, "chs-tools", "all-code", "CodeEntries", "gml_GlobalScript_scrLoadInternalText.gml");
const humanPath = path.join(root, "chs-tools", "translations", "meta-human-zh.json");
const outputPath = path.join(root, "chs-tools", "staging", "JAPANESE", "m_Text.json");

const lines = fs.readFileSync(gmlPath, "utf8").split(/\r?\n/).slice(0, 5923);
const assignment = /global\.TEXT_META(?:\.([A-Za-z0-9_]+)|\[\$\s*"([^"]+)"\])\s*=\s*("(?:\\.|[^"\\])*");/;
const english = {};
for (const line of lines) {
  const match = line.match(assignment);
  if (!match) continue;
  english[match[1] ?? match[2]] = JSON.parse(match[3]);
}

const human = JSON.parse(fs.readFileSync(humanPath, "utf8"));
const output = JSON.parse(fs.readFileSync(outputPath, "utf8"));
const englishKeys = Object.keys(english);
const outputKeys = Object.keys(output);
const unknownHuman = Object.keys(human).filter(key => !(key in english));
const missing = englishKeys.filter(key => !(key in output));
const extra = outputKeys.filter(key => !(key in english));
const nonHumanDiff = englishKeys.filter(key => !(key in human) && output[key] !== english[key]);
const layoutDiff = englishKeys.filter(key => /_(?:lim|wl|wc)$/.test(key) && output[key] !== english[key]);
const missingHuman = Object.keys(human).filter(key => output[key] !== human[key]);

const failures = { unknownHuman, missing, extra, nonHumanDiff, layoutDiff, missingHuman };
for (const [name, values] of Object.entries(failures)) {
  if (values.length) throw new Error(`${name}：${values.slice(0, 20).join(", ")}`);
}

const categories = {
  hint: Object.keys(human).filter(key => key.startsWith("hint_")).length,
  description: Object.keys(human).filter(key => key.startsWith("game_description_")).length,
  history: Object.keys(human).filter(key => key.startsWith("game_history_")).length,
  message: Object.keys(human).filter(key => key.startsWith("game_meta_message_")).length
};

console.log(JSON.stringify({
  englishKeys: englishKeys.length,
  outputKeys: outputKeys.length,
  humanApproved: Object.keys(human).length,
  categories,
  nonHumanDiff: nonHumanDiff.length,
  layoutDiff: layoutDiff.length,
  missingHuman: missingHuman.length
}, null, 2));
