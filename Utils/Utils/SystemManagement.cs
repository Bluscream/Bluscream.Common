#if USE_SYSTEMMANAGEMENT
using System;
using System.Management;

public static partial class Utils
{
    #region bool
    public static bool IsDoNotDisturbActiveFocusAssistCim()
    {
        try
        {
            var scope = new System.Management.ManagementScope(@"\\.\root\cimv2\mdm\dmmap");
            var query = new System.Management.ObjectQuery(
                "SELECT QuietHoursState FROM MDM_Policy_Config_QuietHours"
            );
            using (var searcher = new System.Management.ManagementObjectSearcher(scope, query))
            using (var results = searcher.Get())
            {
                foreach (System.Management.ManagementObject obj in results)
                {
                    var stateObj = obj["QuietHoursState"];
                    if (stateObj != null && int.TryParse(stateObj.ToString(), out int state))
                    {
                        return state == (int)FocusAssistState.PRIORITY_ONLY
                            || state == (int)FocusAssistState.ALARMS_ONLY;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Utils] IsDoNotDisturbActiveFocusAssistCim failed: {ex.Message}");
        }
        return false;
    }
    #endregion
}
#endif 