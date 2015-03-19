// This file is part of the TA.NetMF.MotorControl project
// 
// Copyright © 2014-2015 Tigra Astronomy, all rights reserved.
// This source code is licensed under the MIT License, see http://opensource.org/licenses/MIT
// 
// File: Program.cs  Created: 2015-01-13@13:45
// Last modified: 2015-01-19@00:25 by Tim

#region Shield selection - uncomment only one of the following shields
#define AdafruitV1Shield
//#define AdafruitV2Shield
//#define SparkfunArduMotoShield
//#define LedSimulatorShield
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
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware.NetduinoPlus;
using TA.NetMF.Motor;
using TA.NetMF.ShieldDriver;

#if SparkfunArduMotoShield
using TA.NetMF.SparkfunArdumotoShield;
#elif AdafruitV1Shield

#elif AdafruitV2Shield
using TA.NetMF.AdafruitMotorShieldV2;
#elif LedSimulatorShield
using TA.NetMF.MotorSimulator;
#else
#error Incorrect shield configuration - please uncomment exactly one #define
#endif

namespace TA.NetMF.MotorControl.Samples
    {
    public class Program
        {
        static readonly Random randomGenerator = new Random();
        static OutputPort Led;
        static bool LedState;

        public static void Main()
            {
            // shield-specific setup. Uncomment one of the shield #define lines above.
#if UseOnboardLedForDiagnostics
            Led = new OutputPort(Pins.ONBOARD_LED, false);
#endif
            HBridge bridge1;
            HBridge bridge2;
#if AdafruitV1Shield
            var shield = new AdafruitV1MotorShield();
            shield.InitializeShield();
            bridge1 = shield.GetDcMotor(1);
            bridge2 = shield.GetDcMotor(2);
#elif AdafruitV2Shield
            var shield = new AdafruitV2MotorShield();
            shield.InitializeShield();
            bridge1 = shield.GetDcMotor(1);
            bridge2 = shield.GetDcMotor(2);
#elif SparkfunArduMotoShield
            var shield = new SparkfunArdumoto();
            shield.InitializeShield();
            bridge1 = shield.GetHBridge(Connector.A, TargetDevice.Netduino);
            bridge2 = shield.GetHBridge(Connector.B, TargetDevice.Netduino);
#elif LedSimulatorShield
            bridge1 = LedMotorSimulator.GetDcMotor(Pins.GPIO_PIN_D0, PWMChannels.PWM_ONBOARD_LED);
            bridge2 = LedMotorSimulator.GetDcMotor(Pins.GPIO_PIN_D1, PWMChannels.PWM_PIN_D6);
#else
            throw new ApplicationException("Uncomment one of the shield #define statements");
#endif

            // Create the stepper motor axes and link them to the Adafruit driver.
            var motor1 = new DcMotor(bridge1);
            var motor2 = new DcMotor(bridge2);
            while (true)
                {
                var targetSpeed1 = randomGenerator.NextDouble() / 2.0 + 0.5; // range -1.0 to +1.0
                var targetSpeed2 = randomGenerator.NextDouble() * 2.0 - 1.0; // range -1.0 to +1.0
                motor1.AccelerateToVelocity(targetSpeed1);
                motor2.AccelerateToVelocity(targetSpeed2);
                Thread.Sleep(6000);
                }
            }

        }
    }
