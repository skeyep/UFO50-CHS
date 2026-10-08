# 维护与发布工作流

## 翻译

1. 从维护者合法持有的英文资源提取稳定键名和原文。
2. 参考官方日文确认称呼、语气和版面关系。
3. 由 GPT-5.6 Sol 逐条翻译和自校，维护者决定方向、术语并最终验收，在 `source/translations/` 维护键名驱动的中文值；禁止批量机翻服务和未经审核的机器占位。
4. 运行键数、占位符、控制符和布局字段审计；以官方 Zpix 8pt/96 DPI 的真实字宽模拟显式文本框，检查中文预测行数不得超过原版限制。
5. 生成 `payload/ext/JAPANESE/` 中文槽文件。
6. 在游戏内检查字体覆盖、基线、换行、图标和语境；优先复核静态审计判定“中文需要新增换行”的条目。

用户负责最终验收，但不承担项目翻译和校对工作。

## 格式与数字审阅

1. 用 `scripts/audit-format-contracts.py --output <报告.json>` 清点动态字段与字符上限。提供 `--code-dir <CodeEntries>` 时，同时导出日文参数顺序分支和标签绘制上下文；逐项检查调用者传参顺序、缩写及相邻数字位置。
2. 修复前在隔离游戏副本中复现，修复后复测同一场景及一次相关交互。保留独立存档、静音、禁用 Steam 和真实输入的测试流程。
3. 记录实际绘制字体、坐标、宽高和摄像机尺寸。纯数字与固定格属性沿原版字体、字距和遮罩绘制；中文标签按实测宽度预留间隔。
4. 用 `scripts/make-number-closeups.py --evidence-dir <证据目录>` 自动放大数字；同时保存所有文字的 `*-texts.json`，用 `scripts/audit-text-adjacency.py --evidence-dir <证据目录>` 识别同排标签与数字，输出间距及包含两者的特写。间距小于 4px 的条目进入人工复核清单。
5. 完整截图保留上下文，图片先保存本机，使用原尺寸压缩 JPG 和最近邻放大的 JPG 逐张查看。检查每位数字、正负号、补零、缩写、基线、间距及图标。工具报告与实际画面一起归档。

`*-texts.json` 的每条记录包含 `text/x/y/width/height/gui/viewX/viewY/viewWidth/viewHeight/guiWidth/guiHeight`；坐标和宽高取实际绘制时使用的字体与对齐状态。间距工具检查已记录的同排中文标签和独立数值，完整界面的图标、边框、逐字绘制和换行仍按场景检查。

## 描边与背景对比度

1. 用 `scripts/audit-outline-draws.py --code-dir <原版 CodeEntries> --evidence-dir <实机证据目录> --output <报告.json>` 清点绘制调用、字体来源和中文场景。原版精灵字体需查看实际字形，区分黑边、整格黑底和无描边字体。
2. 优先复现文字直接盖在天空、砖墙、地图、亮色菜单和选中条上的场景。中文字体保留原版请求字体的描边意图；普通、换行和彩色绘制共同检查。原本无描边、黑色字面、原版数字和图标分别沿原路径。
3. 保存同场景修改前、修改后和英文对照，自动生成保持像素边缘的最近邻 JPG 特写，逐张确认字面、黑边、边框、行距、颜色和透明度。已手绘描边的 helper 使用原生绘制，避免再次经过通用描边包装。
4. 运行数字字体校验和特写审阅，检查暂停／恢复后请求字体是否仍成对保存。实际看不到文字的转场、遮挡和画面外记录进入补测清单。

## data.win 构建

维护者需要在本地准备：

- 与 `release-config.json` 哈希一致的干净原版 `data.win`；
- 官方 UndertaleModTool CLI；
- 本仓库 `payload/patch-font.csx`。

示例：

```powershell
.\scripts\rebuild-data.ps1 `
  -BaselineDataWin C:\private\ufo50\data.win `
  -UtmtExe C:\private\utmt\UndertaleModCli.exe `
  -OutputPath C:\private\build\data.win
```

脚本会同时检查原版输入哈希和补丁输出哈希。私有输入与输出不得提交。

## Release

1. 更新 `release-config.json` 的版本、支持原版哈希和预期补丁哈希。
2. 运行 `scripts/validate-repository.ps1`。
3. 使用私有原版基线完成一次 `rebuild-data.ps1`。
4. 运行 `scripts/update-manifest.ps1` 更新仓库核心文件清单。
5. 运行 `scripts/build-release.ps1`：构建机下载并校验官方原版 Zpix、官方 UndertaleModTool CLI 及对应 GPLv3 源码，生成可完全离线安装的 ZIP 和 SHA-256。
6. 在隔离目录中放置原版 `data.win` 与一个测试用 `ufo50.exe` 占位文件。
7. 断开网络或阻断下载后从 ZIP 执行安装，核对补丁哈希、字体哈希和 52 个文本文件。
8. 执行卸载，确认 `data.win` 精确恢复到原版哈希。
9. 将 ZIP 和 `.sha256` 上传到对应 GitHub 预发行版。

v0.1.0 是首次跑通全流程的测试版；v0.1.1 完成第一轮全局换行、行距、图标混排和居中修复；v0.1.2 将《摇滚岛》开场等显式切行界面纳入真实文本框与运行时行高规则；v0.2.0 完成第 51 款《瘴气塔》的谜题安全汉化，并将其纳入元数据和布局审计。后续问题通过 Issue 记录，修复通过 PR 合并，并由新的 Release 分发。
