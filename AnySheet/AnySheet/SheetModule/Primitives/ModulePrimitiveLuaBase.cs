using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Lua;
using LuaLib;

namespace AnySheet.SheetModule.Primitives;

/// <summary>
/// Base lua API class for all module primitives. All classes that inherit from this must have the
/// <c>[LuaObject]</c> attribute (this class can't have it for technical reasons). The following members need the
/// <c>[LuaMember]</c> attribute with the corresponding lua name:
/// <list type="bullet">
///     <item><description><c>GridWidth</c>: <c>"width"</c></description></item>
///     <item><description><c>CreateLua</c>: <c>"create"</c></description></item>
/// </list>
///
/// <c>CreateLua</c> must be overriden to return a new instance of the class. The argument object should at least
/// contain the <c>"x"</c>, <c>"y"</c>, <c>"width"</c>, and <c>"height"</c> keys. <c>CreateUiControl</c> should return a
/// new instance of the corresponding UI control. <c>FromLua</c> should do nothing except read the <c>LuaValue</c> as an
/// instance of the class and return it.
/// </summary>
public abstract class ModulePrimitiveLuaBase
{
    private static readonly Dictionary<string, LuaValueType> PositionArgs = new()
    {
        ["x"] = LuaValueType.Number,
        ["y"] = LuaValueType.Number,
        ["width"] = LuaValueType.Number,
        ["height"] = LuaValueType.Number
    };
    
    public int GridX { get; protected init; }
    public int GridY { get; protected init; }
    public int GridWidth { get; protected init; }
    public int GridHeight { get; protected init; }
    
    public abstract UserControl CreateUiControl();

    protected static void VerifyPositionArgs(LuaTable args)
    {
        LuaSandbox.VerifyTable(args, PositionArgs);
        var x = args["x"].Read<float>();
        var y = args["y"].Read<float>();
        var width = args["width"].Read<float>();
        var height = args["height"].Read<float>();
        if (x % 1 != 0 || x < 0)
        {
            throw new ArgumentException("Module x coordinate must be a positive integer.");
        }
        if (y % 1 != 0 || y < 0)
        {
            throw new ArgumentException("Module y coordinate must be a positive integer.");
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
    public static ModulePrimitiveLuaBase FromLua(LuaValue value)
    {
        throw new InvalidOperationException("How did you even get here??");
    }
    // ReSharper disable once UnusedMember.Global
    public static ModulePrimitiveLuaBase CreateLua(LuaTable args)
    {
        throw new InvalidOperationException("How did you even get here??");
    }
}