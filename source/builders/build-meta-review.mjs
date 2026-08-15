import fs from "node:fs";
import path from "node:path";

const root = path.resolve(import.meta.dirname, "..");
const gmlPath = path.join(root, "chs-tools", "all-code", "CodeEntries", "gml_GlobalScript_scrLoadInternalText.gml");
const legacyPath = path.join(root, "chs-tools", "translations", "meta-zh-cache.json");
const humanPath = path.join(root, "chs-tools", "translations", "meta-human-zh.json");
const reviewDir = path.join(root, "chs-review");

const assignment = /global\.TEXT_META(?:\.([A-Za-z0-9_]+)|\[\$\s*"([^"]+)"\])\s*=\s*("(?:\\.|[^"\\])*");/g;
const source = fs.readFileSync(gmlPath, "utf8");
const variants = {};
for (const match of source.matchAll(assignment)) {
  const key = match[1] ?? match[2];
  (variants[key] ??= []).push(JSON.parse(match[3]));
}

const legacy = JSON.parse(fs.readFileSync(legacyPath, "utf8"));
const human = JSON.parse(fs.readFileSync(humanPath, "utf8"));
const keys = Object.keys(legacy);
for (const key of Object.keys(human)) {
  if (!(key in variants)) throw new Error(`人工元数据包含未知键：${key}`);
}

function category(key) {
  if (key.startsWith("hint_")) return "寻宝提示";
  if (key.startsWith("game_description_")) return "游戏简介";
  if (key.startsWith("game_history_")) return "开发历史";
  return "资料留言";
}

function escapeHtml(value) {
  return String(value)
    .replaceAll("&", "&amp;")
    .replaceAll("<", "&lt;")
    .replaceAll(">", "&gt;")
    .replaceAll('"', "&quot;")
    .replaceAll("'", "&#39;");
}

const rows = keys.map((key, index) => {
  const values = variants[key] ?? [];
  if (values.length < 2) throw new Error(`缺少多语言元数据源：${key}`);
  return {
    index: index + 1,
    key,
    category: category(key),
    status: key in human ? "人工初校" : "待人工重译",
    en: values[0],
    ja: values.at(-1),
    zh: human[key] ?? "（尚未人工翻译）"
  };
});

const reviewed = rows.filter(row => row.status === "人工初校").length;
const categories = [...new Set(rows.map(row => row.category))];
const rowHtml = rows.map(row => `<tr data-category="${row.category}" data-status="${row.status}">
  <td>${row.index}</td><td><code>${escapeHtml(row.key)}</code><small>${row.category} · ${row.status}</small></td>
  <td>${escapeHtml(row.en)}</td><td>${escapeHtml(row.ja)}</td><td>${escapeHtml(row.zh)}</td></tr>`).join("\n");

const html = `<!doctype html><html lang="zh-CN"><head><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1">
<title>UFO 50 合集资料中文对照稿</title><style>
:root{--paper:#f4efe4;--panel:#fffdf7;--ink:#211f1a;--muted:#6e675b;--line:#d8cfbe;--accent:#9a3f2d}*{box-sizing:border-box}
body{margin:0;background:var(--paper);color:var(--ink);font:14px/1.55 "Microsoft YaHei UI",sans-serif}header{position:sticky;top:0;z-index:2;padding:16px 24px;background:#f4efe4f5;border-bottom:1px solid var(--line)}
h1{margin:0 0 5px;font:700 24px/1.25 Georgia,"Microsoft YaHei UI",serif}.summary,.note,small{color:var(--muted)}.controls{display:grid;grid-template-columns:minmax(260px,1fr) 180px 160px auto auto;gap:8px;margin-top:10px}
input,select,button{padding:8px 10px;border:1px solid var(--line);border-radius:6px;background:var(--panel);font:inherit}button{color:var(--accent);font-weight:700}main{padding:18px 24px 48px}.note{margin:0 0 14px}
.wrap{overflow:auto;border:1px solid var(--line);border-radius:8px;background:var(--panel)}table{width:100%;min-width:1250px;border-collapse:collapse}th,td{padding:10px;border-bottom:1px solid #ece5d8;vertical-align:top;text-align:left}th{position:sticky;top:0;background:#e9dfcd}td:first-child{width:48px;text-align:right;color:var(--muted)}td:nth-child(2){width:230px}td:nth-child(n+3){white-space:pre-wrap}code{font:12px Consolas,monospace}small{display:block;margin-top:5px}
</style></head><body><header><h1>UFO 50 合集资料中文对照稿</h1><div class="summary">共 ${rows.length} 条 · 人工初校 ${reviewed} · 待人工重译 ${rows.length - reviewed} · 当前显示 <strong id="count">${rows.length}</strong> 条</div>
<div class="controls"><input id="search" type="search" placeholder="搜索键名、英文、日文或中文……"><select id="category"><option value="">全部分类</option>${categories.map(x => `<option>${x}</option>`).join("")}</select><select id="status"><option value="">全部状态</option><option>待人工重译</option><option>人工初校</option></select><button id="pending">只看待重译</button><button id="reset">清除筛选</button></div></header>
<main><p class="note">所有中文均由 Codex 依据英文原文并参考官方日文逐条人工翻译和自校。旧机器稿不会写入本页面的中文栏，也不会进入新构建；用户只负责最终验收。</p><div class="wrap"><table><thead><tr><th>#</th><th>键名 / 状态</th><th>英文原文</th><th>官方日文参考</th><th>当前人工中文</th></tr></thead><tbody>${rowHtml}</tbody></table></div></main>
<script>const q=document.querySelector('#search'),c=document.querySelector('#category'),s=document.querySelector('#status'),rows=[...document.querySelectorAll('tbody tr')],n=document.querySelector('#count');function filter(){const x=q.value.trim().toLowerCase();let count=0;for(const r of rows){const show=(!x||r.textContent.toLowerCase().includes(x))&&(!c.value||r.dataset.category===c.value)&&(!s.value||r.dataset.status===s.value);r.hidden=!show;if(show)count++}n.textContent=count}q.addEventListener('input',filter);c.addEventListener('change',filter);s.addEventListener('change',filter);document.querySelector('#pending').addEventListener('click',()=>{s.value='待人工重译';filter()});document.querySelector('#reset').addEventListener('click',()=>{q.value='';c.value='';s.value='';filter()});</script></body></html>`;

const text = rows.flatMap(row => [
  `# ${row.index} [${row.status}] [${row.category}] ${row.key}`,
  `EN: ${row.en}`,
  `JA: ${row.ja}`,
  `ZH: ${row.zh}`,
  ""
]).join("\n");

fs.mkdirSync(reviewDir, { recursive: true });
fs.writeFileSync(path.join(reviewDir, "meta-review.html"), html, "utf8");
fs.writeFileSync(path.join(reviewDir, "meta-review.txt"), text, "utf8");
console.log(`已生成合集资料对照稿：${rows.length} 条，人工初校 ${reviewed}，待人工重译 ${rows.length - reviewed}。`);
