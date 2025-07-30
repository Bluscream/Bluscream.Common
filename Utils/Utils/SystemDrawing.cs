#if USE_SYSTEMDRAWING
using System;
using System.Collections.Generic;
using System.Drawing;

namespace Bluscream;

public static partial class Utils
{
    #region Color
    public static Color GetColor(List<ushort> _c) => Color.FromArgb(_c[0], _c[1], _c[2], _c[3]);
    #endregion

    #region (Color, double)
    public static (Color, double) ParseColorAndOpacity(
        string? colorString,
        Color defaultColor,
        double defaultOpacity
    )
    {
        if (!string.IsNullOrWhiteSpace(colorString))
        {
            try
            {
                var colorStr = colorString.TrimStart('#');
                if (colorStr.Length == 8)
                {
                    var a = Convert.ToByte(colorStr.Substring(0, 2), 16);
                    var r = Convert.ToByte(colorStr.Substring(2, 2), 16);
                    var g = Convert.ToByte(colorStr.Substring(4, 2), 16);
                    var b = Convert.ToByte(colorStr.Substring(6, 2), 16);
                    return (Color.FromArgb(r, g, b), a / 255.0);
                }
                else if (colorStr.Length == 6)
                {
                    var r = Convert.ToByte(colorStr.Substring(0, 2), 16);
                    var g = Convert.ToByte(colorStr.Substring(2, 2), 16);
                    var b = Convert.ToByte(colorStr.Substring(4, 2), 16);
                    return (Color.FromArgb(r, g, b), defaultOpacity);
                }
            }
            catch { }
        }
        return (defaultColor, defaultOpacity);
    }
    #endregion

    #region Bitmap
    public static Bitmap CreateDefaultIcon()
    {
        var bitmap = new Bitmap(32, 32);
        using (var g = Graphics.FromImage(bitmap))
        {
            g.FillRectangle(new SolidBrush(Color.FromArgb(30, 30, 30)), 0, 0, 32, 32);
            using (var pen = new Pen(Color.Orange, 2))
            {
                var points = new[]
                {
                    new System.Drawing.Point(10, 8),
                    new System.Drawing.Point(22, 16),
                    new System.Drawing.Point(10, 24),
                };
                g.DrawLines(pen, points);
            }
        }
        return bitmap;
    }
    #endregion
}
#endif 