# v1.2.0 译文修改列表

本轮参考官方七语并结合原代码与实际场景校对。以下列出全部93处修改；大部分标题与既定创意命名沿用。

|位置|修改前|修改后|原因|
|---|---|---|---|
|0/info_controls_23_2|空中按 [2：扣篮|空中按 [2：下砸|七语 DUNK 在 Pingolf 高尔夫语境是球向下加速。o23_Ball_Other_11:1–20 fire2pressed && dunkReady 后 SET_YSPEED max(4,ysp+4)，没有篮球篮筐。|
|0/misc_demo|试玩版|演示|七语为 DEMO/JPデモプレイ。oGame_Draw_0:65 检查 attractModeLibrary 后在 :72 绘制，是闲置游戏演示，不是软件试玩版本。|
|6/cybergopher_intro_2|太迟了！你沉睡多年时，我只会变得更强！球道将属于我！|太迟了！你沉睡的这些年里，我已变得更强！球道将属于我！|EN have grown stronger 与六语均为沉睡期间力量已经增长；现译“我只会变得更强”把既成事实写成未来趋势。|
|6/parbot_log_10|“我们无法留下等待凯蒂的牺牲是否奏效。今天起，我们将躲入地下。”*哔*|“我们无法等到确认凯蒂的牺牲是否奏效。今天起，我们将躲入地下。”*哔*|原文无法继续等着观察牺牲是否奏效；现译“等待…是否”句法残缺。改为等到确认保持日志时间线，与地下避难所台词衔接。德文把 cannot wait 理解为迫不及待，与 EN/JP/IT 上下文不符。|
|7/boss_final_intro_4|而你，就是我以他们之名斩杀的第一个祭品！消失吧！|而你，就是我要以他们之名斩杀的第一个敌人！消失吧！|七语为第一个斩杀对象，JP/FR/IT 显示敌人/阻碍者。现译“祭品”添了祭祀语义；前条以自己为利刃、后条 boss 开战说明是杀敌宣言。|
|7/game_outro_3_gold|R-维因格号注视着胡珀。装甲头盔遮住了双眼，无人能看透驾驶者的神情。|R-维因格注视着胡珀。装甲头盔遮住了双眼，无人能看透驾驶者的神情。|主角是可穿戴装甲：terminal_mission_1_1 七语 armor；game_outro_3_gold suit/helm/JP装着者；terminal_clones wearer。型号 R/Y 保留，去除飞船命名后缀“号”，保持游戏标题与人名。|
|7/intro_1|R-维因格号——佣兵企业“战属公司”的杰作——在机械卫星伊奥的休眠舱中苏醒……|R-维因格装甲——佣兵企业“战属公司”的杰作——在机械卫星伊奥的休眠舱中苏醒……|主角是可穿戴装甲：terminal_mission_1_1 七语 armor；game_outro_3_gold suit/helm/JP装着者；terminal_clones wearer。型号 R/Y 保留，去除飞船命名后缀“号”，保持游戏标题与人名。|
|7/intro_3|R-维因格号唯一的家园，如今已化为敌人。|R-维因格唯一的家园，如今已化为敌人。|主角是可穿戴装甲：terminal_mission_1_1 七语 armor；game_outro_3_gold suit/helm/JP装着者；terminal_clones wearer。型号 R/Y 保留，去除飞船命名后缀“号”，保持游戏标题与人名。|
|7/terminal_armory_room|接◆受……吾之力力力……同◆意……献◆上你的一切切切……——？？？|HHCRC OENT FI FNEAETH ACCEPTUSS... AGEERH POWWWWRRR... TAEHGAN THEAX... FAEAEI IYRSHL TEPAPPTGH... -???|七语共同保留外星乱码段，官译只把可辨认词局部音近转写。现中文将乱码全解为“同意/献上一切”，添加未知内容并消除未解读状态；按本次加密语言规则保留英文资源整段及原字体。|
|7/terminal_bio_savepoint_lower|别担心。只有伊普西隆博士能驾驶R-维因格号，而我已经确保博士继续处于休眠状态。——胡珀|别担心。只有伊普西隆博士能驾驶R-维因格，而我已经确保博士继续处于休眠状态。——胡珀|主角是可穿戴装甲：terminal_mission_1_1 七语 armor；game_outro_3_gold suit/helm/JP装着者；terminal_clones wearer。型号 R/Y 保留，去除飞船命名后缀“号”，保持游戏标题与人名。|
|7/terminal_boss_hang1|伊普西隆博士在新型R-维因格号原型机的演习中受了危及生命的重伤。|伊普西隆博士在新型R-维因格原型机的演习中受了危及生命的重伤。|主角是可穿戴装甲：terminal_mission_1_1 七语 armor；game_outro_3_gold suit/helm/JP装着者；terminal_clones wearer。型号 R/Y 保留，去除飞船命名后缀“号”，保持游戏标题与人名。|
|7/terminal_camera_control|R-维因格号拥有强化感知能力。短暂向上或向下观察，即可看见更远处。——伊普西隆|R-维因格拥有强化感知能力。短暂向上或向下观察，即可看见更远处。——伊普西隆|主角是可穿戴装甲：terminal_mission_1_1 七语 armor；game_outro_3_gold suit/helm/JP装着者；terminal_clones wearer。型号 R/Y 保留，去除飞船命名后缀“号”，保持游戏标题与人名。|
|7/terminal_clones|R-维因格号能生成驾驶者的克隆体，这项能力极为强大。在伊奥，每次穿过门时都会采集DNA。——伊普西隆|R-维因格能生成驾驶者的克隆体，这项能力极为强大。在伊奥，每次穿过门时都会采集DNA。——伊普西隆|主角是可穿戴装甲：terminal_mission_1_1 七语 armor；game_outro_3_gold suit/helm/JP装着者；terminal_clones wearer。型号 R/Y 保留，去除飞船命名后缀“号”，保持游戏标题与人名。|
|7/terminal_temperature|若不进一步改装，R-维因格号缺乏抵御极端温度所需的防护。——伊普西隆|若不进一步改装，R-维因格缺乏抵御极端温度所需的防护。——伊普西隆|主角是可穿戴装甲：terminal_mission_1_1 七语 armor；game_outro_3_gold suit/helm/JP装着者；terminal_clones wearer。型号 R/Y 保留，去除飞船命名后缀“号”，保持游戏标题与人名。|
|7/terminal_zero|新型R-维因格号配备了改良的永久模块“重力翻转”，在空中再次跳跃即可启动。——伊普西隆|新型R-维因格配备了改良的永久模块“重力翻转”，在空中再次跳跃即可启动。——伊普西隆|主角是可穿戴装甲：terminal_mission_1_1 七语 armor；game_outro_3_gold suit/helm/JP装着者；terminal_clones wearer。型号 R/Y 保留，去除飞船命名后缀“号”，保持游戏标题与人名。|
|7/terminal_zone2_bottom_center|启动Y-维因格号，在战斗模拟中测试她的性能吧。——胡珀|启动Y-维因格，在战斗模拟中测试她的性能吧。——胡珀|主角是可穿戴装甲：terminal_mission_1_1 七语 armor；game_outro_3_gold suit/helm/JP装着者；terminal_clones wearer。型号 R/Y 保留，去除飞船命名后缀“号”，保持游戏标题与人名。|
|7/terminal_zone4_3|胡珀疯了。我只剩一件事可做……解除伊普西隆博士的休眠，让博士进入R-维因格号。永别了。——吴|胡珀疯了。我只剩一件事可做……解除伊普西隆博士的休眠，让博士进入R-维因格。永别了。——吴|主角是可穿戴装甲：terminal_mission_1_1 七语 armor；game_outro_3_gold suit/helm/JP装着者；terminal_clones wearer。型号 R/Y 保留，去除飞船命名后缀“号”，保持游戏标题与人名。|
|m/game_description_11|穿越森林、洞穴与群山，寻找那颗蛋。|跳跃冲刺，穿越森林、洞穴与群山，寻找那颗蛋。|英语及法、德、意、葡、西都明确 JUMP AND DASH；补全两种移动动作。|
|m/game_description_43|拿上豆袋，到烫脚球场上一决高下！|拿上沙包，到烫脚球场上一决高下！|Beanbag 为投掷用沙包，与游戏教程、角色能力的既定术语统一。游戏标题保持原有名称。|
|m/game_description_46|梅隆买了台新相机，为了试拍踏上银河冒险！|梅隆得到了一台新相机，为了试拍踏上银河冒险！|GOT／てにいれた等原文未交代购买；去掉凭空添加的购买方式。|
|m/game_description_7|维因格号重返伊奥，却发现自己遭到背叛。找出敌人，彻底消灭。|维因格装甲重返伊奥，却发现自己遭到背叛。找出敌人，彻底消灭。|Vainger 指可穿戴装甲。正文 terminal_mission_1_1、terminal_clones、game_outro_3_gold 分别明确 armor、wearer、helm；移除飞船后缀“号”。|
|m/game_history_42|为防止玩家一味防守、令比赛乏味，擂台机制在开发后期才加入。|为防止玩家一味防守、令比赛乏味，圆环机制在开发后期才加入。|Ring 为胜负规则的圆环道具，与 collect_rings／rings_to_win 一致；o42_Game_Create_0 定义 OBJECTIVE_RING、DEFAULT_RINGS_REQUIRED，不是擂台。|
|m/game_meta_message_1|那是我们成功的最后机会。我把一切都炸毁了吗？|那是我们成功的最后机会。我把一切都搞砸了吗？|原文 blow it all up 与炸弹兵有双关；日语及其余五语表达搞砸成功机会。实际在最后一命炸弹兵选择页按上显示，采用“搞砸”传达留言的本义。|
|10/octo_warningTextNear|雷达发现附近有个大家伙……|探测到附近有个大家伙……|EN picking up/JP巨大な生き物いるみたい及五语均未指定雷达。o10_eOcto_Step_0:31–34 只检查 scrOnScreenEx(128) 和 warningRange==0，未要求雷达装备。|
|12/camo_end|迷彩的效果消失了。|好运已经用尽了。|七语 GOOD FORTUNE HAS RUN OUT/しゅくふくはきえた；o12__Game_Other_14:284–290 盐与 :829–833 迷彩共用 camoCount。o12_Player_Step_0:184–189 归零统一显示 camo_end，不能只称迷彩效果。|
|12/enemy_SKINWALKER|行皮者|皮行者|七语民俗名称 SKINWALKER；修正原语序，保持同一敌人身份。|
|12/item_fBANDANA|头巾|方巾|f 类经 ItemCheckType:73 / ItemCanEquip:37 属颈部饰物；方巾覆盖颈部佩戴，避免引入头部装备槽。|
|12/item_hCOTTON|棉帽|棉衣|ItemCheckType:81 / ItemCanEquip:40 将 g/h 同归 ARMOR；h 是图标类别，不是 head；按身体槽与七语材质/王室名称修正为衣服。|
|12/item_hLEATHER|皮帽|皮衣|ItemCheckType:81 / ItemCanEquip:40 将 g/h 同归 ARMOR；h 是图标类别，不是 head；按身体槽与七语材质/王室名称修正为衣服。|
|12/item_hROYAL|王冠|王室服|ItemCheckType:81 / ItemCanEquip:40 将 g/h 同归 ARMOR；h 是图标类别，不是 head；按身体槽与七语材质/王室名称修正为衣服。|
|12/item_hSILK|丝绸帽|丝绸衣|ItemCheckType:81 / ItemCanEquip:40 将 g/h 同归 ARMOR；h 是图标类别，不是 head；按身体槽与七语材质/王室名称修正为衣服。|
|12/item_hWOOL|羊毛帽|羊毛衣|ItemCheckType:81 / ItemCanEquip:40 将 g/h 同归 ARMOR；h 是图标类别，不是 head；按身体槽与七语材质/王室名称修正为衣服。|
|12/item_kFOCUS|专注道具|专注知识|Other_14:597–628 使用知识物品后才学会 FOCUS 并消耗物品；NPC 对白明确 knowledge，不是泛指道具。|
|12/item_kMINE|采矿工具|采矿知识|Other_14:557–588 使用知识物品后才学会 MINE 并消耗物品；NPC 对白明确 knowledge，不是采矿工具。|
|12/npc_aust_sheriff_03b|（获得技能‘专注’。）|（获得专注知识。）|七语 obtained skill 指技能知识奖励；o12_mapNPC_Step_0:1974 实际 scr12_InvAdd("k FOCUS", SOUND_ITEM_BUY)，:1989 才显示本提示。需另行在背包学习，与 item_kFOCUS 统一。|
|12/npc_miner_03b|（获得技能‘采矿’。）|（获得采矿知识。）|七语 obtained skill 指技能知识奖励；o12_mapNPC_Step_0:1895 实际 scr12_InvAdd("k MINE", SOUND_ITEM_BUY)，:1910 才显示本提示。需另行在背包学习，不是在此获得已学会技能。|
|12/pre_gold_record|用时 **********|位于 **********|scr12_SaveGame:132 将 latestTown 经 GetTownName 保存为纪录地点；IN 与七语地方介词的参数不是耗时。|
|12/skill_CHARM|魅力|魅惑|SetupSmallEnemy:320/338 CHARM 效果 CONFUSE；主动技能造成混乱，魅惑比名词魅力准确。|
|12/skill_FLASK|烧瓶|酒壶|CreateEnemies:442/455 D.CAPTAIN/D.SOLDIER FLASK 效果 HEAL_SELF；日スキットル为随身酒壶。|
|12/skill_TENSE|紧张|蓄力|CreateEnemies:507 ALLIGATOR TENSE 效果 RAISE_SELF_ATK，日パワー；绷紧身体提高攻击，改蓄力。|
|12/skill_WAVE|波涛|波动|CreateEnemies:93 R.HAND WAVE 效果 HEAL_ALLY；六欧语波动含义，日なぎはらい分歧，波动避免误添水属性。原效果动画由主 Agent 验证。|
|12/status_hurt|受伤|重伤|PartyIsHurt:3 低于1/3 HP且不足100；显示处排除HP0死亡；日ひんし，重伤体现状态阈值。|
|12/use_item_focus_noskill|* 尚未掌握学习‘专注’所需的技能！|* 的 SP 上限不足，无法学习‘专注’！|Other_14:608 实际判断 spMax < SkillGetCost(FOCUS)，失败原因是 SP 上限而非前置技能。|
|12/use_item_mine_noskill|* 尚未掌握学习‘采矿’所需的技能！|* 的 SP 上限不足，无法学习‘采矿’！|Other_14:568 实际判断 spMax < SkillGetCost(MINE)，失败原因是 SP 上限而非前置技能；七语泛称能力，按实际条件明确。|
|14/breeder_greeting_2|我这儿有几只上好的新品种！|我这儿有几只新来的好苗子！|七语为 fine new specimens/いいクイブル，不是新品种。scr14_GetSponsee:6–39 从已有 quibbles 随机筛出未来参赛且未获赞助者；各自有既存比赛记录，没有生成新物种。|
|15/intro|在宇宙深处的某个地方……太空海盗阿尔法被恶魔公主查卡斯俘获，扔进名为维尔格莱斯的无底深坑。从来没人能活着离开维尔格莱斯，那里遍布致命的陷阱与怪物。所幸阿尔法熬过漫长坠落，还找回了自己的爆能枪。|在宇宙深处的某个地方……太空海盗阿尔法被恶魔公主查卡斯俘获，扔进名为维尔格莱斯的深坑。从来没人能活着离开维尔格莱斯，那里遍布致命的陷阱与怪物。所幸阿尔法熬过漫长坠落，还找回了自己的爆能枪。|七语开场讲角色被投入很深的坑并熬过坠落；无底扩译与有底的地牢叙事冲突。EN DEEP PIT，日文 奈落の底，其余语种没有无限深度限定。|
|19/bg_ufo|某种船只的残骸，已被珊瑚与海藻彻底包裹。|某种载具的残骸，已被珊瑚与海藻彻底包裹。|原room02在400,1408生成num10调查物件，绑定s19_bgProp10及bg_ufo；EN some form of vessel刻意不确定载具种类，JPのりもの同样宽泛。采用载具避免额外限定水面船只。|
|21/item_desc_1|安全试跳，不怕失足|用球试探跳跃路线|原ITEM_BEACH_BALL创建o21_Beachball，复制jumpStrength/aimerAngle并消耗球，保留holdX/holdY且暂时关闭Waldorf操控；不是人物获得防跌保护。球与人物使用相同0.05重力。JP明确跳跃前确认安全。|
|21/item_desc_3|暂时减缓重力|暂时减缓下落|原GRAVITY恒为0.05；气球把TERMINAL_VELOCITY由8改0.3，而不是减小重力加速度。JP说跳跃后缓慢落下，现候选表达实际行为。|
|30/gear_medkits_desc_1|每 30 秒获得一个医疗包。|每 30 秒出现一个医疗包。|原GEAR_DESC[MEDKITS][0]是购买一级升级前的说明。装备一级时原战场事件达到1800帧创建Medkit；HP直到玩家碰撞且spawnTimer归零才增加30。说明改出现，避免让玩家期待自动治疗。|
|30/gear_medkits_desc_2|每 15 秒获得一个医疗包。|每 15 秒出现一个医疗包。|原GEAR_DESC[MEDKITS][1]是二级升级前的说明；原战场事件达到900帧创建Medkit，仍须手动接近拾取才能恢复HP。与一级统一表达，保留15秒差别。|
|35/chest_find_small|里面装着一小块{0}！|里面装着一颗小{0}！|原代码此模板仅用于 contains=GEMS（物品40宝石），与 item_gem_plural 的“颗”统一，保留 SMALL 大小信息。|
|35/item_sapphire_inspect|一颗硕大的蓝宝石，如海浪般粼粼生辉。|一颗硕大的蓝宝石，如海浪般闪闪发光。|七语均为如海浪般闪光；用“闪闪发光”替换笔画多的“粼粼生辉”，保留完整比喻。|
|35/outro_6_TRUE|你伸手从口袋里取出一颗闪亮的小星星，熠熠生辉，仿佛刚从夜空中摘下。|你伸手从口袋里取出一颗闪亮的小星星，闪着光，仿佛刚从夜空中摘下。|七语 glimmering/shining 语义一致；“闪着光”替换笔画多的“熠熠生辉”，保留星光和夜空意象。|
|35/setpiece_void_examine_entered|远处矗立着一根石柱，上方夜空繁星闪烁。|远处立着一根石柱，上方夜空繁星闪烁。|七语一致描述远处石柱和其上星空；“立着”替换笔画多的“矗立着”。|
|37/they_left_bench|好。他们离开工作台，欢快地吹起口哨。|好。这位朋友离开工作台，欢快地吹起口哨。|英文单数 they，意葡西文按单个朋友表述；原代码停工归还一名 HelperFriend 并令 friends 加1，原译“他们”误为多人。|
|37/they_will_keep_making_ingots|好。他们会继续生产月锭。|好。这位朋友会继续生产月锭。|英文单数 they，意葡西文按单个朋友表述；原代码停工归还一名 HelperFriend 并令 friends 加1，原译“他们”误为多人。|
|38/npc_52|恶！我还以为永远都甩不掉背上那只水蛭了。|真恶心！我还以为永远都甩不掉背上那只水蛭了。|七语均为摆脱背上水蛭后的厌恶语气，原译“恶！”是单字形容词，中文反应不自然。明确厌恶语气并保留back这一身体部位。|
|39/look_bathroom_sink|水槽几乎要被腐臭的褐色污水漫出来。|水槽里的腐臭褐色污水几乎要漫出来。|七语均描述水槽几乎溢出污水；修正“水槽要被污水漫出来”的主宾错误。|
|39/look_childrens_desk|小书桌上堆满稚拙的蜡笔画，以及各门小学课程的课本。|小书桌上堆满粗浅的蜡笔画，以及各门小学课程的课本。|crudely drawn 指画得不成熟；用常用的“粗浅”保留画作稚嫩的意思，避免换成笔画更多的“粗糙”。|
|39/look_computer_windows|屏幕上层叠显示着几个数字窗口。|屏幕上层叠显示着几个窗口。|EN digital boxes及六语描述屏幕的数字化窗口，不是显示数字的窗口；删除“数字”歧义。|
|39/look_upper_bath_faucet|一只锃亮水龙头装在水槽上方。|一只闪亮水龙头装在水槽上方。|七语 shiny/glänzend，常用“闪亮”保留光泽，替换像素下难认的“锃亮”。|
|39/more_incapacitate|……随即瘫倒在地。确认他不再动弹后，你搜查尸体，找到一把生锈的铁钥匙。|……随即瘫倒在地。确认他不再动弹后，你搜了搜他身上，找到一把生锈的铁钥匙。|七语均只确认不再动弹并搜身，body/Körper/体不提前断言死亡。原代码命中弩箭后killerTime=-1、销毁杀手，再在substate3搜身取铁钥匙；此处按照旁白观察措辞。|
|39/room_attic|昏暗阁楼仿佛被时间遗忘。地板积满灰尘，头顶椽木也被蛛网缠住。|昏暗阁楼仿佛被时间遗忘。地板积满灰尘，头顶木梁也被蛛网缠住。|七语rafters/たる木/chevrons/travi均指屋顶木构，改用“木梁”替换生僻“椽木”，保留梁上蛛网。|
|39/room_backyard|寂静夜色中，小动物穿过后院疯长荒草的窸窣声清晰可闻。|寂静夜色中，小动物穿过后院疯长荒草的沙沙声清晰可闻。|七语rustling/ガサガサ均为草丛窸窣响声，改用易认的“沙沙声”，保留小动物和荒草声音。|
|39/room_front_entrance|庞大宅邸仿佛挑衅般矗立在面前。你只想尽快远离这里。|庞大宅邸仿佛挑衅般立在面前。你只想尽快远离这里。|七语stands before you，改用“立在”替换笔画多的“矗立在”，保留宅邸压迫和挑衅意象。|
|39/room_shed|木棚狭小逼仄，令人喘不过气。每一处表面似乎都覆着木屑。|木棚狭小拥挤，令人喘不过气。每一处表面似乎都覆着木屑。|七语cramped/claustrophobic，使用常用的“拥挤”替换难认的“逼仄”，保持狭窄压迫感。|
|39/use_crossbow_mirror|余光瞥见动静，你立刻扣动十字弩！这才发现那只是镜中的自己……糟糕。|余光看见动静，你立刻扣动十字弩！这才发现那只是镜中的自己……糟糕。|七语out of corner of eye，保留“余光”明确观察位置，用“看见”替换复杂的“瞥见”。|
|39/use_drawers|抽屉里只有寥寥几本读物。|抽屉里只有少量读物。|七语a small selection/いくつか描述数量少，用“少量”替换难认的“寥寥”，保留少量读物。|
|39/use_flashlight_dead|你反复按动手电开关，却没有一丝光亮。电池没电了。|你反复按动手电开关，却没有一丝光亮。|七语 It is dead/没亮均未确认故障原因；look_flashlight明确不知道电池还是灯泡故障。原代码仅判item1==THE FLASHLIGHT并播放开关音，没有电池判定，删除提前断言“电池没电”。|
|39/use_gear_gearbox|落地钟里的金属齿轮与这组齿轮完美啮合。|落地钟里的金属齿轮与这组齿轮完美咬合。|七语fits perfectly/かみあった，在齿轮语境“咬合”保留机械配合含义且比“啮合”易认。|
|39/use_knife_bundle|你用那把可靠的刀划开布料，割破了包裹。里面是一具尸体，手中还紧紧攥着什么。|你用那把可靠的刀划开布料，割破了包裹。里面是一具尸体，手中还紧紧握着什么。|七语clasping tightly/にぎりしめた，使用“握着”替换笔画多的“攥着”，保留紧握和未知物品。|
|39/use_piano_ready|你开始弹琴，到第十六个音符时，一根琴弦铮然断裂。你伸手将断弦抽了出来。|你开始弹琴，到第十六个音符时，一根琴弦发出一声脆响，断裂了。你伸手将断弦抽了出来。|七语snap/twang/ブチッ指弦断裂声，以常用“脆响”替换“铮然”，保留第十六音和抽弦的机制线索。|
|39/use_steel_trunk|你忐忑地打开汽车后备厢。里面除了一些暗色污迹外空无一物，你又将它关上。|你紧张地打开汽车后备厢。里面除了一些暗色污迹外空无一物，你又将它关上。|七语Nervously/恐怖にみがまえながら，改用“紧张”替换不易认的“忐忑”，保持打开后备厢前的不安。|
|39/use_upper_bath_cabinet|柜子里有各种洗漱用品和浴室清洁剂。一瓶过氧化物引起你的注意，你将它带走。|柜子里有各种洗漱用品和浴室清洁剂。一瓶双氧水引起你的注意，你将它带走。|原七语peroxide/かさん化水素/acqua ossigenata实际是双氧水；与item_peroxide和use_peroxide_bowl统一，避免同一物品在取得与使用时改成上位类别“过氧化物”。|
|41/algean12_q2|在那里，我和有生以来见过最英俊的藻族四目相对……|在那里，我和有生以来见过最英俊的藻族对上了眼……|原Algean12的requestText[1]是遇见另一藻族并希望送情书的叙述，非描述人类。EN特意LOCKED EYE而非EYES，其他六语均不加入四个眼睛；用对上了眼保留相视语义并移除原文没有的眼数。既定族名与恋爱语境保留。原NPC眼部视觉仍由root fixture核验。|
|41/algean_wise_q2|我已像穴居生物一样双目失明……|我已像穴居生物一样看不见了……|原Teacher的requestText[1]为失去视觉；EN BLIND/JP失去光及其他五语均未添两眼。看不见了表达完全失明、保留穴居生物比喻，适用于原NPC外形。新增眼数不帮助原语义，删去双目即可。原NPC眼部视觉仍由root fixture核验。|
|41/ant_bureaucrat_c1|文官：请别伤害我！我只是个文官！|文官：请别伤害我！我只是个平民！|称谓BUREAUCRAT对应文官保留；其首次原completedText[0]说CIVILIAN，JPミンカンアリ及六语区分非军人身份。把后半句复译文官丢失正在请求不要攻击平民的理由。|
|41/lampian_random_7|灯民：绘中人的按钮下面，住着一个满脑子怪念头的异端。|灯民：绘中人的纽扣下面，住着一个满脑子怪念头的异端。|原bg41_InfinitesimalBig2人物画像穿马甲，三枚圆纽扣；画像tile在rm41_Small1600,864，纽扣下原ShrinkZoneRoomAuto1968,1568。原GotoMicroRoom(1976,1571)进入MicroLamp5，连接室实际LampianExile656,456。BUTTONS在这里是衣服纽扣，不是操作按钮。|
|42/pre_gold_record|等级 *|关卡 *|原 scr42_SaveGame 保存锦标赛最高关卡，并非角色等级；改为“关卡”，保留占位符。|
|43/close_game|险胜！|比分接近！|七语描述比分接近；结果界面胜负双方共用，现译在你输了下仍显示险胜。|
|43/tutorial_extra_5|强化期间，全速奔跑时掷出沙包。|强化期间，蓄满力后掷出沙包。|七语提示投掷速度；实际特殊投掷由蓄力计时判定，现译错误要求全速奔跑。|
|45/guin_hack_done|系统已将你锁在外面。|系统已禁止你继续访问。|七语指被计算机系统拒绝访问；现译像被关在建筑物外，与终端黑客操作语境不符。|
|46/enemy_6|切割机|直升机|EN CHOPPER法意西均为直升机；root已导出并查看s46_eChopper为头顶旋翼橙色飞行器，当前切割机误取chopper另一词义。|
|46/pre_gold_record|等级 *|关卡 *|原 scr46_SaveGame 保存最高到达关卡，并非角色等级；改为“关卡”，保留占位符。|
|47/detail_1|单局最多生命数|同时最多生命数|EN的MOST LIVES AT ONCE、JA最大生命数及其他五语均指某一刻持有的生命峰值。原译“单局最多”容易读成一局累计获得；CreditCoin Step 比较当前credit与mostLives，存档统计跨局保留。|
|48/nature_0_msg_8|只有尖锐的弹射物才能安全击碎水晶。|只有尖头弹丸才能安全击碎水晶。|七语均指尖锐射弹；实际为射钉枪的钉弹，弹射物表达易误解。|
|49/cut_name_5|最终二人|最后两人|七语均指最终留在竞赛中的两人；原译“最终二人”生硬，改成自然中文并保持字幕短标题用途。|
|51/game_51_interact_bola_trash_1|垃圾桶是空的，但你似乎瞥见它后面的地上有什么东西。|垃圾桶是空的，但你似乎看见它后面的地上有什么东西。|七语均为仿佛看见垃圾桶后东西；“看见”替换复杂字“瞥见”，保留似乎的不确定性。|
|51/game_51_interact_file_cab_21|ZANG, HOBART。管理员兼藏品经理。|ZANG, HOBART。管理员兼催收经理。|EN COLLECTIONS MANAGER 有藏品/催收歧义，但法文 recouvrement、德文 Geld eintreiber、意文 riscossore、葡文 cobranças、西文 pagos 均明确收账催款，日文回収係；原译藏品经理偏离全部官方译本。|
|51/game_51_interact_front_door|门外一片漆黑，令你猝不及防。刚才不还是白天吗？|门外一片漆黑，让你吃了一惊。刚才不还是白天吗？|七语均指外面突然天黑带来的惊讶；用“吃了一惊”替换复杂字“猝不及防”。|
|51/game_51_interact_spinz_cab|你拉开抽屉，看见像是爬虫的东西，随后发现那只不过是鱼饵。|你拉开抽屉，看见像是爬动的虫子，随后发现那只不过是鱼饵。|EN crawling insects 及全部官方译本均是虫子；中文“爬虫”也指爬行动物，改为爬动的虫子明确主角错看鱼饵的对象。|
|51/game_51_interact_wallet_1|有人忘了钱包。你决定先留在这里，免得失主回来找。|有人忘了钱包。你决定把它留在这里，等失主回来取。|七语均为保留钱包以便失主返回领取；原译“免得失主回来找”令因果方向含混。|

## 保留判断

|位置|保留译文|原因|
|---|---|---|
|12/name_2|翁布拉|既有音译“翁布拉”与七语 UMBRA／ウンブラ 指向同一人物；未发现需要更换名称的语境依据，沿用现译。|
|12/enemy_LECHUZA|莱丘萨|既有音译“莱丘萨”与七语 LECHUZA／レチューザ 指向同一敌人；未发现需要更换名称的语境依据，沿用现译。|
|39/look_clearing_statue|空地中央，一尊头戴兜帽的人形石像庄严而孤独地伫立着。|保留现行“伫立”。“伫”6画，改为“站”未改善像素密度；原译保留石像庄严而孤独地立在空地中央的语气。|
|39/look_dining_table|桌上铺着精美布餐垫。你想象这里曾经享用过许多顿佳肴。|保留现行“佳肴”。替换为“美食”未降低整体笔画，继续保留原文回想昔日丰盛餐食的语气。|
