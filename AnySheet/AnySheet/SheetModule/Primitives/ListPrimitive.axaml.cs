using System;
using System.Collections.ObjectModel;
using System.Text.Json.Nodes;
using AnySheet.ViewModels;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Lua;

namespace AnySheet.SheetModule.Primitives;

[LuaObject]
public partial class ListPrimitiveLua : ModulePrimitiveLuaBase
{
    private ListPrimitive? _uiControl;

    [LuaMember("create")]
    public new static ListPrimitiveLua CreateLua(LuaTable args)
    {
        VerifyPositionArgs(args);
        
        return new ListPrimitiveLua
        {
            GridX = args["x"].Read<int>(),
            GridY = args["y"].Read<int>(),
            GridWidth = args["width"].Read<int>(),
            GridHeight = args["height"].Read<int>(),
        };
    }
    
    public static bool TryReadLua(LuaValue value, out ListPrimitiveLua result)
    {
        return value.TryRead(out result);
    }

    public override UserControl CreateUiControl()
    {
        return _uiControl = new ListPrimitive(GridX, GridY, GridWidth, GridHeight);
    }
    
    public override void EnableUiControl()
    {
        _uiControl.IsEnabled = true;
    }

    public override void DisableUiControl()
    {
        _uiControl.IsEnabled = false;
    }
    
    public override JsonObject? GetSaveObject()
    {
        return null;
    }
    public override void LoadSaveObject(JsonObject obj) {}
}

public partial class ListPrimitive : UserControl
{
    public ObservableCollection<ListPrimitiveEntryViewModel> Entries { get; } = [];
    
    public ListPrimitive(int x, int y, int width, int height)
    {
        InitializeComponent();
        DataContext = this;
        
        Grid.SetColumn(this, x);
        Grid.SetRow(this, y);
        Grid.SetColumnSpan(this, width);
        Grid.SetRowSpan(this, height);
    }

    public void AddEntryButtonClick(object? sender, RoutedEventArgs? args)
    {
        if (!string.IsNullOrEmpty(NewEntryTextBox.Text))
        {
            Console.WriteLine($"Added list entry: '{NewEntryTextBox.Text}'");
            var entry = new ListPrimitiveEntryViewModel(this, NewEntryTextBox.Text);
            Entries.Add(entry);
            Console.WriteLine(Entries.Count);
            NewEntryTextBox.Text = "";
        }
    }

    public void RemoveEntry(ListPrimitiveEntryViewModel entry)
    {
        Entries.Remove(entry);
    }
}