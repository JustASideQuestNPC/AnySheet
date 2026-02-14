using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using AvaloniaDialogs.Views;

namespace AnySheet.Popups;

public partial class ConfirmUnsavedSheetDialog : BaseDialog<bool>
{
    public ConfirmUnsavedSheetDialog()
    {
        InitializeComponent();
    }

    private void SaveButtonClick(object? sender, RoutedEventArgs e)
    {
        Close(true);
    }
    
    private void DiscardButtonClick(object? sender, RoutedEventArgs e)
    {
        Close(true);
    }
    
    private void CancelButtonClick(object? sender, RoutedEventArgs e)
    {
        Close(false);
    }
}