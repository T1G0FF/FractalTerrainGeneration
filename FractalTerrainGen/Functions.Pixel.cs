using System;
using System.Drawing;

namespace Functions
{
    static class Pixel
    {
        /// <summary>
        /// Transforms a given RGB colour by a given hue.
        /// </summary>
        /// <param name="colorIn">The colour to transform.</param>
        /// <param name="Hue">The number of degrees to transform the colour by.</param>
        /// <returns>The transformed colour.</returns>
        /// <seealso cref="http://stackoverflow.com/questions/8507885/shift-hue-of-an-rgb-color"/>
        static public Color TransformHue(Color colorIn, int Hue)
        {
            float cosA = (float)Math.Cos(Hue * Math.PI / 180);  // Convert degrees to radians
            float sinA = (float)Math.Sin(Hue * Math.PI / 180);  // Convert degrees to radians
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

        static public Color Gradient(byte value, byte min, byte max, Color Colour1, Color Colour2)
        {
            byte R = Gradient(value, min, max, Colour1.R, Colour2.R);
            byte G = Gradient(value, min, max, Colour1.G, Colour2.G);
            byte B = Gradient(value, min, max, Colour1.B, Colour2.B);

            return Color.FromArgb(R, G, B);
        }

        static public byte Gradient(byte value, byte min, byte max, byte SubColour1, byte SubColour2)
        {
            if(SubColour1 > SubColour2)
            {
                byte deltaColour = (byte)(SubColour1 - SubColour2);
                return (byte)Value.Clamp((SubColour1 - getColorBlend(value, min, max, deltaColour)), 255);
            }
            else
            {
                byte deltaColour = (byte)(SubColour2 - SubColour1);
                return (byte)Value.Clamp((SubColour1 + getColorBlend(value, min, max, deltaColour)), 255);
            }
            
        }

        static private byte getColorBlend(byte value, byte min, byte max, byte colorDelta)
        {
            double colorRatio = (double)(value - min) / (double)(max - min);
            return (byte)Value.Clamp(Math.Round(colorRatio * colorDelta), 255);
        }
    }
}
