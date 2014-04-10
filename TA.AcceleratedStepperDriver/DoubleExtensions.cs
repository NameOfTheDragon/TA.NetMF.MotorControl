// This file is part of the TA.NetMF.StepperDriver project
// 
// Copyright © 2014 TiGra Networks, all rights reserved.
// 
// File: DoubleExtensions.cs  Created: 2014-04-09@00:49
// Last modified: 2014-04-10@02:54 by Tim

namespace TA.AcceleratedStepperDriver
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
