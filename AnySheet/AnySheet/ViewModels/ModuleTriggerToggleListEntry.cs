using CommunityToolkit.Mvvm.ComponentModel;

namespace AnySheet.ViewModels;

/// <summary>
/// A single entry in a module's trigger list; shown when editing triggers.
/// </summary>
public partial class ModuleTriggerToggleListEntry(SheetModule.SheetModule parent, string name,
    bool buttonState) : ViewModelBase
{
    private SheetModule.SheetModule _parent = parent;
    
    [ObservableProperty] private bool _buttonState = buttonState;
    public string Name { get; set; } = name;

    public void Toggle()
    {
        _parent.UpdateTriggerGroup(Name, ButtonState);
    }
}