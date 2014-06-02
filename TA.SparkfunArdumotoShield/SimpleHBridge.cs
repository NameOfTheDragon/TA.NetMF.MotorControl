using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using TA.NetMF.Utils;

namespace TA.SparkfunArdumotoShield
{
    public class SimpleHBridge : HBridge
    {
        OutputPort direction;
        PWM enable;

        public SimpleHBridge(PWM enable, OutputPort direction)
        {
            this.enable = enable;
            this.direction = direction;
        }

        public override void SetOutputPowerAndPolarity(double duty)
        {
            base.SetOutputPowerAndPolarity(duty);
            var polarity = (duty >= 0.0);
            var magnitude = System.Math.Abs(duty);
            SetOutputPowerAndPolarity(magnitude, polarity);
        }

        private void SetOutputPowerAndPolarity(double magnitude, bool polarity)
        {
            if (polarity != this.Polarity)
                enable.DutyCycle = 0.0;  // If reversing direction, set power to zero first.
            direction.Write(polarity);
            enable.DutyCycle = magnitude;
        }
    }
}
