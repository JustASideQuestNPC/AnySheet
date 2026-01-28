using System;
using System.Collections.Generic;
using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Lua;
using LuaLib;

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
    
    public double CurrentValue { get; private set; } = 0;
    public double MinValue { get; private set; } = 0;
    public double MaxValue { get; private set; } = double.MaxValue;
    public bool IntegerOnly { get; private set; } = false;
    
    [LuaMember("create")]
    private new static NumberBoxLua CreateLua(LuaTable args)
    {
        VerifyPositionArgs(args);
        LuaSandbox.VerifyTable(args, ConstructorArgs);
        
        var defaultValue = LuaSandbox.GetTableValueOrDefault(args, "defaultValue", 0);
        var minValue = LuaSandbox.GetTableValueOrDefault(args, "minValue", 0);
        var maxValue = LuaSandbox.GetTableValueOrDefault(args, "maxValue", double.MaxValue);
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
        return new NumberBoxPrimitive(GridX, GridY, GridWidth, GridHeight, CurrentValue, MinValue, MaxValue,
                                      IntegerOnly);
    }
}

public partial class NumberBoxPrimitive : UserControl
{
    public NumberBoxPrimitive(int x, int y, int width, int height, double defaultValue, double minValue,
                              double maxValue, bool integerOnly)
    {
        InitializeComponent();
        
        Grid.SetColumn(this, x);
        Grid.SetRow(this, y);
        Grid.SetColumnSpan(this, width);
        Grid.SetRowSpan(this, height);
    }
}