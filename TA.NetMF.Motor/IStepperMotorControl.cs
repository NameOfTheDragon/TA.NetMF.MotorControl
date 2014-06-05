// This file is part of the TA.NetMF.MotorControl project
// 
// Copyright © 2014 Tigra Astronomy, all rights reserved.
// This source code is licensed under Creative Commons Attribution International 4.0 license
// http://creativecommons.org/licenses/by/4.0/
// 
// File: IStepperMotorControl.cs  Created: 2014-06-05@02:27
// Last modified: 2014-06-05@12:23 by Tim

namespace TA.NetMF.Motor
    {
    public interface IStepperMotorControl
        {
        /// <summary>
        ///   Configures the motor coils for the specified microstep index.
        /// </summary>
        /// <param name="direction">The direction, +1 for forwards, -1 for reverse, 0 for stop.</param>
        void PerformMicrostep(int direction);
        }
    }
