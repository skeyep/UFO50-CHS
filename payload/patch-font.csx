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
        if (global.language == global.LANG_JAPANESE && font_exists(global.fontDefault_CHS) && !_spriteDigits && _font != global.fontAlien)
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
                if (global.language == global.LANG_JAPANESE && ord(char) > 127)
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
            if (global.language == global.LANG_JAPANESE && ord(char) > 127)
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
        if (global.language == global.LANG_JAPANESE && global.currFont == global.fontDefault_CHS && ord(cc) > 127)
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
                        _tokenWidth += (ord(_glyph) > 127) ? string_width(_glyph) : 8;
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
// Sprite fonts with a shadow/outline lose that layer when routed to Zpix.
// Keep the request paired with the active font; bare fonts and sprite digits
// retain their original paths. Solid-cell backgrounds remain caller-owned.
importGroup.QueueReplace("gml_GlobalScript_UFO50_CHS_outline_enabled", @"
function UFO50_CHS_outline_enabled()
{
    if (global.language != global.LANG_JAPANESE || draw_get_font() != global.fontDefault_CHS)
        return false;
    var _requested = global.chsRequestedFont;
    return _requested == global.fontDefault || _requested == global.fontDefault_JP
        || _requested == global.fontThinOutline || _requested == global.fontTall
        || _requested == global.fontTallBG;
}
");
importGroup.QueueReplace("gml_GlobalScript_UFO50_CHS_draw_text", @"
function UFO50_CHS_draw_text(arg0, arg1, arg2)
{
    var _font = draw_get_font();
    var _numberFont = UFO50_CHS_number_font(arg2);
    if (_numberFont >= 0) draw_set_font(_numberFont);
    var _spriteDigits = (_font == global.fontDigital || _font == global.fontDigitalMini || _font == global.fontDigitalBig || _font == global.fontDigital2);
    if (global.language == global.LANG_JAPANESE && _font != global.fontAlien && !_spriteDigits && _numberFont < 0)
        arg1 -= 1;
    if (_numberFont < 0 && UFO50_CHS_outline_enabled() && draw_get_color() != c_black)
    {
        var _faceColor = draw_get_color();
        draw_set_color(c_black);
        for (var _dx = -1; _dx <= 1; _dx++)
            for (var _dy = -1; _dy <= 1; _dy++)
                if (_dx != 0 || _dy != 0) draw_text(arg0 + _dx, arg1 + _dy, arg2);
        draw_set_color(_faceColor);
    }
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
    if (global.language == global.LANG_JAPANESE && _font != global.fontAlien && !_spriteDigits && _numberFont < 0)
    {
        arg1 -= 1;
        arg2 = UFO50_CHS_wrap_text(arg2, arg4);
        var _lineStep = max(8, ceil(string_height(""中"")));
        if (arg3 > 0 && arg3 < _lineStep)
            arg3 = _lineStep;
    }
    if (_numberFont < 0 && UFO50_CHS_outline_enabled() && draw_get_color() != c_black)
    {
        var _faceColor = draw_get_color();
        draw_set_color(c_black);
        for (var _dx = -1; _dx <= 1; _dx++)
            for (var _dy = -1; _dy <= 1; _dy++)
                if (_dx != 0 || _dy != 0) draw_text_ext(arg0 + _dx, arg1 + _dy, arg2, arg3, arg4);
        draw_set_color(_faceColor);
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
    if (global.language == global.LANG_JAPANESE && _font != global.fontAlien && !_spriteDigits && _numberFont < 0)
        arg1 -= 1;
    if (_numberFont < 0 && UFO50_CHS_outline_enabled()
        && (arg3 != c_black || arg4 != c_black || arg5 != c_black || arg6 != c_black))
    {
        for (var _dx = -1; _dx <= 1; _dx++)
            for (var _dy = -1; _dy <= 1; _dy++)
                if (_dx != 0 || _dy != 0)
                    draw_text_color(arg0 + _dx, arg1 + _dy, arg2, c_black, c_black, c_black, c_black, arg7);
    }
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

// Elfazar card reward: retain the six-step reveal/retract animation, with
// measured CJK advances. The original 8px path remains for other languages.
FixLayout("gml_Object_o31_CardFx_Draw_0",
    "draw_text((x - 24) + (8 * i), y, c);",
    "if (global.language == global.LANG_JAPANESE)\n        {\n            var _cardText = string_copy(text, 1, 6);\n            var _cardLeft = x - string_width(_cardText) * 0.5;\n            UFO50_CHS_draw_card_text(_cardLeft + string_width(string_copy(_cardText, 1, i)), y, c);\n        }\n        else draw_text((x - 24) + (8 * i), y, c);");
// 原版精灵字体自带黑边；Zpix 卡牌提示补回 1px 黑边和亮色字面。
var cardContrastGroup = new UndertaleModLib.Compiler.CodeImportGroup(Data);
cardContrastGroup.AutoCreateAssets = true;
cardContrastGroup.QueueReplace("gml_GlobalScript_UFO50_CHS_draw_card_text", """
function UFO50_CHS_draw_card_text(_x, _y, _text)
{
    _y -= 1;
    var _oldColor = draw_get_color();
    var _oldAlpha = draw_get_alpha();
    draw_set_alpha(1);
    draw_set_color(c_black);
    for (var _dx = -1; _dx <= 1; _dx++)
        for (var _dy = -1; _dy <= 1; _dy++)
            if (_dx != 0 || _dy != 0) draw_text(_x + _dx, _y + _dy, _text);
    draw_set_color(c_white);
    draw_text(_x, _y, _text);
    draw_set_color(_oldColor);
    draw_set_alpha(_oldAlpha);
}
""");
cardContrastGroup.Import();

// Campanella 2 NPC dialogue: the original physical text widths are 192px
// (shop) and 160px (side). Center the visible substring by its real width.
FixLayout("gml_Object_o38_NPC_Draw_0",
    "var _x = (camera_get_view_x(view_get_camera(0)) + 192) - (floor(chrNum[ii] * 0.5) * 8);",
    "var _x = (camera_get_view_x(view_get_camera(0)) + 192) - ((global.language == global.LANG_JAPANESE) ? string_width(stPart[ii]) * 0.5 : floor(chrNum[ii] * 0.5) * 8);");
FixLayout("gml_Object_o38_NPC_Draw_0", "var _y = y + 88 + (8 * i);",
    "var _npcLineStep = (global.language == global.LANG_JAPANESE) ? max(8, ceil(string_height(\"中\"))) : 8;\n            var _y = y + 88 + (_npcLineStep * i);");
FixLayout("gml_Object_o38_NPC_Draw_0", "var _x = (x + 144) - (floor(chrNum[i] * 0.5) * 8);",
    "var _x = (x + 144) - ((global.language == global.LANG_JAPANESE) ? string_width(stPart[i]) * 0.5 : floor(chrNum[i] * 0.5) * 8);");
FixLayout("gml_Object_o38_NPC_Draw_0", "var _y = y + 72 + (8 * i);",
    "var _npcLineStep = (global.language == global.LANG_JAPANESE) ? max(8, ceil(string_height(\"中\"))) : 8;\n        var _y = y + 72 + (_npcLineStep * i);");


// Preserve the original black-backed HUD labels, measured in the active font.
FixLayout("gml_GlobalScript_scr04_DrawText",
    "draw_rectangle(arg0, arg1, (arg0 + (string_length(_str) * 8)) - 1, arg1 + 7, false);",
    "var _chsLabel = global.language == global.LANG_JAPANESE && draw_get_font() == global.fontDefault_CHS && UFO50_CHS_number_font(_str) < 0;\n    var _boxWidth = _chsLabel ? string_width(_str) : string_length(_str) * 8;\n    var _boxHeight = _chsLabel ? string_height(_str) : 8;\n    var _boxTop = arg1 - (_chsLabel ? 1 : 0);\n    draw_rectangle(arg0, _boxTop, arg0 + _boxWidth - 1, _boxTop + _boxHeight - 1, false);");
FixLayout("gml_Object_o04_Game_Draw_0",
    "var centerX = 100 + ((152 - (8 * string_length(levelName[levelID]))) / 2);",
    "var _nameWidth = (global.language == global.LANG_JAPANESE) ? string_width(levelName[levelID]) : 8 * string_length(levelName[levelID]);\n            var centerX = 100 + floor((152 - _nameWidth) / 2);");

var camouflageLabelGroup = new UndertaleModLib.Compiler.CodeImportGroup(Data);
camouflageLabelGroup.AutoCreateAssets = true;
camouflageLabelGroup.QueueReplace("gml_GlobalScript_UFO50_CHS_draw_camouflage_level", """
function UFO50_CHS_draw_camouflage_level(_x, _y, _level)
{
    var _label = scrString("level") + " ";
    var _number = string(_level);
    if (global.language != global.LANG_JAPANESE)
    {
        scr04_DrawText(_x, _y, _label + _number);
        return;
    }
    var _font = draw_get_font();
    var _labelWidth = string_width(_label);
    var _labelHeight = string_height(_label);
    var _numberFont = UFO50_CHS_number_font(_number);
    if (_numberFont >= 0) draw_set_font(_numberFont);
    var _numberWidth = string_width(_number);
    draw_set_font(_font);
    var _color = draw_get_color();
    draw_set_color(c_black);
    draw_rectangle(_x, _y - 1, _x + _labelWidth + _numberWidth - 1, _y + _labelHeight - 2, false);
    draw_set_color(_color);
    UFO50_CHS_draw_labeled_number(_x, _y, _label, _number, fa_left);
}
""");
camouflageLabelGroup.Import();
FixLayout("gml_Object_o04_Game_Draw_0", "scr04_DrawText(16, 200, scrString(\"level\") + \" \" + string(levelID));", "UFO50_CHS_draw_camouflage_level(16, 200, levelID);");
FixLayout("gml_Object_o04_Game_Draw_0", "scr04_DrawText(16, 200, scrString(\"level\") + \" \" + string(lvl));", "UFO50_CHS_draw_camouflage_level(16, 200, lvl);");

// Same-scene Overbold library CHS/EN/JA confirms year and record digits use Zpix.
// Limit mixed routing to fixed library year/record fields.
var metaNumberImports = new UndertaleModLib.Compiler.CodeImportGroup(Data);
metaNumberImports.QueueReplace("gml_GlobalScript_UFO50_CHS_fixed_mixed_layout", """
function UFO50_CHS_fixed_mixed_layout(_text)
{
    var _font = draw_get_font();
    var _parts = [];
    var _types = [];
    var _widths = [];
    var _total = 0;
    var _run = "";
    var _numeric = false;
    for (var _i = 1; _i <= string_length(_text) + 1; _i++)
    {
        var _c = _i <= string_length(_text) ? string_char_at(_text, _i) : "";
        var _isNumeric = _c != "" && string_pos(_c, "0123456789:+-/.,%$()MPXx×^") > 0;
        if (_run != "" && (_isNumeric != _numeric || _c == ""))
        {
            var _n = array_length(_parts);
            _parts[_n] = _run;
            draw_set_font(_font);
            var _runFont = _numeric ? UFO50_CHS_number_font(_run) : -1;
            _types[_n] = _runFont >= 0;
            if (_runFont >= 0) draw_set_font(_runFont);
            _widths[_n] = string_width(_run);
            _total += _widths[_n];
            _run = "";
        }
        _numeric = _isNumeric;
        _run += _c;
    }
    draw_set_font(_font);
    return {parts:_parts, numeric:_types, widths:_widths, total:_total};
}
""");
metaNumberImports.QueueReplace("gml_GlobalScript_UFO50_CHS_fixed_mixed_width", """
function UFO50_CHS_fixed_mixed_width(_text)
{
    var _layout = UFO50_CHS_fixed_mixed_layout(_text);
    return _layout.total;
}
""");
metaNumberImports.QueueReplace("gml_GlobalScript_UFO50_CHS_draw_fixed_mixed", """
function UFO50_CHS_draw_fixed_mixed(_x, _y, _text, _labelDelta)
{
    var _font = draw_get_font();
    var _halign = draw_get_halign();
    var _labelOffset = is_undefined(_labelDelta) ? 0 : _labelDelta;
    var _layout = UFO50_CHS_fixed_mixed_layout(_text);
    var _parts = _layout.parts;
    var _types = _layout.numeric;
    var _widths = _layout.widths;
    var _total = _layout.total;
    if (_halign == fa_center) _x -= floor(_total / 2);
    else if (_halign == fa_right) _x -= _total;
    draw_set_halign(fa_left);
    for (var _j = 0; _j < array_length(_parts); _j++)
    {
        // The common wrapper routes numeric-only spans to the caller's sprite font.
        draw_text(_x, _y + (_types[_j] ? 0 : _labelOffset), _parts[_j]);
        _x += _widths[_j];
    }
    draw_set_font(_font);
    draw_set_halign(_halign);
}
""");
metaNumberImports.Import();
FixLayout("gml_Object_oLibrary_Draw_0", "draw_text(192, _TEXT_Y, _year + \"年\");",
    "UFO50_CHS_draw_fixed_mixed(192, _TEXT_Y, _year + \"年\", -1);");
FixLayout("gml_Object_oLibrary_Draw_0", "draw_text(248, _TEXT_Y, strProgress[global.selGame]);",
    "if (global.language == global.LANG_JAPANESE) UFO50_CHS_draw_fixed_mixed(248, _TEXT_Y, strProgress[global.selGame], -1);\n                else draw_text(248, _TEXT_Y, strProgress[global.selGame]);");
FixLayout("gml_Object_oLibrary_Draw_0", "draw_text(_nameX, _TEXT_Y, _gameName);",
    "draw_text(_nameX, _TEXT_Y - 1, _gameName);");

// 私有提案：合入根Agent候选的 FixLayout 段；尚待UTMT编译/同场景验证。
// 位于原版调用重定向之前。只改中文（日语槽）。
FixLayout("gml_Object_o03_LevelEnd_Draw_0",
    "draw_text_bg_centered(xv + 192, yv + 48, \" \" + string_even(stageString, 2) + \" \", 0, 8, 8, false);",
    "draw_text_bg_centered(xv + 192, yv + ((global.language == global.LANG_JAPANESE) ? 44 : 48), \" \" + string_even(stageString, 2) + \" \", 0, 8, ((global.language == global.LANG_JAPANESE) ? max(8, string_height(\"中\")) : 8), false);");
FixLayout("gml_Object_o03_LevelEnd_Draw_0",
    "var clearLength = string_length(scrStringEven(\"level_end_clear\", 2));",
    "var clearLength = string_length(scrStringEven(\"level_end_clear\", 2));\n    var clearHalfWidth = (global.language == global.LANG_JAPANESE) ? ceil(string_width(scrStringEven(\"level_end_clear\", 2)) / 2) : 4 * clearLength;");
FixLayout("gml_Object_o03_LevelEnd_Draw_0",
    "(xv + 192) - (4 * clearLength) - 16",
    "(xv + 192) - clearHalfWidth - 16");
FixLayout("gml_Object_o03_LevelEnd_Draw_0",
    "xv + 192 + (4 * clearLength) + 8",
    "xv + 192 + clearHalfWidth + 8");

var ufo3DesignHelpers = new UndertaleModLib.Compiler.CodeImportGroup(Data);
ufo3DesignHelpers.AutoCreateAssets = true;
ufo3DesignHelpers.QueueReplace("gml_GlobalScript_UFO50_CHS_draw_ufo3_title", """
function UFO50_CHS_draw_ufo3_title(_label, _name)
{
    // 原HUD左侧黑区；两行均为12px，姓名按实际字宽居中，装饰跟随姓名边缘。
    var _nameY = 20;
    UFO50_CHS_draw_text(floor(64 - string_width(_label) / 2), 8, _label);
    scrSetFont(global.fontTall);
    var _nameWidth = string_width(_name);
    var _nameX = floor(64 - _nameWidth / 2);
    UFO50_CHS_draw_text(_nameX, _nameY, _name);
    scrSetFont(global.fontDefault);
    var _starWidth = string_width("*");
    UFO50_CHS_draw_text(_nameX - _starWidth - 4, _nameY, "*");
    UFO50_CHS_draw_text(_nameX + _nameWidth + 4, _nameY, "*");
}
""");
ufo3DesignHelpers.QueueReplace("gml_GlobalScript_UFO50_CHS_ufo3_dialogue_layout", """
function UFO50_CHS_ufo3_dialogue_layout(_dialogue, _last)
{
    // 只读原Step已经揭示的字符串，不改lineDialogue/stepDialogue/计时或终止状态。
    var _text = "";
    for (var _i = 0; _i <= _last; _i++)
    {
        if (_i > 0) _text += global.CARRIAGE_RETURN_SIMPLIFIED;
        _text += _dialogue[_i];
    }
    var _lines = UFO50_CHS_wrap_text_array(_text, 96, 0);
    // wrap helper省略尾部空行；原Step进入下一源行时保留当前空光标行。
    if (_last > 0 && _dialogue[_last] == "") _lines[array_length(_lines)] = "";
    var _lastLine = array_length(_lines) - 1;
    // 光标是8px原精灵；满行时先进入下一空行，保证光标也在96px区域内。
    if (string_width(_lines[_lastLine]) + 8 > 96)
    {
        _lines[array_length(_lines)] = "";
        _lastLine++;
    }
    var _start = max(0, array_length(_lines) - 2);
    return {lines:_lines, first:_start, cursorX:16 + string_width(_lines[_lastLine]), cursorY:8 + ((_lastLine - _start) * 12)};
}
""");
ufo3DesignHelpers.QueueReplace("gml_GlobalScript_UFO50_CHS_draw_ufo3_dialogue", """
function UFO50_CHS_draw_ufo3_dialogue(_dialogue, _last, _blink)
{
    var _layout = UFO50_CHS_ufo3_dialogue_layout(_dialogue, _last);
    for (var _i = _layout.first; _i < array_length(_layout.lines); _i++)
        UFO50_CHS_draw_text(16, 8 + ((_i - _layout.first) * 12), _layout.lines[_i]);
    if (_blink) draw_sprite(s08_HUD_8x8, 3, _layout.cursorX, _layout.cursorY);
}
""");
ufo3DesignHelpers.Import();

FixLayout("gml_Object_o08_Mas_Draw_0", """
        draw_text(16, 16, stageText[global.gv08_stage][0]);
        draw_text(24, 24, "*");
        draw_text(96, 24, "*");
        scrSetFont(global.fontTall);
        var _str = stageText[global.gv08_stage][1];
        draw_text(floor8(64 - (string_length(_str) * 4)), 24, _str);
        scrSetFont(global.fontDefault);
""", """
        if (global.language == global.LANG_JAPANESE)
            UFO50_CHS_draw_ufo3_title(stageText[global.gv08_stage][0], stageText[global.gv08_stage][1]);
        else
        {
            draw_text(16, 16, stageText[global.gv08_stage][0]);
            draw_text(24, 24, "*");
            draw_text(96, 24, "*");
            scrSetFont(global.fontTall);
            var _str = stageText[global.gv08_stage][1];
            draw_text(floor8(64 - (string_length(_str) * 4)), 24, _str);
            scrSetFont(global.fontDefault);
        }
""");

// PORGY中文两行博士弹窗：正文顶基线188/200，25px黑底覆盖187..211，距屏底4px。
FixLayout("gml_Object_o10_Game_Draw_73",
    "var _dialog_y = _uiy + 196;",
    "var _chsDialog = global.language == global.LANG_JAPANESE;\nvar _dialog_y = _uiy + (_chsDialog ? 188 : 196);");
FixLayout("gml_Object_o10_Game_Draw_73",
    "draw_rectangle(_dialog_x, _dialog_y, _dialog_x + 255, _dialog_y + 15, 0);",
    "draw_rectangle(_dialog_x, _dialog_y - (_chsDialog ? 1 : 0), _dialog_x + 255, _dialog_y + (_chsDialog ? 23 : 15), 0);");
FixLayout("gml_Object_o10_Game_Draw_73",
    "draw_sprite(s10_DrFaces, dialog_face, _dialog_x, _dialog_y);",
    "draw_sprite(s10_DrFaces, dialog_face, _dialog_x, _dialog_y + (_chsDialog ? 4 : 0));");
FixLayout("gml_Object_o10_Game_Draw_73",
    "draw_text(_dialog_x + 16, _dialog_y + 8, string_copy(dialog_curr[1], 1, dialog_time - 30));",
    "draw_text(_dialog_x + 16, _dialog_y + (_chsDialog ? 12 : 8), string_copy(dialog_curr[1], 1, dialog_time - 30));");
FixLayout("gml_Object_o08_Mas_Draw_0", """
    draw_text(16, 16, scrString("guardian"));
    draw_text(24, 24, "*");
    draw_text(96, 24, "*");
    scrSetFont(global.fontTall);
    var _str = stageText[global.gv08_stage][2];
    draw_text(floor8(64 - (string_length(_str) * 4)), 24, _str);
    scrSetFont(global.fontDefault);
""", """
    if (global.language == global.LANG_JAPANESE)
        UFO50_CHS_draw_ufo3_title(scrString("guardian"), stageText[global.gv08_stage][2]);
    else
    {
        draw_text(16, 16, scrString("guardian"));
        draw_text(24, 24, "*");
        draw_text(96, 24, "*");
        scrSetFont(global.fontTall);
        var _str = stageText[global.gv08_stage][2];
        draw_text(floor8(64 - (string_length(_str) * 4)), 24, _str);
        scrSetFont(global.fontDefault);
    }
""");
FixLayout("gml_Object_o08_Mas_Draw_0", """
    for (var i = 0; i < (lineDialogue + 1); i++)
    {
        draw_text(16, 8 + (8 * i), strDialogue[i]);
    }
    if (stepDialogue < 12)
    {
        var _x = 16 + (8 * stepDialogue);
        var _y = 8 + (8 * lineDialogue);
        if ((t % 6) < 3)
        {
            draw_sprite(s08_HUD_8x8, 3, _x, _y);
        }
    }
""", """
    if (global.language == global.LANG_JAPANESE)
        UFO50_CHS_draw_ufo3_dialogue(strDialogue, lineDialogue, (t % 6) < 3);
    else
    {
        for (var i = 0; i < (lineDialogue + 1); i++)
        {
            draw_text(16, 8 + (8 * i), strDialogue[i]);
        }
        if (stepDialogue < 12)
        {
            var _x = 16 + (8 * stepDialogue);
            var _y = 8 + (8 * lineDialogue);
            if ((t % 6) < 3) draw_sprite(s08_HUD_8x8, 3, _x, _y);
        }
    }
""");

// DEVILITION实际放置/胜利画面确认11px中文重叠；每个组内保持12px，原英文8px。
FixLayout("gml_Object_o05_Game_Draw_0", "scrDrawTextInput(56, 112, scrStringVal(\"hold_x_to_cancel_2\", \"[1\"), 0, 0, true);",
    "scrDrawTextInput(56, (global.language == global.LANG_JAPANESE) ? 116 : 112, scrStringVal(\"hold_x_to_cancel_2\", \"[1\"), 0, 0, true);");
FixLayout("gml_Object_o05_Game_Draw_0", "scrDrawTextInput(56, 136, scrStringVal(\"hold_x_to_cancel_4\", \"[1\"), 0, 0, true);",
    "scrDrawTextInput(56, (global.language == global.LANG_JAPANESE) ? 140 : 136, scrStringVal(\"hold_x_to_cancel_4\", \"[1\"), 0, 0, true);");
FixLayout("gml_Object_o05_Game_Draw_0", "scrStringDrawCenterAltExt(56, 72, \"leftover_bonus_2\", 0, 12, 4);",
    "scrStringDrawCenterAltExt(56, (global.language == global.LANG_JAPANESE) ? 76 : 72, \"leftover_bonus_2\", 0, 12, 4);");

// VAINGER地图符号使用原fontNoShadow；helper内raw draw_text保持原8px基线与无描边。
var vaingerSymbolHelpers = new UndertaleModLib.Compiler.CodeImportGroup(Data);
vaingerSymbolHelpers.AutoCreateAssets = true;
vaingerSymbolHelpers.QueueReplace("gml_GlobalScript_UFO50_CHS_draw_vainger_map_symbol", """
function UFO50_CHS_draw_vainger_map_symbol(_x, _y, _key)
{
    var _font = draw_get_font();
    if (global.language == global.LANG_JAPANESE)
    {
        var _originalFont = global.chsRequestedFont;
        if (!font_exists(_originalFont) || _originalFont == global.fontDefault_CHS) _originalFont = global.fontNoShadow;
        draw_set_font(_originalFont);
        var _symbol = (_key == "ui_save_pod") ? "S" : ((_key == "ui_teleporter") ? "T" : "E");
        draw_text(_x, _y, _symbol);
        draw_set_font(_font);
    }
    else draw_text(_x, _y, scrString(_key));
}
""");
vaingerSymbolHelpers.Import();
foreach (var key in new[] {"ui_save_pod", "ui_teleporter", "ui_exit"})
    FixLayout("gml_GlobalScript_scr07_DrawMapUI",
        "draw_text((_xx * 16) + arg0 + 188, (_yy * 8) + arg1 + 44, scrString(\"" + key + "\"));",
        "UFO50_CHS_draw_vainger_map_symbol((_xx * 16) + arg0 + 188, (_yy * 8) + arg1 + 44, \"" + key + "\");");

// STUDY成功反馈本地化实际习得技能内部码。
FixLayout("gml_GlobalScript_scr12_BattlePartyUseSkill",
    "scrStringVal(\"battle_study_learns\", skillTemp)",
    "scrStringVal(\"battle_study_learns\", scr12_SkillGetName(skillTemp))");

FixLayout("gml_GlobalScript_scr07_DrawMapUI",
    "draw_text((arg0 + 144) - 104, arg1 + 16, scrString(\"ui_level\") + string(clearanceLevel));",
    "if (global.language == global.LANG_JAPANESE) UFO50_CHS_draw_labeled_number((arg0 + 144) - 104, arg1 + 16, scrString(\"ui_level\"), string(clearanceLevel), fa_left);\n    else draw_text((arg0 + 144) - 104, arg1 + 16, scrString(\"ui_level\") + string(clearanceLevel));");

// 原英日装备界面resume的L0沿fontTall；已看第三中文resume仍整串Zpix。
FixLayout("gml_GlobalScript_scr07_DrawModUI",
    "draw_text((arg0 + 351) - 104, arg1 + 16, scrString(\"ui_level\") + string(clearanceLevel));",
    "if (global.language == global.LANG_JAPANESE) UFO50_CHS_draw_labeled_number((arg0 + 351) - 104, arg1 + 16, scrString(\"ui_level\"), string(clearanceLevel), fa_left);\n    else draw_text((arg0 + 351) - 104, arg1 + 16, scrString(\"ui_level\") + string(clearanceLevel));");

// 羽毛真实背包确认已复现内部码FEATHER；同事件两处仅本地化物品名参数。
FixLayout("gml_Object_o12__Game_Other_14", "scrStringVal(\"use_item_check\", currItem)",
    "scrStringVal(\"use_item_check\", scr12_ItemGetName(currItem))");

// 已看fourth-before原50%隐藏留言叠字。中文在原96px HUD内按真实宽度排两行。
FixLayout("gml_Object_o08_Mas_Draw_0", """
        var _str = string_line_breaks(scrStringManual("game_meta_message_8", 0), 12, 4);
        draw_text(16, 8, _str[0]);
        draw_text(16, 16, _str[1]);
        draw_text(16, 24, _str[2]);
        draw_text(16, 32, _str[3]);
""", """
        if (global.language == global.LANG_JAPANESE)
        {
            var _metaLines = UFO50_CHS_wrap_text_array(scrStringManual("game_meta_message_8", 0), 96, 0);
            for (var _i = 0; _i < array_length(_metaLines); _i++)
                UFO50_CHS_draw_text(16, 8 + 12 * _i, _metaLines[_i]);
        }
        else
        {
            var _str = string_line_breaks(scrStringManual("game_meta_message_8", 0), 12, 4);
            draw_text(16, 8, _str[0]);
            draw_text(16, 16, _str[1]);
            draw_text(16, 24, _str[2]);
            draw_text(16, 32, _str[3]);
        }
""");

// 第六三语商店实图及数字特写确认：持有X0整串Zpix，与相邻原8px数字不同。
FixLayout("gml_Object_o12__Game_Draw_0",
    "draw_text(viewx + 280, viewy + 64 + 24 + 64, scrString(\"shop_owned\") + string(n));",
    "if (global.language == global.LANG_JAPANESE) UFO50_CHS_draw_labeled_number(viewx + 280, viewy + 64 + 24 + 64, scrString(\"shop_owned\"), string(n), fa_left);\n                else draw_text(viewx + 280, viewy + 64 + 24 + 64, scrString(\"shop_owned\") + string(n));");

// 已看ID1英日/旧中文玩家1/2：整串Zpix数字与原fontTall字高、字宽不同。
// 原边框已验完整，保留；只拆数字并将两种字高围绕原数字块中线排齐。
var mortolPlayerHelpers = new UndertaleModLib.Compiler.CodeImportGroup(Data);
mortolPlayerHelpers.AutoCreateAssets = true;
mortolPlayerHelpers.QueueReplace("gml_GlobalScript_UFO50_CHS_draw_mortol_player", """
function UFO50_CHS_draw_mortol_player(_centerX, _topY, _number)
{
    var _font = draw_get_font();
    var _numberFont = UFO50_CHS_number_font(_number);
    var _label = scrStringVal("player_x", "");
    var _labelWidth = string_width(_label);
    draw_set_font(_numberFont);
    var _numberWidth = string_width(_number);
    var _numberHeight = string_height(_number);
    draw_set_font(_font);
    var _ha = draw_get_halign(); var _va = draw_get_valign();
    draw_set_halign(fa_left); draw_set_valign(fa_middle);
    var _left = _centerX - floor((_labelWidth + _numberWidth) / 2);
    var _middle = _topY + floor(_numberHeight / 2);
    // Zpix包装器固定上移1px，中文+1使两种字高共享实际中线。
    UFO50_CHS_draw_text(_left, _middle + 1, _label);
    UFO50_CHS_draw_text(_left + _labelWidth, _middle, _number);
    draw_set_halign(_ha); draw_set_valign(_va); draw_set_font(_font);
}
""");
mortolPlayerHelpers.Import();
FixLayout("gml_Object_o01_Game_Draw_0",
    "draw_text_centered(_xv + 96, _yv + 176, str, 8);",
    "if (global.language == global.LANG_JAPANESE) UFO50_CHS_draw_mortol_player(_xv + 96, _yv + 176, string(currPlayer + 1));\n        else draw_text_centered(_xv + 96, _yv + 176, str, 8);");

// Private candidate fragments. Insert after FixLayout is defined and before the
// final draw-text wrapper redirection. Confirmed CHS before screenshots only;
// after-candidate CHS interaction and original EN/JA regression remain pending.

// 23: Keep the 56px HUD. Three rows have a measured CJK step (at least 12px),
// including the caller's original numeric glyphs on the same baseline.
FixLayout("gml_Object_o23_Mas_Draw_0", "var _yHud = _yview + 160;",
    "var _yHud = _yview + 160;\nvar _chsGolfStep = (global.language == global.LANG_JAPANESE) ? max(12, ceil(string_height(\"中\")) + 1) : 8;");
FixLayout("gml_Object_o23_Mas_Draw_0", "_yHud + 24,", "_yHud + 16 + _chsGolfStep,");
// The distance row was already 16px below par. It retains its old coordinate
// in EN/JA and follows the two CJK rows in CHS.
FixLayout("gml_Object_o23_Mas_Draw_0", "_yHud + 40,",
    "_yHud + ((global.language == global.LANG_JAPANESE) ? 16 + (2 * _chsGolfStep) : 40),");
FixLayout("gml_Object_o23_Mas_Draw_0", "scrStringDraw(_xHud + 32, _yHud, _strPlayer);",
    "if (global.language == global.LANG_JAPANESE) UFO50_CHS_draw_labeled_number(_xHud + 32, _yHud, string_delete(scrString(_strPlayer), string_length(scrString(_strPlayer)), 1) + \" \", string(_playerNum + 1), fa_left);\n    else scrStringDraw(_xHud + 32, _yHud, _strPlayer);");

// 25: Preserve the yellow paper and the title/map row. Two color groups become
// two columns rather than trying to fit six 11px rows into its 72px height.
// The frame center is the original mapName anchor; all columns are relative.
// Original board outer x180..299/y68..159. Expand each side 4px; the
// pointer ends near x174 and the new left edge176 keeps it visible.
FixLayout("gml_Object_o25__Game_Draw_0", "scrSetFont(global.fontNoShadow);\n    draw_set_color(c_black);",
    "scrSetFont(global.fontNoShadow);\n    if (global.language == global.LANG_JAPANESE)\n    {\n        draw_sprite_part_ext(bg25_AntWarBrief, 0, 180, 68, 60, 92, 1712, 68, 1, 1, c_white, 1);\n        draw_sprite_part_ext(bg25_AntWarBrief, 0, 236, 68, 8, 92, 1772, 68, 1, 1, c_white, 1);\n        draw_sprite_part_ext(bg25_AntWarBrief, 0, 240, 68, 60, 92, 1780, 68, 1, 1, c_white, 1);\n    }\n    draw_set_color(c_black);");
FixLayout("gml_Object_o25__Game_Draw_0", "scrDrawTextCenteredPoint(mapName, 1776, 80, 8);",
    "var _briefFrameCenterX = 1776;\n    var _briefBlueX = _briefFrameCenterX - 48;\n    var _briefRedX = _briefBlueX;\n    var _briefValueOffset = 72;\n    var _briefRowStep = 8;\n    if (global.language == global.LANG_JAPANESE)\n    {\n        var _briefFont = draw_get_font();\n        var _briefLabelWidth = max(string_width(scrStringLimit(\"ants\", 8) + \":\"), string_width(scrStringLimit(\"queens\", 8) + \":\"));\n        var _briefNumberFont = UFO50_CHS_number_font(\"99\");\n        if (_briefNumberFont >= 0) draw_set_font(_briefNumberFont);\n        var _briefNumberWidth = string_width(\"99\");\n        draw_set_font(_briefFont);\n        _briefValueOffset = ceil(_briefLabelWidth) + 4;\n        var _briefGroupWidth = _briefValueOffset + ceil(_briefNumberWidth);\n        var _briefBothWidth = (2 * _briefGroupWidth) + 4;\n        _briefBlueX = _briefFrameCenterX - floor(_briefBothWidth * 0.5);\n        _briefRedX = _briefBlueX + _briefGroupWidth + 4;\n        _briefRowStep = max(12, ceil(string_height(\"中\")) + 1);\n    }\n    scrDrawTextCenteredPoint(mapName, _briefFrameCenterX, 80, 8);");
FixLayout("gml_Object_o25__Game_Draw_0", "draw_text(1728, 112, scrStringLimit(\"ants\", 8) + \":\");",
    "draw_text(_briefBlueX, 112, scrStringLimit(\"ants\", 8) + \":\");");
FixLayout("gml_Object_o25__Game_Draw_0", "draw_text(1728, 120, scrStringLimit(\"queens\", 8) + \":\");",
    "draw_text(_briefBlueX, 112 + _briefRowStep, scrStringLimit(\"queens\", 8) + \":\");");
FixLayout("gml_Object_o25__Game_Draw_0", "draw_text(1728, 128, scrStringLimit(\"ants\", 8) + \":\");",
    "draw_text((global.language == global.LANG_JAPANESE) ? _briefRedX : _briefBlueX, (global.language == global.LANG_JAPANESE) ? 112 : 128, scrStringLimit(\"ants\", 8) + \":\");");
FixLayout("gml_Object_o25__Game_Draw_0", "draw_text(1728, 136, scrStringLimit(\"queens\", 8) + \":\");",
    "draw_text((global.language == global.LANG_JAPANESE) ? _briefRedX : _briefBlueX, (global.language == global.LANG_JAPANESE) ? 112 + _briefRowStep : 136, scrStringLimit(\"queens\", 8) + \":\");");
FixLayout("gml_Object_o25__Game_Draw_0", "draw_text(1800, 112, blueAnts);",
    "draw_text(_briefBlueX + _briefValueOffset, 112, blueAnts);");
FixLayout("gml_Object_o25__Game_Draw_0", "draw_text(1800, 120, blueQueens);",
    "draw_text(_briefBlueX + _briefValueOffset, 112 + _briefRowStep, blueQueens);");
FixLayout("gml_Object_o25__Game_Draw_0", "draw_text(1800, 128, redAnts);",
    "draw_text(_briefRedX + _briefValueOffset, (global.language == global.LANG_JAPANESE) ? 112 : 128, redAnts);");
FixLayout("gml_Object_o25__Game_Draw_0", "draw_text(1800, 136, redQueens);",
    "draw_text(_briefRedX + _briefValueOffset, (global.language == global.LANG_JAPANESE) ? 112 + _briefRowStep : 136, redQueens);");

// 31: Keep the original name backing and let it follow the actual text size.
// The blood bar starts below the full CJK name; original EN/JA stays 8px.
FixLayout("gml_GlobalScript_scr31_DrawBossHUD", "var _x2 = _x1 + (string_length(arg0) * 8);",
    "var _x2 = _x1 + ((global.language == global.LANG_JAPANESE) ? ceil(string_width(arg0)) : string_length(arg0) * 8);");
FixLayout("gml_GlobalScript_scr31_DrawBossHUD", "var _y2 = _y1 + 8;",
    "var _y2 = _y1 + ((global.language == global.LANG_JAPANESE) ? max(12, ceil(string_height(arg0)) + 1) : 8);");
FixLayout("gml_GlobalScript_scr31_DrawBossHUD", "draw_rectangle_color(_x1, _y1, _x2 - 1, _y2 - 1,",
    "draw_rectangle_color(_x1, _y1 - ((global.language == global.LANG_JAPANESE) ? 1 : 0), _x2 - 1, _y2 - 1,");

// 32: Preserve a two-line destination and expand the original dark backing
// vertically inside the sidebar, above the radar. Top 118, second 130.
FixLayout("gml_Object_o32_Mas_Draw_0", "draw_text(_xv + 16, _yv + 120, checkpointActiveName1);\ndraw_text(_xv + 16, _yv + 128, checkpointActiveName2);",
    "if (global.language == global.LANG_JAPANESE)\n{\n    var _destinationStep = max(12, ceil(string_height(\"中\")) + 1);\n    var _destinationTop = 118;\n    var _destinationWidth = max(string_width(checkpointActiveName1), string_width(checkpointActiveName2));\n    var _destinationRight = max(80, 16 + ceil(_destinationWidth) + 2);\n    draw_rectangle_color(_xv + 14, _yv + _destinationTop - 2, _xv + _destinationRight, _yv + _destinationTop + (2 * _destinationStep) + 1, c_black, c_black, c_black, c_black, 0);\n    draw_text(_xv + 16, _yv + _destinationTop, checkpointActiveName1);\n    draw_text(_xv + 16, _yv + _destinationTop + _destinationStep, checkpointActiveName2);\n}\nelse\n{\n    draw_text(_xv + 16, _yv + 120, checkpointActiveName1);\n    draw_text(_xv + 16, _yv + 128, checkpointActiveName2);\n}");

// 33: Menu options keep their logical hierarchy. Full ten-row scenario list
// is recentered as one measured block; number and title share each baseline.
FixLayout("gml_Object_o33_Game_Draw_0", "if (state == STATE_MODE_SELECT)\n{",
    "if (state == STATE_MODE_SELECT)\n{\n    var _chsMenuStep = (global.language == global.LANG_JAPANESE) ? max(12, ceil(string_height(\"中\")) + 1) : 8;");
foreach (var key in new[] { "quick_match", "skirmish", "medium" })
    FixLayout("gml_Object_o33_Game_Draw_0", "scrStringDrawCenterAltExt(192, 104, \"" + key + "\",",
        "scrStringDrawCenterAltExt(192, 96 + _chsMenuStep, \"" + key + "\",");
foreach (var key in new[] { "streak", "hard" })
    FixLayout("gml_Object_o33_Game_Draw_0", "scrStringDrawCenterAltExt(192, 112, \"" + key + "\",",
        "scrStringDrawCenterAltExt(192, 96 + (2 * _chsMenuStep), \"" + key + "\",");
FixLayout("gml_Object_o33_Game_Draw_0", "var yStart = (216 - ((NUM_LEVELS * 8) + 24)) / 2;\n        yStart = 8 * floor(yStart / 8);",
    "var yStart = (216 - ((NUM_LEVELS * _chsMenuStep) + 24)) / 2;\n        yStart = (global.language == global.LANG_JAPANESE) ? floor(yStart) : 8 * floor(yStart / 8);");
FixLayout("gml_Object_o33_Game_Draw_0", "yStart + 24 + (8 * i)", "yStart + 24 + (_chsMenuStep * i)");

// 33 shop: Four possible rows fit at48/60/72/84, then the divider96 and
// sale icons102. Original EN/JA step remains8. Fixed statistics split the
// numeric tail so labels use CHS while digits retain the original Fancy font.
FixLayout("gml_Object_o33_Game_Draw_0", "scrStringDrawExt(viewx + 312, 48, \"melee_stat\", selUnit.meleeDamage, 8, 0);",
    "var _chsShopStep = (global.language == global.LANG_JAPANESE) ? max(12, ceil(string_height(\"中\")) + 1) : 8;\n                if (global.language == global.LANG_JAPANESE) UFO50_CHS_draw_labeled_number(viewx + 312, 48, string_replace_all(scrString(\"melee_stat\"), \"*\", \"\") + \" \", string(selUnit.meleeDamage), fa_left);\n                else scrStringDrawExt(viewx + 312, 48, \"melee_stat\", selUnit.meleeDamage, 8, 0);");
FixLayout("gml_Object_o33_Game_Draw_0", "scrStringDrawExt(viewx + 312, 56, \"range_stat\", selUnit.rangeDamage, 8, 0);\n                    yShift = 8;",
    "if (global.language == global.LANG_JAPANESE) UFO50_CHS_draw_labeled_number(viewx + 312, 48 + _chsShopStep, string_replace_all(scrString(\"range_stat\"), \"*\", \"\") + \" \", string(selUnit.rangeDamage), fa_left);\n                    else scrStringDrawExt(viewx + 312, 56, \"range_stat\", selUnit.rangeDamage, 8, 0);\n                    yShift = _chsShopStep;");
FixLayout("gml_Object_o33_Game_Draw_0", "scrStringDrawExt(viewx + 312, 56 + yShift, \"hp_stat\", selUnit.hpMax, 8, 0);",
    "if (global.language == global.LANG_JAPANESE) UFO50_CHS_draw_labeled_number(viewx + 312, 48 + _chsShopStep + yShift, string_replace_all(scrString(\"hp_stat\"), \"*\", \"\") + \" \", string(selUnit.hpMax), fa_left);\n                else scrStringDrawExt(viewx + 312, 56 + yShift, \"hp_stat\", selUnit.hpMax, 8, 0);");
foreach (var key in new[] { "aquamove", "poison", "rigid", "hp_drain", "stun" })
    FixLayout("gml_Object_o33_Game_Draw_0", "scrStringDrawExt(viewx + 312, 64 + yShift, \"" + key + "\",",
        "scrStringDrawExt(viewx + 312, 48 + (2 * _chsShopStep) + yShift, \"" + key + "\",");
FixLayout("gml_Object_o33_Game_Draw_0", "draw_text(viewx + 312, 64, string(selUnit.mining) + \"X   \" + string(selUnit.mining) + \"X\");",
    "draw_text(viewx + 312, (global.language == global.LANG_JAPANESE) ? 48 + (2 * _chsShopStep) + yShift : 64, string(selUnit.mining) + \"X   \" + string(selUnit.mining) + \"X\");");
foreach (var icon in new[] { new { Name = "s33_Gold", X = 328 }, new { Name = "s33_Crystals", X = 368 } })
    FixLayout("gml_Object_o33_Game_Draw_0", "draw_sprite(" + icon.Name + ", 1, viewx + " + icon.X + ", 64 + yShift);",
        "draw_sprite(" + icon.Name + ", 1, viewx + " + icon.X + ", 48 + (2 * _chsShopStep) + yShift);");
FixLayout("gml_Object_o33_Game_Draw_0", "draw_line(viewx + 312, 88, viewx + 375, 88);",
    "var _chsShopDividerY = (global.language == global.LANG_JAPANESE) ? 48 + (4 * _chsShopStep) : 88;\n                draw_line(viewx + 312, _chsShopDividerY, viewx + 375, _chsShopDividerY);");
// 33 streak CURRENT/BEST are fixed statistics. Existing resource formats end
// in the single numeric placeholder **. Split only these two labels/counters.
FixLayout("gml_Object_o33_Game_Draw_0", "scrStringDrawVal(80, 176, \"current\", currStreak);",
    "if (global.language == global.LANG_JAPANESE)\n                    UFO50_CHS_draw_labeled_number(80, 176, string_replace_all(scrString(\"current\"), \"**\", \"\") + \" \", string(currStreak), fa_left);\n                else scrStringDrawVal(80, 176, \"current\", currStreak);");
FixLayout("gml_Object_o33_Game_Draw_0", "scrStringDrawVal(192, 176, \"best\", bestStreak);",
    "if (global.language == global.LANG_JAPANESE)\n                    UFO50_CHS_draw_labeled_number(192, 176, string_replace_all(scrString(\"best\"), \"**\", \"\") + \" \", string(bestStreak), fa_left);\n                else scrStringDrawVal(192, 176, \"best\", bestStreak);");

// 33 battle: Original panel is80x56. Keep the shot sequence at8 and the
// first attribute at24. CHS rows use12px, and copy the original bottom8px
// strip down so its white border follows the full last row with4px padding.
// EN/JA retains the original sprite, row coordinates and original number font.
FixLayout("gml_Object_o33_Game_Draw_0", "draw_sprite(s33_BattleHUD, 3, viewx, viewy);",
    "var _chsBattleStep = (global.language == global.LANG_JAPANESE) ? max(12, ceil(string_height(\"中\")) + 1) : 8;\n                        draw_sprite(s33_BattleHUD, 3, viewx, viewy);\n                        if (global.language == global.LANG_JAPANESE)\n                        {\n                            var _chsBattleExtra = (2 * _chsBattleStep) - 16;\n                            draw_rectangle_color(viewx + 2, viewy + 52, viewx + 77, viewy + 51 + _chsBattleExtra, c_black, c_black, c_black, c_black, false);\n                            draw_sprite_part_ext(s33_BattleHUD, 3, 0, 48, 80, 8, viewx, viewy + 48 + _chsBattleExtra, 1, 1, c_white, 1);\n                        }");
FixLayout("gml_Object_o33_Game_Draw_0", "scrStringDrawVal(viewx + 8, viewy + 24, \"melee_stat\", u.meleeDamage + u.bolstered);",
    "if (global.language == global.LANG_JAPANESE) UFO50_CHS_draw_labeled_number(viewx + 8, viewy + 24, string_replace_all(scrString(\"melee_stat\"), \"*\", \"\") + \" \", string(u.meleeDamage + u.bolstered), fa_left);\n                        else scrStringDrawVal(viewx + 8, viewy + 24, \"melee_stat\", u.meleeDamage + u.bolstered);");
FixLayout("gml_Object_o33_Game_Draw_0", "scrStringDrawExt(viewx + 8, viewy + 32, \"range_stat\", u.rangeDamage, 8, 0);\n                            yShift = 8;",
    "if (global.language == global.LANG_JAPANESE) UFO50_CHS_draw_labeled_number(viewx + 8, viewy + 24 + _chsBattleStep, string_replace_all(scrString(\"range_stat\"), \"*\", \"\") + \" \", string(u.rangeDamage), fa_left);\n                            else scrStringDrawExt(viewx + 8, viewy + 32, \"range_stat\", u.rangeDamage, 8, 0);\n                            yShift = _chsBattleStep;");
// Applies to abilities, mining text and its two original sprites in this block.
FixLayout("gml_Object_o33_Game_Draw_0", "viewy + 32 + yShift", "viewy + 24 + _chsBattleStep + yShift");

// 33 cancel: CHS is one complete row48px wide. Original EN is two rows
// in the same48px backing. Keep y1/y9 and the16px panel; only measure width.
FixLayout("gml_Object_o33_Game_Draw_0", "var messageWidth = max(string_length(cancelMessage[0]), string_length(cancelMessage[1]));",
    "var messageWidth = max(string_length(cancelMessage[0]), string_length(cancelMessage[1]));\n                if (global.language == global.LANG_JAPANESE) messageWidth = ceil(max(string_width(cancelMessage[0]), string_width(cancelMessage[1])) / 8);");

// 27 meta: Two11px rows currently overlap at64/72. The pink panel has
// room below the second row. Preserve first row and move the second to76.
FixLayout("gml_Object_o27__Game_Draw_0", "draw_text(_xview + 16, _yview + 72, scrStringManual(\"game_meta_message_27_2\", 0));",
    "draw_text(_xview + 16, _yview + 64 + ((global.language == global.LANG_JAPANESE) ? max(12, ceil(string_height(\"中\")) + 1) : 8), scrStringManual(\"game_meta_message_27_2\", 0));");

// 30: Two undecided-match rules stay above the original bottom panel edge.
// CHS originals and both EN/JA same-scene screenshots have now been reviewed.
FixLayout("gml_Object_o30_Game_Draw_0", "scrStringDrawExt(40, 176, \"2p_instruct_2\",",
    "scrStringDrawExt(40, 168 + ((global.language == global.LANG_JAPANESE) ? max(12, ceil(string_height(\"中\")) + 1) : 8), \"2p_instruct_2\",");

// 34: True ending camera is y=-448 after removing gameplay player sprites.
// Keep both player labels at y160 and move numeric rows to y172; input is y192.
foreach (var playerLabelX in new[] { 96, 224 })
    FixLayout("gml_Object_o34_Mas_Draw_0", "draw_text(_xv + " + playerLabelX + ", _yv + 168, pointsString);",
        "draw_text(_xv + " + playerLabelX + ", _yv + 160 + ((global.language == global.LANG_JAPANESE) ? max(12, ceil(string_height(\"中\")) + 1) : 8), pointsString);");
foreach (var playerPair in new[] { new { X = 96, Num = 1 }, new { X = 224, Num = 2 } })
    FixLayout("gml_Object_o34_Mas_Draw_0", "draw_text(_xv + " + playerPair.X + ", _yv + 160, scrStringVal(\"player_x\", " + playerPair.Num + "));",
        "if (global.language == global.LANG_JAPANESE) UFO50_CHS_draw_labeled_number(_xv + " + playerPair.X + ", _yv + 160, string_replace_all(scrString(\"player_x\"), \"*\", \"\"), \"" + playerPair.Num + "\", fa_left);\n            else draw_text(_xv + " + playerPair.X + ", _yv + 160, scrStringVal(\"player_x\", " + playerPair.Num + "));");

// Private proposal; game 20 original resource icons and all English branches retained.
// Existing formal bugGroup already lowers meters4px; keep its verified28/68/92 layout.
FixLayout("gml_Object_o20_Game_Draw_0", "draw_rectangle(16, 104, 16 + (8 * string_length(infoCard.displayName)), 119, false);",
    "if (global.language == global.LANG_JAPANESE) scrSetFont(fontTall);\n        var _chsCardPlateWidth = (global.language == global.LANG_JAPANESE) ? ceil(string_width(infoCard.displayName)) + 2 : 8 * string_length(infoCard.displayName);\n        draw_rectangle(16, 104, 16 + _chsCardPlateWidth, 119, false);");
// Same 80px description region, 12px CHS row step; longest real card is 5 lines, ends187.
FixLayout("gml_Object_o20_Game_Draw_0", "draw_text_ext(16, 152, \"+ \" + string(infoCard.description), 8, 80);",
    "draw_text_ext(16, 152, \"+ \" + string(infoCard.description), (global.language == global.LANG_JAPANESE) ? max(12, ceil(string_height(\"中\")) + 1) : 8, 80);");
FixLayout("gml_Object_o20_Game_Draw_0", "draw_text_ext(16, 128, string(infoCard.description), 8, 80);",
    "draw_text_ext(16, 128, string(infoCard.description), (global.language == global.LANG_JAPANESE) ? max(12, ceil(string_height(\"中\")) + 1) : 8, 80);");

// Private proposal, based on actual original EN/JA and formal CHS battle shots.
// Fixed HUD digits retain their caller's Tall/Default sprite font and original Y.
var overboldHudGroup = new UndertaleModLib.Compiler.CodeImportGroup(Data);
overboldHudGroup.QueueReplace("gml_GlobalScript_UFO50_CHS_draw_overbold_hud_number", """
function UFO50_CHS_draw_overbold_hud_number(_x, _numberY, _label, _number, _align, _labelY, _gap)
{
    var _font=draw_get_font();
    var _numberFont=UFO50_CHS_number_font(_number);
    var _labelWidth=string_width(_label);
    if(_numberFont>=0) draw_set_font(_numberFont);
    var _numberWidth=string_width(_number);
    draw_set_font(_font);
    var _oldAlign=draw_get_halign();
    if(_align==fa_right) _x-=_labelWidth+_gap+_numberWidth;
    draw_set_halign(fa_left);
    // Common CHS wrapper shifts the label up1; +1 preserves the chosen native top.
    draw_text(_x,_labelY+1,_label);
    if(_numberFont>=0) draw_set_font(_numberFont);
    draw_text(_x+_labelWidth+_gap,_numberY,_number);
    draw_set_font(_font); draw_set_halign(_oldAlign);
}
""");
overboldHudGroup.Import();
foreach(var overboldCode in new[]{"gml_Object_o30_Game_Draw_0","gml_Object_o30_TopText_Draw_0"})
{
    FixLayout(overboldCode,"draw_text(PRIZE_X, PRIZE_Y, str);",
        "if(global.language==global.LANG_JAPANESE) UFO50_CHS_draw_overbold_hud_number(PRIZE_X,PRIZE_Y,scrStringVal(\"prize\",\"\"),string(prize),fa_left,PRIZE_Y,2);\n        else draw_text(PRIZE_X, PRIZE_Y, str);");
    FixLayout(overboldCode,"draw_text(ENEMIES_X, ENEMIES_Y, str);",
        "if(global.language==global.LANG_JAPANESE) UFO50_CHS_draw_overbold_hud_number(ENEMIES_X,ENEMIES_Y,scrStringVal(\"enemies\",\"\"),string(enemiesLeft),fa_right,ENEMIES_Y,2);\n        else draw_text(ENEMIES_X, ENEMIES_Y, str);");
}
FixLayout("gml_Object_o30_Game_Draw_0","draw_text(LVL_X, LVL_Y, scrStringExt(\"level_abbrev\", \"*\", 3, 0) + \"-\" + string(lvl));",
    "if(global.language==global.LANG_JAPANESE) UFO50_CHS_draw_overbold_hud_number(LVL_X,LVL_Y,scrStringExt(\"level_abbrev\",\"*\",3,0)+\"-\",string(lvl),fa_left,LVL_Y-3,0);\n        else draw_text(LVL_X, LVL_Y, scrStringExt(\"level_abbrev\", \"*\", 3, 0) + \"-\" + string(lvl));");

// Actual bottom glyph comparison confirms rows9/10 of 弹 are clipped at y207.
FixLayout("gml_Object_o30_Game_Draw_0", "scrStringDrawExt(MINE_X, MINE_Y, \"bomb\", \"*\", 5, 0);", "scrStringDrawExt(MINE_X, ((global.language == global.LANG_JAPANESE) ? MINE_Y - 2 : MINE_Y), \"bomb\", \"*\", 5, 0);");

// Private proposal: job1 is confirmed by original EN/JA and formal CHS card scene.
// Player 1/2 and opponent status are verified in sixteenth EN/JA/CHS before screenshots.
var bughunterNumberGroup = new UndertaleModLib.Compiler.CodeImportGroup(Data);
bughunterNumberGroup.QueueReplace("gml_GlobalScript_UFO50_CHS_draw_bughunter_hud_number", """
function UFO50_CHS_draw_bughunter_hud_number(_x,_y,_label,_value)
{
    var _font=draw_get_font();
    var _labelWidth=string_width(_label);
    var _numberFont=UFO50_CHS_number_font(_value);
    if(_numberFont>=0) draw_set_font(_numberFont);
    var _numberWidth=string_width(_value);
    draw_set_font(_font);
    var _oldColor=draw_get_color();
    // 10after native face bbox: CHS label4..13 vs Default digits4..9.
    // Move only label2px upward; keep digit requested font and original Y4.
    var _labelY=_y-2;
    draw_set_color(c_black);
    draw_rectangle(_x,_labelY-1,_x+_labelWidth+_numberWidth-1,_labelY+max(8,ceil(string_height("中")))-1,false);
    draw_set_color(_oldColor);
    draw_text(_x,_labelY,_label);
    if(_numberFont>=0) draw_set_font(_numberFont);
    draw_text(_x+_labelWidth,_y,_value);
    draw_set_font(_font);
}
""");
bughunterNumberGroup.Import();
var bughunterHudCode=Data.Code.ByName("gml_Object_o20_Game_Draw_0");
var bughunterHudSource=GetDecompiledText(bughunterHudCode,new GlobalDecompileContext(Data));
var bughunterHudPattern=@"draw_text_bg\(16,\s*(?<y>[^;]+?),\s*(?<label>scrStringExt\(""job"",\s*0,\s*8,\s*0\))\s*\+\s*""-""\s*\+\s*string\((?<value>job)\),\s*0,\s*8,\s*8,\s*1\);";
if(System.Text.RegularExpressions.Regex.Matches(bughunterHudSource,bughunterHudPattern).Count!=1)
    throw new System.Exception("Expected exactly one Bug Hunter fixed job HUD.");
var bughunterHudResult=System.Text.RegularExpressions.Regex.Replace(bughunterHudSource,bughunterHudPattern,m=>
    "if(global.language==global.LANG_JAPANESE) UFO50_CHS_draw_bughunter_hud_number(16,4,"+m.Groups["label"].Value+"+\"-\",string("+m.Groups["value"].Value+"));\n        else "+m.Value);
var bughunterPlayerPattern=@"draw_text_bg\(16,\s*(?<y>[^;]+?),\s*(?<label>scrStringExt\(""player"",\s*0,\s*8,\s*0\))\s*\+\s*""-""\s*\+\s*string\((?<value>!?cp\s*\+\s*1)\),\s*0,\s*8,\s*8,\s*1\);";
if(System.Text.RegularExpressions.Regex.Matches(bughunterHudResult,bughunterPlayerPattern).Count!=2)
    throw new System.Exception("Expected exactly two Bug Hunter fixed player HUD calls.");
bughunterHudResult=System.Text.RegularExpressions.Regex.Replace(bughunterHudResult,bughunterPlayerPattern,m=>
    "if(global.language==global.LANG_JAPANESE) UFO50_CHS_draw_bughunter_hud_number(16,4,"+m.Groups["label"].Value+"+\"-\",string("+m.Groups["value"].Value+"));\n        else "+m.Value);
var bughunterTurnPattern=@"scrDrawTextInput\(CARD_LEFT_X,\s*CARD_TOP_Y\s*-\s*16,\s*helpMessage,\s*cp,\s*0,\s*0\);";
if(System.Text.RegularExpressions.Regex.Matches(bughunterHudResult,bughunterTurnPattern).Count!=1)
    throw new System.Exception("Expected exactly one Bug Hunter input message draw.");
bughunterHudResult=System.Text.RegularExpressions.Regex.Replace(bughunterHudResult,bughunterTurnPattern,m=>
    "if(global.language==global.LANG_JAPANESE && numPlayers==2 && textMessageFull==scrStringExt(\"player_turn\",cp+1,0,0)) UFO50_CHS_draw_fixed_mixed(CARD_LEFT_X,CARD_TOP_Y-16,helpMessage,-2);\n            else "+m.Value);
var bughunterHudImport = new UndertaleModLib.Compiler.CodeImportGroup(Data);
var bughunterTurnExtPattern=@"draw_text_ext\(CARD_LEFT_X,\s*CARD_TOP_Y\s*-\s*16,\s*string\(helpMessage\),\s*8,\s*192\);";
if(System.Text.RegularExpressions.Regex.Matches(bughunterHudResult,bughunterTurnExtPattern).Count!=1)
    throw new System.Exception("Expected exactly one Bug Hunter non-input message draw.");
bughunterHudResult=System.Text.RegularExpressions.Regex.Replace(bughunterHudResult,bughunterTurnExtPattern,m=>
    "if(global.language==global.LANG_JAPANESE && numPlayers==2 && textMessageFull==scrStringExt(\"player_turn\",cp+1,0,0)) UFO50_CHS_draw_fixed_mixed(CARD_LEFT_X,CARD_TOP_Y-16,helpMessage,-2);\n            else "+m.Value);
bughunterHudImport.QueueReplace(bughunterHudCode,bughunterHudResult);
bughunterHudImport.Import();

// Private proposals: merge before the final draw-call redirection pass.
// Needs the existing FixLayout helper; source English/Japanese paths stay intact.
var design3551 = new UndertaleModLib.Compiler.CodeImportGroup(Data);
design3551.AutoCreateAssets = true;
design3551.ThrowOnNoOpFindReplace = true;
design3551.QueueReplace("gml_GlobalScript_UFO50_CHS_draw_valbrace_fight", """
function UFO50_CHS_draw_valbrace_fight()
{
    var _vx = camera_get_view_x(view_get_camera(0));
    var _vy = camera_get_view_y(view_get_camera(0));
    var _index = instance_number(o35__Enemy) - 1;
    with (o35__Enemy)
    {
        var _yy = _vy + 64 + 28 * _index;
        // Full name gets its own 62px row, followed by icon and health bar.
        var _nameX = _vx + 376 - string_width(name);
        var _oldColor = draw_get_color();
        draw_set_color(c_black);
        draw_rectangle(_nameX - 1, _yy - 2, _vx + 377, _yy + ceil(string_height(name)), false);
        draw_set_color(_oldColor);
        draw_text(_nameX, _yy, name);
        draw_sprite_ext(s35_eIcon, enemy_type, _vx + 320, _yy + 13, 1, 1, 0, lerp(8421504, 16777215, inForeground || !global.canDoShaders), 1);
        scrDrawVarBar(_vx + 337, _yy + 14, _vx + 374, _yy + 21, 0, hp, hpMax, 3292384, 0);
        draw_set_colour(c_white);
        _index--;
    }
    return instance_number(o35__Enemy);
}
""");
design3551.QueueReplace("gml_GlobalScript_UFO50_CHS_draw_valbrace_status", """
function UFO50_CHS_draw_valbrace_status()
{
    var _labels = [];
    var _colors = [];
    var _n = 0;
    var _enemies = instance_number(o35__Enemy);
    if (ds_map_find_value(myInventory, "ACCESSORY") != -1 && _enemies == 0)
    {
        var _traits = scr35_EquipmentTraits(ds_map_find_value(myInventory, "ACCESSORY"));
        var _lines = string_line_breaks(_traits[0], 7, 2);
        for (var _a = 0; _a < 2; _a++) if (_lines[_a] != "")
        {
            _labels[_n] = _lines[_a];
            _colors[_n] = global.palette[2 + ((current_time div 160) % 2)];
            _n++;
        }
    }
    var _enabled = [shieldUnbreakable, hasInitiative, isShielded > 0, isHasted > 0, isPoisoned > 0, isPowerful > 0, isBurning > 0, isStone > 0, isFlying > 0, isInvisible > 0, isSilenced > 0, isSlimed > 0];
    var _keys = ["status_sturdy", "status_initiative", "status_shield", "status_haste", "status_poison", "status_power", "status_burn", "status_stone", "status_fly", "status_invis", "status_silence", "status_slimed"];
    for (var _i = 0; _i < 12; _i++) if (_enabled[_i])
    {
        _labels[_n] = scrString(_keys[_i]);
        _colors[_n] = global.palette[0];
        _n++;
    }
    if (_n == 0) return;
    var _vx = camera_get_view_x(view_get_camera(0));
    var _vy = camera_get_view_y(view_get_camera(0));
    var _step = max(13, ceil(string_height("中")) + 2);
    var _top = max(96, 64 + 28 * _enemies + 4);
    var _capacity = max(1, floor((200 - ceil(string_height("中")) - _top) / _step) + 1);
    var _pages = ceil(_n / _capacity);
    // Start at page 1 when the active status count changes; rotate every 3s.
    if (!variable_instance_exists(id, "chs35StatusCount") || chs35StatusCount != _n)
    {
        chs35StatusCount = _n;
        chs35StatusStartedAt = current_time;
    }
    var _page = floor((current_time - chs35StatusStartedAt) / 3000) % _pages;
    var _first = _page * _capacity;
    var _last = min(_n, _first + _capacity);
    for (var _j = _first; _j < _last; _j++)
    {
        draw_set_color(_colors[_j]);
        draw_text(_vx + 324, _vy + _top + (_j - _first) * _step, _labels[_j]);
    }
    draw_set_color(global.palette[0]);
    if (_pages > 1) draw_text(_vx + 348, _vy + 204, string(_page + 1) + "/" + string(_pages));
}
""");
design3551.QueueReplace("gml_GlobalScript_UFO50_CHS_draw_valbrace_inventory", """
function UFO50_CHS_draw_valbrace_inventory(arg0, arg1)
{
    var _step = max(13, ceil(string_height("中")) + 2);
    var _x = arg0 - 8;
    var _y = arg1 + (interactSelect + 1) * _step;
    var _vy = camera_get_view_y(view_get_camera(0));
    var _inv = ds_map_find_value(myInventory, "ITEM_LIST");
    var _count = ds_list_size(_inv);
    if (_count == 0) return;
    var _w = 128;
    for (var _i = 0; _i < _count; _i++)
    {
        var _traits = scr35_EquipmentTraits(ds_list_find_value(_inv, _i));
        _w = max(ceil(string_width(_traits[0]) / 8) * 8 + 40, _w);
    }
    // Reserve the bottom action row before choosing the visible item count.
    // The original inventorySelect and navigation indices are unchanged.
    var _visible = max(1, min(_count, floor((_vy + 178 - _y - 29) / _step)));
    var _first = clamp(inventorySelect - floor(_visible / 2), 0, max(0, _count - _visible));
    var _last = min(_count, _first + _visible);
    var _height = 29 + _visible * _step;
    if (gameState != 4) textBoxLerp = max(textBoxLerp, 3);
    textBoxLerp = approach(textBoxLerp, 3, 0.2);
    scrDrawMenuBorder(_x, _y, _w, 17 + ceil(((_height - 17) * clamp(textBoxLerp - 2, 0, 1)) / 4) * 4);
    if (textBoxLerp < 3) return;
    var _left = _x + 16;
    var _bodyTop = _y + 10;
    var _eq = ds_map_find_value(myInventory, "EQUIPPED_SELECTION");
    for (var _j = _first; _j < _last; _j++)
    {
        var _item = ds_list_find_value(_inv, _j);
        var _traits = scr35_EquipmentTraits(_item);
        var _yy = _bodyTop + (_j - _first) * _step;
        var _o = 0;
        if (_traits[1] == 0 && _traits[4] > statSTR) draw_set_color(global.palette[9]);
        if (_j == inventorySelect)
        {
            draw_text(_left, _yy, ">");
            draw_set_color(global.palette[3]); _o = 8;
        }
        if (_eq[0] == _j || _eq[1] == _j || _eq[2] == _j || _eq[3] == _j)
        {
            if (_o == 0)
            {
                draw_set_color(global.palette[4]);
                draw_text(_left, _yy, scrStringFormat("*{}", _traits[0]));
            }
            else draw_text(_left, _yy, scrStringFormat(" *{}", _traits[0]));
        }
        else if (_item == 40 || _item == 39)
        {
            var _value = "(" + string((_item == 40) ? gemCount : keyCount) + ")";
            UFO50_CHS_draw_labeled_number(_left + _o, _yy, _traits[0] + " ", _value, fa_left);
        }
        else draw_text(_left + _o, _yy, _traits[0]);
        draw_set_color(global.palette[0]);
    }
    if (_count > _visible)
        draw_text(_x + _w - 48, _y + _height - 10, string(_first + 1) + "-" + string(_last) + "/" + string(_count));
    if (gameState == 7)
    {
        var _give = 0;
        var _traits = scr35_EquipmentTraits(ds_list_find_value(_inv, inventorySelect));
        var _dropname = scrString("inventory_drop");
        var _usename = "";
        if (_traits[1] == 0 || _traits[1] == 1 || _traits[1] == 2) _usename = scrString("inventory_equip");
        else if (_traits[1] == 3) _usename = scrString("inventory_use");
        else if (_traits[1] == 4) _usename = scrString("inventory_hold");
        with (o35_iRedKnight) if (ox == other.pX && oy == other.pY) _give = 1;
        with (o35_iCrone) if (ox == other.pX && oy == other.pY) _give = 2;
        if (_give == 1) _dropname = scrString("inventory_give");
        else if (_give == 2) _dropname = scrString("inventory_sell");
        var _actionWidth = string_width(_usename) + string_width(_dropname) + 64;
        var _actionTop = min(_vy + 190, _y + _height + 4);
        scrDrawMenuBorder(_left, _actionTop, _actionWidth, 24);
        var _textY = _actionTop + 9;
        if (inventoryAction == 0)
        {
            draw_set_color(global.palette[3]);
            draw_text(_left + 8, _textY, ">" + _usename);
        }
        else
        {
            draw_set_color(global.palette[0]);
            draw_text(_left + 16, _textY, _usename);
        }
        draw_set_color(global.palette[0]);
        if (_give == 1 && floor(current_time / 500) % 2 == 0) draw_set_color(global.palette[7]);
        if (_give == 2 && floor(current_time / 500) % 2 == 0) draw_set_color(global.palette[3]);
        if (inventoryAction == 1)
        {
            draw_set_color(global.palette[3]); _dropname = ">" + _dropname;
        }
        draw_text(_left + _actionWidth - 16 - string_width(_dropname), _textY, _dropname);
        draw_set_color(global.palette[0]);
    }
}
""");
design3551.QueueReplace("gml_GlobalScript_UFO50_CHS_draw_seaside_wave", """
function UFO50_CHS_draw_seaside_wave(_centerX, _y, _text, _time)
{
    var _font = draw_get_font();
    var _widths = [];
    var _width = 0;
    for (var _i = 1; _i <= string_length(_text); _i++)
    {
        var _c = string_char_at(_text, _i);
        var _numericFont = UFO50_CHS_number_font(_c);
        if (_numericFont >= 0) draw_set_font(_numericFont);
        _widths[_i] = string_width(_c);
        _width += _widths[_i];
        draw_set_font(_font);
    }
    var _xx = _centerX - floor(_width / 2);
    for (var _j = 1; _j <= string_length(_text); _j++)
    {
        var _c = string_char_at(_text, _j);
        var _yy = round(_y + sin((_time + 16 + _j * 8) / 10) * 2);
        draw_text(_xx, _yy, _c);
        _xx += _widths[_j];
    }
}
""");
design3551.QueueReplace("gml_GlobalScript_UFO50_CHS_draw_pilot_menu_label", """
function UFO50_CHS_draw_pilot_menu_label(_x, _y, _text, _color, _meat)
{
    var _prefixLength = 0;
    for (var _i = 1; _i <= string_length(_text); _i++)
    {
        if (ord(string_char_at(_text, _i)) > 127) break;
        _prefixLength++;
    }
    var _prefix = string_copy(_text, 1, _prefixLength);
    var _label = string_delete(_text, 1, _prefixLength);
    var _font = draw_get_font();
    var _numberFont = UFO50_CHS_number_font(_prefix);
    if (_numberFont >= 0) draw_set_font(_numberFont);
    var _prefixWidth = string_width(_prefix);
    draw_set_font(_font);
    var _width = _prefixWidth + string_width(_label);
    var _height = max(8, ceil(string_height("中")));
    draw_rectangle_color(_x - 1, _y - 2, _x + _width, _y + _height, c_black, c_black, c_black, c_black, false);
    if (_meat) draw_sprite(s44_Resources, 1, _x, _y);
    if (_prefix != "") draw_text_color(_x, _y, _prefix, _color, _color, _color, _color, 1);
    draw_text_color(_x + _prefixWidth, _y, _label, _color, _color, _color, _color, 1);
}
""");
design3551.Import();

FixLayout("gml_GlobalScript_scr35_DrawFightUI", "    var _xview = camera_get_view_x(view_get_camera(0));", "    if (global.language == global.LANG_JAPANESE) return UFO50_CHS_draw_valbrace_fight();\n    var _xview = camera_get_view_x(view_get_camera(0));");
FixLayout("gml_GlobalScript_scr35_DrawStatusUI", "    var _k = 0;", "    if (global.language == global.LANG_JAPANESE) { UFO50_CHS_draw_valbrace_status(); return; }\n    var _k = 0;");
FixLayout("gml_GlobalScript_scr35_DrawInventoryUI", "    var _x = arg0;", "    if (global.language == global.LANG_JAPANESE) { UFO50_CHS_draw_valbrace_inventory(arg0, arg1); return; }\n    var _x = arg0;");

// Existing interaction menu remains the source of selection and animations.
// Its measured step is also passed implicitly through the inventory position.
FixLayout("gml_GlobalScript_scr35_DrawMenuUI", "    var _str = scrStringSplit(\"interaction_prompt\", 26, 3);", "    var _step = (global.language == global.LANG_JAPANESE) ? max(13, ceil(string_height(\"中\")) + 2) : 10;\n    var _str = scrStringSplit(\"interaction_prompt\", 26, 3);");
FixLayout("gml_GlobalScript_scr35_DrawMenuUI", "scrDrawTextBoxGetHeight(_str, 32, 10, 3)", "scrDrawTextBoxGetHeight(_str, 32, _step, 3)");
FixLayout("gml_GlobalScript_scr35_DrawMenuUI", "draw_text(_x + 16, _y + 22, _str[1]);", "draw_text(_x + 16, _y + 12 + _step, _str[1]);");
FixLayout("gml_GlobalScript_scr35_DrawMenuUI", "draw_text(_x + 16, _y + 32, _str[2]);", "draw_text(_x + 16, _y + 12 + 2 * _step, _str[2]);");
// Replace all matching row terms in this one script, including frame height.
var design35Menu = new UndertaleModLib.Compiler.CodeImportGroup(Data);
design35Menu.ThrowOnNoOpFindReplace = true;
design35Menu.QueueRegexFindReplace(Data.Code.ByName("gml_GlobalScript_scr35_DrawMenuUI"), @"\b10 \* (i|ds_list_size\(interactList\))", "_step * $1", true);
design35Menu.Import();

FixLayout("gml_Object_o47_BossExplosion_Draw_0", "    var text = string_even(text, 3);", "    if (global.language == global.LANG_JAPANESE)\n    {\n        UFO50_CHS_draw_seaside_wave(room_width / 2, 88, text, time);\n        exit;\n    }\n    var text = string_even(text, 3);");

FixLayout("gml_Object_o42_Game_Draw_0", "draw_text(376 - (8 * string_length(tempName)), 200, tempName);", "draw_text(376 - ((global.language == global.LANG_JAPANESE) ? string_width(tempName) : (8 * string_length(tempName))), 200, tempName);");

FixLayout("gml_Object_o46_Prologue_Draw_0", "    scrSetFont(global.fontDefault);\n    for (var i = 0; i < 3; i++)", "    scrSetFont(global.fontDefault);\n    var _chsDialogueStep = (global.language == global.LANG_JAPANESE) ? max(12, ceil(string_height(\"中\"))) : 8;\n    var _chsDialogueTop = (global.language == global.LANG_JAPANESE) ? 216 - (3 * _chsDialogueStep) : 184;\n    for (var i = 0; i < 3; i++)");
FixLayout("gml_Object_o46_Prologue_Draw_0", "camera_get_view_y(view_get_camera(0)) + 184 + (8 * i), strText[i]", "camera_get_view_y(view_get_camera(0)) + _chsDialogueTop + (_chsDialogueStep * i), strText[i]");

// Same 44 scene in EN/JA and 4x details confirms width already fit, while
// Zpix glyphs cross the 8px-high backdrop into the tile floor. Split price
// prefix from label so its digits keep the original font and metrics.
var design44Original = """
                draw_rectangle_color(xoff, yoff2, xoff + (string_length(str) * 8), yoff2 + 8, c_black, c_black, c_black, c_black, false);
                if (_drawMeatIcon)
                {
                    draw_sprite(s44_Resources, 1, xoff, yoff2);
                }
                draw_text_color(xoff, yoff2, str, col, col, col, col, 1);
""";
var design44Replacement = """
                if (global.language == global.LANG_JAPANESE)
                {
                    UFO50_CHS_draw_pilot_menu_label(xoff, yoff2, str, col, _drawMeatIcon);
                }
                else
                {
                    draw_rectangle_color(xoff, yoff2, xoff + (string_length(str) * 8), yoff2 + 8, c_black, c_black, c_black, c_black, false);
                    if (_drawMeatIcon)
                    {
                        draw_sprite(s44_Resources, 1, xoff, yoff2);
                    }
                    draw_text_color(xoff, yoff2, str, col, col, col, col, 1);
                }
""";
FixLayout("gml_Object_o44__Game_Draw_0", design44Original, design44Replacement);


// Confirmed first-after scene: SCORE 10000 and right-aligned 1UP:9
// still share the Chinese font. Limit mixed metrics to these numeric UI calls.
var design47Numeric = new UndertaleModLib.Compiler.CodeImportGroup(Data);
design47Numeric.QueueReplace("gml_GlobalScript_UFO50_CHS_draw_seaside_counter", """
function UFO50_CHS_draw_seaside_counter(_x, _y, _text, _alignment, _even)
{
    var _font = draw_get_font();
    var _original = UFO50_CHS_number_font("0");
    var _savedAlign = draw_get_halign();
    if (_even > 0) _text = string_even(_text, _even - 1);
    var _parts = [];
    var _fonts = [];
    var _widths = [];
    var _total = 0;
    var _index = 0;
    var _at = 1;
    while (_at <= string_length(_text))
    {
        var _ascii = ord(string_char_at(_text, _at)) < 128;
        var _part = "";
        while (_at <= string_length(_text) && ((ord(string_char_at(_text, _at)) < 128) == _ascii))
        {
            _part += string_char_at(_text, _at);
            _at++;
        }
        var _partFont = (_ascii && _original >= 0) ? _original : _font;
        draw_set_font(_partFont);
        _parts[_index] = _part;
        _fonts[_index] = _partFont;
        _widths[_index] = string_width(_part);
        _total += _widths[_index];
        _index++;
    }
    var _xx = _x;
    if (_alignment == fa_center) _xx -= floor(_total / 2);
    if (_alignment == fa_right) _xx -= _total;
    draw_set_halign(fa_left);
    for (var _j = 0; _j < _index; _j++)
    {
        draw_set_font(_fonts[_j]);
        draw_text(_xx, _y, _parts[_j]);
        _xx += _widths[_j];
    }
    draw_set_font(_font);
    draw_set_halign(_savedAlign);
}
""");
design47Numeric.Import();
FixLayout("gml_Object_o47_BossExplosion_Draw_0", "draw_text_ce(camera_get_view_x(view_get_camera(0)) + 192, 48, scrStringVal(\"stage_clear\", o47_Control.level + 1), 4);", "if (global.language == global.LANG_JAPANESE) UFO50_CHS_draw_seaside_counter(camera_get_view_x(view_get_camera(0)) + 192, 48, scrStringVal(\"stage_clear\", o47_Control.level + 1), fa_center, 4); else draw_text_ce(camera_get_view_x(view_get_camera(0)) + 192, 48, scrStringVal(\"stage_clear\", o47_Control.level + 1), 4);");
FixLayout("gml_Object_o47_BossExplosion_Draw_0", "draw_text_ce(camera_get_view_x(view_get_camera(0)) + 192, 72, scrStringVal(\"score_x\", levelscore), 4);", "if (global.language == global.LANG_JAPANESE) UFO50_CHS_draw_seaside_counter(camera_get_view_x(view_get_camera(0)) + 192, 72, scrStringVal(\"score_x\", levelscore), fa_center, 4); else draw_text_ce(camera_get_view_x(view_get_camera(0)) + 192, 72, scrStringVal(\"score_x\", levelscore), 4);");
FixLayout("gml_Object_o47_BossExplosion_Draw_0", "draw_text_ce(camera_get_view_x(view_get_camera(0)) + 192, 72, scrStringVal(\"score_x\", oPlayer.pscore), 4);", "if (global.language == global.LANG_JAPANESE) UFO50_CHS_draw_seaside_counter(camera_get_view_x(view_get_camera(0)) + 192, 72, scrStringVal(\"score_x\", oPlayer.pscore), fa_center, 4); else draw_text_ce(camera_get_view_x(view_get_camera(0)) + 192, 72, scrStringVal(\"score_x\", oPlayer.pscore), 4);");
FixLayout("gml_Object_o47_BossExplosion_Draw_0", "draw_text_ce(room_width / 2, 88, scrStringVal(\"x_lives_lost\", misstimes), 4);", "if (global.language == global.LANG_JAPANESE) UFO50_CHS_draw_seaside_counter(room_width / 2, 88, scrStringVal(\"x_lives_lost\", misstimes), fa_center, 4); else draw_text_ce(room_width / 2, 88, scrStringVal(\"x_lives_lost\", misstimes), 4);");
FixLayout("gml_Object_o47_BossExplosion_Draw_0", "draw_text_ce(room_width / 2, 88, text, 4);", "if (global.language == global.LANG_JAPANESE) UFO50_CHS_draw_seaside_counter(room_width / 2, 88, text, fa_center, 4); else draw_text_ce(room_width / 2, 88, text, 4);");
FixLayout("gml_Object_o47_Player_Draw_0", "scrStringDrawVal(dgx + 329, _yHud + 4, \"1up\", credit);", "if (global.language == global.LANG_JAPANESE) UFO50_CHS_draw_seaside_counter(dgx + 329, _yHud + 4, scrStringVal(\"1up\", credit), fa_right, 0); else scrStringDrawVal(dgx + 329, _yHud + 4, \"1up\", credit);");

// ID44 wave splash: fixed UI numeral remains in the original fontTall.
var wave44 = new UndertaleModLib.Compiler.CodeImportGroup(Data);
wave44.AutoCreateAssets = true;
wave44.ThrowOnNoOpFindReplace = true;
wave44.QueueReplace("gml_GlobalScript_UFO50_CHS_draw_pilot_wave_label", """
function UFO50_CHS_draw_pilot_wave_label(_x, _y, _wave)
{
    var _format = scrString("wave_text");
    var _split = string_pos("*", _format);
    if (_split <= 0) { draw_text(_x - floor(string_width(_format) / 2), _y, _format); return; }
    var _prefix = string_copy(_format, 1, _split - 1);
    var _suffix = string_copy(_format, _split + 1, string_length(_format));
    var _digit = string(_wave);
    var _font = draw_get_font();
    var _nf = UFO50_CHS_number_font(_digit);
    var _pw = string_width(_prefix);
    var _sw = string_width(_suffix);
    if (_nf >= 0) draw_set_font(_nf);
    var _nw = string_width(_digit);
    var _nh = string_height(_digit);
    draw_set_font(_font);
    var _align = draw_get_halign();
    var _valign = draw_get_valign();
    draw_set_halign(fa_left);
    draw_set_valign(fa_middle);
    var _middle = _y + _nh / 2;
    var _left = _x - ceil((_pw + _nw + _sw) / 2);
    // Keep the original Tall numeral's vertical origin and center CHS beside it.
    // Chinese wrapper moves its Y by -1, compensated at this local middle.
    draw_text(_left, _middle + 1, _prefix);
    draw_text(_left + _pw, _middle, _digit);
    draw_text(_left + _pw + _nw, _middle + 1, _suffix);
    draw_set_halign(_align);
    draw_set_valign(_valign);
}
""");
wave44.Import();
FixLayout("gml_Object_o44__Game_Draw_0",
    "scrDrawTextCenteredPoint(scrStringVal(\"wave_text\", wave), _xview + 32 + 176, _yview + 108, 8);",
    "if (global.language == global.LANG_JAPANESE) UFO50_CHS_draw_pilot_wave_label(_xview + 32 + 176, _yview + 108, wave);\n        else scrDrawTextCenteredPoint(scrStringVal(\"wave_text\", wave), _xview + 32 + 176, _yview + 108, 8);");

// ID50 scenario screen: witnessed 8px overlapping CHS rows in fourth-before.
// Keep English/Japanese original rows; Chinese list/selection use the same step.
FixLayout("gml_Object_o50_Game_Draw_0",
    "        var topY = 76;",
    "        var topY = 60;\n        var _scenarioStep = (global.language == global.LANG_JAPANESE) ? max(13, ceil(string_height(\"中\")) + 2) : 8;\n        if (global.language != global.LANG_JAPANESE) topY = 76;");
// Exact fixed offsets apply only inside the original scenario list block.
foreach (var k in Enumerable.Range(1, 7))
    FixLayout("gml_Object_o50_Game_Draw_0",
        "topY + " + (8 * k).ToString() + ", scrString(\"scenario_" + (k + 1).ToString() + "\")",
        "topY + _scenarioStep * " + k.ToString() + ", scrString(\"scenario_" + (k + 1).ToString() + "\")");
FixLayout("gml_Object_o50_Game_Draw_0",
    "topY + (8 * scenarioY)", "topY + (_scenarioStep * scenarioY)");

// Sixth-before CHS prompt and all four choices overlap at the original 10px.
// Lay out the prompt first, then place the choices below its completed frame.
FixLayout("gml_GlobalScript_scr35_DrawWishUI",
    "    var _str = scrStringSplit(\"wishwell_dialog\", 26, 3);",
    "    var _chs = global.language == global.LANG_JAPANESE;\n    var _step = _chs ? max(13, ceil(string_height(\"中\")) + 2) : 10;\n    var _str = scrStringSplit(\"wishwell_dialog\", 26, 3);");
FixLayout("gml_GlobalScript_scr35_DrawWishUI",
    "scrDrawTextBoxGetHeight(_str, 36, 10, 3)", "scrDrawTextBoxGetHeight(_str, 36, _step, 3)");
FixLayout("gml_GlobalScript_scr35_DrawWishUI",
    "draw_text(_x + 16, _y + 22, _str[1]);", "draw_text(_x + 16, _y + 12 + _step, _str[1]);");
FixLayout("gml_GlobalScript_scr35_DrawWishUI",
    "draw_text(_x + 16, _y + 32, _str[2]);", "draw_text(_x + 16, _y + 12 + 2 * _step, _str[2]);");
FixLayout("gml_GlobalScript_scr35_DrawWishUI",
    "scrDrawMenuBorder(_xview + 80, (_yview + _h) - 7, _w, 17 + (10 * _optionCount));",
    "scrDrawMenuBorder(_xview + 80, _yview + _h + (_chs ? 12 : -7), _w, 17 + (_step * _optionCount));");
FixLayout("gml_GlobalScript_scr35_DrawWishUI",
    "draw_text(_xview + 96, _yview + _h + 3 + (10 * i), _choice);",
    "draw_text(_xview + 96, _yview + _h + (_chs ? 22 : 3) + (_step * i), _choice);");

// Tenth-before real throne: three choices overlap at10px and points use Zpix.
FixLayout("gml_GlobalScript_scr35_DrawThroneUI", "    var _level = throneSit.levelList;",
    "    var _chs = global.language == global.LANG_JAPANESE;\n    var _rowStep = _chs ? 13 : 10;\n    var _level = throneSit.levelList;");
FixLayout("gml_GlobalScript_scr35_DrawThroneUI", "17 + (10 * ds_list_size(_level))", "17 + (_rowStep * ds_list_size(_level))");
FixLayout("gml_GlobalScript_scr35_DrawThroneUI", "50 + (10 * i)", "50 + (_rowStep * i)");
var design35PointsImports = new UndertaleModLib.Compiler.CodeImportGroup(Data);
design35PointsImports.QueueReplace("gml_GlobalScript_UFO50_CHS_draw_throne_points", """
function UFO50_CHS_draw_throne_points(_x, _y, _value)
{
    var _font = draw_get_font();
    var _number = string(_value);
    var _numberFont = UFO50_CHS_number_font(_number);
    var _halign = draw_get_halign();
    var _valign = draw_get_valign();
    var _label = string_replace(scrString("throne_stat_upgrade_cost"), "{0}", "");
    draw_set_font(_numberFont);
    var _numberWidth = string_width(_number);
    draw_set_halign(fa_left);
    draw_set_valign(fa_middle);
    var _middle = _y + 5;
    draw_text(_x, _middle, _number);
    draw_set_font(_font);
    // Zpix visible glyph center lies 3px below its fa_middle metric.
    // Lift the label from the compensated midpoint; keep original digits centered.
    draw_text(_x + _numberWidth + 4, _middle - 2, _label);
    draw_set_font(_font);
    draw_set_halign(_halign);
    draw_set_valign(_valign);
}
""");
design35PointsImports.Import();
FixLayout("gml_GlobalScript_scr35_DrawThroneUI",
    "    draw_text(camera_get_view_x(view_get_camera(0)) + 222, camera_get_view_y(view_get_camera(0)) + 48, scrStringFormat(scrString(\"throne_stat_upgrade_cost\"), throneSit.sitPoints));",
    "    if (_chs) UFO50_CHS_draw_throne_points(camera_get_view_x(view_get_camera(0)) + 222, camera_get_view_y(view_get_camera(0)) + 48, throneSit.sitPoints);\n    else draw_text(camera_get_view_x(view_get_camera(0)) + 222, camera_get_view_y(view_get_camera(0)) + 48, scrStringFormat(scrString(\"throne_stat_upgrade_cost\"), throneSit.sitPoints));");

// Runtime third-after CHS long dialogue drops its final punctuation: inserting
// a newline between Chinese glyphs increases full-string length after Set.
// The original typewriter completion bound still held the pre-wrap length.
FixLayout("gml_GlobalScript_scrMessageInsertBreaks",
    "    textMessageFull = _msgReassembled;",
    "    textMessageFull = _msgReassembled;\n    if (global.language == global.LANG_JAPANESE) textMessageLength = string_length(textMessageFull);");

// Before CHS real ending slide: score digits use Zpix and variable-width labels
// shift their starting column. Keep the original 16px rows and native digits.
var design45TallyImports = new UndertaleModLib.Compiler.CodeImportGroup(Data);
design45TallyImports.QueueReplace("gml_GlobalScript_UFO50_CHS_draw_owls_tally", """
function UFO50_CHS_draw_owls_tally(_x, _y, _scores)
{
    var _labels = [];
    var _labelWidth = 0;
    var _savedAlign = draw_get_halign();
    draw_set_halign(fa_left);
    for (var _i = 0; _i < 6; _i++)
    {
        var _label = scrString("cutscene_win_summary_" + string(_i + 1));
        var _placeholder = string_pos("{0}", _label);
        if (_placeholder > 0) _label = string_copy(_label, 1, _placeholder - 1);
        _labels[_i] = _label;
        _labelWidth = max(_labelWidth, string_width(_label));
    }
    var _total = 0;
    for (var _i = 0; _i < 5; _i++) _total += _scores[_i];
    for (var _i = 0; _i < 6; _i++)
    {
        var _value = (_i == 5) ? _total : _scores[_i];
        draw_text(_x, _y + 16 * _i, _labels[_i]);
        // Pure-number wrapper restores the caller's original requested font.
        draw_text(_x + _labelWidth + 8, _y + 16 * _i, string(_value));
    }
    draw_set_halign(_savedAlign);
}
""");
design45TallyImports.Import();
FixLayout("gml_Object_o45__VictoryScreenTally_Draw_73",
    "    var _c = oCutsceneControl;",
    "    var _c = oCutsceneControl;\n    if (global.language == global.LANG_JAPANESE)\n    {\n        UFO50_CHS_draw_owls_tally(camera_get_view_x(view_camera[0]) + 120, camera_get_view_y(view_camera[0]) + 112, _c.score_45);\n        exit;\n    }");

// Real standard MOVE2: title/DIAGONAL/SURPRISE/ATTACK overlap at8px.
// CHS keeps title + two13px detail rows in the original40px action slot.
var design50NameImports = new UndertaleModLib.Compiler.CodeImportGroup(Data);
design50NameImports.QueueReplace("gml_GlobalScript_UFO50_CHS_draw_avianos_name", """
function UFO50_CHS_draw_avianos_name(_center, _y, _text)
{
    var _font = draw_get_font();
    var _width = 64;
    if (global.language == global.LANG_JAPANESE)
    {
        draw_set_font(global.fontDefault_CHS);
        _width = 0;
        for (var _i = 1; _i <= string_length(_text); _i++)
        {
            var _char = string_char_at(_text, _i);
            _width += (ord(_char) < 128) ? 8 : max(8, round(string_width(_char)));
        }
        draw_set_font(_font);
    }
    UFO50_CHS_draw_avianos_mixed(_center - floor(_width / 2), _y, _text);
}
""");
design50NameImports.Import();
FixLayout("gml_Object_o50_Game_Draw_0", "        UFO50_CHS_draw_avianos_mixed((HUD_TEXT_CENTER + hudShiftX) - 32, 56, GOD_NAMES[currGod]);",
    "        UFO50_CHS_draw_avianos_name(HUD_TEXT_CENTER + hudShiftX, 56, GOD_NAMES[currGod]);");
FixLayout("gml_Object_o50_Game_Draw_0", "        UFO50_CHS_draw_avianos_mixed((HUD_TEXT_CENTER + hudShiftX) - 32, 56, string_even(GOD_NAMES[god], 4));",
    "        UFO50_CHS_draw_avianos_name(HUD_TEXT_CENTER + hudShiftX, 56, (global.language == global.LANG_JAPANESE) ? GOD_NAMES[god] : string_even(GOD_NAMES[god], 4));");
FixLayout("gml_Object_o50_Game_Draw_0", "                        var yShift = 8;",
    "                        var _chsMove = global.language == global.LANG_JAPANESE;\n                        var _moveStep = _chsMove ? 13 : 8;\n                        var yShift = _moveStep;");
var design50MoveImports = new UndertaleModLib.Compiler.CodeImportGroup(Data);
design50MoveImports.ThrowOnNoOpFindReplace = true;
design50MoveImports.QueueRegexFindReplace(Data.Code.ByName("gml_Object_o50_Game_Draw_0"), @"(?m)^ {28}yShift \+= 8;", "                            yShift += _moveStep;", true);
design50MoveImports.QueueFindReplace("gml_Object_o50_Game_Draw_0", "                                UFO50_CHS_draw_avianos_mixed(HUD_TEXT_LEFT + hudShiftX + 8, yStart + yShift, scrStringExt(\"move_attack_bonus\", \"*\", 8, 0));\n                                yShift += 8;\n                                UFO50_CHS_draw_avianos_mixed(HUD_TEXT_LEFT + hudShiftX + 8, yStart + yShift, scrStringExt(\"move_attack_bonus_2\", \"*\", 8, 0));",
    "                                if (_chsMove)\n                                    UFO50_CHS_draw_avianos_mixed(HUD_TEXT_LEFT + hudShiftX + 8, yStart + yShift, scrString(\"move_attack_bonus\") + scrString(\"move_attack_bonus_2\"));\n                                else\n                                {\n                                    UFO50_CHS_draw_avianos_mixed(HUD_TEXT_LEFT + hudShiftX + 8, yStart + yShift, scrStringExt(\"move_attack_bonus\", \"*\", 8, 0));\n                                    yShift += 8;\n                                    UFO50_CHS_draw_avianos_mixed(HUD_TEXT_LEFT + hudShiftX + 8, yStart + yShift, scrStringExt(\"move_attack_bonus_2\", \"*\", 8, 0));\n                                }", true);
design50MoveImports.Import();

// Fifteenth-before three-language real recruit menu: CHS title touches first
// cost, and the fourth "done" row reaches the following action's border.
// ASCII/icon rows keep their native 8px step; title-to-cost gap becomes 13px.
FixLayout("gml_Object_o50_Game_Draw_0",
    "yStart + (8 * j) + 8",
    "yStart + (8 * j) + (global.language == global.LANG_JAPANESE ? 13 : 8)");
FixLayout("gml_Object_o50_Game_Draw_0",
    "                    var doneY = i;",
    "                    var doneY = i;\n                    var _chsDoneHeader = global.language == global.LANG_JAPANESE;\n                    var _doneHeaderLeft = HUD_TEXT_LEFT + hudShiftX + 48;");
FixLayout("gml_Object_o50_Game_Draw_0",
    "UFO50_CHS_draw_avianos_mixed(HUD_TEXT_LEFT + hudShiftX + 8, ACTION_TOP + (ACTION_HEIGHT * actionIndex) + (8 * doneY) + 8, scrString(\"done\"));",
    "UFO50_CHS_draw_avianos_mixed(_chsDoneHeader ? _doneHeaderLeft : HUD_TEXT_LEFT + hudShiftX + 8, ACTION_TOP + (ACTION_HEIGHT * actionIndex) + (_chsDoneHeader ? 0 : (8 * doneY) + 8), scrString(\"done\"));");
FixLayout("gml_Object_o50_Game_Draw_0", "                    if (i == 3)", "                    if (i == 3 && !_chsDoneHeader)");
// All three choice cursor branches use the same title-row done target.
FixLayout("gml_Object_o50_Game_Draw_0",
    "HUD_TEXT_LEFT + hudShiftX, ACTION_TOP + (ACTION_HEIGHT * actionIndex) + (8 * subY) + 8",
    "HUD_TEXT_LEFT + hudShiftX + ((_chsDoneHeader && subY == doneY) ? 40 : 0), ACTION_TOP + (ACTION_HEIGHT * actionIndex) + ((_chsDoneHeader && subY == doneY) ? 0 : (8 * subY) + (_chsDoneHeader ? 13 : 8))");

// 合入现有ID4黑底/层标签拆数候选之后，顶层文字调用统一重定向之前。
// 白字实机bbox：ID4中文中线205、原数字203；ID7中文层21、原Tall0为23.5。
// 仅调整中文文字，原数字Y、3/3固定黑底与原语言保持。
FixLayout("gml_GlobalScript_scr04_DrawText",
    "var _boxTop = arg1 - (_chsLabel ? 1 : 0);",
    "var _chsTextY = arg1 - (_chsLabel ? 2 : 0);\n    var _boxTop = _chsTextY - (_chsLabel ? 1 : 0);");
FixLayout("gml_GlobalScript_scr04_DrawText",
    "draw_text(arg0, arg1, _str);",
    "draw_text(arg0, _chsTextY, _str);");
var camouflageMidlineGroup = new UndertaleModLib.Compiler.CodeImportGroup(Data);
camouflageMidlineGroup.AutoCreateAssets = true;
camouflageMidlineGroup.QueueReplace("gml_GlobalScript_UFO50_CHS_draw_camouflage_level", """
function UFO50_CHS_draw_camouflage_level(_x, _y, _level)
{
    var _label = scrString("level") + " ";
    var _number = string(_level);
    if (global.language != global.LANG_JAPANESE)
    {
        scr04_DrawText(_x, _y, _label + _number);
        return;
    }
    var _font = draw_get_font();
    var _labelWidth = string_width(_label);
    var _labelHeight = string_height(_label);
    var _numberFont = UFO50_CHS_number_font(_number);
    if (_numberFont >= 0) draw_set_font(_numberFont);
    var _numberWidth = string_width(_number);
    draw_set_font(_font);
    var _color = draw_get_color();
    var _ha = draw_get_halign();
    draw_set_halign(fa_left);
    draw_set_color(c_black);
    draw_rectangle(_x, _y - 3, _x + _labelWidth + _numberWidth - 1, _y + _labelHeight - 4, false);
    draw_set_color(_color);
    UFO50_CHS_draw_text(_x, _y - 2, _label);
    UFO50_CHS_draw_text(_x + _labelWidth, _y, _number);
    draw_set_halign(_ha);
}
""");
camouflageMidlineGroup.Import();

var vaingerMidlineGroup = new UndertaleModLib.Compiler.CodeImportGroup(Data);
vaingerMidlineGroup.AutoCreateAssets = true;
vaingerMidlineGroup.QueueReplace("gml_GlobalScript_UFO50_CHS_draw_vainger_level", """
function UFO50_CHS_draw_vainger_level(_x, _y, _label, _number, _align)
{
    var _labelWidth = string_width(_label);
    var _ha = draw_get_halign();
    draw_set_halign(fa_left);
    // +2px后中文字面中线23，原Tall中线23.5，保留整数像素绘制。
    UFO50_CHS_draw_text(_x, _y + 2, _label);
    UFO50_CHS_draw_text(_x + _labelWidth, _y, _number);
    draw_set_halign(_ha);
}
""");
vaingerMidlineGroup.Import();
FixLayout("gml_GlobalScript_scr07_DrawMapUI",
    "UFO50_CHS_draw_labeled_number(",
    "UFO50_CHS_draw_vainger_level(");
FixLayout("gml_GlobalScript_scr07_DrawModUI",
    "UFO50_CHS_draw_labeled_number(",
    "UFO50_CHS_draw_vainger_level(");

// 第十七轮三语同场已验：G2数值整串Zpix；G8关标题/奖励重叠且标题失去原中心；
// G9 P2源代码直接拼价格，三语均外露*。仅中文分支修布局，其他语言保持原调用。
// 合入UFO50_CHS_draw_fixed_mixed之后、顶层draw_text统一重定向之前。
var mixedResultsShopHelpers = new UndertaleModLib.Compiler.CodeImportGroup(Data);
mixedResultsShopHelpers.AutoCreateAssets = true;
mixedResultsShopHelpers.QueueReplace("gml_GlobalScript_UFO50_CHS_draw_fixed_centered", """
function UFO50_CHS_draw_fixed_centered(_x, _y, _text, _labelDelta)
{
    var _ha = draw_get_halign();
    draw_set_halign(fa_center);
    UFO50_CHS_draw_fixed_mixed(_x, _y, _text, _labelDelta);
    draw_set_halign(_ha);
}
""");
mixedResultsShopHelpers.QueueReplace("gml_GlobalScript_UFO50_CHS_draw_fist_shop", """
function UFO50_CHS_draw_fist_shop(_x, _y, _key, _price)
{
    var _label = string_replace_all(scrStringVal(_key, ""), "$", "");
    var _number = "$" + string(_price);
    var _font = draw_get_font();
    var _numberFont = UFO50_CHS_number_font(_number);
    var _labelWidth = string_width(_label);
    draw_set_font(_numberFont);
    var _numberWidth = string_width(_number);
    draw_set_font(_font);
    var _ha = draw_get_halign();
    draw_set_halign(fa_left);
    var _left = _x - floor((_labelWidth + _numberWidth) / 2);
    // 原Tall数字Y不动，中文姓名向下2px使字面中线齐平。
    UFO50_CHS_draw_text(_left, _y + 2, _label);
    UFO50_CHS_draw_text(_left + _labelWidth, _y, _number);
    draw_set_halign(_ha);
}
""");
mixedResultsShopHelpers.Import();

foreach (var entry in new[] {
    ("scrStringDrawVal(232, 96 + _yshift, \"level_x\", min(SINGLE_PLAYER_LEVELS, highestLevel));", "scrStringVal(\"level_x\", min(SINGLE_PLAYER_LEVELS, highestLevel))", "96 + _yshift"),
    ("scrStringDrawVal(232, 112 + _yshift, \"rank_x\", rank + rankDelta);", "scrStringVal(\"rank_x\", rank + rankDelta)", "112 + _yshift"),
    ("scrStringDrawVal(232, 128 + _yshift, \"points_x\", highScore);", "scrStringVal(\"points_x\", highScore)", "128 + _yshift")
}) FixLayout("gml_Object_o02_Game_Draw_0", entry.Item1,
    "if (global.language == global.LANG_JAPANESE) UFO50_CHS_draw_fixed_mixed(232, " + entry.Item3 + ", " + entry.Item2 + ", -2); else " + entry.Item1);

FixLayout("gml_Object_o08_Mas_Draw_0", "draw_text(136, 160, scrString(\"bonus\"));",
    "if (global.language == global.LANG_JAPANESE) UFO50_CHS_draw_fixed_centered(152, 156, scrString(\"bonus\"), 0); else draw_text(136, 160, scrString(\"bonus\"));");
FixLayout("gml_Object_o08_Mas_Draw_0", "draw_text(160, 16, scrString(\"results\"));",
    "if (global.language == global.LANG_JAPANESE) UFO50_CHS_draw_fixed_centered(192, 16, scrString(\"results\"), 2); else draw_text(160, 16, scrString(\"results\"));");
FixLayout("gml_Object_o08_Mas_Draw_0", "draw_text(144, 192, scrString(\"total\") + strTotal);",
    "if (global.language == global.LANG_JAPANESE) UFO50_CHS_draw_fixed_centered(192, 192, scrString(\"total\") + strTotal, 2); else draw_text(144, 192, scrString(\"total\") + strTotal);");
FixLayout("gml_Object_o08_Mas_Draw_0", "draw_text(xdd + 16, ydd + (56 * i), stageText[i][0]);",
    "if (global.language == global.LANG_JAPANESE)\n        {\n            draw_rectangle_color(xdd + 8, ydd + (56 * i) - 5, (xdd + 80) - 1, (ydd + (56 * i) + 8) - 1, _c, _c, _c, _c, 0);\n            UFO50_CHS_draw_fixed_centered(xdd + 44, ydd + (56 * i) - 4, stageText[i][0], 0);\n        }\n        else draw_text(xdd + 16, ydd + (56 * i), stageText[i][0]);");

foreach (var key in new[] { "shop_weights", "shop_jrope", "shop_headgear" })
{
    var first = "scrDrawTextCenteredPoint(scrStringVal(\"" + key + "\", upgradePrice), _x, _y, 8);";
    var second = "scrDrawTextCenteredPoint(scrString(\"" + key + "\") + string(upgradePrice), _x, _y, 8);";
    var chs = "if (global.language == global.LANG_JAPANESE) UFO50_CHS_draw_fist_shop(_x, _y, \"" + key + "\", upgradePrice); else ";
    FixLayout("gml_Object_o09__Game_Draw_0", first, chs + first);
    FixLayout("gml_Object_o09__Game_Draw_0", second, chs + second);
}

// Private: CHS + original EN/JA same-scene 17before confirmed fixed mixed UI.
// Uses root fixed_mixed layout, original requested digit font and original Y.
var fixed17Imports = new UndertaleModLib.Compiler.CodeImportGroup(Data);
fixed17Imports.QueueReplace("gml_GlobalScript_UFO50_CHS_draw_fixed_mixed_center", """
function UFO50_CHS_draw_fixed_mixed_center(_x,_y,_text,_labelDelta,_ceilHalf)
{
    var _align=draw_get_halign();
    var _width=UFO50_CHS_fixed_mixed_width(_text);
    var _half=_ceilHalf ? ceil(_width/2) : floor(_width/2);
    draw_set_halign(fa_left);
    UFO50_CHS_draw_fixed_mixed(_x-_half,_y,_text,_labelDelta);
    draw_set_halign(_align);
}
""");
fixed17Imports.Import();
FixLayout("gml_Object_o20_Game_Draw_0", "scrStringDrawCenterAltExt(192, 56, \"jobs_completed\", string(job), 26, 4);",
    "if(global.language==global.LANG_JAPANESE) UFO50_CHS_draw_fixed_mixed_center(192,56,scrStringExt(\"jobs_completed\",string(job),26,4),2,false);\n            else scrStringDrawCenterAltExt(192, 56, \"jobs_completed\", string(job), 26, 4);");
FixLayout("gml_Object_o20_Game_Draw_0", "draw_text_ce(192, 88, \"< \" + scrString(\"job\") + \" \" + string(jobSel) + \" >\", 4);",
    "if(global.language==global.LANG_JAPANESE) UFO50_CHS_draw_fixed_mixed_center(192,88,\"< \"+scrString(\"job\")+\" \"+string(jobSel)+\" >\",2,false);\n    else draw_text_ce(192, 88, \"< \" + scrString(\"job\") + \" \" + string(jobSel) + \" >\", 4);");
foreach(var side in new[]{("56","LT"),("328","RT")})
{
    var old=$"draw_text_ce({side.Item1}, 424, scrStringVal(\"win_count\", string(wins[{side.Item2}])), 4);";
    FixLayout("gml_Object_o22_Game_Draw_0",old,$"if(global.language==global.LANG_JAPANESE) UFO50_CHS_draw_fixed_mixed_center({side.Item1},424,scrStringVal(\"win_count\",string(wins[{side.Item2}])),-2,false);\n        else "+old);
}
FixLayout("gml_Object_o29_Game_Draw_0", "draw_text_ce(_xview + 192, _yview + 104, scrStringVal(\"with_x_lives\", myLives + 1), 3);",
    "if(global.language==global.LANG_JAPANESE) UFO50_CHS_draw_fixed_mixed_center(_xview+192,_yview+104,scrStringVal(\"with_x_lives\",myLives+1),-2,false);\n    else draw_text_ce(_xview + 192, _yview + 104, scrStringVal(\"with_x_lives\", myLives + 1), 3);");
FixLayout("gml_Object_o32_Mas_Draw_0", "draw_text(_xNews + 188, _yNews + 16, scrString(\"day\") + string(dayNumber + 1));",
    "if(global.language==global.LANG_JAPANESE) UFO50_CHS_draw_fixed_mixed(_xNews+188,_yNews+16,scrString(\"day\")+string(dayNumber+1),2);\n    else draw_text(_xNews + 188, _yNews + 16, scrString(\"day\") + string(dayNumber + 1));");
FixLayout("gml_Object_o32_Mas_Draw_0", "draw_text_ce(_xv + 240, _yv + 48, scrStringVal(\"intro_day_x\", dayNumber + 1), 4);",
    "if(global.language==global.LANG_JAPANESE) UFO50_CHS_draw_fixed_mixed_center(_xv+240,_yv+48,scrStringVal(\"intro_day_x\",dayNumber+1),2,false);\n        else draw_text_ce(_xv + 240, _yv + 48, scrStringVal(\"intro_day_x\", dayNumber + 1), 4);");
FixLayout("gml_Object_o32_Mas_Draw_0", "scrDrawTextCentered(scrStringVal(\"lives_left\", continues), _x, _y, 8, _w);",
    "if(global.language==global.LANG_JAPANESE) UFO50_CHS_draw_fixed_mixed_center(_x+_w/2,_y,scrStringVal(\"lives_left\",continues),2,false);\n        else scrDrawTextCentered(scrStringVal(\"lives_left\", continues), _x, _y, 8, _w);");
// Both game-over and success summary use these two sites. Counts/icons stay intact.
FixLayout("gml_Object_o32_Mas_Draw_0", "draw_text(_x + 8, _y + (16 * i) + 24, scrStringFormat(scrString(\"day_tally\"), string(i + 1)));",
    "if(global.language==global.LANG_JAPANESE) UFO50_CHS_draw_fixed_mixed(_x+8,_y+(16*i)+24,scrStringFormat(scrString(\"day_tally\"),string(i+1)),-2);\n        else draw_text(_x + 8, _y + (16 * i) + 24, scrStringFormat(scrString(\"day_tally\"), string(i + 1)));");
FixLayout("gml_Object_o32_Mas_Draw_0", "scrDrawTextCenteredPoint(strOnionTotal, _xv + 240, _yv + 168, 8);",
    "if(global.language==global.LANG_JAPANESE) UFO50_CHS_draw_fixed_mixed_center(_xv+240,_yv+168,strOnionTotal,2,true);\n    else scrDrawTextCenteredPoint(strOnionTotal, _xv + 240, _yv + 168, 8);");

// Mini & Max inventory title crosses the white top rule in the original scene.
// Its backing may cross the frame, following the existing title treatment.
FixLayout("gml_Object_o41_Game_Draw_0",
    "draw_text_bg(64, 8, scrString(\"inventory_name\"), 0, 8, 8, false);",
    "if (global.language == global.LANG_JAPANESE)\n        {\n            var _inventoryTitle = scrString(\"inventory_name\");\n            draw_rectangle_color(63, 6, 64 + ceil(string_width(_inventoryTitle)), 8 + ceil(string_height(_inventoryTitle)), c_black, c_black, c_black, c_black, false);\n            draw_text(64, 8, _inventoryTitle);\n        }\n        else draw_text_bg(64, 8, scrString(\"inventory_name\"), 0, 8, 8, false);");

// Driver20 CHS/EN/JA confirms overlapping favorite instructions and off-center filters.
var libraryDesignImports = new UndertaleModLib.Compiler.CodeImportGroup(Data);
libraryDesignImports.QueueReplace("gml_GlobalScript_UFO50_CHS_draw_library_input", """
function UFO50_CHS_draw_library_input(_x, _y, _text, _player, _type, _center)
{
    var _oldDelta = variable_global_exists("chsLibraryInputLabelDelta") ? global.chsLibraryInputLabelDelta : 0;
    global.chsLibraryInputLabelDelta = global.language == global.LANG_JAPANESE ? -1 : 0;
    scrDrawTextInput(_x, _y, _text, _player, _type, _center);
    global.chsLibraryInputLabelDelta = _oldDelta;
}
""");
libraryDesignImports.Import();
// Same-scene detail header: Chinese face extends two pixels below the 9px color strip.
FixLayout("gml_Object_oLibrary_Draw_0",
    "draw_rectangle(_textX, _textY + 1, (_textX + _windowFullWidth) - 17, _textY + 9, false);",
    "draw_rectangle(_textX, _textY + 1, (_textX + _windowFullWidth) - 17, _textY + max(9, ceil(string_height(\"中\")) + 1), false);");
FixLayout("gml_Object_oLibrary_Draw_0", "scrDrawTextInput(248, _TEXT_Y,",
    "UFO50_CHS_draw_library_input(248, _TEXT_Y,");
FixLayout("gml_Object_oLibrary_Draw_0", "draw_text(_textX, _textY - 1, descContent[i]);",
    "if (global.language == global.LANG_JAPANESE) UFO50_CHS_draw_fixed_ascii(_textX, _textY - 1, descContent[i], -2, false);\n                            else draw_text(_textX, _textY - 1, descContent[i]);");
FixLayout("gml_GlobalScript_scrDrawTextInput", "draw_text(xx, yy, char);",
    "var _libraryLabelDelta = variable_global_exists(\"chsLibraryInputLabelDelta\") ? global.chsLibraryInputLabelDelta : 0;\n            draw_text(xx, yy + _libraryLabelDelta, char);");
FixLayout("gml_Object_oLibrary_Draw_0",
    "scrDrawMenuBorder(_xv + 90, _yv + 48, 210, 72);",
    "var _faveStep = global.language == global.LANG_JAPANESE ? max(12, ceil(string_height(\"中\")) + 1) : 8;\n                var _faveLines = string_line_breaks(scrStringVal(\"fave_instructions\", \"[2]\"), 22, 4);\n                var _faveCount = 4;\n                while (_faveCount > 0 && _faveLines[_faveCount - 1] == \"\") _faveCount--;\n                var _faveHeight = global.language == global.LANG_JAPANESE ? max(72, 40 + _faveCount * _faveStep) : 72;\n                scrDrawMenuBorder(_xv + 90, _yv + 48, 210, _faveHeight);");
foreach (var row in new[] { (Y:88, I:1), (Y:96, I:2), (Y:104, I:3) })
    FixLayout("gml_Object_oLibrary_Draw_0",
        $"scrDrawTextInput(_xv + 192, _yv + {row.Y}, str[{row.I}], 0, 0, 2);",
        $"scrDrawTextInput(_xv + 192, _yv + 80 + {row.I} * _faveStep, str[{row.I}], 0, 0, 2);");
FixLayout("gml_Object_oLibrary_Draw_0",
    "var _xLeft = (232 + _xLeftChange) - ((string_length(_sortName) * 8) / 2);",
    "var _sortWidth = global.language == global.LANG_JAPANESE ? string_width(_sortName) : string_length(_sortName) * 8;\n                var _xLeft = (232 + _xLeftChange) - (global.language == global.LANG_JAPANESE ? floor(_sortWidth / 2) : _sortWidth / 2);");
foreach (var key in new[] { "bar_time", "bar_filter" })
{
    var old = key == "bar_time" ? "draw_text(8, 206, scrString(\"bar_time\"));" : "draw_text(8, _TEXT_Y, scrString(\"bar_filter\"));";
    var oldY = key == "bar_time" ? "206" : "_TEXT_Y";
    FixLayout("gml_Object_oLibrary_Draw_0", old,
        $"draw_text(8, global.language == global.LANG_JAPANESE ? _TEXT_Y - 1 : {oldY}, scrString(\"{key}\"));");
}
FixLayout("gml_Object_oLibrary_Draw_0", "draw_text(_xLeft, _TEXT_Y, _sortName);",
    "draw_text(_xLeft, _TEXT_Y - (global.language == global.LANG_JAPANESE ? 1 : 0), _sortName);");
foreach (var key in new[] { "filter_button_type", "filter_button_remix", "filter_button_list", "filter_button_close", "filter_button_times" })
{
    var old = "scrDrawTextInput(_sortButtonX, _TEXT_Y, scrStringVal(\"" + key + "\", \"[2\"), 0, 0, false);";
    FixLayout("gml_Object_oLibrary_Draw_0", old, old.Replace("scrDrawTextInput(", "UFO50_CHS_draw_library_input("));
}

// Same-scene CHS/EN time detail shows native 12 / 12:34:56, but rank12 remains Zpix.
// Preserve native value Y; shift Chinese faces -2 to share the 6px native face center.
var timeDetailImports = new UndertaleModLib.Compiler.CodeImportGroup(Data);
timeDetailImports.QueueReplace("gml_GlobalScript_UFO50_CHS_draw_key_value_row", """
function UFO50_CHS_draw_key_value_row(arg0,arg1,arg2,arg3,arg4)
{
    var _label=string(arg3); var _value=string(arg4);
    var _lineStep=max(8,ceil(string_height("中"))); var _gap=8;
    var _labelWidth=string_width(_label);
    var _valueWidth=UFO50_CHS_fixed_mixed_width(_value);
    draw_set_halign(fa_left);
    UFO50_CHS_draw_text(arg0,arg1-2,_label);
    draw_set_halign(fa_right);
    if(_labelWidth+_gap+_valueWidth<=arg2) {
        UFO50_CHS_draw_fixed_mixed(arg0+arg2,arg1,_value,-2);
        draw_set_halign(fa_left); return _lineStep;
    }
    UFO50_CHS_draw_fixed_mixed(arg0+arg2,arg1+_lineStep,_value,-2);
    draw_set_halign(fa_left); return _lineStep*2;
}
""");
timeDetailImports.Import();

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
// 坎帕内拉 3 的 48×32 小挑战屏幕：保留数字行，按实际中文高度分开标签。
FixLayout("gml_Object_o08_mg_Mas_Draw_0", "draw_text_centered(centerX, centerY + 8, scrString(",
    "draw_text_centered(centerX, centerY + ((global.language == global.LANG_JAPANESE) ? max(8, ceil(string_height(\"中\"))) : 8), scrString(");
FixLayout("gml_Object_o08_mg_Mas_Draw_0", "draw_text_centered(centerX, centerY, scrString(\"mg_score\"), 8);",
    "draw_text_centered(centerX, centerY + ((global.language == global.LANG_JAPANESE) ? max(0, ceil(string_height(\"中\")) - 8) : 0), scrString(\"mg_score\"), 8);");
FixLayout("gml_Object_o08_mg_Mas_Draw_0", "draw_text_centered(centerX, centerY, \"  ",
    "draw_text_centered(centerX, centerY + ((global.language == global.LANG_JAPANESE) ? max(0, ceil(string_height(\"中\")) - 8) : 0), \"  ");
FixLayout("gml_Object_o08_mg_Mas_Draw_0", "centerY + 16, c_black, c_black, c_black, c_black, 0);",
    "centerY + ((global.language == global.LANG_JAPANESE) ? 2 * max(8, ceil(string_height(\"中\"))) : 16), c_black, c_black, c_black, c_black, 0);");

// 弹球高尔夫的结果表头：编号和洞名保持独立两行。
FixLayout("gml_Object_o23_Mas_Draw_0", "draw_text(xs + 24, ys + 16, _stageName);",
    "draw_text(xs + 24, ys + 8 + ((global.language == global.LANG_JAPANESE) ? max(8, ceil(string_height(\"中\"))) : 8), _stageName);");
// 点阵遮罩为原版大字体设计；中文祝贺语在遮罩之后绘制。
FixLayout("gml_Object_o23_Mas_Draw_0", "scrStringDrawCenter(\"congrats\", _xview, _yview + 16, 20, 384);",
    "if (global.language != global.LANG_JAPANESE) scrStringDrawCenter(\"congrats\", _xview, _yview + 16, 20, 384);");
FixLayout("gml_Object_o23_Mas_Draw_0", "draw_sprite(s23_DMgrid, 0, _xview + 320, _yview);",
    "draw_sprite(s23_DMgrid, 0, _xview + 320, _yview);\n    if (global.language == global.LANG_JAPANESE) scrStringDrawCenter(\"congrats\", _xview, _yview + 16, 20, 384);");

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

// 17before CHS body face rows160..169 /171..180; their 1px shadows
// share row170. EN/JA8px original metrics do not have this CJK overlap.
// Frame bottom native149, text area through215. Shift CJK body160→156,
// then 12px steps: last of5 faces204..213, shadow ends214.
// Run AFTER the baseline news _newsStep patch, before draw-call redirection.
var newsStepCode=Data.Code.ByName("gml_Object_o32_Mas_Draw_0");
var newsStepSource=GetDecompiledText(newsStepCode,new GlobalDecompileContext(Data));
var newsStepPattern=@"var\s+_newsStep\s*=\s*[^;]*;";
var newsYPattern=@"var\s+_y\s*=\s*_yNews\s*\+\s*136\s*\+\s*[^;]*\b_newsStep\b[^;]*;";
if(System.Text.RegularExpressions.Regex.Matches(newsStepSource,newsStepPattern).Count!=1
 || System.Text.RegularExpressions.Regex.Matches(newsStepSource,newsYPattern).Count!=1)
 throw new System.Exception("Expected the baseline news step and exactly one news row coordinate.");
newsStepSource=System.Text.RegularExpressions.Regex.Replace(newsStepSource,newsStepPattern,
 "var _newsStep = (global.language == global.LANG_JAPANESE) ? max(12, ceil(string_height(\"中\")) + 1) : 8;");
newsStepSource=System.Text.RegularExpressions.Regex.Replace(newsStepSource,newsYPattern,
 "var _y = _yNews + ((global.language == global.LANG_JAPANESE) ? 132 : 136) + (i * _newsStep);");
var newsStepImports=new UndertaleModLib.Compiler.CodeImportGroup(Data);
newsStepImports.QueueReplace(newsStepCode,newsStepSource);newsStepImports.Import();

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

// Grimstone shop: preserve complete stat IDs instead of ambiguous one/two-letter labels.
FixLayout("gml_Object_o12__Game_Draw_0", """
                        if (n < 0)
                        {
                            draw_set_colour(#E03C32);
                            draw_text(viewx + 280 + 48, viewy + 64 + 24 + (16 * j), string(n));
                        }
                        else
                        {
                            draw_set_colour(#63B31D);
                            draw_text(viewx + 280 + 48, viewy + 64 + 24 + (16 * j), "+" + string(n));
                        }
                        if (abs(n) > 9)
                        {
                            if (isWeapon)
                            {
                                draw_text(viewx + 280 + 48 + 24, viewy + 64 + 24 + (16 * j), scrString("stat_atk_shortest"));
                            }
                            else if (isNecklace)
                            {
                                draw_text(viewx + 280 + 48 + 24, viewy + 64 + 24 + (16 * j), scrString("stat_evd_shortest"));
                            }
                            else if (isArmor)
                            {
                                draw_text(viewx + 280 + 48 + 24, viewy + 64 + 24 + (16 * j), scrString("stat_def_shortest"));
                            }
                            else if (isShoes)
                            {
                                draw_text(viewx + 280 + 48 + 24, viewy + 64 + 24 + (16 * j), scrString("stat_spd_shortest"));
                            }
                        }
                        else if (isWeapon)
                        {
                            draw_text(viewx + 280 + 48 + 16, viewy + 64 + 24 + (16 * j), scrString("stat_atk_short"));
                        }
                        else if (isNecklace)
                        {
                            draw_text(viewx + 280 + 48 + 16, viewy + 64 + 24 + (16 * j), scrString("stat_evd_short"));
                        }
                        else if (isArmor)
                        {
                            draw_text(viewx + 280 + 48 + 16, viewy + 64 + 24 + (16 * j), scrString("stat_def_short"));
                        }
                        else if (isShoes)
                        {
                            draw_text(viewx + 280 + 48 + 16, viewy + 64 + 24 + (16 * j), scrString("stat_spd_short"));
                        }
""", """
                        if (global.language == global.LANG_JAPANESE)
                        {
                            // Full stat identifiers share the original 8px grid with the delta.
                            // Right alignment leaves room for the longest three-character name.
                            var _statAbbrev = "SPD";
                            if (isWeapon) _statAbbrev = "ATK";
                            else if (isNecklace) _statAbbrev = "EVD";
                            else if (isArmor) _statAbbrev = "DEF";
                            var _deltaText = ((n > 0) ? "+" : "") + string(n) + _statAbbrev;
                            var _statFont = draw_get_font();
                            draw_set_font(global.fontGrimstone);
                            draw_set_halign(fa_right);
                            draw_set_colour((n < 0) ? #E03C32 : #63B31D);
                            draw_text(viewx + 368, viewy + 88 + (16 * j), _deltaText);
                            draw_set_halign(fa_left);
                            draw_set_font(_statFont);
                        }
                        else
                        {
                        if (n < 0)
                        {
                            draw_set_colour(#E03C32);
                            draw_text(viewx + 280 + 48, viewy + 64 + 24 + (16 * j), string(n));
                        }
                        else
                        {
                            draw_set_colour(#63B31D);
                            draw_text(viewx + 280 + 48, viewy + 64 + 24 + (16 * j), "+" + string(n));
                        }
                        if (abs(n) > 9)
                        {
                            if (isWeapon)
                            {
                                draw_text(viewx + 280 + 48 + 24, viewy + 64 + 24 + (16 * j), scrString("stat_atk_shortest"));
                            }
                            else if (isNecklace)
                            {
                                draw_text(viewx + 280 + 48 + 24, viewy + 64 + 24 + (16 * j), scrString("stat_evd_shortest"));
                            }
                            else if (isArmor)
                            {
                                draw_text(viewx + 280 + 48 + 24, viewy + 64 + 24 + (16 * j), scrString("stat_def_shortest"));
                            }
                            else if (isShoes)
                            {
                                draw_text(viewx + 280 + 48 + 24, viewy + 64 + 24 + (16 * j), scrString("stat_spd_shortest"));
                            }
                        }
                        else if (isWeapon)
                        {
                            draw_text(viewx + 280 + 48 + 16, viewy + 64 + 24 + (16 * j), scrString("stat_atk_short"));
                        }
                        else if (isNecklace)
                        {
                            draw_text(viewx + 280 + 48 + 16, viewy + 64 + 24 + (16 * j), scrString("stat_evd_short"));
                        }
                        else if (isArmor)
                        {
                            draw_text(viewx + 280 + 48 + 16, viewy + 64 + 24 + (16 * j), scrString("stat_def_short"));
                        }
                        else if (isShoes)
                        {
                            draw_text(viewx + 280 + 48 + 16, viewy + 64 + 24 + (16 * j), scrString("stat_spd_short"));
                        }
                        }
""");

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

// Seventeenth same-scene CHS/EN/JA audit: fixed UI mixed numerals retain caller sprite font.
// Requires the shared UFO50_CHS_fixed_mixed_layout/width/draw helpers.
void FixMixed3551Regex(string codeName, string pattern, string replacement)
{
    var code = Data.Code.ByName(codeName);
    var source = GetDecompiledText(code, new UndertaleModLib.Decompiler.GlobalDecompileContext(Data));
    var count = System.Text.RegularExpressions.Regex.Matches(source, pattern).Count;
    if (count != 1) throw new System.Exception($"Mixed3551 anchor expected once: {codeName}, matches={count}");
    var group = new UndertaleModLib.Compiler.CodeImportGroup(Data);
    group.ThrowOnNoOpFindReplace = true;
    group.QueueRegexFindReplace(code, pattern, replacement, true);
    group.Import();
}
FixLayout("gml_GlobalScript_UFO50_CHS_draw_valbrace_shop", "string_width(scrStringFormat(scrString(\"crone_trade_format\"), \"\", scr35_ItemCost(_item)))", "UFO50_CHS_fixed_mixed_width(scrStringFormat(scrString(\"crone_trade_format\"), \"\", scr35_ItemCost(_item)))");
FixMixed3551Regex("gml_GlobalScript_UFO50_CHS_draw_valbrace_shop", "(?<!\\w)(?:UFO50_CHS_)?draw_text\\([^;\\n]+_rowY,\\s*scrStringFormat\\(scrString\\(\"crone_trade_format\"\\),\\s*\"\",\\s*scr35_ItemCost\\(_item\\)\\)\\);", "UFO50_CHS_draw_fixed_mixed((_xv + 80 + _w) - 12, _rowY, scrStringFormat(scrString(\"crone_trade_format\"), \"\", scr35_ItemCost(_item)));");
FixLayout("gml_GlobalScript_UFO50_CHS_draw_valbrace_shop", "string_width(_balance) + 16", "UFO50_CHS_fixed_mixed_width(_balance) + 16");
FixMixed3551Regex("gml_GlobalScript_UFO50_CHS_draw_valbrace_shop", "(?<!\\w)(?:UFO50_CHS_)?draw_text\\([^;\\n]+_balanceY\\s*\\+\\s*8,\\s*_balance\\);", "UFO50_CHS_draw_fixed_mixed(_xv + 96, _balanceY + 8, _balance);");
FixLayout("gml_GlobalScript_scr36_DrawTextBG", "    scrSetFont(global.fontDefault);", "    scrSetFont(global.fontDefault);\n    if (global.language == global.LANG_JAPANESE && UFO50_CHS_number_font(arg2) < 0)\n    {\n        var _chsBGColor = draw_get_color();\n        var _chsBGWidth = UFO50_CHS_fixed_mixed_width(arg2);\n        var _chsBGHeight = max(8, ceil(string_height(\"中\")));\n        draw_set_color(arg3);\n        draw_rectangle(arg0, arg1, arg0 + _chsBGWidth - 1, arg1 + _chsBGHeight - 1, false);\n        draw_set_color(_chsBGColor);\n        UFO50_CHS_draw_fixed_mixed(arg0, arg1 + 1, arg2);\n        exit;\n    }");
FixLayout("gml_Object_o36_Game_Draw_0", "metaStr = scrStringExt(\"p1_won_last_x\", scenarioWinsP1[i], 16, 0);", "metaStr = global.language == global.LANG_JAPANESE ? scrStringVal(\"p1_won_last_x\", scenarioWinsP1[i]) : scrStringExt(\"p1_won_last_x\", scenarioWinsP1[i], 16, 0);");
FixLayout("gml_Object_o36_Game_Draw_0", "metaStr = scrStringExt(\"p2_won_last_x\", scenarioWinsP2[i], 16, 0);", "metaStr = global.language == global.LANG_JAPANESE ? scrStringVal(\"p2_won_last_x\", scenarioWinsP2[i]) : scrStringExt(\"p2_won_last_x\", scenarioWinsP2[i], 16, 0);");
FixLayout("gml_Object_o36_Game_Draw_0", "draw_text(232, 56, scrStringVal(\"best_streak\", longestStreak));", "if (global.language == global.LANG_JAPANESE) UFO50_CHS_draw_fixed_mixed(232, 56, scrStringVal(\"best_streak\", longestStreak));\n        else draw_text(232, 56, scrStringVal(\"best_streak\", longestStreak));");
FixLayout("gml_Object_o42_Game_Draw_0", "draw_text_centered(192, 120, scrStringVal(\"team_x_wins\", endState + 1), 8);", "if (global.language == global.LANG_JAPANESE)\n    {\n        var _chsMixedAlign = draw_get_halign();\n        draw_set_halign(fa_center);\n        UFO50_CHS_draw_fixed_mixed(192, 120, scrStringVal(\"team_x_wins\", endState + 1));\n        draw_set_halign(_chsMixedAlign);\n    }\n    else draw_text_centered(192, 120, scrStringVal(\"team_x_wins\", endState + 1), 8);");
FixLayout("gml_GlobalScript_scr45_UIStageSelect", "draw_text(_xview + 272, (_yview + 216) - 43, scrStringFormat(scrString(\"score_curr\"), string(score_45[sel])));", "if (global.language == global.LANG_JAPANESE) UFO50_CHS_draw_fixed_mixed(_xview + 272, (_yview + 216) - 43, scrStringFormat(scrString(\"score_curr\"), string(score_45[sel])));\n                else draw_text(_xview + 272, (_yview + 216) - 43, scrStringFormat(scrString(\"score_curr\"), string(score_45[sel])));");
FixLayout("gml_GlobalScript_scr45_UIStageSelect", "draw_text(_xview + 272, (_yview + 216) - 43, scrStringFormat(scrString(\"score_best\"), string(best_45[sel])));", "if (global.language == global.LANG_JAPANESE) UFO50_CHS_draw_fixed_mixed(_xview + 272, (_yview + 216) - 43, scrStringFormat(scrString(\"score_best\"), string(best_45[sel])));\n                else draw_text(_xview + 272, (_yview + 216) - 43, scrStringFormat(scrString(\"score_best\"), string(best_45[sel])));");
FixLayout("gml_GlobalScript_scr45_UIStageSelect", "draw_text(_xview + 272, (_yview + 216) - 19, scrStringFormat(scrString(\"score_full\"), _totalScore));", "if (global.language == global.LANG_JAPANESE) UFO50_CHS_draw_fixed_mixed(_xview + 272, (_yview + 216) - 19, scrStringFormat(scrString(\"score_full\"), _totalScore));\n                else draw_text(_xview + 272, (_yview + 216) - 19, scrStringFormat(scrString(\"score_full\"), _totalScore));");
FixLayout("gml_Object_o49_Game_Draw_0", "draw_text_centered(192, 64, levelString, 16);", "if (global.language == global.LANG_JAPANESE)\n    {\n        var _chsMixedAlign = draw_get_halign();\n        draw_set_halign(fa_center);\n        UFO50_CHS_draw_fixed_mixed(192, 64, levelString);\n        draw_set_halign(_chsMixedAlign);\n    }\n    else draw_text_centered(192, 64, levelString, 16);");
FixLayout("gml_Object_o49_Game_Draw_0", "draw_text_ce(192, 88, scrStringVal(\"goal_percent\", paintGoal), 4);", "if (global.language == global.LANG_JAPANESE)\n    {\n        var _chsMixedAlign = draw_get_halign();\n        draw_set_halign(fa_center);\n        UFO50_CHS_draw_fixed_mixed(192, 88, scrStringVal(\"goal_percent\", paintGoal));\n        draw_set_halign(_chsMixedAlign);\n    }\n    else draw_text_ce(192, 88, scrStringVal(\"goal_percent\", paintGoal), 4);");
FixLayout("gml_Object_o49_Game_Draw_0", "draw_text_ce(192, 144, scrStringVal(\"score\", totalScore), 4);", "if (global.language == global.LANG_JAPANESE)\n    {\n        var _chsMixedAlign = draw_get_halign();\n        draw_set_halign(fa_center);\n        UFO50_CHS_draw_fixed_mixed(192, 144, scrStringVal(\"score\", totalScore));\n        draw_set_halign(_chsMixedAlign);\n    }\n    else draw_text_ce(192, 144, scrStringVal(\"score\", totalScore), 4);");
FixLayout("gml_Object_o49_Game_Draw_0", "draw_text_ce(192, 136, scoreString, 4);", "if (global.language == global.LANG_JAPANESE)\n    {\n        var _chsMixedAlign = draw_get_halign();\n        draw_set_halign(fa_center);\n        UFO50_CHS_draw_fixed_mixed(192, 136, scoreString);\n        draw_set_halign(_chsMixedAlign);\n    }\n    else draw_text_ce(192, 136, scoreString, 4);");
FixLayout("gml_Object_o49_Game_Draw_0", "draw_text_ce(192, 64, scrStringVal(\"score\", totalScore), 4);", "if (global.language == global.LANG_JAPANESE)\n    {\n        var _chsMixedAlign = draw_get_halign();\n        draw_set_halign(fa_center);\n        UFO50_CHS_draw_fixed_mixed(192, 64, scrStringVal(\"score\", totalScore));\n        draw_set_halign(_chsMixedAlign);\n    }\n    else draw_text_ce(192, 64, scrStringVal(\"score\", totalScore), 4);");
FixLayout("gml_Object_o49_Game_Draw_0", "draw_text_ce(192, 88, \"< \" + scrStringVal(\"level\", levelSel) + \" >\", 4);", "if (global.language == global.LANG_JAPANESE)\n    {\n        var _chsMixedAlign = draw_get_halign();\n        draw_set_halign(fa_center);\n        UFO50_CHS_draw_fixed_mixed(192, 88, \"< \" + scrStringVal(\"level\", levelSel) + \" >\");\n        draw_set_halign(_chsMixedAlign);\n    }\n    else draw_text_ce(192, 88, \"< \" + scrStringVal(\"level\", levelSel) + \" >\", 4);");
FixLayout("gml_Object_o49_Game_Draw_0", "draw_text_bg_centered(192, 96, readyString, 0, 8, 8, true);", "if (global.language == global.LANG_JAPANESE)\n        {\n            var _chsReadyFont = draw_get_font();\n            var _chsReadyAlign = draw_get_halign();\n            var _chsReadyColor = draw_get_color();\n            var _chsReadyLayout = UFO50_CHS_fixed_mixed_layout(readyString);\n            var _chsReadyX = 192 - floor(_chsReadyLayout.total / 2);\n            var _chsReadyHeight = max(8, ceil(string_height(\"中\")));\n            var _chsReadyCursor = _chsReadyX;\n            for (var _r = 0; _r < array_length(_chsReadyLayout.parts); _r++)\n            {\n                var _chsReadyRun = _chsReadyLayout.parts[_r];\n                draw_set_font(_chsReadyFont);\n                if (_chsReadyLayout.numeric[_r]) draw_set_font(UFO50_CHS_number_font(_chsReadyRun));\n                for (var _q = 1; _q <= string_length(_chsReadyRun); _q++)\n                {\n                    var _chsReadyChar = string_char_at(_chsReadyRun, _q);\n                    var _chsReadyCharWidth = string_width(_chsReadyChar);\n                    if (_chsReadyChar != \" \")\n                    {\n                        draw_set_color(c_black);\n                        draw_rectangle(_chsReadyCursor, 95, _chsReadyCursor + _chsReadyCharWidth - 1, 96 + _chsReadyHeight - 1, false);\n                    }\n                    _chsReadyCursor += _chsReadyCharWidth;\n                }\n            }\n            draw_set_font(_chsReadyFont);\n            draw_set_color(_chsReadyColor);\n            draw_set_halign(fa_left);\n            UFO50_CHS_draw_fixed_mixed(_chsReadyX, 96, readyString);\n            draw_set_halign(_chsReadyAlign);\n        }\n        else draw_text_bg_centered(192, 96, readyString, 0, 8, 8, true);");
FixLayout("gml_Object_o49_Game_Draw_0", "    draw_set_color(global.palette[13]);\n    draw_text_ce(192, 192, scrString(\"final_showdown\") + \": \" + string(levelScore[26]), 4);\n    draw_set_color(c_white);\n    var _coverUpText = scrString(\"final_showdown\") + \": \";\n    repeat (string_length(string(levelScore[26])))\n    {\n        _coverUpText += \" \";\n    }\n    draw_text_ce(192, 192, _coverUpText, 4);", "    if (global.language == global.LANG_JAPANESE)\n    {\n        var _chsFinalLabel = scrString(\"final_showdown\") + \": \";\n        var _chsFinalText = _chsFinalLabel + string(levelScore[26]);\n        var _chsFinalX = 192 - floor(UFO50_CHS_fixed_mixed_width(_chsFinalText) / 2);\n        var _chsFinalAlign = draw_get_halign();\n        draw_set_halign(fa_left);\n        draw_set_color(global.palette[13]);\n        UFO50_CHS_draw_fixed_mixed(_chsFinalX, 192, _chsFinalText);\n        draw_set_color(c_white);\n        UFO50_CHS_draw_fixed_mixed(_chsFinalX, 192, _chsFinalLabel);\n        draw_set_halign(_chsFinalAlign);\n    }\n    else\n    {\n    draw_set_color(global.palette[13]);\n    draw_text_ce(192, 192, scrString(\"final_showdown\") + \": \" + string(levelScore[26]), 4);\n    draw_set_color(c_white);\n    var _coverUpText = scrString(\"final_showdown\") + \": \";\n    repeat (string_length(string(levelScore[26])))\n    {\n        _coverUpText += \" \";\n    }\n    draw_text_ce(192, 192, _coverUpText, 4);\n    }");

// 第十八轮原英日中全部八职业27天赋、四阶段Starwasp同场已验。
// 固定等级和结算数字沿原请求fontGrimstone/Tall/Default，原数值Y保留。
// 合入固定混排/居中helper之后，顶层重定向之前。
foreach (var perk in new[] { "melee", "pistol", "shotgun", "rifle", "heavy", "doc", "spirit", "survive" })
{
    var field = "perk" + perk.ToUpperInvariant();
    var list = "draw_text(viewx + 280, viewy + 24 + (16 * n), scrString(\"perk_" + perk + "\") + string(party[currPlayer]." + field + "));";
    var title = "draw_text(viewx + tx, viewy + ty + 8 + 8 + 16, scrString(\"perk_" + perk + "_01\") + string(party[currPlayer]." + field + ") + \":\");";
    FixLayout("gml_Object_o12__Game_Draw_0", list,
        "if (global.language == global.LANG_JAPANESE) UFO50_CHS_draw_fixed_mixed(viewx + 280, viewy + 24 + (16 * n), scrString(\"perk_" + perk + "\") + string(party[currPlayer]." + field + "), -2); else " + list);
    FixLayout("gml_Object_o12__Game_Draw_0", title,
        "if (global.language == global.LANG_JAPANESE) UFO50_CHS_draw_fixed_mixed(viewx + tx, viewy + ty + 8 + 8 + 16, scrString(\"perk_" + perk + "_01\") + string(party[currPlayer]." + field + ") + \":\", -2); else " + title);
}
var starwaspMixedHelpers = new UndertaleModLib.Compiler.CodeImportGroup(Data);
starwaspMixedHelpers.AutoCreateAssets = true;
starwaspMixedHelpers.QueueReplace("gml_GlobalScript_UFO50_CHS_draw_starwasp_fixed", """
function UFO50_CHS_draw_starwasp_fixed(_centerX, _y, _text, _labelDelta)
{
    var _font = draw_get_font();
    var _numberFont = UFO50_CHS_number_font("0");
    var _ha = draw_get_halign();
    var _parts = []; var _numeric = []; var _widths = [];
    var _run = ""; var _isNumber = false; var _total = 0;
    for (var _i = 1; _i <= string_length(_text) + 1; _i++)
    {
        var _char = _i <= string_length(_text) ? string_char_at(_text, _i) : "";
        // 固定模板的ASCII空格含原string_even填充，全部按请求字体8px格保留。
        var _nextNumber = _char != "" && string_pos(_char, " 0123456789.:,+-/") > 0;
        if (_run != "" && (_nextNumber != _isNumber || _char == ""))
        {
            var _n = array_length(_parts); _parts[_n] = _run; _numeric[_n] = _isNumber;
            draw_set_font(_isNumber ? _numberFont : _font);
            _widths[_n] = string_width(_run); _total += _widths[_n]; _run = "";
        }
        _isNumber = _nextNumber; _run += _char;
    }
    draw_set_halign(fa_left);
    var _x = _centerX - floor(_total / 2);
    for (var _j = 0; _j < array_length(_parts); _j++)
    {
        draw_set_font(_numeric[_j] ? _numberFont : _font);
        draw_text(_x, _y + (_numeric[_j] ? 0 : _labelDelta), _parts[_j]);
        _x += _widths[_j];
    }
    draw_set_font(_font); draw_set_halign(_ha);
}
""");
starwaspMixedHelpers.Import();

FixLayout("gml_Object_o17__Game_Draw_0", "scrStringDrawExt(dispWaveX, dispWaveY, \"wave_x\", string(wave), 0, 4);",
    "if (global.language == global.LANG_JAPANESE) UFO50_CHS_draw_starwasp_fixed(192, dispWaveY, scrStringExt(\"wave_x\", string(wave), 0, 4), 2); else scrStringDrawExt(dispWaveX, dispWaveY, \"wave_x\", string(wave), 0, 4);");
// 三个TIME分支、两个LIVES分支及一个BONUS分支，源坐标和原动画时序保持。
foreach (var yOffset in new[] { 16, 32, 48 })
{
    var call = "draw_text_centered(192, dispWaveY + " + yOffset + ", str, 8);";
    FixLayout("gml_Object_o17__Game_Draw_0", call,
        "if (global.language == global.LANG_JAPANESE) UFO50_CHS_draw_starwasp_fixed(192, dispWaveY + " + yOffset + ", str, -2); else " + call);
}

// Private proposal: same-scene CHS/EN/JA captures checked before changing.
// Waldorf retains the original 40px label slot, 3x8px number slot and x72 icon.
var fixed18Imports = new UndertaleModLib.Compiler.CodeImportGroup(Data);
fixed18Imports.QueueReplace("gml_GlobalScript_UFO50_CHS_draw_waldorf_record", """
function UFO50_CHS_draw_waldorf_record(_x,_y,_label,_value)
{
    var _align=draw_get_halign();
    draw_set_halign(fa_left);
    draw_text(_x,_y-2,_label);
    draw_text(_x+40,_y,_value);
    draw_set_halign(_align);
}
""");
fixed18Imports.QueueReplace("gml_GlobalScript_UFO50_CHS_draw_blok_warp", """
function UFO50_CHS_draw_blok_warp(_x,_y,_text)
{
    var _align=draw_get_halign();
    var _color=draw_get_color();
    var _width=UFO50_CHS_fixed_mixed_width(_text);
    var _left=_x-floor(_width/2);
    draw_set_color(c_black);
    draw_rectangle(_left,_y,_left+_width-1,_y+15,false);
    draw_set_color(_color);
    draw_set_halign(fa_left);
    UFO50_CHS_draw_fixed_mixed(_left,_y,_text,2);
    draw_set_halign(_align);
}
""");
fixed18Imports.Import();
foreach(var number in new[]{"highestPercent","highestShells"})
{
    var old=$"draw_text(xview + 8, yview + 8, scrStringExt(\"best_score\", 0, 5, 0) + string_format({number}, 3, 0));";
    FixLayout("gml_Object_o21_Game_Draw_0",old,$"if(global.language==global.LANG_JAPANESE) UFO50_CHS_draw_waldorf_record(xview+8,yview+8,scrStringExt(\"best_score\",0,5,0),string_format({number},3,0));\n            else "+old);
}
FixLayout("gml_Object_o28_Mas_Draw_0","draw_text(xv + 32, yv + 0, rStr);",
    "if(global.language==global.LANG_JAPANESE) UFO50_CHS_draw_fixed_mixed(xv+32,yv,rStr,2);\n    else draw_text(xv + 32, yv + 0, rStr);");
FixLayout("gml_Object_o18__Game_Draw_0","draw_text_bg_centered(_xv + 192, (_yv + 216) - 32, scrStringExt(\"warp_to_level\", string(obj.level), 42, 4), 0, 8, 16, false);",
    "if(global.language==global.LANG_JAPANESE) UFO50_CHS_draw_blok_warp(_xv+192,(_yv+216)-32,scrStringExt(\"warp_to_level\",string(obj.level),42,4));\n                else draw_text_bg_centered(_xv + 192, (_yv + 216) - 32, scrStringExt(\"warp_to_level\", string(obj.level), 42, 4), 0, 8, 16, false);");

// Eighteenth CHS/EN/JA real scenes: map stage, match/player/result counts and life bonus.
// Shared mixed helper required. Pure padded scores and existing map-name layout retained.
FixLayout("gml_Object_o38_Mas_Draw_0", "draw_text(_xview + 160, _yview + 16, _stage_num_string);", "if (global.language == global.LANG_JAPANESE)\n    {\n        var _chsMixed3843Align = draw_get_halign();\n        draw_set_halign(fa_center);\n        UFO50_CHS_draw_fixed_mixed(_xview + 192, _yview + 16, _stage_num_string);\n        draw_set_halign(_chsMixed3843Align);\n    }\n    else draw_text(_xview + 160, _yview + 16, _stage_num_string);");
FixLayout("gml_Object_o43_Game_Draw_0", "scrStringDrawCenterExt(\"game_x\", 0, 96, 8, 384, game, 24, 4);", "if (global.language == global.LANG_JAPANESE)\n    {\n        var _chsMixed3843Align = draw_get_halign();\n        draw_set_halign(fa_center);\n        UFO50_CHS_draw_fixed_mixed(192, 96, scrStringExt(\"game_x\", game, 24, 4));\n        draw_set_halign(_chsMixed3843Align);\n    }\n    else scrStringDrawCenterExt(\"game_x\", 0, 96, 8, 384, game, 24, 4);");
FixLayout("gml_Object_o43_Game_Draw_0", "scrStringDrawCenterExt(\"red_team_wins\", 0, 96, 8, 384, 0, 24, 4);", "if (global.language == global.LANG_JAPANESE)\n    {\n        var _chsMixed3843Align = draw_get_halign();\n        draw_set_halign(fa_center);\n        UFO50_CHS_draw_fixed_mixed(192, 96, scrStringExt(\"red_team_wins\", 0, 24, 4));\n        draw_set_halign(_chsMixed3843Align);\n    }\n    else scrStringDrawCenterExt(\"red_team_wins\", 0, 96, 8, 384, 0, 24, 4);");
FixLayout("gml_Object_o43_Game_Draw_0", "scrStringDrawCenterExt(\"blue_team_wins\", 0, 96, 8, 384, 0, 24, 4);", "if (global.language == global.LANG_JAPANESE)\n    {\n        var _chsMixed3843Align = draw_get_halign();\n        draw_set_halign(fa_center);\n        UFO50_CHS_draw_fixed_mixed(192, 96, scrStringExt(\"blue_team_wins\", 0, 24, 4));\n        draw_set_halign(_chsMixed3843Align);\n    }\n    else scrStringDrawCenterExt(\"blue_team_wins\", 0, 96, 8, 384, 0, 24, 4);");
FixLayout("gml_Object_o43_Game_Draw_0", "scrStringDrawCenterExt(\"num_blowouts\", 0, 192, 8, 384, blowouts, 24, 4);", "if (global.language == global.LANG_JAPANESE)\n    {\n        var _chsMixed3843Align = draw_get_halign();\n        draw_set_halign(fa_center);\n        UFO50_CHS_draw_fixed_mixed(192, 192, scrStringExt(\"num_blowouts\", blowouts, 24, 4));\n        draw_set_halign(_chsMixed3843Align);\n    }\n    else scrStringDrawCenterExt(\"num_blowouts\", 0, 192, 8, 384, blowouts, 24, 4);");
FixLayout("gml_Object_o47_LevelText_Draw_0", "draw_text_ce(room_width / 2, (room_height / 2) - 16, leveltext, 4);", "if (global.language == global.LANG_JAPANESE)\n    {\n        var _chsMixed3843Align = draw_get_halign();\n        draw_set_halign(fa_center);\n        UFO50_CHS_draw_fixed_mixed(room_width / 2, (room_height / 2) - 16, leveltext);\n        draw_set_halign(_chsMixed3843Align);\n    }\n    else draw_text_ce(room_width / 2, (room_height / 2) - 16, leveltext, 4);");
FixLayout("gml_Object_o47_Player_Draw_0", "scrStringDrawCE(x, y - 32, \"p2_explanation\", 0);", "if (global.language == global.LANG_JAPANESE)\n    {\n        var _chsMixed3843Align = draw_get_halign();\n        draw_set_halign(fa_center);\n        UFO50_CHS_draw_fixed_mixed(x, y - 32, scrStringExt(\"p2_explanation\", \"*\", 0, 0));\n        draw_set_halign(_chsMixed3843Align);\n    }\n    else scrStringDrawCE(x, y - 32, \"p2_explanation\", 0);");
FixLayout("gml_Object_o47_RisingText_Draw_0", "sw = string_width(text) / 2;", "var _chsLifeBonus = global.language == global.LANG_JAPANESE && text != scrString(\"level_up\") && UFO50_CHS_number_font(text) < 0;\nsw = (_chsLifeBonus ? UFO50_CHS_fixed_mixed_width(text) : string_width(text)) / 2;");
FixLayout("gml_Object_o47_RisingText_Draw_0", "draw_text_ce(xx, yy, text, 4);", "if (_chsLifeBonus)\n    {\n        var _chsMixed3843Align = draw_get_halign();\n        draw_set_halign(fa_center);\n        UFO50_CHS_draw_fixed_mixed(xx, yy, text);\n        draw_set_halign(_chsMixed3843Align);\n    }\n    else draw_text_ce(xx, yy, text, 4);");

// 20 三语同场before已逐张看；局部固定字段候选，待父编译和after。
// 放在顶层draw_text重定向之前；UFO50_CHS_draw_fixed_ascii 必须加入 wrapperCodes 排除。
// 原字体ASCII直接draw_text：完整A-1、空格、星号也保留原Y与度量。
var verified20Helpers = new UndertaleModLib.Compiler.CodeImportGroup(Data);
verified20Helpers.AutoCreateAssets = true;
verified20Helpers.QueueReplace("gml_GlobalScript_UFO50_CHS_fixed_ascii_layout", """
function UFO50_CHS_fixed_ascii_layout(_text)
{
    var _font=draw_get_font();
    var _original=global.chsRequestedFont;
    var _parts=[]; var _ascii=[]; var _widths=[]; var _total=0;
    var _run=""; var _wasAscii=false;
    for(var _i=1;_i<=string_length(_text)+1;_i++) {
        var _c=(_i<=string_length(_text))?string_char_at(_text,_i):"";
        var _isAscii=_c!="" && ord(_c)<128;
        if(_run!="" && (_isAscii!=_wasAscii || _c=="")) {
            var _n=array_length(_parts); _parts[_n]=_run; _ascii[_n]=_wasAscii;
            draw_set_font(_wasAscii?_original:_font);
            _widths[_n]=string_width(_run); _total+=_widths[_n]; _run="";
        }
        _wasAscii=_isAscii; _run+=_c;
    }
    draw_set_font(_font);
    return {parts:_parts,ascii:_ascii,widths:_widths,total:_total,original:_original};
}
""");
verified20Helpers.QueueReplace("gml_GlobalScript_UFO50_CHS_draw_fixed_ascii", """
function UFO50_CHS_draw_fixed_ascii(_x,_y,_text,_labelDelta,_center)
{
    var _font=draw_get_font(); var _ha=draw_get_halign();
    var _layout=UFO50_CHS_fixed_ascii_layout(_text);
    if(_center) _x-=floor(_layout.total/2);
    else if(_ha==fa_center) _x-=floor(_layout.total/2);
    else if(_ha==fa_right) _x-=_layout.total;
    draw_set_halign(fa_left);
    for(var _i=0;_i<array_length(_layout.parts);_i++) {
        if(_layout.ascii[_i]) {
            draw_set_font(_layout.original);
            draw_text(_x,_y,_layout.parts[_i]);
        } else {
            draw_set_font(_font);
            UFO50_CHS_draw_text(_x,_y+_labelDelta,_layout.parts[_i]);
        }
        _x+=_layout.widths[_i];
    }
    draw_set_font(_font); draw_set_halign(_ha);
}
""");
verified20Helpers.QueueReplace("gml_GlobalScript_UFO50_CHS_draw_fixed_ascii_bg", """
function UFO50_CHS_draw_fixed_ascii_bg(_x,_y,_text,_labelDelta,_height)
{
    var _layout=UFO50_CHS_fixed_ascii_layout(_text);
    var _color=draw_get_color();
    draw_set_color(c_black);
    draw_rectangle(_x-floor(_layout.total/2),_y,_x-floor(_layout.total/2)+_layout.total-1,_y+_height-1,false);
    draw_set_color(_color);
    UFO50_CHS_draw_fixed_ascii(_x,_y,_text,_labelDelta,true);
}
""");
verified20Helpers.QueueReplace("gml_GlobalScript_UFO50_CHS_draw_ufo3_title", """
function UFO50_CHS_draw_ufo3_title(_label,_name)
{
    UFO50_CHS_draw_fixed_ascii(64,8,_label,0,true);
    scrSetFont(global.fontTall);
    var _numbered=false;
    for(var _i=1;_i<=string_length(_name);_i++)
        if(string_pos(string_char_at(_name,_i),"0123456789")>0) _numbered=true;
    var _y=_numbered?24:20;
    var _delta=_numbered?2:0;
    var _layout=UFO50_CHS_fixed_ascii_layout(_name);
    var _left=64-floor(_layout.total/2);
    UFO50_CHS_draw_fixed_ascii(64,_y,_name,_delta,true);
    scrSetFont(global.fontDefault);
    var _starLayout=UFO50_CHS_fixed_ascii_layout("*");
    UFO50_CHS_draw_fixed_ascii(_left-_starLayout.total-4,_y+_delta,"*",0,false);
    UFO50_CHS_draw_fixed_ascii(_left+_layout.total+4,_y+_delta,"*",0,false);
}
""");
verified20Helpers.Import();

FixLayout("gml_Object_o01_Game_Draw_0",
    "scrDrawTextCentered(str, _xv, (_yv + 108) - 8, 8, 384);",
    "if(global.language==global.LANG_JAPANESE) UFO50_CHS_draw_fixed_ascii(_xv+192,(_yv+108)-8,str,2,true); else scrDrawTextCentered(str, _xv, (_yv + 108) - 8, 8, 384);");
FixLayout("gml_Object_o03_Mas_Draw_0",
    "draw_text_bg_centered(_xview + 192, _yview + 104, _string, 0, 8, 16, false);",
    "if(global.language==global.LANG_JAPANESE && room!=rm03_UFO_w6) UFO50_CHS_draw_fixed_ascii_bg(_xview+192,_yview+104,_string,2,16); else draw_text_bg_centered(_xview + 192, _yview + 104, _string, 0, 8, 16, false);");
FixLayout("gml_GlobalScript_scr07_DrawDeathUI", "scrDrawTextCentered(_string2, arg0, arg1 + 108 + 16, 8, 384);",
    "if(global.language==global.LANG_JAPANESE) UFO50_CHS_draw_fixed_ascii(arg0+192,arg1+108+16,_string2,2,true); else scrDrawTextCentered(_string2, arg0, arg1 + 108 + 16, 8, 384);");
FixLayout("gml_Object_o08_mg_Mas_Draw_0",
    "draw_text_centered(centerX, centerY - 8, string(names[gamesOrdered[game]]), 8);",
    "if(global.language==global.LANG_JAPANESE) UFO50_CHS_draw_fixed_ascii(centerX,centerY-8,string(names[gamesOrdered[game]]),-2,true); else draw_text_centered(centerX, centerY - 8, string(names[gamesOrdered[game]]), 8);");

FixLayout("gml_Object_o09__Game_Draw_0",
    "scrDrawTextCenteredPoint(\"SHOP\", _xv + 192, _yv + 8, 8);",
    "if(global.language==global.LANG_JAPANESE) UFO50_CHS_draw_fixed_ascii(_xv+192,_yv+8,\"商店\",2,true); else scrDrawTextCenteredPoint(\"SHOP\", _xv + 192, _yv + 8, 8);");
FixLayout("gml_Object_o09__Game_Draw_0",
    "scrDrawTextCenteredPoint(scrString(\"shop_burger\"), _x, _y, 8);",
    "if(global.language==global.LANG_JAPANESE) UFO50_CHS_draw_fixed_ascii(_x,_y,scrString(\"shop_burger\"),2,true); else scrDrawTextCenteredPoint(scrString(\"shop_burger\"), _x, _y, 8);");
foreach(var entry in new[]{("p1_ready","_xv + 96"),("p2_ready","_xv + 96 + 192")}) {
    var old="scrDrawTextCenteredPoint(scrString(\""+entry.Item1+"\"), "+entry.Item2+", (_yv + 96) - 24, 8);";
    FixLayout("gml_Object_o09__Game_Draw_0",old,
        "if(global.language==global.LANG_JAPANESE) UFO50_CHS_draw_fixed_ascii("+entry.Item2+",(_yv+96)-24,scrString(\""+entry.Item1+"\"),2,true); else "+old);
}
FixLayout("gml_Object_o10_Game_Draw_73", "draw_text(_box_x + 16, _box_y + 8, str_equipment_choose);",
    "if(global.language==global.LANG_JAPANESE) UFO50_CHS_draw_fixed_ascii(_box_x+16,_box_y+8,str_equipment_choose,-2,false); else draw_text(_box_x + 16, _box_y + 8, str_equipment_choose);");
FixLayout("gml_Object_o10_Game_Draw_73", "draw_text(_box_x + 112, _box_y + 8, str_equipment_equipped);",
    "draw_text(_box_x+112,_box_y+8-((global.language==global.LANG_JAPANESE)?2:0),str_equipment_equipped);");
FixLayout("gml_Object_o10_Game_Draw_73", "draw_text_ext(_box_x + 16, _box_y + 80, _desc[itemSelect], 8, 168);",
    "if(global.language==global.LANG_JAPANESE && _desc[itemSelect]==scrString(\"itemsub_armorsys\")) UFO50_CHS_draw_fixed_ascii(_box_x+16,_box_y+80,_desc[itemSelect],-2,false); else draw_text_ext(_box_x + 16, _box_y + 80, _desc[itemSelect], 8, 168);");
FixLayout("gml_Object_o13_Game_Draw_0", "draw_text_ce(midX, 176, scrStringVal(\"earns_star\", drawLevel.starGoal), 2);",
    "if(global.language==global.LANG_JAPANESE) UFO50_CHS_draw_fixed_ascii(midX,176,string_even(scrStringVal(\"earns_star\",drawLevel.starGoal),1),-2,true); else draw_text_ce(midX, 176, scrStringVal(\"earns_star\", drawLevel.starGoal), 2);");
FixLayout("gml_Object_o13_Game_Draw_0", "draw_text_centered(midX, 192, str, 8);",
    "if(global.language==global.LANG_JAPANESE) UFO50_CHS_draw_fixed_ascii(midX,192,str,-2,true); else draw_text_centered(midX, 192, str, 8);");
FixLayout("gml_Object_o13_Game_Draw_0", "draw_text_ce(viewx + 192, viewy + 72, scrStringVal(\"ending_stars\", endingStars, NUM_LEVELS * 3), 4);",
    "if(global.language==global.LANG_JAPANESE) UFO50_CHS_draw_fixed_ascii(viewx+192,viewy+72,string_even(scrStringVal(\"ending_stars\",endingStars,NUM_LEVELS*3),3),-2,true); else draw_text_ce(viewx + 192, viewy + 72, scrStringVal(\"ending_stars\", endingStars, NUM_LEVELS * 3), 4);");
FixLayout("gml_Object_o16_Mas_Draw_0",
    "draw_rectangle_color(_xview, _yview + 176, _xview + 384, _yview + 192, _col, _col, _col, _col, 0);",
    "draw_rectangle_color(_xview,_yview+((global.language==global.LANG_JAPANESE)?168:176),_xview+384,_yview+((global.language==global.LANG_JAPANESE)?196:192),_col,_col,_col,_col,0);");
foreach(var sprite in new[]{("s16_Coffee","56"),("s16_Capsule","72")}) {
    var old="draw_sprite("+sprite.Item1+", 0, _xview + "+sprite.Item2+", _yview + 176);";
    FixLayout("gml_Object_o16_Mas_Draw_0",old,"draw_sprite("+sprite.Item1+",0,_xview+"+sprite.Item2+",_yview+((global.language==global.LANG_JAPANESE)?172:176));");
}
FixLayout("gml_Object_o16_Mas_Draw_0", "draw_text(_xview + 112, _yview + 184, strTime);",
    "if(global.language==global.LANG_JAPANESE) UFO50_CHS_draw_fixed_ascii(_xview+112,_yview+184,strTime,0,false); else draw_text(_xview + 112, _yview + 184, strTime);");
FixLayout("gml_Object_o16_Mas_Draw_0", "draw_text(_xview + 96, _yview + 176, strDestination);",
    "draw_text(_xview+96,_yview+((global.language==global.LANG_JAPANESE)?172:176),strDestination);");

// 在fix-twentieth-verified-fields.csx之后追加；20完整三语before已验。
FixLayout("gml_Object_o09__Game_Draw_0", "draw_text(_xv + 16, (_yv + 216) - 16, gameOverMessage);",
    "if(global.language==global.LANG_JAPANESE && survivalMode) UFO50_CHS_draw_fixed_ascii(_xv+16,(_yv+216)-16,gameOverMessage,-2,false); else draw_text(_xv + 16, (_yv + 216) - 16, gameOverMessage);");
FixLayout("gml_Object_o14_Game_Draw_0", "scrDrawTextCentered(string(floor(stateTimer / 6)) + \" / \" + string(NUM_FAKE_RACES) + \" \" + scrString(\"races\"), viewx, viewy + 120, 8, 384);",
    "if(global.language==global.LANG_JAPANESE) UFO50_CHS_draw_fixed_ascii(viewx+192,viewy+120,string(floor(stateTimer/6))+\" / \"+string(NUM_FAKE_RACES)+\" \"+scrString(\"races\"),-2,true); else scrDrawTextCentered(string(floor(stateTimer / 6)) + \" / \" + string(NUM_FAKE_RACES) + \" \" + scrString(\"races\"), viewx, viewy + 120, 8, 384);");

// Private proposal. Before CHS/EN/JA JPG and original event changes reviewed.
// Append after existing 17/18 snippets and before native draw redirection.
var fixed20Imports = new UndertaleModLib.Compiler.CodeImportGroup(Data);
fixed20Imports.AutoCreateAssets = true;
fixed20Imports.QueueReplace("gml_GlobalScript_UFO50_CHS_draw_fixed_short_bg", """
function UFO50_CHS_draw_fixed_short_bg(_x,_y,_text,_labelDelta)
{
    var _align=draw_get_halign();
    var _color=draw_get_color();
    var _width=UFO50_CHS_fixed_mixed_width(_text);
    var _left=_x-floor(_width/2);
    var _offset=is_undefined(_labelDelta) ? -2 : _labelDelta;
    draw_set_color(c_black);
    draw_rectangle(_left,_y+_offset-1,_left+_width-1,_y+_offset+10,false);
    draw_set_color(_color);
    draw_set_halign(fa_left);
    UFO50_CHS_draw_fixed_mixed(_left,_y,_text,_offset);
    draw_set_halign(_align);
}
""");
fixed20Imports.QueueReplace("gml_GlobalScript_UFO50_CHS_draw_mortol_level_id", """
function UFO50_CHS_draw_mortol_level_id(_x,_y,_id)
{
    // Both callers use CHS + fa_left. The shared raw ASCII helper preserves
    // the original requested font and Y for the entire 1-A identifier.
    UFO50_CHS_draw_fixed_ascii(_x,_y,_id,0,false);
}
""");
fixed20Imports.QueueReplace("gml_GlobalScript_UFO50_CHS_draw_mortol_level_intro", """
function UFO50_CHS_draw_mortol_level_intro(_x,_y,_label,_id)
{
    var _font=draw_get_font();
    var _align=draw_get_halign();
    var _original=UFO50_CHS_number_font("0");
    var _labelWidth=string_width(_label);
    if(_original>=0) draw_set_font(_original);
    var _valueWidth=string_width(_id);
    var _gap=string_width(" ");
    draw_set_font(_font);
    var _left=_x-floor((_labelWidth+_gap+_valueWidth)/2);
    draw_set_halign(fa_left);
    draw_text(_left,_y-2,_label);
    UFO50_CHS_draw_mortol_level_id(_left+_labelWidth+_gap,_y,_id);
    draw_set_halign(_align);
}
""");
fixed20Imports.Import();
foreach(var key in new[]{"battle_over_1","battle_over_2"})
{
    var old=$"draw_text_bg_centered(xview + 192, yview + 96, scrStringExt(\"{key}\", 0, 0, 3), 0, 8, 8, false);";
    FixLayout("gml_Object_o21_Game_Draw_0",old,$"if(global.language==global.LANG_JAPANESE) UFO50_CHS_draw_fixed_short_bg(xview+192,yview+92,scrStringExt(\"{key}\",0,0,3));\n        else "+old);
}
FixLayout("gml_Object_o25__Game_Draw_0",
    "scrDrawTextCentered(scrStringVal(\"respawn_in_x\", string(ceil(respawnCount[0] / 60))), camera_get_view_x(view_get_camera(0)), (camera_get_view_y(view_get_camera(0)) + 216) - 24, 8, 384);",
    "if(global.language==global.LANG_JAPANESE) UFO50_CHS_draw_fixed_mixed_center(camera_get_view_x(view_get_camera(0))+192,camera_get_view_y(view_get_camera(0))+192,scrStringVal(\"respawn_in_x\",string(ceil(respawnCount[0]/60))),2,false);\n        else scrDrawTextCentered(scrStringVal(\"respawn_in_x\", string(ceil(respawnCount[0] / 60))), camera_get_view_x(view_get_camera(0)), (camera_get_view_y(view_get_camera(0)) + 216) - 24, 8, 384);");
FixLayout("gml_Object_o33_Game_Draw_0",
    "draw_text_bg_centered(viewx + 192, viewy + 104, scrStringVal(\"timeout_warning\", countDown), 0, 8, 8, false);",
    "if(global.language==global.LANG_JAPANESE) UFO50_CHS_draw_fixed_short_bg(viewx+192,viewy+104,scrStringVal(\"timeout_warning\",countDown),0);\n            else draw_text_bg_centered(viewx + 192, viewy + 104, scrStringVal(\"timeout_warning\", countDown), 0, 8, 8, false);");
FixLayout("gml_Object_o23_Mas_Draw_0","draw_text(xs + 24, ys + 8, _stageNumber);",
    "if(global.language==global.LANG_JAPANESE) UFO50_CHS_draw_fixed_mixed(xs+24,ys+6,_stageNumber,-2);\n        else draw_text(xs + 24, ys + 8, _stageNumber);");
// Replace the existing header-height rule rather than stacking an extra offset.
FixLayout("gml_Object_o23_Mas_Draw_0",
    "draw_text(xs + 24, ys + 8 + ((global.language == global.LANG_JAPANESE) ? max(8, ceil(string_height(\"中\"))) : 8), _stageName);",
    "draw_text(xs + 24, ys + 16, _stageName);");
FixLayout("gml_Object_o23_Mas_Draw_0","draw_text(xs + 40, ys + 48 + (16 * i), _name);",
    "if(global.language==global.LANG_JAPANESE)\n            {\n                if(_contestant==0 || (_contestant==1 && global.numPlayers==2)) UFO50_CHS_draw_fixed_mixed(xs+40,ys+48+(16*i),_name,-2);\n                else draw_text(xs+40,ys+46+(16*i),_name);\n            }\n            else draw_text(xs + 40, ys + 48 + (16 * i), _name);");
FixLayout("gml_Object_o29_Game_Draw_0",
    "var _levelLabel = scrString(\"level\") + \" \" + levelNum;\n    scrDrawTextCentered((global.language == global.LANG_JAPANESE) ? _levelLabel : string_even(_levelLabel, 3), 0, 72, 8, 384);",
    "var _levelLabel = scrString(\"level\") + \" \" + levelNum;\n    if(global.language==global.LANG_JAPANESE) UFO50_CHS_draw_mortol_level_intro(192,72,scrString(\"level\"),levelNum);\n    else scrDrawTextCentered(string_even(_levelLabel,3),0,72,8,384);");
FixLayout("gml_Object_o29_Game_Draw_0",
    "draw_text(_xview + _levelX, _yview + 72 + (8 * i), string(levelNum));",
    "if(global.language==global.LANG_JAPANESE) UFO50_CHS_draw_mortol_level_id(_xview+_levelX,_yview+72+(8*i),string(levelNum));\n        else draw_text(_xview + _levelX, _yview + 72 + (8 * i), string(levelNum));");

// Private proposal. Same-scene CHS/EN/JA complete JPG + numeric closeups reviewed.
// Preserve original source routes in EN; append before native draw redirection.
FixLayout("gml_Object_o22_Game_Draw_0",
    "draw_text(_x, _y + 8 + (i * 16), _optionname[i]);",
    "if(global.language==global.LANG_JAPANESE) UFO50_CHS_draw_fixed_mixed(_x,_y+8+(i*16),_optionname[i],-2);\n        else draw_text(_x, _y + 8 + (i * 16), _optionname[i]);");
FixLayout("gml_Object_o22_Game_Draw_0",
    "draw_sprite(s22_HandP1, 0, _x + string_width(_optionname[i]) + 6, _y + 2 + (i * 16));",
    "var _optionWidth=global.language==global.LANG_JAPANESE ? UFO50_CHS_fixed_mixed_width(_optionname[i]) : string_width(_optionname[i]);\n            draw_sprite(s22_HandP1,0,_x+_optionWidth+6,_y+2+(i*16));");
FixLayout("gml_Object_o28_Mas_Draw_0",
    "scrStringDrawCenterAltExt(xv + 192, yv + 56, \"race_x\", string_replace_all(string_format(currStage + 1, 2, 0), \" \", \"0\"), 12, 4);",
    "if(global.language==global.LANG_JAPANESE) UFO50_CHS_draw_fixed_mixed_center(xv+192,yv+56,scrStringExt(\"race_x\",string_replace_all(string_format(currStage+1,2,0),\" \",\"0\"),12,4),-2,false);\n        else scrStringDrawCenterAltExt(xv + 192, yv + 56, \"race_x\", string_replace_all(string_format(currStage + 1, 2, 0), \" \", \"0\"), 12, 4);");
FixLayout("gml_Object_o21_Game_Draw_0",
    "var leftX = (384 - (string_length(winMsg) * 8)) / 2;",
    "var leftX = (global.language==global.LANG_JAPANESE) ? 192-floor(UFO50_CHS_fixed_mixed_width(winMsg)/2) : (384-(string_length(winMsg)*8))/2;\n    var _chsRainbowX=leftX;");
FixLayout("gml_Object_o21_Game_Draw_0",
    "draw_text(xview + leftX + ((i - 1) * 8), yview + 64, string_char_at(winMsg, i));",
    "if(global.language==global.LANG_JAPANESE)\n        {\n            var _rainbowChar=string_char_at(winMsg,i);\n            UFO50_CHS_draw_fixed_mixed(xview+_chsRainbowX,yview+64,_rainbowChar,2);\n            _chsRainbowX+=UFO50_CHS_fixed_mixed_width(_rainbowChar);\n        }\n        else draw_text(xview + leftX + ((i - 1) * 8), yview + 64, string_char_at(winMsg, i));");

// 第21三语before已逐图核对；在已冻结第20片段之后、顶层重定向之前追加。
// 原数字/ASCII raw Y不动；仅已取景的分支。修改后同场与原交互待验。
// 固定数值原字体/Y
FixLayout("gml_Object_o14_Game_Draw_0","draw_text(viewx + 72, viewy + 32 + (68 * i) + 24, scrString(\"wins\") + \": \" + string(currQ[i].wins) + \" / \" + string(currQ[i].races));","if(global.language==global.LANG_JAPANESE) UFO50_CHS_draw_fixed_ascii(viewx+72,viewy+32+(68*i)+24,scrString(\"wins\")+\": \"+string(currQ[i].wins)+\" / \"+string(currQ[i].races),-2,false); else draw_text(viewx + 72, viewy + 32 + (68 * i) + 24, scrString(\"wins\") + \": \" + string(currQ[i].wins) + \" / \" + string(currQ[i].races));");
// 完整原ASCII姓名G-14/UNT 9
FixLayout("gml_Object_o14_Game_Draw_0","draw_text_centered(viewx + 184, viewy + 4 + global.jpvoff, scrStringExt(\"whose_turn\", name[currPlayer], 0, 2), 8);","if(global.language==global.LANG_JAPANESE) UFO50_CHS_draw_fixed_ascii(viewx+184,viewy+4+global.jpvoff,scrStringExt(\"whose_turn\",name[currPlayer],0,2),-2,true); else draw_text_centered(viewx + 184, viewy + 4 + global.jpvoff, scrStringExt(\"whose_turn\", name[currPlayer], 0, 2), 8);");
// 已验主/打手/信息/训练画面原ASCII姓名
FixLayout("gml_Object_o14_Game_Draw_0","draw_text_centered(viewx + 184, viewy + 4, scrStringExt(\"whose_turn\", name[currPlayer], 0, 2), 8);\n    draw_text_bg_centered(viewx + 56, viewy + 142, scrStringEven(\"thug\", 4), 0, 8, 8, true);","if(global.language==global.LANG_JAPANESE) UFO50_CHS_draw_fixed_ascii(viewx+184,viewy+4,scrStringExt(\"whose_turn\",name[currPlayer],0,2),-2,true); else draw_text_centered(viewx + 184, viewy + 4, scrStringExt(\"whose_turn\", name[currPlayer], 0, 2), 8);\n    draw_text_bg_centered(viewx + 56, viewy + 142, scrStringEven(\"thug\", 4), 0, 8, 8, true);");
// 已验主/打手/信息/训练画面原ASCII姓名
FixLayout("gml_Object_o14_Game_Draw_0","draw_text_centered(viewx + 184, viewy + 4, scrStringExt(\"whose_turn\", name[currPlayer], 0, 2), 8);\n    draw_text_bg_centered(viewx + 56, viewy + 142, scrStringExt(\"infobot\", 0, 10, 2), 0, 8, 8, true);","if(global.language==global.LANG_JAPANESE) UFO50_CHS_draw_fixed_ascii(viewx+184,viewy+4,scrStringExt(\"whose_turn\",name[currPlayer],0,2),-2,true); else draw_text_centered(viewx + 184, viewy + 4, scrStringExt(\"whose_turn\", name[currPlayer], 0, 2), 8);\n    draw_text_bg_centered(viewx + 56, viewy + 142, scrStringExt(\"infobot\", 0, 10, 2), 0, 8, 8, true);");
// 已验主/打手/信息/训练画面原ASCII姓名
FixLayout("gml_Object_o14_Game_Draw_0","draw_text_centered(viewx + 184, viewy + 4, scrStringExt(\"whose_turn\", name[currPlayer], 0, 2), 8);\n    draw_text_bg_centered(viewx + 56, viewy + 142, scrStringExt(\"trainer\", 0, 10, 2), 0, 8, 8, true);","if(global.language==global.LANG_JAPANESE) UFO50_CHS_draw_fixed_ascii(viewx+184,viewy+4,scrStringExt(\"whose_turn\",name[currPlayer],0,2),-2,true); else draw_text_centered(viewx + 184, viewy + 4, scrStringExt(\"whose_turn\", name[currPlayer], 0, 2), 8);\n    draw_text_bg_centered(viewx + 56, viewy + 142, scrStringExt(\"trainer\", 0, 10, 2), 0, 8, 8, true);");
// 训练三行12px分组
FixLayout("gml_Object_o14_Game_Draw_0","draw_text(1704, 320 - (40 * i), string(stable[i].name));","draw_text(1704,((global.language==global.LANG_JAPANESE)?314:320)-(40*i),string(stable[i].name));");
// 训练速度59/60原Default，数字Y328不动
FixLayout("gml_Object_o14_Game_Draw_0","draw_text(1704, 328 - (40 * i), scrString(\"speed\") + \": \" + string(stable[i].qSpeed));","if(global.language==global.LANG_JAPANESE) UFO50_CHS_draw_fixed_ascii(1704,328-(40*i),scrString(\"speed\")+\": \"+string(stable[i].qSpeed),-2,false); else draw_text(1704, 328 - (40 * i), scrString(\"speed\") + \": \" + string(stable[i].qSpeed));");
// 训练三行12px分组
FixLayout("gml_Object_o14_Game_Draw_0","draw_text(1704, 336 - (40 * i), cs);","draw_text(1704,((global.language==global.LANG_JAPANESE)?338:336)-(40*i),cs);");
// 规则换算=4原Grimstone ASCII与金币图标
FixLayout("gml_Object_o13_Game_Draw_0","draw_text(192, 96, \"= 4\");","if(global.language==global.LANG_JAPANESE) UFO50_CHS_draw_fixed_ascii(192,96,\"= 4\",0,false); else draw_text(192, 96, \"= 4\");");
// 规则换算尾括号原ASCII
FixLayout("gml_Object_o13_Game_Draw_0","draw_text(232, 96, \")\");","if(global.language==global.LANG_JAPANESE) UFO50_CHS_draw_fixed_ascii(232,96,\")\",0,false); else draw_text(232, 96, \")\");");
// 坚持10回合数字原Grimstone
FixLayout("gml_Object_o13_Game_Draw_0","draw_text(80, 144, scrStringVal(\"rules_content_5\", TURN_COUNT_2P));","if(global.language==global.LANG_JAPANESE) UFO50_CHS_draw_fixed_ascii(80,144,scrStringVal(\"rules_content_5\",TURN_COUNT_2P),-2,false); else draw_text(80, 144, scrStringVal(\"rules_content_5\", TURN_COUNT_2P));");
// 双人规则12/9原FancyShort与补位
FixLayout("gml_Object_o13_Game_Draw_0","draw_text_ce(midX, 192, str, 2);","if(global.language==global.LANG_JAPANESE) UFO50_CHS_draw_fixed_ascii(midX,192,string_even(str,1),-2,true); else draw_text_ce(midX, 192, str, 2);");
// 12秒原Grimstone
FixLayout("gml_Object_o13_Game_Draw_0","draw_text_ce(centerX, viewy + 96, scrStringVal(\"seconds_left\", floor(matchTimer / 60)), 4);","if(global.language==global.LANG_JAPANESE) UFO50_CHS_draw_fixed_ascii(centerX,viewy+96,string_even(scrStringVal(\"seconds_left\",floor(matchTimer/60)),3),-2,true); else draw_text_ce(centerX, viewy + 96, scrStringVal(\"seconds_left\", floor(matchTimer / 60)), 4);");
// NPC三行保留40px黑条，中文12px，顶178至底213
FixLayout("gml_Object_o16_Npc_Draw_0","draw_text(xBar + 64, yBar + 8 + (i * 8), textPart[i]);","draw_text(xBar+64,yBar+((global.language==global.LANG_JAPANESE)?(2+i*12):(8+i*8)),textPart[i]);");

// 第21高分表三语before已验：姓名录入/原Step光标遮罩/完成。
// 冻结第20 fixed_ascii helper之后、顶层draw_text重定向之前追加。
// 姓名全部ASCII沿原请求字体、原Y、8px格；原8x8遮罩保持。
FixLayout("gml_Object_oHighscore_Draw_0","draw_text(xv + 32, yv + (16 * i), hstable[i][0]);","if(global.language==global.LANG_JAPANESE) UFO50_CHS_draw_fixed_ascii(xv+32,yv+(16*i),hstable[i][0],0,false); else draw_text(xv + 32, yv + (16 * i), hstable[i][0]);");

// 第22打手/信息列表同源最长中文姓名三语before和原Down/取消已验。
// fixed_ascii helper之后、顶层draw_text重定向前。序号沿原Default原Y，中文标签-2。
// 原BET图标len*8与中文实际宽不同；恢复原ASCII后按真实混排宽保留原8px尾隙。
FixLayout("gml_Object_o14_Game_Draw_0","draw_text(viewx + 80, viewy + 80 + (16 * i), string(i + 1) + \". \" + currQ[i].name);","if(global.language==global.LANG_JAPANESE) UFO50_CHS_draw_fixed_ascii(viewx+80,viewy+80+(16*i),string(i+1)+\". \"+currQ[i].name,-2,false); else draw_text(viewx + 80, viewy + 80 + (16 * i), string(i + 1) + \". \" + currQ[i].name);");
FixLayout("gml_Object_o14_Game_Draw_0","draw_text(viewx + 64 + 16, viewy + 64 + 16 + (16 * i), string(i + 1) + \". \" + currQ[i].name);","if(global.language==global.LANG_JAPANESE) UFO50_CHS_draw_fixed_ascii(viewx+80,viewy+80+(16*i),string(i+1)+\". \"+currQ[i].name,-2,false); else draw_text(viewx + 64 + 16, viewy + 64 + 16 + (16 * i), string(i + 1) + \". \" + currQ[i].name);");
FixLayout("gml_Object_o14_Game_Draw_0","draw_sprite(s14_BetIcon, 0, viewx + 88 + (len * 8), viewy + 80 + (16 * i));","var _betX=viewx+88+(len*8);\n                if(global.language==global.LANG_JAPANESE) {var _layout=UFO50_CHS_fixed_ascii_layout(string(i+1)+\". \"+currQ[i].name); _betX=viewx+88+_layout.total;}\n                draw_sprite(s14_BetIcon,0,_betX,viewy+80+(16*i));");

// Private proposal: driver22 cleared the stale original oTextBlink opening.
// CHS/EN/JA loss, win and shop-number full JPG + digit crops reviewed.
FixLayout("gml_Object_o30_Game_Draw_0",
    "scrStringDrawExt(THREAT_TEXT_X, THREAT_TEXT_Y, \"fight_num\", lvl, 10, 0);",
    "if(global.language==global.LANG_JAPANESE) UFO50_CHS_draw_fixed_mixed(THREAT_TEXT_X,THREAT_TEXT_Y,scrStringExt(\"fight_num\",lvl,10,0),-2);\n        else scrStringDrawExt(THREAT_TEXT_X, THREAT_TEXT_Y, \"fight_num\", lvl, 10, 0);");
FixLayout("gml_Object_o30_Game_Draw_0",
    "scrStringDrawCenterAltExt(192, 96, \"player_loss\", controller, 20, 4);",
    "if(global.language==global.LANG_JAPANESE) UFO50_CHS_draw_fixed_mixed_center(192,96,scrStringExt(\"player_loss\",controller,20,4),2,false);\n            else scrStringDrawCenterAltExt(192, 96, \"player_loss\", controller, 20, 4);");
FixLayout("gml_Object_o30_Game_Draw_0",
    "scrStringDrawCenterAltExt(192, 88, \"player_win\", controller, 20, 4);",
    "if(global.language==global.LANG_JAPANESE) UFO50_CHS_draw_fixed_mixed_center(192,88,scrStringExt(\"player_win\",controller,20,4),2,false);\n        else scrStringDrawCenterAltExt(192, 88, \"player_win\", controller, 20, 4);");

// Driver22 CHS/EN/JA same terminal: code, grid and MAIN.UFO header are fixed ASCII.
// Keep the original Tall/Default face, Y, grid, mask and sprite positions.
FixLayout("gml_Object_oPauseMenu_Draw_0", "draw_text(_xview + tx, _yview + ty, codeDispFull);",
    "if(global.language==global.LANG_JAPANESE) UFO50_CHS_draw_fixed_ascii(_xview+tx,_yview+ty,codeDispFull,0,false); else draw_text(_xview + tx, _yview + ty, codeDispFull);");
FixLayout("gml_Object_oPauseMenu_Draw_0", "draw_text(_xview + tx + (charSpaceH * i), _yview + ty + (16 * j), currChar);",
    "if(global.language==global.LANG_JAPANESE) UFO50_CHS_draw_fixed_ascii(_xview+tx+(charSpaceH*i),_yview+ty+(16*j),currChar,0,false); else draw_text(_xview + tx + (charSpaceH * i), _yview + ty + (16 * j), currChar);");
foreach(var expression in new[]{
    "\"[ \" + scrStringManual(\"game_internal_name_color_racer\", 0) + scrString(\"term_info_file_extension\") + \" ]\"",
    "\"[ \" + scrStringManual(\"game_internal_name_hover\", 0) + scrString(\"term_info_file_extension\") + \" ]\"",
    "\"[ \" + scrStringManual(\"game_internal_name_godsblood\", 0) + scrString(\"term_info_file_extension\") + \" ]\"",
    "\"[ \" + scrStringManual(\"game_internal_name_\" + string(global.currGameID), 0) + scrString(\"term_info_file_extension\") + \" ]\"",
    "\"[ \" + scrStringManual(\"game_internal_name_0\", 0) + scrString(\"term_info_file_extension\") + \" ]\""
}) {
    var old="draw_text(_x, _y, "+expression+");";
    FixLayout("gml_Object_oPauseMenu_Draw_0",old,
        "if(global.language==global.LANG_JAPANESE) UFO50_CHS_draw_fixed_ascii(_x,_y,"+expression+",0,false); else "+old);
}

// 第18 after：四小挑战索引30/48/13/23字形与原英文一致，旧中文行高规则使Y19替代原16。
// 第20标题已经上移2px，标题Y5..16与原索引Y16相接；恢复纯索引原centerY。
// 在第20/21/22片段之后、顶层draw_text重定向之前。2处anchor（补零/两位索引）。
FixLayout("gml_Object_o08_mg_Mas_Draw_0",
    "draw_text_centered(centerX, centerY + ((global.language == global.LANG_JAPANESE) ? max(0, ceil(string_height(\"中\")) - 8) : 0), \"  ",
    "draw_text_centered(centerX, centerY, \"  ");

// driver24 三语完整场与原交互已审；局部恢复原FancyShort ASCII、Y和补位。
// 接在20/21/22片段之后，顶层重定向之前。best顶边中文保持原Y。
FixLayout("gml_Object_o02_Game_Draw_0", "draw_text_ce(192, 32, scrStringVal(\"level\", lvl), 4);", "if(global.language==global.LANG_JAPANESE) UFO50_CHS_draw_fixed_ascii(192,32,string_even(scrStringVal(\"level\",lvl),3),-2,true); else draw_text_ce(192, 32, scrStringVal(\"level\", lvl), 4);");
FixLayout("gml_Object_o02_Game_Draw_0", "draw_text(startX, (drawY * 32) + startY + 8, scrStringVal(\"extra_units\", round(levels[lvl][AI_BUFF])));", "if(global.language==global.LANG_JAPANESE) UFO50_CHS_draw_fixed_ascii(startX,(drawY*32)+startY+8,scrStringVal(\"extra_units\",round(levels[lvl][AI_BUFF])),-2,false); else draw_text(startX, (drawY * 32) + startY + 8, scrStringVal(\"extra_units\", round(levels[lvl][AI_BUFF])));");
FixLayout("gml_Object_o02_Game_Draw_0", "draw_text_ce(192, 96, scrStringVal(\"score\", string(points)), 4);", "if(global.language==global.LANG_JAPANESE) UFO50_CHS_draw_fixed_ascii(192,96,string_even(scrStringVal(\"score\",string(points)),3),-2,true); else draw_text_ce(192, 96, scrStringVal(\"score\", string(points)), 4);");
FixLayout("gml_Object_o02_Game_Draw_0", "draw_text(0, 0, scrStringVal(\"best\", highestRank));", "if(global.language==global.LANG_JAPANESE) UFO50_CHS_draw_fixed_ascii(0,0,scrStringVal(\"best\",highestRank),0,false); else draw_text(0, 0, scrStringVal(\"best\", highestRank));");
FixLayout("gml_Object_o02_Game_Draw_0", "draw_text_ce(192, 80, moveString, 4);", "if(global.language==global.LANG_JAPANESE) UFO50_CHS_draw_fixed_ascii(192,80,string_even(moveString,3),-2,true); else draw_text_ce(192, 80, moveString, 4);");
FixLayout("gml_Object_o02_Game_Draw_0", "draw_text_ce(192, 96, challengeString, 4);", "if(global.language==global.LANG_JAPANESE) UFO50_CHS_draw_fixed_ascii(192,96,string_even(challengeString,3),-2,true); else draw_text_ce(192, 96, challengeString, 4);");

// CHS18after same-scene measured faces: Chinese200..209 vs Default200..205.
// Move only CJK glyphs of DEFLECT descriptions up2. Original digits stay Y200.
// Keep original scrDrawTextInput typewriter, input tokens, width and state action.
FixLayout("gml_GlobalScript_scrDrawTextInput",
    "draw_text(xx, yy + _libraryLabelDelta, char);",
    "var _overboldLabelDelta = (global.language == global.LANG_JAPANESE && ord(char) > 127 && variable_global_exists(\"chsOverboldGearLabelDelta\")) ? global.chsOverboldGearLabelDelta : 0;\n            draw_text(xx, yy + _libraryLabelDelta + _overboldLabelDelta, char);");
FixLayout("gml_Object_o30_Game_Draw_0",
    "scrDrawTextInput(MESSAGE_LEFT, MESSAGE_TOP, msg, controller - 1, 0, false);",
    "if (global.language == global.LANG_JAPANESE && (textMessageFull == scrString(\"gear_deflect_desc_1\") || textMessageFull == scrString(\"gear_deflect_desc_2\")))\n    {\n        var _oldGearLabelDelta = variable_global_exists(\"chsOverboldGearLabelDelta\") ? global.chsOverboldGearLabelDelta : 0;\n        global.chsOverboldGearLabelDelta = -2;\n        scrDrawTextInput(MESSAGE_LEFT, MESSAGE_TOP, msg, controller - 1, 0, false);\n        global.chsOverboldGearLabelDelta = _oldGearLabelDelta;\n    }\n    else scrDrawTextInput(MESSAGE_LEFT, MESSAGE_TOP, msg, controller - 1, 0, false);");

// geometry18候选及after18中英日完整图/最近邻已审：score中文11px下沿侵入Y16原数字。
// score上移2px；myScore padded digits保持原Y16、原精灵font与8px间隔。
FixLayout("gml_Object_o17__Game_Draw_0", "scrStringDraw(8, 8, \"score\");", "scrStringDraw(8, (global.language==global.LANG_JAPANESE)?6:8, \"score\");");
// 同场high-score原数字Y8；中文标签与数字字面中线对齐，不修改共享helper。
FixLayout("gml_Object_o17__Game_Draw_0", "UFO50_CHS_draw_labeled_number(376, 8, scrString(\"high_score\") + \":\", hiScoreFormat, 2);", "{ var _hsText=scrString(\"high_score\")+\":\"+hiScoreFormat; var _hsLayout=UFO50_CHS_fixed_ascii_layout(_hsText); UFO50_CHS_draw_fixed_ascii(376-_hsLayout.total,8,_hsText,-2,false); }");

// 最终22稳定G13返回标题三行已实图确认重叠：原8px步进、中文11px。
// 共用菜单最多4行；12px步进最后Y188，字面到198，版权从Y200起。
// 两个分支（普通标题与SUB_SELECTED闪选）及pointer共享_lineSpacing，保持同步。
FixLayout("gml_Object_oTitleScreens_Draw_0", "_lineSpacing = 8;", "_lineSpacing = (global.language == global.LANG_JAPANESE) ? max(12, ceil(string_height(\"中\")) + 1) : 8;");

// 已验29原英日同场：P1/P2 INPUT64px+原16px尾隙；A/B原字形16px选项间隔、白灰态原Confirm切换。中文before标签56px、A起点216px零间隙。
FixLayout("gml_Object_oTitleScreens_Draw_0", """
                if (menuToggle[i] == 0)
                {
                    draw_text((160 + menuToggleX) - titleX, 152 + (_lineSpacing * i) + titleY, "A");
                    draw_text_color((176 + menuToggleX) - titleX, 152 + (_lineSpacing * i) + titleY, "B", global.palette[16], global.palette[16], global.palette[16], global.palette[16], 1);
                }
                else if (menuToggle[i] == 1)
                {
                    draw_text_color((160 + menuToggleX) - titleX, 152 + (_lineSpacing * i) + titleY, "A", global.palette[16], global.palette[16], global.palette[16], global.palette[16], 1);
                    draw_text((176 + menuToggleX) - titleX, 152 + (_lineSpacing * i) + titleY, "B");
                }
""", """
                if (global.language == global.LANG_JAPANESE && menuToggle[i] >= 0)
                {
                    // scrDrawTextInput实际CJK单元+ASCII8px推进；全部控制标签共用最大宽度、16px原尾隙。
                    var _toggleOffset=0;var _cell=max(8,round(string_width("中")));
                    for(var _j=0;_j<=menuSelBot;_j++) if(menuToggle[_j]>=0) {
                        var _labelWidth=0;
                        for(var _k=1;_k<=string_length(menuOption[_j]);_k++)
                            _labelWidth+=(ord(string_char_at(menuOption[_j],_k))>=12288)?_cell:8;
                        _toggleOffset=max(_toggleOffset,_labelWidth+16);
                    }
                    var _toggleColor=draw_get_color();var _toggleAlpha=draw_get_alpha();
                    draw_set_alpha(1);
                    draw_set_color(menuToggle[i]==0?_toggleColor:global.palette[16]);
                    UFO50_CHS_draw_fixed_ascii((160+_toggleOffset)-titleX,152+(_lineSpacing*i)+titleY,"A",0,false);
                    draw_set_color(menuToggle[i]==1?_toggleColor:global.palette[16]);
                    UFO50_CHS_draw_fixed_ascii((176+_toggleOffset)-titleX,152+(_lineSpacing*i)+titleY,"B",0,false);
                    draw_set_color(_toggleColor);draw_set_alpha(_toggleAlpha);
                }
                else
                {
                if (menuToggle[i] == 0)
                {
                    draw_text((160 + menuToggleX) - titleX, 152 + (_lineSpacing * i) + titleY, "A");
                    draw_text_color((176 + menuToggleX) - titleX, 152 + (_lineSpacing * i) + titleY, "B", global.palette[16], global.palette[16], global.palette[16], global.palette[16], 1);
                }
                else if (menuToggle[i] == 1)
                {
                    draw_text_color((160 + menuToggleX) - titleX, 152 + (_lineSpacing * i) + titleY, "A", global.palette[16], global.palette[16], global.palette[16], global.palette[16], 1);
                    draw_text((176 + menuToggleX) - titleX, 152 + (_lineSpacing * i) + titleY, "B");
                }
                }
""");

// 已验标题copyright三语：原ASCII/年份原字体、8px格、string_even尾空格与Y200居中。
FixLayout("gml_Object_oTitleScreens_Draw_0", """
draw_text_ce(192, 200 + titleY, "@ " + string(_yearRounded) + " " + scrStringManual("copyright_unlimited_solutions", 0), 2);
""", """
if(global.language==global.LANG_JAPANESE) UFO50_CHS_draw_fixed_ascii(192,200+titleY,string_even("@ " + string(_yearRounded) + " " + scrStringManual("copyright_unlimited_solutions", 0),1),0,true); else draw_text_ce(192, 200 + titleY, "@ " + string(_yearRounded) + " " + scrStringManual("copyright_unlimited_solutions", 0), 2);
""");

// 已验标题copyright三语：原ASCII/年份原字体、8px格、string_even尾空格与Y200居中。
FixLayout("gml_Object_oTitleScreens_Draw_0", """
draw_text_ce(192, 200 + titleY, "@ " + string(_yearRounded) + "         ", 2);
""", """
if(global.language==global.LANG_JAPANESE) UFO50_CHS_draw_fixed_ascii(192,200+titleY,string_even("@ " + string(_yearRounded) + "         ",1),0,true); else draw_text_ce(192, 200 + titleY, "@ " + string(_yearRounded) + "         ", 2);
""");

// 已验标题copyright三语：原ASCII/年份原字体、8px格、string_even尾空格与Y200居中。
FixLayout("gml_Object_oTitleScreens_Draw_0", """
draw_text_ce(192, 200 + titleY, "@ " + string(_yearRounded) + " " + scrStringManual("copyright_ufo_soft", 0), 2);
""", """
if(global.language==global.LANG_JAPANESE) UFO50_CHS_draw_fixed_ascii(192,200+titleY,string_even("@ " + string(_yearRounded) + " " + scrStringManual("copyright_ufo_soft", 0),1),0,true); else draw_text_ce(192, 200 + titleY, "@ " + string(_yearRounded) + " " + scrStringManual("copyright_ufo_soft", 0), 2);
""");

// 已验30 G20/G34三语同场：中文制作与下行人名紧邻；ASCII姓名恢复原字体8px格、string_even(str,3)、Y112。
FixLayout("gml_Object_oTitleScreens_Draw_0", """
draw_text_ce(192 + titleX, 104 + titleY, scrStringManual("title_credits_1", 0), 4);
""", """
draw_text_ce(192 + titleX, (global.language==global.LANG_JAPANESE?100:104) + titleY, scrStringManual("title_credits_1", 0), 4);
""");

// 已验30 G20/G34三语同场：中文制作与下行人名紧邻；ASCII姓名恢复原字体8px格、string_even(str,3)、Y112。
FixLayout("gml_Object_oTitleScreens_Draw_0", """
draw_text_ce(192 + titleX, 112 + titleY, scrStringManual("title_credits_2", 0), 4);
""", """
if(global.language==global.LANG_JAPANESE) UFO50_CHS_draw_fixed_ascii(192+titleX,112+titleY,string_even(scrStringManual("title_credits_2",0),3),0,true); else draw_text_ce(192 + titleX, 112 + titleY, scrStringManual("title_credits_2", 0), 4);
""");


// Player issues #5-#7: semantic glyphs and cursor-owned menu columns.
var feedbackHelpers = new CodeImportGroup(Data);
feedbackHelpers.QueueReplace("gml_GlobalScript_UFO50_CHS_draw_grim_battle_row", """
function UFO50_CHS_draw_grim_battle_row(_x,_y,_key) {
    var _text=scrString(_key);
    if(global.language!=global.LANG_JAPANESE) { draw_text(_x,_y,_text); return; }
    // Spaces/pad glyphs separate semantic options; the navigation owns 80px columns.
    var _words=[]; var _word="";
    for(var _i=1;_i<=string_length(_text)+1;_i++) {
        var _ch=_i<=string_length(_text)?string_char_at(_text,_i):" ";
        if(_ch==" " || _ch=="☐") {
            if(_word!="") { _words[array_length(_words)]=_word; _word=""; }
        } else _word+=_ch;
    }
    for(var _i=0;_i<array_length(_words);_i++) draw_text(_x+80*_i,_y,_words[_i]);
}
""");
feedbackHelpers.QueueReplace("gml_GlobalScript_UFO50_CHS_grim_item_layout", """
function UFO50_CHS_grim_item_layout(_item) {
    var _name=scr12_ItemGetName(_item);
    var _icon="";
    if(string_length(_item)>=2 && string_char_at(_item,2)==" " && string_pos(string_char_at(_item,1),"abcdefghijk")>0) _icon=string_char_at(_item,1);
    // Chinese translations keep semantic names; icon identity comes from the stored ID.
    // Preserve the original 8px glyph plus 8px separator, not a new CJK space.
    return {name:_name, icon:_icon, prefix:(_icon!=""?16:0), width:string_width(_name)+(_icon!=""?16:0)};
}
""");
feedbackHelpers.QueueReplace("gml_GlobalScript_UFO50_CHS_draw_grim_item", """
function UFO50_CHS_draw_grim_item(_x,_y,_item) {
    if(global.language!=global.LANG_JAPANESE) { draw_text(_x,_y,scr12_ItemGetName(_item)); return; }
    var _font=draw_get_font(); var _ha=draw_get_halign();
    var _layout=UFO50_CHS_grim_item_layout(_item);
    if(_ha==fa_center) _x-=floor(_layout.width/2);
    else if(_ha==fa_right) _x-=_layout.width;
    draw_set_halign(fa_left);
    if(_layout.icon!="") {
        draw_set_font(global.fontGrimstone);
        // Top-level draw_text will be redirected. +1 cancels the CJK wrapper Y correction
        // for this native icon; currentIsCHS is false so the CJK outline stays disabled.
        draw_text(_x,_y+1,_layout.icon);
    }
    draw_set_font(_font);
    draw_text(_x+_layout.prefix,_y,_layout.name);
    draw_set_halign(_ha);
}
""");
feedbackHelpers.QueueReplace("gml_GlobalScript_UFO50_CHS_48_begin_cipher", """
function UFO50_CHS_48_begin_cipher(_isMeta)
{
    var _alien = _isMeta || (verify(speaker) && messageLanguage == LANG_ALIEN && !translateOn);
    chs48CipherMessage = global.language == global.LANG_JAPANESE && _alien;
    if (!chs48CipherMessage) return -1;
    if (!variable_global_exists("chs48CipherOriginal")) {
        var _file = string_replace(global.EXTERNAL_TEXT_FILE, "*", "48");
        _file = string_replace(_file, "#", global.LANG_HEADERS[global.LANG_ENGLISH]);
        var _buffer = buffer_load(_file);
        var _content = buffer_read(_buffer, buffer_string);
        buffer_delete(_buffer);
        if (global.decoding[48] == 1) _content = base64_decode(_content);
        global.chs48CipherOriginal = json_parse(_content);
    }
    var _state = { language: global.language, text: global.TEXT_GAME[48], font: draw_get_font(), current: global.currFont, requested: global.chsRequestedFont };
    // The original English string and original8px wrapping generate cipher glyphs.
    global.TEXT_GAME[48] = global.chs48CipherOriginal;
    global.language = global.LANG_ENGLISH;
    draw_set_font(global.fontAlien);
    global.currFont = global.fontAlien;
    global.chsRequestedFont = global.fontAlien;
    return _state;
}
""");
feedbackHelpers.QueueReplace("gml_GlobalScript_UFO50_CHS_48_end_cipher", """
function UFO50_CHS_48_end_cipher(_state)
{
    if (is_real(_state)) return;
    global.language = _state.language;
    global.TEXT_GAME[48] = _state.text;
    if (font_exists(_state.font)) draw_set_font(_state.font);
    global.currFont = _state.current;
    global.chsRequestedFont = _state.requested;
}
""");
feedbackHelpers.QueueReplace("gml_GlobalScript_scr48_MessageLocal", """
function scr48_MessageLocal(arg0, arg1)
{
    var _chsCipherState = UFO50_CHS_48_begin_cipher(arg0 == "meta");
    var str;
    if (arg0 == "meta")
    {
        str = chs48CipherMessage ? "MEESHA DANRY IS MY HERO." : scrStringManual("game_meta_message_48", 0);
    }
    else
    {
        str = scrStringExt(arg0, arg1, MESSAGE_MAX, 0);
    }
    scrMessageSet(str);
    scrMessageInsertBreaks(MESSAGE_LINE_LENGTH * 8);
    UFO50_CHS_48_end_cipher(_chsCipherState);
}
""");
feedbackHelpers.QueueReplace("gml_GlobalScript_scr48_MessageLocalClue", """
function scr48_MessageLocalClue(arg0)
{
    var _chsCipherState = UFO50_CHS_48_begin_cipher(false);
    if (arg0 == 1)
    {
        var xDiff = obscureH - pX;
        var yDiff = obscureK - pY;
        var xMessage;
        if (xDiff < 0)
        {
            if (abs(xDiff) == 1)
            {
                xMessage = scrStringVal("secret_west_singular", abs(xDiff));
            }
            else
            {
                xMessage = scrStringVal("secret_west_plural", abs(xDiff));
            }
        }
        else if (abs(xDiff) == 1)
        {
            xMessage = scrStringVal("secret_east_singular", abs(xDiff));
        }
        else
        {
            xMessage = scrStringVal("secret_east_plural", abs(xDiff));
        }
        var yMessage;
        if (yDiff < 0)
        {
            if (abs(yDiff) == 1)
            {
                yMessage = scrStringVal("secret_north_singular", abs(yDiff));
            }
            else
            {
                yMessage = scrStringVal("secret_north_plural", abs(yDiff));
            }
        }
        else if (abs(yDiff) == 1)
        {
            yMessage = scrStringVal("secret_south_singular", abs(yDiff));
        }
        else
        {
            yMessage = scrStringVal("secret_south_plural", abs(yDiff));
        }
        var dirMessage;
        if (xDiff != 0 && yDiff != 0)
        {
            dirMessage = scrStringVal("secret_2step", xMessage, yMessage);
        }
        else if (xDiff != 0)
        {
            dirMessage = scrStringVal("secret_1step", xMessage);
        }
        else
        {
            dirMessage = scrStringVal("secret_1step", yMessage);
        }
        scrMessageSet(dirMessage);
        scrMessageInsertBreaks(MESSAGE_LINE_LENGTH * 8);
    }
    if (arg0 == 2)
    {
        var str = scrStringExt(clueMessage, "*", MESSAGE_MAX, 0);
        scrMessageSet(str);
        scrMessageInsertBreaks(MESSAGE_LINE_LENGTH * 8);
    }
    UFO50_CHS_48_end_cipher(_chsCipherState);
}
""");
feedbackHelpers.Import();
// Insert helpers.gml function definitions via CodeImportGroup before final redirect pass.
// This candidate is not applied to the public patch.
foreach(var key in new[]{"battle_menu_01","battle_menu_02"})
    FixLayout("gml_Object_o12__Game_Draw_0",
        "draw_text(viewx + 30, (viewyBot - 44) + 8, scrString(\""+key+"\"));",
        "UFO50_CHS_draw_grim_battle_row(viewx + 30, (viewyBot - 44) + 8, \""+key+"\");");
FixLayout("gml_Object_o12__Game_Draw_0",
    "draw_text(viewx + 30, (viewyBot - 44) + 24, scrString(\"battle_menu_03\"));",
    "UFO50_CHS_draw_grim_battle_row(viewx + 30, (viewyBot - 44) + 24, \"battle_menu_03\");");
FixLayout("gml_GlobalScript_scr12_BattleMoveNext",
    "scrStringVal(\"battle_player_reload\", player.name, player.rightHand)",
    "scrStringVal(\"battle_player_reload\", player.name, (global.language == global.LANG_JAPANESE) ? scr12_ItemGetName(player.rightHand) : player.rightHand)");
// 14 matches in baseline. Verify total before import.
var grimDrawCode=Data.Code.ByName("gml_Object_o12__Game_Draw_0");
var grimDrawSource=GetDecompiledText(grimDrawCode,new UndertaleModLib.Decompiler.GlobalDecompileContext(Data));
var itemPattern=@"(?<!\w)draw_text\(([^;\n]*?),\s*([^;\n]*?),\s*scr12_ItemGetName\(([^;\n]*)\)\);";
if(System.Text.RegularExpressions.Regex.Matches(grimDrawSource,itemPattern).Count!=14)
    throw new System.Exception("Issue5 expects 14 Grimstone item draw calls");
var grimItems=new UndertaleModLib.Compiler.CodeImportGroup(Data);
grimItems.ThrowOnNoOpFindReplace=true;
grimItems.QueueRegexFindReplace(grimDrawCode,itemPattern,"UFO50_CHS_draw_grim_item($1, $2, $3);",true);
grimItems.Import();
FixLayout("gml_Object_o48_Game_Draw_0", "if (textMessageFull == scrStringManual(\"game_meta_message_48\", 0))", "if ((variable_instance_exists(id, \"chs48CipherMessage\") && chs48CipherMessage) || textMessageFull == scrStringManual(\"game_meta_message_48\", 0))");

// Additional code-confirmed same-class risk; apply only after before screenshot.
// oTextBox yes/no positions derive pixel width as 8*char count instead of runtime CJK width.
FixLayout("gml_Object_oTextBox_Draw_0",
    "var _w = 8 * max(string_length(_y), string_length(_n));",
    "var _w = (global.language == global.LANG_JAPANESE) ? max(string_width(_y), string_width(_n)) : 8 * max(string_length(_y), string_length(_n));");
foreach(var label in new[]{"_y","_n"})
    FixLayout("gml_Object_oTextBox_Draw_0",
        "round((string_length("+label+") * 8) / 2)",
        "round(((global.language == global.LANG_JAPANESE) ? string_width("+label+") : (string_length("+label+") * 8)) / 2)");

// Root must before/after verify real garage escape question and right/left navigation.
// Keep original pointer drawing, x72 and x216 relative to camera. s39_Pointer
// origin0,0 and opaque bbox [5,4,31,20) reaches x102 / x246 inclusive.
// Use the English first-column anchor108 and second-column252: 5 empty pixels.
FixLayout("gml_Object_o39__Game_Step_0", "str2 = \"        はい          いいえ\";",
    "str2 = scrString(\"yesno\");");
FixLayout("gml_Object_o39__Game_Draw_0",
    "        draw_text(camera_get_view_x(view_get_camera(0)) + 12, ((camera_get_view_y(view_get_camera(0)) + 216) - 64) + 12 + 14, str2);",
    "        draw_text(camera_get_view_x(view_get_camera(0)) + 108, camera_get_view_y(view_get_camera(0)) + 180, scrStringManual(\"option_yes\", 0));\n        draw_text(camera_get_view_x(view_get_camera(0)) + 252, camera_get_view_y(view_get_camera(0)) + 180, scrStringManual(\"option_no\", 0));");


// Root must reproduce trophies beside translated names in both Draft and Build Team.
// Sprite stays on its original Y; text width uses current Tall/CHS request.
FixLayout("gml_Object_o43_Game_Draw_0",
    "draw_sprite(s43_TrophyIcon, 0, 112 + (8 * string_length(chars[draftOptions[draftX]][NAME])), 32);",
    "draw_sprite(s43_TrophyIcon, 0, 112 + ((global.language == global.LANG_JAPANESE) ? string_width(chars[draftOptions[draftX]][NAME]) : 8 * string_length(chars[draftOptions[draftX]][NAME])), 32);");
FixLayout("gml_Object_o43_Game_Draw_0",
    "draw_sprite(s43_TrophyIcon, 0, 16 + xShift + (8 * string_length(chars[draftOptions[draftPos]][NAME])), 32);",
    "draw_sprite(s43_TrophyIcon, 0, 16 + xShift + ((global.language == global.LANG_JAPANESE) ? string_width(chars[draftOptions[draftPos]][NAME]) : 8 * string_length(chars[draftOptions[draftPos]][NAME])), 32);");

// ID48 trade choices: native 8px stepping overlaps Zpix11px glyphs.
// Measure the current Chinese label height; two rows use height+3px.
// Grow the panel upwards; its bottom stays viewy+216-BORDER_BOTTOM.
// Native English/unreadable-alien paths retain their original panel and coordinates.
FixLayout("gml_Object_o48_Game_Draw_0", """
    var yHeight;
    if (messageChoice)
    {
        yHeight = 80;
    }
""", """
    var _chsTradeChoice = global.language == global.LANG_JAPANESE && messageChoice && !iconsHidden;
    var _choiceHeight = _chsTradeChoice ? max(8,ceil(max(string_height(scrString("yes")),string_height(scrString("no"))))) : 8;
    var _choiceStep = _chsTradeChoice ? max(14,_choiceHeight+3) : 8;
    var _choiceFirst = _chsTradeChoice ? 52 : 56;
    var yHeight;
    if (messageChoice)
    {
        yHeight = _chsTradeChoice ? max(80,8+_choiceFirst+_choiceStep+_choiceHeight+3) : 80;
    }
""");
FixLayout("gml_Object_o48_Game_Draw_0", """
        scrStringDraw(xLeft + 136, yTopText + 56, "yes");
        scrStringDraw(xLeft + 136, yTopText + 64, "no");
        draw_text(xLeft + 120, yTopText + 56 + (8 * messageY), ">");
""", """
        scrStringDraw(xLeft + 136, yTopText + _choiceFirst, "yes");
        scrStringDraw(xLeft + 136, yTopText + _choiceFirst + _choiceStep, "no");
        if (_chsTradeChoice) UFO50_CHS_draw_fixed_ascii(xLeft + 120, yTopText + _choiceFirst + (_choiceStep * messageY), ">", 0, false);
        else draw_text(xLeft + 120, yTopText + 56 + (8 * messageY), ">");
""");

// Before screenshots confirm shop rows/hint lines overlap in Zpix.
// Prices stay in their native X96 column and use the existing numeric-font route.
// Selector and item labels share the same measured row origin/step.
FixLayout("gml_Object_o40_Shop_Draw_0", """
if (state == 2)
{
""", """
if (state == 2)
{
    var _chsShop = global.language == global.LANG_JAPANESE;
    var _shopGlyphHeight = _chsShop ? max(8,ceil(string_height("中"))) : 8;
    var _shopStep = _chsShop ? max(14,_shopGlyphHeight+3) : 8;
    var _shopExitY = _chsShop ? max(80,40+itemCount*_shopStep) : 80;
    var _shopPanelHeight = _chsShop ? max(64,_shopExitY-32+_shopGlyphHeight+7) : 64;
""");
FixLayout("gml_Object_o40_Shop_Draw_0", "scrDrawMenuBorder(xx, yy + 32, 128, 64);", "scrDrawMenuBorder(xx, yy + 32, 128, _shopPanelHeight);");
FixLayout("gml_Object_o40_Shop_Draw_0", "var yD = yy + 40 + (8 * i);", "var yD = yy + 40 + (_shopStep * i);");
FixLayout("gml_Object_o40_Shop_Draw_0", "draw_text(xx + 8, yy + 80, scrStringLimit(\"exit\", 14));", "draw_text(xx + 8, yy + _shopExitY, scrStringLimit(\"exit\", 14));");
FixLayout("gml_Object_o40_Shop_Draw_0", "draw_sprite(s40_TextCursor, 0, xx, yy + 40 + (8 * selCurr));", "draw_sprite(s40_TextCursor, 0, xx, yy + 40 + (_shopStep * selCurr));");
FixLayout("gml_Object_o40_Shop_Draw_0", "draw_sprite(s40_TextCursor, 0, xx, yy + 80);", "draw_sprite(s40_TextCursor, 0, xx, yy + _shopExitY);");

// All ten original hint slots fit in134px at12px step: cameraY+32..166.
// Native96px panel and8px stepping remain exact for English.
FixLayout("gml_Object_o40_Shop_Draw_0", "scrDrawMenuBorder(xx, yy, 128, 96);", """
var _hintStep = (global.language == global.LANG_JAPANESE) ? max(12,ceil(string_height("中"))+1) : 8;
    var _hintHeight = (global.language == global.LANG_JAPANESE) ? max(96,8+10*_hintStep+6) : 96;
    scrDrawMenuBorder(xx, yy, 128, _hintHeight);
""");
FixLayout("gml_Object_o40_Shop_Draw_0", "var yD = yy + 8 + (8 * i);", "var yD = yy + 8 + (_hintStep * i);");

// Before evidence: issues-before-cursor6-chs-design16-cursor-death.
// Keep the real retry/station input indices; text and native sprite share one row step.
FixLayout("gml_Object_o16_Mas_Draw_0", """
    draw_sprite(s16_GameOver, 0, _xview + 112, _yview + 32);
    draw_sprite(s16_Pointer, 0, _xview + 120, _yview + 160 + (8 * menuSel));
    draw_text(_xview + 136, _yview + 160, strRetrySector);
    draw_text(_xview + 136, _yview + 168, strReturnToStation);
""", """
    draw_sprite(s16_GameOver, 0, _xview + 112, _yview + 32);
    var _deathStep = (global.language == global.LANG_JAPANESE) ? max(14,ceil(string_height("中"))+3) : 8;
    draw_sprite(s16_Pointer, 0, _xview + 120, _yview + 160 + (_deathStep * menuSel));
    draw_text(_xview + 136, _yview + 160, strRetrySector);
    draw_text(_xview + 136, _yview + 160 + _deathStep, strReturnToStation);
""");

// Before evidence: issues-before-cursor6-chs-design19-cursor-battle.
// Four CHS rows use 14px, y160/174/188/202; 11px ink ends at213 inside216px.
// The left equipment/remaining-uses and right action menu keep original X columns.
// Every label, native icon, cursor and numeric draw shares the row origin.
var battleDraw567 = Data.Code.ByName("gml_Object_o19_Mas_Draw_0");
var battleSource567 = GetDecompiledText(battleDraw567, new UndertaleModLib.Decompiler.GlobalDecompileContext(Data));
var battleStart567 = battleSource567.IndexOf("if (battleState == 0)");
var battleEnd567 = battleSource567.IndexOf("else if (battleState < 90)", battleStart567);
if (battleStart567 < 0 || battleEnd567 < 0) throw new System.Exception("Missing original Divers battle choice branch.");
var battleBefore567 = battleSource567.Substring(battleStart567, battleEnd567 - battleStart567);
var battleAfter567 = battleBefore567.Replace("var currItemType, currItemElement;",
    "var currItemType, currItemElement;\n        var _battleStep = (global.language == global.LANG_JAPANESE) ? max(14,ceil(string_height(\"中\"))+3) : 8;")
    .Replace("(8 * i)", "(_battleStep * i)")
    .Replace("(8 * menuSel)", "(_battleStep * menuSel)")
    .Replace("(8 * menuSel2)", "(_battleStep * menuSel2)");
if (battleAfter567 == battleBefore567) throw new System.Exception("Divers battle choice layout did not change.");
FixLayout("gml_Object_o19_Mas_Draw_0", battleBefore567, battleAfter567);

// ID12 ally is selected from enemies[].name (internal ID); party/custom names keep their route.
// Isolated candidate for SHOUT / WAVE / RALLY original-effect fixtures.
foreach (var key in new[] { "battle_attack_up", "battle_ally_refreshed", "battle_defense_up" })
    FixLayout("gml_GlobalScript_scr12_BattleEnemyMove",
        "scrStringVal(\"" + key + "\", ally.name)",
        "scrStringVal(\"" + key + "\", (global.language == global.LANG_JAPANESE) ? scr12_EnemyGetName(ally.name) : ally.name)");

EnsureDataLoaded();

var vaingerImports567 = new CodeImportGroup(Data);
string VaingerRead567(string name) => name switch
{
"vainger-cipher-open-begin.gml" => """
    // Preserve the damaged, unidentified terminal language and its original layout.
    var _chs7Cipher = global.language == global.LANG_JAPANESE && stringLoad == "terminal_armory_room";
    if (_chs7Cipher)
    {
        var _chs7Language = global.language;
        var _chs7DrawFont = draw_get_font();
        var _chs7CurrFont = global.currFont;
        var _chs7HadRequestedFont = variable_global_exists("chsRequestedFont");
        var _chs7RequestedFont = _chs7HadRequestedFont ? global.chsRequestedFont : -1;
        if (!variable_global_exists("chs7CipherOriginal")) {
            var _chs7File = string_replace(global.EXTERNAL_TEXT_FILE,"*","7");
            _chs7File = string_replace(_chs7File,"#",global.LANG_HEADERS[global.LANG_ENGLISH]);
            var _chs7Buffer = buffer_load(_chs7File);
            var _chs7Content = buffer_read(_chs7Buffer,buffer_string);buffer_delete(_chs7Buffer);
            if(global.decoding[7]==1)_chs7Content=base64_decode(_chs7Content);
            global.chs7CipherOriginal=json_parse(_chs7Content);
        }
        var _chs7TextResource = global.TEXT_GAME[7];
        global.TEXT_GAME[7] = global.chs7CipherOriginal;
        global.language = global.LANG_ENGLISH;
        scrSetFont(global.fontDefault);
    }

""",
"vainger-cipher-open-end.gml" => """
    if (_chs7Cipher)
    {
        _chs7Text.chsOriginalCipher = true;
        global.TEXT_GAME[7] = _chs7TextResource;
        global.language = _chs7Language;
        global.currFont = _chs7CurrFont;
        if (_chs7HadRequestedFont) global.chsRequestedFont = _chs7RequestedFont;
        draw_set_font(_chs7DrawFont);
    }

""",
"vainger-cipher-draw-begin.gml" => """
var _chs7CipherDraw = variable_instance_exists(id, "chsOriginalCipher") && chsOriginalCipher && global.language == global.LANG_JAPANESE;
if (_chs7CipherDraw)
{
    var _chs7DrawLanguage = global.language;
    var _chs7SavedDrawFont = draw_get_font();
    var _chs7SavedCurrFont = global.currFont;
    var _chs7DrawHadRequestedFont = variable_global_exists("chsRequestedFont");
    var _chs7SavedRequestedFont = _chs7DrawHadRequestedFont ? global.chsRequestedFont : -1;
    global.language = global.LANG_ENGLISH;
}

""",
"vainger-cipher-draw-end.gml" => """
if (_chs7CipherDraw)
{
    global.language = _chs7DrawLanguage;
    global.currFont = _chs7SavedCurrFont;
    if (_chs7DrawHadRequestedFont) global.chsRequestedFont = _chs7SavedRequestedFont;
    draw_set_font(_chs7SavedDrawFont);
}

""",
_ => throw new System.Exception("Unknown embedded GML dependency: " + name)
};
string VaingerSource567(string name) {
    var code=Data.Code.ByName(name);
    if(code==null) throw new Exception("Missing Vainger cipher code: "+name);
    return GetDecompiledText(code,new GlobalDecompileContext(Data));
}
string VaingerOnce567(string source,string anchor,string replacement) {
    if(source.Split(new[]{anchor},StringSplitOptions.None).Length!=2)
        throw new Exception("Vainger cipher anchor count: "+anchor);
    return source.Replace(anchor,replacement);
}
var terminalName="gml_Object_o07_Terminal_Other_10";
var terminal=VaingerSource567(terminalName);
if(terminal.Contains("_chs7Cipher")) throw new Exception("Vainger cipher delta already applied");
var percent="    var _percent = floor(100 *";
terminal=VaingerOnce567(terminal,percent,VaingerRead567("vainger-cipher-open-begin.gml")+"\n"+percent);
var drawBox="scrDrawTextBoxEx(0, 0, 384, scrDrawTextBoxGetHeight(_str, 32, 16, 3), -10, _str[0], _str[1], _str[2]);";
terminal=VaingerOnce567(terminal,drawBox,"var _chs7Text = "+drawBox+"\n"+VaingerRead567("vainger-cipher-open-end.gml"));
vaingerImports567.QueueReplace(Data.Code.ByName(terminalName),terminal);
var drawName="gml_Object_oTextBox_Draw_0";
var draw=VaingerSource567(drawName);
var early=new Regex(@"\Aif\s*\(skipFrame\)\s*\{\s*skipFrame\s*=\s*false;\s*exit;\s*\}");
var match=early.Match(draw);
if(!match.Success) throw new Exception("Vainger cipher textbox skipFrame prefix changed");
var body=draw.Substring(match.Length);
if(Regex.IsMatch(body,@"\b(exit|return)\s*[;\(]")) throw new Exception("Vainger cipher textbox body has an early exit; inspect scope restoration");
draw=draw.Substring(0,match.Length)+"\n"+VaingerRead567("vainger-cipher-draw-begin.gml")+body+"\n"+VaingerRead567("vainger-cipher-draw-end.gml");
vaingerImports567.QueueReplace(Data.Code.ByName(drawName),draw);
vaingerImports567.Import();

// Independent issue567 candidate. Append before the global draw redirect pass.
// Based on candidate-unified1: changes only confirmed ID31/Crack credit draw paths.


var crack31Helpers="""
function UFO50_CHS_31_reveal_layout(_lines,_width,_blankStep,_ending) {
    var _glyphs=[],_yy=0,_step=max(12,ceil(string_height("中"))+1);
    for(var _r=0;_r<array_length(_lines);_r++) {
        var _text=_lines[_r],_xx=0,_begin=1;
        var _empty=string_replace_all(string_replace_all(_text,chr(10),""),chr(13),"")=="";
        if(_empty) {_yy+=_blankStep;continue;}
        // Keep the source's leading-space reveal ticks, but place the final label at the right edge.
        if(_ending && _r==array_length(_lines)-1) {
            while(_begin<=string_length(_text) && string_char_at(_text,_begin)==" ")_begin++;
            _xx=max(0,_width-string_width(string_copy(_text,_begin,string_length(_text))));
        }
        for(var _c=_begin;_c<=string_length(_text);_c++) {
            var _char=string_char_at(_text,_c),_w=string_width(_char);
            if(_char==chr(10) || _char==chr(13)) {_xx=0;_yy+=_step;continue;}
            if(_xx>0 && _xx+_w>_width) {_xx=0;_yy+=_step;}
            array_push(_glyphs,{sourceRow:_r,sourceChar:_c,text:_char,x:_xx,y:_yy,width:_w});
            _xx+=_w;
        }
        _yy+=_step;
    }
    return {glyphs:_glyphs,height:_yy,step:_step};
}

function UFO50_CHS_31_draw_reveal(_layout,_x,_y,_sourceRow,_sourceCount) {
    var _ha=draw_get_halign();draw_set_halign(fa_left);
    for(var _i=0;_i<array_length(_layout.glyphs);_i++) {
        var _g=_layout.glyphs[_i];
        if(_g.sourceRow<_sourceRow || (_g.sourceRow==_sourceRow && _g.sourceChar<=_sourceCount))
            UFO50_CHS_draw_text(_x+_g.x,_y+_g.y,_g.text);
    }
    draw_set_halign(_ha);
}

function UFO50_CHS_crack_has_cjk(_text) {
    for(var _i=1;_i<=string_length(_text);_i++)if(ord(string_char_at(_text,_i))>=11904)return true;
    return false;
}

function UFO50_CHS_crack_draw_original(_x,_y,_text) {
    // Native sprite font: cancel the global CHS baseline correction for nonnumeric strings.
    UFO50_CHS_draw_text(_x,_y+((UFO50_CHS_number_font(_text)<0)?1:0),_text);
}

function UFO50_CHS_draw_crack_localized() {
    var _savedFont=draw_get_font(),_savedRequest=global.chsRequestedFont,_ha=draw_get_halign();
    if(style==0) {
        scrSetFont(global.fontCrackHeader);
        if(!positioned){x=oLibrary.cracktroLeftX;positioned=true;}
        var _layout=UFO50_CHS_31_reveal_layout(str,144,16,false);
        draw_set_halign(fa_left);
        for(var _i=0;_i<array_length(_layout.glyphs);_i++) {
            var _g=_layout.glyphs[_i];
            UFO50_CHS_draw_text(true_round(x+_g.x),true_round(y+_g.y+cos(wave+_g.sourceChar-1)),_g.text);
        }
    } else if(style==1 || style==2) {
        var _native=(style==1)?global.fontCrackBig:global.fontCrackSmall;
        scrSetFont(_native);draw_set_halign(fa_right);
        if(!positioned){x=oLibrary.cracktroRightX+((style==2)?16:((gnomeID==-1)?32:0));positioned=true;}
        var _offset=0;
        for(var _i=0;_i<array_length(str);_i++) {
            var _cjk=UFO50_CHS_crack_has_cjk(str[_i]);
            if(_cjk){scrSetFont(_native);UFO50_CHS_draw_text(x,y+_offset,str[_i]);}
            else {draw_set_font(_native);global.chsRequestedFont=_native;UFO50_CHS_crack_draw_original(x,y+_offset,str[_i]);}
            _offset+=(style==1)?16:(_cjk?12:8);
        }
        if(style==1 && gnomeID>-1)draw_sprite(sCrackPortraits,gnomeID,x,y);
    } else if(style==3) {
        if(!positioned){x=oLibrary.cracktroLeftX;positioned=true;}
        draw_sprite(sMossmouth,0,x,y);
    } else if(style==4) {
        scrSetFont(global.fontDefault);
        if(!positioned){x=oLibrary.cracktroLeftX+80;positioned=true;}
        var _step=max(12,ceil(string_height("中"))+1),_watermarkY=8;
        draw_set_halign(fa_center);
        for(var _i=0;_i<array_length(str);_i++) {
            UFO50_CHS_draw_text(x,y+_step*_i,str[_i]);
            if(str[_i]!="")_watermarkY=(_i+1)*_step+8;
        }
        draw_set_halign(fa_left);draw_set_font(global.fontCrackBig);global.chsRequestedFont=global.fontCrackBig;
        UFO50_CHS_crack_draw_original(x-8,y+_watermarkY,"@");
    }
    draw_set_font(_savedFont);global.chsRequestedFont=_savedRequest;draw_set_halign(_ha);
}

""";
var crack31Functions=Regex.Matches(crack31Helpers,@"(?m)^function (\w+)\(");
var crack31Import=new CodeImportGroup(Data);crack31Import.AutoCreateAssets=true;
for(var crack31i=0;crack31i<crack31Functions.Count;crack31i++) {
    var m=crack31Functions[crack31i];
    var end=(crack31i+1<crack31Functions.Count)?crack31Functions[crack31i+1].Index:crack31Helpers.Length;
    crack31Import.QueueReplace("gml_GlobalScript_"+m.Groups[1].Value,crack31Helpers.Substring(m.Index,end-m.Index));
}
crack31Import.Import();
FixLayout("gml_Object_o31_Mas_Draw_0", "    for (var i = 0; i < 5; i++)\n    {\n        if (i == textLine)\n        {\n            for (var j = 0; j < textCount; j++)\n            {\n                draw_text(xv + 24 + (8 * j), yv + 48 + (8 * i), string_char_at(mgText[i], j + 1));\n            }\n        }\n        else if (i < textLine)\n        {\n            draw_text(xv + 24, yv + 48 + (8 * i), mgText[i]);\n        }\n    }\n    if (subState == 1)\n    {\n        scrStringDraw(xv + 40, yv + 96, \"mg_yes\");\n        scrStringDraw(xv + 88, yv + 96, \"mg_no\");\n        draw_sprite(s31_Pointer, floor(tPointer * 0.1), xv + 32 + (mgSelect * 48), yv + 96);\n    }\n    else if (subState == 2)\n    {\n        if ((t % 6) > 2)\n        {\n            if (mgSelect == 0)\n            {\n                scrStringDraw(xv + 40, yv + 96, \"mg_yes\");\n            }\n            else\n            {\n                scrStringDraw(xv + 88, yv + 96, \"mg_no\");\n            }\n            draw_sprite(s31_Pointer, 0, xv + 32 + (mgSelect * 48), yv + 96);\n        }\n    }\n", "    if(global.language==global.LANG_JAPANESE) {\n        var _chsMenuFont=draw_get_font(),_chsMenuRequest=global.chsRequestedFont,_chsMenuColor=draw_get_color();\n        scrSetFont(global.fontDefault);\n        var _chsMenuLayout=UFO50_CHS_31_reveal_layout(mgText,96,12,false);\n        var _chsMenuGlyphHeight=max(8,ceil(string_height(\"中\")));\n        var _chsMenuTop=min(40,120-_chsMenuLayout.height-4-_chsMenuGlyphHeight-5);\n        var _chsChoiceY=_chsMenuTop+_chsMenuLayout.height+4;\n        // Expand upward and down to y119; the original decorative board starts at y120.\n        draw_set_color(c_white);draw_rectangle(xv+16,yv+_chsMenuTop-8,xv+127,yv+119,false);\n        draw_set_color(c_black);draw_rectangle(xv+20,yv+_chsMenuTop-4,xv+123,yv+115,false);\n        draw_set_color(_chsMenuColor);\n        UFO50_CHS_31_draw_reveal(_chsMenuLayout,xv+24,yv+_chsMenuTop,textLine,textCount);\n        if(subState==1 || (subState==2 && (t%6)>2)) {\n            if(subState==1 || mgSelect==0)scrStringDraw(xv+40,yv+_chsChoiceY,\"mg_yes\");\n            if(subState==1 || mgSelect==1)scrStringDraw(xv+88,yv+_chsChoiceY,\"mg_no\");\n            draw_sprite(s31_Pointer,(subState==1)?floor(tPointer*0.1):0,xv+32+mgSelect*48,yv+_chsChoiceY);\n        }\n        draw_set_font(_chsMenuFont);global.chsRequestedFont=_chsMenuRequest;\n    } else {\n    for (var i = 0; i < 5; i++)\n    {\n        if (i == textLine)\n        {\n            for (var j = 0; j < textCount; j++)\n            {\n                draw_text(xv + 24 + (8 * j), yv + 48 + (8 * i), string_char_at(mgText[i], j + 1));\n            }\n        }\n        else if (i < textLine)\n        {\n            draw_text(xv + 24, yv + 48 + (8 * i), mgText[i]);\n        }\n    }\n    if (subState == 1)\n    {\n        scrStringDraw(xv + 40, yv + 96, \"mg_yes\");\n        scrStringDraw(xv + 88, yv + 96, \"mg_no\");\n        draw_sprite(s31_Pointer, floor(tPointer * 0.1), xv + 32 + (mgSelect * 48), yv + 96);\n    }\n    else if (subState == 2)\n    {\n        if ((t % 6) > 2)\n        {\n            if (mgSelect == 0)\n            {\n                scrStringDraw(xv + 40, yv + 96, \"mg_yes\");\n            }\n            else\n            {\n                scrStringDraw(xv + 88, yv + 96, \"mg_no\");\n            }\n            draw_sprite(s31_Pointer, 0, xv + 32 + (mgSelect * 48), yv + 96);\n        }\n    }\n    }\n");
FixLayout("gml_Object_o31_Mas_Draw_0", "        for (var i = 0; i < array_length(endText[endType]); i++)\n        {\n            if (i == textLine)\n            {\n                for (var j = 0; j < textCount; j++)\n                {\n                    draw_text(xv + 24 + (8 * j), yv + 32 + (8 * i), string_char_at(endText[endType][i], j + 1));\n                }\n            }\n            else if (i < textLine)\n            {\n                draw_text(xv + 24, yv + 32 + (8 * i), endText[endType][i]);\n            }\n        }\n", "        if(global.language==global.LANG_JAPANESE) {\n            var _chsEndFont=draw_get_font(),_chsEndRequest=global.chsRequestedFont;\n            scrSetFont(global.fontDefault);\n            var _chsEndLayout=UFO50_CHS_31_reveal_layout(endText[endType],208,6,true);\n            // Source rows and source character counts still drive the original animation/timing.\n            // Adapt the drawing origin to the full measured block; keep the right image at x256.\n            var _chsEndTop=min(32,max(8,208-_chsEndLayout.height));\n            UFO50_CHS_31_draw_reveal(_chsEndLayout,xv+24,yv+_chsEndTop,textLine,textCount);\n            draw_set_font(_chsEndFont);global.chsRequestedFont=_chsEndRequest;\n        } else {\n        for (var i = 0; i < array_length(endText[endType]); i++)\n        {\n            if (i == textLine)\n            {\n                for (var j = 0; j < textCount; j++)\n                {\n                    draw_text(xv + 24 + (8 * j), yv + 32 + (8 * i), string_char_at(endText[endType][i], j + 1));\n                }\n            }\n            else if (i < textLine)\n            {\n                draw_text(xv + 24, yv + 32 + (8 * i), endText[endType][i]);\n            }\n        }\n        }\n");
FixLayout("gml_Object_oTextCrack_Draw_0", "if (style == 0)", "if(global.language==global.LANG_JAPANESE) { UFO50_CHS_draw_crack_localized(); exit; }\nif (style == 0)");
FixLayout("gml_GlobalScript_scrCrackCredit", "oLibrary.cracktroHeight += (_lineHeight * numLines) + argument[2];", "if(global.language==global.LANG_JAPANESE && text.style==STYLE_DISCLAIMER)_lineHeight=12;\n    oLibrary.cracktroHeight += (_lineHeight * numLines) + argument[2];");

// Candidate only. Apply after existing title spacing and A/B layout changes.
// Keep the expanded Chinese row spacing, and anchor the final row at the native Y.
// This snippet is applied to the Chinese candidate; the English branch retains shift zero.
// FixLayout imports/decompiles between edits, so its anchors use normalized compiled source.
string TitleBottomCompiledSource567() {
    var code=Data.Code.ByName("gml_Object_oTitleScreens_Draw_0");
    if(code==null)throw new System.Exception("Shared title Draw0 missing");
    return GetDecompiledText(code,new UndertaleModLib.Decompiler.GlobalDecompileContext(Data));
}
void TitleBottomChecked567(string anchor,string replacement,int expected) {
    var source=TitleBottomCompiledSource567();
    int count=source.Split(new[]{anchor},System.StringSplitOptions.None).Length-1;
    if(count!=expected)throw new System.Exception("Shared title anchor count "+count+" expected "+expected+": "+anchor);
    FixLayout("gml_Object_oTitleScreens_Draw_0",anchor,replacement);
}
TitleBottomChecked567("    var _showCopyright = true;", """
    var _chsMenuShift = 0;
    if (global.language == global.LANG_JAPANESE && menuSelBot > 1)
        _chsMenuShift = (max(12, ceil(string_height("中")) + 1) - 8) * menuSelBot;
    var _showCopyright = true;
""",1);
// Label rows, native-language A/B branches, and SUB_SELECTED share this expression.
TitleBottomChecked567("152 + (_lineSpacing * i)", "152 - _chsMenuShift + (_lineSpacing * i)",8);
// Import normalizes the existing compact A/B expression; the preceding replacement covers it.
// Native hand cursor in normal and selected states, and the intro-style arrow.
TitleBottomChecked567("149 + (menuSel * _lineSpacing)", "149 - _chsMenuShift + (menuSel * _lineSpacing)",2);
TitleBottomChecked567("152 + (menuSel * _lineSpacing)", "152 - _chsMenuShift + (menuSel * _lineSpacing)",1);
var titleBottomAfter567=TitleBottomCompiledSource567();
if(titleBottomAfter567.Split(new[]{"_chsMenuShift"},System.StringSplitOptions.None).Length-1!=13)
    throw new System.Exception("Shared title shift must have two setup references and eleven synchronized row/cursor references");
ScriptMessage("Shared title bottom anchor counts 1/8/2/1; synchronized shift references 13 passed.");

// Root witnessed CHS title/body overlap in cursor12-avianos-2/3 before this fix.
// Run after the existing AVIANOS mixed-font route; supports either ext call name.
// Chinese-only miracle description panel, original EN/JP drawing block retained.
var miraclePanelCode567 = Data.Code.ByName("gml_Object_o50_Game_Draw_0");
var miraclePanelSource567 = GetDecompiledText(miraclePanelCode567, new GlobalDecompileContext(Data));
var miraclePanelPattern567 = @"var mirIndex = act\.params\[subY\]\[0\];\s*draw_sprite\(s50_ActionWindow, 0, HUD_LEFT \+ hudShiftX \+ HUD_WIDTH \+ 8, \(ACTION_TOP \+ \(ACTION_HEIGHT \* actionIndex\)\) - 8\);\s*draw_sprite\(s50_Miracles, mirIndex, HUD_LEFT \+ hudShiftX \+ HUD_WIDTH \+ 16, ACTION_TOP \+ \(ACTION_HEIGHT \* actionIndex\)\);\s*draw_set_color\(global\.palette\[14\]\);\s*(?:draw_text|UFO50_CHS_draw_avianos_mixed)\(HUD_LEFT \+ hudShiftX \+ HUD_WIDTH \+ 56, ACTION_TOP \+ \(ACTION_HEIGHT \* actionIndex\), MIR_NAMES\[mirIndex\]\);\s*draw_set_color\(c_white\);\s*(?:draw_text_ext|UFO50_CHS_draw_text_ext)\(HUD_LEFT \+ hudShiftX \+ HUD_WIDTH \+ 56, ACTION_TOP \+ \(ACTION_HEIGHT \* actionIndex\) \+ 8, MIR_DESC\[mirIndex\], 8, 128\);";
var miraclePanelMatches567 = System.Text.RegularExpressions.Regex.Matches(miraclePanelSource567, miraclePanelPattern567);
if (miraclePanelMatches567.Count != 1)
    throw new System.Exception("AVIANOS miracle panel anchor count != 1: " + miraclePanelMatches567.Count);
var miraclePanelOriginal567 = miraclePanelMatches567[0].Value;
var miraclePanelElse567 = miraclePanelOriginal567.Substring(miraclePanelOriginal567.IndexOf(';') + 1);
var miraclePanelCHS567 = """
var mirIndex = act.params[subY][0];
                            if (global.language == global.LANG_JAPANESE)
                            {
                                // Measure and draw the same complete wrapped lines.
                                var _mirLines = UFO50_CHS_wrap_text_array(MIR_DESC[mirIndex], 128, 0);
                                var _mirGlyphHeight = max(12, ceil(string_height("中")));
                                var _mirLineStep = max(14, _mirGlyphHeight + 2);
                                var _mirTitleGap = _mirLineStep;
                                var _mirBodyHeight = (array_length(_mirLines) - 1) * _mirLineStep + _mirGlyphHeight;
                                var _mirSpriteWidth = sprite_get_width(s50_ActionWindow);
                                var _mirSpriteHeight = sprite_get_height(s50_ActionWindow);
                                var _mirPanelHeight = max(_mirSpriteHeight, ceil((16 + _mirTitleGap + _mirBodyHeight + 8) / 8) * 8);
                                var _mirPanelLeft = HUD_LEFT + hudShiftX + HUD_WIDTH + 8;
                                // Keep the panel above the original y200 message line.
                                var _mirPanelTop = min(ACTION_TOP + ACTION_HEIGHT * actionIndex - 16, 200 - _mirPanelHeight);
                                var _mirTitleY = _mirPanelTop + 16;
                                // Original top/bottom 8px borders, stretched empty middle.
                                draw_sprite_part_ext(s50_ActionWindow, 0, 0, 0, _mirSpriteWidth, 8, _mirPanelLeft, _mirPanelTop, 1, 1, c_white, draw_get_alpha());
                                draw_sprite_part_ext(s50_ActionWindow, 0, 0, 8, _mirSpriteWidth, _mirSpriteHeight - 16, _mirPanelLeft, _mirPanelTop + 8, 1, (_mirPanelHeight - 16) / (_mirSpriteHeight - 16), c_white, draw_get_alpha());
                                draw_sprite_part_ext(s50_ActionWindow, 0, 0, _mirSpriteHeight - 8, _mirSpriteWidth, 8, _mirPanelLeft, _mirPanelTop + _mirPanelHeight - 8, 1, 1, c_white, draw_get_alpha());
                                draw_sprite(s50_Miracles, mirIndex, HUD_LEFT + hudShiftX + HUD_WIDTH + 16, _mirTitleY);
                                draw_set_color(global.palette[14]);
                                UFO50_CHS_draw_avianos_mixed(HUD_LEFT + hudShiftX + HUD_WIDTH + 56, _mirTitleY, MIR_NAMES[mirIndex]);
                                draw_set_color(c_white);
                                for (var _mirRow = 0; _mirRow < array_length(_mirLines); _mirRow++)
                                    UFO50_CHS_draw_avianos_mixed(HUD_LEFT + hudShiftX + HUD_WIDTH + 56, _mirTitleY + _mirTitleGap + _mirRow * _mirLineStep, _mirLines[_mirRow]);
                            }
                            else
                            {
""" + miraclePanelElse567 + "\n                            }";
var miraclePanelImports567 = new UndertaleModLib.Compiler.CodeImportGroup(Data);
miraclePanelImports567.ThrowOnNoOpFindReplace = true;
miraclePanelImports567.QueueFindReplace(miraclePanelCode567, miraclePanelOriginal567, miraclePanelCHS567, true);
miraclePanelImports567.Import();
ScriptMessage("AVIANOS CHS miracle panel: complete128px wrapped body, title/body gap>=14px, borders measured, panel bottom<=200; original other-language branch retained.");
// Task-only candidate. Root approved after new15 real-game image review.
// Append after base FixLayout candidates, before global draw redirect.
// Depends on the base patch's FixLayout and existing UFO50_CHS draw helpers.


var dust4647Helpers="""
function UFO50_CHS_dust_title_letter(_x,_y,_name,_index) {
    var _font=draw_get_font(),_request=global.chsRequestedFont,_current=global.currFont,_align=draw_get_halign();
    scrSetFont(global.fontDefault);
    draw_set_halign(fa_left);
    // Original 8px formula places the complete block around x+16.
    // Source character indices and the caller's vertical reveal offset remain original.
    var _measureFont=draw_get_font(),_total=0,_prefix=0;
    for(var _i=1;_i<=string_length(_name);_i++) {
        var _char=string_char_at(_name,_i),_native=UFO50_CHS_number_font(_char);
        if(_native>=0)draw_set_font(_native);
        var _width=string_width(_char);
        draw_set_font(_measureFont);
        if(_i<_index)_prefix+=_width;
        _total+=_width;
    }
    var _left=_x+16-floor(_total/2);
    UFO50_CHS_draw_text(_left+_prefix,_y,string_char_at(_name,_index));
    draw_set_font(_font);global.chsRequestedFont=_request;global.currFont=_current;draw_set_halign(_align);
}

function UFO50_CHS_46_warning_lines(_x,_y,_first,_second) {
    var _font=draw_get_font(),_request=global.chsRequestedFont,_current=global.currFont,_align=draw_get_halign();
    scrSetFont(global.fontDefault);
    draw_set_halign(fa_left);
    var _step=max(12,ceil(string_height("中"))+1);
    UFO50_CHS_draw_text(_x,_y,_first);
    UFO50_CHS_draw_text(_x,_y+_step,_second);
    draw_set_font(_font);global.chsRequestedFont=_request;global.currFont=_current;draw_set_halign(_align);
}

function UFO50_CHS_47_original_credit(_x,_y,_text) {
    var _font=draw_get_font(),_request=global.chsRequestedFont,_current=global.currFont,_align=draw_get_halign();
    // These rows come from the unchanged English manual language slot.
    // Reproduce original draw_text_ce(...,4): original even-padding and 8px width.
    var _even=string_even(_text,3);
    var _left=_x-floor(string_length(_even)*8/2);
    draw_set_font(global.fontDefault);global.chsRequestedFont=global.fontDefault;
    draw_set_halign(fa_left);
    // The CHS wrapper subtracts one for nonnumeric strings; cancel that for the native font.
    var _cancel=(UFO50_CHS_number_font(_even)<0)?1:0;
    UFO50_CHS_draw_text(_left,_y+_cancel,_even);
    draw_set_font(_font);global.chsRequestedFont=_request;global.currFont=_current;draw_set_halign(_align);
}

""";
var dust4647Functions=Regex.Matches(dust4647Helpers,@"(?m)^function (\w+)\(");
// Complete each function registration before compiling any caller.
for(var dust4647i=0;dust4647i<dust4647Functions.Count;dust4647i++) {
    var m=dust4647Functions[dust4647i];
    var end=(dust4647i+1<dust4647Functions.Count)?dust4647Functions[dust4647i+1].Index:dust4647Helpers.Length;
    var imports=new CodeImportGroup(Data);imports.AutoCreateAssets=true;
    imports.QueueReplace("gml_GlobalScript_"+m.Groups[1].Value,dust4647Helpers.Substring(m.Index,end-m.Index));
    imports.Import();
}
FixLayout("gml_Object_oIcon_Draw_0",
    "            draw_text(((x + (8 * i)) - (4 * string_length(myName))) + 8, y - 16 - _letterYOffset, string_char_at(myName, i));",
    "            if(global.language==global.LANG_JAPANESE) UFO50_CHS_dust_title_letter(x,y-16-_letterYOffset,myName,i);\n            else draw_text(((x + (8 * i)) - (4 * string_length(myName))) + 8, y - 16 - _letterYOffset, string_char_at(myName, i));");
FixLayout("gml_Object_o46_Mas_Draw_0",
    "        draw_text(_xv + 72, _yv + 16, scrString(\"boss_warning_1\"));\n        draw_text(_xv + 72, _yv + 24, scrString(\"boss_warning_2\"));",
    "        if(global.language==global.LANG_JAPANESE) UFO50_CHS_46_warning_lines(_xv+72,_yv+16,scrString(\"boss_warning_1\"),scrString(\"boss_warning_2\"));\n        else {\n        draw_text(_xv + 72, _yv + 16, scrString(\"boss_warning_1\"));\n        draw_text(_xv + 72, _yv + 24, scrString(\"boss_warning_2\"));\n        }");
FixLayout("gml_Object_o47_EndingCut_Draw_0",
    "        scrStringDrawCE(textx, credity, \"credits_title\", 4);",
    "        if(global.language==global.LANG_JAPANESE) UFO50_CHS_47_original_credit(textx,credity,scrString(\"credits_title\"));\n        else scrStringDrawCE(textx, credity, \"credits_title\", 4);");
FixLayout("gml_Object_o47_EndingCut_Draw_0",
    "            draw_text_ce(textx, true_round(credity + 80 + (8 * i)), scrStringManual(\"game_credits_47_\" + string(i + 1), 0), 4);",
    "            if(global.language==global.LANG_JAPANESE) UFO50_CHS_47_original_credit(textx,true_round(credity+80+8*i),scrStringManual(\"game_credits_47_\"+string(i+1),0));\n            else draw_text_ce(textx, true_round(credity + 80 + (8 * i)), scrStringManual(\"game_credits_47_\" + string(i + 1), 0), 4);");

var chsDrawText = Data.Code.ByName("gml_GlobalScript_UFO50_CHS_draw_text");
var chsDrawTextExt = Data.Code.ByName("gml_GlobalScript_UFO50_CHS_draw_text_ext");
var chsDrawTextColor = Data.Code.ByName("gml_GlobalScript_UFO50_CHS_draw_text_color");
var chsAvianosMixed = Data.Code.ByName("gml_GlobalScript_UFO50_CHS_draw_avianos_mixed");
if (chsDrawText == null || chsDrawTextExt == null || chsDrawTextColor == null || chsAvianosMixed == null)
    throw new System.Exception("Failed to create CHS baseline wrapper code entries.");

var chsMaskedCounter = Data.Code.ByName("gml_GlobalScript_UFO50_CHS_draw_masked_counter");
if (chsMaskedCounter == null) throw new System.Exception("Missing original-font masked counter.");
var chsCardText = Data.Code.ByName("gml_GlobalScript_UFO50_CHS_draw_card_text");
if (chsCardText == null) throw new System.Exception("Missing outlined card text helper.");
var wrapperCodes = new HashSet<UndertaleCode>() { Data.Code.ByName("gml_GlobalScript_UFO50_CHS_draw_fixed_ascii"), chsDrawText, chsDrawTextExt, chsDrawTextColor, chsAvianosMixed, chsMaskedCounter, chsCardText };
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
// Post-native-redirection candidate21. Original sinusoidal animation remains per glyph.
// Chinese nominal baseline -2; original numeral Y/font/phase remains.




var wave47g=new CodeImportGroup(Data);
wave47g.ThrowOnNoOpFindReplace=true;
var wave47c=Data.Code.ByName("gml_GlobalScript_UFO50_CHS_draw_seaside_wave");
var wave47s=GetDecompiledText(wave47c,new GlobalDecompileContext(Data));
var wave47a="UFO50_CHS_draw_text(_xx, _yy, _c);";
if(Regex.Matches(wave47s,Regex.Escape(wave47a)).Count!=1)throw new Exception("47 wave Y anchor");
wave47g.QueueRegexFindReplace(wave47c,Regex.Escape(wave47a),"UFO50_CHS_draw_text(_xx, _yy - ((ord(_c) >= 128) ? 2 : 0), _c);",true);
wave47g.Import();

// Post-native-redirection, validated against candidate19 basic real EN/JA/CHS scenes.
// Only label Y changes; original numerical Y/X, padded strings and request font remain.




var basic4046 = new CodeImportGroup(Data);
basic4046.ThrowOnNoOpFindReplace = true;
void Basic4046Replace(string name,string from,string to) {
 var c=Data.Code.ByName(name);if(c==null)throw new Exception(name);
 var src=GetDecompiledText(c,new GlobalDecompileContext(Data));
 if(Regex.Matches(src,Regex.Escape(from)).Count!=1)throw new Exception("basic19 anchor "+name);
 basic4046.QueueRegexFindReplace(c,Regex.Escape(from),to,true);
}
Basic4046Replace("gml_Object_o40_Mas_Draw_0",
 "UFO50_CHS_draw_text(xv + 16, yv + 24, scrStringLimit(\"cash\", 4));",
 "UFO50_CHS_draw_text(xv + 16, yv + 24 - ((global.language == global.LANG_JAPANESE) ? 2 : 0), scrStringLimit(\"cash\", 4));");
var base46=GetDecompiledText(Data.Code.ByName("gml_GlobalScript_UFO50_CHS_draw_labeled_number"),new GlobalDecompileContext(Data));
var label46="UFO50_CHS_draw_text(arg0, arg1, arg2);";
if(Regex.Matches(base46,Regex.Escape(label46)).Count!=1)throw new Exception("46 label helper anchor");
base46=base46.Replace("function UFO50_CHS_draw_labeled_number(","function UFO50_CHS_draw_shutter_next(").Replace(label46,"UFO50_CHS_draw_text(arg0, arg1 - 2, arg2);");
basic4046.QueueReplace("gml_GlobalScript_UFO50_CHS_draw_shutter_next",base46);
Basic4046Replace("gml_Object_o46_Mas_Draw_0",
 "UFO50_CHS_draw_labeled_number(_xv + 136, _yv + 160, scrString(\"to_next\") + \": \", strNext, 0);",
 "UFO50_CHS_draw_shutter_next(_xv + 136, _yv + 160, scrString(\"to_next\") + \": \", strNext, 0);");
basic4046.Import();

// Post-native-redirection candidate18; after full scenes and number crops.
// Label deltas only. Original fixed numerals retain font, request state and Y.




var align384347 = new CodeImportGroup(Data);
align384347.ThrowOnNoOpFindReplace = true;
void Align384347(string name,string call,int delta) {
 var code=Data.Code.ByName(name);var src=GetDecompiledText(code,new GlobalDecompileContext(Data));
 if(Regex.Matches(src,Regex.Escape(call)).Count!=1)throw new Exception("after18 alignment anchor "+name+" "+call);
 align384347.QueueRegexFindReplace(code,Regex.Escape(call),call.Substring(0,call.Length-2)+", "+delta+");",true);
}
Align384347("gml_Object_o38_Mas_Draw_0","UFO50_CHS_draw_fixed_mixed(_xview + 192, _yview + 16, _stage_num_string);",-2);
Align384347("gml_Object_o43_Game_Draw_0","UFO50_CHS_draw_fixed_mixed(192, 96, scrStringExt(\"game_x\", game, 24, 4));",2);
Align384347("gml_Object_o43_Game_Draw_0","UFO50_CHS_draw_fixed_mixed(192, 96, scrStringExt(\"red_team_wins\", 0, 24, 4));",2);
Align384347("gml_Object_o43_Game_Draw_0","UFO50_CHS_draw_fixed_mixed(192, 96, scrStringExt(\"blue_team_wins\", 0, 24, 4));",2);
Align384347("gml_Object_o43_Game_Draw_0","UFO50_CHS_draw_fixed_mixed(192, 192, scrStringExt(\"num_blowouts\", blowouts, 24, 4));",-2);
Align384347("gml_Object_o47_LevelText_Draw_0","UFO50_CHS_draw_fixed_mixed(room_width / 2, (room_height / 2) - 16, leveltext);",-2);
Align384347("gml_Object_o47_Player_Draw_0","UFO50_CHS_draw_fixed_mixed(x, y - 32, scrStringExt(\"p2_explanation\", \"*\", 0, 0));",-2);
var ss47code=Data.Code.ByName("gml_GlobalScript_UFO50_CHS_draw_seaside_counter");
var ss47src=GetDecompiledText(ss47code,new GlobalDecompileContext(Data));
var ss47anchor="UFO50_CHS_draw_text(_xx, arg1, _parts[_j]);";
if(Regex.Matches(ss47src,Regex.Escape(ss47anchor)).Count!=1)throw new Exception("47 counter anchor");
align384347.QueueRegexFindReplace(ss47code,Regex.Escape(ss47anchor),"UFO50_CHS_draw_text(_xx, arg1 - ((_fonts[_j] == _font) ? 2 : 0), _parts[_j]);",true);
align384347.Import();

// Post-native-redirection stage, verified against candidate16.
// Round22 original User2 initializes roundedTime; User3 10->8 verified in CHS/EN/JA.




var warning22 = new CodeImportGroup(Data);
warning22.AutoCreateAssets = true;
warning22.ThrowOnNoOpFindReplace = true;
var warning22code = Data.Code.ByName("gml_Object_o49_Game_Draw_0");
var warning22source = GetDecompiledText(warning22code,new GlobalDecompileContext(Data));
var warning22anchor = "draw_text_bg_centered(warningX, 96, warningString, 0, 8, 8, true);";
if (Regex.Matches(warning22source,Regex.Escape(warning22anchor)).Count != 1) throw new Exception("49 warning22 unique anchor");
warning22.QueueReplace("gml_GlobalScript_UFO50_CHS_draw_paint_warning_bg", """
function UFO50_CHS_draw_paint_warning_bg(_x, _y, _text)
{
    var _font = draw_get_font();
    var _align = draw_get_halign();
    var _color = draw_get_color();
    var _layout = UFO50_CHS_fixed_mixed_layout(_text);
    var _start = _x - floor(_layout.total / 2);
    var _cursor = _start;
    var _height = max(8, ceil(string_height("中")));
    for (var _r = 0; _r < array_length(_layout.parts); _r++)
    {
        var _run = _layout.parts[_r];
        draw_set_font(_font);
        if (_layout.numeric[_r]) draw_set_font(UFO50_CHS_number_font(_run));
        for (var _q = 1; _q <= string_length(_run); _q++)
        {
            var _char = string_char_at(_run, _q);
            var _width = string_width(_char);
            if (_char != " ")
            {
                draw_set_color(c_black);
                draw_rectangle(_cursor, _y - 3, _cursor + _width - 1, _y + _height - 1, false);
            }
            _cursor += _width;
        }
    }
    draw_set_font(_font);
    draw_set_color(_color);
    draw_set_halign(fa_left);
    UFO50_CHS_draw_fixed_mixed(_start, _y, _text, -2);
    draw_set_halign(_align);
}
""");
warning22.QueueRegexFindReplace(warning22code,Regex.Escape(warning22anchor),"if (global.language == global.LANG_JAPANESE) UFO50_CHS_draw_paint_warning_bg(warningX, 96, warningString); else " + warning22anchor,true);
warning22.Import();

// Post-native-redirection stage, tested against candidate-design-sixteenth.win.
// Twentyfirst full CHS/EN/JA original Create/User/Step scenes inspected.




var ui21364649 = new CodeImportGroup(Data);
ui21364649.AutoCreateAssets = true;
ui21364649.ThrowOnNoOpFindReplace = true;
void FixUI21(string name, string find, string replace) {
    var code=Data.Code.ByName(name); var src=GetDecompiledText(code,new GlobalDecompileContext(Data));
    if (Regex.Matches(src,Regex.Escape(find)).Count != 1) throw new Exception("UI21 anchor " + name + " / " + find);
    ui21364649.QueueRegexFindReplace(code,Regex.Escape(find),replace,true);
}
foreach (var entry in new[] {
    new[]{"popularity","selCard.popularity"},new[]{"popularity_negative","abs(selCard.popularity)"},
    new[]{"money_maker","selCard.money"},new[]{"money_loser","abs(selCard.money)"}
}) {
    var call="UFO50_CHS_draw_text(304, lineY, scrStringExt(\""+entry[0]+"\", "+entry[1]+", 8, 0));";
    FixUI21("gml_Object_o36_Game_Draw_0",call,"if (global.language == global.LANG_JAPANESE) UFO50_CHS_draw_fixed_mixed(304, lineY, scrStringExt(\""+entry[0]+"\", "+entry[1]+", 8, 0), -2); else "+call);
}
FixUI21("gml_Object_o36_Game_Draw_0","UFO50_CHS_draw_text_ext(8, MESSAGE_Y, msg, 8, 280);","if (global.language == global.LANG_JAPANESE && state == STATE_WIN && numPlayers == 2) UFO50_CHS_draw_fixed_mixed(8, MESSAGE_Y, msg, -2); else UFO50_CHS_draw_text_ext(8, MESSAGE_Y, msg, 8, 280);");
FixUI21("gml_Object_o46_Mas_Draw_0","UFO50_CHS_draw_text(_xv + 136, _yv + 96 + (16 * i), string(i + 1) + \".\" + stageName[i]);","if (global.language == global.LANG_JAPANESE) UFO50_CHS_draw_fixed_mixed(_xv + 136, _yv + 96 + (16 * i), string(i + 1) + \".\" + stageName[i], -2); else UFO50_CHS_draw_text(_xv + 136, _yv + 96 + (16 * i), string(i + 1) + \".\" + stageName[i]);");
// PU Chinese white strokes occupy screenshot y192..211; original8px backing
// occupies y192..207. Extend this local backing to the measured Chinese height.
ui21364649.QueueReplace("gml_GlobalScript_UFO50_CHS_draw_paint_powerup_bg", """
function UFO50_CHS_draw_paint_powerup_bg(_x, _y, _text)
{
    var _oldColor = draw_get_color();
    var _oldAlign = draw_get_halign();
    var _xx = _x - floor(string_width(_text) / 2);
    var _start = _xx;
    var _height = max(8, ceil(string_height("中")));
    for (var _i = 1; _i <= string_length(_text); _i++)
    {
        var _char = string_char_at(_text, _i);
        var _width = string_width(_char);
        if (_char != " ")
        {
            draw_set_color(c_black);
            draw_rectangle(_xx, _y - 1, _xx + _width - 1, _y + _height - 1, false);
        }
        _xx += _width;
    }
    draw_set_color(_oldColor);
    draw_set_halign(fa_left);
    UFO50_CHS_draw_text(_start, _y, _text);
    draw_set_halign(_oldAlign);
}
""");
FixUI21("gml_Object_o49_Game_Draw_0","draw_text_bg_centered(192, 96, puString, 0, 8, 8, true);","if (global.language == global.LANG_JAPANESE) UFO50_CHS_draw_paint_powerup_bg(192, 96, puString); else draw_text_bg_centered(192, 96, puString, 0, 8, 8, true);");
ui21364649.Import();

// Twentieth CHS/EN/JA progress/resume inspected. Preserve original number Y.




var fixed5044 = new CodeImportGroup(Data);
fixed5044.ThrowOnNoOpFindReplace = true;
void Fix5044(string name, string find, string replace) {
    var code = Data.Code.ByName(name);
    var src = GetDecompiledText(code,new GlobalDecompileContext(Data));
    if (Regex.Matches(src,Regex.Escape(find)).Count != 1) throw new Exception("Fixed5044 anchor: " + name + " / " + find);
    fixed5044.QueueRegexFindReplace(code,Regex.Escape(find),replace,true);
}
var name50 = "gml_Object_o50_Game_Draw_0";
Fix5044(name50,"scrStringDraw(56, 176, \"custom_seventh_flag\");","if (global.language == global.LANG_JAPANESE) UFO50_CHS_draw_fixed_mixed(56, 176, scrString(\"custom_seventh_flag\"), -2); else scrStringDraw(56, 176, \"custom_seventh_flag\");");
Fix5044(name50,"scrStringDrawVal(224, 112, \"setting_second_player_bonus\", secondPlayerBonus);","if (global.language == global.LANG_JAPANESE) UFO50_CHS_draw_fixed_mixed(224, 112, scrStringVal(\"setting_second_player_bonus\", secondPlayerBonus), -2); else scrStringDrawVal(224, 112, \"setting_second_player_bonus\", secondPlayerBonus);");
Fix5044(name50,"UFO50_CHS_draw_avianos_mixed(224, 176, scrStringVal(\"setting_seventh_flag_2\", SEVENTH_FLAG_CUSTOM[seventhFlagIndex]));","if (global.language == global.LANG_JAPANESE) UFO50_CHS_draw_fixed_mixed(224, 176, scrStringVal(\"setting_seventh_flag_2\", SEVENTH_FLAG_CUSTOM[seventhFlagIndex]), -2); else UFO50_CHS_draw_avianos_mixed(224, 176, scrStringVal(\"setting_seventh_flag_2\", SEVENTH_FLAG_CUSTOM[seventhFlagIndex]));");
Fix5044(name50,"draw_text_centered(192, GRID_TOP + 72, endMsg, 8);","if (global.language == global.LANG_JAPANESE) { var _chsEndAlign = draw_get_halign(); draw_set_halign(fa_center); UFO50_CHS_draw_fixed_mixed(192, GRID_TOP + 72, endMsg, 2); draw_set_halign(_chsEndAlign); } else draw_text_centered(192, GRID_TOP + 72, endMsg, 8);");
Fix5044(name50,"UFO50_CHS_draw_text_ext(GRID_LEFT + 16, GRID_TOP + 96, endSubMsg, 8, 160);","if (global.language == global.LANG_JAPANESE) UFO50_CHS_draw_fixed_mixed(GRID_LEFT + 16, GRID_TOP + 96, endSubMsg, -2); else UFO50_CHS_draw_text_ext(GRID_LEFT + 16, GRID_TOP + 96, endSubMsg, 8, 160);");
// Notification is one original line; restrict replacement to original end-turn notification.
Fix5044(name50,"UFO50_CHS_draw_avianos_mixed(GRID_LEFT, 200, scrMessageWrite(2));","if (global.language == global.LANG_JAPANESE && state == STATE_END_TURN && (subState == 1.6 || subState == 1.7)) UFO50_CHS_draw_fixed_mixed(GRID_LEFT, 200, scrMessageWrite(2), -2); else UFO50_CHS_draw_avianos_mixed(GRID_LEFT, 200, scrMessageWrite(2));");
var helper44 = "gml_GlobalScript_UFO50_CHS_draw_pilot_menu_label";
Fix5044(helper44,"var _width = _prefixWidth + string_width(_label);","var _width = _prefixWidth + UFO50_CHS_fixed_mixed_width(_label);");
Fix5044(helper44,"draw_rectangle_color(arg0 - 1, arg1 - 2,","draw_rectangle_color(arg0 - 1, arg1 - 3,");
Fix5044(helper44,"UFO50_CHS_draw_text_color(arg0 + _prefixWidth, arg1, _label, arg3, arg3, arg3, arg3, 1);","var _chsMenuColor = draw_get_color(); var _chsMenuAlpha = draw_get_alpha(); draw_set_color(arg3); draw_set_alpha(1); UFO50_CHS_draw_fixed_mixed(arg0 + _prefixWidth, arg1, _label, -2); draw_set_color(_chsMenuColor); draw_set_alpha(_chsMenuAlpha);");
fixed5044.Import();

// FLOOR0 English/Japanese original and candidate14 Chinese start scene viewed.
// Fixed floor transition title; preserve original Tall numericY92.




var floor35Code = Data.Code.ByName("gml_Object_o35_Game_Draw_0");
var floor35Source = GetDecompiledText(floor35Code,new GlobalDecompileContext(Data));
var floor35Pattern = @"scrDrawTextCentered\(scrStringExt\(""descent_format"", currentFloor, 0, true\), 0, 92, 8, 384\);";
if(Regex.Matches(floor35Source,floor35Pattern).Count != 1) throw new Exception("Floor35 title anchor count!=1");
var floor35Group = new CodeImportGroup(Data);
floor35Group.QueueRegexFindReplace(floor35Code,floor35Pattern,
    "if (global.language == global.LANG_JAPANESE)\n    {\n        var _chsFloorAlign = draw_get_halign();\n        draw_set_halign(fa_center);\n        UFO50_CHS_draw_fixed_mixed(192, 92, scrStringExt(\"descent_format\", currentFloor, 0, true), 2);\n        draw_set_halign(_chsFloorAlign);\n    }\n    else scrDrawTextCentered(scrStringExt(\"descent_format\", currentFloor, 0, true), 0, 92, 8, 384);",true);
floor35Group.Import();

// Candidate14 same-scene full JPG and all 82 number crops inspected.
// Change Chinese label Y only; preserve original number Y and font.




var align364549 = new CodeImportGroup(Data);
align364549.ThrowOnNoOpFindReplace = true;
void Align364549(string name, string find, string replacement, int expected = 1)
{
    var code = Data.Code.ByName(name);
    var source = GetDecompiledText(code, new GlobalDecompileContext(Data));
    var count = Regex.Matches(source, find).Count;
    if (count != expected) throw new Exception($"Alignment anchor {name}: expected {expected}, found {count}");
    align364549.QueueRegexFindReplace(code, find, replacement, true);
}
void Align364549Call(string name, string call, int offset, int expected = 1)
{
    Align364549(name, Regex.Escape(call), call.Substring(0, call.Length - 2) + ", " + offset + ");", expected);
}
Align364549Call("gml_GlobalScript_scr36_DrawTextBG", "UFO50_CHS_draw_fixed_mixed(arg0, arg1 + 1, arg2);", -2);
Align364549("gml_GlobalScript_scr36_DrawTextBG", @"draw_rectangle\(arg0, arg1,", "draw_rectangle(arg0, arg1 - 2,");
Align364549Call("gml_Object_o36_Game_Draw_0", "UFO50_CHS_draw_fixed_mixed(232, 56, scrStringVal(\"best_streak\", longestStreak));", -2);
// Four original map-rendering branches share identical current/best calls.
Align364549Call("gml_GlobalScript_scr45_UIStageSelect", "UFO50_CHS_draw_fixed_mixed(_xview + 272, (_yview + 216) - 43, scrStringFormat(scrString(\"score_curr\"), string(score_45[sel])));", -2, 4);
Align364549Call("gml_GlobalScript_scr45_UIStageSelect", "UFO50_CHS_draw_fixed_mixed(_xview + 272, (_yview + 216) - 43, scrStringFormat(scrString(\"score_best\"), string(best_45[sel])));", -2, 4);
Align364549Call("gml_GlobalScript_scr45_UIStageSelect", "UFO50_CHS_draw_fixed_mixed(_xview + 272, (_yview + 216) - 19, scrStringFormat(scrString(\"score_full\"), _totalScore));", -2);
Align364549Call("gml_Object_o49_Game_Draw_0", "UFO50_CHS_draw_fixed_mixed(192, 64, levelString);", 2);
Align364549Call("gml_Object_o49_Game_Draw_0", "UFO50_CHS_draw_fixed_mixed(192, 88, scrStringVal(\"goal_percent\", paintGoal));", -2);
Align364549Call("gml_Object_o49_Game_Draw_0", "UFO50_CHS_draw_fixed_mixed(192, 144, scrStringVal(\"score\", totalScore));", -2);
Align364549Call("gml_Object_o49_Game_Draw_0", "UFO50_CHS_draw_fixed_mixed(192, 136, scoreString);", -2);
Align364549Call("gml_Object_o49_Game_Draw_0", "UFO50_CHS_draw_fixed_mixed(192, 64, scrStringVal(\"score\", totalScore));", -2);
Align364549Call("gml_Object_o49_Game_Draw_0", "UFO50_CHS_draw_fixed_mixed(_chsReadyX, 96, readyString);", -2);
Align364549("gml_Object_o49_Game_Draw_0", @"draw_rectangle\(_chsReadyCursor, 95,", "draw_rectangle(_chsReadyCursor, 93,");
Align364549Call("gml_Object_o49_Game_Draw_0", "UFO50_CHS_draw_fixed_mixed(_chsFinalX, 192, _chsFinalText);", -2);
Align364549Call("gml_Object_o49_Game_Draw_0", "UFO50_CHS_draw_fixed_mixed(_chsFinalX, 192, _chsFinalLabel);", -2);
// Tall glyph height13 vs Chinese height10 yields a half-pixel center difference.
// Integer offset+2 minimizes difference to0.5px without moving the original number.
Align364549Call("gml_Object_o49_Game_Draw_0", "UFO50_CHS_draw_fixed_mixed(192, 88, \"< \" + scrStringVal(\"level\", levelSel) + \" >\");", 2);
align364549.Import();

// Confirmed against candidate13 same-scene CHS full JPG and number close-ups.
// Keep original sprite-font number Y. Align only the Chinese label/name glyphs.




var align3542 = new CodeImportGroup(Data);
align3542.ThrowOnNoOpFindReplace = true;
void Align3542(string name, string find, string replacement)
{
    var code = Data.Code.ByName(name);
    var source = GetDecompiledText(code, new GlobalDecompileContext(Data));
    if (Regex.Matches(source, find).Count != 1) throw new Exception("Alignment anchor count != 1 " + name + " " + find);
    align3542.QueueRegexFindReplace(code, find, replacement, true);
}
Align3542("gml_GlobalScript_UFO50_CHS_draw_valbrace_shop",
    @"UFO50_CHS_draw_text\(_xv \+ 112, _rowY, _traits\[0\]\);",
    "UFO50_CHS_draw_text(_xv + 112, _rowY - 2, _traits[0]);");
Align3542("gml_GlobalScript_UFO50_CHS_draw_valbrace_shop",
    @"UFO50_CHS_draw_fixed_mixed\([^;\n]+_rowY,\s*scrStringFormat\(scrString\(""crone_trade_format""\),\s*"""",\s*scr35_ItemCost\(_item\)\)\);",
    "UFO50_CHS_draw_fixed_mixed((_xv + 80 + _w) - 12, _rowY, scrStringFormat(scrString(\"crone_trade_format\"), \"\", scr35_ItemCost(_item)), -2);");
Align3542("gml_GlobalScript_UFO50_CHS_draw_valbrace_shop",
    @"UFO50_CHS_draw_fixed_mixed\(_xv \+ 96, _balanceY \+ 8, _balance\);",
    "UFO50_CHS_draw_fixed_mixed(_xv + 96, _balanceY + 8, _balance, -2);");
Align3542("gml_Object_o42_Game_Draw_0",
    @"UFO50_CHS_draw_fixed_mixed\(192, 120, scrStringVal\(""team_x_wins"", endState \+ 1\)\);",
    "UFO50_CHS_draw_fixed_mixed(192, 120, scrStringVal(\"team_x_wins\", endState + 1), 1);");
align3542.Import();


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
