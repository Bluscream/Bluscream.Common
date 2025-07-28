#if USE_NLOG

public static partial class Utils
{
    public static void Debug(object message, params object[] parms)
    {
        if (!PluginConfig.EnableLogging.Value)
            return;
        Logger.Debug(message.ToString(), parms);
    }

    public static void Log(object message, params object[] parms)
    {
        if (!PluginConfig.EnableLogging.Value)
            return;
        Logger.Msg(message.ToString(), parms);
    }

    public static void Error(object message, params object[] parms)
    {
        if (!PluginConfig.EnableLogging.Value)
            return;
        Logger.Error(message.ToString(), parms);
    }

    public static void BigError(object message)
    {
        if (!PluginConfig.EnableLogging.Value)
            return;
        Logger.BigError(message.ToString());
    }

    public static void Warn(object message, params object[] parms) => Warning(message, parms);
    public static void Warning(object message, params object[] parms)
    {
        if (!PluginConfig.EnableLogging.Value)
            return;
        Logger.Warning(message.ToString(), parms);
    }
}
#endif