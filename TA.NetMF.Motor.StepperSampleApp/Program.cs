// This file is part of the TA.NetMF.MotorControl project
// 
// Copyright © 2014 Tigra Astronomy, all rights reserved.
// This source code is licensed under Creative Commons Attribution International 4.0 license
// http://creativecommons.org/licenses/by/4.0/
// 
// File: Program.cs  Created: 2014-06-05@02:27
// Last modified: 2014-10-14@05:19 by Tim

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
        #region Configuration
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
        const double RampTime = 2; // seconds to reach full speed (acceleration)
        #endregion

        static OutputPort Led;
        static bool LedState;
        static IStepperMotorControl StepperM1M2;
        static IStepperMotorControl StepperM3M4;
        static readonly Random randomGenerator = new Random();

        public static void Main()
            {
            // First we set up an LED so we can see what's happening
            Led = new OutputPort(Pins.ONBOARD_LED, false);

            // Now for the shield-specific setup.

            #region Adaruit shield setup
            var adafruitMotorShieldV2 = new MotorShield();  // use shield at default I2C address.
            adafruitMotorShieldV2.InitializeShield();
            #endregion

            // The shield details are abstracted from the motor control, so whatever shield we have,
            // we just ask it for an IStepperMotorControl, specifying the number of microsteps and the
            // output numbers of the two motor phases.
            StepperM1M2 = adafruitMotorShieldV2.GetStepperMotor(MicrostepsPerStep, 1, 2);
            StepperM3M4 = adafruitMotorShieldV2.GetStepperMotor(MicrostepsPerStep, 3, 4);

            // Create the stepper motor axes and link them to the Adafruit driver.
            // On the first axis, we use the optional step callback to wire up a diagnostic LED.
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

            // Now we subscribe to the MotorStopped event on each axis. When the event fires, 
            // we start the axis going again with a new random target position.
            axis1.MotorStopped += HandleAxisStoppedEvent;
            axis2.MotorStopped += HandleAxisStoppedEvent;

            // We need to call our event handler once manually to get things going.
            HandleAxisStoppedEvent(axis1);
            //HandleAxisStoppedEvent(axis2);

            // Finally, we sleep forever as there is nothing else to do.
            // The motors continue to work in the background.
            Thread.Sleep(Timeout.Infinite);
            }

        /// <summary>
        /// Handles the axis stopped event for an axis.
        /// Picks a new random position and starts a new move.
        /// </summary>
        /// <param name="axis">The axis that has stopped.</param>
        static void HandleAxisStoppedEvent(AcceleratingStepperMotor axis)
            {
            Thread.Sleep(3000); // Wait a short time before starting the next move.
            var randomTarget = randomGenerator.Next(LimitOfTravel);
            Trace.Print("Starting move to " + randomTarget.ToString());
            axis.MoveToTargetPosition(randomTarget);
            }

        /// <summary>
        /// Toggles the Netduino's on-board led.
        /// </summary>
        /// <param name="direction">The direction.</param>
        static void UpdateDiagnosticLed(int direction)
            {
            LedState = !LedState;
            Led.Write(LedState);
            }
        }
    }
