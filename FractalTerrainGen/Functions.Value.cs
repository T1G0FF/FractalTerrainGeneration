namespace Functions
{
    static class Value
    {
        static public double Clamp(double value)
        {
            return Clamp(value, 0, 1);
        }

        static public double Clamp(double value, double max)
        {
            return Clamp(value, 0, max);
        }

        static public double Clamp(double value, double min, double max)
        {
            if (value < min) { return min; }
            if (value > max) { return max; }
            return value;
        }

        static public double Normalise(double value, double currentFloor, double currentCeiling)
        {
            return Normalise(value, currentFloor, currentCeiling, 0, 1);
        }

        static public double Normalise(double value, double currentFloor, double currentCeiling, double newFloor, double newCeiling)
        {
            return ((newCeiling - newFloor) * (value - currentFloor)) / (currentCeiling - currentFloor) + newFloor;
        }

        static public double LinearInterpolation(double value1, double value2, double alpha)
        {
            return (1 - alpha) * value2 + alpha * value1;
        }
    }
}
