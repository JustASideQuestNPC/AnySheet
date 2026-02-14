using System;
using System.Collections.Generic;
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

namespace AnySheet.SheetModule.Primitives;

[LuaObject]
public partial class TripleToggleLua : ModulePrimitiveLuaBase
{
    private static readonly Dictionary<string, LuaValueType> ConstructorArgs = new()
    {
        ["x"] = LuaValueType.Number,
        ["y"] = LuaValueType.Number,
        ["[onToggle]"] = LuaValueType.Function,
        ["[onStateChange]"] = LuaValueType.Function,
    };

    private TripleTogglePrimitive _uiControl = null!;
    private LuaFunction? _onToggle = null;
    private LuaFunction? _onStateChange = null;

    private int _buttonState = 1;

    [LuaMember("state")]
    private int State
    {
        get => _uiControl.ButtonDisabled ? 0 : _uiControl.Button.IsChecked == true ? 2 : 1;
        set => _uiControl.CurrentState = value;
    }
    
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

        var onToggle = args.ContainsKey("onToggle") ? args["onToggle"].Read<LuaFunction>() : null;
        var onStateChange = args.ContainsKey("onStateChange") ? args["onStateChange"].Read<LuaFunction>() : null;
        return new TripleToggleLua
        {
            GridX = (int)x,
            GridY = (int)y,
            GridWidth = 1,
            GridHeight = 1,
            _onToggle = onToggle,
            _onStateChange = onStateChange
        };
    }

    public static bool TryReadLua(LuaValue value, out TripleToggleLua module)
    {
        return value.TryRead(out module);
    }

    public override UserControl CreateUiControl()
    {
        _uiControl = new TripleTogglePrimitive(this, GridX, GridY, _onToggle, _onStateChange);
        _uiControl.CurrentState = _buttonState;
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
    
    public override bool HasBeenModified => _uiControl.CurrentState != _buttonState;
    public override JsonObject GetSaveObject()
    {
        var state = _uiControl.ButtonDisabled ? 0 : _uiControl.Button.IsChecked == true ? 2 : 1;
        Console.WriteLine($"Saving state {state}");
        return new JsonObject {["state"] = state};
    }

    public override void LoadSaveObject(JsonObject? obj)
    {
        if (obj?["state"] != null && obj["state"]!.AsValue().TryGetValue<int>(out var state))
        {
            _buttonState = state;
            return;
        }

        throw new JsonException("Invalid save data for TripleTogglePrimitive.");
    }
}

public partial class TripleTogglePrimitive : UserControl
{
    public bool ButtonDisabled = false;

    private int _currentState = 0;
    public int CurrentState
    {
        get => _currentState;
        set
        {
            _currentState = value;
            if (value == 0)
            {
                Button.Classes.Add("Disabled");
                Button.Classes.Remove("Enabled");
                Button.IsChecked = true;
            }
            else
            {
                Button.Classes.Add("Enabled");
                Button.Classes.Remove("Disabled");
                Button.IsChecked = (value == 2);
            }
        }
    }
    
    private TripleToggleLua _parent;
    private LuaFunction? _onToggle;
    private LuaFunction? _onStateChange;
    
    public TripleTogglePrimitive(TripleToggleLua parent, int x, int y, LuaFunction? onToggle,
                                 LuaFunction? onStateChange)
    {
        InitializeComponent();
        
        Grid.SetColumn(this, x);
        Grid.SetRow(this, y);
        Grid.SetColumnSpan(this, 1);
        Grid.SetRowSpan(this, 1);
        
        _parent = parent;
        _onToggle = onToggle;
        _onStateChange = onStateChange;
        
        Button.AddHandler(PointerPressedEvent, ButtonPointerPressed, RoutingStrategies.Tunnel);
    }

    private void ButtonPointerPressed(object? sender, PointerPressedEventArgs args)
    {
        if (args.GetCurrentPoint(this).Properties.IsRightButtonPressed)
        {
            // i can't just disable the button because it would break the pointer capture
            ButtonDisabled = !ButtonDisabled;
            if (ButtonDisabled)
            {
                Button.Classes.Add("Disabled");
                Button.Classes.Remove("Enabled");
                Button.IsChecked = true;

                if (_onStateChange != null)
                {
                    _parent.Lua.DoFunctionAsync(_onStateChange, [0]);
                }
            }
            else
            {
                Button.Classes.Add("Enabled");
                Button.Classes.Remove("Disabled");
                Button.IsChecked = false;
                
                if (_onStateChange != null)
                {
                    _parent.Lua.DoFunctionAsync(_onStateChange, [1]);
                }
            }
        }
    }
    
    public void ButtonClick(object sender, RoutedEventArgs args)
    {
        // hack because i can't disable the button
        if (ButtonDisabled)
        {
            Button.IsChecked = true;
            return;
        }

        if (_onStateChange != null)
        {
            _parent.Lua.DoFunctionAsync(_onStateChange, [(Button.IsChecked ?? false) ? 2 : 1]);
        }
        
        if (_onToggle != null)
        {
            _parent.Lua.DoFunctionAsync(_onToggle, [Button.IsChecked ?? false]);
        }
    }
}