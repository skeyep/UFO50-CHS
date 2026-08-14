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

var importGroup = new UndertaleModLib.Compiler.CodeImportGroup(Data);
importGroup.AutoCreateAssets = true;
importGroup.ThrowOnNoOpFindReplace = true;
importGroup.QueueReplace("gml_GlobalScript_scrInitFonts", initFonts);
importGroup.QueueReplace("gml_GlobalScript_scrSetFont", setFont);
importGroup.QueueReplace("gml_GlobalScript_scrLoadLibraryText", loadLibraryText);
importGroup.QueueReplace("gml_GlobalScript_scrLoadProfile", loadProfile);
importGroup.QueueReplace("gml_GlobalScript_scrDrawProfile", drawProfile);
importGroup.QueueFindReplace("gml_Object_oLibrary_Draw_0", oldInfoBar, newInfoBar, true);
importGroup.QueueFindReplace("gml_Object_oLibrary_Other_24", "LINE_WIDTH = 20;", "LINE_WIDTH = (global.language == global.LANG_JAPANESE) ? 13 : 20;", true);
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
importGroup.QueueReplace("gml_GlobalScript_UFO50_CHS_draw_text_ext", @"
function UFO50_CHS_draw_text_ext(arg0, arg1, arg2, arg3, arg4)
{
    if (global.language == global.LANG_JAPANESE)
        arg1 -= 1;
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
importGroup.Import();

// Zpix 保持官方原文件不变。GameMaker 没有 TTF 基线偏移参数，因此把三个
// 实际使用的内置文字绘制函数重定向到包装脚本，只在中文（日语槽）把 Y 上移 1px。
var chsDrawText = Data.Code.ByName("gml_GlobalScript_UFO50_CHS_draw_text");
var chsDrawTextExt = Data.Code.ByName("gml_GlobalScript_UFO50_CHS_draw_text_ext");
var chsDrawTextColor = Data.Code.ByName("gml_GlobalScript_UFO50_CHS_draw_text_color");
if (chsDrawText == null || chsDrawTextExt == null || chsDrawTextColor == null)
    throw new System.Exception("Failed to create CHS baseline wrapper code entries.");

var wrapperCodes = new HashSet<UndertaleCode>() { chsDrawText, chsDrawTextExt, chsDrawTextColor };
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
if (redirectedCalls["draw_text"] < 1500 || redirectedCalls["draw_text_ext"] < 60 || redirectedCalls["draw_text_color"] < 15)
    throw new System.Exception($"Unexpected text-call coverage: draw_text={redirectedCalls["draw_text"]}, draw_text_ext={redirectedCalls["draw_text_ext"]}, draw_text_color={redirectedCalls["draw_text_color"]}");
baselineImportGroup.Import();

foreach (var str in Data.Strings.Where(str => str.Content == "にほんご"))
    str.Content = "中文";
ScriptMessage($"UFO 50 CHS patch applied; baseline wrappers redirected draw_text={redirectedCalls["draw_text"]}, draw_text_ext={redirectedCalls["draw_text_ext"]}, draw_text_color={redirectedCalls["draw_text_color"]}.");
