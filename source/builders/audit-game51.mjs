import fs from "node:fs";
import path from "node:path";

const root = path.resolve(import.meta.dirname, "..");
const gmlPath = path.join(root, "chs-tools", "all-code", "CodeEntries", "gml_GlobalScript_scrLoadInternalText.gml");
const translationPath = path.join(root, "chs-tools", "translations", "game-51-human-zh.json");
const metaPath = path.join(root, "chs-tools", "translations", "meta-human-zh.json");
const outputPath = path.join(root, "chs-tools", "staging", "JAPANESE", "m_Text.json");
const assignment = /global\.TEXT_META(?:\.([A-Za-z0-9_]+)|\[\$\s*"([^"]+)"\])\s*=\s*("(?:\\.|[^"\\])*");/;

function extract(lines) {
  const values = {};
  for (const line of lines) {
    const match = line.match(assignment);
    if (match) values[match[1] ?? match[2]] = JSON.parse(match[3]);
  }
  return values;
}

function fail(message) {
  throw new Error(message);
}

const lines = fs.readFileSync(gmlPath, "utf8").split(/\r?\n/);
const english = extract(lines.slice(2, 5923));
const japanese = extract(lines.slice(35528));
const chinese = JSON.parse(fs.readFileSync(translationPath, "utf8"));
const meta = JSON.parse(fs.readFileSync(metaPath, "utf8"));
const output = JSON.parse(fs.readFileSync(outputPath, "utf8"));
const keys = Object.keys(english).filter(key => key.startsWith("game_51_") && !/_(?:lim|wl|wc)$/.test(key));

if (keys.length !== 547) fail(`英文正文键数量异常：${keys.length}`);
if (Object.keys(chinese).length !== 547) fail(`中文正文键数量异常：${Object.keys(chinese).length}`);

const missing = keys.filter(key => !(key in chinese));
const extra = Object.keys(chinese).filter(key => !keys.includes(key));
if (missing.length) fail(`缺少中文键：${missing.join(", ")}`);
if (extra.length) fail(`出现未知中文键：${extra.join(", ")}`);

const starDiff = keys.filter(key => (english[key].match(/\*/g) ?? []).length !== (chinese[key].match(/\*/g) ?? []).length);
if (starDiff.length) fail(`星号控制符数量不一致：${starDiff.join(", ")}`);

const required = {
  game_51_interact_LXI_4: ["EXEC-COLO"],
  game_51_interact_LXI_6: ["EXEC-COLO"],
  game_51_interact_locust_1: ["G"],
  game_51_interact_LX3_box: ["MURZBACH PRESERVATION INDUSTRIES"],
  game_51_interact_LX2_3: ["EXEC-HOVR"],
  game_51_interact_LX2_5: ["EXEC-HOVR"],
  game_51_interact_file_cab_14: ["MILK, GREGORY"],
  game_51_interact_LX3_milk_7: ["EXEC-GODB"],
  game_51_interact_LX3_milk_9: ["EXEC-GODB"],
  game_51_interact_jordan_1: ["MR. MILK"],
  game_51_interact_jordan_3: ["MR. MILK"],
  game_51_interact_chun_trash: ["MPI?"],
  game_51_interact_chun_office_3: ["GREG"],
  game_51_interact_bola_trash_2: ["GODSBLOOD"],
  game_51_interact_tao_1: ["GREG"],
  game_51_interact_capsule_1: ["MPI"],
  game_51_ending_2: ["GREGORY MILK"]
};

for (const [key, literals] of Object.entries(required)) {
  for (const literal of literals) {
    if (!chinese[key].includes(literal)) fail(`${key} 丢失谜题字面量：${literal}`);
  }
}

const originalProtocolKeys = [
  "game_51_camp_starting_text",
  "game_51_gods_terminal",
  "game_51_gods_thug",
  "game_51_gods_player_name",
  "game_51_gods_combat_mode",
  "game_51_gods_end_combat",
  "game_51_gods_title"
];
for (const key of originalProtocolKeys) {
  if (chinese[key] !== english[key]) fail(`${key} 不再使用原版协议文本`);
}

for (const key of ["game_history_0", "game_history_39"]) {
  if (!meta[key].includes("GREG-MILK")) fail(`${key} 丢失 GREG-MILK 谜题简称`);
}
if (meta.game_name_51 !== "瘴气塔") fail("第 51 款正式中文名不再是《瘴气塔》");

// 中文终端遵循官方日文本地化策略：自然语言可翻译，实际输入协议原样保留；
// 日期代码则沿用官方日文的东亚年月日顺序，而不是英文的月日年顺序。
const japaneseTerminalProtocolKeys = [
  "game_internal_name_51",
  "game_cheat_51_0",
  "game_cheat_51_1",
  "game_cheat_51_2",
  "game_cheat_51_3",
  "game_cheat_51_4"
];
const japaneseTerminalProtocolDiff = japaneseTerminalProtocolKeys.filter(key => output[key] !== japanese[key]);
if (japaneseTerminalProtocolDiff.length) {
  fail(`终端协议未遵循官方日文：${japaneseTerminalProtocolDiff.join(", ")}`);
}

const identityCreditKeys = Object.keys(english).filter(key =>
  key.startsWith("game_credits_") && /GREGORY MILK|STEN MURZBACH/.test(english[key])
);
const identityCreditDiff = identityCreditKeys.filter(key => output[key] !== english[key]);
if (identityCreditDiff.length) fail(`职员表谜题人名被改写：${identityCreditDiff.join(", ")}`);

const outputMismatch = keys.filter(key => output[key] !== chinese[key]);
if (outputMismatch.length) fail(`构建输出未同步中文：${outputMismatch.join(", ")}`);

console.log(JSON.stringify({
  total: keys.length,
  translated: Object.keys(chinese).length,
  starControlDiff: starDiff.length,
  protectedLiteralKeys: Object.keys(required).length,
  originalProtocolKeys: originalProtocolKeys.length,
  japaneseTerminalProtocolKeys: japaneseTerminalProtocolKeys.length,
  identityCreditKeys: identityCreditKeys.length,
  outputMismatch: outputMismatch.length
}, null, 2));

