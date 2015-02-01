// This file is part of the TA.NetMF.MotorControl project
// 
// Copyright © 2014-2015 Tigra Astronomy, all rights reserved.
// This source code is licensed under the MIT License, see http://opensource.org/licenses/MIT
// 
// File: AcceleratingStepperMotor.cs  Created: 2015-01-31@17:10
// Last modified: 2015-01-31@23:42 by Tim

using System;
using System.Diagnostics;
using System.Threading;
using Microsoft.SPOT;
using Math = System.Math;

namespace TA.NetMF.Motor
    {
    public class AcceleratingStepperMotor : StepperMotor
        {
        /// <summary>
        ///   The acceleration factor
        /// </summary>
        double acceleration = 100.0; // Default is 100 steps per second per second.

        Thread computationThread;
        /*volatile*/
        double nextSpeed;

        public AcceleratingStepperMotor(int limitOfTravel, IStepSequencer stepper) : base(limitOfTravel, stepper)
            {
            RampTime = 5.0; // Default to 5 second ramp, compute acceleration.
            }

        /// <summary>
        ///   Gets or sets the acceleration in steps per second per second.
        ///   Setting this property affects <see cref="RampTime" /> and vice versa.
        /// </summary>
        /// <value>The acceleration, in steps per second per second.</value>
        public double Acceleration
            {
            get { return acceleration; }
            set
                {
                acceleration = value;
                PrintAccelerationParameters("Acceleration");
                }
            }

        /// <summary>
        ///   Gets or sets the time (in seconds) in which the motor will accelerate to full speed. Setting this property affects
        ///   <see cref="Acceleration" /> and vice versa.
        /// </summary>
        /// <value>The ramp time, in seconds.</value>
        public double RampTime
            {
            get { return RampTimeFromAcceleration(acceleration); }
            set
                {
                acceleration = AccelerationFromRampTime(value);
                PrintAccelerationParameters("RampTime");
                }
            }

        /// <summary>
        ///   Gets or sets the maximum speed, in steps per second. Must be greater than
        ///   <see cref="StepperMotor.MotorStoppedThreshold" /> but no greater than
        ///   <see cref="StepperMotor.MaximumPossibleSpeed" />.  When setting this property,
        ///   acceleration remains constant so <see cref="RampTime" /> must be recomputed. If for
        ///   some reason a constant ramp time is required, then it must be reset each time the
        ///   speed is changed.
        /// </summary>
        /// <value>The maximum allowed speed.</value>
        public override double MaximumSpeed
            {
            get { return base.MaximumSpeed; }
            set
                {
                base.MaximumSpeed = value;
                PrintAccelerationParameters("MaximumSpeed");
                }
            }

        [Conditional("DEBUG")]
        void PrintAccelerationParameters(string why)
            {
            Debug.Print(why + " changed. Acceleration parameters: MaxSpeed=" + MaximumSpeed.ToString("F4") +
                        " Acceleration=" + acceleration.ToString("F4") + " RampTime=" + RampTime.ToString("F4"));
            }

        double RampTimeFromAcceleration(double acceleration)
            {
            return MaximumSpeed/acceleration; // From v = u + at; since u is 0, v = at
            }

        double AccelerationFromRampTime(double time)
            {
            return MaximumSpeed/time; // From v = u + at; since u is 0, v = at
            }

        /// <summary>
        ///   Computes the new motor velocity based on the current velocity, such that the velocity always moves towards
        ///   the set point (if regulation is active), or the acceleration curve dictated by the <see cref="Acceleration" />
        ///   property.
        ///   The magnitude of the returned speed will never be greater than <see cref="StepperMotor.MaximumSpeed" /> which in turn
        ///   can never
        ///   exceed <see cref="StepperMotor.MaximumPossibleSpeed" />.
        ///   <see cref="StepperMotor.MaximumSpeed" />.
        /// </summary>
        /// <returns>The computed velocity, in steps per second.</returns>
        /// <remarks>
        ///   A positive value indicates speed in the forward direction, while a negative value indicates speed in the reverse
        ///   direction.
        ///   'forward' is defined as motion towards a higher position value; 'reverse' is defined as motion towards a lower
        ///   position value.
        ///   No attempt is made here to define the mechanical direction. A speed value that is within
        ///   <see cref="StepperMotor.MotorStoppedThreshold" />
        ///   of zero is considered to mean that the motor should be stopped.
        /// </remarks>
        double ComputeAcceleratedVelocity()
            {
            var distanceToGo = ComputeDistanceToTarget();
            var absoluteDistance = Math.Abs(distanceToGo);
            var direction = Math.Sign(distanceToGo);
            if (distanceToGo == 0)
                return 0.0; // We're there.
            if (!IsMoving)
                return Math.Sqrt(2.0*Acceleration)*direction; // Accelerate away from stop.
            // Compute the unconstrained target speed based on the deceleration curve or the regulation set point.
            var targetSpeed = regulating ? speedSetpoint : Math.Sqrt(2.0*absoluteDistance*Acceleration)*direction;
            // The change in speed is a function of the absolute current speed and acceleration.
            var increment = Acceleration/Math.Abs(motorSpeed);
            var directionOfChange = Math.Sign(targetSpeed - motorSpeed);
            var newSpeed = motorSpeed + increment*directionOfChange;
            // The computed new speed must be constrained by both the MaximumSpeed and the acceleration curve.
            var clippedSpeed = newSpeed.ConstrainToLimits(-maximumSpeed, +maximumSpeed);
            return clippedSpeed;
            }

        void StartComputationThread()
            {
            if (computationThread == null)
                {
                CreateAndStartNewComputationThread();
                }
            else
                switch (computationThread.ThreadState)
                    {
                    case ThreadState.Running:
                        break;
                    case ThreadState.StopRequested:
                    case ThreadState.Stopped:
                    case ThreadState.WaitSleepJoin:
                    case ThreadState.AbortRequested:
                    case ThreadState.Aborted:
                        computationThread.Abort();
                        computationThread.Join(10);
                        computationThread = null;
                        CreateAndStartNewComputationThread();
                        break;
                    case ThreadState.Unstarted:
                        computationThread.Start();
                        break;
                    default:
                        computationThread.Resume();
                        break;
                    }

            }

        void CreateAndStartNewComputationThread()
            {
            var threadStarter = new ThreadStart(ComputeAccelerationCurveThread);
            computationThread = new Thread(threadStarter);
            computationThread.Priority = ThreadPriority.Normal;
            computationThread.Start();
            }

        void StopComputationThread()
            {
            computationThread.Suspend();
            }

        void ComputeAccelerationCurveThread()
            {
            while (true)
                {
                nextSpeed = ComputeAcceleratedVelocity();
                Thread.Sleep(0); // Yield
                }
            }

        /// <summary>
        ///   Handles the step timer tick event.
        /// </summary>
        /// <param name="state">Not used.</param>
        /// <remarks>
        ///   Beware! Tick events can still fire even after the timer has been disabled.
        /// </remarks>
        protected virtual void StepTimerTick(object state)
            {
            if (nextSpeed != motorSpeed)
                SetSpeed(nextSpeed);
            if (IsMoving)
                MoveOneStep(Direction);
            }

        protected override void StartStepping(short moveDirection)
            {
            var initialSpeed = ComputeAcceleratedVelocity();
            nextSpeed = initialSpeed;
            SetSpeed(initialSpeed);
            StartComputationThread();
            }

        protected override void SetSpeed(double speed)
            {
            base.SetSpeed(speed);
            var absoluteSpeed = Math.Abs(speed);
            if (absoluteSpeed < MotorStoppedThreshold)
                {
                StopComputationThread();
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
