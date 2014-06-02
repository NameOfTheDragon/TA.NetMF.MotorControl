using System;
using Microsoft.SPOT.Hardware;

namespace TA.NetMF.Utils
{
    /// <summary>
    /// Class HBridge. Represents an H-Bridge motor driver with polarity and power control.
    /// An H-bridge allows the direction of current flow to be controlled (polarity)
    /// and allows the current to be turn on or off (enable). By using a PWM waveform
    /// generator at the enable pin, the power output can be varied
    /// by varying the PWN duty cycle. Thus, one H-bridge can control a single
    /// motor winding, be that a DC motor or one phase of a stepper motor.
    /// </summary>
    public abstract class HBridge
    {
        double duty;

        /// <summary>
        /// Sets the output power and polarity of the H-bridge.
        /// </summary>
        /// <param name="duty">
        /// The output power expressed as a fraction, in the range -1.0 to +1.0 inclusive.
        /// Positive values represent normal polarity and negative values represent reversed polarity.
        /// </param>
        /// <remarks>
        /// No attempt is made to define the sense of the polarity, it is merely 'one way or the other'.
        /// The actual sign of voltages or current produced are dependent on the application.
        /// </remarks>
        public virtual void SetOutputPowerAndPolarity(double duty)
        {
            if (duty > 1.0 || duty < -1.0)
            {
                throw new ArgumentOutOfRangeException("duty", "-1.0 to 1.0 inclusive");
            }
            this.duty = duty;
        }

        public bool Polarity { get { return (duty >= 0); } }
        public double Power { get { return Math.Abs(duty); } }
    }
}
