using System;
using TA.NetMF.Utils;

namespace TA.NetMF.Utils
{
    public class MicrosteppingStepperMotor : IStepperMotorControl
    {
        private HBridge phase1;
        private HBridge phase2;
        private int microsteps;
        private int phaseIndex;
        private int maxIndex;
        private double[] inPhaseDutyCycle;
        private double[] outOfPhaseDutyCycle;
        public MicrosteppingStepperMotor(HBridge phase1bridge, HBridge phase2bridge, int microsteps)
        {
            this.phase1 = phase1bridge;
            this.phase2 = phase2bridge;
            this.microsteps = microsteps;
            this.maxIndex = microsteps;
            ComputeMicrostepTables();
            phaseIndex = 0;
        }

        private void ComputeMicrostepTables()
        {
            // This implementation prefers performance over memory footprint.
            var radiansPerIndex = (2*Math.PI) / maxIndex;
            inPhaseDutyCycle = new double[maxIndex];
            outOfPhaseDutyCycle = new double[maxIndex];
            for (int i = 0; i < maxIndex; ++i)
            {
                var phaseAngle = i * radiansPerIndex;
                inPhaseDutyCycle[i] = Math.Sin(phaseAngle);
                outOfPhaseDutyCycle[i] = Math.Cos(phaseAngle);
            }
        }
        public void PerformMicrostep()
        {
            phaseIndex = ++phaseIndex % maxIndex;
            phase1.SetOutputPowerAndPolarity(inPhaseDutyCycle[phaseIndex]);
            phase2.SetOutputPowerAndPolarity(outOfPhaseDutyCycle[phaseIndex]);
        }
    }
}
