using System;
using System.Collections.Generic;
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
    public const int GridSpacing = 6;
    private const int BorderWidth = 4;
    
    private LuaSandbox _lua = null!;
    private CharacterSheet _parent = null!;
    private string _scriptPath = ""; // for saving
    private bool _relativeToWorkingDirectory = false;
    private int _gridX;
    private int _gridY;
    private int _gridWidth;
    private int _gridHeight;

    // for saving and the trigger system when i get to that
    private readonly List<ModulePrimitiveLuaBase> _items = [];
    public static readonly StyledProperty<bool> DragEnabledProperty =
        AvaloniaProperty.Register<SheetModule, bool>("DragEnabled");
    public static readonly StyledProperty<int> GridSnapProperty =
        AvaloniaProperty.Register<SheetModule, int>("GridSnap");

    [RelayCommand]
    private void DragCompleted(ModuleDragBehavior.DragCompletedCommandParameters args)
    {
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

    public SheetModule(CharacterSheet parent, JsonArray saveData)
    {
        if (saveData.Count != 4)
        {
            throw new ArgumentException("Cannot load module: save data must contain 4 elements.");
        }

        if (
            saveData[0] != null && saveData[0]!.AsValue().TryGetValue<string>(out var path) &&
            saveData[1] != null && saveData[1]!.AsValue().TryGetValue<int>(out var x) &&
            saveData[2] != null && saveData[2]!.AsValue().TryGetValue<int>(out var y) &&
            saveData[3] != null && saveData[3]!.AsArray() is { Count: > 0 } itemData
        )
        {
            _setup(parent, x, y, path, itemData);
        }
    }

    public SheetModule(CharacterSheet parent, int gridX, int gridY, string scriptPath, bool relativeToWorkingDirectory)
    {
        _setup(parent, gridX, gridY, (relativeToWorkingDirectory ? "~" : "") + scriptPath, []);
    }
    
    private void _setup(CharacterSheet parent, int gridX, int gridY, string scriptPath, JsonArray itemData)
    {
        _parent = parent;
        GridX = gridX;
        GridY = gridY;
        GridSnap = GridSize + GridSpacing;

        if (scriptPath.StartsWith("~"))
        {
            _scriptPath = scriptPath.Substring(1);
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
                ["Divider"]          = new DividerLua()
            }
        };

        Loaded += async (_, _) =>
        {
            var buildScript = _runBuildScript(
                (_relativeToWorkingDirectory ? Environment.CurrentDirectory + "\\" : "") + _scriptPath, itemData);
            await buildScript;
            // immediately remove the module if the build script fails
            if (!buildScript.Result)
            {
                parent.RemoveModule(this);
                return;
            }

            Console.WriteLine($"Loaded module '{scriptPath}' with {PrimitiveGrid.Children.Count} elements. Module " +
                              $"size: {Width}x{Height} ({GridWidth}x{GridHeight} on grid), position: {GridX},{GridY}.");
            Container.AddHandler(PointerPressedEvent, ContainerPointerPressed, RoutingStrategies.Tunnel);
            Container.IsVisible = true;
        };
    }

    private async Task<bool> _runBuildScript(string path, JsonArray itemData)
    {
        var task = await _lua.DoFileAsync(path);
        
        // do way too much error checking
        if (task.Length != 1)
        {
            Console.WriteLine("Module script must return a SheetModule instance.");
            return false;
        }

        LuaSheetModule module;
        try
        {
            module = task[0].Read<LuaSheetModule>();
        }
        catch (InvalidOperationException)
        {
            Console.WriteLine("Module script must return a SheetModule instance.");
            return false;
        }

        if (module.NoBorder)
        {
            // Container.BorderBrush = Avalonia.Media.Brushes.Transparent;
        }

        var i = 0;
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
                Console.WriteLine($"Element '{e}' is not a valid primitive.");
                return false;
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
            
            // Container.Width = Width + BorderWidth;
            // Container.Height = Height + BorderWidth;
            // Container.BorderThickness = new Thickness(BorderWidth);
            
            PrimitiveGrid.Children.Add(primitive.CreateUiControl());
        }

        return true;
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