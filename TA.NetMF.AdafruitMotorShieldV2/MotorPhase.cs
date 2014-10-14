// This file is part of the TA.NetMF.MotorControl project
// 
// Copyright © 2014 Tigra Astronomy, all rights reserved.
// This source code is licensed under Creative Commons Attribution International 4.0 license
// http://creativecommons.org/licenses/by/4.0/
// 
// File: MotorPhase.cs  Created: 2014-06-07@15:30
// Last modified: 2014-09-28@05:10 by Tim

using System;
using TA.NetMF.Motor;

namespace TA.NetMF.AdafruitMotorShieldV2
    {
    /// <summary>
    ///   Class MotorPhase. Represents one winding (or phase) within a motor.
    ///   DC motors have a single phase, stepper motors have 2 phases.
    ///   This type of circuitry is often referred to as an <see cref="HBridge"/>.
    /// </summary>
    internal class MotorPhase : HBridge
        {
        readonly PwmBoolean in1;
        readonly PwmBoolean in2;
        readonly PwmChannel powerControl;
        Pca9685PwmController pwmController;

        /// <summary>
        ///   Initializes a new instance of the <see cref="MotorPhase" /> class.
        /// </summary>
        /// <param name="pwmController">The 16-channel PWM controller instance that controls this motor phase.</param>
        /// <param name="in1PwmChannelNumber">The PWM channel number that provides the IN1 input for this winding.</param>
        /// <param name="in2PwmChannelNumber">The PWM channel number that provides the IN2 input for this winding.</param>
        /// <param name="pwmPowerControlChannelNumber">The PWM source.</param>
        public MotorPhase(Pca9685PwmController pwmController,
            ushort in1PwmChannelNumber,
            ushort in2PwmChannelNumber,
            ushort pwmPowerControlChannelNumber)
            {
            this.pwmController = pwmController;
            in1 = new PwmBoolean(pwmController.GetPwmChannel(in1PwmChannelNumber));
            in2 = new PwmBoolean(pwmController.GetPwmChannel(in2PwmChannelNumber));
            powerControl = pwmController.GetPwmChannel(pwmPowerControlChannelNumber);
            }

        /// <summary>
        ///   Releases this phase (removes power, releases any holding torque).
        /// </summary>
        void Release()
            {
            powerControl.DutyCycle = 0.0;
            in1.State = false;
            in2.State = false;
            }

        /// <summary>
        ///   Configures the motor winding to drive in the forward direction.
        ///   Forward is arbitrarily defined to be Clockwise (CW).
        /// </summary>
        void Forward()
            {
            // Order is important; must avoid setting both outputs to true, which would cause a BRAKE condition.
            in2.State = false; 
            in1.State = true;
            }

        /// <summary>
        ///   Configures the motor winding to drive in the reverse direction.
        ///   Reverse is arbitrarily defined to be Counter Clockwise (CCW).
        /// </summary>
        void Reverse()
            {
            in1.State = false;
            in2.State = true;
            }

        /// <summary>
        ///   Sets the output power and polarity of the H-bridge.
        /// </summary>
        /// <param name="duty">
        ///   The output power expressed as a fraction, in the range -1.0 to +1.0 inclusive.
        ///   Positive values represent normal (forwards) polarity and negative values represent reversed polarity.
        /// </param>
        public override void SetOutputPowerAndPolarity(double duty)
            {
            base.SetOutputPowerAndPolarity(duty);
            var polarity = (duty >= 0.0);
            var magnitude = Math.Abs(duty);
            SetOutputPowerAndPolarity(magnitude, polarity);
            }

        /// <summary>
        ///   Sets the output power and polarity of the H-Bridge.
        /// </summary>
        /// <param name="magnitude">The magnitude, or absolute power setting.</param>
        /// <param name="polarity">if set to <c>true</c> then the motor runs in the forward direction; otherwise in reverse.</param>
        void SetOutputPowerAndPolarity(double magnitude, bool polarity)
            {
            if (polarity != Polarity)
                powerControl.DutyCycle = 0.0; // If reversing direction, set power to zero first.
            if (polarity)
                Forward();
            else
                Reverse();
            powerControl.DutyCycle = magnitude;
            }
        }
    }
