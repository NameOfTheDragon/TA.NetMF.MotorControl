// This file is part of the TA.NetMF.MotorControl project
// 
// Copyright © 2014-2014 Tigra Astronomy, all rights reserved.
// This source code is licensed under the MIT License, see http://opensource.org/licenses/MIT
// 
// File: Program.cs  Created: 2014-10-14@03:43
// Last modified: 2014-11-30@13:57 by Tim
using System;
using System.Threading;
using TA.NetMF.AdafruitMotorShieldV2;
using TA.NetMF.Motor;

namespace TA.NetMF.MotorControl.Samples.AxisSequencer
    {
    public class Program
        {
        const int LimitOfTravel = 5000;

        public static void Main()
            {
            var shield = new MotorShield();
            var motor1 = shield.GetStepperMotor(64, 1, 2); // 64 microsteps, outputs M1 and M2
            var motor2 = shield.GetStepperMotor(64, 3, 4); // 64 microsteps, outputs M3 and M4
            var axis1 = new AcceleratingStepperMotor(LimitOfTravel, motor1)
                {
                MaximumSpeed = AcceleratingStepperMotor.MaximumPossibleSpeed,
                RampTime = 2.0
                };
            var axis2 = new AcceleratingStepperMotor(LimitOfTravel, motor2)
                {
                MaximumSpeed = AcceleratingStepperMotor.MaximumPossibleSpeed,
                RampTime = 2.0
                };
            var sequencer = new DualAxisSequencer(axis1, axis2);
            var randomGenerator = new Random();

            while (true)
                {
                var target = randomGenerator.Next(LimitOfTravel);
                sequencer.RunInSequence(target, target);
                sequencer.BlockUntilSequenceComplete();
                Thread.Sleep(5000); // A pause here just makes it easier to observe what is going on.
                }
            }
        }
    }
