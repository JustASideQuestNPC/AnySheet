using Lua;
using Lua.Standard;

namespace LuaLib;

public class LuaSandbox
{
    // a TimeoutException is thrown if a script takes more than this many milliseconds to execute
    private const int ScriptTimeout = 5000;

    private readonly LuaState _state;
    
    public LuaTable Environment => _state.Environment;

    public LuaSandbox()
    {
        _state = LuaState.Create();
        // only open safe libraries
        _state.OpenBasicLibrary();
        _state.OpenStringLibrary();
        _state.OpenTableLibrary();
        _state.OpenMathLibrary();
        
        // redefine print to add a little header
        var oldPrint = _state.Environment["print"].Read<LuaFunction>();
        _state.Environment["print"] = new LuaFunction(async (context, ct) =>
        {
            var args = context.Arguments.ToArray();
            if (args.Length > 0)
            {
                args[0] = $"Lua says: {args[0]}";
            }
            
            var printTask = _state.CallAsync(oldPrint, args, ct);
            await printTask;
            context.Return(printTask.Result);
            
            // i have no idea why this is necessary
            Console.Write("\r");
            return 1;
        });
    }
    
    // helper that throws an exception if a task takes too long to run
    private static async Task<T> TimeoutTask<T>(Task<T> task)
    {
        using var timeoutTokenSource = new CancellationTokenSource();
        var completedTask = await Task.WhenAny(task, Task.Delay(ScriptTimeout, timeoutTokenSource.Token));
        if (completedTask == task)
        {
            await timeoutTokenSource.CancelAsync();
            return await task; // propagates exceptions
        }
        throw new TimeoutException();
    }
    
    private static async Task<(LuaValue[], bool, string)> RunLuaTaskAsync(ValueTask<LuaValue[]> luaTask)
    {
        try
        {
            var task = await TimeoutTask(luaTask.AsTask());
            return (task, true, "");
        }
        catch (TimeoutException)
        {
            Console.WriteLine("Lua execution timed out.");
            return ([], false, "Lua execution timed out.");
        }
        catch (LuaRuntimeException e)
        {
            Console.WriteLine($"Lua execution failed: {e.Message}");
            return ([], false, e.Message);
        }
    }
    
    public Task<(LuaValue[], bool, string)> DoStringAsync(string code) => RunLuaTaskAsync(_state.DoStringAsync(code));
    
    public Task<(LuaValue[], bool, string)> DoFileAsync(string path) => RunLuaTaskAsync(_state.DoFileAsync(path));
    
    public Task<(LuaValue[], bool, string)> DoFunctionAsync(LuaFunction function) => RunLuaTaskAsync(_state.CallAsync(function, []));
    public Task<(LuaValue[], bool, string)> DoFunctionAsync(LuaFunction function, LuaValue[] args) =>
        RunLuaTaskAsync(_state.CallAsync(function, args));

    public static T GetTableValueOrDefault<T>(LuaTable table, LuaValue key, T defaultValue)
    {
        var tableValue = table[key];
        return tableValue.Type == LuaValueType.Nil ? defaultValue : tableValue.Read<T>();
    }

    public static void VerifyTable(LuaTable table, Dictionary<string, LuaValueType> expectedFields, bool strict = false)
    {
        List<string> foundKeys = [];
        foreach (var (key, value) in expectedFields)
        {
            // keys surrounded by square brackets are optional
            var k = key;
            var optional = key.StartsWith('[') && key.EndsWith(']');
            if (optional)
            {
                k = key[1..^1];
            }

            if (table.ContainsKey(k))
            {
                if (table[k].Type != value)
                {
                    throw new ArgumentException($"Table field '{key}' is of type '{table[k].Type}' but should be of " +
                                                $"type'{value}'.");
                }
            }
            else if (!optional)
            {
                throw new ArgumentException($"Table is missing required field '{key}'.");
            }
            
            foundKeys.Add(key);
        }

        if (strict)
        {
            foreach (var key in expectedFields.Keys)
            {
                if (!foundKeys.Contains(key))
                {
                    throw new ArgumentException($"Table contains unexpected field '{key}'.");
                }
            }
        }
    }
}