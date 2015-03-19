// This file is part of the TA.NetMF.MotorControl project
// 
// Copyright © 2014-2015 Tigra Astronomy, all rights reserved.
// This source code is licensed under the MIT License, see http://opensource.org/licenses/MIT
// 
// File: DcMotor.cs  Created: 2015-02-02@03:07
// Last modified: 2015-02-02@05:43 by Tim

using System;
using System.Threading;
using Microsoft.SPOT;
using Math = System.Math;

namespace TA.NetMF.Motor
    {
    public class DcMotor
        {
        const int DefaultResolution = 100; // milliseconds
        Timer accelerationTimer;
        long startTime;
        double startVelocity;
        double targetVelocity;
        readonly int accelerationResolutionInMilliseconds;
        readonly HBridge motorWinding;

        /// <summary>
        ///   Initializes a new instance of the <see cref="DcMotor" /> class.
        /// </summary>
        /// <param name="motorWinding">The H-Bridge that controls the motor winding.</param>
        /// <param name="accelerationResolutionInMilliseconds">
        ///   Optional. The resolution of the
        ///   acceleration curve computation, in system clock ticks. Must be greater than
        ///   <see cref="TimeSpan.TicksPerMillisecond" />
        /// </param>
        public DcMotor(HBridge motorWinding, int accelerationResolutionInMilliseconds = DefaultResolution)
            {
            Acceleration = 0.2;
            this.motorWinding = motorWinding;
            this.accelerationResolutionInMilliseconds = accelerationResolutionInMilliseconds;
            }

        /// <summary>
        ///   Gets the current velocity. Positive values represent forward motion; negative values
        ///   represent reverse motion.
        /// </summary>
        /// <value>The current velocity, or 0 if stopped.</value>
        public double CurrentVelocity { get; private set; }

        /// <summary>
        ///   Gets or sets the acceleration, or rate of change of velocity. Velocity is expressed in
        ///   terms of a fraction of output power to the motor, where 1.0 is full power and 0.0 is no power,
        ///   therefore acceleration is in terms of change in output power per second and must be a
        ///   positive value greater than zero.
        /// </summary>
        /// <value>The acceleration, a value greater than 0.</value>
        public double Acceleration { get; set; }

        public void AccelerateToVelocity(double velocity)
            {
            Debug.Print("Accelerate to: " + velocity.ToString("F4"));
            StopAccelerating();
            startTime = DateTime.UtcNow.Ticks;
            startVelocity = CurrentVelocity;
            targetVelocity = velocity;
            StartAccelerating();
            }

        void StartAccelerating()
            {
            Debug.Print("Start acceleration timer");
            if (accelerationTimer == null)
                {
                accelerationTimer = new Timer(HandleAccelerationTimerTick,
                    null,
                    accelerationResolutionInMilliseconds,
                    accelerationResolutionInMilliseconds);
                }
            else
                accelerationTimer.Change(accelerationResolutionInMilliseconds, accelerationResolutionInMilliseconds);
            }

        void StopAccelerating()
            {
            Debug.Print("Stop acceleration timer");
            if (accelerationTimer != null)
                accelerationTimer.Change(Timeout.Infinite, Timeout.Infinite);
            }

        void HandleAccelerationTimerTick(object ignored)
            {
            var accelerationSign = Math.Sign(targetVelocity - CurrentVelocity);
            var acceleratedVelocity = ComputeAcceleratedVelocity(Acceleration*accelerationSign);
            var newVelocity = accelerationSign >= 0 ? Accelerate(acceleratedVelocity) : Decelerate(acceleratedVelocity);
            Debug.Print("Velocity " + newVelocity.ToString("F4"));
            motorWinding.SetOutputPowerAndPolarity(newVelocity);
            CurrentVelocity = newVelocity;
            }

        double Decelerate(double newVelocity)
            {
            if (newVelocity <= targetVelocity)
                {
                StopAccelerating();
                return targetVelocity;
                }
            return newVelocity;
            }

        double Accelerate(double newVelocity)
            {
            if (newVelocity >= targetVelocity)
                {
                StopAccelerating();
                return targetVelocity;
                }
            return newVelocity;
            }

        /// <summary>
        ///   Computes the accelerated velocity based on the formula v = u + at.
        /// </summary>
        /// <returns>System.Double.</returns>
        double ComputeAcceleratedVelocity(double acceleration)
            {
            var elapsedTime = (DateTime.UtcNow.Ticks - startTime)/(double)TimeSpan.TicksPerSecond;
            var v = startVelocity + acceleration*elapsedTime; // v = u + at
            return v;
            }
        }
    }
