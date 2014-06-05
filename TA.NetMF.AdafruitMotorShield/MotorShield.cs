// This file is part of the TA.NetMF.MotorControl project
// 
// Copyright © 2014 Tigra Astronomy, all rights reserved.
// This source code is licensed under Creative Commons Attribution International 4.0 license
// http://creativecommons.org/licenses/by/4.0/
// 
// File: MotorShield.cs  Created: 2014-06-05@02:27
// Last modified: 2014-06-05@12:23 by Tim

using Microsoft.SPOT.Hardware;
using TA.NetMF.Motor;

namespace TA.NetMF.AdafruitMotorShield
    {
    public sealed class MotorShield
        {
        readonly OutputPort clock; // clocks the shift register
        readonly OutputPort data; // Writes a data bit to the shift register
        readonly OutputPort enable; // Enables the latch outputs.
        readonly OutputPort latch; // Latches the new data from the shift register into the latch output register
        Octet latchState; // The last octet written to the latch.

        /// <summary>
        ///   Initializes a new instance of the <see cref="Stepper" /> class.
        /// </summary>
        /// <param name="latch">The latch store register clock pin (STCP).</param>
        /// <param name="enable">The latch output enable pin (OE).</param>
        /// <param name="data">The latch serial data pin (DS).</param>
        /// <param name="clock">The latch shift register clock pin (SHCP).</param>
        public MotorShield(OutputPort latch, OutputPort enable, OutputPort data, OutputPort clock)
            {
            this.latch = latch;
            this.enable = enable;
            this.data = data;
            this.clock = clock;
            Reset();
            }

        /// <summary>
        ///   Resets the latch so that all motors and outputs are disabled.
        /// </summary>
        /// <exception cref="System.NotImplementedException"></exception>
        public void Reset()
            {
            WriteLatch(Octet.Zero);
            enable.Low(); // disables the latch outputs.
            }

        /// <summary>
        ///   Clocks an octet into the latch shift register and then latches it onto the outputs.
        /// </summary>
        /// <param name="bits">The octet to write (MSB first).</param>
        /// <remarks>
        ///   The Adafruit motor shield uses a 74HCT595N 8-bit latch with serial input and parallel tri-state output.
        ///   This gains 8 bits of output while using only 4 output pins on the Netduino/Arduino, at the expense of
        ///   slightly more complicated management and slower throughput.
        /// </remarks>
        void WriteLatch(Octet bits)
            {
            latch.Low();
            data.Low();
            for (var i = 7; i >= 0; --i)
                {
                clock.Low();
                data.Write(bits[i]);
                clock.High();
                }
            latch.High();
            latchState = bits;
            }

        /// <summary>
        ///   Configures the motor shield for stepper motor control, using the supplied HBridge and returns the control interface.
        /// </summary>
        /// <param name="hbridge">The hbridge.</param>
        /// <returns>IStepperMotor.</returns>
        public IStepperMotorControl GetStepper(HBridge phase1bridge, HBridge phase2bridge, int microsteps)
            {
            return new MicrosteppingStepperMotor(phase1bridge, phase2bridge, microsteps);
            }
        }
    }
