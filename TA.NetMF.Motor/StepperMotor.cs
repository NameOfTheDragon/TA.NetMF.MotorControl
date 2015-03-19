// This file is part of the TA.NetMF.MotorControl project
// 
// Copyright © 2014-2015 Tigra Astronomy, all rights reserved.
// This source code is licensed under the MIT License, see http://opensource.org/licenses/MIT
// 
// File: StepperMotor.cs  Created: 2015-01-13@13:45
// Last modified: 2015-01-31@22:59 by Tim

using System;
using System.Threading;
using Microsoft.SPOT;
using Math = System.Math;

namespace TA.NetMF.Motor
    {
    /// <summary>
    ///   Class StepperMotor. A general purpose stepper motor controller, with acceleration and
    ///   microstepping. Supports multiple simultaneous instances. Once started, the motor will run
    ///   to its target position asynchronously and autonomously, raising the
    ///   <see cref="MotorStopped" /> event when it gets there.
    /// </summary>
    /// <remarks>
    ///   This class draws inspiration from the AccelStepper library, see
    ///   http://www.airspayce.com/mikem/arduino/AccelStepper, see also
    ///   https://github.com/adafruit/AccelStepper although the code has been completely rewritten
    ///   in C# and has some significant differences.
    /// </remarks>
    public class StepperMotor
        {
        /// <summary>
        ///   Delegate MotorEventHandler - callback signature usd by the <see cref="StepperMotor.MotorStopped" />
        ///   event.
        /// </summary>
        /// <param name="axis">The axis.</param>
        public delegate void AxisEventHandler(StepperMotor axis);

        /// <summary>
        ///   Delegate StepCallback - user-supplied delegate to write step data to the motor hardware.
        /// </summary>
        /// <param name="direction">The direction of the step.</param>
        public delegate void StepCallback(int direction);

        /// <summary>
        ///   The maximum possible theoretical speed that this driver is able to support. This is
        ///   limited by the timer resolution of the netduino which is 1 millisecond. In fact, the
        ///   <see cref="InstantaneousStepperMotor.StepTimerTick" /> handler takes somewhere between 2 and 3 milliseconds to
        ///   run on a Netduino Plus, so the actual achievable speed may be much less. Setting a
        ///   speed that is too fast has no adverse effect, the motor will run as fast as possible
        ///   and everything will work, but at a slower speed than expected.
        /// </summary>
        public const int MaximumPossibleSpeed = 1000;

        /// <summary>
        ///   The motor stopped threshold, The speed below which the motor is considered to be stopped.
        /// </summary>
        protected const double MotorStoppedThreshold = 0.00001;

        const int computeEvery = 64;
        static readonly TimeSpan InfiniteTimeout = new TimeSpan(0, 0, 0, 0, -1);
        int computeDelay = 0;

        /// <summary>
        ///   The current position
        /// </summary>
        int currentPosition;

        /// <summary>
        ///   The maximum run speed (in steps/second).
        ///   Defaults to the maximum theooretical possible speed; in practice the achievable speed will be lower.
        /// </summary>
        protected double maximumSpeed = 100;

        /// <summary>
        ///   The current motor speed in steps per second.
        /// </summary>
        protected double motorSpeed;

        /// <summary>
        ///   The regulated speed when running at constant speed rather than running to a position.
        /// </summary>
        double regulatedSpeed = 0;

        /// <summary>
        ///   True when speed regulation is operational; false when moving to a position.
        /// </summary>
        protected bool regulating;

        /// <summary>
        ///   The speed set point (in steps per second) when running at a regulated speed.
        /// </summary>
        protected double speedSetpoint;

        /// <summary>
        ///   The target position for a move operation. Range 0 to <see cref="limitOfTravel" />.
        /// </summary>
        int targetPosition;

        /// <summary>
        ///   The limit of travel (in steps)
        /// </summary>
        readonly int limitOfTravel;

        /// <summary>
        ///   The motor update lock - used as a mutual exclusion.
        /// </summary>
        protected readonly object motorUpdateLock = new object();

        /// <summary>
        ///   user-supplied delegate to write step data to the motor hardware.
        /// </summary>
        readonly StepCallback performStepCallback;

        /// <summary>
        ///   The stepper hardware
        /// </summary>
        readonly IStepSequencer stepper;

        /// <summary>
        ///   The step timer - times the interval between steps.
        /// </summary>
        protected readonly Timer stepTimer;

        /// <summary>
        ///   Initializes a new instance of the <see cref="StepperMotor" /> class.
        /// </summary>
        /// <param name="limitOfTravel">The limit of travel in steps.</param>
        /// <param name="stepper">The hardware driver that will perform the actual microstep.</param>
        /// <param name="performMicrostep">A method to write the current microstep value to the motor hardware.</param>
        /// <param name="microSteps">The number of micro steps per whole step.</param>
        protected StepperMotor(int limitOfTravel, IStepSequencer stepper, StepCallback performMicrostep = null)
            {
            //ToDo: this can be improved by deprecating the use of a delegate and instead providing
            // an implementation of IStepSequencer (defined in the Adafruit motor shield assembly).
            this.limitOfTravel = limitOfTravel;
            this.stepper = stepper;
            performStepCallback = performMicrostep ?? NullStepCallback;
            stepTimer = new Timer(StepTimerTick, null, Timeout.Infinite, Timeout.Infinite); // Stopped
            }

        /// <summary>
        ///   Gets or sets the maximum speed, in steps per second.
        ///   Must be greater than <see cref="MotorStoppedThreshold" /> but no greater than <see cref="MaximumPossibleSpeed" />.
        /// </summary>
        /// <value>The maximum speed.</value>
        public virtual double MaximumSpeed
            {
            get { return maximumSpeed; }
            set
                {
                if (value > MaximumPossibleSpeed)
                    throw new ArgumentException("Cannot be larger than MaximumPossibleSpeed");
                if (value <= MotorStoppedThreshold)
                    throw new ArgumentException("Must be larger than MotorStoppedThreshold");
                maximumSpeed = value;
                }
            }

        /// <summary>
        ///   Gets the motor speed, in steps per second.
        /// </summary>
        /// <value>The motor speed.</value>
        public double MotorSpeed { get { return motorSpeed; } }

        /// <summary>
        ///   Gets a value indicating whether this motor is moving.
        /// </summary>
        /// <value><c>true</c> if this motor is moving; otherwise, <c>false</c>.</value>
        public bool IsMoving { get { return Math.Abs(motorSpeed) > MotorStoppedThreshold; } }

        /// <summary>
        ///   The current direction of travel; 1 forward; 0 stopped; -1 reverse
        /// </summary>
        public int Direction { get { return IsMoving ? Math.Sign(motorSpeed) : 0; } }

        /// <summary>
        ///   Gets the current position of the stepper motor, measured in microsteps from the power-on position.
        /// </summary>
        /// <value>The position.</value>
        public int Position
            {
            get { return currentPosition; }
            set
                {
                if (IsMoving)
                    {
                    throw new InvalidOperationException(
                        "Position may only be set when the motor is stopped; check the IsMoving property or call AllStop().");
                    }
                if (value > limitOfTravel || value < 0)
                    {
                    throw new ArgumentOutOfRangeException("value",
                        "Position must be in the range 0 to " + limitOfTravel);
                    }
                currentPosition = value;
                }
            }

        protected virtual void StepTimerTick(object state)
            {
            throw new NotImplementedException();
            }

        /// <summary>
        ///   An empty method that serves as the default StepCallback delegate if the user doesn't supply one.
        /// </summary>
        /// <param name="direction">The direction.</param>
        void NullStepCallback(int direction) {}

        /// <summary>
        ///   Raised when the motor stops at the end of a move operation.
        /// </summary>
        public event AxisEventHandler MotorStopped;

        public event AxisEventHandler BeforeStep;

        /// <summary>
        ///   Raises the <see cref="MotorStopped" /> event.
        /// </summary>
        /// <param name="axis">The axis.</param>
        void OnMotorStopped(StepperMotor axis)
            {
            var handler = MotorStopped;
            if (handler != null)
                handler(axis);
            }

        void RaiseBeforeStep(StepperMotor axis)
            {
            var handler = BeforeStep;
            if (handler != null)
                handler(axis);

            }

        /// <summary>
        ///   Wraps the position.
        /// </summary>
        void WrapPosition()
            {
            if (currentPosition < 0)
                currentPosition += limitOfTravel;
            else
                currentPosition -= limitOfTravel;
            }

        /// <summary>
        ///   Moves one step in the indicated direction.
        /// </summary>
        /// <param name="direction">
        ///   A signed integer value indicating The direction to step. Any positive value results in a single forward step.
        ///   any negative value results in a backward step. Zero results in no step.
        /// </param>
        public void MoveOneStep(int direction)
            {
            var safeDirection = Math.Sign(direction);
            if (safeDirection == 0)
                return; // Not moving.
            RaiseBeforeStep(this);
            stepper.PerformStep(safeDirection);
            currentPosition += safeDirection;
            if (currentPosition > limitOfTravel || currentPosition < 0)
                WrapPosition();
            }

        /// <summary>
        ///   Computes the distance to target, in the current motor direction.
        /// </summary>
        /// <returns>
        ///   Returns the positive distance (in steps) to the target position. This value is always positive regardless of
        ///   direction of travel.
        /// </returns>
        protected long ComputeDistanceToTarget()
            {
            if (currentPosition == targetPosition)
                return 0;
            return targetPosition - currentPosition;
            }

        /// <summary>
        ///   Moves the motor to the specified position.
        /// </summary>
        /// <param name="target">The target position, in steps.</param>
        public void MoveToTargetPosition(int target)
            {
            var moveDirection = (short)Math.Sign(target - currentPosition);
            if (moveDirection == 0)
                {
                AllStop();
                return;
                }
            lock (motorUpdateLock)
                {
                targetPosition = target;
                regulating = false;
                StartStepping(moveDirection);
                }
            }

        protected virtual void StartStepping(short moveDirection)
            {
            throw new NotImplementedException();
            }

        /// <summary>
        ///   Moves the motor at a regulated speed, using acceleration whenever the speed changes.
        ///   Once moving, the speed may be changed by calling this method again with the new value.
        ///   To decelerate to a stop, call this method with a speed of 0.
        /// </summary>
        /// <param name="speed">The target speed.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">
        ///   speed;speed must be in the range -MaximumPossibleSpeed to
        ///   +MaximumPossibleSpeed
        /// </exception>
        public void MoveAtRegulatedSpeed(double speed)
            {
            var absoluteSpeed = Math.Abs(speed);
            if (absoluteSpeed > MaximumPossibleSpeed)
                {
                throw new ArgumentOutOfRangeException("speed",
                    "speed must be in the range -MaximumPossibleSpeed to +MaximumPossibleSpeed");
                }
            //if (maximumSpeed < absoluteSpeed)
            //    maximumSpeed = absoluteSpeed;

            // Set the target position, effectively, infinitely far away so we will never stop.
            targetPosition = speed >= 0 ? int.MaxValue : int.MinValue;
            speedSetpoint = absoluteSpeed < MotorStoppedThreshold ? 0.0 : speed;
            var direction = (short)Math.Sign(targetPosition - currentPosition);
            lock (motorUpdateLock)
                {
                regulating = true;
                if (!IsMoving)
                    StartStepping(direction);
                }
            }

        /// <summary>
        ///   Instantly stops the motor and raises the <see cref="OnMotorStopped" /> event.
        /// </summary>
        public void AllStop()
            {
            stepTimer.Change(Timeout.Infinite, Timeout.Infinite); // The timer may still tick!
            motorSpeed = 0;
            OnMotorStopped(this);
            }

        /// <summary>
        ///   Sets the motor speed.
        /// </summary>
        /// <param name="speed">The speed, in steps per second.</param>
        protected virtual void SetSpeed(double speed)
            {
            // Should be overridden in derived classes.
            Debug.Print("Set speed "+speed.ToString("F4"));
            }
        }
    }
