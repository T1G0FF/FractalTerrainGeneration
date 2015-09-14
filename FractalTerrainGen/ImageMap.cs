﻿using System;
using System.Drawing;
using System.IO;
using xRay.Toolkit.Utilities;
using Noise;
using Functions;
using System.Drawing.Imaging;

namespace FractalTerrainGen
{
    class ImageMap : BaseMap
    {
        #region Public
        public const int MIN_SIZE = 8;
        public const int MAX_SIZE = 8192;   // Size^2 * 32 Bits per Pixel = File Size (256MB limit)
        public const int DEFAULT_SIZE = 512;
        public const int TERRAIN_HEIGHT = Constants.MAXLEVEL;
        public const float DEFAULT_SEALEVEL = 0.61F;
        private int MAX_POWER = (int)Math.Log(MAX_SIZE, 2);
        
        public enum WriteOption { None, Overwrite, SaveSeperate }

        public float SealevelScale { get; private set; }
        byte Sealevel { get { return (byte)Math.Floor((double)TERRAIN_HEIGHT * SealevelScale); } }
        byte Mountains { get { return (byte)Math.Floor((Sealevel + ((TERRAIN_HEIGHT - Sealevel) * 0.7D))); } }
        #endregion

        #region Constructors
        public ImageMap()
            : this(Guid.NewGuid().GetHashCode())
        { }

        public ImageMap(int seed, int size = DEFAULT_SIZE, float scale = DEFAULT_SCALE, float sealevel = DEFAULT_SEALEVEL, int passes = DEFAULT_PASSES)
        {
            bool changes = false;

            if (size > MAX_SIZE)
            {
                Console.WriteLine("Sizes above 8192 cause Bitmap files greater than 1GB to generate.");
                changes = true;
            }
            size = (int)Value.Clamp(size, MIN_SIZE, MAX_SIZE);

            for (int Power = MAX_POWER; Power >= 2; Power--)
            {
                int powerOfTwo = (int)Math.Pow(2, Power);

                if (size > (powerOfTwo * 0.9F))
                {
                    Size = powerOfTwo;
                    break;
                }
            }

            Seed = seed;
            SealevelScale = (float)Value.Clamp(sealevel);
            if (SealevelScale != sealevel)
            {
                Console.WriteLine("Sealevel must be between 0 and 1 (0 < {0} < 1)", SealevelScale);
                changes = true;
            }

            // (2^Passes * Scale * MAXVALUE) must be less than (int.MaxValue)

            double MaxPasses = Generate.getMaxPasses(scale);
            Passes = (int)Value.Clamp(passes, 1, MaxPasses);

            double MaxScale = Generate.getMaxScale(Passes);
            Scale = (float)Value.Clamp(scale, MaxScale);

            if (Passes != passes || Scale != scale)
            {
                double limit = (int)(TERRAIN_HEIGHT * MaxScale * Math.Pow(2, Passes));
                Console.WriteLine("Passes is now {0} and", Passes);
                Console.WriteLine("Scale is now {0} to ensure that:", Scale);
                Console.WriteLine("(MaxNoise * Scale * 2^Passes) <= int.MaxValue");
                Console.WriteLine("({0} * {1} * 2^{2}) = {3} <= {4}", TERRAIN_HEIGHT, (int)MaxScale, Passes, (int)limit, int.MaxValue);
                changes = true;
            }

            if (changes)
            {
                Console.ReadKey(true);
                Console.Clear();
            }

            TerrainMap = Generate.MultiPassNoise(Passes, Size, Scale, Seed);
        }
        #endregion

        #region Public Methods
        public void SaveToImage(string filePath, string function, WriteOption option = WriteOption.None)
        {
            switch (option)
            {
                default:
                case WriteOption.SaveSeperate:
                    SaveToImage(filePath, function, false);
                    break;

                case WriteOption.Overwrite:
                    SaveToImage(filePath, function, true);
                    break;

                case WriteOption.None:
                    SingleMap(filePath, function);
                    break;
            }
        }
        #endregion

        #region Private Methods
        private void SaveToImage(string folderPath, string function, bool overwrite)
        {
            string fileName;
            string filePath;
            PixelFormat formatType = PixelFormat.Format32bppRgb;
            Func<byte, Color> delegateName;

            switch (function)
            {
                case "Greyscale":
                    fileName = String.Format("{0}_{1}_{2:F2}_{3}_NoiseMap.png", Seed, Size, Scale, Passes);
                    delegateName = GreyScale;
                    break;

                case "Smooth":
                    fileName = String.Format("{0}_{1}_{2:F2}_{3:F2}_{4}_Realism.png", Seed, Size, Scale, SealevelScale, Passes);
                    delegateName = SmoothGradient;
                    break;

                case "Color":
                case "Colour":
                    fileName = String.Format("{0}_{1}_{2:F2}_{3:F2}_{4}_Contours.png", Seed, Size, Scale, SealevelScale, Passes);
                    delegateName = BlockGradient;
                    break;

                case "BlockTexture":
                    fileName = String.Format("{0}_{1}_{2:F2}_{3:F2}_{4}_TexturedContours.png", Seed, Size, Scale, SealevelScale, Passes);
                    delegateName = TexturedBlockGradient;
                    break;

                case "SmoothTexture":
                    fileName = String.Format("{0}_{1}_{2:F2}_{3:F2}_{4}_TexturedRealism.png", Seed, Size, Scale, SealevelScale, Passes);
                    delegateName = TexturedSmoothGradient;
                    break;

                default:
                    return;
            }

            if (!Directory.Exists(folderPath)) { Directory.CreateDirectory(folderPath); }

            filePath = folderPath + fileName;

            if (File.Exists(filePath) == false || overwrite)
            {
                Bitmap TerrainImage = ToImage(fileName, formatType, delegateName);
                TerrainImage.Save(filePath, ImageFormat.Png);
            }
        }

        private void SingleMap(string folderPath, string function)
        {
            string fileName;
            string filePath;
            PixelFormat formatType = PixelFormat.Format32bppRgb;
            Func<byte, Color> delegateName;

            switch (function)
            {
                case "Greyscale":
                    fileName = String.Format("CurrentMap_NoiseMap.png");
                    delegateName = GreyScale;
                    break;

                case "Smooth":
                    fileName = String.Format("CurrentMap_Realism.png");
                    delegateName = SmoothGradient;
                    break;

                case "Color":
                case "Colour":
                    fileName = String.Format("CurrentMap_Contours.png");
                    delegateName = BlockGradient;
                    break;

                case "BlockTexture":
                    fileName = String.Format("CurrentMap_TexturedContours.png");
                    delegateName = TexturedBlockGradient;
                    break;

                case "SmoothTexture":
                    fileName = String.Format("CurrentMap_TexturedRealism.png");
                    delegateName = TexturedSmoothGradient;
                    break;

                default:
                    return;
            }

            if (!Directory.Exists(folderPath)) { Directory.CreateDirectory(folderPath); }

            filePath = folderPath + fileName;

            Bitmap TerrainImage = ToImage(fileName, formatType, delegateName);
            TerrainImage.Save(filePath, ImageFormat.Png);
        }

        private Bitmap ToImage(string fileName, PixelFormat pixelFormat, Func<byte, Color> getTerrainColour)
        {
            Bitmap TerrainImage = new Bitmap(Size, Size, pixelFormat);
            NonBlockingConsole.WriteLine(String.Format("Writing to Image [{0}]...", fileName));
            int xMax = TerrainMap.GetUpperBound(0); // Returns Index
            int yMax = TerrainMap.GetUpperBound(1); // Number of elements = Index + 1

            for (int xCoord = 0; xCoord < xMax; xCoord++)
            {
                for (int yCoord = 0; yCoord < yMax; yCoord++)
                {
                    byte current = TerrainMap[xCoord, yCoord];
                    Color clr = getTerrainColour(current);
                    TerrainImage.SetPixel(xCoord, yCoord, clr);
                }
            }
            NonBlockingConsole.WriteLine(String.Format("\r" + "{0,-59}", String.Format("{0} Complete", fileName)));
            return TerrainImage;
        }

        private Color GreyScale(byte currentElevation)
        {
            return Color.FromArgb(currentElevation, currentElevation, currentElevation);
        }

        private Color BlockGradient(byte currentElevation)
        {
            int Divide;
            Color result = Color.Empty;

            if (currentElevation > Mountains)
            {
                Divide = (int)Math.Floor((TERRAIN_HEIGHT - Mountains) / 4D);
                if (currentElevation < Mountains + (Divide * 1))
                    result = Color.FromArgb(128, 128, 128);
                else if (currentElevation < Mountains + (Divide * 2))
                    result = Color.FromArgb(156, 156, 156);
                else if (currentElevation < Mountains + (Divide * 3))
                    result = Color.FromArgb(183, 183, 183);
                else
                    result = Color.FromArgb(211, 211, 211);
            }
            else if (currentElevation < Sealevel)
            {
                Divide = (int)Math.Floor(Sealevel / 4D);
                if (currentElevation > Sealevel - (Divide * 1))
                    result = Color.FromArgb(135, 206, 250);
                else if (currentElevation > Sealevel - (Divide * 2))
                    result = Color.FromArgb(123, 187, 246);
                else if (currentElevation > Sealevel - (Divide * 3))
                    result = Color.FromArgb(112, 168, 241);
                else
                    result = Color.FromArgb(100, 149, 237);
            }
            else
            {
                Divide = (int)Math.Floor((Mountains - Sealevel) / 4D);
                int LandStart = Sealevel + (Divide * 1);
                if (currentElevation < LandStart - Divide / 2)
                    result = Color.PowderBlue;
                else if (currentElevation < LandStart)
                    result = Color.FromArgb(255, 242, 226);
                else if (currentElevation < Sealevel + (Divide * 2))
                    result = Color.FromArgb(60, 179, 113);
                else if (currentElevation < Sealevel + (Divide * 3))
                    result = Color.FromArgb(53, 159, 100);
                else
                    result = Color.FromArgb(46, 139, 87);
            }
            return result;
        }
        private Color TexturedBlockGradient(byte currentElevation)
        {
            Color Block = BlockGradient(currentElevation);
            return TexturedGradient(Block, currentElevation);
        }

        private Color TexturedSmoothGradient(byte currentElevation)
        {
            Color Smooth = SmoothGradient(currentElevation);
            return TexturedGradient(Smooth, currentElevation);
        }

        private Color TexturedGradient(Color currentColor, byte currentElevation)
        {
            // Custom
            int Sandlevel = (int)(Sealevel + Math.Floor((Mountains - Sealevel) / 4D));
            if (currentElevation < Sealevel)
            {
                double elev = Value.Normalise(currentElevation, 0, Sealevel, 0.25, 0.75);
                currentColor = RGBHSL.SetBrightness(currentColor, elev);
                return currentColor;
            }
            else if (currentElevation < Sandlevel)
            {
                return currentColor;
            }
            else if (currentElevation <= Mountains)
            {
                double elev = Value.Normalise(currentElevation, Sandlevel, TERRAIN_HEIGHT, 0.25, 0.75);
                //double sat = currentColor.GetSaturation();
                currentColor = RGBHSL.SetSaturation(currentColor, elev);
                return currentColor;
            }
            else
            {
                return currentColor;
            }

            /*
            // Brightness - Water Only
            if (currentElevation < Sealevel)
            {
                double elev = Value.Normalise(currentElevation, 0, Sealevel, 0.25, 0.75);
                currentColor = RGBHSL.SetBrightness(currentColor, elev);
                return currentColor;
            }
            else
            {
                return currentColor;
            }

            // Average
            byte R = (byte)((currentColor.R + currentElevation) / 2);
            byte G = (byte)((currentColor.G + currentElevation) / 2);
            byte B = (byte)((currentColor.B + currentElevation) / 2);
            return Color.FromArgb(R, G, B);

            // Linear Interpolation
            float alpha = 0.25F;
            byte R = (byte)Value.LinearInterpolation(currentColor.R, currentElevation,alpha);
            byte G = (byte)Value.LinearInterpolation(currentColor.G, currentElevation,alpha);
            byte B = (byte)Value.LinearInterpolation(currentColor.B, currentElevation,alpha);
            return Color.FromArgb(R, G, B);

            // Hue Transform
            return Pixel.TransformHue(currentColor, currentElevation);

            // Brightness Transform
            double brightness = Value.Normalise(currentElevation, 0, TERRAINHEIGHT, .25, .75);
            return RGBHSL.SetBrightness(currentColor, brightness);
            */
        }

        private Color SmoothGradient(byte currentElevation)
        {
            Color result;

            int OceanDivide = (int)Math.Floor((Sealevel) / 4D);
            byte DropOff = (byte)(OceanDivide * 3);

            int ShallowsDivide = (int)Math.Floor((Mountains - Sealevel) / 6D);
            byte ShallowsLine = (byte)(Sealevel + ShallowsDivide);

            int ShoreDivide = (int)Math.Floor((Mountains - ShallowsLine) / 6D);
            byte ShoreLine = (byte)(ShallowsLine + ShoreDivide);
            byte LandLine = (byte)(ShallowsLine + (ShoreDivide * 2));

            if (currentElevation < Sealevel)
            {
                if (currentElevation < DropOff)
                    result = Pixel.Gradient(currentElevation, 0, DropOff, TerrainType.deepocean, TerrainType.midocean);
                else
                    result = Pixel.Gradient(currentElevation, DropOff, Sealevel, TerrainType.midocean, TerrainType.seashore);
            }
            else if (currentElevation < Mountains)
            {
                if (currentElevation < ShallowsLine)
                    result = Pixel.Gradient(currentElevation, Sealevel, ShallowsLine, TerrainType.shallows, TerrainType.beach);
                else
                {
                    if (currentElevation < ShoreLine)
                        result = Pixel.Gradient(currentElevation, ShallowsLine, ShoreLine, TerrainType.beach, TerrainType.beachgrass);
                    else if (currentElevation < LandLine)
                        result = Pixel.Gradient(currentElevation, ShoreLine, LandLine, TerrainType.beachgrass, TerrainType.grass);
                    else
                        result = Pixel.Gradient(currentElevation, ShoreLine, Mountains, TerrainType.grass, TerrainType.forest);
                }
            }
            else
                result = Pixel.Gradient(currentElevation, Mountains, TERRAIN_HEIGHT, TerrainType.lowermountains, TerrainType.uppermountains);

            return result;
        }
        #endregion
    }

    struct TerrainType
    {
        public static Color deepocean       = Color.FromArgb(18, 25, 35);
        public static Color midocean        = Color.FromArgb(49, 73, 107);
        public static Color seashore        = Color.FromArgb(41, 110, 156);
        public static Color shallows        = Color.FromArgb(41, 110, 156);
        public static Color beach           = Color.FromArgb(209, 194, 166);
        public static Color beachgrass      = Color.FromArgb(71, 91, 67);
        public static Color grass           = Color.FromArgb(57, 81, 57);
        public static Color forest          = Color.FromArgb(30, 44, 35);
        public static Color lowermountains  = Color.FromArgb(39, 44, 36);
        public static Color uppermountains  = Color.FromArgb(177, 181, 189);
    }
}
