using Avalonia;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace AnySheet;

sealed class Program
{
    private static string _crashHandlerPath = "CrashHandler.exe";
    private static bool _noCrashHandler = false;
    
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        // command line arguments are used for the crash handler and for file association
        var targetFile = "";
        if (args.Length > 0)
        {
            for (var i = 0; i < args.Length; ++i)
            {
                switch (args[i])
                {
                    case "--noCrashHandler":
                        if (_noCrashHandler)
                        {
                            // todo: figure out what exception class to throw here
                            throw new Exception("Repeat command line argument \"--noCrashHandler\".");
                        }
                        _noCrashHandler = true;
                        break;
                    case "--crashHandler":
                    {
                        if (_crashHandlerPath != "CrashHandler.exe")
                        {
                            throw new Exception("Repeat command line argument \"--crashHandler\".");
                        }
                        ++i;
                        if (i == args.Length)
                        {
                            throw new Exception("Path to crash handler is required.");
                        }
                        if (!Path.Exists(Path.GetFullPath(args[i])))
                        {
                            throw new FileNotFoundException($"Crash handler does not exist at {args[i]}.");
                        }
                        _crashHandlerPath = args[i];
                        break;
                    }
                    case "-f":
                        if (targetFile != "")
                        {
                            throw new Exception("Repeat command line argument \"-f\".");
                        }++i;
                        if (i == args.Length)
                        {
                            throw new Exception("Path to file is required.");
                        }
                        // if (!Path.Exists(Path.GetFullPath(args[i])))
                        // {
                        //     throw new FileNotFoundException($"Sheet file does not exist at {args[i]}.");
                        // }
                        targetFile = args[i];
                        break;
                }
            }
        }
        Console.WriteLine($"crash handler at {Path.GetFullPath(_crashHandlerPath)}");
        
        // apparently, running the app by double-clicking a file will make the working directory the same place as that
        // file, not the location of the exe
        if (!Environment.CurrentDirectory.EndsWith("AnySheet"))
        {
            Environment.CurrentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ??
                                           Environment.CurrentDirectory;
        }
        // Environment.CurrentDirectory = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule?.FileName) ?? Environment.CurrentDirectory;

        try
        {
            BuildAvaloniaApp().StartWithClassicDesktopLifetime([targetFile]);
        }
        catch (Exception e)
        {
            if (_noCrashHandler)
            {
                throw;
            }
            
            OpenCrashHandler(e);
        }
    }
    
    private static void OpenCrashHandler(Exception e)
    {
        Console.WriteLine($"crashed: {e}");
        var startInfo = new ProcessStartInfo
        {
            FileName = Path.GetFullPath(_crashHandlerPath),
            Arguments = $"{Directory.CreateDirectory(Environment.CurrentDirectory + "/logs/")} {e}",
            WindowStyle = ProcessWindowStyle.Normal,
            UseShellExecute = true
        };
        Process.Start(startInfo);
        Environment.Exit(-1);
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
                     .UsePlatformDetect()
                     .WithInterFont()
                     .LogToTrace();
}