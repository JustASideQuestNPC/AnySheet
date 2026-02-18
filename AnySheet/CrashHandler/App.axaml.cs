using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using CrashHandler.ViewModels;
using CrashHandler.Views;
using System;
using System.Linq;

namespace CrashHandler;

public partial class App : Application
{
    // i have ai suggestions enabled in my editor. i got 3 characters into this link and it perfectly autocompleted the
    // rest of it. i didn't even get to "youtube", it was just "htt" and then boom there's the entire thing. i haven't
    // changed any of the settings either so apparently it's just built this way.
    private const string TargetLink = "https://www.youtube.com/watch?v=dQw4w9WgXcQ?autoplay=1";

    private MainWindow _window = null!;
    
    public static TopLevel? TopLevel =>
        Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop ?
            desktop.MainWindow : null;
    
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
            // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
            DisableAvaloniaDataAnnotationValidation();
            
            var commandLine = Environment.CommandLine;
            // strip off the first argument, which is the path to the executable
            var logPathStart = commandLine.IndexOf(' ', 1);
            var logPathEnd = commandLine.IndexOf(' ', logPathStart + 1);

            MainWindowViewModel dataContext;
            if (logPathStart == -1)
            {
                dataContext = new MainWindowViewModel("", "");
            }
            else
            {
                var logPath = commandLine[(logPathStart + 1)..logPathEnd];
                var errorMessage = commandLine[(logPathEnd + 1)..];
                dataContext = new MainWindowViewModel(logPath, errorMessage);
            }

            _window = new MainWindow
            {
                DataContext = dataContext
            };

            if (logPathStart == -1)
            {
                _window.Closing += (sender, e) => PunishUserOnExit();
            }
            
            desktop.MainWindow = _window;
            _window.Focus();
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void DisableAvaloniaDataAnnotationValidation()
    {
        // Get an array of plugins to remove
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        // remove each entry found
        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }

    private void PunishUserOnExit()
    {
        var launcher = _window.Launcher;
        launcher.LaunchUriAsync(new Uri(TargetLink));
    }
}