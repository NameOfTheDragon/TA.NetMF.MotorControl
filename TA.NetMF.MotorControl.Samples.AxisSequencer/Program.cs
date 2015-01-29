// This file is part of the TA.NetMF.MotorControl project
// 
// Copyright © 2014-2014 Tigra Astronomy, all rights reserved.
// This source code is licensed under the MIT License, see http://opensource.org/licenses/MIT
// 
// File: Program.cs  Created: 2014-10-14@03:43
// Last modified: 2014-11-30@13:57 by Tim

#region Shield selection - uncomment only one of the following shields
//#define AdafruitV1Shield
//#define AdafruitV2Shield
//#define SparkfunArduMotoShield
#define LedSimulatorShield
#endregion

#region Hardware options -- do not edit without good reason
#if !SparkfunArduMotoShield
#define UseSecondAxis   // Sparkfun shield only has one H-Bridge so cannot support a second axis.
#endif
#if !LedSimulatorShield
#define UseOnboardLedForDiagnostics
#endif
#endregion


using System;
using System.Threading;
#if AdafruitV2Shield
using TA.NetMF.AdafruitMotorShieldV2;
#elif AdafruitV1Shield
#elif SparkfunArduMotoShield
using TA.
using TA.NetMF.Motor;

namespace TA.NetMF.MotorControl.Samples.AxisSequencer
    {
    public class Program
        {
        const int LimitOfTravel = 5000;

        public static void Main()
            {
            var shield = new MotorShield();
            var motor1 = shield.GetMicrosteppingStepperMotor(64, 1, 2); // 64 microsteps, outputs M1 and M2
            var motor2 = shield.GetMicrosteppingStepperMotor(64, 3, 4); // 64 microsteps, outputs M3 and M4
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
