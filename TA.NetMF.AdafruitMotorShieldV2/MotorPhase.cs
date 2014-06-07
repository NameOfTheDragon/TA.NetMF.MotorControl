using System;
using Microsoft.SPOT;

namespace TA.NetMF.AdafruitMotorShieldV2
    {
    /// <summary>
    /// Class MotorPhase. Represents one winding (or phase) within a motor.
    /// DC motors have a single phase, stepper motors have 2 phases.
    /// </summary>
    internal class MotorPhase
        {
        int AllocatedToMotor = -1;
        byte In1Shource;
        byte In2Source;
        byte PwmSource;
        Pca9685PwmController pwm;

        /// <summary>
        /// Initializes a new instance of the <see cref="MotorPhase"/> class.
        /// </summary>
        /// <param name="pwm">The PWM controller instance that controls this motor phase.</param>
        /// <param name="in1Shource">The PWM channel number that provides the IN1 input for this winding.</param>
        /// <param name="in2Source">The PWM channel number that provides the IN2 input for this winding.</param>
        /// <param name="pwmSource">The PWM source.</param>
        public MotorPhase(Pca9685PwmController pwm, ushort in1Shource, ushort in2Source, ushort pwmSource)
            {
            this.pwm = pwm;
            In1Shource = (byte)(Pca9685.ChannelBase + (in1Shource*4));
            In2Source = (byte)(Pca9685.ChannelBase + (in2Source * 4));
            PwmSource = (byte)(Pca9685.ChannelBase + (pwmSource * 4));
            Release();
            }

        /// <summary>
        /// Releases this phase (removes power, releases any holding torque).
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        void Release()
            {
            throw new NotImplementedException();
            }
        }
    }
