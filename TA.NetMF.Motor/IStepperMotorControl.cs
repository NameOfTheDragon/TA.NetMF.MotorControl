// This file is part of the TA.NetMF.MotorControl project
// 
// Copyright © 2014-2014 Tigra Astronomy, all rights reserved.
// This source code is licensed under the MIT License, see http://opensource.org/licenses/MIT
// 
// File: IStepperMotorControl.cs  Created: 2014-06-05@02:27
// Last modified: 2014-11-30@13:57 by Tim
namespace TA.NetMF.Motor
    {
    public interface IStepperMotorControl
        {
        /// <summary>
        ///   Configures the motor coils for the specified microstep index.
        /// </summary>
        /// <param name="direction">The direction, +1 for forwards, -1 for reverse, 0 for stop.</param>
        void PerformMicrostep(int direction);

        /// <summary>
        /// Releases the holding torque by de-energizing all coils.
        /// This allows the motor to rotate freely.
        /// </summary>
        void ReleaseHoldingTorque();
        }
    }
