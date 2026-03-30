using Avalonia.Controls;
using Lua;
using LuaLib;
using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;

namespace AnySheet.SheetModule.Primitives;

/// <summary>
/// Base class for the Lua half of a module primitive. Each primitive should be paired with a corresponding Avalonia
/// control. The <code>TryReadLua()</code> and <code>CreateLua()</code> methods must be overriden (they can't be marked
/// as abstract for technical reasons).
/// </summary>
public abstract class ModulePrimitiveLuaBase
{
    /// <summary>
    /// Lua argument types for x, y, width, and height.
    /// </summary>
    private static readonly Dictionary<string, LuaValueType> PositionArgs = new()
    {
        ["x"] = LuaValueType.Number,
        ["y"] = LuaValueType.Number,
        ["width"] = LuaValueType.Number,
        ["height"] = LuaValueType.Number
    };

    public LuaSandbox Lua;
    
    [LuaMember("x")]
    public int GridX { get; protected init; }
    
    [LuaMember("y")]
    public int GridY { get; protected init; }
    
    [LuaMember("width")]
    public int GridWidth { get; protected init; }
    
    [LuaMember("height")]
    public int GridHeight { get; protected init; }
    
    [LuaMember("type")]
    public abstract string Type { get; }

    public abstract bool HasBeenModified();
    public abstract void ResetModified();

    /// <summary>
    /// Creates the Avalonia control for this primitive.
    /// </summary>
    /// <param name="xOffset">Where to place the primitive on the X axis, in grid squares.</param>
    /// <param name="yOffset">Where to place the primitive on the Y axis, in grid squares.</param>
    /// <returns></returns>
    public abstract UserControl CreateUiControl(int xOffset, int yOffset);
    /// <summary>
    /// Enables input for all ui elements in the Avalonia control for this primitive.
    /// </summary>
    public abstract void EnableUiControl();
    /// <summary>
    /// Disables input for all ui elements in the Avalonia control for this primitive.
    /// </summary>
    public abstract void DisableUiControl();
    /// <summary>
    /// Returns any save data (text box values, button states, etc.). If the primitive has no data to save, this can
    /// return either an empty <code>JsonObject</code> or null.
    /// </summary>
    public abstract JsonObject? GetSaveObject();
    /// <summary>
    /// Loads any save (text box values, button states, etc.).
    /// </summary>
    /// <param name="obj">The save data. This will be null if <code>GetSaveObject()</code> returns null.</param>
    public abstract void LoadSaveObject(JsonObject? obj);

    /// <summary>
    /// Helper method that verifies the X, Y, Width, and Height arguments in a Lua table.
    /// </summary>
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
    
    /// <summary>
    /// Attempts to read a Lua value as an instance of this primitive. This needs to be overriden but can't be marked
    /// abstract for technical reasons. The override should use the subclass's type for `module`, and do nothing except
    /// return <code>value.TryRead(out module)</code>.
    /// </summary>
    /// <returns>Whether the Lua value could be read.</returns>
    // ReSharper disable once UnusedMember.Global
    public static bool TryReadLua(LuaValue value, out ModulePrimitiveLuaBase module)
    {
        throw new InvalidOperationException("How did you even get here??");
    }
    /// <summary>
    /// Creates an instance of this primitive from an argument table. This needs to be overriden but can't be marked
    /// abstract for technical reasons.
    /// </summary>
    public static ModulePrimitiveLuaBase CreateLua(LuaTable args)
    {
        throw new InvalidOperationException("How did you even get here??");
    }
}