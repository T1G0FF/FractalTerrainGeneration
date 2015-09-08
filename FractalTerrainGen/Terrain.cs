using System;

namespace FractalTerrainGen
{
    class Terrain
    {
        public string Name { get; private set; }
        public char Symbol { get; private set; }
        public byte Elevation { get; private set; }
        public ConsoleColor Fore { get; private set; }
        public ConsoleColor Back { get; private set; }

        public Terrain(string name, char symbol, byte elevation, ConsoleColor fore, ConsoleColor back)
        {
            Name = name;
            Symbol = symbol;
            Elevation = elevation;
            Fore = fore;
            Back = back;
        }

        public override string ToString()
        {
            Console.ForegroundColor = Fore;
            Console.BackgroundColor = Back;
            return Symbol.ToString();
        }
    }
}
