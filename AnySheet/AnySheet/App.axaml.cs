using System;
using System.Collections.Generic;
using System.IO;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AnySheet.SheetModule.Primitives;
using Avalonia.Markup.Xaml;
using AnySheet.ViewModels;
using AnySheet.Views;
using Avalonia.Controls;
using Lua;
using LuaLib;

namespace AnySheet;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }
    
    public new static App? Current => Application.Current as App;
    public static TopLevel? TopLevel =>
        Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop ?
            desktop.MainWindow : null;
    
    public static MainWindow? Window { get; private set; }
    
    // the loaded sheet in the main window gets passed through this so that the module view models can add modules
    // to it. it's spaghetti but the alternative was to give all of them a reference to the main window and i liked that
    // option even less than this (doing it this way also future-proofs things because the trigger system will also
    // probably need this once i get to adding that)
    public static CharacterSheet? LoadedSheet =>
        Window is { DataContext: MainWindowViewModel vm } ? vm.LoadedSheet : null;

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
            // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
            DisableAvaloniaDataAnnotationValidation();
            
            var windowContext = new MainWindowViewModel();
            var window = new MainWindow
            {
                DataContext = windowContext
            };
            window.Closing += (sender, e) => windowContext.OnWindowClosed(sender, e);
            window.Closing += OnWindowClosed;
            
            desktop.MainWindow = window;
            Window = window;
            
            // this may cause a race condition somewhere but whatever i'll burn that bridge when i get there
            LoadConfig();
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

    private static async Task LoadConfig()
    {
        await Utils.TryLoadConfigFile();
        ((MainWindowViewModel)Window.DataContext).UpdateModuleFileTree(Utils.ModuleFileTree);
    }
    
    private static void OnWindowClosed(object? sender, EventArgs? e)
    {
        Utils.SaveConfigFile();
    }
}