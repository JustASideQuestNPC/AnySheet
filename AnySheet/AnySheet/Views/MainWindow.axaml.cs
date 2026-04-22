using System;
using System.Reactive;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Styling;

namespace AnySheet.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        this.GetObservable(ActualThemeVariantProperty)
            .Subscribe(new AnonymousObserver<ThemeVariant>(_ => UpdateIcon()));
    }

    /// <summary>
    /// Whenever the window theme changes between light and dark, switches to whichever desktop icon is more visible.
    /// </summary>
    private void UpdateIcon()
    {
        // names correspond to the color of the image, *not* which theme they're used for
        var iconName = (ActualThemeVariant == ThemeVariant.Light ? "Dark" : "Light");
        Icon = new WindowIcon(
            AssetLoader.Open(new Uri($"avares://AnySheet/Assets/{iconName}Icon.ico", UriKind.RelativeOrAbsolute)));
    }
}