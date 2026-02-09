using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;
using Avalonia.Controls;
using Lua;
using LuaLib;

namespace AnySheet.SheetModule.Primitives;

[LuaObject]
public partial class ToggleButtonLua : ModulePrimitiveLuaBase
{
    private static readonly Dictionary<string, LuaValueType> ConstructorArgs = new()
    {
        ["x"] = LuaValueType.Number,
        ["y"] = LuaValueType.Number
    };
    private ToggleButtonPrimitive _uiControl = null!;
    
    private bool _state = false;

    [LuaMember("state")]
    private bool State
    {
        get => _state;
        set
        {
            _state = value;
            _uiControl.Button.IsChecked = value;
        }
    }
    
    [LuaMember("create")]
    private new static ToggleButtonLua CreateLua(LuaTable args)
    {
        LuaSandbox.VerifyTable(args, ConstructorArgs);
        var x = args["x"].Read<float>();
        var y = args["y"].Read<float>();
        if (x % 1 != 0 || x < 0)
        {
            throw new ArgumentException("Module x coordinate must be a positive integer.");
        }
        if (y % 1 != 0 || y < 0)
        {
            throw new ArgumentException("Module y coordinate must be a positive integer.");
        }

        return new ToggleButtonLua()
        {
            GridX = (int)x,
            GridY = (int)y,
            GridWidth = 1,
            GridHeight = 1
        };
    }

    public static bool TryReadLua(LuaValue value, out ToggleButtonLua module)
    {
        return value.TryRead(out module);
    }

    public override UserControl CreateUiControl()
    {
        _uiControl = new ToggleButtonPrimitive(GridX, GridY)
        {
            Button =
            {
                IsChecked = _state
            }
        };
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

    public override JsonObject? GetSaveObject()
    {
        return new JsonObject {["state"] = _uiControl.Button.IsChecked};
    }

    public override void LoadSaveObject(JsonObject? obj)
    {
        if (obj?["state"] != null && obj["state"]!.AsValue().TryGetValue<bool>(out var state))
        {
            _state = state;
            return;
        }

        throw new JsonException("Invalid save data for ToggleButtonPrimitive.");
    }
}

public partial class ToggleButtonPrimitive : UserControl
{
    // public bool GetState() {}
    
    public ToggleButtonPrimitive(int x, int y)
    {
        InitializeComponent();
        
        Grid.SetColumn(this, x);
        Grid.SetRow(this, y);
        Grid.SetColumnSpan(this, 1);
        Grid.SetRowSpan(this, 1);
    }
}