// This file is part of the TA.NetMF.MotorControl project
// 
// Copyright © 2014-2015 Tigra Astronomy, all rights reserved.
// This source code is licensed under the MIT License, see http://opensource.org/licenses/MIT
// 
// File: AdafruitV1MotorShield.cs  Created: 2015-01-16@17:30
// Last modified: 2015-01-16@18:05 by Tim

using System;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware.NetduinoPlus;
using TA.NetMF.Motor;

namespace TA.NetMF.ShieldDriver.AdafruitV1
	{
	/// <summary>
	/// Class AdafruitV1MotorShield. This class cannot be inherited. Provides access to the resources of the
	/// Adafruit Motor Shield (version 1).
	/// <image url="$(ProjectDir)\Datasheet\mshieldv12schem.png" />
	/// </summary>
	public sealed class AdafruitV1MotorShield
		{
		Octet latchState; // The last octet written to the latch.
		readonly OutputPort clock; // clocks the shift register
		readonly OutputPort data; // Writes a data bit to the shift register
		readonly OutputPort enable; // Enables the latch outputs.
		readonly OutputPort latch; // Latches the new data from the shift register into the latch output register
		SerialShiftRegister serialShiftRegister;

		/// <summary>
		///   Initializes a new instance of the <see cref="AdafruitV1MotorShield" /> class.
		/// </summary>
		/// <param name="latch">The latch store register clock pin (STCP).</param>
		/// <param name="enable">The latch output enable pin (OE).</param>
		/// <param name="data">The latch serial data pin (DS).</param>
		/// <param name="clock">The latch shift register clock pin (SHCP).</param>
		public AdafruitV1MotorShield(OutputPort latch, OutputPort enable, OutputPort data, OutputPort clock)
			{
			this.latch = latch;
			this.enable = enable;
			this.data = data;
			this.clock = clock;
			serialShiftRegister = new SerialShiftRegister(enable, clock, data, latch);
			}

        /// <summary>
        /// Initializes a new instance of the <see cref="AdafruitV1MotorShield"/> class using the default hardware outputs.
        /// </summary>
	    public AdafruitV1MotorShield()
	        {
            latch = new OutputPort(Pins.GPIO_PIN_D12, false);
            enable = new OutputPort(Pins.GPIO_PIN_D7, true);
            data = new OutputPort(Pins.GPIO_PIN_D8, false);
            clock = new OutputPort(Pins.GPIO_PIN_D4, false);
            serialShiftRegister = new SerialShiftRegister(enable, clock, data, latch);
	        }

		/// <summary>
		///   Resets the latch so that all motors and outputs are disabled. This method should be called
		///   before using any of the shield's resources to ensure that the hardware is in a consistent
		///   state.
		/// </summary>
		/// <exception cref="System.NotImplementedException"></exception>
		public void InitializeShield()
			{
			enable.High(); // disables the latch outputs.
			serialShiftRegister.Write(Octet.Zero);
			enable.Low();   // Enables the parallel output register drivers.
			}

		/// <summary>
		///   Gets a stepper motor controller that performs 4 whole steps per stepping cycle.
		///   The phases specify which of the 4 motor outputs the stepper motor
		///   windings are connected to. The outputs M1, M2, M3 and M4 can be
		///   read from the silk screen of the shield.
		/// </summary>
		/// <param name="phase1">The output number (M1, M2, M3 or M4) that the first motor phase is connected to.</param>
		/// <param name="phase2">The output number (M1, M2, M3 or M4) that the  second motor phase is connected to.</param>
		/// <returns>
		///   An implementation of <see cref="IStepSequencer" /> that can control the specified motor windings in
		///   whole steps.
		/// </returns>
		public IStepSequencer GetFullSteppingStepperMotor(int phase1, int phase2)
			{
			return GetMicrosteppingStepperMotor(4, phase1, phase2);
			}

		/// <summary>
		///   Gets a stepper motor controller that performs 8 half steps per stepping cycle.
		///   The phases specify which of the 4 motor outputs the stepper motor
		///   windings are connected to. The outputs M1, M2, M3 and M4 can be
		///   read from the silk screen of the shield.
		/// </summary>
		/// <param name="phase1">The output number (M1, M2, M3 or M4) that the first motor phase is connected to.</param>
		/// <param name="phase2">The output number (M1, M2, M3 or M4) that the  second motor phase is connected to.</param>
		/// <returns>
		///   An implementation of <see cref="IStepSequencer" /> that can control the specified motor windings in half
		///   steps.
		/// </returns>
		public IStepSequencer GetHalfSteppingStepperMotor(int phase1, int phase2)
			{
			return GetMicrosteppingStepperMotor(8, phase1, phase2);
			}

		/// <summary>
		///   Gets a stepper motor with the specified number of microsteps. The
		///   phases specify which of the 4 motor outputs the stepper motor
		///   windings are connected to. The outputs M1, M2, M3 and M4 can be
		///   read from the silk screen of the shield.
		/// </summary>
		/// <param name="microsteps">The number of microsteps per stepping cycle. </param>
		/// <param name="phase1">The output number (M1, M2, M3 or M4) that the first motor phase is connected to.</param>
		/// <param name="phase2">The output number (M1, M2, M3 or M4) that the  second motor phase is connected to.</param>
		/// <returns>
		///   An implementation of <see cref="IStepSequencer" />  that can control the specified motor windings in
		///   microsteps.
		/// </returns>
		public IStepSequencer GetMicrosteppingStepperMotor(int microsteps, int phase1, int phase2)
			{
			if (phase1 > 4 || phase1 < 1)
				throw new ArgumentOutOfRangeException("phase1", "must be 1, 2, 3 or 4");
			if (phase2 > 4 || phase2 < 1)
				throw new ArgumentOutOfRangeException("phase2", "must be 1, 2, 3 or 4");
			if (phase1 == phase2)
				throw new ArgumentException("The motor phases must be on different outputs");
			var hbridge1 = GetHbridge(phase1);
			var hbridge2 = GetHbridge(phase2);
			var motor = new TwoPhaseMicrosteppingSequencer(hbridge1, hbridge2, microsteps);
			return motor;
			}

		HBridge GetHbridge(int phase)
			{
			switch (phase)
				{
				case 1: // M1
					return new MultiplexedHBridge(GetPwm(PWMChannels.PWM_PIN_D11), serialShiftRegister, 2, 3);
				case 2:
					return new MultiplexedHBridge(GetPwm(PWMChannels.PWM_PIN_D3), serialShiftRegister, 1, 4);
				case 3:
					return new MultiplexedHBridge(GetPwm(PWMChannels.PWM_PIN_D5), serialShiftRegister, 0, 6);
				case 4:
					return new MultiplexedHBridge(GetPwm(PWMChannels.PWM_PIN_D6), serialShiftRegister, 5, 7);
				default:
					throw new ArgumentOutOfRangeException("phase", "Must be 1..4");
				}
			}

		PWM GetPwm(Cpu.PWMChannel pwmChannel)
			{
			var pwm = new PWM(pwmChannel, 64000.0, 0.0, false);
			return pwm;
			}

        /// <summary>
        /// Gets a DC motor.
        /// </summary>
        /// <param name="motorNumber">The motor number, corresponding to the motor numbers on the shield silk screen (M1, M2, M3, M4).</param>
        /// <returns>An <see cref="HBridge"/> instance configured to control the specified motor winding.</returns>
	    public HBridge GetDcMotor(int motorNumber)
	        {
	        return GetHbridge(motorNumber);
	        }
		}
	}
