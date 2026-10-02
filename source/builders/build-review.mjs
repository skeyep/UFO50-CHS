import fs from "node:fs";
import path from "node:path";
import { project } from "./project-paths.mjs";
import { grimstoneStatus } from "./grimstone-review-policy.mjs";

const root = project.repoRoot;
const reviewDir = project.reviewDir;
const englishPath = path.join(project.englishDir, "12_Text.json");
const japanesePath = path.join(project.japaneseDir, "12_Text.json");
const cachePath = path.join(project.translationsDir, "grimstone-zh-cache.json");

function decode(file) {
  const raw = Buffer.from(fs.readFileSync(file, "ascii").trim(), "base64").toString("utf8");
  return JSON.parse(raw.replace(/,\s*}\s*$/, "\n}"));
}

function escapeHtml(value) {
  return String(value)
    .replaceAll("&", "&amp;")
    .replaceAll("<", "&lt;")
    .replaceAll(">", "&gt;")
    .replaceAll('"', "&quot;")
    .replaceAll("'", "&#39;");
}

function categoryFor(key) {
  if (/^(?:gold_|cherry_|garden_|pre_gold_|post_gold_|detail_)/.test(key)) return "合集目标";
  if (/^(?:menu_|status_|stat_|whats_|shop_|bank_|stable_|hotel_|bar_|train_|no_teeth)/.test(key)) return "界面与设施";
  if (/^battle_/.test(key)) return "战斗文本";
  if (/^(?:name_|name_wolf_|pdesc_)/.test(key)) return "角色资料";
  if (/^town_/.test(key)) return "地点名称";
  if (/^item_/.test(key)) return "物品装备";
  if (/^(?:skill_|learned_|use_skill_)/.test(key)) return "技能";
  if (/^perk_/.test(key)) return "特性";
  if (/^npc_/.test(key)) return "NPC 对话";
  if (/^(?:boss_|enemy_)/.test(key)) return "敌人与首领";
  if (/^animal_/.test(key)) return "动物文本";
  if (/^(?:intro_|ending_|biggan_|fire_|malus_|diary_|obelisk_)/.test(key)) return "主线剧情";
  return "其他文本";
}

const english = decode(englishPath);
const japanese = decode(japanesePath);
const chinese = JSON.parse(fs.readFileSync(cachePath, "utf8"));

const rows = Object.entries(english)
  .filter(([key, value]) => typeof value === "string" && value.trim() && !/_(?:lim|wl|wc)$/.test(key))
  .map(([key, en], index) => ({
    index: index + 1,
    key,
    category: categoryFor(key),
    status: grimstoneStatus(key),
    en,
    ja: japanese[key] ?? "",
    zh: chinese[key] ?? "（尚未翻译）"
  }));

const statusCounts = Object.groupBy(rows, row => row.status);
const categoryNames = [...new Set(rows.map(row => row.category))].sort((a, b) => a.localeCompare(b, "zh-CN"));
const rowHtml = rows.map(row => `
  <tr data-category="${escapeHtml(row.category)}" data-status="${escapeHtml(row.status)}">
    <td class="num">${row.index}</td>
    <td><code>${escapeHtml(row.key)}</code><div class="badges"><span>${escapeHtml(row.category)}</span><span class="status ${row.status === "待人工重译" ? "machine" : "reviewed"}">${escapeHtml(row.status)}</span></div></td>
    <td class="text en">${escapeHtml(row.en)}</td>
    <td class="text ja">${escapeHtml(row.ja)}</td>
    <td class="text zh">${escapeHtml(row.zh)}</td>
  </tr>`).join("");

const html = `<!doctype html>
<html lang="zh-CN">
<head>
<meta charset="utf-8">
<meta name="viewport" content="width=device-width, initial-scale=1">
<title>UFO 50《Grimstone》中文校对稿</title>
<style>
  :root { color-scheme: light; --ink:#211f1a; --muted:#6e675b; --paper:#f4efe4; --panel:#fffdf7; --line:#d8cfbe; --accent:#9a3f2d; --ok:#315f46; --machine:#986018; }
  * { box-sizing:border-box; }
  body { margin:0; color:var(--ink); background:var(--paper); font:14px/1.55 "Microsoft YaHei UI","Noto Sans SC",sans-serif; }
  header { position:sticky; top:0; z-index:3; padding:18px 24px 14px; background:rgba(244,239,228,.96); border-bottom:1px solid var(--line); backdrop-filter:blur(8px); }
  h1 { margin:0 0 5px; font:700 24px/1.25 Georgia,"Microsoft YaHei UI",serif; }
  .summary { color:var(--muted); margin-bottom:12px; }
  .controls { display:grid; grid-template-columns:minmax(260px,1fr) 180px 150px auto auto; gap:8px; }
  input,select,button { border:1px solid var(--line); border-radius:6px; background:var(--panel); color:var(--ink); padding:8px 10px; font:inherit; }
  button { cursor:pointer; color:var(--accent); font-weight:700; }
  main { padding:18px 24px 48px; }
  .note { max-width:1100px; margin:0 0 14px; color:var(--muted); }
  .table-wrap { overflow:auto; border:1px solid var(--line); border-radius:8px; background:var(--panel); box-shadow:0 10px 30px rgba(63,48,27,.06); }
  table { width:100%; min-width:1250px; border-collapse:collapse; }
  th { position:sticky; top:0; z-index:2; background:#e9dfcd; text-align:left; padding:10px; border-bottom:1px solid var(--line); }
  td { vertical-align:top; padding:10px; border-bottom:1px solid #ece5d8; }
  tr:hover td { background:#fff9eb; }
  .num { width:48px; color:var(--muted); text-align:right; }
  code { font:12px/1.45 Consolas,monospace; color:#614637; overflow-wrap:anywhere; }
  .badges { display:flex; flex-wrap:wrap; gap:5px; margin-top:7px; }
  .badges span { border-radius:999px; padding:2px 7px; background:#ede5d6; color:var(--muted); font-size:11px; }
  .badges .reviewed { background:#dbeadd; color:var(--ok); }
  .badges .machine { background:#f4e4c8; color:var(--machine); }
  .text { width:25%; white-space:pre-wrap; overflow-wrap:anywhere; }
  .zh { color:#182b20; font-weight:600; }
  .hidden { display:none; }
  @media print { header { position:static; } .controls { display:none; } main { padding:0; } .table-wrap { overflow:visible; box-shadow:none; } }
</style>
</head>
<body>
<header>
  <h1>UFO 50《Grimstone》中文校对稿</h1>
  <div class="summary">共 ${rows.length} 条实际文本 · 人工初校 ${(statusCounts["人工初校"] ?? []).length} · 术语初校 ${(statusCounts["术语初校"] ?? []).length} · 待人工重译 ${(statusCounts["待人工重译"] ?? []).length} · 当前显示 <strong id="visibleCount">${rows.length}</strong> 条</div>
  <div class="controls">
    <input id="search" type="search" placeholder="搜索键名、英文、日文或中文……">
    <select id="category"><option value="">全部分类</option>${categoryNames.map(name => `<option>${escapeHtml(name)}</option>`).join("")}</select>
    <select id="status"><option value="">全部状态</option><option>待人工重译</option><option>术语初校</option><option>人工初校</option></select>
    <button id="machineOnly" type="button">只看待重译</button>
    <button id="reset" type="button">清除筛选</button>
  </div>
</header>
<main>
  <p class="note">“待人工重译”是旧机器稿占位，不计入汉化完成度；“术语初校”表示短标签和专名已经逐条检查；“人工初校”表示已依据英文原文并参考官方日文完成翻译与第一轮自校。此页面供最终审核与进度查看，不要求用户承担校对。</p>
  <div class="table-wrap">
    <table>
      <thead><tr><th>#</th><th>键名 / 状态</th><th>英文原文</th><th>官方日文参考</th><th>当前中文</th></tr></thead>
      <tbody>${rowHtml}</tbody>
    </table>
  </div>
</main>
<script>
  const search = document.querySelector('#search');
  const category = document.querySelector('#category');
  const status = document.querySelector('#status');
  const rows = [...document.querySelectorAll('tbody tr')];
  function applyFilters() {
    const query = search.value.trim().toLocaleLowerCase('zh-CN');
    let visible = 0;
    for (const row of rows) {
      const matches = (!query || row.textContent.toLocaleLowerCase('zh-CN').includes(query))
        && (!category.value || row.dataset.category === category.value)
        && (!status.value || row.dataset.status === status.value);
      row.classList.toggle('hidden', !matches);
      if (matches) visible++;
    }
    document.querySelector('#visibleCount').textContent = visible;
  }
  search.addEventListener('input', applyFilters);
  category.addEventListener('change', applyFilters);
  status.addEventListener('change', applyFilters);
  document.querySelector('#machineOnly').addEventListener('click', () => { status.value='待人工重译'; applyFilters(); });
  document.querySelector('#reset').addEventListener('click', () => { search.value=''; category.value=''; status.value=''; applyFilters(); });
</script>
</body>
</html>`;

const text = rows.map(row => [
  `# ${row.index} [${row.status}] [${row.category}] ${row.key}`,
  `EN: ${row.en}`,
  `JA: ${row.ja}`,
  `ZH: ${row.zh}`,
  ""
].join("\n")).join("\n");

fs.mkdirSync(reviewDir, { recursive: true });
fs.writeFileSync(path.join(reviewDir, "grimstone-review.html"), html, "utf8");
fs.writeFileSync(path.join(reviewDir, "grimstone-review.txt"), text, "utf8");
console.log(`已生成 Grimstone 校对稿：${rows.length} 条 -> ${reviewDir}`);
