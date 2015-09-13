namespace Functions
{
    static class Value
    {
        /// <summary>
        /// Clamps a value between 0 and 1.
        /// </summary>
        /// <param name="value">The number to clamp.</param>
        /// <returns>The clamped value as a double.</returns>
        static public double Clamp(double value)
        {
            return Clamp(value, 0, 1);
        }

        /// <summary>
        /// Clamps a value between 0 and a given maximum.
        /// </summary>
        /// <param name="value">The number to clamp.</param>
        /// <param name="max">The maximum value the number can have.</param>
        /// <returns>The clamped value as a double.</returns>
        static public double Clamp(double value, double max)
        {
            return Clamp(value, 0, max);
        }

        /// <summary>
        /// Clamps a value between a given minimum and maximum. 
        /// </summary>
        /// <param name="value">The number to clamp.</param>
        /// <param name="min">The minimum value the number can have.</param>
        /// <param name="max">The maximum value the number can have.</param>
        /// <returns>The clamped value as a double.</returns>
        static public double Clamp(double value, double min, double max)
        {
            if (value < min) { return min; }
            if (value > max) { return max; }
            return value;
        }

        /// <summary>
        /// Rescales a value from a given range to a value between 0 and 1. 
        /// </summary>
        /// <param name="value">The value to be Normalised.</param>
        /// <param name="currentFloor">The minimum of the value's range.</param>
        /// <param name="currentCeiling">The maximum of the value's range.</param>
        /// <returns>The normalised value as a double.</returns>
        static public double Normalise(double value, double currentFloor, double currentCeiling)
        {
            return Normalise(value, currentFloor, currentCeiling, 0, 1);
        }

        /// <summary>
        /// Rescales a value from a given range to a value between another given range. 
        /// </summary>
        /// <param name="value">The value to be Normalised.</param>
        /// <param name="currentFloor">The original minimum range.</param>
        /// <param name="currentCeiling">The original maximum range.</param>
        /// <param name="newFloor">The new minimum range.</param>
        /// <param name="newCeiling">The new maximum range.</param>
        /// <returns>The normalised value as a double.</returns>
        static public double Normalise(double value, double currentFloor, double currentCeiling, double newFloor, double newCeiling)
        {
            return ((newCeiling - newFloor) * (value - currentFloor)) / (currentCeiling - currentFloor) + newFloor;
        }

        /// <summary>
        /// Returns the weighted average of 2 values.
        /// </summary>
        /// <param name="value1">The first value to Interpolate.</param>
        /// <param name="value2">The second value to Interpolate.</param>
        /// <param name="weight">The weight to assign the first value, where weight is a value between 0 and 1.</param>
        /// <returns>The weighted average value as a double.</returns>
        static public double LinearInterpolation(double value1, double value2, double weight)
        {
            weight = Clamp(weight);
            return (1 - weight) * value2 + weight * value1;
        }
    }
}
