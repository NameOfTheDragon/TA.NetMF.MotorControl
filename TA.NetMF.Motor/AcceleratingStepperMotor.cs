using System;
using System.Threading;

namespace TA.NetMF.Motor
    {
    public class AcceleratingStepperMotor : StepperMotor
        {
        /// <summary>
        ///   The acceleration factor
        /// </summary>
        double acceleration = 1.0f;

        public AcceleratingStepperMotor(int limitOfTravel, IStepSequencer stepper) : base(limitOfTravel, stepper)
            {
            
            }

        /// <summary>
        ///   Gets or sets the acceleration rate in steps per second per second.
        ///   Setting this property affects the <see cref="RampTime" /> and vice versa.
        /// </summary>
        /// <value>The acceleration, in steps per second per second.</value>
        public double Acceleration { get { return acceleration; } set { acceleration = value; } }

        /// <summary>
        ///   Gets or sets the time in which the motor will accelerate to full speed. Setting this property affects
        ///   <see cref="Acceleration" /> and vice versa.
        /// </summary>
        /// <value>The ramp time, in seconds.</value>
        public double RampTime { get { return MaximumSpeed/Acceleration; } set { Acceleration = MaximumSpeed/value; } }

        /// <summary>
        ///   Computes the new motor velocity based on the current velocity, such that the velocity always moves towards
        ///   the set point (if regulation is active), or the acceleration curve dictated by the <see cref="Acceleration" />
        ///   property.
        ///   The magnitude of the returned speed will never be greater than <see cref="StepperMotor.MaximumSpeed" /> which in turn can never
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

        /// <summary>
        ///   Handles the step timer tick event.
        /// </summary>
        /// <param name="state">Not used.</param>
        /// <remarks>
        /// Beware! Tick events can still fire even after the timer has been disabled.
        /// </remarks>
        protected virtual void StepTimerTick(object state)
            {
            if (IsMoving)
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

        protected override void StartStepping(short moveDirection)
            {
            SetSpeed(ComputeAcceleratedVelocity());
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
            var millisecondsUntilNextStep = (int)(1000.0 / absoluteSpeed);
            lock (motorUpdateLock)
                {
                motorSpeed = speed;
                stepTimer.Change(millisecondsUntilNextStep, Timeout.Infinite);
                }

            }
        }
    }