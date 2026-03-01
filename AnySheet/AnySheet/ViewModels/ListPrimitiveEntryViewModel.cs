using AnySheet.SheetModule.Primitives;

namespace AnySheet.ViewModels;

/// <summary>
/// A single entry in a list module primitive.
/// </summary>
public partial class ListPrimitiveEntryViewModel : ViewModelBase
{
    private ListPrimitive _parent;
    
    public string Text { get; set; } = "";
    private string _initialText = "";
    
    public bool HasBeenModified => Text != _initialText;

    public ListPrimitiveEntryViewModel(ListPrimitive parent, string text)
    {
        _parent = parent;
        _initialText = text;
        Text = text;
    }
    
    public void RemoveEntry()
    {
        _parent.RemoveEntry(this);
    }
}