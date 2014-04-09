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
using SecretLabs.NETMF.Hardware.NetduinoPlus;
using TA.AcceleratedStepperDriver;
using Math = System.Math;

namespace TA.NetMF.StepperDriver
    {
    public class Program
        {
        const int StepsPerRevolution = 10000;
        const int MicrostepsPerStep = 8;
        const double RampTime = 5;
        static OutputPort Led;
        static bool LedState;
        static Random brandon = new Random();
        static int ledThreshold;

        public static void Main()
            {
            Led = new OutputPort(Pins.ONBOARD_LED, false);
            ledThreshold = MicrostepsPerStep/2;
            var axis = new AcceleratingStepperMotor(StepsPerRevolution, SimulateMicrostep, MicrostepsPerStep)
                {
                MaximumSpeed = 500,
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
            var randomTarget = brandon.Next(StepsPerRevolution);
            Trace.Print("Starting move to " + randomTarget.ToString());
            axis.MoveToTargetPosition(randomTarget);
            }

        static void SimulateMicrostep(uint stepIndex)
            {
            LedState = (stepIndex >= ledThreshold);
            Led.Write(LedState);
            }
        }
    }
