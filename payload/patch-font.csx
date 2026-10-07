using System.Linq;
using System.IO;
using System.Collections.Generic;

EnsureDataLoaded();

var initFonts = """
function scrInitFonts()
{
    var fontMapNoJP = " !\"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_'abcdefghijklmnopqrstuvwxyz";
    fontMapNoJP += "{|}~¡°¿ÀÁÂÃÄÅÆÇÈÉÊËÌÍÎÏÑÒÓÔÕÖ×ØÙÚÛÜÝẞßàáâãäåæçèéêëìíîïñòóôõöøùúûüýÿ«»±œŒ";
    var fontMapJP = "…♪、。「」『』【】〜〽ぁあぃいぅうぇえぉおかがきぎくぐけげこごさざしじすずせぜそぞただちぢっつづてでとどなにぬねのはばぱひびぴふぶぷへべぺほぼぽまみむめもゃやゅゆょよらりるれろゎわゐをんゔゕゖ゛゜゠ァアィイゥウェエォオカガキギクグケゲコゴサザシジスズセゼソゾタダチヂッツヅテデトドナニヌネノハバパヒビピフブプヘベペホボポマミムメモャヤュユョヨラリルレロヮワヲンヴヵヶヷヺ・ー！：×△〇◎？☐～（）０１２３４５６７８９";
    var fontMapFull = fontMapNoJP + fontMapJP;
    var fontMapBasic = " !'()-./0123456789?@ABCDEFGHIJKLMNOPQRSTUVWXYZ¡¿ÀÁÂÃÄÅÆÇÈÉÊËÌÍÎÏÑÒÓÔÕÖØÙÚÛÜÝẞß«»";
    var fontMapBasicJP = fontMapBasic + fontMapJP;
    var fontMapDigital = "-0123456789[";
    var fontMapDigitalAlphabet = "-0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    font_add_enable_aa(false);
    global.fontThinOutline = font_add_sprite_ext(sFontThinOutline, fontMapNoJP, false, 0);
    global.fontDefault = font_add_sprite_ext(sFontDefault, fontMapNoJP, false, 0);
    global.fontDefault_JP = font_add_sprite_ext(sFontDefault_JP, fontMapFull, false, 0);
    global.fontDefaultNoShadow = font_add_sprite_ext(sFontDefaultNoShadow, fontMapNoJP, false, 0);
    global.fontDefaultNoShadow_JP = font_add_sprite_ext(sFontDefaultNoShadow_JP, fontMapFull, false, 0);
    global.fontNoShadow = font_add_sprite_ext(sFontNoShadow, fontMapFull, false, 0);
    global.fontTerminal = font_add_sprite_ext(sFontNoShadow, fontMapFull, false, 0);
    global.fontGrimstone = font_add_sprite_ext(sFontGrimstone, fontMapFull, false, 0);
    global.fontDefault_CHS = font_add(working_directory + "fonts\\UFO50-CHS.ttf", 7, false, false, 32, 65535);
    global.fontFancyShort = font_add_sprite_ext(sFontGrimstoneNoBG, fontMapFull, false, 0);
    global.fontHandwriting = font_add_sprite_ext(sFontHandwriting, fontMapFull, false, 0);
    global.fontBlocky = font_add_sprite_ext(sFontBlocky, fontMapFull, false, 0);
    global.fontAlien = font_add_sprite_ext(sFontAlien, fontMapFull, false, 0);
    global.fontAvianos = font_add_sprite_ext(sFontAvianos, fontMapFull, false, 0);
    global.fontUFO = font_add_sprite_ext(sFontUFO, fontMapFull, false, 0);
    global.fontTall = font_add_sprite_ext(sFontTall, fontMapFull, false, 0);
    global.fontTallBG = font_add_sprite_ext(sFontTallBG, fontMapFull, false, 0);
    global.fontGradient = font_add_sprite_ext(sFontGradient, fontMapFull, false, 0);
    global.fontKanji12 = font_add(working_directory + "fonts\\JF-Dot-ShinonomeMin12-Alt.ttf", 9, false, false, 32, 127);
    global.fontBlockyTall = font_add_sprite_ext(sFontBlockyTall, fontMapFull, false, 0);
    global.fontBigWestern = font_add_sprite_ext(sFontBigWestern, fontMapFull, false, 0);
    global.fontDigital = font_add_sprite_ext(sFontDigital, fontMapDigital, false, 0);
    global.fontDigital2 = font_add_sprite_ext(sFontDigital2, fontMapDigitalAlphabet, false, 0);
    global.fontDigitalBig = font_add_sprite_ext(sFontDigitalBig, fontMapDigital, false, 0);
    global.fontDigitalMini = font_add_sprite_ext(sFontDigitalMini, fontMapDigital, false, 0);
    global.fontDotMatrix = font_add_sprite_ext(sFontDotMatrix, fontMapBasicJP, true, 2);
    global.fontCrackBig = font_add_sprite_ext(sFontCrackBig, fontMapBasic, false, 0);
    global.fontCrackHeader = font_add_sprite_ext(sFontCrackHeader, fontMapBasic, false, 0);
    global.fontCrackSmall = font_add_sprite_ext(sFontCrackSmall, fontMapBasic, false, 0);
    global.currFont = global.fontDefault;
    global.prePauseFont = global.fontDefault;
    global.chsRequestedFont = global.fontDefault;
    global.chsPrePauseRequestedFont = global.fontDefault;
}
""";

var fontSizeText = System.Environment.GetEnvironmentVariable("UFO50_CHS_FONT_SIZE_PX");
var fontSizePx = 8;
if (!string.IsNullOrWhiteSpace(fontSizeText) &&
    (!int.TryParse(fontSizeText, out fontSizePx) || fontSizePx < 7 || fontSizePx > 9))
{
    throw new System.Exception("UFO50_CHS_FONT_SIZE_PX must be 7, 8, or 9.");
}
initFonts = System.Text.RegularExpressions.Regex.Replace(
    initFonts,
    @"(global\.fontDefault_CHS\s*=\s*font_add\([^\r\n]*?,\s*)7(\s*,)",
    match => match.Groups[1].Value + fontSizePx + match.Groups[2].Value
);

var setFont = """
function scrSetFont(arg0)
{
    var _font = arg0;
    if (font_exists(_font))
    {
        if (_font != global.fontDefault_CHS) global.chsRequestedFont = _font;
        var _spriteDigits = (_font == global.fontDigital || _font == global.fontDigitalMini || _font == global.fontDigitalBig || _font == global.fontDigital2);
        if (global.language == global.LANG_JAPANESE && font_exists(global.fontDefault_CHS) && !_spriteDigits)
        {
            // 墙钟和比分牌沿用原版数字精灵的尺寸，其余文字使用中文字体。
            _font = global.fontDefault_CHS;
        }
        draw_set_font(_font);
        global.currFont = _font;
        return true;
    }
    else
    {
        trace("BAD FONT!");
        return false;
    }
}
""";

var loadLibraryText = """
function scrLoadLibraryText()
{
    global.TEXT_LIBRARY = undefined;
    global.TEXT_LIBRARY = {};
    global.TEXT_META = undefined;
    global.TEXT_META = {};
    var _langHeader = global.LANG_HEADERS[global.language];
    var libFile = string_replace(global.EXTERNAL_TEXT_FILE, "*", "0");
    libFile = string_replace(libFile, "#", _langHeader);
    if (!file_exists(libFile))
    {
        return false;
    }
    var libBuffer = buffer_load(libFile);
    var libContent = buffer_read(libBuffer, buffer_string);
    if (global.decoding[0] == 1)
    {
        libContent = base64_decode(libContent);
    }
    buffer_delete(libBuffer);
    global.TEXT_LIBRARY = json_parse(libContent);
    if (is_undefined(global.TEXT_LIBRARY))
    {
        return false;
    }
    if (global.language == global.LANG_JAPANESE)
    {
        var metaFile = string_replace(global.EXTERNAL_TEXT_FILE, "*", "m");
        metaFile = string_replace(metaFile, "#", _langHeader);
        if (!file_exists(metaFile))
        {
            return false;
        }
        var metaBuffer = buffer_load(metaFile);
        var metaContent = buffer_read(metaBuffer, buffer_string);
        buffer_delete(metaBuffer);
        global.TEXT_META = json_parse(metaContent);
        if (is_undefined(global.TEXT_META))
        {
            return false;
        }
    }
    else
    {
        scrLoadInternalText();
    }
    return true;
}
""";

var loadProfile = """
function scrLoadProfile(arg0)
{
    if (arg0 < 1 || arg0 > global.NUM_PROFILES)
    {
        return false;
    }
    var _languageBeforeProfileLoad = global.language;
    global.currFile = arg0;
    global.timeStamp = scrCurrentTime();
    scrOpenCurrFile();
    if (scrReadSaveStatusManual(37))
    {
        global.timeStampIncremental = scrCurrentTime();
        global.timeSumIncremental = scrReadRealManual(0, "timeSumIncremental", 0);
    }
    else
    {
        global.timeStampIncremental = -1;
        global.timeSumIncremental = 0;
    }
    global.currFileName = scrReadString("profileName", scrStringVal("prof_name_default", global.currFile));
    global.sortDefault = scrReadReal("sortDefault", 0);
    global.randSortLocked = scrReadReal("randSortLocked", false);
    for (var i = 0; i < global.NUM_LIBRARY_GAMES; i++)
    {
        global.randSortOrder[i] = scrReadReal("randSortOrder" + string(i), -1);
    }
    global.libraryBG = scrReadReal("libraryBG", 0);
    var _saveID = scrReadReal("profileLanguage", global.LANG_SAVE_ID[global.defaultLanguage]);
    for (var i = 0; i < global.NUM_LANG; i++)
    {
        if (global.LANG_SAVE_ID[i] == _saveID)
        {
            global.profileLanguage = i;
        }
    }
    if (_languageBeforeProfileLoad == global.LANG_JAPANESE)
    {
        global.profileLanguage = global.LANG_JAPANESE;
    }
    global.goldTimeAll = scrReadReal("goldTimeAll", 0);
    global.cherryTimeAll = scrReadReal("cherryTimeAll", 0);
    global.backupSaveNum[arg0] = scrReadReal("backupSaveNum", 0);
    global.backupTimer = global.BACKUP_MINIMUM_TIME;
    scrCloseCurrFile();
    scrUpdateLanguage(global.profileLanguage);
    return true;
}
""";

var drawProfile = """
function scrDrawProfile(arg0, arg1, arg2)
{
    scrDrawMenuBorder(arg0, arg1, 256, 32);
    scrSetFont(global.fontTall);
    var _textTop = arg1 + 8;
    draw_text(arg0 + 16, _textTop, profileName[arg2]);
    if (fileExists[arg2] == 0)
    {
        draw_set_halign(fa_right);
        draw_text(arg0 + 168, _textTop, scrString("prof_stat_empty"));
        draw_set_halign(fa_left);
    }
    else
    {
        draw_set_halign(fa_right);
        if (global.language == global.LANG_JAPANESE)
        {
            scrSetFont(global.fontDefault);
            var _fullTime = scrTimeFormat(timePlayed[arg2], 2) + ":" + scrTimeFormat(timePlayed[arg2], -3);
            draw_text_bg(arg0 + 208, _textTop, _fullTime, 0, 8, 16, false, true);
        }
        else
        {
            draw_text_bg(arg0 + 160, _textTop, " " + scrTimeFormat(timePlayed[arg2], 2), 0, 8, 16, false, true);
            scrSetFont(global.fontDefault);
            draw_text(arg0 + 184, _textTop + 8, ":" + scrTimeFormat(timePlayed[arg2], -3));
        }
        draw_set_halign(fa_left);
        var _winsAddX = 216;
        draw_sprite(sWinIcons, global.GOLD_WIN, arg0 + _winsAddX, _textTop);
        var winString;
        if (goldWins[arg2] < 10)
        {
            winString = "0" + string(goldWins[arg2]);
        }
        else
        {
            winString = string(goldWins[arg2]);
        }
        draw_text(arg0 + _winsAddX + 16, _textTop, winString);
        draw_sprite(sWinIcons, global.CHERRY_WIN, arg0 + _winsAddX, _textTop + 8);
        if (cherryWins[arg2] < 10)
        {
            winString = "0" + string(cherryWins[arg2]);
        }
        else
        {
            winString = string(cherryWins[arg2]);
        }
        draw_text(arg0 + _winsAddX + 16, _textTop + 8, winString);
    }
}
""";

var oldInfoBar = """
            var _comboName = scrStringFormat("{0} {1}", _gameNum, _gameName);
            draw_text(8, _TEXT_Y, _comboName);
            if (favs[global.selGame])
            {
                if (global.language != global.LANG_JAPANESE)
                {
                    draw_sprite(sFav, 0, 176, 206);
                }
                else
                {
                    draw_sprite(sFav, 0, 24, 206);
                }
            }
            var _year = string(floor(global.mGameYear[global.selGame]));
            if (global.language != global.LANG_JAPANESE)
            {
                draw_text(200, _TEXT_Y, _year);
            }
            else
            {
                draw_text(184, _TEXT_Y, _year + "ねん");
            }
""".Replace("\r\n", "\n");
var newInfoBar = """
            var _comboName = scrStringFormat("{0} {1}", _gameNum, _gameName);
            if (global.language == global.LANG_JAPANESE)
            {
                draw_text(8, _TEXT_Y, _gameNum);
                var _nameX = 24;
                if (favs[global.selGame])
                {
                    draw_sprite(sFav, 0, _nameX, 206);
                    _nameX += 8;
                }
                draw_text(_nameX, _TEXT_Y, _gameName);
            }
            else
            {
                draw_text(8, _TEXT_Y, _comboName);
                if (favs[global.selGame])
                {
                    draw_sprite(sFav, 0, 176, 206);
                }
            }
            var _year = string(floor(global.mGameYear[global.selGame]));
            if (global.language != global.LANG_JAPANESE)
            {
                draw_text(200, _TEXT_Y, _year);
            }
            else
            {
                draw_text(192, _TEXT_Y, _year + "年");
            }
""".Replace("\r\n", "\n");
var drawTextInputHeader = """
    var makeEven = false;
""".Replace("\r\n", "\n");
var drawTextInputHeaderNew = """
    var makeEven = false;
    var cjkCellWidth = 8;
    if (global.language == global.LANG_JAPANESE && global.currFont == global.fontDefault_CHS)
    {
        cjkCellWidth = max(8, round(string_width("中")));
    }
""".Replace("\r\n", "\n");
var drawTextInputMeasure = """
            else
            {
                width += 8;
            }
""".Replace("\r\n", "\n");
var drawTextInputMeasureNew = """
            else
            {
                var charWidth = 8;
                if (global.language == global.LANG_JAPANESE && ord(char) >= 12288)
                {
                    charWidth = cjkCellWidth;
                }
                width += charWidth;
            }
""".Replace("\r\n", "\n");
var drawTextInputGlyph = """
        else
        {
            draw_text(xx, yy, char);
            xx += 8;
        }
""".Replace("\r\n", "\n");
var drawTextInputGlyphNew = """
        else
        {
            draw_text(xx, yy, char);
            var charWidth = 8;
            if (global.language == global.LANG_JAPANESE && ord(char) >= 12288)
            {
                charWidth = cjkCellWidth;
            }
            xx += charWidth;
        }
""".Replace("\r\n", "\n");
var textWithSpritesAdvance = """
        xx += 8;
""".Replace("\r\n", "\n");
var textWithSpritesAdvanceNew = """
        var charWidth = 8;
        if (global.language == global.LANG_JAPANESE && global.currFont == global.fontDefault_CHS && ord(cc) >= 12288)
        {
            charWidth = max(8, round(string_width(cc)));
        }
        xx += charWidth;
""".Replace("\r\n", "\n");
var titleCreditAnchor = """
    if (_showCopyright)
    {
        draw_text_ce(_xv + 192, 200 + screenShakeY, "@ 1989 " + scrStringManual("copyright_ufo_soft", 0), 2);
    }
""".Replace("\r\n", "\n");
var titleCreditReplacement = """
    if (_showCopyright)
    {
        if (global.language == global.LANG_JAPANESE)
        {
            scrFontDefault();
            scrDrawTextCentered("@ 1989 " + scrStringManual("copyright_ufo_soft", 0) + " 汉化：Skeyep_目目", _xv, 200 + screenShakeY, 8, 384);
        }
        else
        {
            draw_text_ce(_xv + 192, 200 + screenShakeY, "@ 1989 " + scrStringManual("copyright_ufo_soft", 0), 2);
        }
    }
""".Replace("\r\n", "\n");
var languageNotice = """

if (state == STATE_LANGUAGE && global.language == global.LANG_JAPANESE)
{
    scrFontDefault();
    draw_text_ce(_xview + 192, _yview + 200, "仅供学习交流，禁止商业使用", 2);
}
""".Replace("\r\n", "\n");
var descriptionGenreSpacingAnchor = """
                            draw_text(_textX, _textY - 1, descContent[i]);
                            scrFontDefault();
                            _textY += 8;
""".Replace("\r\n", "\n");
var descriptionGenreSpacingReplacement = """
                            draw_text(_textX, _textY - 1, descContent[i]);
                            scrFontDefault();
                            _textY += (global.language == global.LANG_JAPANESE) ? max(8, ceil(string_height("中"))) : 8;
""".Replace("\r\n", "\n");
var descriptionMainSpacingAnchor = """
                            draw_text(_textX, _textY, descContent[i]);
                            _textY += 8;
""".Replace("\r\n", "\n");
var descriptionMainSpacingReplacement = """
                            draw_text(_textX, _textY, descContent[i]);
                            _textY += (global.language == global.LANG_JAPANESE) ? max(8, ceil(string_height("中"))) : 8;
""".Replace("\r\n", "\n");
var timeSpentAnchor = """
                else if (currPage == PAGE_TIME_SPENT && selPlays > 0)
                {
                    draw_text(_textX, _textY, scrString("info_time_plays"));
                    _textY += 8;
                    draw_set_halign(fa_right);
                    draw_text(_textX + _textWidth, _textY, string(selPlays));
                    draw_set_halign(fa_left);
                    _textY += 8;
                    draw_text(_textX, _textY, scrString("info_time_total_playtime"));
                    _textY += 8;
                    draw_set_halign(fa_right);
                    draw_text(_textX + _textWidth, _textY, selPlaytime);
                    draw_set_halign(fa_left);
                    _textY += 8;
                    draw_text(_textX, _textY, scrString("info_time_ranking"));
                    _textY += 8;
                    draw_set_halign(fa_right);
                    if (selPlayRanking == 1)
                    {
                        draw_text(_textX + _textWidth, _textY, scrString("info_time_rank_1st"));
                    }
                    else if (selPlayRanking == 21 || selPlayRanking == 31 || selPlayRanking == 41)
                    {
                        draw_text(_textX + _textWidth, _textY, scrStringVal("info_time_rank_st", selPlayRanking));
                    }
                    else if (selPlayRanking == 2 || selPlayRanking == 22 || selPlayRanking == 32 || selPlayRanking == 42)
                    {
                        draw_text(_textX + _textWidth, _textY, scrStringVal("info_time_rank_nd", selPlayRanking));
                    }
                    else if (selPlayRanking == 3 || selPlayRanking == 23 || selPlayRanking == 33 || selPlayRanking == 43)
                    {
                        draw_text(_textX + _textWidth, _textY, scrStringVal("info_time_rank_rd", selPlayRanking));
                    }
                    else
                    {
                        draw_text(_textX + _textWidth, _textY, scrStringVal("info_time_rank_th", selPlayRanking));
                    }
                    draw_set_halign(fa_left);
                    _textY += 8;
                }
""".Replace("\r\n", "\n");
var timeSpentReplacement = """
                else if (currPage == PAGE_TIME_SPENT && selPlays > 0)
                {
                    if (global.language == global.LANG_JAPANESE)
                    {
                        var _timeRankText;
                        if (selPlayRanking == 1)
                        {
                            _timeRankText = scrString("info_time_rank_1st");
                        }
                        else if (selPlayRanking == 21 || selPlayRanking == 31 || selPlayRanking == 41)
                        {
                            _timeRankText = scrStringVal("info_time_rank_st", selPlayRanking);
                        }
                        else if (selPlayRanking == 2 || selPlayRanking == 22 || selPlayRanking == 32 || selPlayRanking == 42)
                        {
                            _timeRankText = scrStringVal("info_time_rank_nd", selPlayRanking);
                        }
                        else if (selPlayRanking == 3 || selPlayRanking == 23 || selPlayRanking == 33 || selPlayRanking == 43)
                        {
                            _timeRankText = scrStringVal("info_time_rank_rd", selPlayRanking);
                        }
                        else
                        {
                            _timeRankText = scrStringVal("info_time_rank_th", selPlayRanking);
                        }
                        _textY += UFO50_CHS_draw_key_value_row(_textX, _textY, _textWidth, scrString("info_time_plays"), string(selPlays));
                        _textY += UFO50_CHS_draw_key_value_row(_textX, _textY, _textWidth, scrString("info_time_total_playtime"), selPlaytime);
                        _textY += UFO50_CHS_draw_key_value_row(_textX, _textY, _textWidth, scrString("info_time_ranking"), _timeRankText);
                    }
                    else
                    {
                        draw_text(_textX, _textY, scrString("info_time_plays"));
                        _textY += 8;
                        draw_set_halign(fa_right);
                        draw_text(_textX + _textWidth, _textY, string(selPlays));
                        draw_set_halign(fa_left);
                        _textY += 8;
                        draw_text(_textX, _textY, scrString("info_time_total_playtime"));
                        _textY += 8;
                        draw_set_halign(fa_right);
                        draw_text(_textX + _textWidth, _textY, selPlaytime);
                        draw_set_halign(fa_left);
                        _textY += 8;
                        draw_text(_textX, _textY, scrString("info_time_ranking"));
                        _textY += 8;
                        draw_set_halign(fa_right);
                        if (selPlayRanking == 1)
                        {
                            draw_text(_textX + _textWidth, _textY, scrString("info_time_rank_1st"));
                        }
                        else if (selPlayRanking == 21 || selPlayRanking == 31 || selPlayRanking == 41)
                        {
                            draw_text(_textX + _textWidth, _textY, scrStringVal("info_time_rank_st", selPlayRanking));
                        }
                        else if (selPlayRanking == 2 || selPlayRanking == 22 || selPlayRanking == 32 || selPlayRanking == 42)
                        {
                            draw_text(_textX + _textWidth, _textY, scrStringVal("info_time_rank_nd", selPlayRanking));
                        }
                        else if (selPlayRanking == 3 || selPlayRanking == 23 || selPlayRanking == 33 || selPlayRanking == 43)
                        {
                            draw_text(_textX + _textWidth, _textY, scrStringVal("info_time_rank_rd", selPlayRanking));
                        }
                        else
                        {
                            draw_text(_textX + _textWidth, _textY, scrStringVal("info_time_rank_th", selPlayRanking));
                        }
                        draw_set_halign(fa_left);
                        _textY += 8;
                    }
                }
""".Replace("\r\n", "\n");
var stringLineBreaksReplacement = """
function string_line_breaks(arg0, arg1, arg2)
{
    // This script is used during rmInit, before appended UFO50_CHS scripts are
    // guaranteed to be registered. Keep the Chinese branch fully self-contained.
    if (global.language == global.LANG_JAPANESE && font_exists(global.fontDefault_CHS))
    {
        var _lines = [];
        var _lineCount = 0;
        var _line = "";
        var _stop = false;
        var _width = arg1 * 8;
        var _lineWidth = 0;
        var _lastToken = "";
        var _lastTokenWidth = 0;
        var _noLineStart = "，。！？；：、）》】」』…";
        var _noLineEnd = "（《【「『";
        var _text = string_replace_all(arg0, global.CARRIAGE_RETURN, global.CARRIAGE_RETURN_SIMPLIFIED);
        var _inputControls = false;
        for (var _scan = 1; _scan < string_length(_text); _scan++)
        {
            if (string_char_at(_text, _scan) == "[" && string_pos(string_char_at(_text, _scan + 1), "UDLR12SEeF") > 0)
                _inputControls = true;
        }
        var _previousFont = global.currFont;
        draw_set_font(global.fontDefault_CHS);
        global.tooLongWord = false;
        for (var _i = 1; _i <= string_length(_text); _i++)
        {
            var _char = string_char_at(_text, _i);
            // Keep runtime fields indivisible: ****, {0}, [1], and related forms.
            if (_char == "*")
            {
                while (_i < string_length(_text) && string_char_at(_text, _i + 1) == "*")
                {
                    _char += "*";
                    _i++;
                }
            }
            else if (_char == "[" && _i < string_length(_text) && string_pos(string_char_at(_text, _i + 1), "UDLR12SEeF") > 0)
            {
                // 输入绘制器同时接受 [U 和 [U]，只消费这一枚图标。
                _char += string_char_at(_text, ++_i);
                if (string_char_at(_text, _i + 1) == "]")
                    _char += string_char_at(_text, ++_i);
            }
            else if (_char == "{" || _char == "[")
            {
                var _closer = (_char == "{") ? "}" : "]";
                while (_i < string_length(_text))
                {
                    var _controlChar = string_char_at(_text, _i + 1);
                    _char += _controlChar;
                    _i++;
                    if (_controlChar == _closer)
                        break;
                }
            }
            if (_char == global.CARRIAGE_RETURN_SIMPLIFIED)
            {
                _lines[_lineCount++] = _line;
                _line = "";
                _lineWidth = 0;
                _lastToken = "";
                _lastTokenWidth = 0;
                if (arg2 > 0 && _lineCount >= arg2)
                {
                    _stop = true;
                    break;
                }
                continue;
            }
            if (_char == " " && _line == "")
                continue;
            var _candidate = _line + _char;
            var _tokenWidth = 0;
            if (_inputControls)
            {
                var _button = string_char_at(_char, 2);
                if (string_char_at(_char, 1) == "[" && string_pos(_button, "UDLR12SEeF") > 0)
                {
                    _tokenWidth = 8;
                    // 早期资源加载尚未初始化按键表；实际详情页按当前绑定测宽。
                    if (variable_global_exists("keyMap") && variable_global_exists("KEY_ICON_WIDTH"))
                    {
                        var _index = string_pos(_button, "UDLR12S") - 1;
                        if (_button == "E") _index = 6;
                        if (global.inputFocus == 2)
                        {
                            if (_index >= 0)
                                _tokenWidth = global.JOY_ICON_WIDTH[scrGetJoyIndex(global.joyMap[0][_index])];
                            else
                                _tokenWidth = 0;
                        }
                        else
                        {
                            var _key = -1;
                            if (_index >= 0) _key = global.keyMap[0][_index];
                            if (_button == "E") _key = 27;
                            if (_button == "e") _key = 13;
                            if (_button == "F") _key = 112;
                            if (_key >= 0) _tokenWidth = global.KEY_ICON_WIDTH[_key];
                        }
                    }
                }
                else if (string_char_at(_char, 1) == "{" && string_pos(_button, "UDLR") > 0)
                {
                    _tokenWidth = 8;
                }
                else
                {
                    for (var _part = 1; _part <= string_length(_char); _part++)
                    {
                        var _glyph = string_char_at(_char, _part);
                        _tokenWidth += (ord(_glyph) >= 12288) ? string_width(_glyph) : 8;
                    }
                }
            }
            var _candidateWidth = _inputControls ? (_lineWidth + _tokenWidth) : string_width(_candidate);
            if (_line != "" && _candidateWidth > _width)
            {
                var _nextLine = (_char == " ") ? "" : _char;
                var _nextWidth = (_char == " ") ? 0 : _tokenWidth;
                if (_char != " ")
                {
                    var _lastPos = string_length(_line);
                    var _lastChar = string_char_at(_line, _lastPos);
                    if ((string_pos(_char, _noLineStart) > 0 || string_pos(_lastChar, _noLineEnd) > 0) && _lastPos > string_length(_lastToken))
                    {
                        // 标点回退也必须移动整枚图标，不能拆开 [2 等控制符。
                        _line = string_delete(_line, _lastPos - string_length(_lastToken) + 1, string_length(_lastToken));
                        _nextLine = _lastToken + _char;
                        _nextWidth += _lastTokenWidth;
                    }
                }
                _lines[_lineCount++] = _line;
                _line = _nextLine;
                _lineWidth = _nextWidth;
                _lastToken = (_nextLine == "") ? "" : _char;
                _lastTokenWidth = (_nextLine == "") ? 0 : _tokenWidth;
                if (arg2 > 0 && _lineCount >= arg2)
                {
                    _stop = true;
                    break;
                }
            }
            else
            {
                _line = _candidate;
                _lineWidth = _candidateWidth;
                _lastToken = _char;
                _lastTokenWidth = _tokenWidth;
                if (_candidateWidth > _width)
                    global.tooLongWord = true;
            }
        }
        if (!_stop && (_line != "" || _lineCount == 0))
            _lines[_lineCount++] = _line;
        if (arg2 > 0)
        {
            while (_lineCount < arg2)
                _lines[_lineCount++] = "";
        }
        if (font_exists(_previousFont))
            draw_set_font(_previousFont);
        return _lines;
    }

    var currLineNum = 1;
    var currLineContent = "";
    var posAbsolute = 1;
    var posInLine = 1;
    var lastSpaceAbsolute = -1;
    var lastSpaceInLine = -1;
    var lineArray = false;
    global.tooLongWord = false;
    if (string_length(arg0) > 2)
        arg0 = string_replace_all(arg0, global.CARRIAGE_RETURN, global.CARRIAGE_RETURN_SIMPLIFIED);
    do
    {
        var currChar = string_char_at(arg0, posAbsolute);
        var nextChar;
        if (posAbsolute < string_length(arg0))
            nextChar = string_char_at(arg0, posAbsolute + 1);
        else
            nextChar = " ";
        var carriageReturn = false;
        if (nextChar == global.CARRIAGE_RETURN_SIMPLIFIED)
        {
            nextChar = " ";
            arg0 = string_replace(arg0, global.CARRIAGE_RETURN_SIMPLIFIED, " ");
            carriageReturn = true;
        }
        currLineContent += currChar;
        if (currChar == " ")
        {
            lastSpaceInLine = posInLine;
            lastSpaceAbsolute = posAbsolute;
        }
        if (posInLine == arg1 || carriageReturn)
        {
            if (currChar != " " && nextChar == " ")
            {
                lineArray[currLineNum - 1] = currLineContent;
                posAbsolute++;
            }
            else if (lastSpaceInLine != -1)
            {
                lineArray[currLineNum - 1] = string_copy(currLineContent, 1, lastSpaceInLine - 1);
                posAbsolute = lastSpaceAbsolute;
            }
            else
            {
                lineArray[currLineNum - 1] = currLineContent;
                global.tooLongWord = true;
                posAbsolute++;
            }
            currLineContent = "";
            posInLine = 1;
            currLineNum++;
            while (string_char_at(arg0, posAbsolute) == " ")
                posAbsolute++;
            lastSpaceAbsolute = -1;
            lastSpaceInLine = -1;
        }
        else
        {
            posAbsolute++;
            posInLine++;
        }
    }
    until (posAbsolute > string_length(arg0));
    if (currLineContent != "")
        lineArray[currLineNum - 1] = currLineContent;
    if (arg2 <= 0)
        return lineArray;
    var fixedArray = false;
    for (var l = 0; l < arg2; l++)
    {
        if (l < array_length(lineArray))
            fixedArray[l] = lineArray[l];
        else
            fixedArray[l] = "";
    }
    return fixedArray;
}
""";
var stringManualReplacement = """
function scrStringManual(arg0, arg1)
{
    var str = "";
    var lim = 0;
    var wl = 0;
    var wc = 0;
    var _data = undefined;
    if (arg1 == 0)
    {
        var first5Chars = string_copy(arg0, 1, 5);
        if (first5Chars == "game_" || first5Chars == "hint_")
            _data = global.TEXT_META;
        else
            _data = global.TEXT_LIBRARY;
    }
    else if (arg1 >= 1 && arg1 <= global.NUM_GAMES)
    {
        _data = @@array_get@@(global.TEXT_GAME, arg1);
    }
    else
    {
        str = "STRING NOT FOUND! BAD GAME ID!";
    }
    if (arg1 >= 0 && arg1 <= global.NUM_GAMES)
    {
        if (is_undefined(_data))
        {
            str = "DATA STRUCTURE NOT FOUND!";
        }
        else
        {
            str = variable_struct_get(_data, arg0);
            var possibleLimit = variable_struct_get(_data, arg0 + "_lim");
            if (!is_undefined(possibleLimit) && !is_undefined(str))
            {
                lim = real(possibleLimit);
                if (lim > 0)
                    str = string_copy(str, 1, lim);
            }
            var numLines = variable_struct_get(_data, arg0 + "_wl");
            if (is_undefined(numLines))
                numLines = struct_get_from_hash(_data, variable_get_hash("default_wl"));
            var charsPerLine = variable_struct_get(_data, arg0 + "_wc");
            if (is_undefined(charsPerLine))
                charsPerLine = struct_get_from_hash(_data, variable_get_hash("default_wc"));
            if (!is_undefined(numLines) && !is_undefined(charsPerLine) && !is_undefined(str))
            {
                wl = real(numLines);
                wc = real(charsPerLine);
                if (wl > 0 && wc > 0)
                {
                    var split = string_line_breaks(str, wc, wl);
                    // These fields are truncation limits in the original loader,
                    // not authoritative display boxes. Actual wrapping belongs to
                    // draw_text_ext(width) or explicit string_line_breaks callers.
                    if (arg1 == 0 || global.language != global.LANG_JAPANESE)
                    {
                        lim = 0;
                        for (var i = 0; i < wl; i++)
                            lim += string_length(split[i]) + 1;
                        if (lim > 0)
                            str = string_copy(str, 1, lim);
                    }
                }
            }
        }
    }
    if (is_undefined(str))
        return global.EXTERNAL_TEXT_ERROR;
    str = string_replace_all(str, "　", " ");
    str = string_replace_all(str, "§", "\"");
    return str;
}
""";
var drawTextBgTail = """
    draw_set_color(oldColor);
    draw_text(argument[0], argument[1], argument[2]);
}
""".Replace("\r\n", "\n");
var drawTextBgTailNew = """
    draw_set_color(oldColor);
    if (global.language == global.LANG_JAPANESE && global.currGameID == 50)
        UFO50_CHS_draw_avianos_mixed(argument[0], argument[1], argument[2]);
    else
        draw_text(argument[0], argument[1], argument[2]);
}
""".Replace("\r\n", "\n");
var drawTextCeReplacement = """
function draw_text_ce(arg0, arg1, arg2, arg3)
{
    var str;
    var _useActualWidth = global.language == global.LANG_JAPANESE && global.currFont == global.fontDefault_CHS && UFO50_CHS_number_font(arg2) < 0;
    if (arg3 && !_useActualWidth)
    {
        str = string_even(arg2, arg3 - 1);
    }
    else
    {
        str = arg2;
    }
    var pixelWidth = _useActualWidth ? string_width(str) : string_length(str) * 8;
    var startX = arg0 - floor(pixelWidth / 2);
    draw_text(startX, arg1, str);
}
""";
var drawTextCenteredReplacement = """
function draw_text_centered(arg0, arg1, arg2, arg3)
{
    var _useActualWidth = global.language == global.LANG_JAPANESE && global.currFont == global.fontDefault_CHS && UFO50_CHS_number_font(arg2) < 0;
    var pixelWidth = _useActualWidth ? string_width(arg2) : string_length(arg2) * arg3;
    var startX = arg0 - floor(pixelWidth / 2);
    draw_text(startX, arg1, arg2);
}
""";
var drawTextBgCenteredReplacement = """
function draw_text_bg_centered(arg0, arg1, arg2, arg3, arg4, arg5, arg6)
{
    var numChars = string_length(arg2);
    var oldColor = draw_get_color();
    var _useActualWidth = global.language == global.LANG_JAPANESE && global.currFont == global.fontDefault_CHS && UFO50_CHS_number_font(arg2) < 0;
    var pixelWidth = _useActualWidth ? string_width(arg2) : numChars * arg4;
    var startX = arg0 - floor(pixelWidth / 2);
    var _charX = startX;
    for (var q = 1; q <= numChars; q++)
    {
        var c = string_char_at(arg2, q);
        var _charWidth = _useActualWidth ? string_width(c) : arg4;
        if (c != " " || !arg6)
        {
            draw_set_color(arg3);
            draw_rectangle(_charX, arg1, (_charX + _charWidth) - 1, (arg1 + arg5) - 1, false);
        }
        _charX += _charWidth;
    }
    draw_set_color(oldColor);
    draw_text(startX, arg1, arg2);
}
""";

var importGroup = new UndertaleModLib.Compiler.CodeImportGroup(Data);
importGroup.AutoCreateAssets = true;
importGroup.ThrowOnNoOpFindReplace = true;
importGroup.QueueReplace("gml_GlobalScript_scrInitFonts", initFonts);
importGroup.QueueReplace("gml_GlobalScript_scrSetFont", setFont);
importGroup.QueueReplace("gml_GlobalScript_scrLoadLibraryText", loadLibraryText);
importGroup.QueueReplace("gml_GlobalScript_scrLoadProfile", loadProfile);
importGroup.QueueReplace("gml_GlobalScript_scrDrawProfile", drawProfile);
importGroup.QueueFindReplace("gml_Object_oLibrary_Draw_0", oldInfoBar, newInfoBar, true);
importGroup.QueueFindReplace("gml_Object_oLibrary_Draw_0", descriptionGenreSpacingAnchor, descriptionGenreSpacingReplacement, true);
importGroup.QueueFindReplace("gml_Object_oLibrary_Draw_0", descriptionMainSpacingAnchor, descriptionMainSpacingReplacement, true);
importGroup.QueueFindReplace("gml_Object_oLibrary_Draw_0", timeSpentAnchor, timeSpentReplacement, true);
importGroup.QueueReplace("gml_GlobalScript_string_line_breaks", stringLineBreaksReplacement);
importGroup.QueueReplace("gml_GlobalScript_scrStringManual", stringManualReplacement);
importGroup.QueueFindReplace("gml_GlobalScript_draw_text_bg", drawTextBgTail, drawTextBgTailNew, true);
importGroup.QueueReplace("gml_GlobalScript_draw_text_ce", drawTextCeReplacement);
importGroup.QueueReplace("gml_GlobalScript_draw_text_centered", drawTextCenteredReplacement);
importGroup.QueueReplace("gml_GlobalScript_draw_text_bg_centered", drawTextBgCenteredReplacement);
importGroup.QueueFindReplace("gml_Object_oLibrary_Other_24", "NUM_LINES = 7;", "NUM_LINES = (global.language == global.LANG_JAPANESE) ? max(2, floor(56 / max(8, ceil(string_height(\"中\"))))) : 7;", true);
importGroup.QueueFindReplace("gml_Object_oLibrary_Other_24", "string(_justTheYear) + \"ねん\" + string(_monthStr)", "string(_justTheYear) + \"年\" + string(_monthStr)", true);
importGroup.QueueFindReplace("gml_GlobalScript_scr12_Meta", "global.mGameTitle[arg0] = \"GRIMSTONE\";", "global.mGameTitle[arg0] = scrString(\"game_name_12\");", true);
importGroup.QueueFindReplace("gml_GlobalScript_scrDrawTextInput", drawTextInputHeader, drawTextInputHeaderNew, true);
importGroup.QueueFindReplace("gml_GlobalScript_scrDrawTextInput", drawTextInputMeasure, drawTextInputMeasureNew, true);
importGroup.QueueFindReplace("gml_GlobalScript_scrDrawTextInput", drawTextInputGlyph, drawTextInputGlyphNew, true);
importGroup.QueueFindReplace("gml_GlobalScript_scrDrawTextCentered", "var strLen = string_length(arg0) * arg3;", "var strLen = (global.language == global.LANG_JAPANESE && global.currFont == global.fontDefault_CHS && UFO50_CHS_number_font(arg0) < 0) ? string_width(arg0) : string_length(arg0) * arg3;", true);
importGroup.QueueFindReplace("gml_GlobalScript_scrDrawTextCenteredPoint", "strLenTemp = string_length(arg0) * arg3;", "strLenTemp = (global.language == global.LANG_JAPANESE && global.currFont == global.fontDefault_CHS && UFO50_CHS_number_font(arg0) < 0) ? string_width(arg0) : string_length(arg0) * arg3;", true);
importGroup.QueueFindReplace("gml_GlobalScript_scrPause", "global.prePauseFont = global.currFont;", "global.prePauseFont = global.currFont;\n    global.chsPrePauseRequestedFont = global.chsRequestedFont;", true);
importGroup.QueueFindReplace("gml_GlobalScript_scrUnpause", "scrSetFont(global.prePauseFont);", "scrSetFont(global.prePauseFont);\n    global.chsRequestedFont = global.chsPrePauseRequestedFont;", true);
// Temporary overlays must restore the original requested font with the routed font.
foreach (var overlay in new[] { "gml_Object_oGame_Draw_0", "gml_Object_oAchManager_Draw_0" })
{
    importGroup.QueueFindReplace(overlay, "var _preFont = global.currFont;", "var _preFont = global.currFont;\nvar _preRequestedFont = global.chsRequestedFont;", true);
    importGroup.QueueFindReplace(overlay, "scrSetFont(_preFont);", "scrSetFont(_preFont);\nglobal.chsRequestedFont = _preRequestedFont;", true);
}
importGroup.QueueFindReplace("gml_GlobalScript_draw_text_with_sprites", textWithSpritesAdvance, textWithSpritesAdvanceNew, true);
importGroup.QueueFindReplace("gml_Object_o35b__Game_Draw_0", "draw_set_font(global.fontTall);", "scrSetFont(global.fontTall);", true);
importGroup.QueueFindReplace("gml_Object_o35b__Game_Draw_0", "draw_set_font(global.fontDefault);", "scrSetFont(global.fontDefault);", true);
importGroup.QueueFindReplace("gml_Object_oLibrary_Draw_0", titleCreditAnchor, titleCreditReplacement, true);
importGroup.QueueFindReplace("gml_Object_oPauseMenu_Draw_0", "_xview + 104 + (8 * string_length(global.currFileName))", "_xview + 104 + string_width(global.currFileName)", true);
importGroup.QueueFindReplace("gml_Object_oPauseMenu_Draw_0", "scrDrawTextCentered(string_even(menuHeader, 3), _xview, _yview + 8, 8, 384);", "scrDrawTextCentered((global.language == global.LANG_JAPANESE) ? menuHeader : string_even(menuHeader, 3), _xview, _yview + 8, 8, 384);", true);
importGroup.QueueFindReplace("gml_Object_oPauseMenu_Draw_0", "draw_text_bg_centered(_xview + 192, _yPos, \" \" + string_even(itemName[i], (i % 2) + 2) + \" \"", "draw_text_bg_centered(_xview + 192, _yPos, \" \" + ((global.language == global.LANG_JAPANESE) ? itemName[i] : string_even(itemName[i], (i % 2) + 2)) + \" \"", true);
importGroup.QueueFindReplace("gml_Object_oPauseMenu_Draw_0", "scrDrawTextCentered(string_even(itemName[i], 3), _xview, _yPos, 8, 384);", "scrDrawTextCentered((global.language == global.LANG_JAPANESE) ? itemName[i] : string_even(itemName[i], 3), _xview, _yPos, 8, 384);", true);
importGroup.QueueFindReplace("gml_Object_oTitleScreens_Draw_0", "scrDrawTextCentered(string_even(global.mGameTitle[global.currGame], 3), titleX, 64 + titleY, 8, 384);", "scrDrawTextCentered((global.language == global.LANG_JAPANESE) ? global.mGameTitle[global.currGame] : string_even(global.mGameTitle[global.currGame], 3), titleX, 64 + titleY, 8, 384);", true);
importGroup.QueueFindReplace("gml_Object_oConfirm_Draw_0", "scrDrawTextCentered(string_even(strUpper, 1), tx - 100, ty - 16, 8, 200);", "scrDrawTextCentered((global.language == global.LANG_JAPANESE) ? strUpper : string_even(strUpper, 1), tx - 100, ty - 16, 8, 200);", true);
importGroup.QueueFindReplace("gml_Object_oConfirm_Draw_0", "scrDrawTextCentered(string_even(strUpper, 1), tx - 100, ty - 32, 8, 200);", "scrDrawTextCentered((global.language == global.LANG_JAPANESE) ? strUpper : string_even(strUpper, 1), tx - 100, ty - 32, 8, 200);", true);
importGroup.QueueFindReplace("gml_Object_oConfirm_Draw_0", "scrDrawTextCentered(string_even(strLower, 1), tx - 100, ty - 8, 8, 200);", "scrDrawTextCentered((global.language == global.LANG_JAPANESE) ? strLower : string_even(strLower, 1), tx - 100, ty - 8, 8, 200);", true);
importGroup.QueueFindReplace("gml_Object_o29_Game_Draw_0", "scrDrawTextCentered(string_even(scrString(\"level\") + \" \" + levelNum, 3), 0, 72, 8, 384);", "var _levelLabel = scrString(\"level\") + \" \" + levelNum;\n    scrDrawTextCentered((global.language == global.LANG_JAPANESE) ? _levelLabel : string_even(_levelLabel, 3), 0, 72, 8, 384);", true);
importGroup.QueueFindReplace("gml_GlobalScript_scrInitDisplay", "    window_set_size(384 * global.scale, 216 * global.scale);", "    var _startupScale = min(global.scale, global.scaleFill);\n    window_set_size(384 * _startupScale, 216 * _startupScale);", true);
importGroup.QueueFindReplace("gml_Object_oPauseMenu_Draw_0", "\nif (state == STATE_TERMINAL)\n", languageNotice + "\nif (state == STATE_TERMINAL)\n", true);
// 纯数字串依据调用者请求的原版字体绘制。包括补零、时间、比分、货币、
// 百分比和倍率；中文正文中的数字继续随正文排版，不在这里拆开。
importGroup.QueueReplace("gml_GlobalScript_UFO50_CHS_number_font", """
function UFO50_CHS_number_font(arg0)
{
    if (global.language != global.LANG_JAPANESE) return -1;
    var _text = string(arg0);
    var _hasDigit = false;
    for (var _i = 1; _i <= string_length(_text); _i++)
    {
        var _char = string_char_at(_text, _i);
        if (string_pos(_char, "0123456789") > 0) _hasDigit = true;
        else if (string_pos(_char, " +-/:.,%$()MXPx×^") == 0) return -1;
        else if (string_pos(_char, "+-/:.,%$()×^") > 0) _hasDigit = true;
    }
    if (!_hasDigit) return -1;
    var _font = draw_get_font();
    if (_font == global.fontDefault_CHS) _font = global.chsRequestedFont;
    return font_exists(_font) && _font != global.fontDefault_CHS ? _font : -1;
}
""");
// Fixed-width counters following translated labels keep their original digits.
importGroup.QueueReplace("gml_GlobalScript_UFO50_CHS_draw_labeled_number", """
function UFO50_CHS_draw_labeled_number(arg0, arg1, arg2, arg3, arg4)
{
    if (global.language != global.LANG_JAPANESE)
    {
        draw_text(arg0, arg1, string(arg2) + string(arg3));
        return;
    }
    var _font = draw_get_font();
    var _numberFont = UFO50_CHS_number_font(arg3);
    if (_numberFont < 0)
    {
        draw_text(arg0, arg1, string(arg2) + string(arg3));
        return;
    }
    var _labelWidth = string_width(arg2);
    draw_set_font(_numberFont);
    var _numberWidth = string_width(arg3);
    draw_set_font(_font);
    var _align = draw_get_halign();
    if (arg4 == fa_right) arg0 -= _labelWidth + _numberWidth;
    else if (arg4 == fa_center) arg0 -= floor((_labelWidth + _numberWidth) / 2);
    draw_set_halign(fa_left);
    draw_text(arg0, arg1, arg2);
    draw_text(arg0 + _labelWidth, arg1, arg3);
    draw_set_halign(_align);
}
""");
importGroup.QueueFindReplace("gml_Object_o17__Game_Draw_0", "hiScoreFormat = scrString(\"high_score\") + \":\" + hiScoreFormat;\ndraw_text(384 - (string_length(hiScoreFormat) * 8) - 8, 8, hiScoreFormat);", "if (global.language == global.LANG_JAPANESE)\n    UFO50_CHS_draw_labeled_number(376, 8, scrString(\"high_score\") + \":\", hiScoreFormat, fa_right);\nelse {\nhiScoreFormat = scrString(\"high_score\") + \":\" + hiScoreFormat;\ndraw_text(384 - (string_length(hiScoreFormat) * 8) - 8, 8, hiScoreFormat);\n}", true);
importGroup.QueueFindReplace("gml_Object_o46_Mas_Draw_0", "draw_text(_xv + 136, _yv + 160, scrString(\"to_next\") + \": \" + strNext);", "UFO50_CHS_draw_labeled_number(_xv + 136, _yv + 160, scrString(\"to_next\") + \": \", strNext, fa_left);", true);
importGroup.QueueFindReplace("gml_Object_o08_Mas_Draw_0", "draw_text(200, 24, scrString(\"cont\") + strExtraLives);", "UFO50_CHS_draw_labeled_number(200, 24, scrString(\"cont\"), strExtraLives, fa_left);", true);
importGroup.QueueFindReplace("gml_Object_o19_Mas_Draw_0", "draw_text(xMenu + 8, yMenu + 8, scrString(\"level_abbreviated\") + string(stringLevel));", "UFO50_CHS_draw_labeled_number(xMenu + 8, yMenu + 8, scrString(\"level_abbreviated\"), string(stringLevel), fa_left);", true);
importGroup.QueueReplace("gml_GlobalScript_UFO50_CHS_draw_text", @"
function UFO50_CHS_draw_text(arg0, arg1, arg2)
{
    var _font = draw_get_font();
    var _numberFont = UFO50_CHS_number_font(arg2);
    if (_numberFont >= 0) draw_set_font(_numberFont);
    var _spriteDigits = (_font == global.fontDigital || _font == global.fontDigitalMini || _font == global.fontDigitalBig || _font == global.fontDigital2);
    if (global.language == global.LANG_JAPANESE && !_spriteDigits && _numberFont < 0)
        arg1 -= 1;
    draw_text(arg0, arg1, arg2);
    if (_numberFont >= 0) draw_set_font(_font);
}
");
importGroup.QueueReplace("gml_GlobalScript_UFO50_CHS_draw_key_value_row", """
function UFO50_CHS_draw_key_value_row(arg0, arg1, arg2, arg3, arg4)
{
    var _label = string(arg3);
    var _value = string(arg4);
    var _lineStep = max(8, ceil(string_height("中")));
    var _gap = 8;
    draw_set_halign(fa_left);
    if (string_width(_label) + _gap + string_width(_value) <= arg2)
    {
        UFO50_CHS_draw_text(arg0, arg1, _label);
        draw_set_halign(fa_right);
        UFO50_CHS_draw_text(arg0 + arg2, arg1, _value);
        draw_set_halign(fa_left);
        return _lineStep;
    }
    UFO50_CHS_draw_text(arg0, arg1, _label);
    draw_set_halign(fa_right);
    UFO50_CHS_draw_text(arg0 + arg2, arg1 + _lineStep, _value);
    draw_set_halign(fa_left);
    return _lineStep * 2;
}
""");
importGroup.QueueReplace("gml_GlobalScript_UFO50_CHS_wrap_text_array", """
function UFO50_CHS_wrap_text_array(arg0, arg1, arg2)
{
    var _lines = [];
    var _lineCount = 0;
    var _line = "";
    var _stop = false;
    var _noLineStart = "，。！？；：、）》】」』…";
    var _noLineEnd = "（《【「『";
    var _text = string_replace_all(arg0, global.CARRIAGE_RETURN, global.CARRIAGE_RETURN_SIMPLIFIED);
    var _previousFont = global.currFont;
    draw_set_font(global.fontDefault_CHS);
    global.tooLongWord = false;
    for (var _i = 1; _i <= string_length(_text); _i++)
    {
        var _char = string_char_at(_text, _i);
        // 动态字段必须作为一个整体保留，否则在星号或 {0} 中间插入换行会让
        // 后续替换器把同一个字段误认成多个参数。
        if (_char == "*")
        {
            while (_i < string_length(_text) && string_char_at(_text, _i + 1) == "*")
            {
                _char += "*";
                _i++;
            }
        }
        else if (_char == "{" || _char == "[")
        {
            var _closer = (_char == "{") ? "}" : "]";
            while (_i < string_length(_text))
            {
                var _controlChar = string_char_at(_text, _i + 1);
                _char += _controlChar;
                _i++;
                if (_controlChar == _closer)
                    break;
            }
        }
        if (_char == global.CARRIAGE_RETURN_SIMPLIFIED)
        {
            _lines[_lineCount] = _line;
            _lineCount++;
            _line = "";
            if (arg2 > 0 && _lineCount >= arg2)
            {
                _stop = true;
                break;
            }
            continue;
        }
        if (_char == " " && _line == "")
            continue;
        var _candidate = _line + _char;
        if (_line != "" && string_width(_candidate) > arg1)
        {
            var _nextLine = _char;
            if (_char == " ")
            {
                _nextLine = "";
            }
            else
            {
                var _lastPos = string_length(_line);
                var _lastChar = string_char_at(_line, _lastPos);
                if ((string_pos(_char, _noLineStart) > 0 || string_pos(_lastChar, _noLineEnd) > 0) && _lastPos > 1)
                {
                    _line = string_delete(_line, _lastPos, 1);
                    _nextLine = _lastChar + _char;
                }
            }
            _lines[_lineCount] = _line;
            _lineCount++;
            _line = _nextLine;
            if (arg2 > 0 && _lineCount >= arg2)
            {
                _stop = true;
                break;
            }
        }
        else
        {
            _line = _candidate;
            if (string_width(_line) > arg1)
                global.tooLongWord = true;
        }
    }
    if (!_stop && (_line != "" || _lineCount == 0))
    {
        _lines[_lineCount] = _line;
        _lineCount++;
    }
    if (arg2 > 0)
    {
        while (_lineCount < arg2)
        {
            _lines[_lineCount] = "";
            _lineCount++;
        }
    }
    if (font_exists(_previousFont))
        draw_set_font(_previousFont);
    return _lines;
}
""");
importGroup.QueueReplace("gml_GlobalScript_UFO50_CHS_wrap_text", """
function UFO50_CHS_wrap_text(arg0, arg1, arg2 = 0)
{
    if (arg1 <= 0)
        return arg0;
    var _lines = UFO50_CHS_wrap_text_array(arg0, arg1, arg2);
    var _result = "";
    var _lastLine = array_length(_lines) - 1;
    while (_lastLine > 0 && _lines[_lastLine] == "")
        _lastLine--;
    for (var _i = 0; _i <= _lastLine; _i++)
    {
        if (_i > 0)
            _result += global.CARRIAGE_RETURN;
        _result += _lines[_i];
    }
    return _result;
}
""");
importGroup.QueueReplace("gml_GlobalScript_UFO50_CHS_draw_avianos_mixed", """
function UFO50_CHS_draw_avianos_mixed(arg0, arg1, arg2)
{
    if (global.language != global.LANG_JAPANESE || !font_exists(global.fontAvianos) || !font_exists(global.fontDefault_CHS))
    {
        draw_text(arg0, arg1, arg2);
        return;
    }
    var _previousFont = global.currFont;
    var _startX = arg0;
    var _xx = arg0;
    var _yy = arg1;
    draw_set_font(global.fontDefault_CHS);
    var _lineStep = max(8, ceil(string_height("中")));
    for (var _i = 1; _i <= string_length(arg2); _i++)
    {
        var _char = string_char_at(arg2, _i);
        if (_char == global.CARRIAGE_RETURN)
        {
            _xx = _startX;
            _yy += _lineStep;
        }
        else if (ord(_char) < 128)
        {
            draw_set_font(global.fontAvianos);
            draw_text(_xx, _yy, _char);
            _xx += 8;
        }
        else
        {
            draw_set_font(global.fontDefault_CHS);
            draw_text(_xx, _yy - 1, _char);
            _xx += max(8, round(string_width(_char)));
        }
    }
    if (font_exists(_previousFont))
        draw_set_font(_previousFont);
}
""");
importGroup.QueueReplace("gml_GlobalScript_UFO50_CHS_draw_text_ext", @"
function UFO50_CHS_draw_text_ext(arg0, arg1, arg2, arg3, arg4)
{
    var _font = draw_get_font();
    var _numberFont = UFO50_CHS_number_font(arg2);
    if (_numberFont >= 0) draw_set_font(_numberFont);
    var _spriteDigits = (_font == global.fontDigital || _font == global.fontDigitalMini || _font == global.fontDigitalBig || _font == global.fontDigital2);
    if (global.language == global.LANG_JAPANESE && !_spriteDigits && _numberFont < 0)
    {
        arg1 -= 1;
        arg2 = UFO50_CHS_wrap_text(arg2, arg4);
        var _lineStep = max(8, ceil(string_height(""中"")));
        if (arg3 > 0 && arg3 < _lineStep)
            arg3 = _lineStep;
    }
    draw_text_ext(arg0, arg1, arg2, arg3, arg4);
    if (_numberFont >= 0) draw_set_font(_font);
}
");
importGroup.QueueReplace("gml_GlobalScript_UFO50_CHS_draw_text_color", @"
function UFO50_CHS_draw_text_color(arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7)
{
    var _font = draw_get_font();
    var _numberFont = UFO50_CHS_number_font(arg2);
    if (_numberFont >= 0) draw_set_font(_numberFont);
    var _spriteDigits = (_font == global.fontDigital || _font == global.fontDigitalMini || _font == global.fontDigitalBig || _font == global.fontDigital2);
    if (global.language == global.LANG_JAPANESE && !_spriteDigits && _numberFont < 0)
        arg1 -= 1;
    draw_text_color(arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7);
    if (_numberFont >= 0) draw_set_font(_font);
}
");
// 佐尔达斯星的计数先画补零，再以原版 8px 格遮罩写入有效位。
// 数字与遮罩必须使用原精灵字体和基线，不能经过中文基线包装。
importGroup.QueueReplace("gml_GlobalScript_UFO50_CHS_draw_masked_counter", """
function UFO50_CHS_draw_masked_counter(arg0, arg1, arg2, arg3)
{
    var _previousFont = draw_get_font();
    var _previousColor = draw_get_color();
    draw_set_font(global.fontBlocky);
    draw_text(arg0, arg1, arg2);
    draw_set_color(c_black);
    for (var _q = 1; _q <= string_length(arg3); _q++)
    {
        if (string_char_at(arg3, _q) != " ")
            draw_rectangle(arg0 + ((_q - 1) * 8), arg1, arg0 + (_q * 8) - 1, arg1 + 7, false);
    }
    draw_set_color(_previousColor);
    draw_text(arg0, arg1, arg3);
    draw_set_font(_previousFont);
}
""");
importGroup.QueueReplace("gml_GlobalScript_UFO50_CHS_draw_name_grid_line", @"
function UFO50_CHS_draw_name_grid_line(arg0, arg1, arg2)
{
    if (global.language != global.LANG_JAPANESE || !font_exists(global.fontDefault_JP))
    {
        draw_text(arg0, arg1, arg2);
        return;
    }
    var _previousFont = global.currFont;
    draw_set_font(global.fontDefault_JP);
    draw_text(arg0, arg1, arg2);
    if (font_exists(_previousFont))
        draw_set_font(_previousFont);
}
");
importGroup.Import();

// The profile-name grid is authored as fixed 8px glyph pairs (character + box)
// on a 16px cursor grid. Keep those ASCII/kana-only rows on the original sprite
// font instead of sending the boxes through proportional Zpix metrics.
var pauseMenuCode = Data.Code.ByName("gml_Object_oPauseMenu_Draw_0");
if (pauseMenuCode == null)
    throw new System.Exception("Missing pause-menu draw code.");
var nameGridGroup = new UndertaleModLib.Compiler.CodeImportGroup(Data);
nameGridGroup.ThrowOnNoOpFindReplace = true;
nameGridGroup.QueueRegexFindReplace(
    pauseMenuCode,
    "(?<![A-Za-z0-9_])draw_text\\(([^\\r\\n;]*\"[^\"\\r\\n]*☐[^\"\\r\\n]*\")\\);",
    "UFO50_CHS_draw_name_grid_line($1);",
    true
);
nameGridGroup.Import();

// The library detail pages manually advance every row by 8px. Zpix is loaded
// by GameMaker in points, so its runtime height is larger than 8 screen pixels.
// Use the measured height for every content row, while preserving the tab/header
// spacer that is immediately followed by the language-specific header branch.
var libraryDrawCode = Data.Code.ByName("gml_Object_oLibrary_Draw_0");
if (libraryDrawCode == null)
    throw new System.Exception("Missing library draw code.");
var libraryLayoutGroup = new UndertaleModLib.Compiler.CodeImportGroup(Data);
libraryLayoutGroup.ThrowOnNoOpFindReplace = true;
libraryLayoutGroup.QueueRegexFindReplace(
    libraryDrawCode,
    @"_textY\s*\+=\s*8;(?!\s*if\s*\(global\.language\s*!=\s*global\.LANG_JAPANESE\))",
    "_textY += (global.language == global.LANG_JAPANESE) ? max(8, ceil(string_height(\"中\"))) : 8;",
    true
);
libraryLayoutGroup.Import();

// Several games split text into arrays and then draw each row manually. Those
// call sites bypass draw_text_ext(), so the general CHS line-step correction
// cannot reach them. PARTY HOUSE is the most visible case: its 80px sidebar
// used a 64px wrapping width and advanced Zpix rows by only 8px. Use the real
// safe width (72px, leaving an 8px right margin) and the measured CHS height.
var partyHouseDrawCode = Data.Code.ByName("gml_Object_o36_Game_Draw_0");
if (partyHouseDrawCode == null)
    throw new System.Exception("Missing PARTY HOUSE draw code.");
var partyHouseLayoutGroup = new UndertaleModLib.Compiler.CodeImportGroup(Data);
partyHouseLayoutGroup.ThrowOnNoOpFindReplace = true;
partyHouseLayoutGroup.QueueRegexFindReplace(
    partyHouseDrawCode,
    @"var lineY\s*=\s*INFO_Y\s*\+\s*8;",
    "var lineY = INFO_Y + 8;\n        var _infoLineStep = (global.language == global.LANG_JAPANESE) ? max(8, ceil(string_height(\"中\"))) : LINE_SPACE;\n        var _infoWrapChars = (global.language == global.LANG_JAPANESE) ? 9 : 8;",
    true
);
partyHouseLayoutGroup.QueueRegexFindReplace(
    partyHouseDrawCode,
    @"scrStringSplit\(([^\r\n]*?),\s*8,\s*0\)",
    "scrStringSplit($1, _infoWrapChars, 0)",
    true
);
partyHouseLayoutGroup.QueueFindReplace(
    partyHouseDrawCode,
    "string_line_breaks(scrStringVal(\"prestige_1\", PRESTIGE_GOAL), 8, 0)",
    "string_line_breaks(scrStringVal(\"prestige_1\", PRESTIGE_GOAL), _infoWrapChars, 0)",
    true
);
partyHouseLayoutGroup.QueueRegexFindReplace(
    partyHouseDrawCode,
    @"lineY\s*\+=\s*LINE_SPACE;",
    "lineY += _infoLineStep;",
    true
);
partyHouseLayoutGroup.Import();

// Apply the same measured row step to the remaining explicit split-and-draw
// loops that still hard-code 8px. Other languages keep their original layout.
var manualLineStepTargets = new[]
{
    new { Code = "gml_Object_o05_Game_Draw_0", Old = "96 + (8 * i)", New = "96 + (((global.language == global.LANG_JAPANESE) ? max(8, ceil(string_height(\"中\"))) : 8) * i)" },
    new { Code = "gml_GlobalScript_scr07_DrawModUI", Old = "(64 * i) + (8 * j)", New = "(64 * i) + (((global.language == global.LANG_JAPANESE) ? max(8, ceil(string_height(\"中\"))) : 8) * j)" },
    new { Code = "gml_Object_o38_Mas_Draw_0", Old = "var _y = yText + (8 * k);\n            if (_y > (_yview + 208))", New = "var _cutLineStep = (global.language == global.LANG_JAPANESE) ? max(8, ceil(string_height(\"中\"))) : 8;\n            var _y = yText + (_cutLineStep * k);\n            if ((_y + _cutLineStep) > (_yview + 216))" },
    new { Code = "gml_Object_o40_HammerMan_Draw_0", Old = "yy + 8 + (8 * i)", New = "yy + 8 + (((global.language == global.LANG_JAPANESE) ? max(8, ceil(string_height(\"中\"))) : 8) * i)" },
    new { Code = "gml_Object_o40_RodMan_Draw_0", Old = "yy + 8 + (8 * i)", New = "yy + 8 + (((global.language == global.LANG_JAPANESE) ? max(8, ceil(string_height(\"中\"))) : 8) * i)" },
    new { Code = "gml_Object_o40_Shop_Draw_0", Old = "draw_text(xx + 8, yy + 16, str[1]);", New = "draw_text(xx + 8, yy + 8 + ((global.language == global.LANG_JAPANESE) ? max(8, ceil(string_height(\"中\"))) : 8), str[1]);" }
};
foreach (var target in manualLineStepTargets)
{
    var targetCode = Data.Code.ByName(target.Code);
    if (targetCode == null)
        throw new System.Exception($"Missing manual line-step target: {target.Code}");
    var targetGroup = new UndertaleModLib.Compiler.CodeImportGroup(Data);
    targetGroup.ThrowOnNoOpFindReplace = true;
    targetGroup.QueueFindReplace(targetCode, target.Old, target.New, true);
    targetGroup.Import();
}

// 烫脚球的三行隐藏留言绕过通用多行绘制；按实际行高保留底部边界。
var hotfootMetaOld = """
                draw_text_bg_centered(192, 176, scrStringManual("game_meta_message_43_1", 0), 0, 8, 8, 0);
                draw_text_bg_centered(192, 184, scrStringManual("game_meta_message_43_2", 0), 0, 8, 8, 0);
                draw_text_bg_centered(192, 192, scrStringManual("game_meta_message_43_3", 0), 0, 8, 8, 0);
""";
var hotfootMetaNew = """
                var _metaStep = (global.language == global.LANG_JAPANESE) ? max(8, ceil(string_height("中"))) : 8;
                var _metaTop = min(176, 216 - (3 * _metaStep));
                draw_text_bg_centered(192, _metaTop, scrStringManual("game_meta_message_43_1", 0), 0, 8, _metaStep, 0);
                draw_text_bg_centered(192, _metaTop + _metaStep, scrStringManual("game_meta_message_43_2", 0), 0, 8, _metaStep, 0);
                draw_text_bg_centered(192, _metaTop + (2 * _metaStep), scrStringManual("game_meta_message_43_3", 0), 0, 8, _metaStep, 0);
""";
var hotfootMetaGroup = new UndertaleModLib.Compiler.CodeImportGroup(Data);
hotfootMetaGroup.ThrowOnNoOpFindReplace = true;
hotfootMetaGroup.QueueFindReplace(Data.Code.ByName("gml_Object_o43_Game_Draw_0"), hotfootMetaOld, hotfootMetaNew, true);
hotfootMetaGroup.Import();

// AVIANOS 把 ASCII 字符映射为资源、兵种、建筑和状态图标；中文槽不能把这些
// 字符一并路由到 Zpix。该游戏的普通绘制改为逐字符混排：ASCII 保留原图标字体，
// 中文使用 Zpix。draw_text_bg 的尾部已在上方单独接入同一混排函数。
var avianosDrawCode = Data.Code.ByName("gml_Object_o50_Game_Draw_0");
if (avianosDrawCode == null)
    throw new System.Exception("Missing AVIANOS draw code.");
var avianosDrawTextCalls = avianosDrawCode.Instructions.Count(instruction =>
    instruction.Kind == UndertaleInstruction.Opcode.Call && instruction.ValueFunction?.Name?.Content == "draw_text");
if (avianosDrawTextCalls < 50)
    throw new System.Exception($"Unexpected AVIANOS draw_text coverage: {avianosDrawTextCalls}");
var avianosImportGroup = new UndertaleModLib.Compiler.CodeImportGroup(Data);
avianosImportGroup.ThrowOnNoOpFindReplace = true;
avianosImportGroup.QueueRegexFindReplace(
    avianosDrawCode,
    @"(?<![A-Za-z0-9_])draw_text\s*\(",
    "UFO50_CHS_draw_avianos_mixed(",
    true
);
avianosImportGroup.Import();

// Zpix 保持官方原文件不变。GameMaker 没有 TTF 基线偏移参数，因此把三个
// 实际使用的内置文字绘制函数重定向到包装脚本，只在中文（日语槽）把 Y 上移 1px。

// 隔离实机复现的固定行排版：测量中文行高，保留原框和图标数量。
void FixLayout(string codeName, string oldText, string newText)
{
    var code = Data.Code.ByName(codeName);
    if (code == null) throw new System.Exception($"Missing verified layout target: {codeName}");
    var group = new UndertaleModLib.Compiler.CodeImportGroup(Data);
    group.ThrowOnNoOpFindReplace = true;
    group.QueueFindReplace(code, oldText, newText, true);
    group.Import();
}

foreach (var counter in new[] {
    new { Position = "viewx + 320, viewy + 200", Template = "0:00:00", Value = "string_format(hours, 1, 0) + \":\" + string_format(minutes, 2, 0) + \":\" + string_format(seconds, 2, 0)" },
    new { Position = "viewx + 352, viewy + 64 + (24 * i)", Template = "00", Value = "string_format(resources[i] - resDelta[i], 2, 0)" }
})
{
    var original = $"draw_text({counter.Position}, \"{counter.Template}\");\n    draw_text_bg({counter.Position}, {counter.Value}, 0, 8, 8, true, false);";
    // 资源循环的第二行缩进为 8 个空格。
    if (counter.Template == "00") original = original.Replace("\n    draw_text_bg", "\n        draw_text_bg");
    FixLayout("gml_Object_o48_Game_Draw_0", original,
        $"if (global.language == global.LANG_JAPANESE)\n        UFO50_CHS_draw_masked_counter({counter.Position}, \"{counter.Template}\", {counter.Value});\n    else {{\n    {original}\n    }}");
}

FixLayout("gml_Object_o22_Game_Draw_0",
    "else if (state == STATE_CHAR_SELECT)\n{",
    "else if (state == STATE_CHAR_SELECT)\n{\n    var _statStep = (global.language == global.LANG_JAPANESE) ? max(8, ceil(string_height(\"中\"))) : 8;\n    var _statTop = (global.language == global.LANG_JAPANESE) ? min(400, 432 - (3 * _statStep) - 2) : 400;");
foreach (var row in new[] { new { Y = 400, Key = "speed", I = 0 }, new { Y = 408, Key = "control", I = 1 }, new { Y = 416, Key = "power", I = 2 } })
{
    FixLayout("gml_Object_o22_Game_Draw_0",
        $"+ 192, {row.Y}, \"{row.Key}\",",
        $"+ 192, _statTop + ({row.I} * _statStep), \"{row.Key}\",");
}
var statGroup = new UndertaleModLib.Compiler.CodeImportGroup(Data);
statGroup.ThrowOnNoOpFindReplace = true;
foreach (var row in new[] { new { Y = 403, I = 0 }, new { Y = 411, I = 1 }, new { Y = 419, I = 2 } })
{
    statGroup.QueueRegexFindReplace(Data.Code.ByName("gml_Object_o22_Game_Draw_0"),
        @"(draw_sprite\(sFX_StarRotate,[^\r\n]*), " + row.Y + @"\);",
        "$1, _statTop + (" + row.I + " * _statStep) + 3);", true);
    statGroup.QueueRegexFindReplace(Data.Code.ByName("gml_Object_o22_Game_Draw_0"),
        @"(draw_sprite_ext\(sFX_StarRotate,[^\r\n]*), " + (row.Y + 1) + @", 1\.2,",
        "$1, _statTop + (" + row.I + " * _statStep) + 4, 1.2,", true);
}
statGroup.Import();

FixLayout("gml_Object_o32_Mas_Draw_0", "var _y = _yNews + 136 + (i * 8);",
    "var _newsStep = (global.language == global.LANG_JAPANESE) ? max(8, ceil(string_height(\"中\"))) : 8;\n        var _y = _yNews + 136 + (i * _newsStep);");

FixLayout("gml_Object_o38_Mas_Draw_0", "var _w_stage_name = string_length(_stage_name_string) * 8;",
    "var _stageStep = (global.language == global.LANG_JAPANESE) ? max(8, ceil(string_height(\"中\"))) : 8;\n        scrSetFont(global.fontTall);\n        var _w_stage_name = (global.language == global.LANG_JAPANESE) ? string_width(_stage_name_string) : string_length(_stage_name_string) * 8;");
FixLayout("gml_Object_o38_Mas_Draw_0", "var _y_stage_name = _yview + 24;",
    "var _y_stage_name = _yview + 16 + _stageStep + ((global.language == global.LANG_JAPANESE) ? 2 : 0);");

// 超级斗士单人角色页：标签、值和输入图标分别留足实测行高。
foreach (var row in new[] { new { Old = 48, New = 44, Key = "\"movement\"" }, new { Old = 56, New = 55, Key = "charMovementName[char[0]]" }, new { Old = 72, New = 70, Key = "\"weapon\"" }, new { Old = 80, New = 81, Key = "charWeaponName[char[0]]" } })
{
    FixLayout("gml_Object_o42_Game_Draw_0", $"104, {row.Old}, 8, 64, 0, 8, 4);".Insert(0, row.Key + ", "),
        $"{row.Key}, 104, ((global.language == global.LANG_JAPANESE) ? {row.New} : {row.Old}), 8, 64, 0, 8, 4);");
}
foreach (var row in new[] { new { Old = 48, New = 52, Key = "controls_weapon_alt", Input = "[U][1]:" }, new { Old = 64, New = 68, Key = "controls_melee_attack", Input = "[D][1]:" }, new { Old = 80, New = 84, Key = "controls_block", Input = "[D]:" } })
{
    FixLayout("gml_Object_o42_Game_Draw_0", $"scrStringDraw(264, {row.Old}, \"{row.Key}\");",
        $"scrStringDraw(264, ((global.language == global.LANG_JAPANESE) ? {row.New} : {row.Old}), \"{row.Key}\");");
    FixLayout("gml_Object_o42_Game_Draw_0", $"scrDrawTextInput(216, {row.Old}, \"{row.Input}\",",
        $"scrDrawTextInput(216, ((global.language == global.LANG_JAPANESE) ? {row.New} : {row.Old}), \"{row.Input}\",");
}


// 虫灾猎手状态栏：汉字与原版 8px 计量图标各留独立空间。
var bugGroup = new UndertaleModLib.Compiler.CodeImportGroup(Data);
bugGroup.ThrowOnNoOpFindReplace = true;
bugGroup.QueueRegexFindReplace(Data.Code.ByName("gml_Object_o20_Game_Draw_0"),
    @"(draw_text_bg\((?:16|320), )8,", "$1((global.language == global.LANG_JAPANESE) ? 4 : 8),", true);
foreach (var row in new[] { new { Sprite = "s20_KillMeter", Y = 24 }, new { Sprite = "s20_KillMeter", Y = 32 }, new { Sprite = "s20_KillMeter", Y = 40 }, new { Sprite = "s20_TreasureMeter", Y = 64 }, new { Sprite = "s20_TurnMeter", Y = 88 } })
    bugGroup.QueueRegexFindReplace(Data.Code.ByName("gml_Object_o20_Game_Draw_0"),
        @"(draw_sprite\(" + row.Sprite + @",[^\r\n]*), " + row.Y + @"\);",
        "$1, ((global.language == global.LANG_JAPANESE) ? " + (row.Y + 4) + " : " + row.Y + "));", true);
bugGroup.Import();

// 双人行动计量条也已实机复现与中文标签重叠。
var actionMeterGroup = new UndertaleModLib.Compiler.CodeImportGroup(Data);
actionMeterGroup.ThrowOnNoOpFindReplace = true;
actionMeterGroup.QueueRegexFindReplace(Data.Code.ByName("gml_Object_o20_Game_Draw_0"),
    @"(draw_sprite\(s20_ActionMeter, [01], 8 \+ \(8 \* i\)), 88\);",
    "$1, ((global.language == global.LANG_JAPANESE) ? 92 : 88));", true);
actionMeterGroup.Import();

// 罗刹的标签使用中文，分数和倒计时保留原版数字字形。
FixLayout("gml_Object_o24_Mas_Draw_0",
    "draw_text_color((_xview + 384) - 32, _yview + 16, tstring, tCol, tCol, tCol, tCol, 1);",
    "if (global.language == global.LANG_JAPANESE) draw_set_font(global.fontDefault);\n" +
    "draw_text_color((_xview + 384) - 32, _yview + ((global.language == global.LANG_JAPANESE) ? 20 : 16), tstring, tCol, tCol, tCol, tCol, 1);\n" +
    "if (global.language == global.LANG_JAPANESE) scrSetFont(global.fontDefault);");
FixLayout("gml_Object_o24_Mas_Draw_0", "draw_text(_xview + 8, _yview + 16, sstring);",
    "if (global.language == global.LANG_JAPANESE) draw_set_font(global.fontDefault);\n" +
    "draw_text(_xview + 8, _yview + ((global.language == global.LANG_JAPANESE) ? 20 : 16), sstring);\n" +
    "if (global.language == global.LANG_JAPANESE) scrSetFont(global.fontDefault);");
var rakshasaGroup = new UndertaleModLib.Compiler.CodeImportGroup(Data);
rakshasaGroup.ThrowOnNoOpFindReplace = true;
rakshasaGroup.QueueRegexFindReplace(Data.Code.ByName("gml_Object_o24_Mas_Draw_0"),
    @"(draw_sprite\(s24_DeathCount,[^\r\n]*), _yview \+ 24\);",
    "$1, _yview + ((global.language == global.LANG_JAPANESE) ? 32 : 24));", true);
rakshasaGroup.Import();

// 魔法花园的日文标签绘在背景精灵上。复用同帧空白板面覆盖标签，
// 主题色和边框随原版 rank 变化，再绘制中文标题；不修改纹理像素。
FixLayout("gml_Object_o27_BG_Draw_0", "draw_sprite(s27_bgSideJP, (_rank * 2) + 1, (_xv + 384) - 96, _yv);",
    "draw_sprite(s27_bgSideJP, (_rank * 2) + 1, (_xv + 384) - 96, _yv);\n" +
    "    draw_sprite_part(s27_bgSideJP, _rank * 2, 4, 80, 72, 12, _xv + 4, _yv + 52);\n" +
    "    draw_sprite_part(s27_bgSideJP, (_rank * 2) + 1, 20, 80, 72, 12, _xv + 308, _yv + 52);\n" +
    "    var _labelColor = draw_get_color();\n    scrSetFont(global.fontDefault);\n    draw_set_color(c_black);\n" +
    "    draw_text(_xv + 16, _yv + 52, \"已救奥比\");\n    draw_text(_xv + 320, _yv + 52, \"分数\");\n    draw_set_color(_labelColor);");

// 双人角色页同样已在隔离游戏中复现并保留证据。
foreach (var row in new[] { new { Old = 48, New = 44, Key = "\"movement\"" }, new { Old = 56, New = 55, Key = "charMovementName[char[1]]" }, new { Old = 72, New = 70, Key = "\"weapon\"" }, new { Old = 80, New = 81, Key = "charWeaponName[char[1]]" } })
    FixLayout("gml_Object_o42_Game_Draw_0", $"{row.Key}, 216, {row.Old}, 8, 64, 0, 8, 4);",
        $"{row.Key}, 216, ((global.language == global.LANG_JAPANESE) ? {row.New} : {row.Old}), 8, 64, 0, 8, 4);");

// 潜水员商店保持原输入索引，绘制按光标滚动；返回项和底部资源栏不挪出原框。
var diverDraw = Data.Code.ByName("gml_Object_o19_Mas_Draw_0");
var diverContext = new UndertaleModLib.Decompiler.GlobalDecompileContext(Data);
var diverSource = GetDecompiledText(diverDraw, diverContext);
var shopStart = diverSource.IndexOf("else if (currMenu == 1)");
var shopEnd = diverSource.IndexOf("else if (currMenu == 2)", shopStart);
if (shopStart < 0 || shopEnd < 0) throw new System.Exception("Missing verified diver shop branch.");
var shopBefore = diverSource.Substring(shopStart, shopEnd - shopStart);
var shopAfter = shopBefore.Replace("var yMenu = yv + 48;", "var yMenu = yv + 48;\n        var _shopStep = (global.language == global.LANG_JAPANESE) ? max(8, ceil(string_height(\"中\"))) : 8;\n        var _shopRows = max(1, floor(80 / _shopStep));\n        var _shopFirst = clamp(((menuSel == 99) ? pageLen : menuSel) - _shopRows + 1, 0, max(0, pageLen + 1 - _shopRows));")
    .Replace("(8 * menuSel)", "(_shopStep * (menuSel - _shopFirst))")
    .Replace("for (var i = 0; i < (pageLen + 1); i++)", "for (var i = _shopFirst; i < min(pageLen + 1, _shopFirst + _shopRows); i++)")
    .Replace("(8 * i)", "(_shopStep * (i - _shopFirst))")
    .Replace("yMenu + 128, stringBank", "yMenu + ((global.language == global.LANG_JAPANESE) ? 132 : 128), stringBank")
    .Replace("yMenu + 128);", "yMenu + ((global.language == global.LANG_JAPANESE) ? 132 : 128));");
shopAfter = shopAfter.Replace("draw_text(xMenu + 216,", "if (global.language == global.LANG_JAPANESE) draw_set_font(global.fontDefault);\n        draw_text(xMenu + 216,")
    .Replace("draw_sprite(s19_MenuCoin, 0, xMenu + 208,", "if (global.language == global.LANG_JAPANESE) scrSetFont(global.fontDefault);\n        draw_sprite(s19_MenuCoin, 0, xMenu + 208,");
shopAfter = shopAfter.Replace("var xstr = (xMenu + 128 + (i * 8)) - (ii * 80);",
    "var xstr = (global.language == global.LANG_JAPANESE) ? xMenu + 128 + string_width(string_copy(strStore[storeText + ii], 1, i % 10)) : (xMenu + 128 + (i * 8)) - (ii * 80);")
    .Replace("var ystr = (yMenu - 16) + (ii * 8);",
    "var ystr = (yMenu - ((global.language == global.LANG_JAPANESE) ? 24 : 16)) + (ii * _shopStep);");
shopAfter = shopAfter.Replace("if (menuSel != 99)\n        {\n            scrStringDraw", "if (_shopFirst > 0) draw_sprite(s19_MenuCursor, 4, xMenu + 16, yMenu + 8);\n        if ((_shopFirst + _shopRows) < (pageLen + 1)) draw_sprite(s19_MenuCursor, 5, xMenu + 240, yMenu + 96);\n        if (menuSel != 99)\n        {\n            scrStringDraw");
// 两种珍宝费用沿用同一实测行高，图标与数量同步。
shopAfter = shopAfter.Replace("yMenu + 120 + (4 * i)", "yMenu + 120 + ((_shopStep * i) / 2)");
if (shopAfter == shopBefore) throw new System.Exception("Diver shop layout was not changed.");
FixLayout("gml_Object_o19_Mas_Draw_0", shopBefore, shopAfter);


// 长物品列表及三名角色左右手槽均已在隔离环境复现。滚动只改变绘制，
// 原版装备/交换/返回逻辑仍接收原索引和 listOffset。
diverSource = GetDecompiledText(diverDraw, new UndertaleModLib.Decompiler.GlobalDecompileContext(Data));
var invStart = diverSource.IndexOf("else if (currMenu == 2)");
var invEnd = diverSource.IndexOf("else if (currMenu == 5)", invStart);
if (invStart < 0 || invEnd < 0) throw new System.Exception("Missing verified diver inventory branch.");
var invBefore = diverSource.Substring(invStart, invEnd - invStart);
var invAfter = invBefore.Replace("var selOff = menuSel + listOffset;",
    "var _invStep = (global.language == global.LANG_JAPANESE) ? max(8, ceil(string_height(\"中\"))) : 8;\n" +
    "        var _invRows = max(1, floor(80 / _invStep));\n" +
    "        var _invFirst = (global.language == global.LANG_JAPANESE) ? clamp(((menuSel == 99) ? pageLen : menuSel) - _invRows + 1, 0, max(0, pageLen + 1 - _invRows)) : -listOffset;\n" +
    "        var selOff = menuSel - _invFirst;")
    .Replace("(8 * selOff)", "(_invStep * selOff)")
    .Replace("var lLength = min(pageLen + 1, 10);", "var lLength = min(pageLen + 1, _invRows);")
    .Replace("var ii = i - listOffset;", "var ii = i + _invFirst;")
    .Replace("(8 * i)", "(_invStep * i)")
    .Replace("if (listOffset < 0)", "if (_invFirst > 0)")
    .Replace("if (pageLen > (9 - listOffset))", "if (pageLen >= (_invFirst + _invRows))");
// 英文保持原版最多十行；中文固定窗口最多七行。
invAfter = invAfter.Replace("var _invRows = max(1, floor(80 / _invStep));", "var _invRows = (global.language == global.LANG_JAPANESE) ? max(1, floor(80 / _invStep)) : 10;");
invAfter = invAfter.Replace("var cursorY = yMenu + (8 * menuSel2);",
    "var cursorY = yMenu + (8 * menuSel2) + ((global.language == global.LANG_JAPANESE) ? ((_invStep - 8) * (menuSel2 % 2)) : 0);")
    .Replace("cursorY = yMenu + (8 * menuSel3);",
    "cursorY = yMenu + (8 * menuSel3) + ((global.language == global.LANG_JAPANESE) ? ((_invStep - 8) * (menuSel3 % 2)) : 0);")
    .Replace("var yOff = 8 * i;", "var yOff = (8 * i) + ((global.language == global.LANG_JAPANESE) ? ((_invStep - 8) * (i % 2)) : 0);");
FixLayout("gml_Object_o19_Mas_Draw_0", invBefore, invAfter);

// 深潜者：珍珠、宝箱和战斗奖励均已实机复现图标覆盖中文及可见的 %。
// % 是原版精灵字体的空白图标格，中文按实际前缀宽度定位，图标仍占 8px/格。
var diverRewards = new UndertaleModLib.Compiler.CodeImportGroup(Data);
diverRewards.AutoCreateAssets = true;
diverRewards.QueueReplace("gml_GlobalScript_UFO50_CHS_divers_icon_x", """
function UFO50_CHS_divers_icon_x(_x, _text)
{
    var _pos = string_pos("%", _text);
    if (global.language != global.LANG_JAPANESE) return _x + (8 * (_pos - 1));
    return _x + round(string_width(string_copy(_text, 1, max(0, _pos - 1))));
}
""");
diverRewards.QueueReplace("gml_GlobalScript_UFO50_CHS_draw_divers_reward", """
function UFO50_CHS_draw_divers_reward(_x, _y, _text, _slots)
{
    var _pos = string_pos("%", _text);
    if (global.language != global.LANG_JAPANESE || _pos == 0)
    {
        draw_text(_x, _y, _text);
        return;
    }
    var _iconX = UFO50_CHS_divers_icon_x(_x, _text);
    draw_text(_x, _y, string_copy(_text, 1, _pos - 1));
    var _tail = string_delete(_text, 1, _pos + _slots - 1);
    var _tailX = _iconX + (8 * _slots);
    var _digits = 0;
    while (_digits < string_length(_tail))
    {
        if (string_pos(string_char_at(_tail, _digits + 1), "0123456789") == 0) break;
        _digits++;
    }
    if (_digits > 0)
    {
        var _value = string_copy(_tail, 1, _digits);
        var _font = draw_get_font();
        var _numberFont = UFO50_CHS_number_font(_value);
        if (_numberFont >= 0) draw_set_font(_numberFont);
        var _numberWidth = string_width(_value);
        draw_text(_tailX, _y, _value);
        draw_set_font(_font);
        _tailX += _numberWidth;
        _tail = string_delete(_tail, 1, _digits);
    }
    draw_text(_tailX, _y, _tail);
}
""");
diverRewards.Import();
FixLayout("gml_Object_o19_Mas_Draw_0", "draw_text(xv + 16, yMenu + 8, textBox[0]);",
    "UFO50_CHS_draw_divers_reward(xv + 16, yMenu + 8, textBox[0], 2);");
FixLayout("gml_Object_o19_Mas_Draw_0", "var iconPos = xv + 16 + (8 * (string_pos(\"%\", textBox[0]) - 1));",
    "var iconPos = UFO50_CHS_divers_icon_x(xv + 16, textBox[0]);");
FixLayout("gml_Object_o19_Mas_Draw_0", "draw_text(xMenu + 16, yMenu + 16 + (8 * i), textBox[i]);",
    "UFO50_CHS_draw_divers_reward(xMenu + 16, yMenu + 16 + (8 * i), textBox[i], 1);");
FixLayout("gml_Object_o19_Mas_Draw_0", "var coinX = xMenu + 16 + (8 * (string_pos(\"%\", textBox[0]) - 1));",
    "var coinX = UFO50_CHS_divers_icon_x(xMenu + 16, textBox[0]);");
FixLayout("gml_Object_o19_Mas_Draw_0", "var relicX = xMenu + 16 + (8 * (string_pos(\"%\", textBox[0]) - 1));",
    "var relicX = UFO50_CHS_divers_icon_x(xMenu + 16, textBox[0]);");
FixLayout("gml_Object_o19_Mas_Draw_0", "draw_text(xv + 16, yBar + 16 + (16 * i), textBox[i]);",
    "UFO50_CHS_draw_divers_reward(xv + 16, yBar + 16 + (16 * i), textBox[i], 1);");
FixLayout("gml_Object_o19_Mas_Draw_0", "var coinX = xv + 16 + (8 * (string_pos(\"%\", textBox[0]) - 1));",
    "var coinX = UFO50_CHS_divers_icon_x(xv + 16, textBox[0]);");
FixLayout("gml_Object_o19_Mas_Draw_0", "var relicX = xv + 16 + (8 * (string_pos(\"%\", textBox[0]) - 1));",
    "var relicX = UFO50_CHS_divers_icon_x(xv + 16, textBox[(global.language == global.LANG_JAPANESE) ? 1 : 0]);");


// 诡石镇状态页/队伍页已复现：三行 vitals 改为实测行高，数值右对齐。
// 生命/法力分数移除展示空格，为四位数预留空间，数字沿用原版字体。
var grimVitals = new UndertaleModLib.Compiler.CodeImportGroup(Data);
grimVitals.AutoCreateAssets = true;
grimVitals.QueueReplace("gml_GlobalScript_UFO50_CHS_draw_grim_vitals", """
function UFO50_CHS_draw_grim_vitals(_x, _y, _player)
{
    var _step = max(8, ceil(string_height("中")));
    var _labels = [scrString("stat_lvl"), scrString("stat_hp"), scrString("stat_sp")];
    _labels[1] = string_replace(_labels[1], "值", "");
    var _values = [string(_player.level), string(_player.hp) + "/" + string(_player.hpMax), string(_player.sp) + "/" + string(_player.spMax)];
    for (var _row = 0; _row < 3; _row++)
    {
        scrSetFont(global.fontGrimstone);
        draw_set_halign(fa_left);
        draw_text(_x + 12, _y + (_row * _step), _labels[_row]);
        draw_set_font(global.fontGrimstone);
        draw_set_halign(fa_right);
        draw_text(_x + 120, _y + (_row * _step), _values[_row]);
    }
    draw_set_halign(fa_left);
    scrSetFont(global.fontGrimstone);
}
""");
grimVitals.Import();
FixLayout("gml_Object_o12__Game_Draw_0", "        tx = 152;\n        draw_text((viewx + tx) - 8, viewy + 32, scrString(\"stat_lvl\"));\n        draw_text(((viewx + tx) - 8) + 32, viewy + 32, string(party[currPlayer].level));\n        draw_text((viewx + tx) - 8, viewy + 40, scrString(\"stat_hp\"));\n        draw_text(((viewx + tx) - 8) + 32, viewy + 40, string(party[currPlayer].hp) + \" / \" + string(party[currPlayer].hpMax));\n        draw_text((viewx + tx) - 8, viewy + 48, scrString(\"stat_sp\"));\n        draw_text(((viewx + tx) - 8) + 32, viewy + 48, string(party[currPlayer].sp) + \" / \" + string(party[currPlayer].spMax));", "if (global.language == global.LANG_JAPANESE) { UFO50_CHS_draw_grim_vitals(viewx + 128, viewy + 28, party[currPlayer]); }\nelse {\n        tx = 152;\n        draw_text((viewx + tx) - 8, viewy + 32, scrString(\"stat_lvl\"));\n        draw_text(((viewx + tx) - 8) + 32, viewy + 32, string(party[currPlayer].level));\n        draw_text((viewx + tx) - 8, viewy + 40, scrString(\"stat_hp\"));\n        draw_text(((viewx + tx) - 8) + 32, viewy + 40, string(party[currPlayer].hp) + \" / \" + string(party[currPlayer].hpMax));\n        draw_text((viewx + tx) - 8, viewy + 48, scrString(\"stat_sp\"));\n        draw_text(((viewx + tx) - 8) + 32, viewy + 48, string(party[currPlayer].sp) + \" / \" + string(party[currPlayer].spMax));\n}");
FixLayout("gml_Object_o12__Game_Draw_0", "            draw_text((viewx + 32 + 120) - 8, viewy + 32 + (48 * i), scrString(\"stat_lvl\"));\n            draw_text(((viewx + 32 + 120) - 8) + 32, viewy + 32 + (48 * i), string(party[i].level));\n            draw_text((viewx + 32 + 120) - 8, viewy + 40 + (48 * i), scrString(\"stat_hp\"));\n            draw_text(((viewx + 32 + 120) - 8) + 32, viewy + 40 + (48 * i), string(party[i].hp) + \" / \" + string(party[i].hpMax));\n            draw_text((viewx + 32 + 120) - 8, viewy + 48 + (48 * i), scrString(\"stat_sp\"));\n            draw_text(((viewx + 32 + 120) - 8) + 32, viewy + 48 + (48 * i), string(party[i].sp) + \" / \" + string(party[i].spMax));", "if (global.language == global.LANG_JAPANESE) { UFO50_CHS_draw_grim_vitals(viewx + 128, viewy + 28 + (48 * i), party[i]); }\nelse {\n            draw_text((viewx + 32 + 120) - 8, viewy + 32 + (48 * i), scrString(\"stat_lvl\"));\n            draw_text(((viewx + 32 + 120) - 8) + 32, viewy + 32 + (48 * i), string(party[i].level));\n            draw_text((viewx + 32 + 120) - 8, viewy + 40 + (48 * i), scrString(\"stat_hp\"));\n            draw_text(((viewx + 32 + 120) - 8) + 32, viewy + 40 + (48 * i), string(party[i].hp) + \" / \" + string(party[i].hpMax));\n            draw_text((viewx + 32 + 120) - 8, viewy + 48 + (48 * i), scrString(\"stat_sp\"));\n            draw_text(((viewx + 32 + 120) - 8) + 32, viewy + 48 + (48 * i), string(party[i].sp) + \" / \" + string(party[i].spMax));\n}");


// 瓦尔布雷斯商店：独立计算名称/价格列，正文、列表、余额框连续布局。
var valbraceGroup = new UndertaleModLib.Compiler.CodeImportGroup(Data);
valbraceGroup.AutoCreateAssets = true;
valbraceGroup.QueueReplace("gml_GlobalScript_UFO50_CHS_draw_valbrace_shop", """
function UFO50_CHS_draw_valbrace_shop()
{
    if (instance_exists(oTextBox)) exit;
    var _xv = camera_get_view_x(view_get_camera(0));
    var _yv = camera_get_view_y(view_get_camera(0));
    var _items = shopWith.itemList;
    var _count = ds_list_size(_items);
    var _step = max(10, ceil(string_height("中"))) + 2;
    var _nameWidth = 0;
    var _priceWidth = 0;
    for (var _i = 0; _i < _count; _i++)
    {
        var _item = ds_list_find_value(_items, _i);
        var _traits = scr35_EquipmentTraits(_item);
        _nameWidth = max(_nameWidth, string_width(_traits[0]));
        if (!stealing) _priceWidth = max(_priceWidth, string_width(scrStringFormat(scrString("crone_trade_format"), "", scr35_ItemCost(_item))));
    }
    var _w = max(128, _nameWidth + _priceWidth + 56);
    var _dialog = scrStringSplit(stealing ? "crone_steal_dialog" : "crone_trade_dialog", 24, 3);
    var _dialogHeight = scrDrawTextBoxGetHeight(_dialog, 32, 12, 3);
    scrDrawMenuBorder(_xv + 72, _yv + 8, 240, _dialogHeight);
    for (var _i = 0; _i < 3; _i++) draw_text(_xv + 88, _yv + 20 + (12 * _i), _dialog[_i]);
    var _top = _yv + 8 + _dialogHeight + 4;
    scrDrawMenuBorder(_xv + 80, _top, _w, 20 + (_step * _count));
    for (var _i = 0; _i < _count; _i++)
    {
        var _item = ds_list_find_value(_items, _i);
        var _traits = scr35_EquipmentTraits(_item);
        var _rowY = _top + 10 + (_step * _i);
        draw_set_halign(fa_left);
        if (_i == inventorySelect) { draw_text(_xv + 96, _rowY, ">"); draw_set_color(global.palette[3]); }
        draw_text(_xv + 112, _rowY, _traits[0]);
        if (!stealing)
        {
            draw_set_halign(fa_right);
            draw_text(_xv + 80 + _w - 12, _rowY, scrStringFormat(scrString("crone_trade_format"), "", scr35_ItemCost(_item)));
        }
        draw_set_color(global.palette[0]);
    }
    draw_set_halign(fa_left);
    var _balance = scrStringFormat(scrString("item_gem_plural"), gemCount);
    var _balanceY = _top + 20 + (_step * _count);
    scrDrawMenuBorder(_xv + 88, _balanceY, string_width(_balance) + 16, 24);
    if (gemCount < scr35_ItemCost(ds_list_find_value(_items, inventorySelect))) draw_set_color(global.palette[9]);
    draw_text(_xv + 96, _balanceY + 8, _balance);
    draw_set_color(global.palette[0]);
}
""");
valbraceGroup.Import();
// 后期商店已复现 NOSTRING：原代码与资源的锁子甲键拼写不同。
FixLayout("gml_GlobalScript_scr35_EquipmentTraits", "scrString(\"armor_chain_mail\")",
    "scrString((global.language == global.LANG_JAPANESE) ? \"armor_chainmail\" : \"armor_chain_mail\")");
FixLayout("gml_GlobalScript_scr35_DrawShopUI", "function scr35_DrawShopUI()\n{",
    "function scr35_DrawShopUI()\n{\n    if (global.language == global.LANG_JAPANESE) { UFO50_CHS_draw_valbrace_shop(); exit; }");

// 诡石镇战斗姓名避免被面板上边框盖住；H/S 与数值沿用原版小字体。
FixLayout("gml_Object_o12__Game_Draw_0", "                draw_text(tx + 8, ty, party[j].name);\n                draw_text(tx + 12, (ty - 4) + 16, scrString(\"stat_hp_short\") + \" \" + string(party[j].hp));\n                draw_text(tx + 12, (ty - 4) + 16 + 8, scrString(\"stat_sp_short\") + \" \" + string(party[j].sp));", "if (global.language == global.LANG_JAPANESE)\n                {\n                    draw_text(tx + 8, ty + 4, party[j].name);\n                    draw_set_font(global.fontGrimstone);\n                    draw_text(tx + 12, ty + 16, scrString(\"stat_hp_short\") + \" \" + string(party[j].hp));\n                    draw_text(tx + 12, ty + 24, scrString(\"stat_sp_short\") + \" \" + string(party[j].sp));\n                    scrSetFont(global.fontGrimstone);\n                }\n                else {\n                draw_text(tx + 8, ty, party[j].name);\n                draw_text(tx + 12, (ty - 4) + 16, scrString(\"stat_hp_short\") + \" \" + string(party[j].hp));\n                draw_text(tx + 12, (ty - 4) + 16 + 8, scrString(\"stat_sp_short\") + \" \" + string(party[j].sp));\n}");

var chsDrawText = Data.Code.ByName("gml_GlobalScript_UFO50_CHS_draw_text");
var chsDrawTextExt = Data.Code.ByName("gml_GlobalScript_UFO50_CHS_draw_text_ext");
var chsDrawTextColor = Data.Code.ByName("gml_GlobalScript_UFO50_CHS_draw_text_color");
var chsAvianosMixed = Data.Code.ByName("gml_GlobalScript_UFO50_CHS_draw_avianos_mixed");
if (chsDrawText == null || chsDrawTextExt == null || chsDrawTextColor == null || chsAvianosMixed == null)
    throw new System.Exception("Failed to create CHS baseline wrapper code entries.");

var chsMaskedCounter = Data.Code.ByName("gml_GlobalScript_UFO50_CHS_draw_masked_counter");
if (chsMaskedCounter == null) throw new System.Exception("Missing original-font masked counter.");
var wrapperCodes = new HashSet<UndertaleCode>() { chsDrawText, chsDrawTextExt, chsDrawTextColor, chsAvianosMixed, chsMaskedCounter };
var redirectedCalls = new Dictionary<string, int>()
{
    { "draw_text", 0 },
    { "draw_text_ext", 0 },
    { "draw_text_color", 0 }
};
var baselineImportGroup = new UndertaleModLib.Compiler.CodeImportGroup(Data);
baselineImportGroup.ThrowOnNoOpFindReplace = true;
foreach (var code in Data.Code.ToList())
{
    if (code == null || code.Offset != 0 || wrapperCodes.Contains(code))
        continue;
    var calledFunctions = new HashSet<string>(
        code.Instructions
            .Where(instruction => instruction.Kind == UndertaleInstruction.Opcode.Call && instruction.ValueFunction?.Name?.Content != null)
            .Select(instruction => instruction.ValueFunction.Name.Content)
    );
    if (!calledFunctions.Contains("draw_text") && !calledFunctions.Contains("draw_text_ext") && !calledFunctions.Contains("draw_text_color"))
        continue;

    foreach (var functionName in new[] { "draw_text_color", "draw_text_ext", "draw_text" })
    {
        if (!calledFunctions.Contains(functionName))
            continue;
        var pattern = @"(?<![A-Za-z0-9_])" + functionName + @"\s*\(";
        baselineImportGroup.QueueRegexFindReplace(code, pattern, "UFO50_CHS_" + functionName + "(", true);
        redirectedCalls[functionName] += code.Instructions.Count(instruction =>
            instruction.Kind == UndertaleInstruction.Opcode.Call && instruction.ValueFunction?.Name?.Content == functionName);
    }
}
if ((redirectedCalls["draw_text"] + avianosDrawTextCalls) < 1500 || redirectedCalls["draw_text_ext"] < 60 || redirectedCalls["draw_text_color"] < 15)
    throw new System.Exception($"Unexpected text-call coverage: draw_text={redirectedCalls["draw_text"]}, avianos_mixed={avianosDrawTextCalls}, draw_text_ext={redirectedCalls["draw_text_ext"]}, draw_text_color={redirectedCalls["draw_text_color"]}");
baselineImportGroup.Import();

// 覆盖门禁：新增数字或动态字符串绘制点也必须进入统一字体路由。
var bypassedDraws = Data.Code.Where(code => code.Offset == 0 && !wrapperCodes.Contains(code))
    .Where(code => code.Instructions.Any(instruction => instruction.Kind == UndertaleInstruction.Opcode.Call &&
        redirectedCalls.ContainsKey(instruction.ValueFunction?.Name?.Content ?? "")))
    .Select(code => code.Name.Content).ToList();
if (bypassedDraws.Count > 0)
    throw new System.Exception("Text drawing bypasses CHS routing: " + string.Join(", ", bypassedDraws));

foreach (var str in Data.Strings.Where(str => str.Content == "にほんご"))
    str.Content = "中文";
ScriptMessage($"UFO 50 CHS patch applied; baseline wrappers redirected draw_text={redirectedCalls["draw_text"]}, avianos_mixed={avianosDrawTextCalls}, draw_text_ext={redirectedCalls["draw_text_ext"]}, draw_text_color={redirectedCalls["draw_text_color"]}.");
