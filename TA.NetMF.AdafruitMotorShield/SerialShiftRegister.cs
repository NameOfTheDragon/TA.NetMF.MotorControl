// This file is part of the TA.NetMF.MotorControl project
// 
// Copyright © 2014-2015 Tigra Astronomy, all rights reserved.
// This source code is licensed under the MIT License, see http://opensource.org/licenses/MIT
// 
// File: SerialShiftRegister.cs  Created: 2015-01-17@21:41
// Last modified: 2015-01-17@23:35 by Tim

using Microsoft.SPOT.Hardware;
using TA.NetMF.Motor;

namespace TA.NetMF.AdafruitMotorShieldV1
    {
    /// <summary>
    ///   Class SerialShiftRegister.
    ///   Models an 8-bit output register fed by a serial shift register,
    ///   specifically a 74HCT595. All operations are thread-safe.
    /// </summary>
    internal class SerialShiftRegister
        {
        Octet outputs = Octet.Zero;
        readonly OutputPort latchPositiveEdge;
        readonly OutputPort outputEnableActiveLow;
        readonly OutputPort serialClockPositiveEdge;
        readonly OutputPort serialData;
        readonly object syncObject = new object();

        public SerialShiftRegister(OutputPort outputEnable,
            OutputPort serialClock,
            OutputPort serialData,
            OutputPort latch)
            {
            outputEnableActiveLow = outputEnable;
            serialClockPositiveEdge = serialClock;
            this.serialData = serialData;
            latchPositiveEdge = latch;
            }

        /// <summary>
        ///   Gets the state of the output pins.
        /// </summary>
        /// <value>The state of the output pins.</value>
        /// <remarks>If an update is in progress, blocks until the update is complete.</remarks>
        public Octet OutputState
            {
            get
                {
                lock (syncObject) // Don't allow return during an update
                    {
                    return outputs;
                    }
                }
            }

        void WriteOneBit(bool bit)
            {
            serialClockPositiveEdge.Write(false);
            serialData.Write(bit);
            serialClockPositiveEdge.Write(true);
            }

        void WriteOctet(Octet data)
            {
            for (int i = 7; i >= 0; i--)
                WriteOneBit(data[i]);
            }

        /// <summary>
        ///   Writes the specified data pattern and latches it onto the output pins.
        ///   The operation is thread-safe.
        /// </summary>
        /// <param name="data">
        ///   The bit pattern to be written.
        ///   Octet[0] corresponds to output Q0 and Octet[7] corresponds to output Q7.
        /// </param>
        public void Write(Octet data)
            {
            lock (syncObject) // prevent race condition
                {
                latchPositiveEdge.Write(false);
                WriteOctet(data);
                latchPositiveEdge.Write(true);
                outputs = data;
                }
            }

        /// <summary>
        ///   Sets or clears one or more output bits in a single, atomic thread-safe operation.
        /// </summary>
        /// <param name="operations">The bit operations to be written.</param>
        public void WriteTransaction(ShiftRegisterOperation[] operations)
            {
            lock (syncObject)
                {
                var targetValues = outputs;
                foreach (var shiftRegisterOperation in operations)
                    targetValues.SetBitValue(shiftRegisterOperation.BitNumber, shiftRegisterOperation.Value);
                WriteOctet(targetValues);
                }
            }
        }
    }
