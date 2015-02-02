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

using SecretLabs.NETMF.Hardware.Netduino;
using System;
using System.Threading;
using TA.NetMF.Motor;
using TA.NetMF.ShieldDriver.Simulator;
#if SparkfunArduMotoShield
using TA.NetMF.SparkfunArdumotoShield;
#elif AdafruitV1Shield
using TA.NetMF.AdafruitMotorShieldV1;
#elif AdafruitV2Shield
using TA.NetMF.AdafruitMotorShieldV2;
#elif LedSimulatorShield

#else
#error Incorrect shield configuration - please uncomment exactly one #define
#endif

namespace TA.NetMF.MotorControl.Samples.AxisSequencer
    {
    public class Program
        {
        const int LimitOfTravel = 5000;

        public static void Main()
            {
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
            StepperM1M2 = adafruitMotorShieldV1.GetHalfSteppingStepperMotor(1, 2);
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
            var StepperM1M2 = LedMotorSimulator.GetSimulatedStepperMotor(Pins.GPIO_PIN_D0,
                PWMChannels.PWM_ONBOARD_LED,
                Pins.GPIO_PIN_D1,
                PWMChannels.PWM_PIN_D6);
            var StepperM3M4 = LedMotorSimulator.GetSimulatedStepperMotor(Pins.GPIO_PIN_D2,
                PWMChannels.PWM_PIN_D9,
                Pins.GPIO_PIN_D3,
                PWMChannels.PWM_PIN_D10);
#else
            throw new ApplicationException("Uncomment one of the shield #define statements");
#endif

            var axis1 = new AcceleratingStepperMotor(LimitOfTravel, StepperM1M2)
                {
                MaximumSpeed = StepperMotor.MaximumPossibleSpeed,
                RampTime = 2.0
                };
            var axis2 = new AcceleratingStepperMotor(LimitOfTravel, StepperM3M4)
                {
                MaximumSpeed = StepperMotor.MaximumPossibleSpeed,
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
