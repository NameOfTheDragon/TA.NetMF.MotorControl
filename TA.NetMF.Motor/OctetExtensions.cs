using System;
using Microsoft.SPOT;

namespace TA.NetMF.Motor
    {
    public static class OctetExtensions
        {
        public static Octet SetBit(this Octet source, ushort bit)
            {
            return source.WithBitSetTo(bit, true);
            }
        public static Octet ClearBit(this Octet source, ushort bit)
            {
            return source.WithBitSetTo(bit, false);
            }
        }
    }
