// This file is part of the TA.NetMF.StepperDriver project
// 
// Copyright © 2014 TiGra Networks, all rights reserved.
// 
// File: Program.cs  Created: 2014-03-25@05:11
// Last modified: 2014-03-26@03:00 by Tim

using System;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;
using TA.AcceleratedStepperDriver;
using TA.SparkfunArdumotoShield;
using TA.NetMF.Utils;
using Math = System.Math;

namespace TA.NetMF.StepperDriver
{
    public class Program
    {
        const int LimitOfTravel = 50000;
        const int MicrostepsPerStep = 8;    // 4=full stepping; 8=half stepping; 9+=microstepping.
        const int MaxSpeed = 75;            // steps per second (Netduino Plus 2 can manage a few hundred)
        const double RampTime = 3.0;        // seconds to reach full speed (acceleration)

        static OutputPort Led;
        static bool LedState;
        static Random brandon = new Random();
        static IStepperMotorControl stepper;

        public static void Main()
        {
            /*
             * First we set up an LED so we can see what's happening, and a couple of PWM outputs.
             * The PWM waveform is fed to the ENABLE pins on the H-Bridge, so that the output current is switched on and off
             * very quickly. The total power to the motor winding is proportional to the duty cycle of the PWM waveform.
             * The frequency will probably need to be tuned to the characteristics of the circuitry. There doesn't seem to be
             * any point in running it faster than the H-Bridge can respond. On the device we tested (L298D) there appears
             * to be a typical maximum delay of about 4 microseconds which corresponds to about 250 KHz.
             */
            Led = new OutputPort(Pins.ONBOARD_LED, false);
            var pwmPhase1 = new Microsoft.SPOT.Hardware.PWM(PWMChannels.PWM_PIN_D5, 500000, 0.0, false); // Set the frequency and 0% duty
            var pwmPhase2 = new Microsoft.SPOT.Hardware.PWM(PWMChannels.PWM_PIN_D6, 500000, 0.0, false); // Set the frequency and 0% duty
            pwmPhase1.Start();
            pwmPhase2.Start();

            // Now for the shield-specific setup.

            #region Adaruit sield setup
            // Create the resources for the Adafruit motor shield, then finally create the motor driver itself.
            //var latchData = new OutputPort(Pins.GPIO_PIN_D8, false);
            //var latchClock = new OutputPort(Pins.GPIO_PIN_D4, false);
            //var latchStore = new OutputPort(Pins.GPIO_PIN_D12, false);
            //var latchEnable = new OutputPort(Pins.GPIO_PIN_D7, false);
            //var pwmPhase1 = new PWM(PWMChannels.PWM_PIN_D5, 1000000.0, 0.0, false); // 1MHz, 0% duty
            //var pwmPhase2 = new PWM(PWMChannels.PWM_PIN_D6, 1000000.0, 0.0, false); // 1MHz, 0% duty

            // Adafruit shield
            //var motorShield = new MotorShield(latchStore, latchEnable, latchData, latchClock);
            //stepper = motorShield.GetStepper(hbridgeB, MicrostepsPerStep);
            #endregion
            #region Sparkfun Ardumoto Shield setup
            // Sparkfun Ardumoto Shield

            var directionA = new OutputPort(Pins.GPIO_PIN_D12, false);
            var bridgeA = new SimpleHBridge(pwmPhase1, directionA);
            var directionB = new OutputPort(Pins.GPIO_PIN_D13, false);
            var bridgeB = new SimpleHBridge(pwmPhase2, directionB);

            var motorShield = new ArdumotoShield(bridgeA, bridgeB);
            #endregion Sparkfun Ardumoto Shield setup

            // The shield details are now abstracted from the motor control, so whatever shield we have, we just ask it for an IStepperMotorControl.
            stepper = motorShield.GetStepperMotor(MicrostepsPerStep);

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
