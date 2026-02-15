using Avalonia;
using System;
using System.Diagnostics;
using System.IO;

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
        // lets me test the crash handler without having to make sure the exe is always in the same folder
        if (args.Length > 0)
        {
            _crashHandlerPath = args[0];
            if (_crashHandlerPath == "--nocrashhandler")
            {
                _noCrashHandler = true;
            }
            else if (!Path.Exists(Path.GetFullPath(_crashHandlerPath)))
            {
                throw new FileNotFoundException("Go fix your crash handler path.");
            }
        }
        Console.WriteLine($"crash handler at {Path.GetFullPath(_crashHandlerPath)}");

        try
        {
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
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