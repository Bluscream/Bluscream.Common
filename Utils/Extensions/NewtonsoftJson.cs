#if USE_NEWTONSOFTJSON
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace Bluscream;

public static partial class Extensions
{
    #region object
    public static string ToJSON(this object obj, bool indented = true)
    {
        return JsonConvert.SerializeObject(
            obj,
            (indented ? Formatting.Indented : Formatting.None),
            new JsonConverter[] { new StringEnumConverter(), new IPAddressConverter(), new IPEndPointConverter() }
        );
    }
    #endregion
}
#endif