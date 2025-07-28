#if USE_FNV1A
using System;
using System.Text;
using Fnv1a;

namespace Bluscream;

public static partial class Extensions
{
    public static readonly enum CallOfDutyFnv1a32 {
        Prime = 0xB3CB2E29,
        OffsetBasis = 0x319712C3,
    }
    #region string
    public static string ToHashFnv1a32(this string text, Fnv1a32 hasher = null)
    {
        text = text.Trim().ToLowerInvariant() + "\0";
        var bytes_encoded = Encoding.ASCII.GetBytes(text);
        if (hasher is null)
            hasher = new Fnv1a32(fnvPrime: FnvPrime, fnvOffsetBasis: FnvOffsetBasis);
        var byte_hash = hasher.ComputeHash(bytes_encoded);
        var uint32 = BitConverter.ToUInt32(byte_hash, 0);
        var uint32_hex = string.Format("0x{0:X}", uint32);
        return uint32_hex;
    }


    #endregion
}
#endif 