using Lua;

namespace LuaLib;

[LuaObject]
public partial class LuaSheetModule
{
    private static readonly Dictionary<string, LuaValueType> ConstructorArgs = new()
    {
        ["elements"] = LuaValueType.Table,
        ["[noBorder]"] = LuaValueType.Boolean
    };
    public bool NoBorder { get; private set; } = false;
    public LuaTable Elements { get; private set; } = new();

    [LuaMember("create")]
    private static LuaSheetModule Create(LuaTable args)
    {
        LuaSandbox.VerifyTable(args, ConstructorArgs);
        
        var elements = args["elements"].Read<LuaTable>();
        if (elements.ArrayLength == 0)
        {
            throw new ArgumentException("Module must contain at least one primitive element.");
        }

        var module = new LuaSheetModule
        {
            NoBorder = LuaSandbox.GetTableValueOrDefault(args, "border", false),
            Elements = elements
        };
        return module;
    }
}