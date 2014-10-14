// This file is part of the TA.NetMF.MotorControl project
// 
// Copyright © 2014 Tigra Astronomy, all rights reserved.
// This source code is licensed under Creative Commons Attribution International 4.0 license
// http://creativecommons.org/licenses/by/4.0/
// 
// File: StepperMotor.cs  Created: 2014-06-05@02:27
// Last modified: 2014-06-05@12:23 by Tim

using System;

namespace TA.NetMF.Motor
    {
    public class MicrosteppingStepperMotor : IStepperMotorControl
        {
        readonly int maxIndex;
        readonly HBridge phase1;
        readonly HBridge phase2;
        double[] inPhaseDutyCycle;
        int microsteps;
        double[] outOfPhaseDutyCycle;
        int phaseIndex;

        public MicrosteppingStepperMotor(HBridge phase1bridge, HBridge phase2bridge, int microsteps)
            {
            phase1 = phase1bridge;
            phase2 = phase2bridge;
            this.microsteps = microsteps;
            maxIndex = microsteps;
            ComputeMicrostepTables();
            phaseIndex = 0;
            }

        public void PerformMicrostep(int direction)
            {
            phaseIndex += direction;
            if (phaseIndex >= maxIndex)
                phaseIndex = 0;
            if (phaseIndex < 0)
                phaseIndex = maxIndex - 1;
            phase1.SetOutputPowerAndPolarity(inPhaseDutyCycle[phaseIndex]);
            phase2.SetOutputPowerAndPolarity(outOfPhaseDutyCycle[phaseIndex]);
            }

        public void ReleaseHoldingTorque()
            {
            phase1.SetOutputPowerAndPolarity(0.0);
            phase2.SetOutputPowerAndPolarity(0.0);
            }

        void ComputeMicrostepTables()
            {
            // This implementation prefers performance over memory footprint.
            var radiansPerIndex = (2*Math.PI)/(maxIndex-1);
            inPhaseDutyCycle = new double[maxIndex];
            outOfPhaseDutyCycle = new double[maxIndex];
            for (var i = 0; i < maxIndex; ++i)
                {
                var phaseAngle = i*radiansPerIndex;
                inPhaseDutyCycle[i] = Math.Sin(phaseAngle);
                outOfPhaseDutyCycle[i] = Math.Cos(phaseAngle);
                }
            }
        }
    }
