using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AnySheet.Views;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Tmds.DBus.Protocol;

namespace AnySheet.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty]
    private CharacterSheet? _characterSheet;

    [ObservableProperty]
    private bool _sheetMenusEnabled = false;
    
    public void CreateNewSheet()
    {
        Console.WriteLine("Create new sheet");
        CharacterSheet = new CharacterSheet();
        SheetMenusEnabled = true;
    }

    [RelayCommand]
    public async Task AddModuleFile(CancellationToken token)
    {
        // menus are disabled until a sheet is loaded, so this *should* be unreachable
        if (CharacterSheet == null)
        {
            throw new InvalidOperationException("How did you even get here?");
        }
        
        var files = await App.TopLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Choose a Lua module file",
            AllowMultiple = false,
            SuggestedFileType = new FilePickerFileType("lua")
        });

        CharacterSheet.AddModuleFromScript(files[0].Path.AbsolutePath, 0, 0);
    }
}