using System.Text.Json;
using System.Text.Json.Nodes;
using Avalonia.Controls;
using Lua;

namespace AnySheet.SheetModule.Primitives;

[LuaObject]
public partial class MultilineTextBoxLua : ModulePrimitiveLuaBase
{
    private string _text = "";
    
    [LuaMember("text")]
    public string Text
    {
        get => _text;
        set
        {
            _text = value;
            if (_uiControl != null) _uiControl.TextBox.Text = value;
        }
    }
    
    private MultiLineTextBoxPrimitive? _uiControl;

    [LuaMember("create")]
    public new static MultilineTextBoxLua CreateLua(LuaTable args)
    {
        VerifyPositionArgs(args);
        return new MultilineTextBoxLua
        {
            GridX = args["x"].Read<int>(),
            GridY = args["y"].Read<int>(),
            GridWidth = args["width"].Read<int>(),
            GridHeight = args["height"].Read<int>(),
            Text = "",
        };
    }
    
    // ReSharper disable once UnusedMember.Global
    public static bool TryReadLua(LuaValue value, out MultilineTextBoxLua module)
    {
        return value.TryRead(out module);
    }
    
    public override UserControl CreateUiControl()
    {
        _uiControl = new MultiLineTextBoxPrimitive(this, GridX, GridY, GridWidth, GridHeight, Text);
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
    
    public override JsonObject GetSaveObject()
    {
        return new JsonObject { ["text"] = Text };
    }
    
    public override void LoadSaveObject(JsonObject obj)
    {
        if (obj["text"] != null && obj["text"]!.AsValue().TryGetValue<string>(out var text))
        {
            Text = text;
            return;
        }

        throw new JsonException("Invalid save data for MultilineTextBoxPrimitive.");
    }
}

public partial class MultiLineTextBoxPrimitive : UserControl
{
    private readonly MultilineTextBoxLua _parent;

    public MultiLineTextBoxPrimitive(MultilineTextBoxLua parent, int x, int y, int width, int height,
        string initialText)
    {
        InitializeComponent();
        
        Grid.SetColumn(this, x);
        Grid.SetRow(this, y);
        Grid.SetColumnSpan(this, width);
        Grid.SetRowSpan(this, height);
        
        _parent = parent;
        
        TextBox.Text = initialText;
    }
}