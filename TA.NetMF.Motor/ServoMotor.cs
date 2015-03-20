// This file is part of the TA.NetMF.MotorControl project
// 
// Copyright © 2014-2015 Tigra Astronomy, all rights reserved.
// This source code is licensed under the MIT License, see http://opensource.org/licenses/MIT
// 
// File: ServoMotor.cs  Created: 2015-03-19@05:29
// Last modified: 2015-03-20@02:12 by Tim

using System;
using Microsoft.SPOT.Hardware;

namespace TA.NetMF.Motor
    {
    /// <summary>
    ///     Class ServoMotor. This class cannot be inherited.
    /// </summary>
    public sealed class ServoMotor : IServoControl
        {
        readonly uint halfRange;
        //readonly uint midpoint;
        readonly uint midpoint;
        readonly uint range;
        readonly uint refreshCycleMilliseconds;
        readonly uint sweepAngle;
        double position;
        PWM pwm;
        double trim;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ServoMotor" /> class.
        /// </summary>
        /// <param name="pwmChannel">
        ///     The PWM channel to use. For Netduino Plus, use one of the values from
        ///     <c>SecretLabs.NETMF.Hardware.NetduinoPlus.PWMChannels</c>.
        /// </param>
        /// <param name="refreshCycleMilliseconds">Controls the interval between servo position updates (controls the PWM period).</param>
        /// The maximum pulse width (in microseconds) to be sent to the servo. Optional, defaults to 2400
        /// uS.
        /// </param>
        /// <param name="rangeRange">
        ///     The amount of variation in pulse period around <paramref name="midpoint" /> that is
        ///     required to achieve full deflection of the servo. Optional, defaults to 2000 us (2 ms).
        /// </param>
        /// <param name="sweepAngle">
        ///     The sweep angle (in degrees) through which the servo is designed to rotate. Optional;
        ///     defaults to 180°.
        /// </param>
        /// <param name="midpoint">The duration of pulse (in microseconds) that results in the servo being centered.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when the specified
        ///     <paramref name="midpoint" />,
        ///     <paramref name="range" /> and
        ///     <paramref name="refreshCycleMilliseconds" />
        ///     would result in an inappropriate configuration; e.g. would result in negative pulse widths or pulses longer than
        ///     the refresh period.
        /// </exception>
        /// <remarks>
        ///     Default values were chosen to be appropriate for typical hobbyist servo motors. Devices do vary between different
        ///     models and even between different devices of the same model, so you should refer to the specifications of your own
        ///     device for the best/ideal values to use and then give the user some way to adjust things (e.g. by setting the
        ///     <see cref="Trim" /> property).
        /// </remarks>
        public ServoMotor(Cpu.PWMChannel pwmChannel, uint refreshCycleMilliseconds = 20, uint midpoint = 1500,
            uint range = 2000, uint sweepAngle = 180)
            {
            halfRange = range/2;
            if (midpoint - halfRange < 1)
                throw new ArgumentOutOfRangeException("range", "would result in a zero or negative pulse width.");
            if (midpoint + halfRange > (refreshCycleMilliseconds*1000))
                throw new ArgumentOutOfRangeException("range",
                    "would result in a pulse width longer than the refresh period.");
            this.refreshCycleMilliseconds = refreshCycleMilliseconds;
            this.midpoint = midpoint;
            this.range = range;
            this.sweepAngle = sweepAngle;
            position = 0.5; // midpoint
            Trim = 0;
            ConfigurePwm(pwmChannel);
            }

        /// <summary>
        ///     Gets or sets the amount of trim, expressed as a fraction of ± unity, that is used to
        ///     offset the servo centre position and sweep angle. This does not increase the servo's
        ///     range of motion, which is often limited by hard stops; therefore trim should be used
        ///     with care and discretion.
        /// </summary>
        /// <value>
        ///     The amount of 'trim' (expressed as a fraction of unity) which is added to or
        ///     subtracted from the servo centre position. Setting trim to +1 would theoretically
        ///     result in a counter-clockwise offset of half the <see cref="SweepAngle" />. Setting
        ///     it to -1 would theoretically result in a clockwise offset of half the
        ///     <see cref="SweepAngle" />.
        /// </value>
        /// <exception cref="System.ArgumentOutOfRangeException">
        ///     value;must be in the range -1.0 to
        ///     +1.0 inclusive.
        /// </exception>
        /// <remarks>
        ///     Trim is designed to compensate for small differences in devices, usually just a few
        ///     percent, due to manufacturing tolerances. Although the trim value is modelled as a
        ///     fraction of unity, setting a trim value that large would be very unwise as that
        ///     would likely stress the device beyond its design limits. Trim values in excess of
        ///     about 20% (±0.2) would represent a very large tolerance and should be regarded with
        ///     suspicion. The need to use large trim values may indicate an underlying systemic
        ///     issue. Consider using some of the optional constructor parameters to set a different
        ///     midpoint and range instead.
        /// </remarks>
        public double Trim
            {
            get { return trim; }
            set
                {
                if (Math.Abs(value) > 1)
                    throw new ArgumentOutOfRangeException("value", "must be in the range -1.0 to +1.0 inclusive.");
                trim = value;
                }
            }

        /// <summary>
        ///     Gets the full sweep angle of the servo motor. 0° corresponds to full clockwise deflection; Angles are measured from
        ///     that point increasing in the counter-clockwise direction.
        /// </summary>
        /// <value>The full sweep angle of the servo motor, in degrees.</value>
        public uint SweepAngle
            {
            get { return sweepAngle; }
            }

        /// <summary>
        ///     Centres the servo motor to the trimmed centre point.
        /// </summary>
        public void Centre()
            {
            Position = 0.5;
            }

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
                pwm.Duration = (uint) MapTrimmedPositionToPulseWidth(value);
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
            get { return position*sweepAngle; }
            set
                {
                var position = value/sweepAngle; // Convert to a fraction of unity then treat like a position.
                Position = position;
                }
            }

        /// <summary>
        ///     Releases the servo torque.
        /// </summary>
        /// <remarks>
        ///     Torque can be re-applied by setting a new <see cref="Position" /> or <see cref="Angle" />.
        /// </remarks>
        public void Release()
            {
            pwm.Duration = 0;
            }

        /// <summary>
        ///     Maps a position expressed as a fraction of unity to the correct pulse width for the
        ///     servo, taking into account the midpoint, range and trim.
        /// </summary>
        /// <param name="position">
        ///     The position, expressed as a fraction of unity, where 0.0 is full clockwise
        ///     deflection and +1.0 is full counter clockwise deflection. The mid point of the sweep
        ///     angle is at 0.5.
        /// </param>
        /// <returns>
        ///     System.Double containing the trimmed pulse width, in microseconds, clipped by
        ///     the sweep angle of the device.
        /// </returns>
        double MapTrimmedPositionToPulseWidth(double position)
            {
            var minimumPulse = midpoint - halfRange;
            var maximumPulse = midpoint + halfRange;
            var deflection = range*position;
            var rawPulseWidth = minimumPulse + deflection;
            var trimmedPulseWidth = rawPulseWidth + trim*halfRange;
            if (trimmedPulseWidth < 1) trimmedPulseWidth = 1; // We must never allow a 0 or -ve pulse width.
            return trimmedPulseWidth;
            }

        /// <summary>
        ///     Configures and starts the supplied PWM channel with the required period (usually 20 ms) and sets the initial
        ///     duration to set the servo at its midpoint.
        /// </summary>
        /// <param name="channel">The PWM channel to be configured and started.</param>
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