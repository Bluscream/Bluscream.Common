#if USE_WINDOWS
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Web;

namespace Bluscream;

public static partial class Extensions
{
    #region DirectoryInfo
    public static void ShowInExplorer(this DirectoryInfo dir)
    {
        Utils.StartProcess("explorer.exe", null, dir.FullName.Quote());
    }
    #endregion

    #region string
    public static string RemoveInvalidFileNameChars(this string filename)
    {
        return string.Concat(filename.Split(Path.GetInvalidFileNameChars()));
    }

    public static string ReplaceInvalidFileNameChars(this string filename)
    {
        return string.Join("_", filename.Split(Path.GetInvalidFileNameChars()));
    }
    #endregion

    #region Uri
    public static void OpenInDefaultBrowser(this Uri uri)
    {
        Process.Start(new ProcessStartInfo { FileName = uri.AbsoluteUri, UseShellExecute = true });
    }

    public static void OpenIn(this Uri uri, string browser)
    {
        var url = uri.ToString();
        try
        {
            Process.Start(browser, url);
        }
        catch
        {
            url = url.Replace("&", "^&");
            Process.Start(new ProcessStartInfo("cmd", $"/c start \"{browser}\" {url}") { CreateNoWindow = true });
        }
    }

    public static void OpenInDefault(this Uri uri)
    {
        var url = uri.ToString();
        try
        {
            Process.Start(url);
        }
        catch
        {
            url = url.Replace("&", "^&");
            Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
        }
    }
    #endregion

    #region Environment.SpecialFolder
    public static DirectoryInfo Combine(this Environment.SpecialFolder specialFolder, params string[] paths) =>
        Combine(new DirectoryInfo(Environment.GetFolderPath(specialFolder)), paths);

    public static FileInfo CombineFile(this Environment.SpecialFolder specialFolder, params string[] paths) =>
        CombineFile(new DirectoryInfo(Environment.GetFolderPath(specialFolder)), paths);
    #endregion

    #region Assembly
    public static string GetVersion(this Assembly assembly)
    {
        string version = "0.0.0.0";
        version = assembly.GetName().Version.ToString();
        if (!string.IsNullOrWhiteSpace(version))
            return version;
        var versionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
        version = versionInfo.FileVersion;
        if (!string.IsNullOrWhiteSpace(version))
            return version;
        version = versionInfo.ProductVersion;
        if (!string.IsNullOrWhiteSpace(version))
            return version;
        return version;
    }

    public static string GetAuthor(this Assembly assembly)
    {
        var author = FileVersionInfo.GetVersionInfo(assembly.Location).CompanyName;
        if (!string.IsNullOrWhiteSpace(author))
            return author;
        object[] attribs = assembly.GetCustomAttributes(typeof(AssemblyCompanyAttribute), true);
        if (attribs.Length > 0)
            return ((AssemblyCompanyAttribute)attribs[0]).Company;
        attribs = assembly.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), true);
        if (attribs.Length > 0)
            return ((AssemblyCopyrightAttribute)attribs[0]).Copyright;
        attribs = assembly.GetCustomAttributes(typeof(AssemblyTrademarkAttribute), true);
        if (attribs.Length > 0)
            return ((AssemblyTrademarkAttribute)attribs[0]).Trademark;
        return "Unknown Author";
    }
    #endregion
}
#endif 