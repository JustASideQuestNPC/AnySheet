using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AnySheet.Views;

public partial class CharacterSheet : UserControl
{
    public enum SheetMode
    {
        Gameplay,
        ModuleEdit,
        TriggerEdit // currently unused
    }

    public SheetMode Mode = SheetMode.Gameplay;
    private readonly List<SheetModule.SheetModule> _modules = [];
    private bool _moduleAddedOrRemoved = false;

    public bool CanvasDragEnabled => Mode == SheetMode.Gameplay;
    public List<string> SaveDataLoadErrorMessages { get; private set; } = [];

    public bool HasBeenModified
    {
        get
        {
            if (_moduleAddedOrRemoved)
            {
                return true;
            }
            
            foreach (var module in _modules)
            {
                if (module.HasBeenModified)
                {
                    return true;
                }
            }

            return false;
        }
    }

    public CharacterSheet()
    {
        InitializeComponent();
    }

    public async Task<bool> LoadSaveFile(IStorageFile file, CancellationToken token)
    {
        using var reader = new StreamReader(await file.OpenReadAsync());
        var saveString = await reader.ReadToEndAsync(cancellationToken: token);
        // why is there no JsonArray.TryParse?
        try
        {
            var saveData = JsonSerializer.Deserialize<JsonArray>(saveString);
            if (saveData == null)
            {
                SaveDataLoadErrorMessages.Add("Save data is null.");
                return false;
            }

            var currentModule = 0;
            foreach (var moduleData in saveData)
            {
                if (moduleData is not JsonArray)
                {
                    // "catch" the error but keep going so i can log every error
                    var dataType = moduleData != null ? moduleData.GetType().Name : "null";
                    SaveDataLoadErrorMessages.Add($"Invalid save data for module {currentModule}: Expected array, " +
                                                  $"got {dataType}.");
                    continue;
                }
                
                
                var module = new SheetModule.SheetModule(this, moduleData.AsArray());
                await module.RunBuildScript();
                if (module.SaveDataLoadError)
                {
                    SaveDataLoadErrorMessages.Add($"Error(s) while loading module {currentModule}:");
                    SaveDataLoadErrorMessages.AddRange(module.SaveDataLoadErrorMessages.Select(msg => $"- {msg}"));
                }
                else
                {
                    _modules.Add(module);
                    ModuleGrid.Children.Add(module);
                    module.SetModuleMode(Mode);
                }
                
                ++currentModule;
            }
        }
        catch (JsonException e)
        {
            SaveDataLoadErrorMessages.Add($"Error while parsing JSON: {e.Message}");
            return false;
        }
        
        return SaveDataLoadErrorMessages.Count == 0;
    }

    public async Task<(bool, List<string>)> AddModuleFromScript(string path, int gridX, int gridY)
    {
        var loadErrors = new List<string>();
        
        var relativeToWorkingDirectory = App.PathContainsWorkingDirectory(path);
        if (relativeToWorkingDirectory)
        {
            path = path[(Environment.CurrentDirectory.Length + 1)..];
        }
        
        var module = new SheetModule.SheetModule(this, gridX, gridY, path, relativeToWorkingDirectory);
        await module.RunBuildScript();

        if (module.SaveDataLoadError)
        {
            loadErrors.Add($"Error(s) while loading module:");
            loadErrors.AddRange(module.SaveDataLoadErrorMessages.Select(msg => $"- {msg}"));
            return (false, loadErrors);
        }
        
        _modules.Add(module);
        ModuleGrid.Children.Add(module);
        module.SetModuleMode(Mode);
        _moduleAddedOrRemoved = true;
        return (true, []);
    }
    
    public void RemoveModule(SheetModule.SheetModule module)
    {
        _modules.Remove(module);
        ModuleGrid.Children.Remove(module);
        _moduleAddedOrRemoved = true;
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