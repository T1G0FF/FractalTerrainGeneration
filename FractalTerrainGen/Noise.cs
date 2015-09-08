using System;
using Functions;

namespace Noise
{
    static class Generate
    {
        static Random randGen;

        #region Public Methods
        static public byte[,] MultiPassNoise(int passes, int size, float scale, int seed, int TerrainHeight = byte.MaxValue)
        {
            byte[][,] TerrainPasses = new byte[passes][,];

            int Divisor = 1;
            for (int pass = 0; pass < passes; pass++)
            {
                float scaleTemp = (float)scale * Divisor; //(float)Math.Pow(scale, Divisor);
                TerrainPasses[pass] = Noise(size, scaleTemp, seed, TerrainHeight);
                Divisor *= 2;
            }
            Console.Clear();

            byte[,] outputArray = new byte[size, size];
            double alpha = 0.75F;
            for (int xCoord = 0; xCoord < size; xCoord++)
            {
                for (int yCoord = 0; yCoord < size; yCoord++)
                {
                    double outputValue = TerrainPasses[0][xCoord, yCoord];
                    for (int pass = 1; pass < passes; pass++)
                    {
                        double p0 = outputValue;
                        double p1 = TerrainPasses[pass][xCoord, yCoord];
                        outputValue = Value.LinearInterpolation(p0, p1, alpha);
                    }
                    outputArray[xCoord, yCoord] = (byte)outputValue;
                }
            }

            outputArray = NormaliseMap(outputArray);

            return outputArray;
        }

        // http://gameprogrammer.com/fractal.html
        static public byte[,] Noise(int size, float deltaTerrain, int seed, int TerrainHeight = byte.MaxValue)
        {
            int Power = (int)Math.Log(size, 2);
            int Size = (int)Math.Pow(2, Power);
            randGen = new Random(seed);

            int Side = Size + 1;
            int Zero = 0;
            byte[,] output = new byte[Side, Side];

            output[Zero, Zero] = getRandomElevation(TerrainHeight);
            output[Zero, Size] = getRandomElevation(TerrainHeight);
            output[Size, Zero] = getRandomElevation(TerrainHeight);
            output[Size, Size] = getRandomElevation(TerrainHeight);

            int Stride = Size / 2;
            double DeltaTerrain = deltaTerrain;

            Console.WriteLine("Generating {0}x{0} at {1:F2}...", Size, deltaTerrain);
            int Pass = 0;
            while (Stride != 0)
            {
                Console.Write("\r" + "Pass {0}", Pass++);
                for (int y = Stride; y < Side; y += Stride)
                {
                    for (int x = Stride; x < Side; x += Stride)
                    {
                        int Current = getRandomElevation((int)Math.Floor(DeltaTerrain * TerrainHeight)) + avgSquareValues(ref output, x, y, Stride);
                        output[x, y] = (byte)Current;
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

                        int Current = getRandomElevation((int)Math.Floor(DeltaTerrain * TerrainHeight)) + avgDiamondValues(ref output, x, y, Stride);
                        output[x, y] = (byte)Current;

                        // Wraps Edges
                        if (x == 0) { output[Size, y] = output[x, y]; }
                        if (y == 0) { output[x, Size] = output[x, y]; }

                        x += Stride;
                    }
                }

                DeltaTerrain = DeltaTerrain / 2;

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
            return getRandomElevation(0, byte.MaxValue);
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

        static private byte[,] NormaliseMap(byte[,] TerrainMap)
        {
            int min = byte.MaxValue;
            int max = 0;

            int xMax = TerrainMap.GetUpperBound(0) + 1;
            int yMax = TerrainMap.GetUpperBound(1) + 1;
            for (int xCoord = 0; xCoord < xMax; xCoord++)
            {
                for (int yCoord = 0; yCoord < yMax; yCoord++)
                {
                    int currentValue = TerrainMap[xCoord, yCoord];
                    if (currentValue > max) max = currentValue;
                    if (currentValue < min) min = currentValue;
                }
            }

            for (int xCoord = 0; xCoord < xMax; xCoord++)
            {
                for (int yCoord = 0; yCoord < yMax; yCoord++)
                {
                    int currentValue = TerrainMap[xCoord, yCoord];
                    TerrainMap[xCoord, yCoord] = (byte)Value.Normalise(currentValue, min, max, 0, byte.MaxValue);
                }
            }

            return TerrainMap;
        }
        #endregion
    }
}
