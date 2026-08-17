# 维护与发布工作流

## 翻译

1. 从维护者合法持有的英文资源提取稳定键名和原文。
2. 参考官方日文确认称呼、语气和版面关系。
3. 由 GPT-5.6 Sol 逐条翻译和自校，维护者决定方向、术语并最终验收，在 `source/translations/` 维护键名驱动的中文值；禁止批量机翻服务和未经审核的机器占位。
4. 运行键数、占位符、控制符和布局字段审计；以官方 Zpix 8pt/96 DPI 的真实字宽模拟显式文本框，检查中文预测行数不得超过原版限制。
5. 生成 `payload/ext/JAPANESE/` 中文槽文件。
6. 在游戏内检查字体覆盖、基线、换行、图标和语境；优先复核静态审计判定“中文需要新增换行”的条目。

用户负责最终验收，但不承担项目翻译和校对工作。

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

v0.1.0 是首次跑通全流程的测试版；v0.1.1 完成第一轮全局换行、行距、图标混排和居中修复；v0.1.2 将《摇滚岛》开场等显式切行界面纳入真实文本框与运行时行高规则。后续问题通过 Issue 记录，修复通过 PR 合并，并由新的 Release 分发。
