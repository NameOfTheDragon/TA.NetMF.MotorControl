using System;

namespace TA.AdafruitMotorShield
{
    public interface IStepperMotorControl
    {
        /// <summary>
        /// Configures the motor coils for the specified microstep index.
        /// </summary>
        /// <param name="stepIndex">Index of the step.</param>
        void PerformMicrostep();
    }
}
