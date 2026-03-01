using Avalonia;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;

namespace CustomControls;
using Avalonia.Controls;

public static class TextFitHelper
{
    // this is public because i can't get an AutoFitTextBox custom control to work, so i'm hacking around it by just
    // calling this manually in the few places where i need to
    public static double FindBestFontSize(string text, FontFamily font, double maxWidth, double maxHeight,
        double minSize = 4, double maxSize = 200)
    {
        var low = minSize;
        var high = maxSize;
        var best = minSize;
        var typeface = new Typeface(font);
        var ts = TextShaper.Current;
        
        var loopTimeout = 1000;
        while (low <= high && loopTimeout-- > 0)
        {
           var mid = (low + high) / 2;
           var shaped = ts.ShapeText(text, new TextShaperOptions(typeface.GlyphTypeface, mid));
           var run = new ShapedTextRun(shaped, new GenericTextRunProperties(typeface, mid));
           
           if (run.Size.Width <= maxWidth && run.GlyphRun.Bounds.Height <= maxHeight)
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
                                                  finalSize.Height - Padding.Top - Padding.Bottom);
        return base.ArrangeOverride(finalSize);
    }
}