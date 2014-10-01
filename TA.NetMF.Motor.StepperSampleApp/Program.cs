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

namespace TA.NetMF.Motor.StepperSampleApp
    {
    public class Program
        {
        const int LimitOfTravel = 100;
        const int MaxSpeed = 2; // steps per second (Netduino Plus 2 can manage a few hundred)
        const int MicrostepsPerStep = 4; // 4=full stepping; 8=half stepping; 9+=microstepping.
        const int PwmFrequencyHz = 1600;
        const double RampTime = 2.5; // seconds to reach full speed (acceleration)
        static OutputPort Led;
        static bool LedState;
        static readonly Random brandon = new Random();
        static IStepperMotorControl stepper;

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
            stepper = adafruitMotorShieldV2.GetStepperMotor(8, 3, 4);

            // Create the stepper motor axes and link them to the Adafruit driver.
            var axis = new AcceleratingStepperMotor(LimitOfTravel, PerformMicrostep)
                {
                MaximumSpeed = MaxSpeed,
                RampTime = RampTime
                };

            axis.MotorStopped += HandleMotorStoppedEvent;
            HandleMotorStoppedEvent(axis);
            Thread.Sleep(Timeout.Infinite);

            //while (true)
            //{
            //    var randomSpeed = brandon.NextDouble() * axis.MaximumSpeed * 2 - axis.MaximumSpeed;
            //    if (Math.Abs(randomSpeed) <= 0.1)
            //        continue;
            //    axis.MoveAtRegulatedSpeed(randomSpeed);
            //    Thread.Sleep((int)(RampTime * 2000));
            //}
            }

        static void HandleMotorStoppedEvent(AcceleratingStepperMotor axis)
            {
            Led.Write(false);
            Thread.Sleep(5000);
            var randomTarget = brandon.Next(LimitOfTravel);
            Trace.Print("Starting move to " + randomTarget.ToString());
            axis.MoveToTargetPosition(randomTarget);
            }

        static void PerformMicrostep(int direction)
            {
            stepper.PerformMicrostep(direction);
            LedState = !LedState;
            Led.Write(LedState);
            }
        }
    }
