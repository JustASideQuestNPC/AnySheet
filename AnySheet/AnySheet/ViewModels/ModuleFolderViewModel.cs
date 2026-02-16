using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AnySheet.ViewModels;

public partial class ModuleFolderViewModel : ViewModelBase
{
    [ObservableProperty]
    private ObservableCollection<ModuleFileViewModel> _fileButtons = [];
    
    [ObservableProperty]
    private string _folderName;

    [ObservableProperty]
    private bool _showFiles;

    public ModuleFolderViewModel(string folderName, List<(string, string)> files)
    {
        Console.WriteLine($"Adding sidebar buttons for folder {folderName}");
        FolderName = folderName;
        foreach (var (fileName, displayName) in files)
        {
            _fileButtons.Add(new ModuleFileViewModel($"~{folderName}\\{fileName}", displayName));
        }
    }
    
    [RelayCommand]
    public void ToggleFiles() => ShowFiles = !ShowFiles;
}