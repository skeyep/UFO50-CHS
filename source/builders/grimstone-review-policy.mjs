const initialManual = new Set([
  "gold_goal", "cherry_goal", "garden_goal", "pre_gold_record", "post_gold_record",
  "detail_1", "detail_2", "status_normal", "menu_buy", "menu_sell", "menu_exit",
  "menu_deposit", "menu_withdraw", "menu_mount", "menu_item", "menu_skill", "menu_equip",
  "menu_stats", "menu_order", "menu_nextpage", "menu_prevpage", "shop_owned",
  "shop_dialogue_01", "shop_dialogue_02", "shop_inv_full", "shop_buy_item",
  "shop_no_teeth", "shop_plot_item", "shop_sell_check", "no_teeth", "battle_menu_01",
  "battle_menu_02", "battle_menu_03", "battle_attack_who", "battle_heal_who",
  "battle_victory", "battle_won_teeth", "battle_won_xp", "battle_levelup_01",
  "battle_levelup_02", "battle_died", "intro_01", "intro_02", "intro_03", "intro_04",
  "intro_05", "intro_06", "intro_07", "battle_uses_on"
]);

const explicitManual = new Set(["pdesc_05", "bar_01"]);
const manualPattern = /^(?:fire_|malus_|biggan_|ending_|boss_|battle_|enemy_|animal_|gained_|use_|perk_|equip_|hotel_|bank_|mount_|dance_|gunsmith_|chest_|stable_|store_|bar_|menu_|interact_|pdesc_|whats_|pasaje_|bedrolls_|camo_|cactus_|trap_|treats_|sell_|train_|church_|map_|priest_|grave_|door_|dynamite_|diary_|island_|orb_|horseshoe_|obelisk_|stash_|npc_(?:pleasant|santonio|heston|auster|fortjason|riovalle|elpasaje|lawbuck|agartha|francesco|badbetty|leo|zad|dungeon|mirror|hermit|elder|bridge|cactus|shaman|southmonk|sam|wounded|medkit|angelina|leo2|xafan|zad2|leo3|special|shop|conductor|revive|gunsmith|banker|dancer|dealer|hotel|priest|nun|worker|miner|sant|aust|lawb|well|riov|elpa|cloud9|pete|possum|wife|bull|sheriff|prisoner|guard|guard2|shapeshift|scammer|demon|robber)_|npc_horse$|(?:lily|piano|well)$)/;
const termPattern = /^(?:name_|name_wolf_|town_|status_|stat_|item_|skill_|learned_)/;

export function grimstoneStatus(key) {
  if (initialManual.has(key) || explicitManual.has(key) || manualPattern.test(key)) return "人工初校";
  if (termPattern.test(key)) return "术语初校";
  return "待人工重译";
}

export function grimstoneIsApproved(key) {
  return grimstoneStatus(key) !== "待人工重译";
}
