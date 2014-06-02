﻿// This file is part of the TA.NetMF.StepperDriver project
// 
// Copyright © 2014 TiGra Networks, all rights reserved.
// 
// File: Program.cs  Created: 2014-03-25@05:11
// Last modified: 2014-03-26@03:00 by Tim

using System;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware.NetduinoPlus;
using TA.AcceleratedStepperDriver;
//using TA.AdafruitMotorShield;
using TA.SparkfunArdumotoShield;
using TA.NetMF.Utils;
using Math = System.Math;

namespace TA.NetMF.StepperDriver
{
    public class Program
    {
        const int LimitOfTravel = 10000;
        const int MicrostepsPerStep = 32;
        const int MaxSpeed = 500;    // steps per second
        const double RampTime = 20;  // seconds to reach full speed
        static OutputPort Led;
        static bool LedState;
        static Random brandon = new Random();
        static IStepperMotorControl stepper;

        public static void Main()
        {
            Led = new OutputPort(Pins.ONBOARD_LED, false);

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

            // Sparkfun Ardumoto Shield
            var pwmPhase1 = new PWM(PWMChannels.PWM_PIN_D5, 1000.0, 0.0, false); // 1MHz, 0% duty
            var pwmPhase2 = new PWM(PWMChannels.PWM_PIN_D6, 1000.0, 0.0, false); // 1MHz, 0% duty
            var directionA = new OutputPort(Pins.GPIO_PIN_D12, false);
            var bridgeA = new SimpleHBridge(pwmPhase1, directionA);
            var directionB = new OutputPort(Pins.GPIO_PIN_D13, false);
            var bridgeB = new SimpleHBridge(pwmPhase2, directionB);

            var motorShield = new ArdumotoShield(bridgeA, bridgeB);
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
            //    {
            //    var randomSpeed = brandon.NextDouble()*axis.MaximumSpeed * 2 - axis.MaximumSpeed;
            //    if (Math.Abs(randomSpeed) <= 0.1)
            //        continue;
            //    axis.MoveAtRegulatedSpeed(randomSpeed);
            //    Thread.Sleep((int)(RampTime*2000));
            //    }
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
            stepper.PerformMicrostep();
            LedState = !LedState;
            Led.Write(LedState);
        }
    }
}
