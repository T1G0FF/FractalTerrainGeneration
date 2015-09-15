using System;
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

        public float SeaLevelScale  { get; private set; }

        byte MountainHeight { get { return (byte)(TERRAIN_HEIGHT - Mountains); } }
        byte LandHeight     { get { return (byte)(TERRAIN_HEIGHT - SeaLevel); } }

        byte Mountains      { get { return (byte)(SeaLevel + Math.Floor(LandHeight * 0.7F)); } }
        byte Dirt           { get { return (byte)(SeaLevel + Math.Floor(LandHeight * 0.25F)); } }
        byte Sand           { get { return (byte)(SeaLevel + Math.Floor(LandHeight * 0.2F)); } }
        byte Shallows       { get { return (byte)(SeaLevel + Math.Floor(LandHeight * 0.1F)); } }
        byte SeaLevel       { get { return (byte)Math.Floor(TERRAIN_HEIGHT * SeaLevelScale); } }
        byte OceanDropOff   { get { return (byte)Math.Floor(SeaLevel * 0.75F); } }
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
            SeaLevelScale = (float)Value.Clamp(sealevel);
            if (SeaLevelScale != sealevel)
            {
                Console.WriteLine("Sealevel must be between 0 and 1 (0 < {0} < 1)", SeaLevelScale);
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
                    fileName = String.Format("{0}_{1}_{2:F2}_{3:F2}_{4}_Realism.png", Seed, Size, Scale, SeaLevelScale, Passes);
                    delegateName = SmoothGradient;
                    break;

                case "Block":
                    fileName = String.Format("{0}_{1}_{2:F2}_{3:F2}_{4}_Contours.png", Seed, Size, Scale, SeaLevelScale, Passes);
                    delegateName = BlockGradient;
                    break;

                case "BlockTexture":
                    fileName = String.Format("{0}_{1}_{2:F2}_{3:F2}_{4}_TexturedContours.png", Seed, Size, Scale, SeaLevelScale, Passes);
                    delegateName = TexturedBlockGradient;
                    break;

                case "SmoothTexture":
                    fileName = String.Format("{0}_{1}_{2:F2}_{3:F2}_{4}_TexturedRealism.png", Seed, Size, Scale, SeaLevelScale, Passes);
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

                case "Block":
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
            int yMax = TerrainMap.GetUpperBound(1);

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

            if (currentElevation < SeaLevel)
            {   // Sea
                Divide = (int)Math.Floor(SeaLevel / 4D);
                if (currentElevation > SeaLevel - (Divide * 1))
                    result = TerrainColour.Simple.upperocean;
                else if (currentElevation > SeaLevel - (Divide * 2))
                    result = TerrainColour.Simple.middleocean;
                else if (currentElevation > SeaLevel - (Divide * 3))
                    result = TerrainColour.Simple.lowerocean;
                else
                    result = TerrainColour.Simple.seafloor;
            }
            else if(currentElevation < Mountains)
            {   // Land
                Divide = (int)Math.Floor(LandHeight / 4D);
                if (currentElevation < Shallows)
                    result = TerrainColour.Simple.shallows;
                else if (currentElevation < Sand)
                    result = TerrainColour.Simple.beach;
                else if (currentElevation < SeaLevel + (Divide * 2))
                    result = TerrainColour.Simple.lowergrass;
                else if (currentElevation < SeaLevel + (Divide * 3))
                    result = TerrainColour.Simple.middlegrass;
                else
                    result = TerrainColour.Simple.uppergrass;
            }
            else 
            {   // Mountains
                Divide = (int)Math.Floor(MountainHeight / 4D);
                if (currentElevation < Mountains + (Divide * 1))
                    result = TerrainColour.Simple.lowermountains;
                else if (currentElevation < Mountains + (Divide * 2))
                    result = TerrainColour.Simple.middlemountains;
                else if (currentElevation < Mountains + (Divide * 3))
                    result = TerrainColour.Simple.uppermountains;
                else
                    result = TerrainColour.Simple.peaks;
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
            if (currentElevation < SeaLevel)
            {
                double elev = Value.Normalise(currentElevation, 0, SeaLevel, 0.25, 0.75);
                currentColor = RGBHSL.SetBrightness(currentColor, elev);
            }
            else if (currentElevation > Sand && currentElevation < Mountains)
            {
                double elev = Value.Normalise(currentElevation, Sand, TERRAIN_HEIGHT, 0.25, 0.75);
                currentColor = RGBHSL.SetBrightness(currentColor, 0.75F - elev);
            }

            return currentColor;

            #region TestCode
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
            #endregion
        }

        private Color SmoothGradient(byte currentElevation)
        {
            Color result;

            if (currentElevation < SeaLevel)
            {
                if (currentElevation < OceanDropOff)
                    result = Pixel.Gradient(currentElevation, 0, OceanDropOff, TerrainColour.Realistic.deepocean, TerrainColour.Realistic.midocean);
                else
                    result = Pixel.Gradient(currentElevation, OceanDropOff, SeaLevel, TerrainColour.Realistic.midocean, TerrainColour.Realistic.seashore);
            }
            else if (currentElevation < Mountains)
            {
                if (currentElevation < Shallows)
                    result = Pixel.Gradient(currentElevation, SeaLevel, Shallows, TerrainColour.Realistic.shallows, TerrainColour.Realistic.beach);
                else
                {
                    if (currentElevation < Sand)
                        result = Pixel.Gradient(currentElevation, Shallows, Sand, TerrainColour.Realistic.beach, TerrainColour.Realistic.beachgrass);
                    else if (currentElevation < Dirt)
                        result = Pixel.Gradient(currentElevation, Sand, Dirt, TerrainColour.Realistic.beachgrass, TerrainColour.Realistic.grass);
                    else
                        result = Pixel.Gradient(currentElevation, Dirt, Mountains, TerrainColour.Realistic.grass, TerrainColour.Realistic.forest);
                }
            }
            else
                result = Pixel.Gradient(currentElevation, Mountains, TERRAIN_HEIGHT, TerrainColour.Realistic.lowermountains, TerrainColour.Realistic.uppermountains);

            return result;
        }
        #endregion
    }

    struct TerrainColour
    {
        public struct Realistic
        {
            public static Color deepocean = Color.FromArgb(18, 25, 35);
            public static Color midocean = Color.FromArgb(49, 73, 107);

            public static Color seashore = Color.FromArgb(41, 110, 156);
            public static Color shallows = Color.FromArgb(41, 110, 156);

            public static Color beach = Color.FromArgb(209, 194, 166);
            public static Color beachgrass = Color.FromArgb(71, 91, 67);

            public static Color grass = Color.FromArgb(57, 81, 57);
            public static Color forest = Color.FromArgb(30, 44, 35);

            public static Color lowermountains = Color.FromArgb(39, 44, 36);
            public static Color uppermountains = Color.FromArgb(177, 181, 189);
        }

        public struct Simple
        {
            public static Color seafloor = Color.FromArgb(100, 149, 237);
            public static Color lowerocean = Color.FromArgb(112, 168, 241);
            public static Color middleocean = Color.FromArgb(123, 187, 246);
            public static Color upperocean = Color.FromArgb(135, 206, 250);

            public static Color shallows = Color.FromArgb(176, 224, 230);
            public static Color beach = Color.FromArgb(255, 242, 226);
            public static Color lowergrass = Color.FromArgb(60, 179, 113);
            public static Color middlegrass = Color.FromArgb(53, 159, 100);
            public static Color uppergrass = Color.FromArgb(46, 139, 87);

            public static Color lowermountains = Color.FromArgb(128, 128, 128);
            public static Color middlemountains = Color.FromArgb(156, 156, 156);
            public static Color uppermountains = Color.FromArgb(183, 183, 183);
            public static Color peaks = Color.FromArgb(211, 211, 211);
        }
    }
}
