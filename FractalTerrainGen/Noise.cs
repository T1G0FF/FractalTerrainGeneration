using System;
using Functions;

namespace Noise
{
    static class Constants
    {
        public const int MINLEVEL = 0;
        public const int MAXLEVEL = byte.MaxValue;
    }

    static class Generate
    {
        static Random randGen;
        
        #region Public Methods
        static public byte[,] MultiPassNoise(int passes, int size, float scale, int seed, int maxHeight = Constants.MAXLEVEL)
        {
            int Divisor = 1;
            float passScale = scale * Divisor;
            byte[,] outputArray = Noise(size, passScale, seed, maxHeight);

            for (int pass = 1; pass < passes; pass++)
            {   
                Divisor *= 2;
                passScale = scale * Divisor;

                byte[,] newPass = Noise(size, passScale, seed, maxHeight);
                double alpha = 0.75F;
                for (int xCoord = 0; xCoord < size; xCoord++)
                {
                    for (int yCoord = 0; yCoord < size; yCoord++)
                    {
                        double p0 = outputArray[xCoord, yCoord];
                        double p1 = newPass[xCoord, yCoord];
                        double outputValue = Value.LinearInterpolation(p0, p1, alpha);
                        outputArray[xCoord, yCoord] = (byte)outputValue;
                    }
                }
                newPass = null;
            }
            Console.Clear();

            outputArray = NormaliseNoiseArray(outputArray);

            return outputArray;
        }

        // http://gameprogrammer.com/fractal.html
        static public byte[,] Noise(int size, float scale, int seed, int maxHeight = Constants.MAXLEVEL)
        {
            int Power = (int)Math.Log(size, 2);
            int Size = (int)Math.Pow(2, Power);
            randGen = new Random(seed);

            int Side = Size + 1;
            int Zero = 0;
            byte[,] output = new byte[Side, Side];

            output[Zero, Zero] = getRandomElevation(maxHeight);
            output[Zero, Size] = getRandomElevation(maxHeight);
            output[Size, Zero] = getRandomElevation(maxHeight);
            output[Size, Size] = getRandomElevation(maxHeight);

            int Stride = Size / 2;
            double passScale = scale;

            Console.WriteLine("Generating {0}x{0} at {1:F2}...", Size, scale);
            int Pass = 0;
            while (Stride != 0)
            {
                Console.Write("\r" + "Pass {0}", Pass++);
                int passMaxHeight = (int)Math.Floor(passScale * maxHeight);
                for (int y = Stride; y < Side; y += Stride)
                {
                    for (int x = Stride; x < Side; x += Stride)
                    {
                        int ElevationSeed = getRandomElevation(passMaxHeight);
                        int ExistingAverage = avgSquareValues(ref output, x, y, Stride);
                        int NewElevation = ElevationSeed + ExistingAverage;
                        output[x, y] = (byte)(NewElevation % 255);
                        x += Stride;
                    }
                    y += Stride;
                }

                bool toggle = false;
                for (int y = 0; y < Side; y += Stride)
                {
                    toggle = !toggle;
                    for (int x = 0; x < Side; x += Stride)
                    {
                        if (toggle && x == 0) { x += Stride; }

                        int ElevationSeed = getRandomElevation(passMaxHeight);
                        int ExistingAverage = avgDiamondValues(ref output, x, y, Stride);
                        int NewElevation = ElevationSeed + ExistingAverage;
                        output[x, y] = (byte)(NewElevation % 255);

                        // Wraps Edges
                        if (x == 0) { output[Size, y] = output[x, y]; }
                        if (y == 0) { output[x, Size] = output[x, y]; }

                        x += Stride;
                    }
                }

                passScale = passScale / 2;

                Stride = Stride / 2;
            }
            Console.Write("\r" + "Generation Complete!" + "\n");
            return output;
        }
        #endregion

        #region Private Methods
        static private int avgSquareValues(ref byte[,] tmpArray, int x, int y, int stride)
        {
            return (tmpArray[x - stride, y - stride] +
                    tmpArray[x - stride, y + stride] +
                    tmpArray[x + stride, y - stride] +
                    tmpArray[x + stride, y + stride]) / 4;
        }

        static private int avgDiamondValues(ref byte[,] tmpArray, int x, int y, int stride)
        {
            int Size = tmpArray.GetUpperBound(0);

            if (x == 0)
            {
                return (tmpArray[x, y - stride] +
                        tmpArray[x, y + stride] +
                        tmpArray[Size - stride, y] +
                        tmpArray[x + stride, y]) / 4;
            }
            else if (x == Size)
            {
                return (tmpArray[x, y - stride] +
                        tmpArray[x, y + stride] +
                        tmpArray[x - stride, y] +
                        tmpArray[0 + stride, y]) / 4;
            }
            else if (y == 0)
            {
                return (tmpArray[x - stride, y] +
                        tmpArray[x + stride, y] +
                        tmpArray[x, y + stride] +
                        tmpArray[x, Size - stride]) / 4;
            }
            else if (y == Size)
            {
                return (tmpArray[x - stride, y] +
                        tmpArray[x + stride, y] +
                        tmpArray[x, y - stride] +
                        tmpArray[x, 0 + stride]) / 4;
            }
            else
            {
                return (tmpArray[x - stride, y] +
                        tmpArray[x + stride, y] +
                        tmpArray[x, y - stride] +
                        tmpArray[x, y + stride]) / 4;
            }
        }

        static private byte getRandomElevation()
        {
            return getRandomElevation(Constants.MINLEVEL, Constants.MAXLEVEL);
        }

        static private byte getRandomElevation(int max)
        {
            int output;
            int m = (int)Math.Floor(max / 2F);
            output = getRandomElevation(-m, m);
            return (byte)output;
        }

        static private byte getRandomElevation(int min, int max)
        {
            int diff = (max - min);
            int rand = randGen.Next(diff + 1);
            return (byte)((rand + min));
        }

        static private byte[,] NormaliseNoiseArray(byte[,] NoiseArray)
        {
            int min = Constants.MAXLEVEL;
            int max = Constants.MINLEVEL;

            int xMax = NoiseArray.GetUpperBound(0) + 1;
            int yMax = NoiseArray.GetUpperBound(1) + 1;
            for (int xCoord = 0; xCoord < xMax; xCoord++)
            {
                for (int yCoord = 0; yCoord < yMax; yCoord++)
                {
                    int currentValue = NoiseArray[xCoord, yCoord];
                    if (currentValue > max) max = currentValue;
                    if (currentValue < min) min = currentValue;
                }
            }

            for (int xCoord = 0; xCoord < xMax; xCoord++)
            {
                for (int yCoord = 0; yCoord < yMax; yCoord++)
                {
                    int currentValue = NoiseArray[xCoord, yCoord];
                    NoiseArray[xCoord, yCoord] = (byte)Value.Normalise(currentValue, min, max, Constants.MINLEVEL, Constants.MAXLEVEL);
                }
            }

            return NoiseArray;
        }
        #endregion
    }
}
