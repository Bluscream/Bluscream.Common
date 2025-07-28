#if USE_SYSTEMTEXTJSON
using System;
using System.Text;
using System.Text.Json;

namespace Bluscream;

public static partial class Extensions
{
    #region string
    public static T JSonToItem<T>(this string jsonString) => JsonSerializer.Deserialize<T>(jsonString);
    #endregion
    #region generic
    public static (bool result, Exception exception) JsonToFile<T>(this T sender, string fileName, bool format = true)
    {
        try
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            File.WriteAllText(fileName, JsonSerializer.Serialize(sender, format ? options : null));
            return (true, null);
        }
        catch (Exception exception)
        {
            return (false, exception);
        }
    }
    #endregion
}
#endif 