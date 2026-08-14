import fs from "node:fs";
import path from "node:path";
import { menuStatus } from "./menu-review-policy.mjs";

const root = path.resolve(import.meta.dirname, "..");
const englishPath = path.join(root, "ext", "ENGLISH", "0_Text.json");
const japanesePath = path.join(root, "chs-backup", "menu-20260809", "ext", "JAPANESE", "0_Text.json");
const chinesePath = path.join(root, "chs-tools", "staging", "JAPANESE", "0_Text.json");
const reviewDir = path.join(root, "chs-review");

function decode(file) {
  const raw = Buffer.from(fs.readFileSync(file, "ascii").trim(), "base64").toString("utf8");
  return JSON.parse(raw.replace(/,\s*}\s*$/, "\n}"));
}

function category(key) {
  if (/^(?:prof_|menu_|option_|joy_)/.test(key)) return "菜单与设置";
  if (/^(?:title_|hi_|fave_)/.test(key)) return "游戏标题界面";
  if (/^(?:term_|crack_)/.test(key)) return "启动与终端";
  if (/^(?:ach_)/.test(key)) return "里程碑";
  if (/^(?:item_)/.test(key)) return "花园物品";
  if (/^(?:info_controls_)/.test(key)) return "游戏操作说明";
  if (/^(?:info_|filter_|genre_|bar_)/.test(key)) return "游戏库资料";
  return "其他通用文本";
}

function escapeHtml(value) {
  return String(value).replaceAll("&", "&amp;").replaceAll("<", "&lt;").replaceAll(">", "&gt;")
    .replaceAll('"', "&quot;").replaceAll("'", "&#39;");
}

const english = decode(englishPath);
const japanese = decode(japanesePath);
const chinese = decode(chinesePath);
const rows = Object.entries(english)
  .filter(([key, value]) => typeof value === "string" && value.trim() && !/_(?:lim|wl|wc)$/.test(key))
  .map(([key, en], index) => ({
    index: index + 1, key, en, ja: japanese[key] ?? "", zh: chinese[key] ?? "",
    category: category(key), status: menuStatus(key)
  }));
const done = rows.filter(row => row.status === "人工复审").length;
const categories = [...new Set(rows.map(row => row.category))];
const body = rows.map(row => `<tr data-category="${row.category}" data-status="${row.status}"><td>${row.index}</td><td><code>${escapeHtml(row.key)}</code><small>${row.category} · ${row.status}</small></td><td>${escapeHtml(row.en)}</td><td>${escapeHtml(row.ja)}</td><td>${escapeHtml(row.zh)}</td></tr>`).join("\n");
const html = `<!doctype html><html lang="zh-CN"><head><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1"><title>UFO 50 通用菜单人工复审台账</title><style>
:root{--paper:#f4efe4;--panel:#fffdf7;--ink:#211f1a;--muted:#6e675b;--line:#d8cfbe;--accent:#9a3f2d}*{box-sizing:border-box}body{margin:0;background:var(--paper);color:var(--ink);font:14px/1.55 "Microsoft YaHei UI",sans-serif}header{position:sticky;top:0;z-index:2;padding:16px 24px;background:#f4efe4f5;border-bottom:1px solid var(--line)}h1{margin:0 0 5px;font:700 24px/1.25 Georgia,"Microsoft YaHei UI",serif}.summary,.note,small{color:var(--muted)}.controls{display:grid;grid-template-columns:minmax(240px,1fr) 180px 160px auto auto;gap:8px;margin-top:10px}input,select,button{padding:8px 10px;border:1px solid var(--line);border-radius:6px;background:var(--panel);font:inherit}button{color:var(--accent);font-weight:700}main{padding:18px 24px 48px}.wrap{overflow:auto;border:1px solid var(--line);border-radius:8px;background:var(--panel)}table{width:100%;min-width:1250px;border-collapse:collapse}th,td{padding:10px;border-bottom:1px solid #ece5d8;vertical-align:top;text-align:left}th{position:sticky;top:0;background:#e9dfcd}td:first-child{width:48px;text-align:right;color:var(--muted)}td:nth-child(2){width:240px}td:nth-child(n+3){white-space:pre-wrap}code{font:12px Consolas,monospace}small{display:block;margin-top:5px}</style></head><body><header><h1>UFO 50 通用菜单人工复审台账</h1><div class="summary">共 ${rows.length} 条 · 人工复审 ${done} · 待人工复审 ${rows.length - done} · 当前显示 <strong id="count">${rows.length}</strong> 条</div><div class="controls"><input id="search" type="search" placeholder="搜索键名、英文、日文或中文……"><select id="category"><option value="">全部分类</option>${categories.map(x => `<option>${x}</option>`).join("")}</select><select id="status"><option value="">全部状态</option><option>待人工复审</option><option>人工复审</option></select><button id="pending">只看待复审</button><button id="reset">清除筛选</button></div></header><main><p class="note">本台账用于 Codex 逐条复审通用菜单的语义、术语、长度与控制符；用户只负责最终验收，不承担校对。</p><div class="wrap"><table><thead><tr><th>#</th><th>键名 / 状态</th><th>英文原文</th><th>官方日文参考</th><th>当前中文</th></tr></thead><tbody>${body}</tbody></table></div></main><script>const q=document.querySelector('#search'),c=document.querySelector('#category'),s=document.querySelector('#status'),rows=[...document.querySelectorAll('tbody tr')],n=document.querySelector('#count');function filter(){const x=q.value.trim().toLowerCase();let count=0;for(const r of rows){const show=(!x||r.textContent.toLowerCase().includes(x))&&(!c.value||r.dataset.category===c.value)&&(!s.value||r.dataset.status===s.value);r.hidden=!show;if(show)count++}n.textContent=count}q.addEventListener('input',filter);c.addEventListener('change',filter);s.addEventListener('change',filter);document.querySelector('#pending').addEventListener('click',()=>{s.value='待人工复审';filter()});document.querySelector('#reset').addEventListener('click',()=>{q.value='';c.value='';s.value='';filter()});</script></body></html>`;
const text = rows.flatMap(row => [`# ${row.index} [${row.status}] [${row.category}] ${row.key}`, `EN: ${row.en}`, `JA: ${row.ja}`, `ZH: ${row.zh}`, ""]).join("\n");
fs.mkdirSync(reviewDir, { recursive: true });
fs.writeFileSync(path.join(reviewDir, "menu-review.html"), html, "utf8");
fs.writeFileSync(path.join(reviewDir, "menu-review.txt"), text, "utf8");
console.log(`已生成通用菜单复审台账：${rows.length} 条，人工复审 ${done}，待人工复审 ${rows.length - done}。`);
