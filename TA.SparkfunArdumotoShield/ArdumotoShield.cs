using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using TA.NetMF.Utils;

namespace TA.SparkfunArdumotoShield
{
    public sealed class ArdumotoShield
    {
        private SimpleHBridge MotorA;
        private SimpleHBridge MotorB;

        public ArdumotoShield(SimpleHBridge a, SimpleHBridge b)
        {
            MotorA = a;
            MotorB = b;
        }
        /// <summary>
        /// Gets a stepper motor configured for the specified number of microsteps per whole step.
        /// </summary>
        /// <param name="microsteps">The microsteps.</param>
        /// <returns>TA.NetMF.Utils.IStepperMotorControl.</returns>
        public IStepperMotorControl GetStepperMotor(int microsteps)
        {
            return new MicrosteppingStepperMotor(MotorA, MotorB, microsteps);
        }
    }
}
