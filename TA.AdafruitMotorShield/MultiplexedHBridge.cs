using System;
using Microsoft.SPOT;

using TA.NetMF.Utils;

namespace TA.AdafruitMotorShield
{
    class MultiplexedHBridge : HBridge
    {
        public override void SetOutputPowerAndPolarity(double duty)
        {
            base.SetOutputPowerAndPolarity(duty);
            var magnitude = System.Math.Abs(duty);
            // ToDo - work out how to configure the h-bridge using Adafruit's funny latch attangement
        }
    }
}
