using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Media;

namespace AnySheet;

public class AppResources
{
    public static Dictionary<string, string> ModuleFonts { get; } = new()
    {
        ["normal"] = "PtSerif",
        ["bold"] = "PtSerifBold",
        ["italic"] = "PtSerifItalic",
        ["bold italic"] = "PtSerifBoldItalic"
    };

    public static T GetResource<T>(string resourceName) where T : class
    {
        return Application.Current?.Resources[resourceName] as T ?? throw new InvalidOperationException();
    }
}