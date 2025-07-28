#if USE_SYSTEMDRAWING
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Bluscream;

public static partial class Extensions
{
    private const string Base64Prefix = "data:image/";
    #region Image
    public static Bitmap Resize(this Image image, int width, int height)
    {
        var destRect = new Rectangle(0, 0, width, height);
        var destImage = new Bitmap(width, height);
        destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);
        using (var graphics = Graphics.FromImage(destImage))
        {
            graphics.CompositingMode = CompositingMode.SourceCopy;
            graphics.CompositingQuality = CompositingQuality.HighQuality;
            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            graphics.SmoothingMode = SmoothingMode.HighQuality;
            graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
            using (var wrapMode = new ImageAttributes())
            {
                wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
            }
        }
        return destImage;
    }

    public static Image Resize(this Image imgToResize, Size size) => new Bitmap(imgToResize, size) as Image;
    #endregion

    #region Color
    public static Color Invert(this Color input)
    {
        return Color.FromArgb(input.ToArgb() ^ 0xffffff);
    }
    #endregion

    #region string
    public static Image ImageFromBase64(this string base64String)
    {
        base64String = base64String.Split(";base64,").Last();
        var converted = Convert.FromBase64String(base64String);
        using (MemoryStream ms = new MemoryStream(converted))
            return Image.FromStream(ms);
    }

    public static Image? ParseImage(this string input)
    {
        if (input.StartsWith(Base64Prefix, StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                return ImageFromBase64(input);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }
        if (
            Uri.TryCreate(input, UriKind.Absolute, out var uri)
            && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)
        )
        {
            try
            {
                return GetImageAsync(uri).Result;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }
        return null;
    }
    #endregion

    #region Uri
    public static Task<Image> GetImageAsync(this Uri uri)
    {
        using (var httpClient = new HttpClient())
        {
            var byteArray = httpClient.GetByteArrayAsync(uri).Result;
            return Task.FromResult(Image.FromStream(new MemoryStream(byteArray)));
        }
    }
    #endregion
}
#endif 