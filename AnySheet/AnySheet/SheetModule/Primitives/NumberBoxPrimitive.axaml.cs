using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices.JavaScript;
using System.Text.Json;
using System.Text.Json.Nodes;
using Avalonia;
using Avalonia.Controls;
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
        ["[allowDecimal]"] = LuaValueType.Boolean
    };
    
    public decimal? CurrentValue { get; set; } = 0;
    private decimal MinValue { get; init; } = 0;
    private decimal MaxValue { get; init; } = decimal.MaxValue;
    private bool IntegerOnly { get; init; } = false;
    
    private NumberBoxPrimitive _uiControl;
    
    [LuaMember("create")]
    private new static NumberBoxLua CreateLua(LuaTable args)
    {
        VerifyPositionArgs(args);
        LuaSandbox.VerifyTable(args, ConstructorArgs);
        
        var defaultValue = (decimal)LuaSandbox.GetTableValueOrDefault<double>(args, "defaultValue", 0);
        var minValue = (decimal)LuaSandbox.GetTableValueOrDefault<double>(args, "minValue", 0);
        // eventually i need to figure out how to actually set this to infinity
        var maxValue = (decimal)LuaSandbox.GetTableValueOrDefault<double>(args, "maxValue", 10000000);
        var integerOnly = !LuaSandbox.GetTableValueOrDefault(args, "allowDecimal", false);

        if (minValue > maxValue)
        {
            throw new ArgumentException("Minimum value must be less than or equal to maximum value.");
        }

        if (defaultValue < minValue || defaultValue > maxValue)
        {
            throw new ArgumentException("Default value must be between minimum and maximum values.");
        }

        if (integerOnly && (defaultValue % 1 != 0 || minValue % 1 != 0 || maxValue % 1 != 0))
        {
            throw new ArgumentException("Integer only mode requires default, minimum and maximum values to be " +
                                        "integers.");
        }

        return new NumberBoxLua
        {
            GridX = args["x"].Read<int>(),
            GridY = args["y"].Read<int>(),
            GridWidth = args["width"].Read<int>(),
            GridHeight = args["height"].Read<int>(),
            MinValue = minValue,
            MaxValue = maxValue,
            IntegerOnly = integerOnly,
            CurrentValue = defaultValue
        };
    }
    
    public static bool TryReadLua(LuaValue value, out NumberBoxLua module)
    {
        return value.TryRead(out module);
    }
    
    public override UserControl CreateUiControl()
    {
        _uiControl = new NumberBoxPrimitive(this, GridX, GridY, GridWidth, GridHeight, CurrentValue, MinValue, MaxValue,
                                            IntegerOnly);
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

    public override void LoadSaveObject(JsonObject obj)
    {
        if (obj["value"] != null && obj["value"]!.AsValue().TryGetValue<decimal>(out var value))
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
    private readonly int _width;
    private readonly int _height;
    
    public NumberBoxPrimitive(NumberBoxLua parent, int x, int y, int width, int height, decimal? defaultValue,
                              decimal minValue, decimal maxValue, bool integerOnly)
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

        NumberBox.Minimum = minValue;
        NumberBox.Maximum = maxValue;
        NumberBox.Value = defaultValue;
    }

    private void ValueChanged(object? sender, NumericUpDownValueChangedEventArgs args)
    {
        if (_integerOnly && args.NewValue != null && args.NewValue % 1 != 0)
        {
            NumberBox.Value = Math.Floor(args.NewValue.Value);
        }

        if (NumberBox.Value != null)
        {
            _parent.CurrentValue = NumberBox.Value;
        }
        
        NumberBox.FontSize = TextFitHelper.FindBestFontSize(
            (NumberBox.Value).ToString().PadLeft(3, '0'), NumberBox.FontFamily,
            (_width * SheetModule.GridSize) - NumberBox.Padding.Left - NumberBox.Padding.Right,
            (_height * SheetModule.GridSize) - NumberBox.Padding.Top - NumberBox.Padding.Bottom,
            NumberBox.TextAlignment, double.NaN);
    }
}