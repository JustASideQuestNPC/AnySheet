using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AnySheet.ViewModels;

public partial class ModuleFileViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _displayName;

    private readonly string _fileName;
    
    public ModuleFileViewModel(string fileName, string displayName)
    {
        _fileName = fileName;
        _displayName = displayName;
    }

    [RelayCommand]
    public async Task AddModule()
    {
        // this *should* be unreachable because the sidebar menu is disabled until a sheet is loaded
        if (App.LoadedSheet == null)
        {
            Console.WriteLine("How did you even get here?");
            return;
        }
        
        await App.LoadedSheet.TryAddModuleFromFile(_fileName);
    }
}