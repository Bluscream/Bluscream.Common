#if USE_WINDOWSAPICODEPACK
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.WindowsAPICodePack.Shell;
using Microsoft.WindowsAPICodePack.Shell.PropertySystem;

namespace Bluscream;

public static partial class Extensions
{
    #region FileInfo
    public static string GetVersion(this FileInfo _file)
    {
        var version = "0.0.0.0";
        try
        {
            version = GetVersion(Assembly.LoadFile(_file.FullName));
        }
        catch { }
        version = ShellFile.FromFilePath(_file.FullName).Properties.System.FileVersion.Value;
        if (!string.IsNullOrWhiteSpace(version))
            return version;
        return version;
    }

    public static string GetAuthor(this FileInfo _file)
    {
        string author;
        try
        {
            author = GetAuthor(Assembly.LoadFile(_file.FullName));
            if (!string.IsNullOrWhiteSpace(author))
                return author;
        }
        catch { }
        var val = ShellFile.FromFilePath(_file.FullName).Properties.System.Author.Value;
        if (val != null && val.Length > 0)
        {
            author = string.Join(", ", val);
            if (!string.IsNullOrWhiteSpace(author))
                return author;
        }
        return "Unknown Author";
    }
    #endregion
}
#endif 