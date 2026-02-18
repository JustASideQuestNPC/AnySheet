using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Lua;
using LuaLib;
using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;

namespace AnySheet.SheetModule.Primitives;

[LuaObject]
public partial class DividerLua : ModulePrimitiveLuaBase
{
    private static readonly Dictionary<string, LuaValueType> ConstructorArgs = new()
    {
        ["x"] = LuaValueType.Number,
        ["y"] = LuaValueType.Number,
        ["direction"] = LuaValueType.String,
        ["color"] = LuaValueType.String,
        ["length"] = LuaValueType.Number,
        ["[thickness]"] = LuaValueType.Number,
        ["[capStart]"] = LuaValueType.Boolean,
        ["[capEnd]"] = LuaValueType.Boolean,
        ["[betweenSquares]"] = LuaValueType.Boolean,
    };

    private bool _horizontal = true;
    private int _length = 1;
    private string _color = "primary";
    private bool _capStart = true;
    private bool _capEnd = true;
    private int _thickness = 1;
    private bool _betweenSquares = false;

    [LuaMember("create")]
    private new static DividerLua CreateLua(LuaTable args)
    {
        LuaSandbox.VerifyTable(args, ConstructorArgs);
        var x = args["x"].Read<float>();
        var y = args["y"].Read<float>();
        var direction = args["direction"].Read<string>();
        var length = args["length"].Read<float>();
        var thickness = LuaSandbox.GetTableValueOrDefault(args, "thickness", 4);
        var color = LuaSandbox.GetTableValueOrDefault(args, "color", "primary");
        if (x % 1 != 0 || x < 0)
        {
            throw new ArgumentException("Module x coordinate must be a positive integer.");
        }

        if (y % 1 != 0 || y < 0)
        {
            throw new ArgumentException("Module y coordinate must be a positive integer.");
        }

        if (direction != "horizontal" && direction != "vertical")
        {
            throw new ArgumentException("Invalid divider direction (expected 'horizontal' or 'vertical', received " +
                                        $"'{direction}').");
        }

        if (length % 1 != 0 || length <= 0)
        {
            throw new ArgumentException("Divider length must be a positive integer.");
        }

        if (thickness % 1 != 0 || thickness <= 0)
        {
            throw new ArgumentException("Divider thickness must be a positive integer.");
        }

        if (color != "primary" && color != "secondary" && color != "tertiary" && color != "accent")
        {
            throw new ArgumentException("Invalid color value (expected 'primary', 'secondary', 'tertiary' or " +
                                        $"'accent', received '{color}').");
        }

        int width, height;
        if (direction == "horizontal")
        {
            width = (int)length;
            height = 1;
        }
        else
        {
            width = 1;
            height = (int)length;
        }

        return new DividerLua
        {
            GridX = (int)x,
            GridY = (int)y,
            GridWidth = width,
            GridHeight = height,
            _horizontal = (direction == "horizontal"),
            _length = (int)length,
            _thickness = thickness,
            _color = string.Concat(color[0].ToString().ToUpper(), color.AsSpan(1)),
            _capStart = LuaSandbox.GetTableValueOrDefault(args, "capStart", false),
            _capEnd = LuaSandbox.GetTableValueOrDefault(args, "capEnd", false),
            _betweenSquares = LuaSandbox.GetTableValueOrDefault(args, "betweenSquares", false)
        };
    }

    public static bool TryReadLua(LuaValue value, out DividerLua module)
    {
        return value.TryRead(out module);
    }

    public override UserControl CreateUiControl(int xOffset, int yOffset)
    {
        return new DividerPrimitive(GridX + xOffset, GridY + yOffset, _horizontal, _length, _thickness, _color,
                                    _capStart, _capEnd, _betweenSquares);
    }

    // dividers aren't interactive and save no data
    public override void EnableUiControl()
    {
    }

    public override void DisableUiControl()
    {
    }

    public override bool HasBeenModified => false;
    public override JsonObject? GetSaveObject() => null;

    public override void LoadSaveObject(JsonObject? obj)
    {
    }
}

public partial class DividerPrimitive : UserControl
{
    public DividerPrimitive(int x, int y, bool horizontal, int length, int thickness, string color, bool capStart,
        bool capEnd, bool betweenSquares)
    {
        InitializeComponent();

        Grid.SetColumn(this, x);
        Grid.SetRow(this, y);

        Divider.Fill = AppResources.GetResource<IBrush>(color);

        if (horizontal)
        {
            Grid.SetColumnSpan(this, length);
            Grid.SetRowSpan(this, betweenSquares ? 2 : 1);
            
            Divider.Height = thickness;
            Divider.VerticalAlignment = VerticalAlignment.Center;
            Divider.HorizontalAlignment = HorizontalAlignment.Stretch;

            if (capStart && capEnd)
            {
                Divider.Margin = new Thickness((float)SheetModule.GridSize / 2 - thickness, 0);
            }
            else if (capStart)
            {
                Divider.Margin = new Thickness((float)SheetModule.GridSize / 2 - thickness, 0, 0, 0);
            }
            else if (capEnd)
            {
                Divider.Margin = new Thickness(0, 0, (float)SheetModule.GridSize / 2 - thickness, 0);
            }
        }
        else
        {
            Grid.SetColumnSpan(this, betweenSquares ? 2 : 1);
            Grid.SetRowSpan(this, length);
            
            Divider.Width = thickness;
            Divider.VerticalAlignment = VerticalAlignment.Stretch;
            Divider.HorizontalAlignment = HorizontalAlignment.Center;
            
            if (capStart && capEnd)
            {
                Divider.Margin = new Thickness(0, (float)SheetModule.GridSize / 2 - thickness);
            }
            else if (capStart)
            {
                Divider.Margin = new Thickness(0, (float)SheetModule.GridSize / 2 - thickness, 0, 0);
            }
            else if (capEnd)
            {
                Divider.Margin = new Thickness(0, 0, 0, (float)SheetModule.GridSize / 2 - thickness);
            }
        }
    }
}