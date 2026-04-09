using System;

namespace AnySheet.ViewModels;

/// <summary>
/// A single entry in the trigger list.
/// </summary>
public partial class TriggerListEntry(MainWindowViewModel parent, string text) : ViewModelBase
{
    private MainWindowViewModel _parent = parent;
    
    public string Text { get; set; } = text;

    public void SetEditing()
    {
        _parent.SetEditingTrigger(this);
    }

    public void RemoveEntry()
    {
        _parent.RemoveTriggerEntry(this);
    }

    public void Activate()
    {
        Console.WriteLine($"Activating trigger: {Text}");
        _parent.ActivateTrigger(this);
    }
}