using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;
using Avalonia.Controls;

namespace AnySheet.Views;

public partial class CharacterSheet : UserControl
{
    public enum SheetMode
    {
        Gameplay,
        ModuleEdit,
        TriggerEdit // currently unused
    }
    
    public const int GridSize = 26;

    public SheetMode Mode = SheetMode.Gameplay;
    private readonly List<SheetModule.SheetModule> _modules = [];
    
    public CharacterSheet()
    {
        InitializeComponent();
    }

    public CharacterSheet(JsonArray saveData) : this()
    {
        foreach (var moduleData in saveData)
        {
            if (moduleData == null)
            {
                throw new JsonException("Invalid save data for SheetModule: null element found.");
            }
            
            var module = new SheetModule.SheetModule(this, moduleData.AsArray());
            _modules.Add(module);
            ModuleGrid.Children.Add(module);
            module.SetModuleMode(Mode);
        }
    }

    public void AddModuleFromScript(string path, int gridX, int gridY)
    {
        var module = new SheetModule.SheetModule(this, gridX, gridY, path);
        _modules.Add(module);
        ModuleGrid.Children.Add(module);
        module.SetModuleMode(Mode);
    }
    
    public void RemoveModule(SheetModule.SheetModule module)
    {
        _modules.Remove(module);
        ModuleGrid.Children.Remove(module);
    }

    public void ChangeSheetMode(SheetMode mode)
    {
        Mode = mode;
        foreach (var module in _modules)
        {
            module.SetModuleMode(mode);
        }
    }

    public JsonArray GetSaveData()
    {
        var saveData = new JsonArray();
        foreach (var module in _modules)
        {
            saveData.Add(module.GetSaveData());
        }
        return saveData;
    }
}