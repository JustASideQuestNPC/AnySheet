using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using System.Linq;
using System.Reflection;
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
    public static readonly IEnumerable<MethodInfo> PrimitiveReaders = typeof(ModulePrimitiveLuaBase)
                                    .Assembly.GetTypes()
                                    .Where(t => t.IsSubclassOf(typeof(ModulePrimitiveLuaBase)) &&!t.IsAbstract)
                                    .Select(t => t.GetMethod("TryReadLua", BindingFlags.Static | BindingFlags.Public))!;

    private static readonly string WorkingDirectoryForwardSlash = Environment.CurrentDirectory.Replace('\\', '/');
    
    // i should probably make a utils class or something but whatever
    public static bool PathContainsWorkingDirectory(string path)
    {
        return path.StartsWith(WorkingDirectoryForwardSlash) || path.StartsWith(Environment.CurrentDirectory);
    }
    
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }
    
    public new static App? Current => Application.Current as App;
    public static TopLevel? TopLevel =>
        Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop ?
            desktop.MainWindow : null;

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
            // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
            DisableAvaloniaDataAnnotationValidation();
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel(),
            };
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
}