namespace AnySheet.ViewModels;

/// <summary>
/// A single entry in a module's trigger list; shown when editing triggers.
/// </summary>
public class ModuleTriggerToggleListEntry(SheetModule.SheetModule parent, string name, bool buttonState)
{
    private SheetModule.SheetModule _parent = parent;
    public bool ButtonState { get; set; } = buttonState;
    public string Name { get; set; } = name;

    public void Toggle()
    {
        _parent.UpdateTriggerGroup(Name, ButtonState);
    }
}