import fs from "node:fs";
import path from "node:path";

const root = path.resolve(import.meta.dirname, "..");
const source = path.join(root, "ext", "ENGLISH", "0_Text.json");
const outputIndex = process.argv.indexOf("--output");
const target = outputIndex >= 0
  ? path.resolve(process.argv[outputIndex + 1])
  : path.join(root, "chs-tools", "staging", "JAPANESE", "0_Text.json");

function decode(file) {
  const raw = Buffer.from(fs.readFileSync(file, "ascii").trim(), "base64").toString("utf8");
  return JSON.parse(raw.replace(/,\s*}\s*$/, "\n}"));
}

function encodeOfficialStyle(file, object) {
  const lines = Object.entries(object).map(([key, value]) => `${JSON.stringify(key)}:\t${JSON.stringify(value)},`);
  const raw = `{\r\n${lines.join("\r\n")}\r\n}\r\n`;
  fs.mkdirSync(path.dirname(file), { recursive: true });
  fs.writeFileSync(file, Buffer.from(raw, "utf8").toString("base64"), "ascii");
}

const text = decode(source);
const zh = {
  crack_launch_text: "按 ** 启动 UFO 50",
  crack_disclaimer: "我们尽可能保留了原始软件的完整面貌。",
  laser_x_slogan: "永远玩下去",
  title_button_start: "按 ** 开始",
  title_button_menu: "按 ** 打开菜单",
  prof_head_select_profile: "选择档案",
  prof_head_copy_from: "复制档案",
  prof_head_copy_to: "复制到哪个档案？",
  prof_head_delete: "删除档案",
  prof_head_confirm_delete: "删除档案？",
  prof_head_confirm_overwrite: "覆盖档案？",
  prof_text_confirm_delete: "确定要删除这个档案吗？此操作无法撤销。",
  prof_text_confirm_overwrite: "确定要覆盖这个档案吗？此操作无法撤销。",
  prof_menu_delete: "删除档案", prof_menu_copy: "复制档案", prof_menu_back: "取消",
  prof_op_back: "取消", prof_op_delete: "删除", prof_op_copy: "复制",
  prof_name_default: "档案 *", prof_stat_empty: "空",
  menu_head_root: "主菜单", menu_head_options: "设置",
  menu_head_keyboard_config: "键盘设置", menu_head_joypad_config: "手柄设置",
  menu_head_video_settings: "画面设置", menu_head_audio_settings: "声音设置",
  menu_head_profile_name: "档案名称", menu_head_profile_settings: "档案",
  menu_head_milestones: "里程碑", menu_head_language: "切换语言",
  menu_head_game_info: "游戏操作",
  menu_item_options: "设置", menu_item_profile_settings: "档案",
  menu_item_keyboard_config: "键盘设置", menu_item_joypad_config: "手柄设置",
  menu_item_video_settings: "画面设置", menu_item_audio_settings: "声音设置",
  menu_item_ufo_terminal: "终端", menu_item_back_to_library: "返回游戏库",
  menu_item_back_to_library_confirm: "确定关闭游戏？", menu_item_switch_profile: "切换档案",
  menu_item_resume: "继续", menu_item_resume_game: "继续游戏",
  menu_item_reset: "重新开始", menu_item_reset_confirm: "确定重新开始？",
  menu_item_quit: "退出", menu_item_quit_confirm: "确定退出？",
  menu_item_change_language: "切换语言", menu_item_view_milestones: "查看里程碑",
  menu_item_back_to_root: "返回主菜单", menu_item_back_to_settings: "返回设置",
  menu_item_controller_type: "样式", menu_item_player: "玩家", menu_item_joypad_pairing: "分配手柄",
  menu_item_up: "上", menu_item_down: "下", menu_item_left: "左", menu_item_right: "右",
  menu_item_fire1: "按键 *", menu_item_fire2: "按键 *", menu_item_start: "菜单",
  menu_item_display: "显示模式", menu_item_scale: "缩放", menu_item_crt: "CRT 效果",
  menu_item_sound: "音效", menu_item_music: "音乐", menu_item_volume: "音量",
  menu_item_profile_name: "修改名称", menu_item_language: "语言",
  menu_item_mood: "背景", menu_item_filter: "默认筛选",
  menu_op_fullscreen: "全屏", menu_op_windowed: "窗口", menu_op_scale_fill: "铺满",
  menu_op_borderless: "无边框", menu_op_on: "开", menu_op_off: "关",
  menu_op_crt_half: "柔和", menu_op_crt_full: "强烈", menu_op_pad_num: "手柄-*",
  menu_item_page: "页面", menu_misc_defaults_restored: "将恢复默认设置。",
  menu_misc_no_joypad: "未连接手柄", menu_misc_attained: "已达成：**/**",
  menu_misc_key_waiting: "请为 * 按下新按键", menu_misc_joy_waiting: "请为 * 按下新按钮",
  menu_misc_joy_assign: "请在要分配给玩家 * 的手柄上按任意按钮。",
  menu_tip_restore_defaults: "** 恢复默认", menu_tip_restore_joy_defaults: "按住 * 和 @ 恢复默认",
  menu_tip_confirm: "** 确认", menu_tip_back: "** 返回", menu_tip_cancel: "** 取消",
  menu_tip_change_key: "** 修改按键", menu_tip_change_button: "** 修改按钮",
  term_title: "终端", term_good_code: "代码已接受！", term_bad_code: "代码无效！",
  term_bad_param: "参数无效！", term_bad_page: "页面 ID 无效！(PGXX)",
  term_access_denied: "拒绝访问！", term_location_inaccessible: "无法访问！",
  term_already_running: "目标已在运行！", term_operation_disabled: "此操作已被禁用！",
  term_no_save: "不可保存", term_no_load: "不可读取",
  // 终端命令必须保留实际可输入的英文关键字，否则中文提示会误导玩家。
  term_cmd_exec: "EXEC", term_cmd_list: "LIST", term_cmd_desc: "DESC", term_cmd_hist: "HIST",
  term_cmd_info: "INFO", term_cmd_help: "HELP", term_cmd_main: "MAIN", term_cmd_read: "READ",
  term_cmd_page: "PG", term_cmd_copy: "COPY", term_cmd_wipe: "WIPE", term_cmd_name: "NAME",
  term_cmd_move: "MOVE", term_cmd_edit: "EDIT", term_cmd_rand: "TEST",
  term_info_file: "文件", term_info_title: "标题", term_info_year: "年份", term_info_multi: "多人",
  term_arcade_mode_config_on: "设置：开", term_arcade_mode_config_off: "设置：关",
  misc_demo: "试玩版",
  bg_name_tangerine: "橘色", bg_name_golf: "高尔夫", bg_name_ceramic: "陶瓷",
  bg_name_indigo: "靛蓝", bg_name_blood: "血红", bg_name_blue_sky: "蓝天",
  bg_name_lemon: "柠檬", bg_name_infinity: "无限", bg_name_celebration: "庆典", bg_name_cola: "可乐",
  filter_name_year: "按年代", filter_name_name: "按名称", filter_name_player: "多人游戏",
  filter_name_fav: "我的收藏", filter_name_random: "随机", filter_name_garden: "花园",
  filter_name_progress: "我的进度", filter_name_playtime: "最常游玩",
  filter_name_coop: "…合作", filter_name_versus: "…对战", filter_name_epic: "史诗体验",
  filter_name_thinky: "动脑体验", filter_name_quick: "快速体验", filter_name_reflex: "反应挑战",
  filter_button_type: "**：类型", filter_button_remix: "**：重排", filter_button_times: "**：通关时间",
  filter_button_list: "**：礼物", filter_button_close: "**：关闭", filter_garden_no_gift: "-无礼物-",
  genre_adventure: "冒险", genre_strategy: "策略", genre_shooter: "射击", genre_arcade: "街机",
  genre_platformer: "平台跳跃", genre_puzzle: "解谜", genre_rpg: "角色扮演",
  genre_sport: "体育", genre_simulation: "模拟",
  info_head_description: "简介", info_head_statistics: "统计", info_head_awards: "通关",
  info_head_time_spent: "游玩时间", info_head_history: "历史", info_head_controls: "操作",
  info_head_garden: "花园", info_stat_empty: "-无记录-", info_time_empty: "-无记录-",
  info_time_plays: "游玩次数：", info_time_total_playtime: "总游玩时间：", info_time_ranking: "时长排名：",
  info_time_rank_1st: "游玩时间最多！", info_time_rank_st: "游玩时长第 ** 名",
  info_time_rank_nd: "游玩时长第 ** 名", info_time_rank_rd: "游玩时长第 ** 名", info_time_rank_th: "游玩时长第 ** 名",
  info_awd_gold_goal: "要获得 *……", info_awd_gold_time: "* 最佳时间……",
  info_awd_cherry_goal: "要获得 *……", info_awd_cherry_time: "* 最佳时间……", info_awd_empty: "-无记录-",
  info_garden_goal: "要赠送 @ 礼物……", info_garden_gift: "@ 赠送了 *************",
  info_garden_time: "于 *************", info_garden_empty: "-无记录-",
  option_yes: "是", option_no: "否", option_ok: "确定", bar_no_record: "-无记录-",
  bar_filter: "筛选", bar_time: "时间", bar_play_info: "**：游玩  **：信息",
  bar_fave_exit: "**：收藏  **：关闭", bar_exit: "**：关闭",
  title_start_game: "按 [2]", title_continue: "继续", title_new_game: "新游戏",
  title_1player: "单人游戏", title_2players: "双人游戏", title_1p_arena: "单人竞技场",
  title_2p_chicken: "双人胆量赛", title_1p_journey: "单人旅程", title_2p_battle: "双人对战",
  title_game_start: "开始游戏", title_high_scores: "高分榜", "title_1-3p_hotseat": "1-3人轮流游戏",
  title_custom: "自定义", title_prologue: "序章", title_vs: "双人对战", title_coop: "双人合作",
  title_options: "选项", title_1p_continue: "单人继续", title_2p_continue: "双人继续",
  title_1p_new_game: "单人新游戏", title_2p_new_game: "双人新游戏", title_2p_versus: "双人对战",
  title_p1_input: "玩家1操作", title_p2_input: "玩家2操作",
  title_overwrite_warning: "这将覆盖当前存档。", title_credits_1: "制作",
  hi_title: "*高分榜*", hi_rank: "排名", hi_name: "名称", hi_score: "分数",
  fave_no_favorites: "你还没有收藏任何游戏",
  fave_instructions: "要收藏游戏，请打开它的信息窗口并按 **。",
  ach_attained: "达成里程碑",
  joy_type_xbox: "X 布局", joy_type_playstation: "P 布局", joy_type_generic: "通用",
  joy_type_lx: "原版 LX", menu_head_button_layout: "按键布局", menu_item_layout: "布局",
  menu_op_standard: "标准", menu_op_inverted: "反转", menu_op_pixel_perfect: "整数缩放"
};

for (let i = 1; i <= 12; i++) {
  zh[`month_${i}`] = `${i}月`;
}

Object.assign(zh, {
  crack_credit_publisher: "发行", crack_credit_presenters: "出品",
  crack_credit_loc1: "本地化", crack_credit_loc2: "WARLOCS 制作",
  crack_credit_testers: "测试人员", crack_credit_qa1: "质量保证",
  crack_credit_thanks: "特别感谢", crack_credit_jp1: "日语",
  crack_credit_jp2: "本地化", crack_credit_port1: "移植",
  crack_credit_port2: "HIDDEN TRAP 制作",
  ach_name_open_1: "初次品尝", ach_name_open_25: "精选拼盘", ach_name_open_50: "全都要",
  ach_name_score_any: "高分领主", ach_name_score_top_3: "街机王牌", ach_name_score_first: "分数霸主",
  ach_name_gold_1: "黄金之路", ach_name_gold_50: "纯金",
  ach_name_cherry_1: "樱桃之路", ach_name_cherry_50: "樱桃派",
  ach_name_terminal: "超级用户", ach_name_all: "游戏大师",
  ach_desc_open_1: "擦亮你的第一张游戏盘。",
  ach_desc_open_25: "在同一档案中擦亮 25 张游戏盘。",
  ach_desc_open_50: "在同一档案中擦亮全部 50 张游戏盘。",
  ach_desc_score_any: "在任意高分榜上留下姓名缩写。",
  ach_desc_score_top_3: "在任意高分榜上进入前三名。",
  ach_desc_score_first: "在任意高分榜上取得第一名。",
  ach_desc_gold_1: "首次通关一款游戏。",
  ach_desc_gold_50: "在同一档案中通关全部 50 款游戏。",
  ach_desc_cherry_1: "获得第一张樱桃盘。",
  ach_desc_cherry_50: "在同一档案中获得 50 张樱桃盘。",
  ach_desc_terminal: "在终端中输入一条有效代码。",
  ach_desc_all: "达成其他所有里程碑。",
  item_hot_tub: "热水浴池", item_painting: "画作", item_telescope: "望远镜", item_lily: "青蛙",
  item_stove: "炉灶", item_gopher: "地鼠", item_tv: "电视", item_lx: "LX-III",
  item_trunk: "旧箱子", item_toilet: "马桶", item_couch: "沙发", item_cow: "奶牛",
  item_guitar: "吉他", item_desk: "电脑", item_fruit: "水果", item_lamp: "台灯",
  item_flower: "向日葵", item_boxes: "纸箱", item_tub: "浴缸", item_spider: "蜘蛛",
  item_bed: "床", item_monkey: "红毛猩猩", item_golf_ball: "高尔夫球", item_mirror: "镜子",
  item_ants: "蚁丘", item_jump_rope: "跳绳", item_water_can: "洒水壶", item_butterfly: "蝴蝶",
  item_plant: "盆栽", item_blender: "搅拌机", item_sun: "太阳", item_phone: "电话",
  item_picnic_blanket: "野餐垫", item_possums: "负鼠", item_books: "书架", item_calendar: "日历",
  item_dirt: "菜地", item_satellite: "卫星天线", item_ghost: "幽灵？", item_trapdoor: "活板门",
  item_rug: "地毯", item_trophy: "奖杯", item_bike: "健身车", item_dino: "雷龙",
  item_owl: "猫头鹰", item_fridge: "冰箱", item_hat_rack: "衣帽架", item_cattails: "香蒲",
  item_canvas: "画架", item_bird: "小鸟"
});

for (const n of [2, 3, 4, 5, 10, 15, 20, 30, 40]) {
  zh[`ach_name_gold_${n}`] = `${n} 张金盘`;
  zh[`ach_desc_gold_${n}`] = `在同一档案中通关 ${n} 款游戏。`;
  zh[`ach_name_cherry_${n}`] = `${n} 张樱桃盘`;
  zh[`ach_desc_cherry_${n}`] = `在同一档案中获得 ${n} 张樱桃盘。`;
}

const controlTranslations = {
  "": "", "???": "???", "-ALWAYS-": "-始终-", "-BATTLE SCREEN-": "-战斗界面-",
  "-FLYING-": "-飞行时-", "-MAP SCREEN-": "-地图界面-", "-ON FOOT-": "-步行时-",
  "-UNAVAILABLE-": "-不可用-", "-WITH BEANBAG-": "-持沙包时-", "-WITHOUT BEANBAG-": "-未持沙包时-",
  "[1 W/ OBJECT: THROW": "持物时按 [1：投掷", "[1: ARROW RITUAL": "[1：箭矢仪式",
  "[1: ATTACK": "[1：攻击", "[1: BACK": "[1：返回", "[1: BACK / CANCEL": "[1：返回／取消",
  "[1: BACK / SKIP": "[1：返回／跳过", "[1: BRAKE / REVERSE": "[1：刹车／倒车",
  "[1: CAMERA MODE": "[1：相机模式", "[1: CANCEL": "[1：取消", "[1: DROP OFF": "[1：放下",
  "[1: FAST FORWARD": "[1：快进", "[1: FREE LOOK": "[1：自由观察",
  "[1: KICK / HEAD / SLIDE": "[1：踢／头顶／滑铲", "[1: NINJA STARS": "[1：手里剑",
  "[1: OPEN MENU / CANCEL": "[1：打开菜单／取消", "[1: PICK UP / SWITCH TEAMMATES": "[1：拾取／切换队友",
  "[1: PICK UP / THROW": "[1：拾取／投掷", "[1: PUNCH": "[1：拳击", "[1: PUNCH / SHOOT": "[1：拳击／射击",
  "[1: RETURN HOME": "[1：返回基地", "[1: ROTATE PIECE": "[1：旋转方块", "[1: SENSE DANGER": "[1：感知危险",
  "[1: SHOOT": "[1：射击", "[1: SHOOT FORWARD": "[1：向前射击", "[1: SHOOT UPWARD": "[1：向上射击",
  "[1: SLASH": "[1：斩击", "[1: SPIT": "[1：吐击", "[1: STRIKE": "[1：击打",
  "[1: TOGGLE BETWEEN LOOK AND USE": "[1：切换观察／使用", "[1: UNDO": "[1：撤销",
  "[1: USE EQUIPMENT": "[1：使用装备", "[1: USE ITEM": "[1：使用物品", "[1: WEAPON": "[1：武器",
  "[1: YOYO": "[1：悠悠球", "[1+[2: SPIN ATTACK": "[1+[2：旋转攻击",
  "[2 AT TOP OF JUMP: SPECIAL ACTION": "跳到最高点按 [2：特殊动作", "[2 IN AIR: DUNK": "空中按 [2：扣篮",
  "[2 IN AIR: FLY": "空中按 [2：飞行", "[2 IN AIR: GRAVITY FLIP": "空中按 [2：反转重力",
  "[2: ACCELERATE": "[2：加速", "[2: BOMB (RED SHIP ONLY)": "[2：炸弹（仅红色飞船）",
  "[2: CHANGE COLOR": "[2：改变颜色", "[2: CONFIRM": "[2：确认", "[2: CONFIRM / USE": "[2：确认／使用",
  "[2: DASH": "[2：冲刺", "[2: DROP BOMB": "[2：投放炸弹", "[2: GRAB UNIT": "[2：抓取单位",
  "[2: INTERACT": "[2：互动", "[2: INTERACT / CONFIRM": "[2：互动／确认",
  "[2: INTERACT / SWITCH ITEMS": "[2：互动／切换物品", "[2: JUMP": "[2：跳跃",
  "[2: JUMP / DBL JUMP": "[2：跳跃／二段跳", "[2: LAUNCH": "[2：发射", "[2: LAUNCH DISK": "[2：发射圆盘",
  "[2: MANEUVER": "[2：机动", "[2: OPEN MENU": "[2：打开菜单", "[2: OPEN MENU / ATTACK": "[2：打开菜单／攻击",
  "[2: ROLL": "[2：翻滚", "[2: SELECT": "[2：选择", "[2: SELECT / PLACE": "[2：选择／放置",
  "[2: SHOOT SIDEWAYS": "[2：横向射击", "[2: SWING": "[2：挥杆", "[2: TAKE PHOTO": "[2：拍照",
  "[2: THRUST": "[2：推进", "[2: WARP": "[2：传送", "[D: BLOCK": "[D：格挡",
  "[D: BUY MODULES": "[D：购买模块", "[D[1 ON OBJECT: GRAB": "站在物体上按 [D[1：抓取",
  "[D[1 W/ OBJECT: DROP": "持物时按 [D[1：放下", "[D[1: MELEE ATTACK": "[D[1：近战攻击",
  "[D[1: STONE RITUAL": "[D[1：石化仪式", "[D[2: ROLL": "[D[2：翻滚", "[D[R/[D[L: DODGE": "[D[R／[D[L：闪避",
  "[L: DRIFT / CHARGE": "[L：漂移／蓄力", "[U: ENTER DOOR / SHIP": "[U：进门／登上飞船",
  "[U: VIEW OPPONENT'S STATUS (2P ONLY)": "[U：查看对手状态（仅双人）", "[U[1: BOMB RITUAL": "[U[1：炸弹仪式",
  "[U[1: WEAPON ALT": "[U[1：副武器", "[U/[D: AIM GUN": "[U／[D：瞄准",
  "[U/[D: AIM JUMP": "[U／[D：调整跳跃方向", "[U/[D/[L/[R: TURN / BRAKE": "[U／[D／[L／[R：转向／刹车",
  "[U/[D/[L/[R/[1/[2: MOVE": "[U／[D／[L／[R／[1／[2：移动",
  "DBL TAP [L/[R: RUN": "双击 [L／[R：奔跑", "DBL TAP [U/[D: DODGE": "双击 [U／[D：闪避",
  "DOUBLE TAP [1: SECONDARY WEAPON": "双击 [1：副武器", "HOLD [1: CANCEL PIECE PLACEMENT": "按住 [1：取消放置方块",
  "HOLD [1: CAST SPELL": "按住 [1：施法", "HOLD [1: CHARGE ATTACK": "按住 [1：蓄力攻击",
  "HOLD [1: CHARGE PUNCH": "按住 [1：蓄力拳", "HOLD [1: GIVE UP": "按住 [1：放弃",
  "HOLD [1: SACRIFICE": "按住 [1：献祭", "HOLD [1: SUPER SHOT": "按住 [1：超级射击",
  "HOLD [1: THROW": "按住 [1：投掷", "HOLD [2: COMMAND MENU": "按住 [2：指令菜单",
  "HOLD [2: DROP ITEM": "按住 [2：丢弃物品", "HOLD [D: SHRINK": "按住 [D：缩小",
  "HOLD [U: GROW": "按住 [U：长大", "HOLD [U: LOAD GUN": "按住 [U：装填",
  "HOLD [U/[D: MOVE CAMERA": "按住 [U／[D：移动镜头", "RITUALS CAN BE CHAINED TOGETHER": "仪式可以连续施放",
  "TAP [1: PASS": "轻按 [1：传球", "UNITS CAN MOVE UP, DOWN, AND BACK.": "单位可以向上、向下及向后移动。",
  "UNITS CAN SWAP WITH FRIENDLY UNITS.": "单位可以与友方单位交换位置。", "VARIES BASED ON MISSION TYPE": "操作会随任务类型变化",
  "WALK INTO STUNNED ENEMY: GRAB": "撞向眩晕的敌人：抓取", "YOUR TEAMMATE JUMPS WHEN YOU JUMP.": "你跳跃时，队友也会跳跃。"
};

for (const [key, value] of Object.entries(text)) {
  if (key.startsWith("info_controls_") && !key.endsWith("_lim") && value in controlTranslations) {
    zh[key] = controlTranslations[value];
  }
}

for (const [key, value] of Object.entries(zh)) {
  if (!(key in text)) throw new Error(`缺少本地化键：${key}`);
  text[key] = value;
}

encodeOfficialStyle(target, text);
console.log(`已写入 ${Object.keys(zh).length} 条 UFO 50 通用菜单中文：${target}`);
