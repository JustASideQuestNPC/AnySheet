using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using AnySheet.Views;
using Avalonia.Controls;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Avalonia.Styling;
using AvaloniaDialogs.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DialogHostAvalonia;

namespace AnySheet.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty]
    private CharacterSheet? _loadedSheet;

    [ObservableProperty]
    private bool _sheetMenusEnabled;
    
    private string _currentFilePath = "";

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
    public async Task CreateNewSheet()
    {
        if (_loadedSheet != null && _loadedSheet.HasBeenModified)
        {
            var dialog = new ThreefoldDialog
            {
                Message = "Do you want to save the current sheet? Unsaved changes will be lost.",
                PositiveText = "Save",
                NegativeText = "Discard",
                NeutralText = "Cancel"
            };
            var result = await dialog.ShowAsync();

            if (result == ThreefoldDialog.ButtonType.Neutral)
            {
                return;
            }

            if (result == ThreefoldDialog.ButtonType.Positive)
            {
                await SaveSheetToCurrentPath(CancellationToken.None);
            }
        }
        
        Console.WriteLine("Create new sheet");
        LoadedSheet = new CharacterSheet();
        SheetMenusEnabled = true;
        _currentFilePath = "";
    }

    [RelayCommand]
    private async Task AddModuleFile(CancellationToken token)
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
            Environment.CurrentDirectory + "/Modules");
        var files = await App.TopLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Choose a Lua module file",
            AllowMultiple = false,
            SuggestedFileType = new FilePickerFileType("lua"),
            SuggestedStartLocation = startFolder
        });

        if (files.Count > 0)
        {
            LoadedSheet.AddModuleFromScript(files[0].Path.AbsolutePath, 0, 0);
        }
    }

    [RelayCommand]
    private async Task SaveSheetToCurrentPath(CancellationToken token)
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
        
        Console.WriteLine($"Saving sheet to {_currentFilePath}");
        if (_currentFilePath == "")
        {
            await SaveSheetToNewPath(token);
            return;
        }
        
        var file = await App.TopLevel.StorageProvider.TryGetFileFromPathAsync(_currentFilePath);
        if (file != null)
        {
            await SaveSheet(file);
        }
    }

    [RelayCommand]
    private async Task SaveSheetToNewPath(CancellationToken token)
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
            await SaveSheet(file);
            _currentFilePath = file.Path.AbsolutePath;
        }
    }

    [RelayCommand]
    private async Task OpenSheetFromFile(CancellationToken token)
    {
        if (_loadedSheet != null && _loadedSheet.HasBeenModified)
        {
            var dialog = new ThreefoldDialog
            {
                Message = "Do you want to save the current sheet? Unsaved changes will be lost.",
                PositiveText = "Save",
                NegativeText = "Discard",
                NeutralText = "Cancel"
            };
            var result = await dialog.ShowAsync();

            if (result == ThreefoldDialog.ButtonType.Neutral)
            {
                return;
            }

            if (result == ThreefoldDialog.ButtonType.Positive)
            {
                await SaveSheetToCurrentPath(CancellationToken.None);
            }
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
        var files = await App.TopLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Choose a saved sheet file",
            // .acs files are for "Microsoft Character Agents", so apparently i'm stealing this from Clippy
            SuggestedFileType = new FilePickerFileType("acs"),
            SuggestedStartLocation = startFolder
        });

        var sheetLoaded = false;
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
                sheetLoaded = true;
            }
            catch (JsonException e)
            {
                Console.WriteLine($"Error parsing JSON: {e.Message}");
            }
        }

        if (sheetLoaded)
        {
            _currentFilePath = files[0].Path.AbsolutePath;
        }
    }

    private bool _forceClose = false;
    public void OnWindowClosed(object? sender, WindowClosingEventArgs e)
    {
        if (_loadedSheet != null && _loadedSheet.HasBeenModified && !_forceClose)
        {
            e.Cancel = true;
            var dialog = new ThreefoldDialog
            {
                Message = "Do you want to save the current sheet? Unsaved changes will be lost.",
                PositiveText = "Save",
                NegativeText = "Discard",
                NeutralText = "Cancel"
            };
            dialog.ShowAsync().ContinueWith(t =>
            {
                if (t.Result == ThreefoldDialog.ButtonType.Positive)
                {
                    SaveSheetToCurrentPath(CancellationToken.None);
                }

                if (t.Result != ThreefoldDialog.ButtonType.Neutral)
                {
                    _forceClose = true;
                    // App.Current
                }
            });
        }
    }

    private async Task SaveSheet(IStorageFile file)
    {
        await using var writer = new StreamWriter(await file.OpenWriteAsync());
        var saveData = LoadedSheet!.GetSaveData();
        await writer.WriteAsync(JsonSerializer.Serialize(saveData));
    }
}