using Avalonia.Controls;
using Lua;
using LuaLib;
using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;

namespace AnySheet.SheetModule.Primitives;

public abstract class ModulePrimitiveLuaBase
{
    private static readonly Dictionary<string, LuaValueType> PositionArgs = new()
    {
        ["x"] = LuaValueType.Number,
        ["y"] = LuaValueType.Number,
        ["width"] = LuaValueType.Number,
        ["height"] = LuaValueType.Number
    };

    public LuaSandbox Lua;
    
    [LuaMember("x")]
    public int GridX { get; set; }
    
    [LuaMember("y")]
    public int GridY { get; set; }
    
    [LuaMember("width")]
    public int GridWidth { get; protected init; }
    
    [LuaMember("height")]
    public int GridHeight { get; protected init; }
    public abstract bool HasBeenModified { get; }
    
    public abstract UserControl CreateUiControl(int xOffset, int yOffset);
    public abstract void EnableUiControl();
    public abstract void DisableUiControl();
    public abstract JsonObject? GetSaveObject();
    public abstract void LoadSaveObject(JsonObject? obj);

    public virtual void OnCameraMoveCompleted() { }

    protected static void VerifyPositionArgs(LuaTable args)
    {
        LuaSandbox.VerifyTable(args, PositionArgs);
        var x = args["x"].Read<float>();
        var y = args["y"].Read<float>();
        var width = args["width"].Read<float>();
        var height = args["height"].Read<float>();
        if (x % 1 != 0)
        {
            throw new ArgumentException("Module x coordinate must be an integer.");
        }
        if (y % 1 != 0)
        {
            throw new ArgumentException("Module y coordinate must be an integer.");
        }
        if (width % 1 != 0 || width <= 0)
        {
            throw new ArgumentException("Module width must be a positive integer.");
        }
        if (height % 1 != 0 || height <= 0)
        {
            throw new ArgumentException("Module height must be a positive integer.");
        }
    }
    // ReSharper disable once UnusedMember.Global
    public static bool TryReadLua(LuaValue value, out ModulePrimitiveLuaBase module)
    {
        throw new InvalidOperationException("How did you even get here??");
    }
    // ReSharper disable once UnusedMember.Global
    public static ModulePrimitiveLuaBase CreateLua(LuaTable args)
    {
        throw new InvalidOperationException("How did you even get here??");
    }
}