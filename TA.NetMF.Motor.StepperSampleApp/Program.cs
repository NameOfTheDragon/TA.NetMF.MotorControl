// This file is part of the TA.NetMF.MotorControl project
// 
// Copyright © 2014 Tigra Astronomy, all rights reserved.
// This source code is licensed under Creative Commons Attribution International 4.0 license
// http://creativecommons.org/licenses/by/4.0/
// 
// File: Program.cs  Created: 2014-06-05@02:27
// Last modified: 2014-09-30@04:23 by Tim

using System;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;
using TA.NetMF.AdafruitMotorShieldV2;
using TA.NetMF.Motor;

namespace TA.NetMF.MotorControl.Samples
    {
    public class Program
        {
        const int LimitOfTravel = 10000;
        const int MaxSpeed = 400; // steps per second (Netduino Plus 2 can manage a few hundred)
        const int MicrostepsPerStep = 8; // 4=full stepping; 8=half stepping; 9+=microstepping.
        const int PwmFrequencyHz = 1600;
        const double RampTime = 2; // seconds to reach full speed (acceleration)
        static OutputPort Led;
        static bool LedState;
        static readonly Random randomGenerator = new Random();
        static IStepperMotorControl StepperM3M4;
        static IStepperMotorControl StepperM1M2;

        public static void Main()
            {
            // First we set up an LED so we can see what's happening
            Led = new OutputPort(Pins.ONBOARD_LED, false);

            // Now for the shield-specific setup.

            #region Adaruit shield setup
            var adafruitMotorShieldV2 = new MotorShield();
            adafruitMotorShieldV2.InitializeShield();
            #endregion

            #region Sparkfun Ardumoto Shield setup
            //// Sparkfun Ardumoto Shield

            //var directionA = new OutputPort(Pins.GPIO_PIN_D12, false);
            //var bridgeA = new SimpleHBridge(pwmPhase1, directionA);
            //var directionB = new OutputPort(Pins.GPIO_PIN_D13, false);
            //var bridgeB = new SimpleHBridge(pwmPhase2, directionB);

            //var motorShield = new ArdumotoShield(bridgeA, bridgeB);
            #endregion Sparkfun Ardumoto Shield setup

            // The shield details are abstracted from the motor control, so whatever shield we have,
            // we just ask it for an IStepperMotorControl, specifying the number of microsteps and the
            // output numbers of the two motor phases.
            StepperM1M2 = adafruitMotorShieldV2.GetStepperMotor(64, 1, 2);
            StepperM3M4 = adafruitMotorShieldV2.GetStepperMotor(64, 3, 4);

            // Create the stepper motor axes and link them to the Adafruit driver.
            var axis1 = new AcceleratingStepperMotor(LimitOfTravel, StepperM1M2, UpdateDiagnosticLed)
            {
                MaximumSpeed = MaxSpeed,
                RampTime = RampTime
            };
            var axis2 = new AcceleratingStepperMotor(LimitOfTravel, StepperM3M4)
            {
                MaximumSpeed = MaxSpeed,
                RampTime = RampTime
            };

            axis1.MotorStopped += HandleAxisStoppedEvent;
            axis2.MotorStopped += HandleAxisStoppedEvent;
            HandleAxisStoppedEvent(axis1);
            HandleAxisStoppedEvent(axis2);
            Thread.Sleep(Timeout.Infinite);

            //while (true)
            //{
            //    var randomSpeed = randomGenerator.NextDouble() * axis.MaximumSpeed * 2 - axis.MaximumSpeed;
            //    if (Math.Abs(randomSpeed) <= 0.1)
            //        continue;
            //    axis.MoveAtRegulatedSpeed(randomSpeed);
            //    Thread.Sleep((int)(RampTime * 2000));
            //}
            }

        static void HandleAxisStoppedEvent(AcceleratingStepperMotor axis)
            {
            //Thread.Sleep(5000);
            var randomTarget = randomGenerator.Next(LimitOfTravel);
            Trace.Print("Starting move to " + randomTarget.ToString());
            axis.MoveToTargetPosition(randomTarget);
            }


        static void UpdateDiagnosticLed(int direction)
            {
            LedState = !LedState;
            Led.Write(LedState);
            }
        }
    }
