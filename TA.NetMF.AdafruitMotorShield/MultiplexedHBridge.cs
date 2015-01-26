// This file is part of the TA.NetMF.MotorControl project
// 
// Copyright © 2014-2015 Tigra Astronomy, all rights reserved.
// This source code is licensed under the MIT License, see http://opensource.org/licenses/MIT
// 
// File: MultiplexedHBridge.cs  Created: 2015-01-16@17:30
// Last modified: 2015-01-19@00:09 by Tim

using System;
using Microsoft.SPOT.Hardware;
using TA.NetMF.Motor;

namespace TA.NetMF.AdafruitMotorShieldV1
    {
    /// <summary>
    ///   Class MultiplexedHBridge. Each instance corresponds to one motor winding.
    ///   <para>
    ///     Models Adafruit's peculiar control method on version 1 of their motor control shield.
    ///     The shield has 2 x L293D H-Bridge motor drivers, each controlling 2 motor windings. Each
    ///     winding can drive a single DC motor, or one phase of a stepper motor. The motor coils
    ///     are named M1, M2, M3 and M4 on the PCB silk screen. The L293D chips are controlled by a
    ///     74HCT595 8-bit data register which is accessed via a serial shift register. Presumably
    ///     this was done to use as few digital output pins as possible. The 74HC595 requires a
    ///     total of 4 output pins as follows:
    ///     <list type="table">
    ///       <listheader>
    ///         <term>Name</term>
    ///         <description>Usage</description>
    ///       </listheader>
    ///       <item>
    ///         <term>Serial clock</term>
    ///         <description>
    ///           Clocks the data bit into the shift register on each positive
    ///           transition.
    ///         </description>
    ///       </item>
    ///       <item>
    ///         <term>Serial Data</term>
    ///         <description>The bit being shifted into the shift register input.</description>
    ///       </item>
    ///       <item>
    ///         <term>Latch</term>
    ///         <description>
    ///           Transfers the shift register into the parallel output register and latches the
    ///           data there. This occurs on a positive transition.
    ///         </description>
    ///       </item>
    ///       <item>
    ///         <term>Output Enable</term>
    ///         <description>
    ///           Active low; enables the output drivers of the tri-state parallel output
    ///           register.
    ///         </description>
    ///       </item>
    ///     </list>
    ///   </para>
    ///   <para>
    ///     The Output Enable pins of the L293D chips are connected directly to digital outputs that
    ///     correspond to PWM outputs from the host microprocessor. The intention is to use the PWM
    ///     generators to modulate the motor circuitry so that the power output can be controlled,
    ///     although if it is only required to run the motor stopped or at full power, then digital
    ///     outputs could be used directly. In this implementation, PWM outputs are used to provide
    ///     variable speed control. The L293D data sheet suggests that 5KHz is an appropriate
    ///     maximum PWM frequency.
    ///   </para>
    /// </summary>
    internal class MultiplexedHBridge : HBridge
        {
        ShiftRegisterOperation[] directionTransaction = new ShiftRegisterOperation[2];
        readonly ushort directionA;
        readonly ushort directionB;
        readonly ShiftRegisterOperation[] forwardTransaction;
        readonly SerialShiftRegister outputShiftRegister;
        readonly ShiftRegisterOperation[] reverseTransaction;
        readonly PWM speedControl;

        /// <summary>
        ///   Initializes a new instance of the <see cref="MultiplexedHBridge" /> class.
        /// </summary>
        /// <param name="speedControl">The PWM channel that will control the output power (5KHz max).</param>
        /// <param name="outputShiftRegister">The output shift register that controls the L293D H-Bridge.</param>
        /// <param name="directionA">The bit number in the shift register output that controls IN1 on the L293D.</param>
        /// <param name="directionB">The bit number in the shift register output that controls IN2 on the L293D.</param>
        public MultiplexedHBridge(PWM speedControl,
            SerialShiftRegister outputShiftRegister,
            ushort directionA,
            ushort directionB)
            {
            this.speedControl = speedControl;
            speedControl.Start();
            this.outputShiftRegister = outputShiftRegister;
            this.directionA = directionA;
            this.directionB = directionB;
            // For efficiency, initialise some direction transactions ready for use later.
            forwardTransaction = new[]
                {
                new ShiftRegisterOperation(directionA, true),
                new ShiftRegisterOperation(directionB, false)
                };
            reverseTransaction = new[]
                {
                new ShiftRegisterOperation(directionA, false),
                new ShiftRegisterOperation(directionB, true)
                };
            }

        public override void SetOutputPowerAndPolarity(double duty)
            {
            var magnitude = Math.Abs(duty);
            var polarity = duty >= 0.0;
            SetOutputPowerAndPolarity(magnitude, polarity);
            base.SetOutputPowerAndPolarity(duty);
            }

        void SetOutputPowerAndPolarity(double magnitude, bool polarity)
            {
            if (polarity != Polarity)
                speedControl.DutyCycle = 0.0;
            outputShiftRegister.WriteTransaction(polarity ? forwardTransaction : reverseTransaction);
            speedControl.DutyCycle = magnitude;
            }
        }
    }
