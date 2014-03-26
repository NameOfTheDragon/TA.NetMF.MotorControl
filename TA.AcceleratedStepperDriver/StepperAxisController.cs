// This file is part of the TA.NetMF.StepperDriver project
// 
// Copyright © 2014 TiGra Networks, all rights reserved.
// 
// File: StepperAxisController.cs  Created: 2014-03-25@22:42
// Last modified: 2014-03-26@03:49 by Tim

using System;
using System.Threading;

namespace TA.AcceleratedStepperDriver
    {
    public class StepperAxisController
        {
        public delegate void AxisEventHandler(StepperAxisController axis);

        public delegate void MicrostepCallback(int index);

        const double MotorStoppedThreshold = 0.1;
        readonly int microStepsPerStep = 64;
        readonly MicrostepCallback performMicrostep;
        readonly Timer stepTimer;
        readonly int stepsPerRevolution;
        double acceleration = 25.0f;
        int currentPosition;
        int direction; // 1=forward; 0=stopped; -1=reverse

        double maximumSpeed = 1000;
        // Maximum axis speed, in steps per second. Practical limit of 1000 imposed by timer resolution.

        int microstep;
        double motorSpeed;
        int targetPosition;

        public StepperAxisController(int stepsPerRevolution, MicrostepCallback performMicrostep, int microSteps = 64)
            {
            this.stepsPerRevolution = stepsPerRevolution;
            this.performMicrostep = performMicrostep;
            microStepsPerStep = microSteps;
            stepTimer = new Timer(StepTimerTick, null, 0, 1000); // Stopped
            }

        /// <summary>
        ///   Gets or sets the maximum speed, in steps per second.
        ///   A practical limit of 1,000 steps per second is imposed by timer resolution.
        /// </summary>
        /// <value>The maximum speed.</value>
        public double MaximumSpeed { get { return maximumSpeed; } set { maximumSpeed = value; } }

        /// <summary>
        ///   Gets or sets the acceleration rate in steps per second per second.
        /// </summary>
        /// <value>The acceleration.</value>
        public double Acceleration { get { return acceleration; } set { acceleration = value; } }

        /// <summary>
        ///   Gets the motor speed, in steps per second.
        /// </summary>
        /// <value>The motor speed.</value>
        public double MotorSpeed { get { return motorSpeed; } }

        public bool IsMoving { get { return motorSpeed > MotorStoppedThreshold; } }
        public event AxisEventHandler AxisStopped;

        protected virtual void OnAxisStopped(StepperAxisController axis)
            {
            var handler = AxisStopped;
            if (handler != null)
                handler(axis);
            }

        void StepTimerTick(object state)
            {
            Step();
            }

        void Step()
            {
            if (direction == 0)
                return;
            performMicrostep(microstep);
            UpdateMicrostep();
            currentPosition = ++currentPosition%stepsPerRevolution;
            var nextInterval = ComputeSpeed();
            SetSpeed(nextInterval);
            }

        void UpdateMicrostep()
            {
            microstep = (short)((microstep + direction)%microStepsPerStep);
            }

        /// <summary>
        ///   Computes the desired speed, in steps per second.
        ///   Takes account of acceleration and deceleration.
        /// </summary>
        /// <returns>System.Single.</returns>
        float ComputeSpeed()
            {
            var distanceToGo = ComputeDistanceToTarget();
            if (distanceToGo == 0)
                return 0.0f; // We're there.
            if (MotorSpeed < MotorStoppedThreshold)
                return (float)Math.Sqrt(2.0*Acceleration); // Accelerate away from stop.
            var targetSpeed = Math.Sqrt(2.0*distanceToGo*Acceleration);
            var increment = Acceleration/MotorSpeed;
            var newSpeed = targetSpeed > MotorSpeed ? MotorSpeed + increment : MotorSpeed - increment;
            var clippedSpeed = (float)Math.Min(newSpeed, MaximumSpeed);
            return clippedSpeed;
            }

        /// <summary>
        ///   Computes the distance to target, in the current motor direction.
        /// </summary>
        /// <returns>System.Int64. Distance (in steps) to the target position.</returns>
        long ComputeDistanceToTarget()
            {
            if (currentPosition == targetPosition)
                return 0;
            return Math.Abs(targetPosition - currentPosition);
            //if (direction == 0)
            //    return 0;
            //var targetGreaterThanCurrent = targetPosition > currentPosition;
            //var distanceClockwise = targetGreaterThanCurrent
            //    ? targetPosition - currentPosition
            //    : stepsPerRevolution + targetPosition - currentPosition;
            //var distanceAnticlockwise = targetGreaterThanCurrent
            //    ? stepsPerRevolution + currentPosition - targetPosition
            //    : currentPosition - targetPosition;
            //return direction == 1 ? distanceClockwise : distanceAnticlockwise;
            }

        /// <summary>
        ///   Runs to the target position in the shortest direction.
        /// </summary>
        /// <param name="target">The target position.</param>
        public void RunToTarget(int target)
            {
            var shortestDirection = (short)Math.Sign(target - currentPosition);
            if (shortestDirection == 0)
                {
                AllStop();
                return;
                }
            RunToTarget(target, shortestDirection);
            }

        void AllStop()
            {
            stepTimer.Change(Timeout.Infinite, Timeout.Infinite);
            direction = 0;
            motorSpeed = 0;
            OnAxisStopped(this);
            }

        /// <summary>
        ///   Runs to the target position in the specified direction.
        /// </summary>
        /// <param name="target">The target position.</param>
        /// <param name="moveDirection">The direction (+1 forward; -1 reverse).</param>
        /// <exception cref="System.ArgumentException">Direction must be +1 or -1</exception>
        public void RunToTarget(int target, short moveDirection)
            {
            if (Math.Abs(moveDirection) != 1)
                throw new ArgumentException("Direction must be +1 or -1");
            targetPosition = target;
            direction = moveDirection;
            SetSpeed(ComputeSpeed());
            }

        /// <summary>
        ///   Sets the timer interval.
        /// </summary>
        /// <param name="speed">The speed, in steps per second.</param>
        void SetSpeed(float speed)
            {
            if (speed < MotorStoppedThreshold)
                {
                AllStop();
                return;
                }
            motorSpeed = speed;
            var millisecondsUntilNextStep = (int)Math.Abs(1000/speed);
            stepTimer.Change(millisecondsUntilNextStep, millisecondsUntilNextStep);
            }
        }
    }
