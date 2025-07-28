#if USE_WINDOWS
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Threading;
using Microsoft.Win32;

namespace Bluscream;

public static partial class Utils
{
    public enum ShowWindowCommand
    {
        SW_HIDE = 0,
        SW_SHOW = 5,
    }

    public enum FocusAssistState
    {
        OFF = 0,
        PRIORITY_ONLY = 1,
        ALARMS_ONLY = 2,
    }

    #region DllImports
    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool AllocConsole();

    [DllImport("User32.dll")]
    public static extern Int32 SetForegroundWindow(int hWnd);

    [DllImport("kernel32.dll")]
    public static extern IntPtr GetConsoleWindow();

    [DllImport("user32.dll")]
    public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImport("user32.dll")]
    public static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

    [DllImport("user32.dll")]
    public static extern bool GetFocusAssistState(out int state);
    #endregion

    #region void
    public static void RestartAsAdmin(string[] arguments)
    {
        if (IsRunAsAdmin())
            return;
        ProcessStartInfo proc = new ProcessStartInfo();
        proc.UseShellExecute = true;
        proc.WorkingDirectory = Environment.CurrentDirectory;
        proc.FileName = Assembly.GetEntryAssembly().CodeBase;
        proc.Arguments += arguments.ToString();
        proc.Verb = "runas";
        try
        {
            Process.Start(proc);
            TryExitApplication();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unable to restart as admin automatically: {ex.Message}");
            Console.WriteLine(
                "This app has to run with elevated permissions (Administrator) to be able to modify files in the Overwolf folder!"
            );
            Console.ReadKey();
            TryExitApplication();
        }
    }

    public static void CreateConsole()
    {
        AllocConsole();
    }

    public static void SetConsoleTitle(string title)
    {
        Console.Title = title;
    }

    public static void ShowFileInExplorer(FileInfo file)
    {
        StartProcess("explorer.exe", null, "/select, " + file.FullName.Quote());
    }

    public static void OpenFolderInExplorer(DirectoryInfo dir)
    {
        StartProcess("explorer.exe", null, dir.FullName.Quote());
    }

    public static void HideConsoleWindow()
    {
        try
        {
            var handle = GetConsoleWindow();
            if (handle != IntPtr.Zero)
            {
                ShowWindow(handle, (int)ShowWindowCommand.SW_HIDE);
            }
            var process = Process.GetCurrentProcess();
            if (process != null && process.MainWindowHandle != IntPtr.Zero)
            {
                ShowWindow(process.MainWindowHandle, (int)ShowWindowCommand.SW_HIDE);
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error: {ex}");
        }
    }
    #endregion

    #region bool
    public static bool IsRunAsAdmin()
    {
        using (var identity = WindowsIdentity.GetCurrent())
        {
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
    }

    public static bool IsAlreadyRunning(string appName)
    {
        System.Threading.Mutex m = new System.Threading.Mutex(false, appName);
        if (m.WaitOne(1, false) == false)
        {
            return true;
        }
        return false;
    }

    public static bool IsDoNotDisturbActiveRegistry()
    {
        try
        {
            using var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(
                @"Software\Microsoft\Windows\CurrentVersion\Notifications\Settings"
            );
            if (key?.GetValue("NOC_GLOBAL_SETTING_TOASTS_ENABLED") is object value)
            {
                return value.ToString() == "0"; // Toasts disabled = Do Not Disturb
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Utils] IsDoNotDisturbActiveRegistry failed: {ex.Message}");
        }
        return false;
    }

    public static bool IsDoNotDisturbActiveFocusAssist()
    {
        try
        {
            if (GetFocusAssistState(out int state))
            {
                return state == (int)FocusAssistState.PRIORITY_ONLY
                    || state == (int)FocusAssistState.ALARMS_ONLY;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Utils] IsDoNotDisturbActiveFocusAssist failed: {ex.Message}");
        }
        return false;
    }

    public static bool IsDoNotDisturbActive()
    {
        return IsDoNotDisturbActiveRegistry()
            || IsDoNotDisturbActiveFocusAssist()
            || IsDoNotDisturbActiveFocusAssistCim();
    }
    #endregion
}
#endif