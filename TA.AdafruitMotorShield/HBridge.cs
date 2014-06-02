using System;
using Microsoft.SPOT.Hardware;

namespace TA.AdafruitMotorShield
{
    /// <summary>
    /// Class HBridge. Represents an H-Bridge motor driver used to drive stepper motors.
    /// In this configuration, the PWM outputs feed the H-bridge Enable (EN) pins, causing
    /// the motor current to be pulsed on and off at a rate determined by the PWM generator.
    /// When integrated over time, this has the effect of variable current control.
    /// </summary>
    public sealed class HBridge
    {
        PWM pwmA;
        PWM pwmB;

        public HBridge(Cpu.PWMChannel channelA, Cpu.PWMChannel channelB)
        {
            // Configure the PWM defaults to 312500 Hz with 0% duty cycle (not energised).
            pwmA = new PWM(channelA, 312500.0, 0.0, false);
            pwmB = new PWM(channelB, 312500.0, 0.0, false);
        }

        public void SetDutyCycle(double pwm1duty, double pwm2duty)
        {
            pwmA.DutyCycle = pwm1duty;
            pwmB.DutyCycle = pwm2duty;
        }
    }
}
