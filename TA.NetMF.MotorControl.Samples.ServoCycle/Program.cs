// This file is part of the TA.NetMF.MotorControl project
// 
// Copyright © 2014-2015 Tigra Astronomy, all rights reserved.
// This source code is licensed under the MIT License, see http://opensource.org/licenses/MIT
// 
// File: Program.cs  Created: 2015-03-18@02:30
// Last modified: 2015-03-18@02:39 by Tim

using System.Threading;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware.NetduinoPlus;
using TA.NetMF.Motor;
using TA.NetMF.ShieldDriver;

namespace TA.NetMF.MotorControl.Samples.ServoCycle
    {
    public class Program
        {
        static double servo1Position;
        static IServoControl servo1;
        static Timer timer;
        static double increment = +0.25;

        public static void Main()
            {
            var latch = new OutputPort(Pins.GPIO_PIN_D12, false);
            var enable = new OutputPort(Pins.GPIO_PIN_D7, true);
            var data = new OutputPort(Pins.GPIO_PIN_D8, false);
            var clock = new OutputPort(Pins.GPIO_PIN_D4, false);
            var adafruitMotorShieldV1 = new AdafruitV1MotorShield(latch, enable, data, clock);
            adafruitMotorShieldV1.InitializeShield();
            servo1 = adafruitMotorShieldV1.GetServoMotor(1);
            servo1Position = 0;
            timer = new Timer(SetServoPosition, null, 40, 40);
            Thread.Sleep(Timeout.Infinite);
            var dummy = 0;
            }

        static void SetServoPosition(object state)
            {
            servo1Position += increment;
            if (servo1Position > 180)
                {
                servo1Position = 180 - increment;
                increment *= -1;
                }
            if (servo1Position < 0)
                {
                servo1Position = 0 - increment;
                increment *= -1;
                }
            servo1.Angle = servo1Position;
            }
        }
    }