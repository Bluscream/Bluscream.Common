
#if USE_CHILLOUTVR
using System;
using System.Collections.Generic;
using ABI_RC.Core.InteractionSystem;
using ABI_RC.Core.Player;
using ABI_RC.Core.Savior;
using ABI_RC.Core.Util;
using Bluscream.MoreChatNotifications;

public static partial class Utils
{
    #region string
    public static string GetPlayerNameById(string playerId)
    {
        return IsLocalPlayer(playerId)
            ? "You"
            : "\"" + CVRPlayerManager.Instance.TryGetPlayerName(playerId) + "\"";
    }
    #endregion

    #region bool
    public static bool IsLocalPlayer(string playerId)
    {
        return playerId == MetaPort.Instance.ownerId;
    }

    public static bool PropsAllowed()
    {
        if (!ModConfig.EnableMod.Value)
            return false;
        if (!CVRSyncHelper.IsConnectedToGameNetwork())
        {
            HUDNotify("Cannot spawn prop", "Not connected to an online Instance");
            return false;
        }
        else if (!MetaPort.Instance.worldAllowProps)
        {
            HUDNotify("Cannot spawn prop", "Props are not allowed in this world");
            return false;
        }
        else if (!MetaPort.Instance.settings.GetSettingsBool("ContentFilterPropsEnabled", false))
        {
            HUDNotify("Cannot spawn prop", "Props are disabled in content filter");
            return false;
        }
        return true;
    }
    #endregion

    #region void
    public static void HUDNotify(
        string header = null,
        string subtext = null,
        string cat = null,
        float? time = null
    )
    {
        if (!ModConfig.EnableMod.Value || !ModConfig.EnableHUDNotifications.Value)
            return;
        cat ??= $"(Local) {MoreChatNotifications.Properties.AssemblyInfoParams.Name}";
        if (time != null)
        {
            ViewManager.Instance.NotifyUser(cat, subtext, time.Value);
        }
        else
        {
            ViewManager.Instance.NotifyUserAlert(cat, header, subtext);
        }
    }

    public static void SendChatNotification(
        object text,
        bool sendSoundNotification = false,
        bool displayInHistory = false
    )
    {
        if (!ModConfig.EnableMod.Value || !ModConfig.EnableChatNotifications.Value)
            return;
        Kafe.ChatBox.API.SendMessage(
            text.ToString(),
            sendSoundNotification: sendSoundNotification,
            displayInChatBox: true,
            displayInHistory: displayInHistory
        );
    }
    #endregion
}
#endif