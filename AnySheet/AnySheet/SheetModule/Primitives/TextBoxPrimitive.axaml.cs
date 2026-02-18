using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using CustomControls;
using Lua;
using LuaLib;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace AnySheet.SheetModule.Primitives;

[LuaObject]
public partial class TextBoxLua : ModulePrimitiveLuaBase
{
    private static readonly Dictionary<string, LuaValueType> ConstructorArgs = new()
    {
        ["[defaultText]"] = LuaValueType.String,
        ["[color]"] = LuaValueType.String,
        ["[borderColor]"] = LuaValueType.String,
        ["[alignment]"] = LuaValueType.String,
        ["[style]"] = LuaValueType.String,
        ["[borderType]"] = LuaValueType.String
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
    private string _borderColor = "";
    private string _borderType = "";

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
        if (color != "primary" && color != "secondary" && color != "tertiary" && color != "accent")
        {
            throw new ArgumentException("Invalid color value (expected 'primary', 'secondary', 'tertiary' or " +
                                        $"'accent', received '{color}').");
        }
        
        var borderColor = args.ContainsKey("borderColor") ? args["borderColor"].Read<string>() :
            LuaSandbox.GetTableValueOrDefault(args, "color", "primary");
        if (borderColor != "primary" && borderColor != "secondary" && borderColor != "tertiary" &&
            borderColor != "accent")
        {
            throw new ArgumentException("Invalid color value (expected 'primary', 'secondary', 'tertiary' or " +
                                        $"'accent', received '{borderColor}').");
        }

        var borderType = LuaSandbox.GetTableValueOrDefault(args, "borderType", "underline");
        if (borderType != "underline" && borderType != "none" && borderType != "full")
        {
            throw new ArgumentException("Invalid border type value (expected 'underline', 'none' or 'full', received " +
                                        $"'{borderType}').");
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
            _color = string.Concat(color[0].ToString().ToUpper(), color.AsSpan(1)),
            _borderColor = string.Concat(borderColor[0].ToString().ToUpper(), borderColor.AsSpan(1)),
            _borderType = borderType
        };
    }

    // ReSharper disable once UnusedMember.Global
    public static bool TryReadLua(LuaValue value, out TextBoxLua module)
    {
        return value.TryRead(out module);
    }

    public override UserControl CreateUiControl(int xOffset, int yOffset)
    {
        _uiControl = new TextBoxPrimitive(this, GridX + xOffset, GridY + yOffset, GridWidth, GridHeight, _alignment,
                                          _fontStyle, _color, Text, _borderType, _borderColor);
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

    public override bool HasBeenModified => Text != _uiControl.TextBox.Text;
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
    private int _width;
    private int _height;
    
    public TextBoxPrimitive(TextBoxLua parent, int x, int y, int width, int height, string alignment, string fontStyle,
                            string color, string initialText, string borderType, string borderColor)
    {
        InitializeComponent();
        
        Grid.SetColumn(this, x);
        Grid.SetRow(this, y);
        Grid.SetColumnSpan(this, width);
        Grid.SetRowSpan(this, height);
        
        _parent = parent;
        _width = width;
        _height = height;

        if (width == 1 && height == 1)
        {
            TextBox.Padding = new Thickness(0);
        }
        else if (width == 1)
        {
            TextBox.Padding = new Thickness(0, 3);
        }
        else if (height == 1)
        {
            TextBox.Padding = new Thickness(3, 0);
        }
        
        TextBox.HorizontalContentAlignment = alignment switch {
            "left"   => HorizontalAlignment.Left,
            "center" => HorizontalAlignment.Center,
            _        => HorizontalAlignment.Right
        };
        TextBox.FontFamily = AppResources.GetResource<FontFamily>(AppResources.ModuleFonts[fontStyle]);
        TextBox.Foreground = AppResources.GetResource<IBrush>(color);
        // "X" is just a dummy string to calculate the font size
        TextBox.FontSize = TextFitHelper.FindBestFontSize("X", TextBox.FontFamily,
                                    (width * SheetModule.GridSize) - TextBox.Padding.Left - TextBox.Padding.Right - 2,
                                    (height * SheetModule.GridSize) - TextBox.Padding.Top - TextBox.Padding.Bottom - 2);
        TextBox.Watermark = initialText;
        
        switch (borderType)
        {
            case "none":
                Container.BorderBrush = Brushes.Transparent;
                Container.BorderThickness = new Thickness(0);
                break;
            case "underline":
                Container.BorderBrush = AppResources.GetResource<IBrush>(borderColor);
                Container.BorderThickness = new Thickness(0, 0, 0, 2);
                break;
            case "full":
                Container.BorderBrush = AppResources.GetResource<IBrush>(borderColor);
                Container.BorderThickness = new Thickness(2);
                break;
        }
    }

    private void TextChanged(object? sender, TextChangedEventArgs e)
    {
        _parent.Text = TextBox.Text!;
        TextBox.FontSize = TextFitHelper.FindBestFontSize(string.IsNullOrEmpty(TextBox.Text) ? "X" : TextBox.Text,
                                  TextBox.FontFamily,
                                  (_width * SheetModule.GridSize) - TextBox.Padding.Left - TextBox.Padding.Right - 2,
                                  (_height * SheetModule.GridSize) - TextBox.Padding.Top - TextBox.Padding.Bottom - 2);
    }
}