// This file is part of the TA.NetMF.MotorControl project
// 
// Copyright © 2014-2014 Tigra Astronomy, all rights reserved.
// This source code is licensed under the MIT License, see http://opensource.org/licenses/MIT
// 
// File: SimpleHBridge.cs  Created: 2014-06-05@02:27
// Last modified: 2014-11-30@13:57 by Tim
using System;
using Microsoft.SPOT.Hardware;
using TA.NetMF.Motor;

namespace TA.NetMF.SparkfunArdumotoShield
    {
    /// <summary>
    /// Class SimpleHBridge. Implements a simple H-Bridge that can be used to control
    /// a DC motor or one winding of a stepper motor. The H-Bridge consists of a direction
    /// control pin and a PWM channel that is used to modulate the Enable pin of the driver chip.
    /// This class is intended to work with devices such as L293D.
    /// </summary>
    public class SimpleHBridge : HBridge
        {
        readonly OutputPort direction;
        readonly PWM power;

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleHBridge"/> class.
        /// </summary>
        /// <param name="powerControlChannel">The PWM channel to be used for power control.</param>
        /// <param name="directionControlPin">The pin to be used for direction control.</param>
        public SimpleHBridge(Cpu.PWMChannel powerControlChannel, Cpu.Pin directionControlPin)
            {
            direction = new OutputPort(directionControlPin, false);
            power = new PWM(powerControlChannel, 500.0, 0.0, false);
            power.Start();
            }

        public override void SetOutputPowerAndPolarity(double duty)
            {
            var polarity = (duty >= 0.0);
            var magnitude = Math.Abs(duty);
            SetOutputPowerAndPolarity(magnitude, polarity);
            base.SetOutputPowerAndPolarity(duty);
            }


        void SetOutputPowerAndPolarity(double magnitude, bool polarity)
            {
            if (polarity != Polarity)
                power.DutyCycle = 0.0; // If reversing direction, set power to zero first.
            direction.Write(polarity);
            power.DutyCycle = magnitude;
            }
        }
    }
