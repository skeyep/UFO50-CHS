import fs from "node:fs";
import path from "node:path";
import { grimstoneIsApproved } from "../source/builders/grimstone-review-policy.mjs";

const root = path.resolve(import.meta.dirname, "..");
function readJson(file) {
  return JSON.parse(fs.readFileSync(file,"utf8").replace(/^\uFEFF/, ""));
}
function readPayload(name) {
  const file = path.join(root,"payload/ext/JAPANESE",name);
  if (name === "m_Text.json") return readJson(file);
  const raw = fs.readFileSync(file,"ascii").trim();
  if (Buffer.from(raw,"base64").toString("base64") !== raw) throw new Error(`载荷 Base64 无效：${name}`);
  return JSON.parse(Buffer.from(raw,"base64").toString("utf8").replace(/,\s*}\s*$/, "}"));
}
let fields = 0;
const layoutKey = /_(?:lim|wl|wc)$/;
const menuOriginal = {
  copyright_unlimited_solutions: "LX SYSTEMS",
  copyright_ufo_soft: "UFO SOFT",
  crack_credit_qa2: "LOLLIPOP ROBOT",
  crack_credit_jp3: "BY 8-4, LTD.",
  term_info_file_extension: ".UFO",
  title_credits_2: "CHUN, PETTER, & SMOLSKI",
};
function checkCoverage(payload, keys, name) {
  for (const [key, value] of Object.entries(payload)) {
    if (typeof value !== "string") throw new Error(`载荷字段必须是字符串：${name}/${key}`);
    const limit = Number(payload[`${key}_lim`] ?? 0);
    if (limit > 0 && [...value].length > limit) {
      throw new Error(`文本被运行时字符上限截断：${name}/${key}，长度 ${[...value].length}，上限 ${limit}`);
    }
    if (!value.trim() || layoutKey.test(key) || keys.has(key)) continue;
    if (name === "0_Text.json" && menuOriginal[key] === value) continue;
    // 元数据中的原版内部简称、输入代码、榜单姓名和职员表由参考资源审计核对。
    if (name === "m_Text.json" && /^game_(?:internal_name_[A-Za-z0-9_]+|(?:cheat|hs)_\d+_\d+|credits_\d+_\d+b?)$/.test(key) && !/[\u3400-\u9fff]/u.test(value)) continue;
    throw new Error(`载荷正文缺少译文源：${name}/${key}`);
  }
}
function compare(sourceName, payloadName) {
  const source = readJson(path.join(root,"source/translations",sourceName));
  const payload = readPayload(payloadName);
  for (const [key,value] of Object.entries(source)) {
    if (typeof value !== "string" || layoutKey.test(key)) throw new Error(`译文源字段无效：${sourceName}/${key}`);
    if (sourceName === "grimstone-zh-cache.json" && !grimstoneIsApproved(key)) throw new Error(`存在未认可的诡石镇译文：${key}`);
    if (payload[key] !== value) throw new Error(`译文源与载荷不同步：${sourceName}/${key}`);
    fields++;
  }
  if (payloadName !== "m_Text.json") checkCoverage(payload, new Set(Object.keys(source)), payloadName);
  return Object.keys(source);
}
compare("menu-human-zh.json","0_Text.json");
for (let id=1;id<=50;id++) compare(id === 12 ? "grimstone-zh-cache.json" : `game-${id}-human-zh.json`,`${id}_Text.json`);
const metaKeys = compare("meta-human-zh.json","m_Text.json");
const game51Keys = compare("game-51-human-zh.json","m_Text.json");
checkCoverage(readPayload("m_Text.json"), new Set([...metaKeys, ...game51Keys]), "m_Text.json");
console.log(`译文源与载荷同步验证通过：${fields} 条，覆盖合集及 51 款内容。`);
