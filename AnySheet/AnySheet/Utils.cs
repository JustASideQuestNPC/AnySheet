using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using AnySheet.SheetModule.Primitives;

namespace AnySheet;

public static class Utils
{
    public static readonly IEnumerable<MethodInfo> PrimitiveReaders = typeof(ModulePrimitiveLuaBase)
                                  .Assembly.GetTypes()
                                  .Where(t => t.IsSubclassOf(typeof(ModulePrimitiveLuaBase)) &&!t.IsAbstract)
                                  .Select(t => t.GetMethod("TryReadLua", BindingFlags.Static | BindingFlags.Public))!;

    private static readonly string WorkingDirectoryForwardSlash = Environment.CurrentDirectory.Replace('\\', '/');
    public static readonly DirectoryInfo LogDir = Directory.CreateDirectory(Environment.CurrentDirectory + "/logs/");
    
    // i should probably make a utils class or something but whatever
    public static bool PathContainsWorkingDirectory(string path)
    {
        return path.StartsWith(WorkingDirectoryForwardSlash) || path.StartsWith(Environment.CurrentDirectory);
    }
    
    // config stuff
    private const string ConfigFileName = "data.json";
    public static Dictionary<string, List<(string, string)>> ModuleFileTree { get; private set; } = new();
    
    public static async Task SaveConfigFile()
    {
        Console.WriteLine("Saving config file...");
        var moduleTreeJson = new JsonObject();
        foreach (var (folderName, files) in ModuleFileTree)
        {
            var folderJson = new JsonArray();
            foreach (var (fileName, displayName) in files)
            {
                folderJson.Add(new JsonArray(fileName, displayName));
            }
            moduleTreeJson[folderName] = folderJson;
        }
        
        var configJson = new JsonObject {["moduleTree"] = moduleTreeJson};
        var configPath = Path.Combine(Environment.CurrentDirectory, ConfigFileName);
        await File.WriteAllTextAsync(configPath, configJson.ToString());
    }
    
    public static async Task TryLoadConfigFile()
    {
        var configPath = Path.Combine(Environment.CurrentDirectory, ConfigFileName);
        if (File.Exists(configPath))
        {
            // i should really do null checks for everything here, but i'm tired and the config file isn't essential, so
            // i'm just going to catch everything and then pray it was an error with the file and not my code
            var fileContents = await File.ReadAllTextAsync(configPath);
            try
            {
                var configJson = JsonSerializer.Deserialize<JsonObject>(fileContents);
                if (configJson == null)
                {
                    throw new JsonException("Config file is null.");
                }
                
                if (configJson["moduleTree"] != null)
                {
                    var moduleTreeJson = configJson["moduleTree"]!.AsObject();
                    foreach (var item in moduleTreeJson!)
                    {
                        Console.WriteLine($"Loading module tree for {item.Key}");
                        var folderJson = item.Value?.AsArray();
                        List<(string, string)> files = [];
                        foreach (var fileJson in folderJson)
                        {
                            var file = fileJson?.AsArray()!;
                            if (file[0]!.AsValue().TryGetValue(out string path) &&
                                file[1]!.AsValue().TryGetValue(out string displayName))
                            {
                                files.Add((path, displayName));
                            }
                        }
                        ModuleFileTree[item.Key] = files;
                    }
                }
                else
                {
                    Console.WriteLine("Module tree does not exist, skipping");
                }
            }
            catch (JsonException e)
            {
                Console.WriteLine("Error parsing config file: " + e.Message);
            }
        }
        else
        {
            Console.WriteLine("No config file found; file will be created at next app exit or autosave.");
        }
        
        Console.WriteLine("Verifying module tree...");
        VerifyModuleTree();
    }

    public static void VerifyModuleTree()
    {
        Console.WriteLine(ModuleFileTree.Count == 0 ? "Module tree is empty; rebuilding..." : "Module tree is valid.");
        
        // check for deleted files/folders
        foreach (var (folderName, fileData) in ModuleFileTree)
        {
            Console.WriteLine($"Checking folder {folderName} for deleted files");
            var folderPath = Path.Combine(Environment.CurrentDirectory + @"\Modules\", folderName);
            if (!Directory.Exists(folderPath))
            {
                RebuildModuleTree();
                return;
            }
            
            foreach (var (fileName, _) in fileData)
            {
                var filePath = Path.Combine(folderPath, fileName);
                if (!File.Exists(filePath))
                {
                    RebuildModuleTree();
                    return;
                }
            }
        }
        
        // check for added files
        var moduleDir = new DirectoryInfo(Environment.CurrentDirectory + @"\Modules");
        foreach (var dir in moduleDir.EnumerateDirectories())
        {
            var dirName = Path.GetRelativePath(Environment.CurrentDirectory + @"\Modules\", dir.FullName);
            Console.WriteLine($"Checking folder {dirName} for added files");
            foreach (var file in dir.EnumerateFiles())
            {
                if (file.Extension != ".lua" || file.Name.EndsWith(".d.lua"))
                {
                    continue;
                }
                if (ModuleFileTree[dirName].All(f => f.Item1 != Path.GetFileName(file.FullName)))
                {
                    RebuildModuleTree();
                    return;
                }
            }
        }
    }

    public static void RebuildModuleTree()
    {
        Console.WriteLine("Module tree is invalid; rebuilding...");
        ModuleFileTree.Clear();
        var moduleDir = new DirectoryInfo(Environment.CurrentDirectory + @"\Modules");
        foreach (var dir in moduleDir.EnumerateDirectories())
        {
            List<(string, string)> files = [];
            foreach (var file in dir.EnumerateFiles())
            {
                // .d.lua files are type definitions for code editors
                if (file.Extension == ".lua" && !file.Name.EndsWith(".d.lua"))
                {
                    files.Add((file.Name, file.Name.Replace(".lua", "")));
                }
            }
            if (files.Count > 0)
            {
                ModuleFileTree[dir.Name] = files;
            }
        }
    }
}