using Avalonia;
using System;
using System.Collections.Generic;

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

    // locates an avalonia resource if it exists
    public static T GetResource<T>(string resourceName) where T : class
    {
        return Application.Current?.Resources[resourceName] as T ?? throw new InvalidOperationException();
    }
    
    // sets a resource if it exists
    public static void SetResource<T>(string resourceName, T value) where T : class
    {
        if (Application.Current?.Resources[resourceName] != null)
        {
            Application.Current.Resources[resourceName] = value;
        }
        throw new InvalidOperationException();
    }
}