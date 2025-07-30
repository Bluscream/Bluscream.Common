#if USE_SYSTEMWINDOWSFORMS
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace Bluscream;

public static partial class Utils
{
    #region FileInfo
    public static FileInfo? PickFile(
        string? title = null,
        string? initialDirectory = null,
        string? filter = null
    )
    {
        using (var fileDialog = new OpenFileDialog())
        {
            if (title != null)
                fileDialog.Title = title;
            fileDialog.InitialDirectory =
                initialDirectory ?? "::{20D04FE0-3AEA-1069-A2D8-08002B30309D}";
            if (filter != null)
                fileDialog.Filter = filter;
            fileDialog.Multiselect = false;
            var result = fileDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                var file = new FileInfo(fileDialog.FileName);
                if (file.Exists)
                    return file;
            }
            return null;
        }
    }

    public static FileInfo? SaveFile(
        string? title = null,
        string? initialDirectory = null,
        string? filter = null,
        string? fileName = null,
        string? content = null
    )
    {
        using (var fileDialog = new SaveFileDialog())
        {
            if (title != null)
                fileDialog.Title = title;
            fileDialog.InitialDirectory =
                initialDirectory ?? "::{20D04FE0-3AEA-1069-A2D8-08002B30309D}";
            if (filter != null)
                fileDialog.Filter = filter;
            fileDialog.FileName = fileName ?? null;
            var result = fileDialog.ShowDialog();
            if (result != DialogResult.OK || fileDialog.FileName.IsNullOrWhiteSpace())
                return null;
            if (content != null)
            {
                using (var fileStream = fileDialog.OpenFile())
                {
                    byte[] info = new UTF8Encoding(true).GetBytes(content);
                    fileStream.Write(info, 0, info.Length);
                }
            }
            return new FileInfo(fileDialog.FileName);
        }
    }
    #endregion

    #region void
    public static void BringSelfToFront()
    {
        // This method requires a specific Form instance
        // For now, we'll provide a placeholder implementation
        // The actual implementation should be provided by the consuming application
    }
    #endregion

}
#endif 