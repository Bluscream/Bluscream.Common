#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;

namespace Bluscream;

public static partial class Utils
{
    #region DirectoryInfo
    public static DirectoryInfo GetOwnDir() {
        var ownPath = GetOwnPath();
        if (ownPath != null && ownPath.Exists && ownPath.Directory != null)
            return ownPath.Directory;
        var path = ownPath?.FullName ?? "";
        if (!string.IsNullOrEmpty(path))
        {
            var dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir))
                return new DirectoryInfo(dir);
        }
        var baseDir = AppContext.BaseDirectory;
        if (!string.IsNullOrEmpty(baseDir) && Directory.Exists(baseDir))
            return new DirectoryInfo(baseDir);
        var entryLoc = Assembly.GetEntryAssembly()?.Location;
        if (!string.IsNullOrEmpty(entryLoc))
        {
            var dir = Path.GetDirectoryName(entryLoc);
            if (!string.IsNullOrEmpty(dir))
                return new DirectoryInfo(dir);
        }
        return new DirectoryInfo(Environment.CurrentDirectory);
    }
    #endregion

    #region List<int>
    public static List<int> GetPadding(
        string input,
        int minWidth = 80,
        int padding = 10
    )
    {
        int totalWidth = minWidth + padding * 2;
        int leftPadding = (totalWidth - input.Length) / 2;
        int rightPadding = totalWidth - input.Length - leftPadding;
        return new List<int> { leftPadding, rightPadding, totalWidth };
    }
    #endregion

    #region string
    public static string Pad(
        string input,
        string outer = "||",
        int minWidth = 80,
        int padding = 10
    )
    {
        var padded = GetPadding(input, minWidth, padding);
        return $"{outer}{new string(' ', padded[index: 0])}{input}{new string(' ', padded[1])}{outer}";
    }

    public static void Log(string message, FileInfo? logFile = null, params object[] args)
    {
        var formattedMessage = string.Format(message, args);
        var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        var logMessage = $"[{timestamp}] {formattedMessage}";
        Console.WriteLine(logMessage);
        
        if (logFile?.Exists ?? false)
        {
            try { logFile.AppendAllText(logMessage + Environment.NewLine); }catch (Exception) {}
        }
    }

    public static string Fill(char c, int width = 80, int padding = 10)
    {
        return new string(c, width + padding * 2 + 4);
    }

    public static string GetOwnName()
    {
        var exePath = GetOwnPath();
        var name = Path.GetFileNameWithoutExtension(exePath.FullName);
        if (!string.IsNullOrEmpty(name)) return name;
        var entryAsm = Assembly.GetEntryAssembly();
        if (entryAsm != null && !string.IsNullOrEmpty(entryAsm.GetName().Name))
            return entryAsm.GetName().Name!;
        var procName = Process.GetCurrentProcess().ProcessName;
        if (!string.IsNullOrEmpty(procName)) return procName;
        var cmd = Environment.GetCommandLineArgs().FirstOrDefault();
        if (!string.IsNullOrEmpty(cmd)) return Path.GetFileNameWithoutExtension(cmd);
        return string.Empty;
    }

    public static Dictionary<string, string> ParseQueryString(string query)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        
        if (string.IsNullOrWhiteSpace(query))
            return result;

        var pairs = query.Split('&', StringSplitOptions.RemoveEmptyEntries);
        foreach (var pair in pairs)
        {
            var keyValue = pair.Split('=', 2);
            if (keyValue.Length == 2)
            {
                var key = Uri.UnescapeDataString(keyValue[0]);
                var value = Uri.UnescapeDataString(keyValue[1]);
                result[key] = value;
            }
            else if (keyValue.Length == 1)
            {
                var key = Uri.UnescapeDataString(keyValue[0]);
                result[key] = string.Empty;
            }
        }
        
        return result;
    }
    
    public static string Base64Encode(string plainText)
    {
        var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
        return Convert.ToBase64String(plainTextBytes);
    }

    public static string Base64Decode(string base64EncodedData)
    {
        var base64EncodedBytes = Convert.FromBase64String(base64EncodedData);
        return Encoding.UTF8.GetString(base64EncodedBytes);
    }

    public static string JoinArgs(params IEnumerable<IEnumerable<string>> args) =>
        string.Join(" ", JoinListsUnique(args));
    #endregion

    #region List<string>
    public static List<string> removeFromToRow(
        string from,
        string where,
        string to,
        string insert = ""
    )
    {
        List<string> list;
        if (where.Contains("\r\n"))
            list = where.Split(new[] { "\r\n" }, StringSplitOptions.None).ToList();
        else
            list = where.Split(new[] { "\n" }, StringSplitOptions.None).ToList();
        return removeFromToRow(from, list, to, insert);
    }

    public static List<string> removeFromToRow(
        string from,
        List<string> where,
        string to,
        string insert = ""
    )
    {
        int start = -1;
        int end = -1;
        for (int i = 0; i < where.Count; i++)
        {
            if (where[i] == from)
            {
                start = i;
            }
            if (start != -1 && where[i] == to)
            {
                end = i;
                break;
            }
        }
        if (start != -1 && end != -1)
        {
            where.RemoveRange(start, end - start + 1);
        }
        if (insert != "")
        {
            where.Insert(start, insert);
        }
        return where;
    }
    #endregion

    #region void

    public static void ErrorAndExit(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(message);
        Console.ResetColor();
        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();

        Exit(1);
    }

    public static void Exit(int exitCode = 0)
    {
#if USE_SYSTEMWINDOWSFORMS
        try
        {
            System.Windows.Forms.Application.Exit();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"System.Windows.Forms.Application.Exit() failed: {ex}");
        }
#endif
        try
        {
            Environment.Exit(exitCode);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Environment.Exit() failed: {ex}");
        }
        try
        {
            var process = Process.GetCurrentProcess();
            process.Kill();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Process.Kill() failed: {ex}");
        }
    }

    public static void RunAdditionalApps(List<List<string>> apps)
    {
        if (apps == null || apps.Count == 0)
            return;
        foreach (var app in apps)
        {
            if (app == null || app.Count == 0 || string.IsNullOrWhiteSpace(app[0]))
                continue;
            var binary = app[0];
            var args =
                app.Count > 1
                    ? string.Join(" ", app.Skip(1).Select(a => a.Contains(' ') ? $"\"{a}\"" : a))
                    : string.Empty;
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = binary,
                    Arguments = args,
                    UseShellExecute = true,
                    CreateNoWindow = true,
                };
                Process.Start(psi);
                Console.WriteLine($"Launched additional app: {binary} {args}");
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"Failed to launch additional app: {binary} {args}\n{ex.Message}"
                );
            }
        }
    }
    #endregion

    #region bool
    public static bool IsVirtualDesktopConnected() =>
        Process.GetProcessesByName("VirtualDesktop.Server").Any();

    public static bool IsSteamVRRunning() =>
        Process.GetProcessesByName("vrmonitor").Any()
        && Process.GetProcessesByName("vrcompositor").Any();

    public static bool IsVrchatRunning() => Process.GetProcessesByName("VRChat").Any();
    #endregion

    #region FileInfo
    public static FileInfo GetOwnPath()
    {
        var possiblePaths = new List<string?>
        {
            Process.GetCurrentProcess().MainModule?.FileName,
            AppContext.BaseDirectory,
            Environment.GetCommandLineArgs().FirstOrDefault(),
            Assembly.GetEntryAssembly()?.Location,
            ".",
        };
        foreach (var path in possiblePaths.Where(p => !string.IsNullOrEmpty(p)))
        {
            if (System.IO.File.Exists(path!))
            {
                return new FileInfo(System.IO.Path.GetFullPath(path!));
            }
        }
        return new FileInfo(string.Empty);
    }

    public static FileInfo DownloadFile(
        string url,
        DirectoryInfo destinationPath,
        string? fileName = null
    )
    {
        if (fileName == null)
            fileName = url.Split('/').Last() ?? "download";
        // Main.webClient.DownloadFile(url, Path.Combine(destinationPath.FullName, fileName));
        return new FileInfo(Path.Combine(destinationPath.FullName, fileName));
    }
    #endregion

    #region Process
    public static Process StartProcess(FileInfo file, params string[] args) =>
        StartProcess(file.FullName, file.DirectoryName, args);

    public static Process StartProcess(string file, string? workDir = null, params string[] args)
    {
        ProcessStartInfo proc = new ProcessStartInfo();
        proc.FileName = file;
        proc.Arguments = string.Join(" ", args);
#if USE_NLOG
        Logger.Debug("Starting Process: {0} {1}", proc.FileName, proc.Arguments);
        if (workDir != null)
        {
            proc.WorkingDirectory = workDir;
            Logger.Debug("Working Directory: {0}", proc.WorkingDirectory);
        }
#endif
        return Process.Start(proc);
    }
    #endregion

    #region IPEndPoint
    public static IPEndPoint? ParseIPEndPoint(string endPoint)
    {
        string[] ep = endPoint.Split(':');
        if (ep.Length < 2)
            return null;
        IPAddress ip;
        if (ep.Length > 2)
        {
            if (!IPAddress.TryParse(string.Join(":", ep, 0, ep.Length - 1), out ip))
            {
                return null;
            }
        }
        else
        {
            if (!IPAddress.TryParse(ep[0], out ip))
            {
                return null;
            }
        }
        int port;
        if (
            !int.TryParse(
                ep[ep.Length - 1],
                NumberStyles.None,
                NumberFormatInfo.CurrentInfo,
                out port
            )
        )
        {
            return null;
        }
        return new IPEndPoint(ip, port);
    }
    #endregion

    #region IEnumerable<string>
    public static IEnumerable<string> JoinListsUnique(
        params IEnumerable<IEnumerable<string>> arglists
    )
    {
        var unique = new HashSet<string>();
        foreach (var args in arglists)
        {
            if (args is null)
                continue;
            foreach (var a in args)
            {
                if (!string.IsNullOrWhiteSpace(a))
                {
                    unique.Add(a.Trim());
                }
            }
        }
        return unique;
    }
    #endregion
}
