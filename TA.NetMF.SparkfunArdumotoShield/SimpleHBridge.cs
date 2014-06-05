// This file is part of the TA.NetMF.MotorControl project
// 
// Copyright © 2014 Tigra Astronomy, all rights reserved.
// This source code is licensed under Creative Commons Attribution International 4.0 license
// http://creativecommons.org/licenses/by/4.0/
// 
// File: SimpleHBridge.cs  Created: 2014-06-05@02:27
// Last modified: 2014-06-05@12:23 by Tim

using System;
using Microsoft.SPOT.Hardware;
using TA.NetMF.Motor;

namespace TA.NetMF.SparkfunArdumotoShield
    {
    public class SimpleHBridge : HBridge
        {
        readonly OutputPort direction;
        readonly PWM enable;

        public SimpleHBridge(PWM enable, OutputPort direction)
            {
            this.enable = enable;
            this.direction = direction;
            }

        public override void SetOutputPowerAndPolarity(double duty)
            {
            base.SetOutputPowerAndPolarity(duty);
            var polarity = (duty >= 0.0);
            var magnitude = Math.Abs(duty);
            SetOutputPowerAndPolarity(magnitude, polarity);
            }

        void SetOutputPowerAndPolarity(double magnitude, bool polarity)
            {
            if (polarity != Polarity)
                enable.DutyCycle = 0.0; // If reversing direction, set power to zero first.
            direction.Write(polarity);
            enable.DutyCycle = magnitude;
            }
        }
    }
