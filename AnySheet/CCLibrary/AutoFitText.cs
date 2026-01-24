using Avalonia;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;

namespace CCLibrary;
using Avalonia.Controls;

internal static class TextFitHelper
{
    public static double FindBestFontSize(string text, FontFamily font, double maxWidth, double maxHeight,
        TextAlignment alignment, double lineHeight, double minSize = 4, double maxSize = 200)
    {
        var low = minSize;
        var high = maxSize;
        var best = minSize;
        var typeface = new Typeface(font);

        var loopTimeout = 1000;
        while (low <= high && loopTimeout-- > 0)
        {
           var mid = (low + high) / 2;
           var layout = new TextLayout(
               text,
               typeface,
               mid,
               Brushes.Black, // useless but required because i need to fill out a bunch of default parameters
               alignment,
               TextWrapping.Wrap,
               TextTrimming.None, // also required
               [], // also required
               FlowDirection.LeftToRight, // also required
               maxWidth,
               double.PositiveInfinity,
               lineHeight);

           if (layout.Width <= maxWidth && layout.Height <= maxHeight)
           {
               best = mid;
               low = mid + 0.5;
           }
           else
           {
               high = mid - 0.5;
           }
        }

        return best;
    }
}

public class AutoFitTextBlock : TextBlock
{
    protected override Size ArrangeOverride(Size finalSize)
    {
        FontSize = TextFitHelper.FindBestFontSize(Text ?? "", FontFamily,
                                                  finalSize.Width - Padding.Left - Padding.Right,
                                                  finalSize.Height - Padding.Top - Padding.Bottom, TextAlignment,
                                                  LineHeight);
        return base.ArrangeOverride(finalSize);
    }
}