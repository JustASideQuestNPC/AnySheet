using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.Json.Nodes;
using AnySheet.ViewModels;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using CCLibrary;
using Lua;

namespace AnySheet.SheetModule.Primitives;

[LuaObject]
public partial class ListPrimitiveLua : ModulePrimitiveLuaBase
{
    private ListPrimitive? _uiControl;
    private readonly List<string> _entries = [];

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
        _uiControl = new ListPrimitive(GridX, GridY, GridWidth, GridHeight);
        foreach (var entry in _entries)
        {
            _uiControl.Entries.Add(new ListPrimitiveEntryViewModel(_uiControl, entry));
        }
        return _uiControl;
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
        var entries = new JsonArray();
        foreach (var entry in _uiControl.Entries)
        {
            entries.Add(entry.Text);
        }
        
        return new JsonObject { ["entries"] = entries };
    }

    public override void LoadSaveObject(JsonObject obj)
    {
        if (obj["entries"] == null || obj["entries"] is not JsonArray entries)
        {
            throw new JsonException("Invalid save data for ListPrimitive.");
        }

        foreach (var entry in entries)
        {
            if (entry is not JsonValue text)
            {
                throw new JsonException("Invalid save data for ListPrimitive.");
            }

            _entries.Add(entry.ToString());
        }
    }
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
            AddEntry(NewEntryTextBox.Text);
            NewEntryTextBox.Text = "";
        }
    }

    public void NewEntryTextBoxEnterPress()
    {
        if (!string.IsNullOrEmpty(NewEntryTextBox.Text))
        {
            AddEntry(NewEntryTextBox.Text);
            NewEntryTextBox.Text = "";
        }
    }

    private void AddEntry(string text)
    {
        Console.WriteLine($"Added list entry: '{text}'");
        var entry = new ListPrimitiveEntryViewModel(this, text);
        Entries.Add(entry);
        Console.WriteLine(Entries.Count);
    }
    
    public void ClearEntryButtonClick(object? sender, RoutedEventArgs? args)
    {
        NewEntryTextBox.Text = "";
    }

    public void RemoveEntry(ListPrimitiveEntryViewModel entry)
    {
        Entries.Remove(entry);
    }
}