using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Windows.Input;
using AnySheet.Behaviors;
using AnySheet.SheetModule.Primitives;
using AnySheet.Views;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using CommunityToolkit.Mvvm.Input;
using LuaLib;

namespace AnySheet.SheetModule;

public partial class SheetModule : UserControl
{
    public const int GridSize = 20;
    public const int GridSpacing = 4;
    private const int BorderWidth = 4;
    
    private LuaSandbox _lua = null!;
    private CharacterSheet _parent = null!;
    private string _scriptPath = ""; // for saving
    private bool _relativeToWorkingDirectory = false;
    private int _gridX;
    private int _gridY;
    private int _gridWidth;
    private int _gridHeight;
    private bool _moduleWasDragged = false;
    private JsonArray _saveData;

    // for saving and the trigger system when i get to that
    private readonly List<ModulePrimitiveLuaBase> _items = [];
    public static readonly StyledProperty<bool> DragEnabledProperty =
        AvaloniaProperty.Register<SheetModule, bool>("DragEnabled");
    public static readonly StyledProperty<int> GridSnapProperty =
        AvaloniaProperty.Register<SheetModule, int>("GridSnap");

    [RelayCommand]
    private void DragCompleted(ModuleDragBehavior.DragCompletedCommandParameters args)
    {
        if (_gridX != (int)args.GridPosition.X || _gridY != (int)args.GridPosition.Y)
        {
            _moduleWasDragged = true;
        }
        
        _gridX = (int)args.GridPosition.X;
        _gridY = (int)args.GridPosition.Y;
    }

    public int GridX
    {
        get => _gridX;
        private set
        {
            _gridX = value;
            Canvas.SetLeft(this, value * (GridSize + GridSpacing));
        }
    }
    public int GridY
    {
        get => _gridY;
        private set
        {
            _gridY = value;
            Canvas.SetTop(this, value * (GridSize + GridSpacing));
        }
    }

    private int GridWidth
    {
        get => _gridWidth;
        set
        {
            _gridWidth = value;
            //Grid.SetColumnSpan(this, value); 
        }
    }

    private int GridHeight
    {
        get => _gridHeight;
        set
        {
            _gridHeight = value;
            //Grid.SetRowSpan(this, value);
        }
    }

    public bool DragEnabled
    {
        get => GetValue(DragEnabledProperty);
        set => SetValue(DragEnabledProperty, value);
    }

    public int GridSnap
    {
        get => GetValue(GridSnapProperty);
        set => SetValue(GridSnapProperty, value);
    }
    
    public bool HasBeenModified
    {
        get
        {
            if (_moduleWasDragged)
            {
                return true;
            }
            
            foreach (var item in _items)
            {
                if (item.HasBeenModified)
                {
                    return true;
                }
            }
            return false;
        }
    }

    public bool SaveDataLoadError => SaveDataLoadErrorMessages.Count > 0;
    public List<string> SaveDataLoadErrorMessages { get; } = [];

    private void LogErrorMessage(string message)
    {
        SaveDataLoadErrorMessages.Add(message);
        Console.WriteLine(message);
    }

    public SheetModule(CharacterSheet parent, JsonArray saveData)
    {
        if (saveData.Count != 4)
        {
            LogErrorMessage($"Save data must contain 4 elements, but contains {saveData.Count}.");
            return;
        }

        if (
            saveData[0] == null || !saveData[0]!.AsValue().TryGetValue<string>(out var path) ||
            saveData[1] == null || !saveData[1]!.AsValue().TryGetValue<int>(out var x) ||
            saveData[2] == null || !saveData[2]!.AsValue().TryGetValue<int>(out var y) ||
            saveData[3] == null ||  saveData[3]!.AsArray() is not { Count: > 0 } itemData
        )
        {
            var dataType1 = saveData[0]?.GetType().Name ?? "null";
            var dataType2 = saveData[1]?.GetType().Name ?? "null";
            var dataType3 = saveData[2]?.GetType().Name ?? "null";
            var dataType4 = saveData[3]?.GetType().Name ?? "null";
            LogErrorMessage("Save data must be an array of type [string, int, int, JsonArray], but " +
                                          $"contains [{dataType1}, {dataType2}, {dataType3}, {dataType4}].");
        }
        else
        {
            _parent = parent;
            GridX = x;
            GridY = y;
            _saveData = itemData;
            _setup(path);
        }
    }

    public SheetModule(CharacterSheet parent, int gridX, int gridY, string scriptPath, bool relativeToWorkingDirectory)
    {
        _parent = parent;
        GridX = gridX;
        GridY = gridY;
        _scriptPath = (relativeToWorkingDirectory ? "~" : "") + scriptPath;
        _saveData = [];
        
        _setup(scriptPath);
    }
    
    private void _setup(string scriptPath)
    {
        GridSnap = GridSize + GridSpacing;

        if (scriptPath.StartsWith('~'))
        {
            _scriptPath = scriptPath[1..];
            _relativeToWorkingDirectory = true;
        }
        else
        {
            _scriptPath = scriptPath;
        }

        InitializeComponent();
        DataContext = this;
        
        // hide the module until it finishes loading (without this it'll temporarily cover the entire window)
        Container.IsVisible = false;
    }

    public async Task RunBuildScript()
    {
        _lua = new LuaSandbox
        {
            Environment =
            {
                ["SheetModule"]      = new LuaSheetModule(),
                ["StaticText"]       = new StaticTextLua(),
                ["TextBox"]          = new TextBoxLua(),
                ["MultiLineTextBox"] = new MultilineTextBoxLua(),
                ["TripleToggle"]     = new TripleToggleLua(),
                ["ToggleButton"]     = new ToggleButtonLua(),
                ["NumberBox"]        = new NumberBoxLua(),
                ["Button"]           = new ButtonLua(),
                ["Divider"]          = new DividerLua(),
                ["List"]             = new ListPrimitiveLua()
            }
        };
        
        var buildScriptSuccessful = await _runBuildScript(
            (_relativeToWorkingDirectory ? Environment.CurrentDirectory + "\\" : "") + _scriptPath, _saveData);
        // immediately remove the module if the build script fails
        if (!buildScriptSuccessful)
        {
            _parent.RemoveModule(this);
            return;
        }

        Console.WriteLine($"Loaded module '{_scriptPath}' with {PrimitiveGrid.Children.Count} elements. Module " +
                          $"size: {Width}x{Height} ({GridWidth}x{GridHeight} on grid), position: {GridX},{GridY}.");
        Container.AddHandler(PointerPressedEvent, ContainerPointerPressed, RoutingStrategies.Tunnel);
        Container.IsVisible = true;
    }

    private async Task<bool> _runBuildScript(string path, JsonArray itemData)
    {
        if (!File.Exists(path))
        {
            LogErrorMessage($"Module script '{path}' does not exist.");
            return false;
        }
        
        var (returnValue, success, errorMessage) = await _lua.DoFileAsync(path);

        if (!success)
        {
            LogErrorMessage($"Error in module script: {errorMessage}");
            return false;
        }
        
        // do way too much error checking
        if (returnValue.Length != 1)
        {
            LogErrorMessage("Module script must return a SheetModule instance.");
            return false;
        }

        LuaSheetModule module;
        try
        {
            module = returnValue[0].Read<LuaSheetModule>();
        }
        catch (InvalidOperationException)
        {
            LogErrorMessage("Module script must return a SheetModule instance.");
            return false;
        }

        if (module.NoBorder)
        {
            // Container.BorderBrush = Avalonia.Media.Brushes.Transparent;
        }

        var i = 0;
        var primitiveLoadFailure = false;
        foreach (var (_, e) in module.Elements)
        {
            // afaik the only way to read the luaValue to the correct primitive class is to brute-force it by trying
            // with every single class until it works
            ModulePrimitiveLuaBase? primitive = null;
            foreach (var reader in App.PrimitiveReaders)
            {
                var args = new object[] { e, primitive! };
                if (reader.Invoke(null, args) is true)
                {
                    primitive = args[1] as ModulePrimitiveLuaBase;
                    break;
                }
            }

            if (primitive == null)
            {
                LogErrorMessage($"Element '{e}' is not a valid primitive.");
                primitiveLoadFailure = true;
                continue;
            }

            while (PrimitiveGrid.ColumnDefinitions.Count < primitive.GridX + primitive.GridWidth)
            {
                PrimitiveGrid.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(GridSize)));
                ++GridWidth;
            }
            while (PrimitiveGrid.RowDefinitions.Count < primitive.GridY + primitive.GridHeight)
            {
                PrimitiveGrid.RowDefinitions.Add(new RowDefinition(new GridLength(GridSize)));
                ++GridHeight;
            }

            Width = GridWidth * (GridSize + PrimitiveGrid.ColumnSpacing) + BorderWidth;
            Height = GridHeight * (GridSize + PrimitiveGrid.RowSpacing) + BorderWidth;
            
            if (itemData.Count > i)
            {
                primitive.LoadSaveObject(itemData[i] != null ? itemData[i]!.AsObject() : null);
                ++i;
            }
            
            primitive.Lua = _lua;
            _items.Add(primitive);
            
            var uiControl = primitive.CreateUiControl();
            PrimitiveGrid.Children.Add(uiControl);
        }

        return !primitiveLoadFailure;
    }

    public void SetModuleMode(CharacterSheet.SheetMode mode)
    {
        switch (mode)
        {
            case CharacterSheet.SheetMode.Gameplay:
                foreach (var item in _items) item.EnableUiControl();
                DragEnabled = false;
                break;
            case CharacterSheet.SheetMode.ModuleEdit:
                foreach (var item in _items) item.DisableUiControl();
                DragEnabled = true;
                break;
            case CharacterSheet.SheetMode.TriggerEdit:
                // currently unused
                break;
        }
    }

    private void ContainerPointerPressed(object? sender, PointerPressedEventArgs args)
    {
        if (args.GetCurrentPoint(this).Properties.IsRightButtonPressed &&
            _parent.Mode == CharacterSheet.SheetMode.ModuleEdit)
        {
            _parent.RemoveModule(this);
        }
    }

    public JsonArray GetSaveData()
    {
        var itemData = new JsonArray();
        foreach (var item in _items)
        {
            itemData.Add(item.GetSaveObject());
        }
        
        return [(_relativeToWorkingDirectory ? "~" : "") + _scriptPath, GridX, GridY, itemData];
    }
}