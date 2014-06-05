// This file is part of the TA.NetMF.MotorControl project
// 
// Copyright © 2014 Tigra Astronomy, all rights reserved.
// This source code is licensed under Creative Commons Attribution International 4.0 license
// http://creativecommons.org/licenses/by/4.0/
// 
// File: MultiplexedHBridge.cs  Created: 2014-06-05@02:27
// Last modified: 2014-06-05@12:23 by Tim

using System;
using TA.NetMF.Motor;

namespace TA.NetMF.AdafruitMotorShield
    {
    internal class MultiplexedHBridge : HBridge
        {
        public override void SetOutputPowerAndPolarity(double duty)
            {
            base.SetOutputPowerAndPolarity(duty);
            var magnitude = Math.Abs(duty);
            // ToDo - work out how to configure the h-bridge using Adafruit's funny latch attangement
            }
        }
    }
