#if USE_SYSTEMMANAGEMENT
using System;
using System.Diagnostics;
using System.Linq;
using System.Management;

namespace Bluscream;

public static partial class Extensions
{
    #region Process
    public static string GetCommandLine(this Process process)
    {
        if (process == null)
        {
            throw new ArgumentNullException(nameof(process));
        }
        try
        {
            using (
                var searcher = new ManagementObjectSearcher(
                    "SELECT CommandLine FROM Win32_Process WHERE ProcessId = " + process.Id
                )
            )
            using (var objects = searcher.Get())
            {
                var result = objects.Cast<ManagementBaseObject>().SingleOrDefault();
                return result?["CommandLine"]?.ToString() ?? string.Empty;
            }
        }
        catch
        {
            return string.Empty;
        }
    }
    #endregion
}
#endif 