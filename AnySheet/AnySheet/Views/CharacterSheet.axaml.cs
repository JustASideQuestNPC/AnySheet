using AnySheet.Behaviors;
using AnySheet.ViewModels;
using Avalonia.Controls;
using Avalonia.Controls.PanAndZoom;
using Avalonia.Platform.Storage;
using Avalonia.Xaml.Interactivity;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;

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
    public ObservableCollection<SheetModule.SheetModule> Modules { get; } = [];
    private bool _moduleAddedOrRemoved = false;

    public bool CanvasDragEnabled => Mode == SheetMode.Gameplay;
    public List<string> SaveDataLoadErrorMessages { get; } = [];

    public bool HasBeenModified
    {
        get
        {
            if (_moduleAddedOrRemoved)
            {
                return true;
            }
            
            foreach (var module in Modules)
            {
                if (module.HasBeenModified)
                {
                    return true;
                }
            }

            return false;
        }
    }
    
    private readonly MainWindowViewModel _parent;

    public CharacterSheet(MainWindowViewModel parent)
    {
        InitializeComponent();
        _parent = parent;
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
                    Modules.Add(module);
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
        
        var relativeToWorkingDirectory = Utils.PathContainsWorkingDirectory(path);
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
        
        Modules.Add(module);
        ModuleGrid.Children.Add(module);
        module.SetModuleMode(Mode);
        _moduleAddedOrRemoved = true;
        return (true, []);
    }

    public void PositionOnGrid(SheetModule.SheetModule module)
    {
        var zoomedGridSize = module.GridSnap * ZoomBorder.ZoomX;
        
        // absolute position means the module was loaded from a file and doesn't need to be repositioned because the
        // "camera" will always be at (0, 0) when loading from a file
        if (!module.AbsolutePosition)
        {
            module.GridX = (int)(Math.Floor(-ZoomBorder.OffsetX / zoomedGridSize) + 1);
            module.GridY = (int)(Math.Floor(-ZoomBorder.OffsetY / zoomedGridSize) + 1);
            module.StartX = module.GridX;
            module.StartY = module.GridY;
        }
            
        Canvas.SetLeft(module, module.GridX * zoomedGridSize);
        Canvas.SetTop(module, module.GridY * zoomedGridSize);
    }
    
    public async Task TryAddModuleFromFile(string path)
    {
        var (success, errorMessages) = await AddModuleFromScript(path, 0, 0);
        if (!success)
        {
            await _parent.LogModuleLoadError(errorMessages);
        }
    }

    public void RemoveModule(SheetModule.SheetModule module)
    {
        Modules.Remove(module);
        ModuleGrid.Children.Remove(module);
        _moduleAddedOrRemoved = true;
    }

    public void ResetModulePositions()
    {
        foreach (var module in Modules)
        {
            Canvas.SetLeft(module, 0);
            Canvas.SetTop(module, 0);
            module.GridX = 0;
            module.GridY = 0;
        }
    }

    public void ChangeSheetMode(SheetMode mode)
    {
        Mode = mode;
        foreach (var module in Modules)
        {
            module.SetModuleMode(mode);
        }
        ZoomBorder.EnableZoom = (mode == SheetMode.Gameplay);
        ZoomBorder.PanButton = (Mode == SheetMode.Gameplay ? ButtonName.Left : ButtonName.Right);
    }

    public JsonArray GetSaveData()
    {
        var saveData = new JsonArray();
        foreach (var module in Modules)
        {
            saveData.Add(module.GetSaveData());
        }
        return saveData;
    }
}