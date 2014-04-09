using System;
using Microsoft.SPOT;

namespace TA.AcceleratedStepperDriver
{
    public static class DoubleExtensions
    {
        public static double ConstrainToLimits(this double value, double min, double max)
            {
            if (value > max)
                return max;
            if (value < min)
                return min;
            return value;
            }
    }
}
