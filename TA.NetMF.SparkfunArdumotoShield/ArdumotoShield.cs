// This file is part of the TA.NetMF.MotorControl project
// 
// Copyright © 2014 Tigra Astronomy, all rights reserved.
// This source code is licensed under Creative Commons Attribution International 4.0 license
// http://creativecommons.org/licenses/by/4.0/
// 
// File: ArdumotoShield.cs  Created: 2014-06-05@02:27
// Last modified: 2014-06-05@12:23 by Tim

using TA.NetMF.Motor;

namespace TA.NetMF.SparkfunArdumotoShield
    {
    public sealed class ArdumotoShield
        {
        readonly SimpleHBridge MotorA;
        readonly SimpleHBridge MotorB;

        public ArdumotoShield(SimpleHBridge a, SimpleHBridge b)
            {
            MotorA = a;
            MotorB = b;
            }

        /// <summary>
        ///   Gets a stepper motor configured for the specified number of microsteps per whole step.
        /// </summary>
        /// <param name="microsteps">The microsteps.</param>
        /// <returns>TA.NetMF.Utils.IStepperMotorControl.</returns>
        public IStepperMotorControl GetStepperMotor(int microsteps)
            {
            return new MicrosteppingStepperMotor(MotorA, MotorB, microsteps);
            }
        }
    }
