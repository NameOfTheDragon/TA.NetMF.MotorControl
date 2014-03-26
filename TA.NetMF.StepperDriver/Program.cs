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

namespace TA.NetMF.StepperDriver
    {
    public class Program
        {
        const int StepsPerRevolution = 10000;
        static OutputPort Led;
        static bool LedState;
        static Random brandon = new Random();

        public static void Main()
            {
            Led = new OutputPort(Pins.ONBOARD_LED, false);
            var axis = new StepperAxisController(StepsPerRevolution, SimulateMicrostep)
                {
                MaximumSpeed = 800,
                Acceleration = 20.0f
                };
            axis.AxisStopped += HandleAxisStoppedEvent;
            HandleAxisStoppedEvent(axis);
            Thread.Sleep(Timeout.Infinite);
            }

        static void HandleAxisStoppedEvent(StepperAxisController axis)
            {
            Thread.Sleep(500);
            var randomTarget = brandon.Next(StepsPerRevolution);
            Trace.Print("Starting move to " + randomTarget.ToString());
            axis.RunToTarget(randomTarget);
            }

        static void SimulateMicrostep(int stepIndex)
            {
            LedState = (stepIndex & 0x01) == 1;
            Led.Write(LedState);
            }
        }
    }
