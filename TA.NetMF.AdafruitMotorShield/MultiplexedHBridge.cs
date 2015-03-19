// This file is part of the TA.NetMF.MotorControl project
// 
// Copyright © 2014-2015 Tigra Astronomy, all rights reserved.
// This source code is licensed under the MIT License, see http://opensource.org/licenses/MIT
// 
// File: MultiplexedHBridge.cs  Created: 2015-01-26@16:47
// Last modified: 2015-01-29@01:59 by Tim

using System;
using Microsoft.SPOT.Hardware;
using TA.NetMF.Motor;

namespace TA.NetMF.ShieldDriver
    {
    /// <summary>
    ///   Class MultiplexedHBridge. Each instance corresponds to one motor winding, i.e. a single DC
    ///   motor or one phase of a stepper motor.
    ///   <para>
    ///     Models Adafruit's peculiar control method on version 1 of their motor control shield.
    ///     The shield has 2 x L293D H-Bridge motor drivers, each controlling 2 motor windings. Each
    ///     winding can drive a single DC motor, or one phase of a stepper motor. The motor coils
    ///     are named M1, M2, M3 and M4 on the PCB silk screen. The L293D chips are controlled by a
    ///     74HCT595 8-bit parallel output register which is accessed via a serial shift register.
    ///     Presumably this was done to use as few digital output pins as possible. The 74HC595
    ///     requires a total of 4 output pins for Serial Clock, Serial Data, Latch and Output
    ///     Enable. Data must be manually clocked into the chip using the Serial Data and Serial
    ///     Clock pins. When enough bits have been shiften into the shift register, they can be
    ///     transferred to the parallel output register using the Latch pin (transfer occurs on the
    ///     rising edge). The Output Enable pin enables or disables the output drivers and is active
    ///     low.
    ///   <para>
    ///     The Output Enable pins of the L293D chips are connected directly to digital outputs that
    ///     correspond to PWM outputs from the host microprocessor. The intention is to use the PWM
    ///     generators to modulate the motor circuitry so that the power output can be controlled,
    ///     although if it is only required to run the motor stopped or at full power, then digital
    ///     outputs could be used directly. In this implementation, PWM outputs are used to provide
    ///     variable speed control. The L293D data sheet suggests that 5KHz is an appropriate
    ///     maximum PWM frequency.
    ///   </para>
    /// <para>
    /// When controlling individual motor windings, care must be taken not to disturb the settings
    /// of the othe motors. The necessary behaviours are modelled in the 
    /// <see cref="SerialShiftRegister"/> and <see cref="ShiftRegisterOperation"/> classes and the 
    /// <see cref="SerialShiftRegister.WriteTransaction"/> method.
    /// </para>
    /// </summary>
    internal class MultiplexedHBridge : HBridge
        {
        ShiftRegisterOperation[] directionTransaction = new ShiftRegisterOperation[2];
        readonly ushort directionA;
        readonly ushort directionB;
        readonly ShiftRegisterOperation[] forwardTransaction;
        readonly SerialShiftRegister outputShiftRegister;
        readonly ShiftRegisterOperation[] brakeTransaction;
        readonly ShiftRegisterOperation[] reverseTransaction;
        readonly PWM speedControl;
        ShiftRegisterOperation[] releaseTransaction;
        int pwmFrequency = 1000;

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
            brakeTransaction = new[]
                {
                new ShiftRegisterOperation(directionA, true),
                new ShiftRegisterOperation(directionB, true)
                };
            releaseTransaction = new[]
                {
                new ShiftRegisterOperation(directionA, false),
                new ShiftRegisterOperation(directionB, false)
                };

            InitializeOutputs(); // Ensure the outputs are in a sane, safe state.
            }

        void InitializeOutputs()
            {
            // base duty will start at 0.0, i.e. magnitude 0, polarity true (forwards)
            speedControl.DutyCycle = 0.5;
            speedControl.Frequency = pwmFrequency;
            speedControl.DutyCycle = 0.0;
            speedControl.Start();
            outputShiftRegister.WriteTransaction(forwardTransaction);
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
                {
                outputShiftRegister.WriteTransaction(polarity ? forwardTransaction : reverseTransaction);
                }
            speedControl.DutyCycle = magnitude;
            }

        /// <summary>
        /// Releases the motor torque such that the motor is no longer driven and can idle freely.
        /// This is achieved by completely disabling the motor driver circuit.
        /// </summary>
        public override void ReleaseTorque()
            {
            base.ReleaseTorque();
            outputShiftRegister.WriteTransaction(releaseTransaction);
            }

        /// <summary>
        /// Applies a passive brake to the motor winding. This works by driving both motor outputs
        /// with the same polarity and enabling the output drivers. This should clamp the voltage
        /// across the coil to zero. This is the best approach for the L293D driver, but anecdotal
        /// evidence suggests that it is not very effective. This type of braking is dynamic and
        /// will not produce any 'holding torque'.
        /// </summary>
        public override void ApplyBrake()
            {
            base.ApplyBrake();
            outputShiftRegister.WriteTransaction(brakeTransaction);
            speedControl.DutyCycle = +1.0;
            }
        }
    }
