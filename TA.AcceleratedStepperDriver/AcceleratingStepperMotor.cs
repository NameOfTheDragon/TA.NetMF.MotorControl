// This file is part of the TA.NetMF.StepperDriver project
// 
// Copyright © 2014 TiGra Networks, all rights reserved.
// 
// File: AcceleratingStepperMotor.cs  Created: 2014-03-26@04:11
// Last modified: 2014-03-28@02:29 by Tim

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
    ///   This class draw inspiration from
    /// </remarks>
    public sealed class AcceleratingStepperMotor
        {
        /// <summary>
        ///   Delegate MicrostepCallback - user-supplied delegate to write step data to the motor hardware.
        /// </summary>
        /// <param name="index">The microstep index.</param>
        public delegate void MicrostepCallback(uint index);

        /// <summary>
        ///   Delegate MotorEventHandler - callback signature usd by the <see cref="AcceleratingStepperMotor.MotorStopped" /> event.
        /// </summary>
        /// <param name="axis">The axis.</param>
        public delegate void MotorEventHandler(AcceleratingStepperMotor axis);

        /// <summary>
        ///   The maximum possible speed that this driver is able to support.
        /// </summary>
        public const int MaximumPossibleSpeed = 1000;

        /// <summary>
        ///   The motor stopped threshold, The speed below which the motor is considered to be stopped.
        /// </summary>
        const double MotorStoppedThreshold = 0.001;

        /// <summary>
        ///   The limit of travel (in steps)
        /// </summary>
        readonly int limitOfTravel;

        /// <summary>
        ///   The number of micro steps per whole step
        /// </summary>
        readonly int microStepsPerStep = 64;

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
        ///   The current direction of travel; 1 forward; 0 stopped; -1 reverse
        /// </summary>
        int direction; // 1=forward; 0=stopped; -1=reverse

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
            stepTimer = new Timer(StepTimerTick, null, 0, 1000); // Stopped
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
        /// Setting this property affects the <see cref="RampTime"/> and vice versa.
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
        public bool IsMoving { get { return motorSpeed > MotorStoppedThreshold; } }

        /// <summary>
        ///   The current direction of travel; 1 forward; 0 stopped; -1 reverse
        /// </summary>
        public int Direction { get { return direction; } }

        /// <summary>
        /// Gets or sets the time in which the motor will accelerate to full speed. Setting this property affects <see cref="Acceleration"/> and vice versa.
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
            StepAndRepeat();
            }

        /// <summary>
        ///   Steps the motor one step in the current direction, then computes and sets the new step speed.
        ///   The act of setting the speed re-arms the step timer.
        /// </summary>
        void StepAndRepeat()
            {
            var canRepeat = MoveOneStep();
            if (!canRepeat)
                {
                AllStop();
                return;
                }
            var nextSpeed = ComputeSpeed();
            SetSpeed(nextSpeed);
            }

        /// <summary>
        ///   Moves one step in the current direction.
        /// </summary>
        /// <returns><c>true</c> if another step in the same direction is possible, <c>false</c> otherwise.</returns>
        bool MoveOneStep()
            {
            if (direction == 0)
                return false;
            performMicrostep(microstep); // user code
            UpdateMicrostep();
            var newPosition = currentPosition + direction;
            if (newPosition > limitOfTravel || newPosition < 0)
                return false;
            currentPosition = newPosition;
            return true;
            }

        /// <summary>
        ///   Updates the microstep index by one step in the current direction..
        /// </summary>
        void UpdateMicrostep()
            {
            microstep = (uint)((microstep + Direction)%microStepsPerStep);
            }

        /// <summary>
        ///   Computes the desired motor speed, in steps per second.
        ///   Takes account of acceleration and deceleration.
        /// </summary>
        /// <returns>System.Single.</returns>
        float ComputeSpeed()
            {
            var distanceToGo = ComputeDistanceToTarget();
            if (distanceToGo == 0)
                return 0.0f; // We're there.
            if (!IsMoving)
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
        /// <returns>
        ///   Returns the positive distance (in steps) to the target position. This value is always positive regardless of
        ///   direction of travel.
        /// </returns>
        long ComputeDistanceToTarget()
            {
            if (currentPosition == targetPosition)
                return 0;
            return Math.Abs(targetPosition - currentPosition);
            }

        /// <summary>
        ///   Moves the motor to the specified position.
        /// </summary>
        /// <param name="target">The target position, in steps.</param>
        public void MoveToTarget(int target)
            {
            var moveDirection = (short)Math.Sign(target - currentPosition);
            if (moveDirection == 0)
                {
                AllStop();
                return;
                }
            targetPosition = target;
            direction = moveDirection;
            SetSpeed(ComputeSpeed());
            }

        /// <summary>
        ///   Stops the motor and raises the <see cref="OnMotorStopped" /> event.
        /// </summary>
        void AllStop()
            {
            stepTimer.Change(Timeout.Infinite, Timeout.Infinite);
            direction = 0;
            motorSpeed = 0;
            OnMotorStopped(this);
            }

        /// <summary>
        ///   Sets the motor speed.
        /// </summary>
        /// <param name="speed">The speed, in steps per second.</param>
        void SetSpeed(double speed)
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
