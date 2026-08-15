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
        if (global.language == global.LANG_JAPANESE && font_exists(global.fontDefault_CHS))
        {
            // 中文不能交给任何只包含拉丁/日文字形的精灵字体。
            // 统一路由也覆盖各子游戏运行时创建并通过变量传入的专用字体。
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
        var _noLineStart = "，。！？；：、）》】」』…";
        var _noLineEnd = "（《【「『";
        var _text = string_replace_all(arg0, global.CARRIAGE_RETURN, global.CARRIAGE_RETURN_SIMPLIFIED);
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
            if (_line != "" && string_width(_candidate) > _width)
            {
                var _nextLine = (_char == " ") ? "" : _char;
                if (_char != " ")
                {
                    var _lastPos = string_length(_line);
                    var _lastChar = string_char_at(_line, _lastPos);
                    if ((string_pos(_char, _noLineStart) > 0 || string_pos(_lastChar, _noLineEnd) > 0) && _lastPos > 1)
                    {
                        _line = string_delete(_line, _lastPos, 1);
                        _nextLine = _lastChar + _char;
                    }
                }
                _lines[_lineCount++] = _line;
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
                if (string_width(_line) > _width)
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
    var _useActualWidth = global.language == global.LANG_JAPANESE && global.currFont == global.fontDefault_CHS;
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
    var _useActualWidth = global.language == global.LANG_JAPANESE && global.currFont == global.fontDefault_CHS;
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
    var _useActualWidth = global.language == global.LANG_JAPANESE && global.currFont == global.fontDefault_CHS;
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
importGroup.QueueFindReplace("gml_GlobalScript_scrDrawTextCentered", "var strLen = string_length(arg0) * arg3;", "var strLen = (global.language == global.LANG_JAPANESE && global.currFont == global.fontDefault_CHS) ? string_width(arg0) : string_length(arg0) * arg3;", true);
importGroup.QueueFindReplace("gml_GlobalScript_scrDrawTextCenteredPoint", "strLenTemp = string_length(arg0) * arg3;", "strLenTemp = (global.language == global.LANG_JAPANESE && global.currFont == global.fontDefault_CHS) ? string_width(arg0) : string_length(arg0) * arg3;", true);
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
importGroup.QueueReplace("gml_GlobalScript_UFO50_CHS_draw_text", @"
function UFO50_CHS_draw_text(arg0, arg1, arg2)
{
    if (global.language == global.LANG_JAPANESE)
        arg1 -= 1;
    draw_text(arg0, arg1, arg2);
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
    if (global.language == global.LANG_JAPANESE)
    {
        arg1 -= 1;
        arg2 = UFO50_CHS_wrap_text(arg2, arg4);
        var _lineStep = max(8, ceil(string_height(""中"")));
        if (arg3 > 0 && arg3 < _lineStep)
            arg3 = _lineStep;
    }
    draw_text_ext(arg0, arg1, arg2, arg3, arg4);
}
");
importGroup.QueueReplace("gml_GlobalScript_UFO50_CHS_draw_text_color", @"
function UFO50_CHS_draw_text_color(arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7)
{
    if (global.language == global.LANG_JAPANESE)
        arg1 -= 1;
    draw_text_color(arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7);
}
");
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
var chsDrawText = Data.Code.ByName("gml_GlobalScript_UFO50_CHS_draw_text");
var chsDrawTextExt = Data.Code.ByName("gml_GlobalScript_UFO50_CHS_draw_text_ext");
var chsDrawTextColor = Data.Code.ByName("gml_GlobalScript_UFO50_CHS_draw_text_color");
var chsAvianosMixed = Data.Code.ByName("gml_GlobalScript_UFO50_CHS_draw_avianos_mixed");
if (chsDrawText == null || chsDrawTextExt == null || chsDrawTextColor == null || chsAvianosMixed == null)
    throw new System.Exception("Failed to create CHS baseline wrapper code entries.");

var wrapperCodes = new HashSet<UndertaleCode>() { chsDrawText, chsDrawTextExt, chsDrawTextColor, chsAvianosMixed };
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

foreach (var str in Data.Strings.Where(str => str.Content == "にほんご"))
    str.Content = "中文";
ScriptMessage($"UFO 50 CHS patch applied; baseline wrappers redirected draw_text={redirectedCalls["draw_text"]}, avianos_mixed={avianosDrawTextCalls}, draw_text_ext={redirectedCalls["draw_text_ext"]}, draw_text_color={redirectedCalls["draw_text_color"]}.");
