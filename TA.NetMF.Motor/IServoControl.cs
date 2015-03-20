// This file is part of the TA.NetMF.MotorControl project
// 
// Copyright © 2014-2015 Tigra Astronomy, all rights reserved.
// This source code is licensed under the MIT License, see http://opensource.org/licenses/MIT
// 
// File: IServoControl.cs  Created: 2015-03-19@05:29
// Last modified: 2015-03-20@02:05 by Tim

using System;

namespace TA.NetMF.Motor
    {
    /// <summary>
    ///     Interface IServoControl - defines hardware-independent methods for controlling the
    ///     position of a servo motor
    /// </summary>
    public interface IServoControl
        {
        /// <summary>
        ///     Gets or sets the position of the servo expressed as a fraction of unity, where 0.0
        ///     represents full clockwise rotation and 1.0 represents full counterclockwise
        ///     rotation.
        /// </summary>
        /// <value>
        ///     The position expressed as a fraction of unity, i.e. in the range 0.0 to +1.0
        ///     inclusive.
        /// </value>
        double Position { get; set; }

        /// <summary>
        ///     Gets or sets the angular position (in positive degrees) of the servo motor. The
        ///     zero-point is the position of maximum clockwise rotation and the angle is measured
        ///     counter-clockwise. The maximum angle is implementation dependent.
        /// </summary>
        /// <value>The angle, in positive degrees, away from full clockwise displacement.</value>
        double Angle { get; set; }

        /// <summary>
        ///     Gets or sets the amount of trim, expressed as a fraction of ± unity, that is used to
        ///     offset the servo centre position and sweep angle. This does not increase the servo's
        ///     range of motion, which is often limited by hard stops; therefore trim should be used
        ///     with care and discretion.
        /// </summary>
        /// <value>
        ///     The amount of 'trim' (expressed as a fraction of unity) which is added to or
        ///     subtracted from the servo centre position. Setting trim to +1 would theoretically result in a
        ///     counter-clockwise offset of half the <see cref="SweepAngle" />. Setting it to -1
        ///     would theoretically result in a clockwise offset of half the <see cref="SweepAngle" />.
        /// </value>
        /// <exception cref="ArgumentOutOfRangeException" accessor="set">
        ///     Thrown if the value is set less than -1 or greater than 1.
        /// </exception>
        /// <remarks>
        ///     Trim is designed to compensate for small differences in devices, usually just a few
        ///     percent, due to manufacturing tolerances. Although the trim value is modelled as a
        ///     fraction of unity, setting a trim value that large would be very unwise as that
        ///     would likely stress the device beyond its design limits. Trim values in excess of
        ///     about 20% (±0.2) would represent a very large tolerance and should be regarded with
        ///     suspicion. The need to use large trim values may indicate an underlying systemic
        ///     issue.
        /// </remarks>
        double Trim { get; set; }

        /// <summary>
        ///     Gets the full sweep angle of the servo motor. 0° corresponds to full clockwise
        ///     deflection; Angles are measured from that point increasing in the counter-clockwise
        ///     direction.
        /// </summary>
        /// <value>The full sweep angle of the servo motor, in degrees.</value>
        uint SweepAngle { get; }

        /// <summary>
        ///     Centres the servo motor to the trimmed centre point.
        /// </summary>
        /// <remarks>
        ///     This is equivalent to setting <see cref="Position" /> to 0.5.
        /// </remarks>
        void Centre();

        /// <summary>
        ///     Releases the servo torque.
        /// </summary>
        /// <remarks>
        ///     Torque can be re-applied by setting <see cref="Position" /> or <see cref="Angle" />.
        /// </remarks>
        void Release();
        }
    }