using AnySheet.Views;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using AvaloniaDialogs.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DialogHostAvalonia;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace AnySheet.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty]
    private CharacterSheet? _loadedSheet;
    
    [ObservableProperty]
    private ObservableCollection<ModuleFolderViewModel> _moduleFolders = [];

    [ObservableProperty]
    private bool _sheetMenusEnabled;
    
    [ObservableProperty]
    private bool _moduleSidebarEnabled;
    
    [ObservableProperty]
    private double _modeIconOpacity = 0.5;

    [ObservableProperty]
    private bool _gameplayModeButtonEnabled;
    
    [ObservableProperty]
    private IBrush _gameplayModeIconColor = Brushes.Black;
    
    [ObservableProperty]
    private bool _moduleEditModeButtonEnabled;
    
    [ObservableProperty]
    private IBrush _moduleEditModeIconColor = Brushes.Black;
    
    private string _currentFilePath = "";

    public void UpdateModuleFileTree(Dictionary<string, List<(string, string)>> moduleFileTree)
    {
        ModuleFolders.Clear();
        foreach (var (folderName, files) in moduleFileTree)
        {
            ModuleFolders.Add(new ModuleFolderViewModel(folderName, files));
        }
    }
    
    [RelayCommand]
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
            ModeIconOpacity = 1;
            switch ((string)parameter)
            {
                case "ModuleEdit":
                    LoadedSheet.ChangeSheetMode(CharacterSheet.SheetMode.ModuleEdit);
                    ModuleEditModeButtonEnabled = false;
                    ModuleEditModeIconColor = AppResources.GetResource<IBrush>("Accent");
                    GameplayModeButtonEnabled = true;
                    GameplayModeIconColor = Brushes.Black;
                    ModuleSidebarEnabled = true;
                    break;
                case "Gameplay":
                    LoadedSheet.ChangeSheetMode(CharacterSheet.SheetMode.Gameplay);
                    GameplayModeButtonEnabled = false;
                    GameplayModeIconColor = AppResources.GetResource<IBrush>("Accent");
                    ModuleEditModeButtonEnabled = true;
                    ModuleEditModeIconColor = Brushes.Black;
                    ModuleSidebarEnabled = false;
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
        if (LoadedSheet != null && LoadedSheet.HasBeenModified)
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
        LoadedSheet = new CharacterSheet(this);
        SheetMenusEnabled = true;
        _currentFilePath = "";
        ChangeUiMode("Gameplay");
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
            var (success, errorMessages) = await LoadedSheet.AddModuleFromScript(files[0].Path.AbsolutePath, 0, 0);
            if (!success)
            {
                await LogModuleLoadError(errorMessages);
            }
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
        if (LoadedSheet != null && LoadedSheet.HasBeenModified)
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

        if (files.Count > 0)
        {
            var characterSheet = new CharacterSheet(this);

            if (await characterSheet.LoadSaveFile(files[0], token))
            {
                LoadedSheet = characterSheet;
                _currentFilePath = files[0].Path.AbsolutePath;
                SheetMenusEnabled = true;
                ChangeUiMode("Gameplay");
            }
            else
            {
                await LogSheetLoadError(characterSheet.SaveDataLoadErrorMessages);
            }
        }
    }

    // press Ctrl+Alt+[ to crash the program. this was for testing the crash handler and isn't useful anymore, but i'm
    // leaving it in because i think it's funny
    [RelayCommand]
    private void CrashProgram()
    {
        throw new Exception("You probably know why this happened because you have to press a really obscure key " +
                            "combo to get it. If you don't, then\n" +
                            "A. This \"error\" is for testing and shouldn't be reported.\n" +
                            "B. How did you even find the key combo? I'm both confused and impressed.");
    }

    [RelayCommand]
    private void ReloadModuleTree()
    {
        Utils.RebuildModuleTree();
        UpdateModuleFileTree(Utils.ModuleFileTree);
    }

    private bool _forceClose = false;
    public async void OnWindowClosed(object? sender, WindowClosingEventArgs e)
    {
        if (LoadedSheet != null && LoadedSheet.HasBeenModified && !_forceClose)
        {
            e.Cancel = true;
            if (!DialogHost.IsDialogOpen("RootDialog"))
            {
                var dialog = new ThreefoldDialog
                {
                    Message = "Do you want to save the current sheet? Unsaved changes will be lost.",
                    PositiveText = "Save",
                    NegativeText = "Discard",
                    NeutralText = "Cancel",
                };
                var result = await dialog.ShowAsync();
                if (result == ThreefoldDialog.ButtonType.Positive)
                {
                    await SaveSheetToCurrentPath(CancellationToken.None);
                }
                
                if (result != ThreefoldDialog.ButtonType.Neutral)
                {
                    _forceClose = true;
                    e.Cancel = false;
                    App.Window.Close();
                }
            }
        }
    }

    private async Task LogSheetLoadError(List<string> errors)
    {
        var dialog = new TwofoldDialog
        {
            Message = "An error occurred while loading this character sheet. The full error will be logged to a file.",
            PositiveText = "Open log folder",
            NegativeText = "OK"
        };
        var task = dialog.ShowAsync();
        
        var dateString = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss", CultureInfo.InvariantCulture);
        var logFile = new FileInfo(Utils.LogDir.FullName + $"/sheetLoadError_{dateString}.log");
        await File.WriteAllLinesAsync(logFile.FullName, errors);

        await task;
        if (task.Result == true)
        {
            Process.Start("explorer.exe", Utils.LogDir.FullName);
        }
    }
    
    public async Task LogModuleLoadError(List<string> errors)
    {
        var dialog = new TwofoldDialog
        {
            Message = "An error occurred while loading this module. The full error will be logged to a file.",
            PositiveText = "Open log folder",
            NegativeText = "OK"
        };
        var task = dialog.ShowAsync();
        
        var dateString = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss", CultureInfo.InvariantCulture);
        var logFile = new FileInfo(Utils.LogDir.FullName + $"/moduleLoadError_{dateString}.log");
        await File.WriteAllLinesAsync(logFile.FullName, errors);

        await task;
        if (task.Result == true)
        {
            Process.Start("explorer.exe", Utils.LogDir.FullName);
        }
    }

    private async Task SaveSheet(IStorageFile file)
    {
        await using var writer = new StreamWriter(await file.OpenWriteAsync());
        var saveData = LoadedSheet!.GetSaveData();
        await writer.WriteAsync(JsonSerializer.Serialize(saveData));
    }
}