using System;
using System.IO;

namespace FractalTerrainGen
{
    class Game
    {
        // Program properties
        static string userprofile = Environment.GetEnvironmentVariable("USERPROFILE");
        static string filePath = userprofile + @"\Desktop\Maps\";
        static bool regenRequired = true;
        static bool consoleMode = false;
        static bool saveAll = false;
        static bool saveNoise = false;
        static bool saveRegular = false;
        static bool saveTexture = false;
        static bool singleFileMode = true;
        
        // Shared
        static int currentSeed = Guid.NewGuid().GetHashCode();
        static int currentPasses = BaseMap.DEFAULTPASSES;
        static float currentScale = BaseMap.DEFAULTSCALE;

        const float DELTA_VERYSMALL = 0.01F;
        const float DELTA_SMALL = 0.1F;
        const float DELTA_NORMAL = 1.0F;
        const float DELTA_BIG = 5.0F;
        const float DELTA_VERYBIG = 10.0F;

        // Image Map
        static ImageMap testImageMap;
        static bool newSealevel = false;
        static int currentImageSize = ImageMap.DEFAULTSIZE;
        static float currentSealevel = ImageMap.DEFAULTSEALEVEL;

        // ASCII Map
        static ASCIIMap testASCIIMap;
        static int currentASCIISize = ASCIIMap.DEFAULTSIZE;

        static void Main(string[] args)
        {
            getArguments(args);
            bool cont = getKeys();

            while (cont)
            {
                if (consoleMode)
                {
                    ConsoleMode();
                    Console.WriteLine("Seed: {0} | Size: {1} | Scale: {2} | Passes: {3}", currentSeed, currentASCIISize, currentScale, currentPasses);
                }
                else
                {
                    ImageMode();
                    Console.WriteLine("Seed: {0} | Size: {1} | Scale: {2} | Sealevel: {3} | Passes: {4}", currentSeed, currentImageSize, currentScale, currentSealevel, currentPasses);
                }

                cont = getKeys();
            }
        }


        static void getArguments(string[] args)
        {
            if (args.Length > 0)
            {
                foreach (string argIn in args)
                {
                    string arg = argIn.ToUpper();

                    switch (arg)
                    {
                        case "-SA":
                        case "--SAVEALL":
                            saveAll = true;
                            singleFileMode = false;
                            break;
                        case "-SN":
                        case "--SAVENOISE":
                            saveNoise = true;
                            singleFileMode = false;
                            break;
                        case "-SR":
                        case "--SAVEREGULAR":
                            saveRegular = true;
                            singleFileMode = false;
                            break;
                        case "-ST":
                        case "--SAVETEXTURE":
                            saveTexture = true;
                            singleFileMode = false;
                            break;
                        case "-C":
                        case "--CONSOLE":
                            consoleMode = true;
                            break;
                    }
                }
            }
        }

        static void ConsoleMode()
        {
            if (regenRequired)
            {
                testASCIIMap = new ASCIIMap(currentSeed, currentASCIISize, currentScale, currentPasses);
            }
            testASCIIMap.Display();
        }

        static void ImageMode()
        {
            if (regenRequired)
            {
                testImageMap = new ImageMap(currentSeed, currentImageSize, currentScale, currentSealevel, currentPasses);

                if (singleFileMode)
                {
                    testImageMap.SaveToImage(filePath, "Color");
                }
                else
                {
                    if (saveAll || saveNoise)
                    {
                        if (newSealevel == false)
                        {
                            testImageMap.SaveToImage(filePath, "Greyscale", ImageMap.WriteOption.SaveSeperate);
                            newSealevel = false;
                        }
                    }

                    if (saveAll || saveRegular)
                    {
                        testImageMap.SaveToImage(filePath, "Colour", ImageMap.WriteOption.SaveSeperate);
                    }

                    if (saveAll || saveTexture)
                    {
                        testImageMap.SaveToImage(filePath, "Texture", ImageMap.WriteOption.SaveSeperate);
                    }
                }
            }
        }

        static bool getKeys()
        {
            bool cont;

            if (consoleMode)
            {
                cont = getConsoleKeys();
            }
            else
            {
                cont = getImageKeys();
            }

            Console.Clear();

            return cont;
        }

        static bool getConsoleKeys()
        {
            regenRequired = true;

            Console.WriteLine("===== MENU =====");
            Console.WriteLine("Space : Generate (N)ew Seed");

            Console.WriteLine("S : Set (S)ize to a specific value");
            Console.WriteLine("E : Set S(e)ed to a specific value");
            Console.WriteLine("C : Set S(c)ale to a specific value");
            Console.WriteLine("P : Set the number of noise generation (P)asses");

            Console.WriteLine("F : Save Noise values to CSV (F)ile");
            Console.WriteLine("D : Reset to (D)efaults");
            Console.WriteLine("Q | Esc : Quit");
            Console.WriteLine("----------------");
            Console.WriteLine("< > : Halve/Double map size");
            Console.WriteLine("; ' : Change Scale by {0}", DELTA_SMALL);


            string key = Console.ReadKey(true).Key.ToString().ToUpper();
            switch (key)
            {
                case "SPACEBAR":
                case "ENTER":
                    currentSeed = Guid.NewGuid().GetHashCode();
                    break;
                //----------------------------
                case "S":
                    int size;
                    Console.Write("Input a new Size for the Map: ");
                    string sizeInput = Console.ReadLine();
                    while (Int32.TryParse(sizeInput, out size) == false)
                    {
                        Console.Write("Input a new Size for the Map: ");
                        sizeInput = Console.ReadLine();
                    }
                    currentASCIISize = size;
                    break;

                case "E":
                    int seed;
                    Console.Write("Input a seed to use for the Map Generator: ");
                    string seedInput = Console.ReadLine();
                    if (Int32.TryParse(seedInput, out seed))
                        currentSeed = seed;
                    else
                        currentSeed = seedInput.GetHashCode();
                    break;

                case "C":
                    float scale;
                    Console.Write("Input a new Scale for the Map: ");
                    string scaleInput = Console.ReadLine();
                    while (Single.TryParse(scaleInput, out scale) == false)
                    {
                        Console.Write("Input a new Scale: ");
                        scaleInput = Console.ReadLine();
                    }
                    currentScale = scale;
                    break;

                case "P":
                    int pass;
                    Console.Write("Input a new number of noise passes: ");
                    string passInput = Console.ReadLine();
                    while (Int32.TryParse(passInput, out pass) == false)
                    {
                        Console.Write("Input a new number of noise passes: ");
                        passInput = Console.ReadLine();
                    }
                    currentPasses = pass;
                    break;
                //----------------------------
                case "F":
                    testASCIIMap.ToFile(filePath);
                    regenRequired = false;
                    break;

                case "D":
                    currentASCIISize = ASCIIMap.DEFAULTSIZE;
                    currentPasses = BaseMap.DEFAULTPASSES;
                    currentScale = BaseMap.DEFAULTSCALE;
                    break;

                case "Q":
                case "ESCAPE":
                    return false;
                //----------------------------
                case "OEMPERIOD": // .
                    currentASCIISize *= 2;
                    break;

                case "OEMCOMMA": // ,
                    currentASCIISize /= 2;
                    break;

                case "OEM7": // '
                    currentScale += DELTA_SMALL;
                    break;

                case "OEM1": // ;
                    currentScale -= DELTA_SMALL;
                    break;
                //----------------------------
                default:
                    regenRequired = false;
                    break;
            }
            return true;
        }

        static bool getImageKeys()
        {
            regenRequired = true;

            Console.WriteLine("===== MENU =====");
            Console.WriteLine("Space : Generate (N)ew Seed");

            Console.WriteLine("S : Set (S)ize to a specific value");
            Console.WriteLine("E : Set S(e)ed to a specific value");
            Console.WriteLine("C : Set S(c)ale to a specific value");
            Console.WriteLine("P : Set the number of noise generation (P)asses");

            Console.WriteLine("N : Save (N)oise Image");
            Console.WriteLine("R : Save (R)egular Image");
            Console.WriteLine("T : Save (T)exture Image");

            Console.WriteLine("F : Save Noise values to CSV (F)ile");
            Console.WriteLine("D : Reset to (D)efaults");
            Console.WriteLine("Q | Esc : Quit");
            Console.WriteLine("----------------");
            Console.WriteLine("< > : Halve/Double map size");
            Console.WriteLine("- + : Change Sea level by {0}", DELTA_SMALL);
            Console.WriteLine("[ ] : Change Sea level by {0}", DELTA_VERYSMALL);
            Console.WriteLine("; ' : Change Scale by {0}", DELTA_SMALL);

            string key = Console.ReadKey(true).Key.ToString().ToUpper();
            switch (key)
            {
                case "SPACEBAR":
                case "ENTER":
                    currentSeed = Guid.NewGuid().GetHashCode();
                    break;
                //----------------------------
                case "S":
                    int size;
                    Console.Write("Input a new Size for the Map: ");
                    string sizeInput = Console.ReadLine();
                    while (Int32.TryParse(sizeInput, out size) == false)
                    {
                        Console.Write("Input a new Size for the Map: ");
                        sizeInput = Console.ReadLine();
                    }
                    currentImageSize = size;
                    break;

                case "E":
                    int seed;
                    Console.Write("Input a Seed to use for the Map Generator: ");
                    string seedInput = Console.ReadLine();
                    if (Int32.TryParse(seedInput, out seed))
                        currentSeed = seed;
                    else
                        currentSeed = seedInput.GetHashCode();
                    break;

                case "C":
                    float scale;
                    Console.Write("Input a new Scale for the Map: ");
                    string scaleInput = Console.ReadLine();
                    while (Single.TryParse(scaleInput, out scale) == false)
                    {
                        Console.Write("Input a new Scale: ");
                        scaleInput = Console.ReadLine();
                    }
                    currentScale = scale;
                    break;

                case "P":
                    int pass;
                    Console.Write("Input a new number of noise passes: ");
                    string passInput = Console.ReadLine();
                    while (Int32.TryParse(passInput, out pass) == false)
                    {
                        Console.Write("Input a new number of noise passes: ");
                        passInput = Console.ReadLine();
                    }
                    currentPasses = pass;
                    break;
                //----------------------------
                case "N":
                    testImageMap.SaveToImage(filePath, "Greyscale", ImageMap.WriteOption.Overwrite);
                    regenRequired = false;
                    break;

                case "R":
                    testImageMap.SaveToImage(filePath, "Color", ImageMap.WriteOption.Overwrite);
                    regenRequired = false;
                    break;

                case "T":
                    testImageMap.SaveToImage(filePath, "Texture", ImageMap.WriteOption.Overwrite);
                    regenRequired = false;
                    break;

                case "M":
                    testImageMap.SaveToImage(filePath, "Smooth", ImageMap.WriteOption.Overwrite);
                    regenRequired = false;
                    break;
                //----------------------------
                case "F":
                    testImageMap.ToFile(filePath);
                    regenRequired = false;
                    break;

                case "D":
                    currentImageSize = ImageMap.DEFAULTSIZE;
                    currentSealevel = ImageMap.DEFAULTSEALEVEL;
                    currentPasses = BaseMap.DEFAULTPASSES;
                    currentScale = BaseMap.DEFAULTSCALE;
                    break;

                case "Q":
                case "ESCAPE":
                    return false;
                //----------------------------
                case "OEMPLUS": // +
                    currentSealevel += DELTA_SMALL;
                    newSealevel = true;
                    break;

                case "OEMMINUS": // -
                    currentSealevel -= DELTA_SMALL;
                    newSealevel = true;
                    break;

                case "OEM6": // ]
                    currentSealevel += DELTA_VERYSMALL;
                    newSealevel = true;
                    break;

                case "OEM4": // [
                    currentSealevel -= DELTA_VERYSMALL;
                    newSealevel = true;
                    break;

                case "OEM7": // '
                    currentScale += DELTA_SMALL;
                    break;

                case "OEM1": // ;
                    currentScale -= DELTA_SMALL;
                    break;

                case "OEMPERIOD": // .
                    currentImageSize *= 2;
                    break;

                case "OEMCOMMA": // ,
                    currentImageSize /= 2;
                    break;
                //----------------------------
                default:
                    regenRequired = false;
                    break;
            }

            return true;
        }
    }
}
