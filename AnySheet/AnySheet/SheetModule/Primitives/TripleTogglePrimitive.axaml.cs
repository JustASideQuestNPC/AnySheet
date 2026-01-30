using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Lua;
using LuaLib;

namespace AnySheet.SheetModule.Primitives;

[LuaObject]
public partial class TripleToggleLua : ModulePrimitiveLuaBase
{
    private static readonly Dictionary<string, LuaValueType> ConstructorArgs = new()
    {
        ["x"] = LuaValueType.Number,
        ["y"] = LuaValueType.Number
    };

    private TripleTogglePrimitive _uiControl;

    [LuaMember("create")]
    private new static TripleToggleLua CreateLua(LuaTable args)
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

        return new TripleToggleLua()
        {
            GridX = (int)x,
            GridY = (int)y,
            GridWidth = 1,
            GridHeight = 1
        };
    }

    public static bool TryReadLua(LuaValue value, out TripleToggleLua module)
    {
        return value.TryRead(out module);
    }

    public override UserControl CreateUiControl()
    {
        _uiControl = new TripleTogglePrimitive(GridX, GridY);
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
}

public partial class TripleTogglePrimitive : UserControl
{
    private bool _buttonDisabled = false;
    
    public TripleTogglePrimitive(int x, int y)
    {
        InitializeComponent();
        
        Grid.SetColumn(this, x);
        Grid.SetRow(this, y);
        Grid.SetColumnSpan(this, 1);
        Grid.SetRowSpan(this, 1);
        
        Button.AddHandler(PointerPressedEvent, ButtonPointerPressed, RoutingStrategies.Tunnel);
        
        // Console.WriteLine(Button.);
    }

    private void ButtonPointerPressed(object? sender, PointerPressedEventArgs args)
    {
        if (args.GetCurrentPoint(this).Properties.IsRightButtonPressed)
        {
            // i can't just disable the button because it would break the pointer capture
            _buttonDisabled = !_buttonDisabled;
            if (_buttonDisabled)
            {
                Button.Classes.Add("Disabled");
                Button.Classes.Remove("Enabled");
                Button.IsChecked = true;
            }
            else
            {
                Button.Classes.Add("Enabled");
                Button.Classes.Remove("Disabled");
                Button.IsChecked = false;
            }
        }
    }
    
    public void ButtonClick(object sender, RoutedEventArgs args)
    {
        if (_buttonDisabled)
        {
            Button.IsChecked = true;
        }
    }
}