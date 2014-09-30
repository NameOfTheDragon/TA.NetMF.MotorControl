// This file is part of the TA.NetMF.MotorControl project
// 
// Copyright © 2014 Tigra Astronomy, all rights reserved.
// This source code is licensed under Creative Commons Attribution International 4.0 license
// http://creativecommons.org/licenses/by/4.0/
// 
// File: Pca9685PwmController.cs  Created: 2014-06-06@18:06
// Last modified: 2014-09-30@05:04 by Tim

using System;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using Math = System.Math;

namespace TA.NetMF.AdafruitMotorShieldV2
    {
    internal class Pca9685PwmController : IPwmController
        {
        const int MaxChannel = 15;
        const int PwmCounterCycle = 4096;
        readonly ushort i2cAddress;
        I2CDevice.Configuration i2CConfiguration;
        I2CDevice i2cDevice;
        double outputModulationFrequencyHz;

        /// <summary>
        ///   Initializes a new instance of the <see cref="Pca9685PwmController" /> class at the specified I2C address
        ///   and with the specified output modulation frequency.
        /// </summary>
        /// <param name="i2cAddress">The base I2C address for the device.</param>
        /// <param name="outputModulationFrequencyHz">
        ///   The output modulation frequency of all 16 PWM channels, in Hertz (cycles per second).
        ///   If not specified, then the default value of 1.6 KHz is used. The theoretical range is
        ///   approximately 24 Hz to 1743 Hz, but extremes should be avoided if possible.
        /// </param>
        public Pca9685PwmController(ushort i2cAddress,
            double outputModulationFrequencyHz = Pca9685.DefaultOutputModulationFrequency)
            {
            this.i2cAddress = i2cAddress;
            InitializeI2CDevice();
            Reset();
            SetOutputModulationFrequency(outputModulationFrequencyHz);
            }

        public PwmChannel GetPwmChannel(uint channel)
            {
            if (channel > MaxChannel)
                throw new ArgumentOutOfRangeException("channel", "Maximum channel is 15");
            return new PwmChannel(this, channel, 0.0);
            }

        public double OutputModulationFrequencyHz { get { return outputModulationFrequencyHz; } }

        public void ConfigureChannelDutyCycle(uint channel, double dutyCycle)
            {
            if (dutyCycle >= 1.0)
                {
                SetFullOn(channel);
                return;
                }
            if (dutyCycle <= 0.0)
                {
                SetFullOff(channel);
                return;
                }
            uint onCount = 0;
            var offCount = (uint)Math.Floor(PwmCounterCycle*dutyCycle);
            if (offCount <= onCount)
                offCount = onCount + 1; // The two counts may not be the same value
            var registerOffset = (byte)(6 + (4*channel));
            WriteConsecutiveRegisters(
                registerOffset,
                (byte)onCount,
                (byte)(onCount >> 8),
                (byte)offCount,
                (byte)(offCount >> 8));
            }

        /// <summary>
        ///   Sets the channel to 0% duty cycle.
        /// </summary>
        /// <param name="channel">The channel number (0-based).</param>
        void SetFullOff(uint channel)
            {
            var registerOffset = (byte)(9 + (4*channel));
            WriteRegister(registerOffset, 0x10);
            }

        /// <summary>
        ///   Sets the channel to 100% duty cycle.
        /// </summary>
        /// <param name="channel">The channel number (0-based).</param>
        void SetFullOn(uint channel)
            {
            var registerOffset = (byte)(7 + (4*channel));
            WriteRegister(registerOffset, 0x10);
            }

        void InitializeI2CDevice()
            {
            i2CConfiguration = new I2CDevice.Configuration(i2cAddress, Pca9685.ClockRateKhz);
            i2cDevice = new I2CDevice(i2CConfiguration);
            }

        void SetBitInRegister(byte registerOffset, ushort bitNumber)
            {
            var bitSetMask = 0x01 << bitNumber;
            var registerValue = (int)ReadRegister(registerOffset);
            registerValue |= bitSetMask;
            WriteRegister(registerOffset, (byte)registerValue);
            }

        void ClearBitInRegister(byte registerOffset, ushort bitNumber)
            {
            var bitClearMask = 0xFF ^ (0x01 << bitNumber);
            var registerValue = (int)ReadRegister(registerOffset);
            registerValue &= bitClearMask;
            WriteRegister(registerOffset, (byte)registerValue);
            }

        /// <summary>
        ///   Resets the PCA9685 PWM controller into a known starting state.
        /// </summary>
        public void Reset()
            {
            WriteRegister(Pca9685.Mode1Register, 0x00);
            SetAllChannelsOff();
            }

        /// <summary>
        ///   Sets all channels to 0% duty cycle.
        /// </summary>
        void SetAllChannelsOff()
            {
            // Sets teh LED_FULL_OFF bit in each and every channel, which sets the duty cycle to 0%.
            WriteConsecutiveRegisters(Pca9685.AllChannelsBaseRegister, 0x00, 0x00, 0x01, 0x10);
            }

        void WriteRegister(byte registerOffset, byte data)
            {
            Trace.Print("Register " + registerOffset.ToString() + " ==> " + data.ToString());
            byte[] writeBuffer = {registerOffset, data};
            var operations = new I2CDevice.I2CTransaction[1];
            operations[0] = I2CDevice.CreateWriteTransaction(writeBuffer);
            i2cDevice.Execute(operations, Pca9685.I2CTimeout);
            }

        void WriteConsecutiveRegisters(byte startRegisterOffset, params byte[] values)
            {
            if (BitIsClear(Pca9685.Mode1Register, Pca9685.AutoIncrementBit))
                SetAutoIncrement(true);
            var bufferSize = values.Length + 1;
            var writeBuffer = new byte[bufferSize];
            writeBuffer[0] = startRegisterOffset;
            var bufferIndex = 0;
            foreach (var value in values)
                writeBuffer[++bufferIndex] = value;
            var transactions = new I2CDevice.I2CTransaction[]
                {
                I2CDevice.CreateWriteTransaction(writeBuffer)
                };
            i2cDevice.Execute(transactions, Pca9685.I2CTimeout);
            }

        bool BitIsSet(byte registerOffset, ushort bitNumber)
            {
            var registerValue = ReadRegister(registerOffset);
            var bitTestMask = 0x01 << bitNumber;
            return (registerValue & bitTestMask) != 0;
            }

        public void SetOutputModulationFrequency(double frequencyHz = Pca9685.DefaultOutputModulationFrequency)
            {
            var computedPrescale = Math.Round(Pca9685.InternalOscillatorFrequencyHz/4096.0/frequencyHz) - 1;
            if (computedPrescale < 3.0 || computedPrescale > 255.0)
                throw new ArgumentOutOfRangeException("frequencyHz", "range 24 Hz to 1743 Hz");
            var prescale = (byte)computedPrescale;
            SetPrescale(prescale);
            outputModulationFrequencyHz = frequencyHz;
            }

        /// <summary>
        ///   Enables or disables the automatic increment mode.
        ///   When enabled, sequential register reads are possible.
        /// </summary>
        /// <param name="enabled">if set to <c>true</c> [enabled].</param>
        void SetAutoIncrement(bool enabled)
            {
            if (enabled)
                SetBitInRegister(Pca9685.Mode1Register, 5);
            else
                ClearBitInRegister(Pca9685.Mode1Register, 5);
            }

        /// <summary>
        ///   Sets the prescale divider without affecting any other mode settings.
        ///   This requires putting the PWM controller to sleep while the prescaler is changed, and
        ///   there will be a delay of at least 5uS to allow the oscillator to restart.
        /// </summary>
        /// <param name="prescale">The prescale.</param>
        void SetPrescale(byte prescale)
            {
            // The prescaler can only be set while the device is in SLEEP mode.
            var mode = ReadRegister(Pca9685.Mode1Register);
            var sleep = (byte)(mode & 0x7F | 0x10); // Set SLEEP mode and take care not to cause a restart (bit 7).
            WriteRegister(Pca9685.Mode1Register, sleep);
            WriteRegister(Pca9685.PrescaleRegister, prescale);
            // Now we have to restore the previous mode and wait at least 5 microseconds for the oscillator to restart.
            WriteRegister(Pca9685.Mode1Register, mode);
            Thread.Sleep(1); // Must wait at least 500 microseconds.
            WriteRegister(Pca9685.Mode1Register, (byte)(mode | 0xa1));
            RestartPwm();
            }

        /// <summary>
        ///   Restarts the PWM counters after the device has been in sleep mode.
        /// </summary>
        void RestartPwm()
            {
            var mode = ReadRegister(Pca9685.Mode1Register);
            if (BitIsClear(Pca9685.Mode1Register, Pca9685.RestartBit))
                return;
            SetBitInRegister(Pca9685.Mode1Register, Pca9685.RestartBit);
            }

        bool BitIsClear(byte registerOffset, ushort bitNumber)
            {
            return !BitIsSet(registerOffset, bitNumber);
            }

        byte ReadRegister(byte registerOffset)
            {
            byte[] writeBuffer = {registerOffset};
            var readBuffer = new byte[1];
            var operations = new I2CDevice.I2CTransaction[2];
            operations[0] = I2CDevice.CreateWriteTransaction(writeBuffer);
            operations[1] = I2CDevice.CreateReadTransaction(readBuffer);
            i2cDevice.Execute(operations, Pca9685.I2CTimeout);
            var result = readBuffer[0];
            Trace.Print("Register " + registerOffset.ToString() + " <== " + result.ToString());
            return result;
            }
        }
    }
