using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Lua;
using LuaLib;

namespace AnySheet.SheetModule.Primitives;

/// <summary>
/// Basic, uneditable block of text.
/// </summary>
[LuaObject]
public partial class StaticTextLua : ModulePrimitiveLuaBase
{
    private static readonly Dictionary<string, LuaValueType> ConstructorArgs = new()
    {
        ["text"] = LuaValueType.String,
        ["[textColor]"] = LuaValueType.String,
        ["[backgroundColor]"] = LuaValueType.String,
        ["[alignment]"] = LuaValueType.String,
        ["[style]"] = LuaValueType.String
    };

    private string _text = "";
    private string _alignment = "";
    private string _fontStyle = "";
    
    [LuaMember("create")]
    private new static StaticTextLua CreateLua(LuaTable args)
    {
        VerifyPositionArgs(args);
        LuaSandbox.VerifyTable(args, ConstructorArgs);
        
        var alignment = LuaSandbox.GetTableValueOrDefault(args, "alignment", "left");
        if (alignment != "left" && alignment != "center" && alignment != "right")
        {
            throw new ArgumentException("Invalid alignment value (expected 'left', 'center' or 'right', received " +
                                        $"'{alignment}').");
        }
        
        var fontStyle = LuaSandbox.GetTableValueOrDefault(args, "style", "normal");
        if (fontStyle != "normal" && fontStyle != "bold" && fontStyle != "italic" && fontStyle != "bold italic")
        {
            throw new ArgumentException("Invalid font style value (expected 'normal', 'bold', 'italic' or " +
                                        $"'bold italic', received '{fontStyle}').");
        }
        
        var module = new StaticTextLua
        {
            GridX = args["x"].Read<int>(),
            GridY = args["y"].Read<int>(),
            GridWidth = args["width"].Read<int>(),
            GridHeight = args["height"].Read<int>(),
            _text = args["text"].Read<string>(),
            _alignment = alignment,
            _fontStyle = fontStyle
        };
        return module;
    }

    // ReSharper disable once UnusedMember.Global
    public new static StaticTextLua FromLua(LuaValue value)
    {
        return value.Read<StaticTextLua>();
    }

    public override UserControl CreateUiControl()
    {
        return new StaticTextControl(GridX, GridY, GridWidth, GridHeight, _text, _alignment, _fontStyle);
    }
}

public partial class StaticTextControl : UserControl
{
    public static readonly StyledProperty<string> DisplayTextProperty =
        AvaloniaProperty.Register<StaticTextControl, string>(
            nameof(DisplayText));

    public string DisplayText
    {
        get => GetValue(DisplayTextProperty);
        set => SetValue(DisplayTextProperty, value);
    }
    
    public StaticTextControl(int x, int y, int width, int height, string text, string alignment, string fontStyle)
    {
        InitializeComponent();
        
        Grid.SetColumn(this, x);
        Grid.SetRow(this, y);
        Grid.SetColumnSpan(this, width);
        Grid.SetRowSpan(this, height);

        DisplayText = text;
        TextBlock.TextAlignment = alignment switch {
            "left"   => TextAlignment.Left,
            "center" => TextAlignment.Center,
            _        => TextAlignment.Right
        };
        TextBlock.FontFamily = AppResources.GetResource<FontFamily>(AppResources.ModuleFonts[fontStyle]);
    }
}