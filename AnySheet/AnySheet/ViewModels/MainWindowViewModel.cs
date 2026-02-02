using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using AnySheet.Views;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AnySheet.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty]
    private CharacterSheet? _loadedSheet;

    [ObservableProperty]
    private bool _sheetMenusEnabled;
    
    public void CreateNewSheet()
    {
        Console.WriteLine("Create new sheet");
        LoadedSheet = new CharacterSheet();
        SheetMenusEnabled = true;
    }

    public void ChangeUiMode(object? parameter)
    {
        // menus are disabled until a sheet is loaded, so this *should* be unreachable
        if (LoadedSheet == null)
        {
            throw new InvalidOperationException("How did you even get here?");
        }
        
        if (parameter != null)
        {
            Console.WriteLine($"Changing UI mode to {parameter}");
            switch ((string)parameter)
            {
                case "ModuleEdit":
                    LoadedSheet.ChangeSheetMode(CharacterSheet.SheetMode.ModuleEdit);
                    break;
                case "Gameplay":
                    LoadedSheet.ChangeSheetMode(CharacterSheet.SheetMode.Gameplay);
                    break;
                // currently unused
                case "TriggerEdit":
                    break;
            }
        }
    }

    [RelayCommand]
    private async Task AddModuleFile(CancellationToken token)
    {
        // menus are disabled until a sheet is loaded, so this *should* be unreachable
        if (LoadedSheet == null)
        {
            throw new InvalidOperationException("How did you even get here?");
        }
        
        var files = await App.TopLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Choose a Lua module file",
            AllowMultiple = false,
            SuggestedFileType = new FilePickerFileType("lua")
        });

        if (files.Count > 0)
        {
            LoadedSheet.AddModuleFromScript(files[0].Path.AbsolutePath, 0, 0);
        }
    }

    [RelayCommand]
    private async Task SaveSheet(CancellationToken token)
    {
        // menus are disabled until a sheet is loaded, so this *should* be unreachable
        if (LoadedSheet == null)
        {
            throw new InvalidOperationException("How did you even get here?");
        }

        if (App.TopLevel == null)
        {
            Console.WriteLine("No top level window!");
            return;
        }
        var startFolder = await App.TopLevel.StorageProvider.TryGetFolderFromPathAsync(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
        if (startFolder == null)
        {
            Console.WriteLine("No documents folder!");
            return;
        }
        var file = await App.TopLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Save sheet as...",
            // .acs files are for "Microsoft Character Agents", so apparently i'm stealing this from Clippy
            DefaultExtension = "acs",
            SuggestedFileName = "CharacterSheet",
            SuggestedStartLocation = startFolder
        });
        if (file != null)
        {
            await using var writer = new StreamWriter(await file.OpenWriteAsync());
            var saveData = LoadedSheet.GetSaveData();
            Console.WriteLine(JsonSerializer.Serialize(saveData));
            await writer.WriteAsync(JsonSerializer.Serialize(saveData));
        }
    }

    [RelayCommand]
    private async Task LoadSheet(CancellationToken token)
    {
        if (App.TopLevel == null)
        {
            Console.WriteLine("No top level window!");
            return;
        }
        var startFolder = await App.TopLevel.StorageProvider.TryGetFolderFromPathAsync(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
        if (startFolder == null)
        {
            Console.WriteLine("No documents folder!");
            return;
        }
        var files = await App.TopLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Save sheet as...",
            // .acs files are for "Microsoft Character Agents", so apparently i'm stealing this from Clippy
            SuggestedFileType = new FilePickerFileType("acs"),
            SuggestedFileName = "CharacterSheet",
            SuggestedStartLocation = startFolder
        });

        if (files.Count > 0)
        {
            using var reader = new StreamReader(await files[0].OpenReadAsync());
            var saveString = await reader.ReadToEndAsync(cancellationToken: token);
            // why is there no JsonArray.TryParse?
            try
            {
                var saveData = JsonSerializer.Deserialize<JsonArray>(saveString);
                if (saveData == null)
                {
                    throw new JsonException("Invalid save data.");
                }
                LoadedSheet = new CharacterSheet(saveData);
                SheetMenusEnabled = true;
            }
            catch (JsonException e)
            {
                Console.WriteLine($"Error parsing JSON: {e.Message}");
            }
        }
    }
}