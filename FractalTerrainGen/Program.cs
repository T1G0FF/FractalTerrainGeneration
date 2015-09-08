using System;
using System.IO;

namespace FractalTerrainGen
{
    class Game
    {
        // Program properties
        static string userprofile   = Environment.GetEnvironmentVariable("USERPROFILE");
        static string filePath      = userprofile + @"\Desktop\Maps\";
        static bool consoleMode = false;
        static bool saveAll     = false;
        
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
        static int currentImageSize     = ImageMap.DEFAULTSIZE;
        static float currentSealevel    = ImageMap.DEFAULTSEALEVEL;

        // ASCII Map
        static ASCIIMap testASCIIMap;
        static int currentASCIISize     = ASCIIMap.DEFAULTSIZE;
        
        static void Main(string[] args)
        {
            getArguments(args);
            bool cont = getKeys();

            while (cont)
            {
                Console.Clear();

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
                        case "-S":
                        case "--SAVEALL":
                            saveAll = true;
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
            testASCIIMap = new ASCIIMap(currentSeed, currentASCIISize, currentScale, currentPasses);
            testASCIIMap.Display();
        }

        static void ImageMode()
        {
            testImageMap = new ImageMap(currentSeed, currentImageSize, currentScale, currentSealevel, currentPasses);

            if (saveAll)
            {
                if (newSealevel == false)
                {
                    testImageMap.SaveToImage(filePath, "Greyscale", ImageMap.WriteOption.SaveAll);
                    Console.Clear();
                    newSealevel = false;
                }

                testImageMap.SaveToImage(filePath, "Color", ImageMap.WriteOption.SaveAll);
            }
            else
            {
                testImageMap.SaveToImage(filePath, "Color");
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
                
            return cont;
        }   

        static bool getConsoleKeys()
        {
            Console.WriteLine("===== MENU =====");
            Console.WriteLine("Space : Generate (N)ew Seed");

            Console.WriteLine("S : Set (S)ize to a specific value");
            Console.WriteLine("E : Set S(e)ed to a specific value");
            Console.WriteLine("C : Set S(c)ale to a specific value");
            Console.WriteLine("P : Set the number of noise generation (P)asses");

            Console.WriteLine("R : (R)egenerate");

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
                case "R":
                default:
                    break;
                //----------------------------
                case "F":
                    testASCIIMap.ToFile(filePath);
                    break;

                case "D":
                    currentASCIISize    = ASCIIMap.DEFAULTSIZE;
                    currentPasses       = BaseMap.DEFAULTPASSES;
                    currentScale        = BaseMap.DEFAULTSCALE;
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
            }

            return true;
        }

        static bool getImageKeys()
        {
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
                    break;

                case "R":
                    testImageMap.SaveToImage(filePath, "Color", ImageMap.WriteOption.Overwrite);
                    break;

                case "T":
                    testImageMap.SaveToImage(filePath, "Texture", ImageMap.WriteOption.Overwrite);
                    break;

                case "M":
                    testImageMap.SaveToImage(filePath, "Smooth", ImageMap.WriteOption.Overwrite);
                    break;
                //----------------------------
                case "F":
                    testImageMap.ToFile(filePath);
                    break;

                case "D":
                    currentImageSize    = ImageMap.DEFAULTSIZE;
                    currentSealevel     = ImageMap.DEFAULTSEALEVEL;
                    currentPasses       = BaseMap.DEFAULTPASSES;
                    currentScale        = BaseMap.DEFAULTSCALE;
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
                
                default:
                    break;
            }

            return true;
        }
    }
}
