#if USE_WINDOWSAPICODEPACK
using System;
using System.IO;
using Microsoft.WindowsAPICodePack.Dialogs;

public static partial class Utils
{
    #region DirectoryInfo
    public static DirectoryInfo PickFolder(string title = null, string initialDirectory = null)
    {
        Microsoft.WindowsAPICodePack.Dialogs.CommonOpenFileDialog dialog =
            new Microsoft.WindowsAPICodePack.Dialogs.CommonOpenFileDialog();
        if (title != null)
            dialog.Title = title;
        dialog.IsFolderPicker = true;
        dialog.DefaultDirectory = initialDirectory ?? "::{20D04FE0-3AEA-1069-A2D8-08002B30309D}";
        if (dialog.ShowDialog() == Microsoft.WindowsAPICodePack.Dialogs.CommonFileDialogResult.Ok)
        {
            var dir = new DirectoryInfo(dialog.FileName);
            if (dir.Exists)
                return dir;
        }
        return null;
    }
    #endregion
}
#endif 