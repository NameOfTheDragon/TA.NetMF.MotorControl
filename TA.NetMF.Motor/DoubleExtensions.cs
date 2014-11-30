// This file is part of the TA.NetMF.MotorControl project
// 
// Copyright © 2014-2014 Tigra Astronomy, all rights reserved.
// This source code is licensed under the MIT License, see http://opensource.org/licenses/MIT
// 
// File: DoubleExtensions.cs  Created: 2014-06-05@12:18
// Last modified: 2014-11-30@13:57 by Tim
namespace TA.NetMF.Motor
    {
    public static class DoubleExtensions
        {
        /// <summary>
        ///   Constrains (or clips) the value to the specified limits. The limits may be in any order.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="limit1">The minimum.</param>
        /// <param name="limit2">The maximum.</param>
        /// <returns>System.Double.</returns>
        public static double ConstrainToLimits(this double value, double limit1, double limit2)
            {
            double max, min;
            if (limit1 > limit2)
                {
                max = limit1;
                min = limit2;
                }
            else
                {
                max = limit2;
                min = limit1;
                }
            if (value > max)
                return max;
            if (value < min)
                return min;
            return value;
            }
        }
    }
