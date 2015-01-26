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
#define SparkfunArduMotoShield
//#define LedSimulatorShield
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
using TA.NetMF.SparkfunArdumotoShield;

namespace TA.NetMF.MotorControl.Samples
    {
    public class Program
        {
        static IStepperMotorControl StepperM1M2;
        static IStepperMotorControl StepperM3M4;
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
            var adafruitMotorShieldV1 = new AdafruitMotorShieldV1.MotorShield(latch, enable, data, clock);
            adafruitMotorShieldV1.InitializeShield();
            StepperM1M2 = adafruitMotorShieldV1.GetMicrosteppingStepperMotor(64, 1, 2);
            StepperM3M4 = adafruitMotorShieldV1.GetFullSteppingStepperMotor(3, 4);
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

            // Create the stepper motor axes and link them to the Adafruit driver.
            var axis1 = new AcceleratingStepperMotor(LimitOfTravel, StepperM1M2, UpdateDiagnosticLed)
                {
                MaximumSpeed = MaxSpeed,
                RampTime = RampTime
                };
            // Now we subscribe to the MotorStopped event on each axis. When the event fires, 
            // we start the axis going again with a new random target position.
            axis1.MotorStopped += HandleAxisStoppedEvent;
            // We need to call our event handler once manually to get things going.
            // After that it is fully automatic.
            HandleAxisStoppedEvent(axis1);

            // Repeat for the second axis, if it's supported.
#if UseSecondAxis
            var axis2 = new AcceleratingStepperMotor(LimitOfTravel, StepperM3M4)
                {
                MaximumSpeed = MaxSpeed,
                RampTime = RampTime
                };
            axis2.MotorStopped += HandleAxisStoppedEvent;
            HandleAxisStoppedEvent(axis2);
#endif

            // Finally, we sleep forever as there is nothing else to do in the main thread.
            // The motors continue to work in the background all by themselves.
            Thread.Sleep(Timeout.Infinite);
            }

        /// <summary>
        ///   Handles the axis stopped event for an axis.
        ///   Picks a new random position and starts a new move.
        /// </summary>
        /// <param name="axis">The axis that has stopped.</param>
        static void HandleAxisStoppedEvent(AcceleratingStepperMotor axis)
            {
            Thread.Sleep(3000); // Wait a short time before starting the next move.
            var randomTarget = randomGenerator.Next(LimitOfTravel);
            Trace.Print("Starting move to " + randomTarget);
            axis.MoveToTargetPosition(randomTarget);
            }

        /// <summary>
        ///   Toggles the Netduino's on-board led.
        /// </summary>
        /// <param name="direction">The direction.</param>
        static void UpdateDiagnosticLed(int direction)
            {
            ConditionalUpdateDiagnosticLed(direction);
            }
        [Conditional("UseOnboardLedForDiagnostics")]
        static void ConditionalUpdateDiagnosticLed(int direction)
            {
            LedState = !LedState;
            Led.Write(LedState);
            }


        #region Stepper Configuration - Change these values to your liking.
        const int LimitOfTravel = 4000;

        /// <summary>
        ///   The maximum speed in steps per second.
        ///   Although the theoretical maximum is 1,000 steps per second, in practice
        ///   the netduino plus can handle about 400 steps (or microsteps) per second, total.
        /// </summary>
        const int MaxSpeed = 400;

        /// <summary>
        ///   The number of microsteps per whole step.
        ///   Specify 4 to use whole steps, 8 to use half-steps, or 9+ to use microsteps.
        ///   The higher the value, the smoother the motor motion will be but the slower it will turn.
        /// </summary>
        const int MicrostepsPerStep = 64;

        /// <summary>
        ///   The ramp time, i.e. the number of seconds taken to reach <see cref="MaxSpeed" />.
        ///   Setting <see cref="AcceleratingStepperMotor.RampTime" /> affects <see cref="AcceleratingStepperMotor.Acceleration" />
        ///   and vice versa.
        /// </summary>
        const double RampTime = 3; // seconds to reach full speed (acceleration)
        #endregion
        }
    }
