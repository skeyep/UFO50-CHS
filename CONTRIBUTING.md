# 参与贡献

感谢帮助改进 UFO 50 简体中文汉化。

## 提交问题

请优先使用仓库的 Issue 模板，并尽量提供：

- 游戏名和具体界面；
- 当前补丁版本；
- 原文、现译文和建议译文；
- 截图与稳定复现步骤；
- 是否使用旧档案、键盘或手柄。

不要上传 `data.win`、游戏 EXE、原始游戏资源、字体文件或整套反编译输出。

## 翻译原则

- 以英文原文为语义基准，并参考官方日文语境。
- 所有新增译文必须由贡献者逐条理解、翻译和自校。
- 禁止提交未经核对的机器翻译批量稿。
- 保留键名、占位符和控制符，例如 `*`、`**`、`@`、`^`、`[1]`、`[2]`、`{0}`。
- 优先使用常用、清楚、符合游戏语气的中文；生僻字应考虑像素字体覆盖和可读性。
- 不因个人偏好随意统一角色口吻、专名或机制术语；改动时说明语境依据。

## Pull Request

1. 从 `main` 创建独立分支。
2. 只提交本次改动涉及的译文、脚本或文档。
3. 运行 `powershell -File scripts/validate-repository.ps1`。
4. 在 PR 中说明改动原因、影响范围、校对依据和验证结果。

涉及游戏画面的改动，请附上中文槽截图；涉及安装器的改动，请完成隔离安装和卸载恢复测试。

## 构建和检查译文

维护源码需要 Node.js 24 和 PowerShell。游戏参考资源在本地提供，`source/translations/` 是当前译文源；构建产物默认写入 `dist/`。

`--reference-root` 指定参考工程，其下需要 `ext/ENGLISH/`、`reference/JAPANESE-original/`，以及 `chs-tools/all-code/CodeEntries/gml_GlobalScript_scrLoadInternalText.gml`。也可用 `--meta-gml` 单独指定元数据参考脚本。

```powershell
node source/builders/build-game.mjs 8 --reference-root '<参考工程>'
node source/builders/audit-game.mjs 8 --reference-root '<参考工程>'
node source/builders/build-menu.mjs --reference-root '<参考工程>'
node source/builders/build-grimstone.mjs --reference-root '<参考工程>'
node source/builders/build-meta.mjs --reference-root '<参考工程>'
node source/builders/audit-game51.mjs --reference-root '<参考工程>'
```

`--output-root` 可指定文本输出目录，`--review-root` 可指定对照稿目录。构建完成后比较 `dist/text/JAPANESE/` 与 `payload/ext/JAPANESE/`，将实际复测通过的资源同步到载荷，再更新清单。仓库检查和 CI 会逐键核对译文源与载荷。

布局检查使用 Python 及 `fontTools`，以原版 8px 格宽和 Zpix 实际字号估算换行：

```powershell
python source/builders/audit-layout.py --reference-root '<参考工程>' --text-root payload/ext/JAPANESE --font '<Zpix 字体路径>'
```

检查报告默认写入 `dist/review/layout-audit.tsv`。显式逐行绘制、图形文字、逐字动画及图标混排需要隔离实机复测。修改《诡石镇》译文时直接编辑 `source/translations/grimstone-zh-cache.json`；历史批量润色脚本已移除。

## 数字显示验收

数字审阅同时检查原始绘制调用和实机画面，覆盖分数、资源、价格、计时器、百分比、倍率、补零遮罩和逐位数字。动态变量也应进入清单。纯数字及其单位符号使用调用者原先请求的精灵字体，保留原版字距、基线和居中规则；中文正文使用 Zpix。

截图先生成保持原尺寸的 JPG。实机验收应自动记录数字绘制区域，另外生成以最近邻放大的数字特写 JPG，并逐张查看。特写标明游戏和场景，保留完整截图用于检查边框、图标及中文标签。检查 `0/1/9`、一位与多位数、补零、进位、价格和时间变化，以及暂停后恢复的字体状态。

绘制补丁构建时检查所有原生文字调用已进入统一路由；特殊图标字体与固定格计数器使用明确的独立绘制函数。字体路由修改后还应进行原版字体像素对照、中文界面复测和英文槽回归。

```powershell
python scripts/audit-numeric-draws.py --code-root '<私有 GML 参考目录>' --output-dir dist/review/numbers
python scripts/make-number-closeups.py --evidence-dir '<实机证据目录>'
python scripts/validate-number-evidence.py --evidence-dir '<实机证据目录>'
```

数字特写脚本需要 Pillow，并读取与截图同名的 `*-numbers.json` 绘制记录；记录包含文字、字体、边界、摄像机和 GUI 尺寸。静态清单中的候选及动态项逐项进入实机场景复核。

标签与数字挤在一起的问题，使用 `scripts/audit-text-adjacency.py` 从所有文字的 `*-texts.json` 记录识别相邻项，生成包含标签与数字的特写及实际间距。字段限长和英日动态参数顺序用 `scripts/audit-format-contracts.py` 清点；流程及记录格式见 [维护与发布工作流](docs/WORKFLOW.md#格式与数字审阅)。

字体检查另外要求记录 `currentIsCHS`，发现纯数字仍使用中文字体时返回失败。临时提示层及暂停菜单必须成对保存、恢复当前字体和原版请求字体；只恢复中文字体句柄会丢失数字样式。检查通过后仍需逐张查看放大特写，包括自动记录在画面外、延迟出现和特殊场景的条目。

特写报告保留区域坐标、同一区域的所有绘制文本和放大倍率。补零底层与覆盖数值落在同一区域时，特写标题使用最后一次绘制值；纯色区域标记为 `uniform`，须补到实际可见状态。字体对照测试要在对应游戏资源已加载时执行。
