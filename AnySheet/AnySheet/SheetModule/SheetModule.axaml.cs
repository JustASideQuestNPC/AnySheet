using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using AnySheet.SheetModule.Primitives;
using AnySheet.Views;
using Avalonia.Controls;
using Avalonia.Interactivity;
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

    // future-proofing for when i get around to adding the trigger system
    private List<ModulePrimitiveLuaBase> _items = [];
    
    public int GridX
    {
        get => _gridX;
        private set
        {
            _gridX = value;
            Grid.SetColumn(this, value);
        }
    }
    public int GridY
    {
        get => _gridY;
        private set
        {
            _gridY = value;
            Grid.SetRow(this, value);
        }
    }
    public int GridWidth
    {
        get => _gridWidth;
        private set
        {
            _gridWidth = value;
            Grid.SetColumnSpan(this, value);
        }
    }
    public int GridHeight
    {
        get => _gridHeight;
        private set
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

        _lua = new LuaSandbox
        {
            Environment =
            {
                ["SheetModule"] = new LuaSheetModule(),
                ["StaticText"]  = new StaticTextLua(),
                ["TextBox"]     = new TextBoxLua()
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
            parent.UpdateGrid(this);
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
            Border.BorderBrush = Avalonia.Media.Brushes.Transparent;
        }

        foreach (var (_, e) in module.Elements)
        {
            // afaik the only way to read the luaValue to the correct primitive class is to brute-force it by trying
            // with every single class and catching exceptions until something works (there's definitely a better way
            // and i'm just too lazy to find it).
            ModulePrimitiveLuaBase? primitive = null;
            foreach (var reader in App.PrimitiveReaders)
            {
                try
                {
                    primitive = reader.Invoke(null, [e]) as ModulePrimitiveLuaBase;
                }
                // i'm invoking the method instead of calling it directly, so instead of just checking for the right
                // exception type i have to do whatever the fuck this is
                catch (TargetInvocationException ex)
                {
                    if (ex.InnerException is not InvalidOperationException)
                    {
                        throw;
                    }
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
            
            _items.Add(primitive);
            PrimitiveGrid.Children.Add(primitive.CreateUiControl());
        }

        return true;
    }

    public void Remove(object? sender, RoutedEventArgs? routedEventArgs)
    {
        _parent.RemoveModule(this);
    }
}