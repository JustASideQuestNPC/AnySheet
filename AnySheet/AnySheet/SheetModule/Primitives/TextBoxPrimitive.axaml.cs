using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using CCLibrary;
using Lua;
using LuaLib;

namespace AnySheet.SheetModule.Primitives;

[LuaObject]
public partial class TextBoxLua : ModulePrimitiveLuaBase
{
    private static readonly Dictionary<string, LuaValueType> ConstructorArgs = new()
    {
        ["[defaultText]"] = LuaValueType.String,
        ["[alignment]"] = LuaValueType.String,
        ["[style]"] = LuaValueType.String
    };

    private string _text = "";
    private string _alignment = "";
    private string _fontStyle = "";
    
    [LuaMember("create")]
    private new static TextBoxLua CreateLua(LuaTable args)
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
        
        var module = new TextBoxLua
        {
            GridX = args["x"].Read<int>(),
            GridY = args["y"].Read<int>(),
            GridWidth = args["width"].Read<int>(),
            GridHeight = args["height"].Read<int>(),
            _text = LuaSandbox.GetTableValueOrDefault(args, "defaultText", ""),
            _alignment = alignment,
            _fontStyle = fontStyle
        };
        return module;
    }

    // ReSharper disable once UnusedMember.Global
    public new static TextBoxLua FromLua(LuaValue value)
    {
        return value.Read<TextBoxLua>();
    }

    public override UserControl CreateUiControl()
    {
        return new TextBoxPrimitive(GridX, GridY, GridWidth, GridHeight);
    }
}

public partial class TextBoxPrimitive : UserControl
{
    public TextBoxPrimitive(int x, int y, int width, int height)
    {
        InitializeComponent();
        
        Grid.SetColumn(this, x);
        Grid.SetRow(this, y);
        Grid.SetColumnSpan(this, width);
        Grid.SetRowSpan(this, height);
        
        
        TextBox.FontSize = TextFitHelper.FindBestFontSize("X", FontFamily,
                                        (width * SheetModule.GridSize) - TextBox.Padding.Left - TextBox.Padding.Right,
                                        (height * SheetModule.GridSize) - TextBox.Padding.Top - TextBox.Padding.Bottom,
                                        TextAlignment.Left, TextBox.LineHeight);
    }
}