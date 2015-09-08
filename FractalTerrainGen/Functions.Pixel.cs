using System;
using System.Drawing;

namespace Functions
{
    static class Pixel
    {
        static public Color TransformHue(Color colorIn, byte Hue)
        {
            // http://stackoverflow.com/questions/8507885/shift-hue-of-an-rgb-color
            float cosA = (float)Math.Cos(Hue * Math.PI / 180);
            float sinA = (float)Math.Sin(Hue * Math.PI / 180);
            double[][] matrix = new double[][] {
                new double[] { cosA + (1.0F - cosA) / 3.0F, 1.0F / 3.0F * (1.0F - cosA) - Math.Sqrt(1.0F / 3.0F) * sinA, 1.0F / 3.0F * (1.0F - cosA) + Math.Sqrt(1.0F / 3.0F) * sinA },
                new double[] { 1.0F / 3.0F * (1.0F - cosA) + Math.Sqrt(1.0F / 3.0F) * sinA, cosA + 1.0F / 3.0F * (1.0F - cosA), 1.0F / 3.0F * (1.0F - cosA) - Math.Sqrt(1.0F / 3.0F) * sinA },
                new double[] { 1.0F / 3.0F * (1.0F - cosA) - Math.Sqrt(1.0F / 3.0F) * sinA, 1.0F / 3.0F * (1.0F - cosA) + Math.Sqrt(1.0F / 3.0F) * sinA, cosA + 1.0F / 3.0F * (1.0F - cosA) }
            };

            byte R = (byte)Value.Clamp(colorIn.R * matrix[0][0] + colorIn.G * matrix[0][1] + colorIn.B * matrix[0][2], 255);
            byte G = (byte)Value.Clamp(colorIn.R * matrix[1][0] + colorIn.G * matrix[1][1] + colorIn.B * matrix[1][2], 255);
            byte B = (byte)Value.Clamp(colorIn.R * matrix[2][0] + colorIn.G * matrix[2][1] + colorIn.B * matrix[2][2], 255);

            return Color.FromArgb(R, G, B);
        }

        static public byte GradientUp(byte value, byte min, byte max, byte minColor, byte maxColor)
        {
            byte deltaColor = (byte)(maxColor - minColor);
            return (byte)(maxColor - getColorBlend(value, min, max, deltaColor));
        }

        static public byte GradientDown(byte value, byte min, byte max, byte minColor, byte maxColor)
        {
            byte deltaColor = (byte)(maxColor - minColor);
            return (byte)(minColor + getColorBlend(value, min, max, deltaColor));
        }

        static private byte getColorBlend(byte value, byte min, byte max, byte colorDelta)
        {
            double colorRatio = (double)(value - min) / (double)(max - min);
            return (byte)Math.Round(colorRatio * colorDelta);
        }
    }
}
