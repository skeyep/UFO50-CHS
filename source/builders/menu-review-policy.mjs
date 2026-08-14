import reviewedKeys from "./translations/menu-reviewed-keys.json" with { type: "json" };

const explicitReviewed = new Set(reviewedKeys);

// 已逐条对照英日原文复审的稳定功能组。后续每完成一组再扩展此规则。
const reviewedPattern = /^(?:copyright_|laser_|misc_|bg_|month_|crack_|term_|prof_|menu_|option_|joy_|ach_|item_|title_|hi_|fave_|filter_|genre_|info_|bar_)/;

export function menuStatus(key) {
  return explicitReviewed.has(key) || reviewedPattern.test(key) ? "人工复审" : "待人工复审";
}
