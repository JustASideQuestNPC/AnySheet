using System.Collections.Generic;
using Avalonia.Media;

namespace AnySheet;

public class AppResources
{
    public static Dictionary<string, FontFamily> ModuleFonts { get; } = new()
    {
        ["normal"] = new FontFamily("avares://AnySheet/Assets/Fonts/PTSerif"),
        ["bold"] = new FontFamily("avares://AnySheet/Assets/Fonts/PTSerif-Bold"),
        ["italic"] = new FontFamily("avares://AnySheet/Assets/Fonts/PTSerif-Italic"),
        ["bold italic"] = new FontFamily("avares://AnySheet/Assets/Fonts/PTSerif-BoldItalic"),
    };
}