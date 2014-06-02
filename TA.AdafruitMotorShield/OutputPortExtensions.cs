using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;

namespace TA.AdafruitMotorShield
{
    public static class OutputPortExtensions
    {
        public static void High(this OutputPort port)
        {
            port.Write(true);
        }
        public static void Low(this OutputPort port)
        {
            port.Write(false);
        }
    }
}
