using AnySheet.Behaviors;
using AnySheet.SheetModule.Primitives;
using AnySheet.Views;
using Avalonia;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
using LuaLib;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using AnySheet.ViewModels;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Lua;

namespace AnySheet.SheetModule;

public partial class SheetModule : UserControl
{
    public const int GridSize = 20;
    public const int GridSpacing = 4;
    private const int BorderWidth = 4;
    
    private LuaSandbox _lua = null!;
    private CharacterSheet _parent = null!;
    private bool _relativeToWorkingDirectory = false;
    private bool _moduleWasDragged = false;
    private JsonArray _saveData;

    // for saving and the trigger system when i get to that
    private readonly List<ModulePrimitiveLuaBase> _items = [];
    public static readonly StyledProperty<bool> ModuleEditsEnabledProperty =
        AvaloniaProperty.Register<SheetModule, bool>("ModuleEditsEnabled");
    public static readonly StyledProperty<int> GridSnapProperty =
        AvaloniaProperty.Register<SheetModule, int>("GridSnap");

    public ObservableCollection<ModuleTriggerToggleListEntry> TriggerToggleButtons { get; } = [];

    [RelayCommand]
    private void DragCompleted(ModuleDragBehavior.DragCompletedCommandParameters args)
    {
        if (GridX - StartX != (int)args.GridPosition.X || GridY - StartY != (int)args.GridPosition.Y)
        {
            _moduleWasDragged = true;
        }
        
        GridX = (int)args.GridPosition.X + StartX;
        GridY = (int)args.GridPosition.Y + StartY;
        
        Console.WriteLine($"Module dragged to ({GridX},{GridY}).");
    }

    public int GridX { get; set; }

    public int GridY { get; set; }
    
    public int StartX { get; set; }
    
    public int StartY { get; set; }

    private int GridWidth { get; set; }

    private int GridHeight { get; set; }

    public bool ModuleEditsEnabled
    {
        get => GetValue(ModuleEditsEnabledProperty);
        set => SetValue(ModuleEditsEnabledProperty, value);
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

    public bool AbsolutePosition { get; private init; }
    public string ScriptPath { get; private set; } = "";
    
    private Dictionary<string, LuaFunction> _triggers = new();
    private readonly Dictionary<string, List<string>> _triggerGroups = new();
    public List<string> TriggerGroupNames => _triggerGroups.Keys.ToList();
    private string _editingTriggerGroup = "";
    
    private CharacterSheet.SheetMode _mode;

    // called when loading sheets from a file
    public SheetModule(CharacterSheet parent, JsonArray saveData)
    {
        if (saveData.Count != 4 && saveData.Count != 5)
        {
            LogErrorMessage($"Save data must contain 4 or 5 elements, but contains {saveData.Count}.");
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
            return;
        }

        if (saveData.Count == 4 || saveData[4] == null || saveData[4]!.AsObject() is not { Count: >= 0 } triggerData)
        {
            triggerData = new JsonObject();
            Console.WriteLine("[Warning]: Module has no trigger data; adding placeholder object.");
        }
        
        _parent = parent;
        GridX = x;
        GridY = y;
        _saveData = itemData;
        AbsolutePosition = true;

        _triggerGroups = new Dictionary<string, List<string>>();
        foreach (var (groupName, triggers) in triggerData)
        {
            if (triggers?.AsArray() is { Count: >= 0 } triggerNames)
            {
                _triggerGroups[groupName] = triggerNames.GetValues<string>().ToList();
            }
            else
            {
                LogErrorMessage($"Trigger group '{groupName}' is not an array of strings.");
            }
        }
            
        _setup(path);
    }
    
    // called when adding a new module
    public SheetModule(CharacterSheet parent, int gridX, int gridY, string scriptPath, bool relativeToWorkingDirectory)
    {
        _parent = parent;
        GridX = gridX;
        GridY = gridY;
        AbsolutePosition = false;
        ScriptPath = (relativeToWorkingDirectory ? "~" : "") + scriptPath;
        _saveData = [];
        _triggerGroups = new Dictionary<string, List<string>>();
        
        _setup(scriptPath);
    }
    
    private void _setup(string scriptPath)
    {
        GridSnap = GridSize + GridSpacing;

        if (scriptPath.StartsWith('~'))
        {
            ScriptPath = scriptPath[1..];
            _relativeToWorkingDirectory = true;
        }
        else
        {
            ScriptPath = scriptPath;
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
                ["MultilineTextBox"] = new MultilineTextBoxLua(),
                ["TripleToggle"]     = new TripleToggleLua(),
                ["ToggleButton"]     = new ToggleButtonLua(),
                ["NumberBox"]        = new NumberBoxLua(),
                ["Button"]           = new ButtonLua(),
                ["Divider"]          = new DividerLua(),
                ["List"]             = new ListPrimitiveLua()
            }
        };
        
        var buildScriptSuccessful = await _runBuildScript(
            (_relativeToWorkingDirectory ? Environment.CurrentDirectory + @"\Modules\" : "") + ScriptPath, _saveData);
        
        var triggerVerificationSuccessful = true;
        foreach (var (groupName, triggerNames) in _triggerGroups)
        {
            foreach (var triggerName in triggerNames) {
                if (!_triggers.ContainsKey(triggerName))
                {
                    LogErrorMessage($"Trigger '{triggerName}' does not exist in group '{groupName}'.");
                    triggerVerificationSuccessful = false;
                }
            }
        }
        
        // immediately remove the module if the build script fails
        if (!buildScriptSuccessful || !triggerVerificationSuccessful)
        {
            _parent.RemoveModule(this);
            return;
        }
        
        _parent.PositionOnGrid(this);
        Console.WriteLine($"Loaded module '{ScriptPath}' with {PrimitiveGrid.Children.Count} elements. Module " +
                          $"size: {Width}x{Height} ({GridWidth}x{GridHeight} on grid), position: {GridX},{GridY}.");
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
        
        if (returnValue.Length != 1 || !returnValue[0].TryRead<LuaSheetModule>(out var module))
        {
            LogErrorMessage("Module script must return a SheetModule instance.");
            return false;
        }
        
        _triggers = module.Triggers;
        foreach (var (triggerName, _) in _triggers)
        {
            TriggerToggleButtons.Add(new ModuleTriggerToggleListEntry(this, triggerName, false));
            Console.WriteLine($"Added trigger '{triggerName}' to module.");
        }

        // for calculating grid size
        var left = int.MaxValue;
        var top = int.MaxValue;
        var right = -int.MaxValue;
        var bottom = -int.MaxValue;

        // load all lua primitives
        var i = 0;
        var primitiveLoadFailure = false;
        foreach (var (_, e) in module.Elements)
        {
            // afaik the only way to read the luaValue to the correct primitive class is to brute-force it by trying
            // with every single class until it works
            ModulePrimitiveLuaBase? primitive = null;
            foreach (var reader in Utils.PrimitiveReaders)
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
            
            left = Math.Min(left, primitive.GridX);
            top = Math.Min(top, primitive.GridY);
            right = Math.Max(right, primitive.GridX + primitive.GridWidth);
            bottom = Math.Max(bottom, primitive.GridY + primitive.GridHeight);
            
            if (itemData.Count > i)
            {
                primitive.LoadSaveObject(itemData[i] != null ? itemData[i]!.AsObject() : null);
                ++i;
            }
            
            primitive.Lua = _lua;
            _items.Add(primitive);
        }
        GridWidth = right - left;
        GridHeight = bottom - top;
        
        for (var x = 0; x < GridWidth; ++x)
        {
            PrimitiveGrid.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(GridSize)));
        }
        for (var y = 0; y < GridHeight; ++y)
        {
            PrimitiveGrid.RowDefinitions.Add(new RowDefinition(new GridLength(GridSize)));
        }
        Width = GridWidth * (GridSize + PrimitiveGrid.ColumnSpacing) + BorderWidth;
        Height = GridHeight * (GridSize + PrimitiveGrid.RowSpacing) + BorderWidth;

        Console.WriteLine($"{_items.Count} elements loaded. Grid size: {GridWidth}x{GridHeight}");
        foreach (var item in _items)
        {
            // i can't just modify the GridX and GridY fields because i want the movement to be invisible from inside
            // the lua script
            var uiControl = item.CreateUiControl(-left, -top);
            
            PrimitiveGrid.Children.Add(uiControl);
        }
        
        return !primitiveLoadFailure;
    }

    [RelayCommand]
    private void RemoveModule(object? _)
    {
        _parent.RemoveModule(this);
    }

    public void SetModuleMode(CharacterSheet.SheetMode mode)
    {
        _mode = mode;
        
        switch (mode)
        {
            case CharacterSheet.SheetMode.Gameplay:
                foreach (var item in _items)
                {
                    item.EnableUiControl();
                }
                ModuleEditsEnabled = false;
                ContextMenu.IsEnabled = false;
                TriggerList.IsVisible = false;
                break;
            case CharacterSheet.SheetMode.ModuleEdit:
                foreach (var item in _items)
                {
                    item.DisableUiControl();
                }
                ModuleEditsEnabled = true;
                ContextMenu.IsEnabled = true;
                TriggerList.IsVisible = false;
                break;
            case CharacterSheet.SheetMode.TriggerEdit:
                foreach (var item in _items)
                {
                    item.DisableUiControl();
                }
                ModuleEditsEnabled = false;
                ContextMenu.IsEnabled = false;
                TriggerList.IsVisible = false;
                break;
        }
    }

    public void SetEditingTrigger(string groupName)
    {
        _editingTriggerGroup = groupName;
        if (!_triggerGroups.TryGetValue(groupName, out var group))
        {
            group = [];
            _triggerGroups[groupName] = group;
        }

        foreach (var triggerButton in TriggerToggleButtons)
        {
            triggerButton.ButtonState = group.Contains(triggerButton.Name);
        }
        
        TriggerList.IsVisible = true;
    }

    public void UpdateTriggerGroup(string triggerName, bool state)
    {
        if (state) {
            _triggerGroups[_editingTriggerGroup].Add(triggerName);
        }
        else
        {
            _triggerGroups[_editingTriggerGroup].Remove(triggerName);
        }
    }

    public async Task RunTriggerGroup(string groupName)
    {
        if (_triggerGroups.TryGetValue(groupName, out var group))
        {
            foreach (var name in group)
            {
                await _lua.DoFunctionAsync(_triggers[name]);
            }
        }
    }

    public JsonArray GetSaveData()
    {
        var itemData = new JsonArray();
        foreach (var item in _items)
        {
            itemData.Add(item.GetSaveObject());
        }
        
        var triggerData = new JsonObject();
        foreach (var (triggerName, triggers) in _triggerGroups)
        {
            var group = new JsonArray();
            foreach (var trigger in triggers)
            {
                group.Add(trigger);
            }
            triggerData[triggerName] = group;
        }
        
        Console.WriteLine($"Saved module at {GridX}, {GridY}");
        
        return [(_relativeToWorkingDirectory ? "~" : "") + ScriptPath, GridX, GridY, itemData, triggerData];
    }

    private void LogErrorMessage(string message)
    {
        SaveDataLoadErrorMessages.Add(message);
        Console.WriteLine(message);
    }
}