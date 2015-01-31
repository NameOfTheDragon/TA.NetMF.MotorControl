// This file is part of the TA.NetMF.MotorControl project
// 
// Copyright © 2014-2015 Tigra Astronomy, all rights reserved.
// This source code is licensed under the MIT License, see http://opensource.org/licenses/MIT
// 
// File: InstantaneousStepperMotor.cs  Created: 2015-01-31@17:27
// Last modified: 2015-01-31@19:07 by Tim

using Microsoft.SPOT;
using Math = System.Math;

namespace TA.NetMF.Motor
    {
    /// <summary>
    ///   Class InstantaneousStepperMotor. A stepper motor with no acceleration that instantly
    ///   assumes the specified speed. The advantage of this type of motor is that the acceleration
    ///   curve does not have to be computed which means it can run considerably faster. This type
    ///   of motor may be suitable where there is a high gear ratio that is able to absorb the shock
    ///   of rapidly accelerating the load.
    /// </summary>
    public sealed class InstantaneousStepperMotor : StepperMotor
        {
        public InstantaneousStepperMotor(int limitOfTravel, IStepSequencer stepper) : base(limitOfTravel, stepper) {}

        /// <summary>
        ///   Handles the step timer tick event.
        /// </summary>
        /// <param name="state">Not used.</param>
        /// <remarks>
        ///   Beware! Tick events can still fire even after the timer has been disabled.
        /// </remarks>
        protected override void StepTimerTick(object state)
            {
            if (IsMoving)
                MoveOneStep(Direction);
            var distanceToGo = ComputeDistanceToTarget();
            //Debug.Print(distanceToGo.ToString());
            if (distanceToGo == 0)
                SetSpeed(0.0);
            }

        protected override void StartStepping(short moveDirection)
            {
            SetSpeed(MaximumSpeed*moveDirection);
            }

        protected override void SetSpeed(double speed)
            {
            base.SetSpeed(speed);
            var absoluteSpeed = Math.Abs(speed);
            if (absoluteSpeed < MotorStoppedThreshold)
                {
                AllStop();
                return;
                }
            var millisecondsUntilNextStep = (int)(1000.0/absoluteSpeed);
            lock (motorUpdateLock)
                {
                motorSpeed = speed;
                stepTimer.Change(millisecondsUntilNextStep, millisecondsUntilNextStep);
                }
            }
        }
    }
