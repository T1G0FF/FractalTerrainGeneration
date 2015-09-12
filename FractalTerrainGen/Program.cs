using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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
        static int currentImageSize = ImageMap.DEFAULT_SIZE;
        static float currentSealevel = ImageMap.DEFAULT_SEALEVEL;

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
                }
                else
                {
                    ImageMode();
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
                Console.WriteLine("Seed: {0} | Size: {1} | Scale: {2} | Passes: {3}", testASCIIMap.Seed, testASCIIMap.Size, testASCIIMap.Scale, testASCIIMap.Passes);
                currentASCIISize = testASCIIMap.Size;
                currentScale = testASCIIMap.Scale;
                currentPasses = testASCIIMap.Passes;
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
                    List<Task> imgTasksList = new List<Task>();

                    if (saveAll || saveNoise)
                    {
                        if (newSealevel == false)
                        {
                            imgTasksList.Add(
                                Task.Factory.StartNew(() =>
                                    testImageMap.SaveToImage(filePath, "Greyscale", ImageMap.WriteOption.SaveSeperate)
                                ));
                            newSealevel = false;
                        }
                    }

                    if (saveAll || saveRegular)
                    {
                        imgTasksList.Add(
                                Task.Factory.StartNew(() =>
                                    testImageMap.SaveToImage(filePath, "Colour", ImageMap.WriteOption.SaveSeperate)
                                ));
                    }

                    if (saveAll || saveTexture)
                    {
                        imgTasksList.Add(
                                Task.Factory.StartNew(() =>
                                    testImageMap.SaveToImage(filePath, "Texture", ImageMap.WriteOption.SaveSeperate)
                                ));
                    }

                    Task.WaitAll(imgTasksList.ToArray());
                }
                Console.WriteLine("Seed: {0} | Size: {1} | Scale: {2} | Sealevel: {3} | Passes: {4}", testImageMap.Seed, testImageMap.Size, testImageMap.Scale, testImageMap.SealevelScale, testImageMap.Passes);
                currentImageSize = testImageMap.Size;
                currentScale = testImageMap.Scale;
                currentSealevel = testImageMap.SealevelScale;
                currentPasses = testImageMap.Passes;
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
            regenRequired = true;

            Console.WriteLine("===== MENU =====");
            Console.WriteLine("Space : Generate New Seed");

            Console.WriteLine("S : Set (S)ize to a specific value");
            Console.WriteLine("E : Set S(E)ed to a specific value");
            Console.WriteLine("C : Set S(C)ale to a specific value");
            Console.WriteLine("P : Set the number of noise generation (P)asses");

            Console.WriteLine("F : Save Noise values to CSV (F)ile");
            Console.WriteLine("D : Reset to (D)efaults");
            Console.WriteLine("Q | Esc : (Q)uit");
            Console.WriteLine("----------------");
            Console.WriteLine("< > : Halve/Double map size");
            Console.WriteLine("; ' : Change Scale by {0}", DELTA_SMALL);


            string key = Console.ReadKey(true).Key.ToString().ToUpper();
            Console.Clear();
            switch (key)
            {
                case "SPACEBAR":
                case "ENTER":
                    currentSeed = Guid.NewGuid().GetHashCode();
                    break;
                //----------------------------
                case "S":
                    currentASCIISize = getSize();
                    break;

                case "E":
                    currentSeed = getSeed();
                    break;

                case "C":
                    currentScale = getScale();
                    break;

                case "P":
                    currentPasses = getPasses();
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
            Console.WriteLine("Space : Generate New Seed");

            Console.WriteLine("S : Set (S)ize to a specific value");
            Console.WriteLine("E : Set S(E)ed to a specific value");
            Console.WriteLine("C : Set S(C)ale to a specific value");
            Console.WriteLine("L : Set Sea(L)evel to a specific value");
            Console.WriteLine("P : Set the number of noise generation (P)asses");

            Console.WriteLine("N : Save (N)oise Image");
            Console.WriteLine("R : Save (R)egular Image");
            Console.WriteLine("T : Save (T)exture Image");

            Console.WriteLine("F : Save Noise values to CSV (F)ile");
            Console.WriteLine("D : Reset to (D)efaults");
            Console.WriteLine("Q | Esc : (Q)uit");
            Console.WriteLine("----------------");
            Console.WriteLine("< > : Halve/Double map size");
            Console.WriteLine("- + : Change Sea level by {0}", DELTA_SMALL);
            Console.WriteLine("[ ] : Change Sea level by {0}", DELTA_VERYSMALL);

            string key = Console.ReadKey(true).Key.ToString().ToUpper();
            Console.Clear();
            switch (key)
            {
                case "SPACEBAR":
                case "ENTER":
                    currentSeed = Guid.NewGuid().GetHashCode();
                    break;
                //----------------------------
                case "S":
                    currentImageSize = getSize();
                    break;

                case "E":
                    currentSeed = getSeed();
                    break;

                case "C":
                    currentScale = getScale();
                    break;

                case "L":
                    currentSealevel = getSealevel();
                    newSealevel = true;
                    break;

                case "P":
                    currentPasses = getPasses();
                    break;
                //----------------------------
                case "N":
                    OverwriteImage("Greyscale");
                    break;

                case "R":
                    OverwriteImage("Color");
                    break;

                case "T":
                    OverwriteImage("Texture");
                    break;

                case "M":
                    OverwriteImage("Smooth");
                    break;
                //----------------------------
                case "F":
                    if (testImageMap == null)
                        NoTerrainError();
                    else
                        testImageMap.ToFile(filePath);
                    regenRequired = false;
                    break;

                case "D":
                    currentImageSize = ImageMap.DEFAULT_SIZE;
                    currentSealevel = ImageMap.DEFAULT_SEALEVEL;
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

                    /*
                case "OEM7": // '
                    currentScale += DELTA_SMALL;
                    break;

                case "OEM1": // ;
                    currentScale -= DELTA_SMALL;
                    break;
                    */

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

        static int getSize()
        {
            return getInt("Input a new Size for the Map: ");
        }

        static int getPasses()
        {
            return getInt("Input a new number of noise passes: ");
        }

        static float getScale()
        {
            return getFloat("Input a new Scale for the Map: ");
        }

        static float getSealevel()
        {
            return getFloat("Input a new Sea level for the Map (0 - 1): ");
        }

        static int getInt(string msg)
        {
            int value;
            Console.Write(msg);
            string intInput = Console.ReadLine();
            while (Int32.TryParse(intInput, out value) == false)
            {
                Console.Write(msg);
                intInput = Console.ReadLine();
            }
            return value;
        }

        static int getSeed()
        {
            int seed;
            Console.Write("Input a Seed to use for the Map Generator: ");
            string seedInput = Console.ReadLine();
            if (Int32.TryParse(seedInput, out seed))
                return seed;
            else
                return seedInput.GetHashCode();
        }

        static float getFloat(string msg)
        {
            float scale;
            Console.Write(msg);
            string scaleInput = Console.ReadLine();
            while (Single.TryParse(scaleInput, out scale) == false)
            {
                Console.Write(msg);
                scaleInput = Console.ReadLine();
            }
            return scale;
        }

        static void OverwriteImage(string function)
        {
            if (testImageMap == null)
                NoTerrainError();
            else
                testImageMap.SaveToImage(filePath, function, ImageMap.WriteOption.Overwrite);
            regenRequired = false;
        }

        static void NoTerrainError()
        {
            Console.WriteLine("Unable to comply, no terrain has been generated." + "\n"
                            + "Please generate some terrain and try again!"
                            );
            Console.ReadKey(true);
        }
    }
}
