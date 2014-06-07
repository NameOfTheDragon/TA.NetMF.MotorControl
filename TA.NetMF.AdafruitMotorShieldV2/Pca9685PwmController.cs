// This file is part of the TA.NetMF.MotorControl project
// 
// Copyright © 2014 Tigra Astronomy, all rights reserved.
// This source code is licensed under Creative Commons Attribution International 4.0 license
// http://creativecommons.org/licenses/by/4.0/
// 
// File: Pca9685PwmController.cs  Created: 2014-06-06@18:06
// Last modified: 2014-06-07@16:45 by Tim

using System;
using System.Threading;
using Microsoft.SPOT.Hardware;

namespace TA.NetMF.AdafruitMotorShieldV2
    {
    internal class Pca9685PwmController : IPwmController
        {
        const int MaxChannel = 15;
        readonly ushort address;
        I2CDevice.Configuration i2CConfiguration;
        I2CDevice iicDevice;

        public Pca9685PwmController(ushort iicAddress)
            {
            address = iicAddress;
            InitializeI2CDevice();
            }

        public PwmChannel GetPwmChannel(uint channel)
            {
            if (channel > MaxChannel)
                throw new ArgumentOutOfRangeException("channel", "Maximum channel is 15");
            return new PwmChannel(this, channel, 100000, 0.0);
            }

        public void SetPeriodAndDuration(uint channel, uint period, uint duration) {}

        void InitializeI2CDevice()
            {
            i2CConfiguration = new I2CDevice.Configuration(address, Pca9685.ClockRateKhz);
            iicDevice = new I2CDevice(i2CConfiguration);
            SetFrequency(); // ToDo - pass in constructor?
            AutoIncrement(true);
            }

        void AutoIncrement(bool enable)
            {
            if (enable)
                SetBitInRegister(Pca9685.Mode1Register, Pca9685.AutoIncrementBit);
            else
                ClearBitInRegister(Pca9685.Mode1Register, Pca9685.AutoIncrementBit);
            }

        void SetBitInRegister(byte register, ushort bitNumber)
            {
            var bitSetMask = 2 ^ bitNumber;
            var registerValue = (int)ReadRegister(register);
            registerValue |= bitSetMask;
            WriteRegister(register, (byte)registerValue);
            }

        void ClearBitInRegister(byte register, ushort bitNumber)
            {
            var bitClearMask = 0xFF - 2 ^ bitNumber;
            var registerValue = (int)ReadRegister(register);
            registerValue &= bitClearMask;
            WriteRegister(register, (byte)registerValue);
            }

        public void Reset()
            {
            var operations = new I2CDevice.I2CTransaction[1];
            byte[] writeBuffer = {Pca9685.Mode1Register, 0x00};
            operations[0] = I2CDevice.CreateWriteTransaction(writeBuffer);
            iicDevice.Execute(operations, Pca9685.I2CTimeout);
            }

        void WriteRegister(byte registerAddress, byte data)
            {
            byte[] writeBuffer = {registerAddress, data};
            var operations = new I2CDevice.I2CTransaction[1];
            operations[0] = I2CDevice.CreateWriteTransaction(writeBuffer);
            iicDevice.Execute(operations, Pca9685.I2CTimeout);
            }

        void SetFrequency(int frequencyHz = Pca9685.DefaultFrquencyHz)
            {
            var prescale = (byte)(25000000/4096/frequencyHz);
            SetPrescale(prescale);
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
            Thread.Sleep(0); // Let other threads run while we are waiting.
            RestartPwm();
            }

        /// <summary>
        ///   Restarts the PWM counters after the device has been in sleep mode.
        /// </summary>
        void RestartPwm()
            {
            var mode = ReadRegister(Pca9685.Mode1Register);
            if ((mode & 0x80) != 0)
                {
                var newMode = (byte)(mode | 0x80);
                WriteRegister(Pca9685.Mode1Register, newMode);
                }
            }

        byte ReadRegister(byte registerAddress)
            {
            byte[] writeBuffer = {registerAddress};
            var readBuffer = new byte[1];
            var operations = new I2CDevice.I2CTransaction[2];
            operations[0] = I2CDevice.CreateWriteTransaction(writeBuffer);
            operations[1] = I2CDevice.CreateReadTransaction(readBuffer);
            iicDevice.Execute(operations, Pca9685.I2CTimeout);
            return readBuffer[0];
            }
        }
    }
