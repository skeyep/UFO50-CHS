import fs from "node:fs";
import path from "node:path";
import { project, readMetaLanguage } from "./project-paths.mjs";

const gmlPath = project.metaGml;
// 旧 meta-zh-cache.json 含机器初稿，只保留作历史参考，禁止写入活动构建。
const cachePath = path.join(project.translationsDir, "meta-human-zh.json");
const game51Path = path.join(project.translationsDir, "game-51-human-zh.json");
const outputPath = path.join(project.outputDir, "m_Text.json");

const meta = readMetaLanguage(gmlPath, "ENGLISH");

if (Object.keys(meta).length < 2500) {
  throw new Error(`英文元数据提取数量异常：${Object.keys(meta).length}`);
}

const cache = JSON.parse(fs.readFileSync(cachePath, "utf8"));
const game51 = JSON.parse(fs.readFileSync(game51Path, "utf8"));
for (const [key, value] of Object.entries({ ...cache, ...game51 })) {
  if (!(key in meta)) throw new Error(`人工译文包含未知元数据键：${key}`);
  if (typeof value !== "string" || /_(?:lim|wl|wc)$/.test(key)) throw new Error(`人工译文元数据字段无效：${key}`);
  meta[key] = value;
}

fs.mkdirSync(path.dirname(outputPath), { recursive: true });
fs.writeFileSync(outputPath, `${JSON.stringify(meta, null, 2)}\r\n`, "utf8");
console.log(`已生成 ${Object.keys(meta).length} 条外置元数据，其中合集中文 ${Object.keys(cache).length} 条、第 51 款中文 ${Object.keys(game51).length} 条：${outputPath}`);
