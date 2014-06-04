using System;

namespace TA.NetMF.Utils
{
    public interface IStepperMotorControl
    {
        /// <summary>
        /// Configures the motor coils for the specified microstep index.
        /// </summary>
        /// <param name="direction">The direction, +1 for forwards, -1 for reverse, 0 for stop.</param>
        void PerformMicrostep(int direction);
    }
}
