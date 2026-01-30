using System;
using System.Windows.Input;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;

namespace AnySheet.Views;

public partial class CharacterSheet : UserControl
{
    public enum SheetMode
    {
        Gameplay,
        ModuleEdit,
        TriggerEdit // currently unused
    }
    
    public const int GridSize = 27;
    
    public CharacterSheet()
    {
        InitializeComponent();
    }

    public void AddModuleFromScript(string path, int gridX, int gridY)
    {
        ModuleGrid.Children.Add(new SheetModule.SheetModule(this, gridX, gridY, path));
    }
    
    public void RemoveModule(SheetModule.SheetModule module)
    {
        ModuleGrid.Children.Remove(module);
    }
}