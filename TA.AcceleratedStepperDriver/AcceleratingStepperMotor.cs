// This file is part of the TA.NetMF.StepperDriver project
// 
// Copyright © 2014 TiGra Networks, all rights reserved.
// 
// File: AcceleratingStepperMotor.cs  Created: 2014-03-26@04:11
// Last modified: 2014-04-09@03:26 by Tim

using System;
using System.Threading;

namespace TA.AcceleratedStepperDriver
    {
    /// <summary>
    ///   Class AcceleratingStepperMotor. A general purpose stepper motor controller, with acceleration and microstepping.
    ///   Supports multiple simultaneous instances.
    ///   Once started, the motor will run to its target position asynchronously and autonomously, raising the
    ///   <see cref="MotorStopped" /> event when it gets there.
    /// </summary>
    /// <remarks>
    ///   This class draws inspiration from the AccelStepper library, see http://www.airspayce.com/mikem/arduino/AccelStepper,
    ///   see also https://github.com/adafruit/AccelStepper although the code has been completely rewritten in C#
    ///   and has some significant differences.
    /// </remarks>
    public sealed class AcceleratingStepperMotor
        {
        /// <summary>
        ///   Delegate MicrostepCallback - user-supplied delegate to write step data to the motor hardware.
        /// </summary>
        /// <param name="index">The microstep index.</param>
        public delegate void MicrostepCallback(uint index);

        /// <summary>
        ///   Delegate MotorEventHandler - callback signature usd by the <see cref="AcceleratingStepperMotor.MotorStopped" />
        ///   event.
        /// </summary>
        /// <param name="axis">The axis.</param>
        public delegate void MotorEventHandler(AcceleratingStepperMotor axis);

        /// <summary>
        ///   The maximum possible theoretical speed that this driver is able to support.
        ///   The actual achievable speed may be much less due to hardware constraints.
        /// </summary>
        public const int MaximumPossibleSpeed = 500;

        /// <summary>
        ///   The motor stopped threshold, The speed below which the motor is considered to be stopped.
        /// </summary>
        const double MotorStoppedThreshold = 0.00001;

        /// <summary>
        ///   The limit of travel (in steps)
        /// </summary>
        readonly int limitOfTravel;

        /// <summary>
        ///   The number of micro steps per whole step
        /// </summary>
        readonly int microStepsPerStep = 64;

        /// <summary>
        ///   The motor update lock - used as a mutual exclusion.
        /// </summary>
        readonly object motorUpdateLock = new object();

        /// <summary>
        ///   user-supplied delegate to write step data to the motor hardware.
        /// </summary>
        readonly MicrostepCallback performMicrostep;

        /// <summary>
        ///   The step timer - times the interval between steps.
        /// </summary>
        readonly Timer stepTimer;

        /// <summary>
        ///   The acceleration factor
        /// </summary>
        double acceleration = 1.0f;

        /// <summary>
        ///   The current position
        /// </summary>
        int currentPosition;

        /// <summary>
        ///   The maximum run speed (in steps/second).
        ///   Defaults to a deliberately low 'safe' value, in practice 1000 is the practical upper limit.
        /// </summary>
        double maximumSpeed = 100;

        /// <summary>
        ///   The microstep value, cycles from 0 to <see cref="microStepsPerStep" />-1.
        /// </summary>
        uint microstep;

        /// <summary>
        ///   The current motor speed in steps per second.
        /// </summary>
        double motorSpeed;

        /// <summary>
        ///   The regulated speed when running at constant speed rather than running to a position.
        /// </summary>
        double regulatedSpeed = 0;

        /// <summary>
        ///   True when speed regulation is operational; false when moving to a position.
        /// </summary>
        bool regulating;

        /// <summary>
        ///   The speed set point (in steps per second) when running at a regulated speed.
        /// </summary>
        double speedSetpoint;

        /// <summary>
        ///   The target position for a move operation. Range 0 to <see cref="limitOfTravel" />.
        /// </summary>
        int targetPosition;

        /// <summary>
        ///   Initializes a new instance of the <see cref="AcceleratingStepperMotor" /> class.
        /// </summary>
        /// <param name="limitOfTravel">The limit of travel in steps.</param>
        /// <param name="performMicrostep">A method to write the current microstep value to the motor hardware.</param>
        /// <param name="microSteps">The number of micro steps per whole step.</param>
        public AcceleratingStepperMotor(int limitOfTravel, MicrostepCallback performMicrostep, int microSteps = 64)
            {
            this.limitOfTravel = limitOfTravel;
            this.performMicrostep = performMicrostep;
            microStepsPerStep = microSteps;
            stepTimer = new Timer(StepTimerTick, null, Timeout.Infinite, Timeout.Infinite); // Stopped
            }

        /// <summary>
        ///   Gets or sets the maximum speed, in steps per second.
        ///   Must be greater than <see cref="MotorStoppedThreshold" /> but no greater than <see cref="MaximumPossibleSpeed" />.
        /// </summary>
        /// <value>The maximum speed.</value>
        public double MaximumSpeed
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
        ///   Gets or sets the acceleration rate in steps per second per second.
        ///   Setting this property affects the <see cref="RampTime" /> and vice versa.
        /// </summary>
        /// <value>The acceleration, in steps per second per second.</value>
        public double Acceleration { get { return acceleration; } set { acceleration = value; } }

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
        ///   Gets or sets the time in which the motor will accelerate to full speed. Setting this property affects
        ///   <see cref="Acceleration" /> and vice versa.
        /// </summary>
        /// <value>The ramp time, in seconds.</value>
        public double RampTime { get { return MaximumSpeed/Acceleration; } set { Acceleration = MaximumSpeed/value; } }

        /// <summary>
        ///   Raised when the motor stops at the end of a move operation.
        /// </summary>
        public event MotorEventHandler MotorStopped;

        /// <summary>
        ///   Raises the <see cref="MotorStopped" /> event.
        /// </summary>
        /// <param name="axis">The axis.</param>
        void OnMotorStopped(AcceleratingStepperMotor axis)
            {
            var handler = MotorStopped;
            if (handler != null)
                handler(axis);
            }

        /// <summary>
        ///   Handles the step timer tick event.
        /// </summary>
        /// <param name="state">Not used.</param>
        void StepTimerTick(object state)
            {
            StepAndAccelerate();
            }

        /// <summary>
        ///   Steps the motor one step in the current direction, then computes and sets the new step speed
        ///   The act of setting the speed re-arms the step timer.
        /// </summary>
        void StepAndAccelerate()
            {
            MoveOneStep(Direction);
            var nextSpeed = ComputeAcceleratedVelocity();
            SetSpeed(nextSpeed); // Stops the motor if speed is close enough to zero.
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
            performMicrostep(microstep); // user code
            UpdateMicrostep(safeDirection); // Increment or decrement the microstep counter.
            currentPosition += safeDirection;
            if (currentPosition > limitOfTravel || currentPosition < 0)
                WrapPosition();
            }

        /// <summary>
        ///   Updates the microstep index by one step in the specified direction..
        /// </summary>
        void UpdateMicrostep(int increment)
            {
            microstep = (uint)((microstep + increment)%microStepsPerStep);
            }

        /// <summary>
        ///   Computes the new motor velocity based on the current velocity, such that the velocity always moves towards
        ///   the set point (if regulation is active), or the acceleration curve dictated by the <see cref="Acceleration" />
        ///   property.
        ///   The magnitude of the returned speed will never be greater than <see cref="MaximumSpeed" /> which in turn can never
        ///   exceed <see cref="MaximumPossibleSpeed" />.
        ///   <see cref="MaximumSpeed" />.
        /// </summary>
        /// <returns>The computed velocity, in steps per second.</returns>
        /// <remarks>
        ///   A positive value indicates speed in the forward direction, while a negative value indicates speed in the reverse
        ///   direction.
        ///   'forward' is defined as motion towards a higher position value; 'reverse' is defined as motion towards a lower
        ///   position value.
        ///   No attempt is made here to define the mechanical direction. A speed value that is within
        ///   <see cref="MotorStoppedThreshold" />
        ///   of zero is considered to mean that the motor should be stopped.
        /// </remarks>
        double ComputeAcceleratedVelocity()
            {
            var distanceToGo = ComputeDistanceToTarget();
            var absoluteDistance = Math.Abs(distanceToGo);
            var direction = Math.Sign(distanceToGo);
            if (distanceToGo == 0)
                return 0.0f; // We're there.
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

        /// <summary>
        ///   Computes the distance to target, in the current motor direction.
        /// </summary>
        /// <returns>
        ///   Returns the positive distance (in steps) to the target position. This value is always positive regardless of
        ///   direction of travel.
        /// </returns>
        long ComputeDistanceToTarget()
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
                SetSpeed(ComputeAcceleratedVelocity());
                }
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
            lock (motorUpdateLock)
                {
                regulating = true;
                if (!IsMoving)
                    SetSpeed(ComputeAcceleratedVelocity()); // kick start the step timer.
                }
            }

        /// <summary>
        ///   Stops the motor and raises the <see cref="OnMotorStopped" /> event.
        /// </summary>
        void AllStop()
            {
            stepTimer.Change(Timeout.Infinite, Timeout.Infinite);
            motorSpeed = 0;
            OnMotorStopped(this);
            }

        /// <summary>
        ///   Sets the motor speed.
        /// </summary>
        /// <param name="speed">The speed, in steps per second.</param>
        void SetSpeed(double speed)
            {
            var absoluteSpeed = Math.Abs(speed);
            if (absoluteSpeed < MotorStoppedThreshold)
                {
                AllStop();
                return;
                }
            lock (motorUpdateLock)
                {
                motorSpeed = speed;
                var millisecondsUntilNextStep = (int)Math.Abs(1000/absoluteSpeed);
                stepTimer.Change(millisecondsUntilNextStep, Timeout.Infinite);
                }
            }

        /// <summary>
        ///   Delegate StepHandler - signature of handler methods that handle the step timer tick event.
        /// </summary>
        delegate void StepHandler();
        }
    }
