// This file is part of the TA.NetMF.MotorControl project
// 
// Copyright © 2014 Tigra Astronomy, all rights reserved.
// This source code is licensed under Creative Commons Attribution International 4.0 license
// http://creativecommons.org/licenses/by/4.0/
// 
// File: Pca9685.cs  Created: 2014-06-07@15:01
// Last modified: 2014-06-07@15:03 by Tim

namespace TA.NetMF.AdafruitMotorShieldV2
    {
    /// <summary>
    ///   Pca9685 constants.
    /// </summary>
    internal class Pca9685
        {
        #region MODE1 register bits
        public const ushort RestartBit = 7;
        public const ushort ExtClockBit = 6;
        public const ushort AutoIncrementBit = 5;
        public const ushort SleepBit = 4;
        public const ushort Sub1Bit = 3;
        public const ushort Sub2Bit = 2;
        public const ushort Sub3Bit = 1;
        public const ushort AllCallBit = 0;
        #endregion MODE1 register bits

        #region Register addresses
        public const byte ChannelBase = 0x06;
        public const byte Channel0OffHigh = 0x09;
        public const byte Channel0OffLow = 0x08;
        public const byte Channel0OnHigh = 0x07;
        public const byte Channel0OnLow = 0x06;
        public const byte Mode1Register = 0x00;
        public const byte Mode2Register = 0x01;
        public const byte PrescaleRegister = 0xFE;
        #endregion Register addresses

        #region Other constants
        public const int ClockRateKhz = 100;
        public const int I2CTimeout = 3000; // milliseconds
        public const int DefaultFrquencyHz = 160000;
        #endregion Other constants
        }
    }
