using Lua;

namespace LuaLib;

[LuaObject]
public partial class LuaSheetModule
{
    private static readonly Dictionary<string, LuaValueType> ConstructorArgs = new()
    {
        ["elements"] = LuaValueType.Table,
        ["[triggers]"] = LuaValueType.Table
    };
    public LuaTable Elements { get; private set; } = new();
    public Dictionary<string, LuaFunction> Triggers { get; private set; } = new();

    [LuaMember("create")]
    private static LuaSheetModule Create(LuaTable args)
    {
        LuaSandbox.VerifyTable(args, ConstructorArgs);
        
        var elements = args["elements"].Read<LuaTable>();
        // LuaSandbox.VerifyTable() can't distinguish between arrays and dictionaries because they both have the same
        // LuaValue type
        if (elements.HashMapCount > 0)
        {
            throw new ArgumentException("Elements must be an array of primitive elements, not a dictionary.");
        }
        if (elements.ArrayLength == 0)
        {
            throw new ArgumentException("Module must contain at least one primitive element.");
        }

        Dictionary<string, LuaFunction> triggers = [];
        if (args.ContainsKey("triggers"))
        {
            if (args["triggers"].Read<LuaTable>().ArrayLength > 0)
            {
                throw new ArgumentException("Triggers must be a dictionary of trigger functions, not an array.");
            }
            
            foreach (var (key, value) in args["triggers"].Read<LuaTable>())
            {
                if (!value.TryRead(out LuaFunction trigger))
                {
                    throw new ArgumentException($"Trigger '{key}' is not a function.");
                }
                triggers[key.Read<string>()] = trigger;
            }
            
            Console.WriteLine($"Module has {triggers.Count} triggers.");
        }
        else
        {
            Console.WriteLine("Module has no triggers.");
        }

        var module = new LuaSheetModule
        {
            Elements = elements,
            Triggers = triggers
        };
        return module;
    }
}