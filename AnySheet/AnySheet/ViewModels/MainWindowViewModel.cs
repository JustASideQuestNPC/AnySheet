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
using Avalonia;
using Avalonia.Animation;
using Avalonia.Styling;
using Avalonia.Xaml.Interactivity;
using Material.Icons.Avalonia;

namespace AnySheet.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty] private CharacterSheet? _loadedSheet;
    [ObservableProperty] private ObservableCollection<ModuleFolderViewModel> _moduleFolders = [];
    [ObservableProperty] private ObservableCollection<TriggerListEntry> _triggerListEntries = [];
    [ObservableProperty] private bool _sheetMenusEnabled;
    [ObservableProperty] private bool _moduleSidebarEnabled;
    [ObservableProperty] private bool _triggerSidebarEnabled;
    [ObservableProperty] private double _modeIconOpacity = 0.5;
    [ObservableProperty] private bool _gameplayModeButtonEnabled;
    [ObservableProperty] private IBrush _gameplayModeIconColor = Brushes.Black;
    [ObservableProperty] private bool _moduleEditModeButtonEnabled;
    [ObservableProperty] private IBrush _moduleEditModeIconColor = Brushes.Black;
    [ObservableProperty] private bool _triggerEditModeButtonEnabled;
    [ObservableProperty] private IBrush _triggerEditModeIconColor = Brushes.Black;
    [ObservableProperty] private bool _zoomButtonsEnabled;
    [ObservableProperty] private TextBlock _saveIndicator;
    [ObservableProperty] private string _newTriggerEntryName = "";

    // path to the currently loaded sheet
    private string _currentFilePath = "";

    public MainWindowViewModel()
    {
        // create a new sheet on startup - this technically means i can remove all the checks for whether a sheet is
        // loaded yet, but i really don't feel like doing that so i'm not going to.
        CreateNewSheet();
    }

    // updates UI controls when the module tree gets rebuilt
    public void UpdateModuleFileTree(Dictionary<string, List<(string, string)>> moduleFileTree)
    {
        ModuleFolders.Clear();
        foreach (var (folderName, files) in moduleFileTree)
        {
            ModuleFolders.Add(new ModuleFolderViewModel(folderName, files));
        }
    }

    [RelayCommand]
    private void ChangeUiMode(object? parameter)
    {
        // this actually is reachable because keybinds can't be disabled
        if (LoadedSheet == null)
        {
            return;
        }

        if (parameter != null)
        {
            Console.WriteLine($"Changing UI mode to {parameter}");
            ModeIconOpacity = 1;
            switch ((string)parameter)
            {
                case "ModuleEdit":
                    LoadedSheet.ChangeSheetMode(CharacterSheet.SheetMode.ModuleEdit);
                    GameplayModeButtonEnabled = true;
                    GameplayModeIconColor = Brushes.Black;
                    ModuleEditModeButtonEnabled = false;
                    ModuleEditModeIconColor = AppResources.GetResource<IBrush>("Accent");
                    TriggerEditModeButtonEnabled = true;
                    TriggerEditModeIconColor = Brushes.Black;
                    ZoomButtonsEnabled = false;
                    ModuleSidebarEnabled = true;
                    TriggerSidebarEnabled = false;
                    break;
                case "Gameplay":
                    LoadedSheet.ChangeSheetMode(CharacterSheet.SheetMode.Gameplay);
                    GameplayModeButtonEnabled = false;
                    GameplayModeIconColor = AppResources.GetResource<IBrush>("Accent");
                    ModuleEditModeButtonEnabled = true;
                    ModuleEditModeIconColor = Brushes.Black;
                    TriggerEditModeButtonEnabled = true;
                    TriggerEditModeIconColor = Brushes.Black;
                    ZoomButtonsEnabled = true;
                    ModuleSidebarEnabled = false;
                    TriggerSidebarEnabled = false;
                    break;
                case "TriggerEdit":
                    LoadedSheet.ChangeSheetMode(CharacterSheet.SheetMode.TriggerEdit);
                    GameplayModeButtonEnabled = true;
                    GameplayModeIconColor = Brushes.Black;
                    ModuleEditModeButtonEnabled = true;
                    ModuleEditModeIconColor = Brushes.Black;
                    TriggerEditModeButtonEnabled = false;
                    TriggerEditModeIconColor = AppResources.GetResource<IBrush>("Accent");
                    ZoomButtonsEnabled = true;
                    ModuleSidebarEnabled = false;
                    TriggerSidebarEnabled = true;
                    break;
            }
        }
    }

    [RelayCommand]
    private async Task CreateNewSheet()
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
        TriggerListEntries.Clear();
        LoadedSheet = new CharacterSheet(this);
        SheetMenusEnabled = true;
        _currentFilePath = "";
        ChangeUiMode("ModuleEdit");
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
            var (success, errorMessages, popupMessage) = await LoadedSheet.AddModuleFromScript(
                files[0].Path.AbsolutePath, 0, 0);
            if (!success)
            {
                await LogModuleLoadError(errorMessages, popupMessage);
            }
        }
    }

    [RelayCommand]
    private async Task SaveSheetToCurrentPath(CancellationToken token)
    {
        // this actually is reachable because keybinds can't be disabled
        if (LoadedSheet == null)
        {
            return;
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

                TriggerListEntries.Clear();
                foreach (var triggerName in LoadedSheet.TriggerNames)
                {
                    var entry = new TriggerListEntry(this, triggerName);
                    TriggerListEntries.Add(entry);
                    Console.WriteLine($"Added trigger '{triggerName}' to trigger menu.");
                }
                _currentFilePath = files[0].Path.AbsolutePath;
                SheetMenusEnabled = true;
                ChangeUiMode("Gameplay");
            }
            else
            {
                await LogSheetLoadError(characterSheet.SaveDataLoadErrorMessages,
                                        characterSheet.SaveDataLoadErrorPopupMessage);
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

    [RelayCommand]
    private void OpenModuleFolder()
    {
        if (App.TopLevel == null)
        {
            Console.WriteLine("No top level window!");
            return;
        }

        var startFolder = Directory.CreateDirectory(Environment.CurrentDirectory + "/Modules/");
        Process.Start("explorer.exe", startFolder.FullName);
    }

    [RelayCommand]
    private void ZoomIn()
    {
        LoadedSheet.ZoomBorder.ZoomInCommand.Execute(null);
    }

    [RelayCommand]
    private void ZoomOut()
    {
        LoadedSheet.ZoomBorder.ZoomOutCommand.Execute(null);
    }

    [RelayCommand]
    private void ResetCamera()
    {
        LoadedSheet?.ZoomBorder.ResetMatrix();
    }

    [RelayCommand]
    private async Task ResetSheetPositions()
    {
        if (LoadedSheet == null)
        {
            return;
        }
        
        var dialog = new TwofoldDialog
        {
            Message = "This will reset the position of ALL modules on the sheet. This should only be use if you " +
                      "loaded a sheet and all the modules are in the wrong position. Are you sure you want to do this?",
            PositiveText = "Yes",
            NegativeText = "No"
        };
        var result = await dialog.ShowAsync();
        if (result == true)
        {
            ResetCamera();
            LoadedSheet.ResetModulePositions();
        }
    }

    [RelayCommand]
    private void CreateNewTrigger()
    {
        if (!string.IsNullOrEmpty(NewTriggerEntryName) && LoadedSheet != null &&
            !LoadedSheet.HasTrigger(NewTriggerEntryName))
        {
            LoadedSheet.AddTrigger(NewTriggerEntryName);
            var entry = new TriggerListEntry(this, NewTriggerEntryName);
            TriggerListEntries.Add(entry);
            SetEditingTrigger(entry);
        }
        ClearTriggerEntryBox();
    }

    [RelayCommand]
    private void ClearTriggerEntryBox()
    {
        NewTriggerEntryName = "";
    }

    public void RemoveTriggerEntry(TriggerListEntry entry)
    {
        TriggerListEntries.Remove(entry);
        LoadedSheet?.RemoveTrigger(entry.Text);
    }
    
    // sets the trigger currently being edited
    public void SetEditingTrigger(TriggerListEntry entry)
    {
        LoadedSheet?.SetEditingTrigger(entry.Text);
    }

    public void ActivateTrigger(TriggerListEntry entry)
    {
        Console.WriteLine($"Activating trigger: {entry.Text}");
        LoadedSheet?.ActivateTrigger(entry.Text);
    }

    private bool _forceClose = false;
    public async Task OnWindowClosed(object? sender, WindowClosingEventArgs e)
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
                    App.Window?.Close();
                }
            }
        }
    }

    private async Task LogSheetLoadError(List<string> errors, string displayMessage)
    {
        var dialog = new TwofoldDialog
        {
            Message = $"1 or more error(s) occurred while loading this character sheet:\n{displayMessage}",
            PositiveText = "Open log folder",
            NegativeText = "OK",
            Width = 500,
            Height = 350
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
    
    public async Task LogModuleLoadError(List<string> errors, string displayMessage)
    {
        var dialog = new TwofoldDialog
        {
            Message = $"An error occurred while loading this module:\n{displayMessage}",
            PositiveText = "Open log folder",
            NegativeText = "OK",
            Width = 500,
            Height = 350
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
        // the animation only plays when the new text block is created
        SaveIndicator = new TextBlock
        {
            Classes = { "SaveIndicator" },
            Text = "Saved"
        };
    }
}