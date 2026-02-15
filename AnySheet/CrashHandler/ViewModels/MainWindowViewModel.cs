using System;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CrashHandler.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty] private string _headerText;
    [ObservableProperty] private string _subtitleText;
    [ObservableProperty] private string _bodyText;
    [ObservableProperty] private bool _buttonsEnabled;

    private string _logPath;
    
    public MainWindowViewModel(string logPath, string errorMessage)
    {
        _logPath = logPath;
        
        if (errorMessage != "")
        {
            _headerText = "Something has gone VERY VERY WRONG!!";
            _subtitleText = "(It probably wasn't your fault)";
            _bodyText = errorMessage;
            _buttonsEnabled = true;
        }
        else
        {
            _headerText = "Nothing has gone wrong.";
            _subtitleText = "This is absolutely your fault.";
            _bodyText = "Why would you even do this? There is literally no reason for you to run the crash handler " +
                        "on its own. If you were hoping for some kind of easter egg, there isn't one. Just this " +
                        "block of text questioning your sanity.";
            _buttonsEnabled = false;
        }
    }

    [RelayCommand]
    private async Task SaveErrorToFile(CancellationToken token)
    {
        if (App.TopLevel == null)
        {
            Console.WriteLine("No top level window!");
            return;
        }
        
        var startFolder = await App.TopLevel.StorageProvider.TryGetFolderFromPathAsync(new Uri(_logPath));
        if (startFolder == null)
        {
            Console.WriteLine("Log folder not found!");
            return;
        }
        
        var dateString = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss", CultureInfo.InvariantCulture);
        
        var file = await App.TopLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Save sheet as...",
            DefaultExtension = "log",
            SuggestedFileName = $"crash_{dateString}",
            SuggestedStartLocation = startFolder
        });

        if (file != null)
        {
            await File.WriteAllTextAsync(file.Path.AbsolutePath, BodyText, token);
        }
    }
}