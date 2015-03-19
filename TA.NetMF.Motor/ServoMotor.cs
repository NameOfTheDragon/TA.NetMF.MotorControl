// This file is part of the TA.NetMF.MotorControl project
// 
// Copyright � 2014-2015 Tigra Astronomy, all rights reserved.
// This source code is licensed under the MIT License, see http://opensource.org/licenses/MIT
// 
// File: ServoMotor.cs  Created: 2015-03-18@02:17
// Last modified: 2015-03-19@01:28 by Tim

using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;

namespace TA.NetMF.Motor
    {
    /// <summary>
    ///     Class ServoMotor. This class cannot be inherited.
    /// </summary>
    public sealed class ServoMotor : IServoControl
        {
        readonly uint maxPulse;
        readonly uint midpoint;
        readonly uint minPulse;
        readonly uint range;
        readonly uint refreshCycleMilliseconds;
        double position;
        PWM pwm;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ServoMotor" /> class.
        /// </summary>
        /// <param name="pwmChannel">
        ///     The PWM channel to use. For Netduino Plus, use one of the values from
        ///     <c>SecretLabs.NETMF.Hardware.NetduinoPlus.PWMChannels</c>.
        /// </param>
        /// <param name="refreshCycleMilliseconds">Controls the interval between servo position updates (controls the PWM period).</param>
        /// <param name="minPulse">The minimum pulse width (in microseconds) to be sent to the servo. Optional, defaults to 544 uS.</param>
        /// <param name="maxPulse">
        ///     The maximum pulse width (in microseconds) to be sent to the servo. Optional, defaults to 2400
        ///     uS.
        /// </param>
        /// <param name="rotationAngle">
        ///     The rotation angle (in degrees) through which the servo is designed to rotate. Optional,
        ///     defaults to 180�
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">minPulse must be greater than maxPulse</exception>
        /// <remarks>
        ///     Default values were chosen to match those used in the Arduino library and are appropriate for typical hobbyist
        ///     servo motors. Devices do vary and you should refer to the specifications of your own device for the best values to
        ///     use.
        /// </remarks>
        public ServoMotor(Cpu.PWMChannel pwmChannel, uint refreshCycleMilliseconds = 20, uint minPulse = 544,
            uint maxPulse = 2400, uint rotationAngle = 180)
            {
            if (minPulse >= maxPulse)
                throw new ArgumentOutOfRangeException("minPulse", "minPulse must be greater than maxPulse");
            this.refreshCycleMilliseconds = refreshCycleMilliseconds;
            this.minPulse = minPulse;
            this.maxPulse = maxPulse;
            SweepAngle = rotationAngle;
            range = maxPulse - minPulse;
            midpoint = minPulse + (range/2);
            ConfigurePwm(pwmChannel);
            }

        /// <summary>
        ///     Gets or sets the sweep angle of the servo motor.
        /// </summary>
        /// <value>The sweep angle.</value>
        public uint SweepAngle { get; set; }

        /// <summary>
        ///     Gets or sets the position of the servo expressed as a fraction of unity, where 0.0 represents full clockwise
        ///     rotation and 1.0 represents full counter-clockwise rotation.
        /// </summary>
        /// <value>The position expressed as a fraction of unity, i.e. in the range 0.0 to +1.0 inclusive.</value>
        public double Position
            {
            get { return position; }
            set
                {
                position = value;
                pwm.Duration = (uint) MapPositionToPulseWidth(value);
                }
            }

        /// <summary>
        ///     Gets or sets the angular position (in positive degrees) of the servo motor. The zero-point is the position of
        ///     maximum
        ///     clockwise rotation and the angle is measured counter-clockwise.
        ///     The maximum angle is that specified in the constructor.
        /// </summary>
        /// <value>The angle, in positive degrees, away from full clockwise displacement.</value>
        public double Angle
            {
            get { return position*SweepAngle; }
            set
                {
                var position = value/SweepAngle;
                var pulseWidth = (uint) MapPositionToPulseWidth(position);
                Debug.Print(pulseWidth.ToString());
                pwm.Duration = pulseWidth;
                }
            }

        double MapPositionToPulseWidth(double position)
            {
            return minPulse + (range*position);
            }

        void ConfigurePwm(Cpu.PWMChannel channel)
            {
            var frequencyHz = 1000/refreshCycleMilliseconds;
            var periodMicroseconds = refreshCycleMilliseconds*1000;
            var durationMicroseconds = midpoint;
            var pwm = new PWM(channel, frequencyHz, 0.0, false);
            pwm.Period = periodMicroseconds;
            pwm.Duration = durationMicroseconds;
            pwm.Start();
            this.pwm = pwm;
            }
        }
    }