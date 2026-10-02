import fs from "node:fs";
import path from "node:path";
import { project, readMetaLanguage } from "./project-paths.mjs";

const gmlPath = project.metaGml;
const humanPath = path.join(project.translationsDir, "meta-human-zh.json");
const game51Path = path.join(project.translationsDir, "game-51-human-zh.json");
const outputPath = path.join(project.outputDir, "m_Text.json");

const english = readMetaLanguage(gmlPath, "ENGLISH");

const human = JSON.parse(fs.readFileSync(humanPath, "utf8"));
const game51 = JSON.parse(fs.readFileSync(game51Path, "utf8"));
const approved = { ...human, ...game51 };
const output = JSON.parse(fs.readFileSync(outputPath, "utf8"));
const englishKeys = Object.keys(english);
const outputKeys = Object.keys(output);
const unknownHuman = Object.keys(approved).filter(key => !(key in english));
const missing = englishKeys.filter(key => !(key in output));
const extra = outputKeys.filter(key => !(key in english));
const nonHumanDiff = englishKeys.filter(key => !(key in approved) && output[key] !== english[key]);
const layoutDiff = englishKeys.filter(key => /_(?:lim|wl|wc)$/.test(key) && output[key] !== english[key]);
const missingHuman = Object.keys(approved).filter(key => output[key] !== approved[key]);

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
  humanApproved: Object.keys(approved).length,
  game51Approved: Object.keys(game51).length,
  categories,
  nonHumanDiff: nonHumanDiff.length,
  layoutDiff: layoutDiff.length,
  missingHuman: missingHuman.length
}, null, 2));
