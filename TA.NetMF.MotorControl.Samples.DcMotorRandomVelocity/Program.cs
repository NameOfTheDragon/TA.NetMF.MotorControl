// This file is part of the TA.NetMF.MotorControl project
// 
// Copyright © 2014-2015 Tigra Astronomy, all rights reserved.
// This source code is licensed under the MIT License, see http://opensource.org/licenses/MIT
// 
// File: Program.cs  Created: 2015-01-13@13:45
// Last modified: 2015-01-19@00:25 by Tim

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
using System.Diagnostics;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;
using TA.NetMF.Motor;
#if SparkfunArduMotoShield
using TA.NetMF.SparkfunArdumotoShield;
#elif AdafruitV1Shield
using TA.NetMF.AdafruitMotorShieldV1;
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
#if AdafruitV1Shield
            var latch = new OutputPort(Pins.GPIO_PIN_D12, false);
            var enable = new OutputPort(Pins.GPIO_PIN_D7, true);
            var data = new OutputPort(Pins.GPIO_PIN_D8, false);
            var clock = new OutputPort(Pins.GPIO_PIN_D4, false);
            var adafruitMotorShieldV1 = new MotorShield(latch, enable, data, clock);
            adafruitMotorShieldV1.InitializeShield();
            StepperM1M2 = adafruitMotorShieldV1.GetFullSteppingStepperMotor(1, 2);
            StepperM3M4 = adafruitMotorShieldV1.GetMicrosteppingStepperMotor(64, 3, 4);
#elif AdafruitV2Shield
            var adafruitMotorShieldV2 = new MotorShield();  // use shield at default I2C address.
            adafruitMotorShieldV2.InitializeShield();
            StepperM1M2 = adafruitMotorShieldV2.GetMicrosteppingStepperMotor(MicrostepsPerStep, 1, 2);
            StepperM3M4 = adafruitMotorShieldV2.GetMicrosteppingStepperMotor(MicrostepsPerStep, 3, 4);
#elif SparkfunArduMotoShield
            var shield = new ArdumotoShield();
            shield.InitializeShield();
            var phase1 = shield.GetHBridge(Connector.A, TargetDevice.Netduino);
            var phase2 = shield.GetHBridge(Connector.B, TargetDevice.Netduino);
            StepperM1M2 = shield.GetMicrosteppingStepperMotor(MicrostepsPerStep, phase1, phase2);
#elif LedSimulatorShield
            var bridge1 = LedMotorSimulator.GetDcMotor(Pins.GPIO_PIN_D0, PWMChannels.PWM_ONBOARD_LED);
            //var bridge2 = LedMotorSimulator.GetDcMotor(Pins.GPIO_PIN_D1, PWMChannels.PWM_PIN_D6);
#else
            throw new ApplicationException("Uncomment one of the shield #define statements");
#endif

            // Create the stepper motor axes and link them to the Adafruit driver.
            var motor1 = new DcMotor(bridge1);
            while (true)
                {
                var targetSpeed = randomGenerator.NextDouble() * 2.0 -1.0; // range -1.0 to +1.0
                motor1.AccelerateToVelocity(targetSpeed);
                Thread.Sleep(6000);
                }
            }

        }
    }
