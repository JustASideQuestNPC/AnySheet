using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Input;
using AnySheet.SheetModule.Primitives;
using AnySheet.Views;
using Avalonia.Controls;
using Avalonia.Interactivity;
using CommunityToolkit.Mvvm.Input;
using LuaLib;

namespace AnySheet.SheetModule;

public partial class SheetModule : UserControl
{
    public const int GridSize = 25;
    
    private readonly LuaSandbox _lua;
    private readonly CharacterSheet _parent;
    private int _gridX;
    private int _gridY;
    private int _gridWidth;
    private int _gridHeight;

    // for saving and the trigger system when i get to that
    private List<ModulePrimitiveLuaBase> _items = [];
    
    public int GridX
    {
        get => _gridX;
        private set
        {
            _gridX = value;
            Canvas.SetLeft(this, value * GridSize);
        }
    }
    public int GridY
    {
        get => _gridY;
        private set
        {
            _gridY = value;
            Canvas.SetTop(this, value * GridSize);
        }
    }

    private int GridWidth
    {
        get => _gridWidth;
        set
        {
            _gridWidth = value;
            Grid.SetColumnSpan(this, value);
        }
    }

    private int GridHeight
    {
        get => _gridHeight;
        set
        {
            _gridHeight = value;
            Grid.SetRowSpan(this, value);
        }
    }

    public SheetModule(CharacterSheet parent, int gridX, int gridY, string scriptPath)
    {
        _parent = parent;
        GridX = gridX;
        GridY = gridY;
        
        InitializeComponent();
        // hide the module until it finishes loading (without this it'll temporarily cover the entire window)
        Container.IsVisible = false;

        _lua = new LuaSandbox
        {
            Environment =
            {
                ["SheetModule"]  = new LuaSheetModule(),
                ["StaticText"]   = new StaticTextLua(),
                ["TextBox"]      = new TextBoxLua(),
                ["TripleToggle"] = new TripleToggleLua(),
                ["NumberBox"]    = new NumberBoxLua()
            }
        };

        Loaded += async (_, _) =>
        {
            var buildScript = _runBuildScript(scriptPath);
            await buildScript;
            // immediately remove the module if the build script fails
            if (!buildScript.Result)
            {
                parent.RemoveModule(this);
                return;
            }
            Console.WriteLine($"Loaded module '{scriptPath}' with {PrimitiveGrid.Children.Count} elements. Module " +
                              $"size: {GridWidth}x{GridHeight}.");
            Container.IsVisible = true;
        };
    }

    private async Task<bool> _runBuildScript(string path)
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
            Container.BorderBrush = Avalonia.Media.Brushes.Transparent;
        }

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
            
            Width = GridWidth * (GridSize + PrimitiveGrid.ColumnSpacing) + Container.BorderThickness.Left + Container.BorderThickness.Right;
            Height = GridHeight * (GridSize + PrimitiveGrid.RowSpacing) + Container.BorderThickness.Top + Container.BorderThickness.Bottom;
            
            _items.Add(primitive);
            PrimitiveGrid.Children.Add(primitive.CreateUiControl());
        }

        return true;
    }

    public void Remove(object? sender, RoutedEventArgs? routedEventArgs)
    {
        _parent.RemoveModule(this);
    }

    public void SetModuleMode(CharacterSheet.SheetMode mode)
    {
        switch (mode)
        {
            case CharacterSheet.SheetMode.Gameplay:
                foreach (var item in _items) item.EnableUiControl();
                break;
            case CharacterSheet.SheetMode.ModuleEdit:
                foreach (var item in _items) item.DisableUiControl();
                break;
            case CharacterSheet.SheetMode.TriggerEdit:
                // currently unused
                break;
        }
    }
}