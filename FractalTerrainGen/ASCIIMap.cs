using System;
using Noise;
using Functions;


namespace FractalTerrainGen
{
    class ASCIIMap : BaseMap
    {
        #region Public
        public const int MIN_SIZE = 4;
        public const int MAX_SIZE = 128;
        public const int DEFAULT_SIZE = 16;
        public const int TERRAIN_HEIGHT = 10;
        private int MAX_POWER = (int)Math.Log(MAX_SIZE, 2); 
        
        //  ░  ▒  ▓

		Terrain SeaFloor    = new Terrain("Sea Floor",  '▒', 0, ConsoleColor.DarkBlue,     ConsoleColor.Black);

		Terrain DeepOcean   = new Terrain("Deep Ocean", ' ', 1, ConsoleColor.Blue,         ConsoleColor.DarkBlue);

		Terrain Ocean       = new Terrain("Ocean",      ' ', 2, ConsoleColor.Cyan,         ConsoleColor.Blue);

		Terrain Shallows    = new Terrain("Shallows",   ' ', 3, ConsoleColor.Cyan,         ConsoleColor.Cyan);

		Terrain Beach       = new Terrain("Beach",      '▒', 4, ConsoleColor.DarkYellow,   ConsoleColor.Yellow);

		Terrain Grassland   = new Terrain("Grassland",  '░', 5, ConsoleColor.DarkGreen,    ConsoleColor.Green);

		Terrain Forest      = new Terrain("Forest",     '*', 6, ConsoleColor.DarkGreen,    ConsoleColor.Green);

		Terrain Hills       = new Terrain("Hills",      '▒', 7, ConsoleColor.Gray,         ConsoleColor.DarkGray);

		Terrain Mountains   = new Terrain("Mountains",  '^', 8, ConsoleColor.Gray,         ConsoleColor.DarkGray);

		Terrain Peaks       = new Terrain("Peaks",      '^', 9, ConsoleColor.White,        ConsoleColor.DarkGray);

		Terrain Error		= new Terrain("Error",      '!', byte.MaxValue, ConsoleColor.DarkMagenta, ConsoleColor.Magenta);
                
        //Random randGen;

        public int Side { get { return Size - 1; } }
        #endregion

        #region Constructors
        public ASCIIMap()
            : this(Guid.NewGuid().GetHashCode())
        { }

        public ASCIIMap(int seed, int size = DEFAULT_SIZE, float scale = DEFAULT_SCALE, int passes = DEFAULT_PASSES)
        {
            Seed = seed;
            //randGen = new Random(seed);

            size = (int)Value.Clamp(size, 4, 128);
            for (int Power = MAX_POWER; Power >= 2; Power--)
            {
                int powerOfTwo = (int)Math.Pow(2, Power);

                if (size > (powerOfTwo * 0.9F))
                {
                    Size = powerOfTwo;
                    break;
                }
            }

            double MaxPasses = Generate.getMaxPasses(scale);
            Passes = (int)Value.Clamp(passes, 1, MaxPasses);

            double MaxScale = Generate.getMaxScale(Passes);
            Scale = (float)Value.Clamp(scale, MaxScale);

            TerrainMap = Generate.MultiPassNoise(Passes, Size, Scale, Seed);
        }
        #endregion

        #region Public Methods
        public void Display()
        {
            for (int xCoord = 0; xCoord < TerrainMap.GetUpperBound(0); xCoord++)
            {
                for (int yCoord = 0; yCoord < TerrainMap.GetUpperBound(1); yCoord++)
                {
                    Terrain current = getTerrain(TerrainMap[xCoord, yCoord]);
                    Console.ForegroundColor = current.Fore;
                    Console.BackgroundColor = current.Back;
                    Console.Write(current);
                    Console.ResetColor();
                }
                Console.WriteLine();
            }
        }

        public Terrain getTerrain(int elevation)
        {
            int elev = (int)Value.Normalise(elevation, 0, byte.MaxValue, 0, TERRAIN_HEIGHT);

            if (elev <= SeaFloor.Elevation)
                return SeaFloor;
            else if (elev == DeepOcean.Elevation)
                return DeepOcean;
            else if (elev == Ocean.Elevation)
                return Ocean;
            else if (elev == Shallows.Elevation)
                return Shallows;
            else if (elev == Beach.Elevation)
                return Beach;
            else if (elev == Grassland.Elevation)
                return Grassland;
            else if (elev == Forest.Elevation)
                return Forest;
            else if (elev == Hills.Elevation)
                return Hills;
            else if (elev == Mountains.Elevation)
                return Mountains;
            else if (elev >= Peaks.Elevation)
                return Peaks;
            else
                return Error;
        }

        /*
        #region Rivers
        public void PlaceRiver()
        {
            for (int xCoord = 0; xCoord < TerrainMap.GetUpperBound(0); xCoord++)
            {
                if (randGen.Next(10) != 0) continue;
                for (int yCoord = 0; yCoord < TerrainMap.GetUpperBound(1); yCoord++)
                {
                    if (randGen.Next(10) != 0) continue;
                    if (TerrainMap[xCoord, yCoord] > Hills.Elevation)// && randGen.Next(10) == 0 )
                    {
                        RiverWorker(xCoord, yCoord, TerrainMap[xCoord, yCoord]);
                    }
                }
            }
        }

        private void RiverWorker(int x, int y, int Previous)
        {
            int Current = TerrainMap[x, y];
            int North = int.MaxValue;
            int East = int.MaxValue;
            int South = int.MaxValue;
            int West = int.MaxValue;

            if (y != 0)
            {
                North = TerrainMap[x, y - 1];
            }
            if (x != Side)
            {
                East = TerrainMap[x + 1, y];
            }
            if (y != Side)
            {
                South = TerrainMap[x, y + 1];
            }
            if (x != 0)
            {
                West = TerrainMap[x - 1, y];
            }


            if (North < Previous && North < East && North < South && North < West)
            {
                TerrainMap[x, y] = Shallows.Elevation;
                RiverWorker(x, y - 1, Current);
            }
            else if (East < Previous && East < North && East < South && East < West)
            {
                TerrainMap[x, y] = Shallows.Elevation;
                RiverWorker(x + 1, y, Current);
            }
            else if (South < Previous && South < North && South < East && South < West)
            {
                TerrainMap[x, y] = Shallows.Elevation;
                RiverWorker(x, y + 1, Current);
            }
            else if (West < Previous && West < North && West < East && West < South)
            {
                TerrainMap[x, y] = Shallows.Elevation;
                RiverWorker(x - 1, y, Current);
            }
            else
            {
                return;
            }
        }
        #endregion

        #region Erode
        public void Erode()
        {
            for (int xCoord = 0; xCoord < TerrainMap.GetUpperBound(0); xCoord++)
            {
                for (int yCoord = 0; yCoord < TerrainMap.GetUpperBound(1); yCoord++)
                {
                    if (randGen.Next(10) == 0)
                    {
                        ErodeWorker(xCoord, yCoord, 0);
                    }
                }
            }
        }

        private void ErodeWorker(int x, int y, int bias)
        {
            byte Current = TerrainMap[x, y];
            if (Current == 0) { return; }

            int North = int.MaxValue;
            int East = int.MaxValue;
            int South = int.MaxValue;
            int West = int.MaxValue;

            if (y != 0)
            {
                North = TerrainMap[x, y - 1];
            }
            if (x != Side)
            {
                East = TerrainMap[x + 1, y];
            }
            if (y != Side)
            {
                South = TerrainMap[x, y + 1];
            }
            if (x != 0)
            {
                West = TerrainMap[x - 1, y];
            }


            if (North < (Current - bias) && North < East && North < South && North < West)
            {
                TerrainMap[x, y] = (byte)(Current - 1);
                ErodeWorker(x, y - 1, bias + 1);
            }
            else if (East < (Current - bias) && East < North && East < South && East < West)
            {
                TerrainMap[x, y] = (byte)(Current - 1);
                ErodeWorker(x + 1, y, bias + 1);
            }
            else if (South < (Current - bias) && South < North && South < East && South < West)
            {
                TerrainMap[x, y] = (byte)(Current - 1);
                ErodeWorker(x, y + 1, bias + 1);
            }
            else if (West < (Current - bias) && West < North && West < East && West < South)
            {
                TerrainMap[x, y] = (byte)(Current - 1);
                ErodeWorker(x - 1, y, bias + 1);
            }
            else
            {
                return;
            }
        }
        #endregion

        #region Settle
        public void Settle()
        {
            for (int xCoord = 0; xCoord < TerrainMap.GetUpperBound(0); xCoord++)
            {
                for (int yCoord = 0; yCoord < TerrainMap.GetUpperBound(1); yCoord++)
                {
                    SettleWorker(xCoord, yCoord, 1);
                }
            }
        }

        private void SettleWorker(int x, int y, int bias)
        {
            byte N = 0;
            byte NE = 0;
            byte E = 0;
            byte SE = 0;
            byte S = 0;
            byte SW = 0;
            byte W = 0;
            byte NW = 0;
            byte Current = TerrainMap[x, y];
            bool Smoothed = false;

            #region Cardinal Directions
            if (y != 0)
                N = TerrainMap[x, y - 1];
            if (x != Side)
                E = TerrainMap[x + 1, y];
            if (y != Side)
                S = TerrainMap[x, y + 1];
            if (x != 0)
                W = TerrainMap[x - 1, y];

            if (N > (Current + bias))
            {
                Smoothed = true;
                TerrainMap[x, y - 1] = (byte)(N - 1);
            }
            if (E > (Current + bias))
            {
                Smoothed = true;
                TerrainMap[x + 1, y] = (byte)(E - 1);
            }
            if (S > (Current + bias))
            {
                Smoothed = true;
                TerrainMap[x, y + 1] = (byte)(S - 1);
            }
            if (W > (Current + bias))
            {
                Smoothed = true;
                TerrainMap[x - 1, y] = (byte)(W - 1);
            }
            #endregion

            #region Diagonals
            //if ( y != 0 && x != Side )
			//	NE = TerrainMap[x + 1, y - 1].Elevation;
			//if ( y != Side && x != Side )
			//	SE = TerrainMap[x + 1, y + 1].Elevation;
			//if ( y != Side && x != 0 )
			//	SW = TerrainMap[x - 1, y + 1].Elevation;
			//if ( y != 0 && x != 0 )
			//	NW = TerrainMap[x - 1, y - 1].Elevation;
            //
			//if ( NE > ( Current + bias ) )
			//	TerrainMap[x + 1, y - 1] = getTerrain(NE - 1);
			//if ( SE > ( Current + bias ) )
			//	TerrainMap[x + 1, y + 1] = getTerrain(SE - 1);
			//if ( SW > ( Current + bias ) )
			//	TerrainMap[x - 1, y + 1] = getTerrain(SW - 1);
			//if ( NW > ( Current + bias ) )
			//	TerrainMap[x - 1, y - 1] = getTerrain(NW - 1);
            #endregion

            if (Smoothed)
                TerrainMap[x, y] = (byte)(Current + 1);
        }
        #endregion

        #region Smooth
        public void SmoothWater()
        {
            for (int xCoord = 0; xCoord < TerrainMap.GetUpperBound(0); xCoord++)
            {
                for (int yCoord = 0; yCoord < TerrainMap.GetUpperBound(1); yCoord++)
                {
                    SmoothWorker(xCoord, yCoord, 1);
                }
            }
        }

        private void SmoothWorker(int x, int y, int bias)
        {
            int Current = TerrainMap[x, y];
            if (Current > Grassland.Elevation) { return; }

            int N = int.MaxValue;
            int E = int.MaxValue;
            int S = int.MaxValue;
            int W = int.MaxValue;

            if (y != 0)
                N = TerrainMap[x, y - 1];
            if (x != Side)
                E = TerrainMap[x + 1, y];
            if (y != Side)
                S = TerrainMap[x, y + 1];
            if (x != 0)
                W = TerrainMap[x - 1, y];

            if (Current == Beach.Elevation)
            {
                if (N < Beach.Elevation)
                    TerrainMap[x, y - 1] = Shallows.Elevation;
                if (E < Beach.Elevation)
                    TerrainMap[x + 1, y] = Shallows.Elevation;
                if (S < Beach.Elevation)
                    TerrainMap[x, y + 1] = Shallows.Elevation;
                if (W < Beach.Elevation)
                    TerrainMap[x - 1, y] = Shallows.Elevation;
            }
            else if (Current == Shallows.Elevation)
            {
                if (N < Shallows.Elevation)
                    TerrainMap[x, y - 1] = Ocean.Elevation;
                if (E < Shallows.Elevation)
                    TerrainMap[x + 1, y] = Ocean.Elevation;
                if (S < Shallows.Elevation)
                    TerrainMap[x, y + 1] = Ocean.Elevation;
                if (W < Shallows.Elevation)
                    TerrainMap[x - 1, y] = Ocean.Elevation;
            }
            else if (Current == Ocean.Elevation)
            {
                if (N < Ocean.Elevation)
                    TerrainMap[x, y - 1] = DeepOcean.Elevation;
                if (E < Ocean.Elevation)
                    TerrainMap[x + 1, y] = DeepOcean.Elevation;
                if (S < Ocean.Elevation)
                    TerrainMap[x, y + 1] = DeepOcean.Elevation;
                if (W < Ocean.Elevation)
                    TerrainMap[x - 1, y] = DeepOcean.Elevation;
            }

            //if ( N < ( Current - bias ) )
			//	TerrainMap[x, y - 1] = getTerrain(N + 1);
			//if ( S < ( Current - bias ) )
			//	TerrainMap[x + 1, y] = getTerrain(S + 1);
			//if ( E < ( Current - bias ) )
			//	TerrainMap[x, y + 1] = getTerrain(E + 1);
			//if ( W < ( Current - bias ) )
			//	TerrainMap[x - 1, y] = getTerrain(W + 1);
        }
        #endregion
        */
        #endregion

        #region Private Methods

        #endregion
    }
}
