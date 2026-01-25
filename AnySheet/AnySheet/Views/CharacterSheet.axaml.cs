using Avalonia.Controls;

namespace AnySheet.Views;

public partial class CharacterSheet : UserControl
{
    public const int GridSize = 27;

    private int _currentWidth = 0;
    private int _currentHeight = 0;
    
    public CharacterSheet()
    {
        InitializeComponent();
        // for testing; will be removed later
        ModuleGrid.Children.Add(new SheetModule.SheetModule(this, 0, 0, "module1.lua"));
    }

    // this has to be called by modules when they finish loading because the build scripts are async
    public void UpdateGrid(SheetModule.SheetModule module)
    {
        // expand the grid if necessary
        while (_currentWidth < module.GridX + module.GridWidth)
        {
            ModuleGrid.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(GridSize)));
            ++_currentWidth;
        }
        while (_currentHeight < module.GridY + module.GridHeight)
        {
            ModuleGrid.RowDefinitions.Add(new RowDefinition(new GridLength(GridSize)));
            ++_currentHeight;
        }
    }

    public void AddModule(int x, int y, string scriptPath)
    {
        var module = new SheetModule.SheetModule(this, x, y, "module1.lua");
    }
    
    public void RemoveModule(SheetModule.SheetModule module)
    {
        ModuleGrid.Children.Remove(module);
    }
}