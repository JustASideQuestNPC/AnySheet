using AnySheet.SheetModule.Primitives;
using Avalonia.Interactivity;

namespace AnySheet.ViewModels;

public partial class ListPrimitiveEntryViewModel : ViewModelBase
{
    private ListPrimitive _parent;
    
    public string Text { get; set; } = "";

    public ListPrimitiveEntryViewModel(ListPrimitive parent, string text)
    {
        _parent = parent;
        Text = text;
    }
    
    public void RemoveEntry()
    {
        _parent.RemoveEntry(this);
    }
}