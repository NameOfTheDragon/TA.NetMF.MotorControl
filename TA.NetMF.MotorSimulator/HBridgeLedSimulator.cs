using Microsoft.SPOT.Hardware;
using TA.NetMF.Motor;
using Math = System.Math;

namespace TA.NetMF.ShieldDriver.Simulator
    {
    public class HBridgeLedSimulator : HBridge
        {
        readonly OutputPort directionIndicator;
        readonly PWM powerLevelIndicator;

        public HBridgeLedSimulator(OutputPort directionIndicator, Cpu.PWMChannel powerPwmChannel)
            {
            this.directionIndicator = directionIndicator;
            powerLevelIndicator = new PWM(powerPwmChannel, 100.0, dutyCycle: 0.5, invert: true);
            powerLevelIndicator.Start();
            }

        public override void SetOutputPowerAndPolarity(double duty)
            {
            base.SetOutputPowerAndPolarity(duty);
            directionIndicator.Write(duty > 0.0);
            powerLevelIndicator.DutyCycle = Math.Abs(duty);
            }
        }
    }
