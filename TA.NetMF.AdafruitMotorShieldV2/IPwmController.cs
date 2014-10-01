// This file is part of the TA.NetMF.MotorControl project
// 
// Copyright © 2014 Tigra Astronomy, all rights reserved.
// This source code is licensed under Creative Commons Attribution International 4.0 license
// http://creativecommons.org/licenses/by/4.0/
// 
// File: IPwmController.cs  Created: 2014-06-07@16:50
// Last modified: 2014-09-29@04:37 by Tim

namespace TA.NetMF.AdafruitMotorShieldV2
    {
    internal interface IPwmController
        {
        /// <summary>
        ///   Gets the output modulation frequency, in Hertz.
        ///   The output modulation frequency is the frequency with which the PWN channels complete
        ///   one on/off cycle.
        /// </summary>
        /// <value>The output modulation frequency in Hertz.</value>
        double OutputModulationFrequencyHz { get; }

        /// <summary>
        ///   Configures the specified PWM channel with the specified duty cycle.
        /// </summary>
        /// <param name="channel">The channel number (0-based).</param>
        /// <param name="dutyCycle">The duty cycle as a fraction of unity.</param>
        void ConfigureChannelDutyCycle(uint channel, double dutyCycle);

        PwmChannel GetPwmChannel(uint channel);
        }
    }
