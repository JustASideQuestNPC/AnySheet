using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Mime;
using System.Reflection;
using System.Runtime.InteropServices.JavaScript;
using System.Text.Json;
using System.Text.Json.Nodes;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Lua;
using LuaLib;
using CCLibrary;

namespace AnySheet.SheetModule.Primitives;

[LuaObject]
public partial class NumberBoxLua : ModulePrimitiveLuaBase
{
    private static readonly Dictionary<string, LuaValueType> ConstructorArgs = new()
    {
        ["[defaultValue]"] = LuaValueType.Number,
        ["[minValue]"] = LuaValueType.Number,
        ["[maxValue]"] = LuaValueType.Number,
        ["[allowDecimal]"] = LuaValueType.Boolean,
        ["[borderType]"] = LuaValueType.String,
        ["[borderColor]"] = LuaValueType.String,
    };

    private double _currentValue = 0;
    [LuaMember("value")]
    private double CurrentValue
    {
        get => _currentValue;
        set
        {
            _currentValue = value;
            if (_uiControl != null)
            {
                _uiControl.CurrentValue = CurrentValue;
            }
        }
    }
    
    private double _minValue = 0;
    [LuaMember("minValue")]
    private double MinValue
    {
        get => _minValue;
        set
        {
            _minValue = value;
            if (_uiControl != null)
            {
                _uiControl.MinValue = MinValue;
            }
        }
    }
    
    private double _maxValue = double.PositiveInfinity;
    [LuaMember("maxValue")]
    private double MaxValue
    {
        get => _maxValue;
        set
        {
            _maxValue = value;
            if (_uiControl != null)
            {
                _uiControl.MaxValue = MaxValue;
            }
        }
    }
    
    private bool IntegerOnly { get; init; }
    
    private NumberBoxPrimitive? _uiControl;
    
    private string _borderType = "";
    private string _borderColor = "";
    
    [LuaMember("create")]
    private new static NumberBoxLua CreateLua(LuaTable args)
    {
        VerifyPositionArgs(args);
        LuaSandbox.VerifyTable(args, ConstructorArgs);
        
        var defaultValue = LuaSandbox.GetTableValueOrDefault<double>(args, "defaultValue", 0);
        var minValue = LuaSandbox.GetTableValueOrDefault<double>(args, "minValue", 0);
        var maxValue = LuaSandbox.GetTableValueOrDefault(args, "maxValue", double.PositiveInfinity);
        var borderType = LuaSandbox.GetTableValueOrDefault(args, "borderType", "underline");
        var integerOnly = !LuaSandbox.GetTableValueOrDefault(args, "allowDecimal", false);
        var borderColor = LuaSandbox.GetTableValueOrDefault(args, "borderColor", "primary");

        if (minValue > maxValue)
        {
            throw new ArgumentException("Minimum value must be less than or equal to maximum value.");
        }

        if (defaultValue < minValue || defaultValue > maxValue)
        {
            throw new ArgumentException("Default value must be between minimum and maximum values.");
        }

        if (integerOnly && (defaultValue % 1 != 0 || 
                            (minValue % 1 != 0 && !double.IsNegativeInfinity(minValue)) ||
                            (maxValue % 1 != 0 && !double.IsPositiveInfinity(maxValue))))
        {
            throw new ArgumentException("Integer only mode requires default, minimum and maximum values to be " +
                                        "integers.");
        }
        
        if (borderType != "underline" && borderType != "none" && borderType != "full")
        {
            throw new ArgumentException("Invalid border type value (expected 'underline', 'none' or 'full', received " +
                                        $"'{borderType}').");
        }
        
        if (borderColor != "primary" && borderColor != "secondary" && borderColor != "tertiary" &&
            borderColor != "accent")
        {
            throw new ArgumentException("Invalid color value (expected 'primary', 'secondary', 'tertiary' or " +
                                        $"'accent', received '{borderColor}').");
        }

        return new NumberBoxLua
        {
            GridX = args["x"].Read<int>(),
            GridY = args["y"].Read<int>(),
            GridWidth = args["width"].Read<int>(),
            GridHeight = args["height"].Read<int>(),
            IntegerOnly = integerOnly,
            MinValue = minValue,
            MaxValue = maxValue,
            CurrentValue = defaultValue,
            _borderType = borderType,
            _borderColor = string.Concat(borderColor[0].ToString().ToUpper(), borderColor.AsSpan(1)),
        };
    }
    
    public static bool TryReadLua(LuaValue value, out NumberBoxLua module)
    {
        return value.TryRead(out module);
    }
    
    public override UserControl CreateUiControl()
    {
        _uiControl = new NumberBoxPrimitive(this, GridX, GridY, GridWidth, GridHeight, CurrentValue, MinValue, MaxValue,
                                            IntegerOnly, _borderType, _borderColor);
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
        return new JsonObject {["value"] = CurrentValue};
    }

    public override void LoadSaveObject(JsonObject? obj)
    {
        if (obj?["value"] != null && obj["value"]!.AsValue().TryGetValue<double>(out var value))
        {
            CurrentValue = value;
            return;
        }

        throw new JsonException("Invalid save data for NumberBoxPrimitive.");
    }
}

public partial class NumberBoxPrimitive : UserControl
{
    private readonly NumberBoxLua _parent;
    private readonly bool _integerOnly;

    private int _width;
    private int _height;

    private double _currentValue;
    public double CurrentValue
    {
        get => _currentValue;
        set
        {
            _currentValue = value;
            TextBox.Text = _currentValue.ToString(CultureInfo.InvariantCulture);
        }
    }
    
    private double _minValue;
    public double MinValue
    {
        get => _minValue;
        set
        {
            _minValue = value;
            CurrentValue = Math.Max(_currentValue, value);
        }
    }
    
    private double _maxValue;
    public double MaxValue
    {
        get => _maxValue;
        set
        {
            _maxValue = value;
            CurrentValue = Math.Min(_currentValue, value);
        }
    }
    
    public NumberBoxPrimitive(NumberBoxLua parent, int x, int y, int width, int height, double? defaultValue,
                              double minValue, double maxValue, bool integerOnly, string borderType,
                              string borderColor)
    {
        InitializeComponent();
        
        Grid.SetColumn(this, x);
        Grid.SetRow(this, y);
        Grid.SetColumnSpan(this, width);
        Grid.SetRowSpan(this, height);
        
        _parent = parent;
        _width = width;
        _height = height;
        
        _integerOnly = integerOnly;
        
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
        
        // "X" is just a dummy string to calculate the font size
        TextBox.FontSize = TextFitHelper.FindBestFontSize("X", FontFamily,
                                        (width * SheetModule.GridSize) - TextBox.Padding.Left - TextBox.Padding.Right,
                                        (height * SheetModule.GridSize) - TextBox.Padding.Top - TextBox.Padding.Bottom,
                                        TextBox.TextAlignment, TextBox.LineHeight);

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

        MinValue = minValue;
        MaxValue = maxValue;
        CurrentValue = Math.Clamp(defaultValue ?? 0, MinValue, MaxValue);
    }
    
    private void TextChanged(object? sender, TextChangedEventArgs e)
    {
        TextBox.FontSize = TextFitHelper.FindBestFontSize("X", FontFamily,
                                        (_width * SheetModule.GridSize) - TextBox.Padding.Left - TextBox.Padding.Right,
                                        (_height * SheetModule.GridSize) - TextBox.Padding.Top - TextBox.Padding.Bottom,
                                        TextBox.TextAlignment, TextBox.LineHeight);
    }
    
    private new void LostFocus(object? sender, RoutedEventArgs e)
    {
        if (!double.TryParse(TextBox.Text, out var newValue) || _integerOnly && newValue % 1 != 0)
        {
            TextBox.Text = _currentValue.ToString(CultureInfo.InvariantCulture);
        }
        else
        {
            CurrentValue = Math.Clamp(newValue, MinValue, MaxValue);
        }
    }
}