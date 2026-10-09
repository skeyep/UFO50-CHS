# 光标与文字排布检查

检查顺序为静态清点、原场景运行、绘制边界记录、自动碰撞定位、放大查看、原导航和确认交互。每个选择项分别采集；画面记录保存原 PNG，模型查看使用 JPG。

`scripts/audit-layout-draws.py --code-root <原版GML目录> --output-dir <审阅目录>` 清点光标、字符数乘8、逐字／逐行固定格、相邻8px文字行与语义字体。每项保留稳定代码位点、上下文和资源键；沿原调用建立场景，检查现有补丁与实际画面后记录处理结果。

`scripts/make-layout-closeups.py` 读取同帧的 `*-texts.json`、`*-sprites.json` 和原精灵的 alpha 边界，检测文字之间、文字与光标之间的矩形交叠。它按摄像机或 GUI 尺寸转换坐标，并处理文字对齐、精灵原点、缩放和旋转。结果写入 `layout-collisions.json`；交叠与选择项自动生成最近邻四倍放大的 JPG，文字框为青色、光标框为黄色、交叠为红色。

```powershell
py -3.14 scripts/make-layout-closeups.py --evidence-dir <实机证据目录> --sprite-metadata <opaque-bboxes.json>
```

文字记录须提供 `text/x/y/width/height`，坐标已经按 `halign/valign` 换算成左上角。精灵记录须提供 `sprite/frame/x/y/scaleX/scaleY/rotation/alpha`。两者均提供 `gui/viewX/viewY/viewWidth/viewHeight/guiWidth/guiHeight`；建议同时记录调用对象、事件、颜色、透明度和原始锚点。精灵元数据为数组，每项包含 `sprite/width/height/originX/originY/frames/opaque`，`opaque` 按帧保存非透明像素的 `[left,top,rightExclusive,bottomExclusive]`。

QA 驱动记录 `draw_sprite`、`draw_sprite_ext`、`draw_self` 及字符光标；动态精灵和默认对象绘制沿静态台账补充。每个可见光标均自动生成特写，并记录与最近标签的间距，覆盖没有交叠的选择状态。原精灵导出后检查 alpha 数据，透明帧和未解析精灵会进入待验列表。没有精灵 trace 的截图单列为缺记录。

特写先保存 PNG；本机存在全局 `screenshot_jpeg.py` 时通过该入口转成等尺寸 JPG。其他环境采用相同的质量 82、无色度降采样压缩参数。

自动报告给出待检查位置。矩形边界包括字符留白、精灵内部透明区，需结合特写确认；同址固定数字补零层另列在 `numericOverlays`，同时沿数字特写流程查看。纯色画面、裁切文字、零字宽／字高、中文字使用原 ASCII 字体、未解析精灵及缺 trace 分别记录。查看完整上下文和特写后，记录实际问题与修后同场结果。

原版闪烁光标的全透明帧按导出的 `pixels=0` 识别，没有可见边界；缺失帧资料继续列为未解析。源码追踪同时保存原版代码条目、调用序号、源码行和光标台账位点，区分同一 Draw 事件中使用相同精灵的多个菜单分支。新增调用或歧义映射单列待核对。

`scripts/validate-layout-targets.py` 检查关键截图中是否实际绘制了目标文字。场景清单按截图指定 `allOf`／`anyOf` 文字或资源键；验证器读取该次构建的语言资源，将同一调用对象的可见文字按绘制顺序拼接，支持逐字揭示、分行和动态占位符。目标缺失时场景失败，记录在 `layout-target-validation.json`。

```powershell
py -3.14 scripts/validate-layout-targets.py --evidence-dir <game证据目录> --expectations <expected-text-cases.json> --text-root <本次语言资源> --mode <场景> --game <内部ID>
```

运行完成、截图数量满足要求和碰撞检测完成分别记录。目标截图为纯色、出现零字宽／字高、中文字字体错误或未解析光标时，运行器将该场景列为失败并补采。视觉验收还需查看特写及执行原方向导航、确认或取消。测试代码保留在隔离构建中，正式补丁从原版基线独立构建。

`scripts/make-text-closeups.py` 按同一份目标清单主动定位正文，即使该位置没有光标或碰撞也生成特写。它将目标资源的实际文字映射到同帧绘制记录，合并逐字／分行的坐标，保存最近邻放大的 PNG，再调用全局 JPG 压缩入口。`text-closeups.json` 保存目标、调用对象、实际字框、裁切范围与 JPG 路径；找不到必要目标时返回失败。生成特写后逐张检查字形、行距、衬底及边框。

```powershell
py -3.14 scripts/make-text-closeups.py --evidence-dir <game证据目录> --expectations <expected-text-cases.json> --text-root <本次语言资源> --mode <场景> --game <内部ID> --jpeg-tool <全局screenshot_jpeg.py>
```
