// This file is part of the TA.NetMF.MotorControl project
// 
// Copyright © 2014-2015 Tigra Astronomy, all rights reserved.
// This source code is licensed under the MIT License, see http://opensource.org/licenses/MIT
// 
// File: LedMotorSimulator.cs  Created: 2015-01-18@19:54
// Last modified: 2015-01-18@20:01 by Tim

using Microsoft.SPOT.Hardware;
using TA.NetMF.Motor;

namespace TA.NetMF.ShieldDriver.Simulator
    {
    /// <summary>
    /// Class LedMotorSimulator. Uses LEDs as simulated motor outputs.
    /// Useful for debugging and in situations where motors are not yet available (e.g. being shipped from China!!)
    /// </summary>
    public static class LedMotorSimulator
        {
        // ToDo - these methods can be factored out of the shield implementations and into the motor control code.
        public static IStepSequencer GetMicrosteppingStepperMotor(ushort microsteps, HBridge phase1, HBridge phase2)
            {
            return new TwoPhaseMicrosteppingSequencer(phase1, phase2, microsteps);
            }

        public static IStepSequencer GetHalfSteppingStepperMotor(HBridge phase1, HBridge phase2)
            {
            return GetMicrosteppingStepperMotor(8, phase1, phase2);
            }

        public static IStepSequencer GetFullSteppingStepperMotor(HBridge phase1, HBridge phase2)
            {
            return GetMicrosteppingStepperMotor(4, phase1, phase2);
            }

        public static IStepSequencer GetSimulatedStepperMotor(Cpu.Pin direction1, Cpu.PWMChannel power1, Cpu.Pin direction2, Cpu.PWMChannel power2)
            {
            var direction1Port = new OutputPort(direction1, false);
            var direction2Port = new OutputPort(direction2, false);
            var bridge1 = new HBridgeLedSimulator(direction1Port, power1);
            var bridge2 = new HBridgeLedSimulator(direction2Port, power2);
            return GetMicrosteppingStepperMotor(256, bridge1, bridge2);
            }

        /// <summary>
        /// Gets a simulated dc motor.
        /// </summary>
        /// <param name="directionIndicator">The direction indicator.</param>
        /// <param name="powerIndicator">The power indicator.</param>
        /// <returns>HBridge.</returns>
        public static HBridge GetDcMotor(Cpu.Pin directionIndicator, Cpu.PWMChannel powerIndicator)
            {
            var direction1Port = new OutputPort(directionIndicator, false);
            var bridge = new HBridgeLedSimulator(direction1Port, powerIndicator);
            return bridge;
            }
        }
    }
