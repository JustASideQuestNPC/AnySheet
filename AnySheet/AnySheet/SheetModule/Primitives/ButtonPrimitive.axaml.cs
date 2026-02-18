using Avalonia.Controls;
using Avalonia.Interactivity;
using Lua;
using LuaLib;
using Material.Icons;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json.Nodes;

namespace AnySheet.SheetModule.Primitives;

[LuaObject]
public partial class ButtonLua : ModulePrimitiveLuaBase
{
    private static readonly Dictionary<string, LuaValueType> ConstructorArgs = new()
    {
        ["icon"] = LuaValueType.String,
        ["callback"] = LuaValueType.Function
    };

    private string _icon = "";
    private LuaFunction _callback = null!;

    private ButtonPrimitive _uiControl = null!;

    [LuaMember("create")]
    private new static ButtonLua CreateLua(LuaTable args)
    {
        VerifyPositionArgs(args);
        LuaSandbox.VerifyTable(args, ConstructorArgs);
        
        var icon = args["icon"].Read<string>();
        var callback = args["callback"].Read<LuaFunction>();
        
        return new ButtonLua
        {
            GridX = args["x"].Read<int>(),
            GridY = args["y"].Read<int>(),
            GridWidth = args["width"].Read<int>(),
            GridHeight = args["height"].Read<int>(),
            _icon = icon,
            _callback = callback
        };
    }

    public static bool TryReadLua(LuaValue value, out ButtonLua module)
    {
        return value.TryRead(out module);
    }
    
    public override UserControl CreateUiControl()
    {
        _uiControl = new ButtonPrimitive(this, GridX, GridY, GridWidth, GridHeight, _icon, _callback);
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

    public override bool HasBeenModified => false;
    public override JsonObject? GetSaveObject() => null;
    public override void LoadSaveObject(JsonObject? obj) {}
}

public partial class ButtonPrimitive : UserControl
{
    private LuaFunction _callback;
    private ButtonLua _parent;
    
    public ButtonPrimitive(ButtonLua parent, int x, int y, int width, int height, string icon, LuaFunction callback)
    {
        InitializeComponent();
        
        Grid.SetColumn(this, x);
        Grid.SetRow(this, y);
        Grid.SetColumnSpan(this, width);
        Grid.SetRowSpan(this, height);
        
        _callback = callback;
        _parent = parent;

        try
        {
            var textInfo = new CultureInfo("en-US", false).TextInfo;
            icon = textInfo.ToTitleCase(icon.ToLower());
            icon = icon.Replace("_", "").Replace(" ", "").Replace("-", "");
            
            Icon.Kind = Enum.Parse<MaterialIconKind>(icon);
        }
        catch
        {
            Icon.Kind = MaterialIconKind.Help;
        }
    }
    
    public void OnClick(object? sender, RoutedEventArgs? args)
    {
        _parent.Lua.DoFunctionAsync(_callback);
    }
}