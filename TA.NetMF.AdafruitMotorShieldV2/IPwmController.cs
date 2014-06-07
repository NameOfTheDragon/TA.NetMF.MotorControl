namespace TA.NetMF.AdafruitMotorShieldV2
    {
    internal interface IPwmController
        {
        /// <summary>
        ///   Sets the PWM waveform period and duration.
        /// </summary>
        /// <param name="channel">The PWM channel number.</param>
        /// <param name="period">The period, that is, the time to complete one complete cycle of the waveform.</param>
        /// <param name="duration">
        ///   The duration, that is, the length of the active part of the duty cycle, must be less than or
        ///   equal to <paramref name="period" />.
        /// </param>
        void SetPeriodAndDuration(uint channel, uint period, uint duration);
        }
    }