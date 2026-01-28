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
        ["[color]"] = LuaValueType.String,
        ["[alignment]"] = LuaValueType.String,
        ["[style]"] = LuaValueType.String
    };

    private string _text = "";
    private string _alignment = "";
    private string _fontStyle = "";
    private string _color = "";
    
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
        
        var color = LuaSandbox.GetTableValueOrDefault(args, "color", "primary");
        if (color != "primary" && color != "secondary" && color != "accent")
        {
            throw new ArgumentException("Invalid color value (expected 'primary', 'secondary' or 'accent', received " +
                                        $"'{color}').");
        }
        
        var module = new StaticTextLua
        {
            GridX = args["x"].Read<int>(),
            GridY = args["y"].Read<int>(),
            GridWidth = args["width"].Read<int>(),
            GridHeight = args["height"].Read<int>(),
            _text = args["text"].Read<string>(),
            _alignment = alignment,
            _fontStyle = fontStyle,
            _color = string.Concat(color[0].ToString().ToUpper(), color.AsSpan(1))
        };
        return module;
    }

    // ReSharper disable once UnusedMember.Global
    public static bool TryReadLua(LuaValue value, out StaticTextLua module)
    {
        return value.TryRead(out module);
    }

    public override UserControl CreateUiControl()
    {
        return new StaticTextPrimitive(GridX, GridY, GridWidth, GridHeight, _text, _alignment, _fontStyle, _color);
    }
}

public partial class StaticTextPrimitive : UserControl
{
    public static readonly StyledProperty<string> DisplayTextProperty =
        AvaloniaProperty.Register<StaticTextPrimitive, string>(
            nameof(DisplayText));

    public string DisplayText
    {
        get => GetValue(DisplayTextProperty);
        set => SetValue(DisplayTextProperty, value);
    }
    
    public StaticTextPrimitive(int x, int y, int width, int height, string text, string alignment, string fontStyle, 
                               string color)
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
        TextBlock.Foreground = AppResources.GetResource<IBrush>(color);
    }
}