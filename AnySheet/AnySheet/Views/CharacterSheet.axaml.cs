using System;
using System.Windows.Input;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;

namespace AnySheet.Views;

public partial class CharacterSheet : UserControl
{
    public const int GridSize = 27;
    
    public CharacterSheet()
    {
        InitializeComponent();
        // for testing; will be removed later
        ModuleGrid.Children.Add(new SheetModule.SheetModule(this, 0, 0, "module1.lua"));
    }
    
    public void RemoveModule(SheetModule.SheetModule module)
    {
        ModuleGrid.Children.Remove(module);
    }
}