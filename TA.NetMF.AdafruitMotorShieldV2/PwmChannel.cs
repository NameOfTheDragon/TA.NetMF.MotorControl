// This file is part of the TA.NetMF.MotorControl project
// 
// Copyright © 2014 Tigra Astronomy, all rights reserved.
// This source code is licensed under Creative Commons Attribution International 4.0 license
// http://creativecommons.org/licenses/by/4.0/
// 
// File: PwmChannel.cs  Created: 2014-06-07@15:58
// Last modified: 2014-06-07@16:28 by Tim

using System;
using System.Collections;

namespace TA.NetMF.AdafruitMotorShieldV2
    {
    /// <summary>
    ///   Class PwmChannel. Represents a single channel of the PCA9685 PWM controller.
    ///   Attribution: This code is based (albeit in a simplified form) on Microsoft.SPOT.Hardware.PWM
    /// </summary>
    public sealed class PwmChannel
        {
        public enum ScaleFactor : uint
            {
            Milliseconds = 1000U,
            Microseconds = 1000000U,
            Nanoseconds = 1000000000U,
            }

        static readonly ArrayList AllocatedChannels = new ArrayList();

        readonly uint channel; // The base address of the channel's registers.

        uint duration; // The duration for which the PWM signal is active.
        uint period; // The PWM waveform period, 1/frequency
        ScaleFactor scale;
        IPwmController controller;

        internal PwmChannel(IPwmController controller, uint channel, double frequencyHz, double dutyCycle)
            {
            this.channel = channel;
            this.controller = controller;
            period = PeriodFromFrequency(frequencyHz, out scale);
            duration = DurationFromDutyCycleAndPeriod(dutyCycle, period);
            try
                {
                Init();
                Commit();
                }
            catch
                {
                Dispose(false);
                }
            }

        internal PwmChannel(IPwmController controller, uint channel, uint period, uint duration, ScaleFactor scale)
            {
            this.channel = channel;
            this.period = period;
            this.duration = duration;
            this.scale = scale;
            this.controller = controller;
            try
                {
                AllocateChannel(channel);
                Init();
                Commit();
                }
            catch
                {
                Dispose(false);
                }
            }

        public double Frequency
            {
            get { return FrequencyFromPeriod(period, scale); }
            set
                {
                var dutyCycle = DutyCycle;
                period = PeriodFromFrequency(value, out scale);
                duration = DurationFromDutyCycleAndPeriod(dutyCycle, period);
                Commit();
                }
            }

        public double DutyCycle
            {
            get { return DutyCycleFromDurationAndPeriod(period, duration); }
            set
                {
                duration = DurationFromDutyCycleAndPeriod(value, period);
                Commit();
                }
            }

        public uint Period
            {
            get { return period; }
            set
                {
                period = value;
                Commit();
                }
            }

        public uint Duration
            {
            get { return duration; }
            set
                {
                duration = value;
                Commit();
                }
            }

        public ScaleFactor Scale
            {
            get { return scale; }
            set
                {
                scale = value;
                Commit();
                }
            }

        /// <summary>
        ///   Allocates the channel after ensuring that it is not already in use.
        ///   Channel instances must be disposed before they can be re-used.
        /// </summary>
        /// <param name="channel">The channel base address.</param>
        /// <exception cref="InvalidOperationException">Thrown if there is already an instance of this channel.</exception>
        void AllocateChannel(uint channel)
            {
            if (AllocatedChannels.Contains(channel))
                throw new InvalidOperationException("Channel with base address " + channel + " is already allocated");
            AllocatedChannels.Add(channel);
            }

        ~PwmChannel()
            {
            Dispose(false);
            }

        public void Dispose()
            {
            Dispose(true);
            }

        public void Start() {}

        public void Stop() {}

        protected void Dispose(bool disposing)
            {
            try
                {
                Stop();
                }
            catch {}
            finally
                {
                Uninit();
                }
            }

        protected void Commit()
            {
            
            }

        protected void Init()
            {
            AllocateChannel(channel);
            }

        protected void Uninit()
            {
            DeallocateChannel();
            }

        void DeallocateChannel()
            {
            if (AllocatedChannels != null && AllocatedChannels.Contains(channel))
                AllocatedChannels.Remove(channel);
            }

        static uint PeriodFromFrequency(double f, out ScaleFactor scale)
            {
            if (f >= 1000.0)
                {
                scale = ScaleFactor.Nanoseconds;
                return (uint)(1000000000.0/f + 0.5);
                }
            if (f >= 1.0)
                {
                scale = ScaleFactor.Microseconds;
                return (uint)(1000000.0/f + 0.5);
                }
            scale = ScaleFactor.Milliseconds;
            return (uint)(1000.0/f + 0.5);
            }

        static uint DurationFromDutyCycleAndPeriod(double dutyCycle, double period)
            {
            if (period <= 0.0)
                throw new ArgumentException();
            if (dutyCycle < 0.0)
                return 0U;
            if (dutyCycle > 1.0)
                return 1U;
            return (uint)(dutyCycle*period);
            }

        static double FrequencyFromPeriod(double period, ScaleFactor scale)
            {
            return (double)scale/period;
            }

        static double DutyCycleFromDurationAndPeriod(double period, double duration)
            {
            if (period <= 0.0)
                throw new ArgumentException();
            if (duration < 0.0)
                return 0.0;
            if (duration > period)
                return 1.0;
            return duration/period;
            }
        }
    }
