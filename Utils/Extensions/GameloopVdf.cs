#if USE_VDF
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Gameloop.Vdf;
using Gameloop.Vdf.JsonConverter;
using Gameloop.Vdf.Linq;
using Newtonsoft.Json.Linq;

namespace Bluscream;

public static partial class Extensions
{
    #region BindingList<Mount>
    public static string toVdf(BindingList<Mount> Mounts)
    {
        var mountcfg = new Dictionary<string, string>();
        foreach (var mount in Mounts)
        {
            mountcfg.Add(mount.Name, mount.Path.FullName);
        }
        return VdfConvert.Serialize(
            new VProperty("mountcfg", JToken.FromObject(mountcfg).ToVdf()),
            new VdfSerializerSettings { UsesEscapeSequences = false }
        );
    }
    #endregion
}
#endif 