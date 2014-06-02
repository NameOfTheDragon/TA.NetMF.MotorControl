using System;

namespace TA.AdafruitMotorShield
{
    class StepperMotor : IStepperMotorControl
    {
        private HBridge hbridge;
        private int microsteps;
        private int phaseIndex;
        private int maxIndex;
        private double[] inPhaseDutyCycle;
        private double[] outOfPhaseDutyCycle;
        public StepperMotor(HBridge bridge, int microsteps)
        {
            this.hbridge = bridge;
            this.microsteps = microsteps;
            maxIndex = microsteps * 2;
            ComputeMicrostepTables();
            phaseIndex = 0;
        }

        private void ComputeMicrostepTables()
        {
            // This implementation prefers performance over memory footprint.
            var radiansPerIndex = Math.PI / maxIndex;
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
            hbridge.SetDutyCycle(inPhaseDutyCycle[phaseIndex],outOfPhaseDutyCycle[phaseIndex]);
        }
    }
}
