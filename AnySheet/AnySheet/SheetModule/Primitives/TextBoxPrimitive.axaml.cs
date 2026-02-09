using System;
using System.Collections.Generic;
using System.Data;
using System.Text.Json;
using System.Text.Json.Nodes;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
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
        ["[color]"] = LuaValueType.String,
        ["[alignment]"] = LuaValueType.String,
        ["[style]"] = LuaValueType.String
    };

    private string _text = "";

    [LuaMember("text")]
    public string Text
    {
        get => _text;
        set
        {
            _text = value;
            if (_uiControl != null) _uiControl.TextBox.Text = value;
        }
    }
    
    private string _alignment = "";
    private string _fontStyle = "";
    private string _color = "";

    private TextBoxPrimitive? _uiControl;

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

        var color = LuaSandbox.GetTableValueOrDefault(args, "color", "primary");
        if (color != "primary" && color != "secondary" && color != "accent")
        {
            throw new ArgumentException("Invalid color value (expected 'primary', 'secondary' or 'accent', received " +
                                        $"'{color}').");
        }

        return new TextBoxLua
        {
            GridX = args["x"].Read<int>(),
            GridY = args["y"].Read<int>(),
            GridWidth = args["width"].Read<int>(),
            GridHeight = args["height"].Read<int>(),
            Text = LuaSandbox.GetTableValueOrDefault(args, "defaultText", ""),
            _alignment = alignment,
            _fontStyle = fontStyle,
            _color = string.Concat(color[0].ToString().ToUpper(), color.AsSpan(1))
        };
    }

    // ReSharper disable once UnusedMember.Global
    public static bool TryReadLua(LuaValue value, out TextBoxLua module)
    {
        return value.TryRead(out module);
    }

    public override UserControl CreateUiControl()
    {
        _uiControl = new TextBoxPrimitive(this, GridX, GridY, GridWidth, GridHeight, _alignment, _fontStyle, _color,
                                          Text);
        return _uiControl;
    }

    public override void EnableUiControl()
    {
        _uiControl.IsEnabled = true;
    }

    public override void DisableUiControl()
    {
        _uiControl.IsEnabled = false;
    }

    public override JsonObject GetSaveObject()
    {
        return new JsonObject { ["text"] = Text };
    }

    public override void LoadSaveObject(JsonObject obj)
    {
        if (obj["text"] != null && obj["text"]!.AsValue().TryGetValue<string>(out var text))
        {
            Text = text;
            return;
        }

        throw new JsonException("Invalid save data for TextBoxPrimitive.");
    }
}

public partial class TextBoxPrimitive : UserControl
{
    private readonly TextBoxLua _parent;
    
    public TextBoxPrimitive(TextBoxLua parent, int x, int y, int width, int height, string alignment, string fontStyle,
                            string color, string initialText)
    {
        InitializeComponent();
        
        Grid.SetColumn(this, x);
        Grid.SetRow(this, y);
        Grid.SetColumnSpan(this, width);
        Grid.SetRowSpan(this, height);
        
        _parent = parent;
        
        TextBox.HorizontalContentAlignment = alignment switch {
            "left"   => HorizontalAlignment.Left,
            "center" => HorizontalAlignment.Center,
            _        => HorizontalAlignment.Right
        };
        TextBox.FontFamily = AppResources.GetResource<FontFamily>(AppResources.ModuleFonts[fontStyle]);
        TextBox.Foreground = AppResources.GetResource<IBrush>(color);
        // "X" is just a dummy string to calculate the font size
        TextBox.FontSize = TextFitHelper.FindBestFontSize("X", FontFamily,
                                        (width * SheetModule.GridSize) - TextBox.Padding.Left - TextBox.Padding.Right,
                                        (height * SheetModule.GridSize) - TextBox.Padding.Top - TextBox.Padding.Bottom,
                                        TextBox.TextAlignment, TextBox.LineHeight);
        TextBox.Text = initialText;
    }


    private void TextChanged(object? sender, TextChangedEventArgs e)
    {
        _parent.Text = TextBox.Text!;
    }
}