using System;

namespace FractalTerrainGen
{
    /// <summary>
    /// Terrain class, used by the ASCIIMap class to describe individual tiles.
    /// </summary>
    class Terrain
    {
        /// <summary>
        /// The name of the terrain type.
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// The symbol used to draw the terrain.
        /// </summary>
        public char Symbol { get; private set; }
        /// <summary>
        /// The elevation the terrain type appears at.
        /// </summary>
        public byte Elevation { get; private set; }
        /// <summary>
        /// The foreground colour of the terrain symbol.
        /// </summary>
        public ConsoleColor Fore { get; private set; }
        /// <summary>
        /// The background colour of the terrain symbol.
        /// </summary>
        public ConsoleColor Back { get; private set; }

        /// <summary>
        /// Terrain type constructor.
        /// </summary>
        /// <param name="name">The name of the terrain type.</param>
        /// <param name="symbol">The symbol used to draw the terrain.</param>
        /// <param name="elevation">The elevation the terrain type appears at.</param>
        /// <param name="fore">The foreground colour of the terrain symbol.</param>
        /// <param name="back">The background colour of the terrain symbol.</param>
        public Terrain(string name, char symbol, byte elevation, ConsoleColor fore, ConsoleColor back)
        {
            Name = name;
            Symbol = symbol;
            Elevation = elevation;
            Fore = fore;
            Back = back;
        }

        /// <summary>
        /// Sets the console's foreground and background colour then returns the symbol.
        /// </summary>
        /// <returns>The symbol used to draw the terrain.</returns>
        public override string ToString()
        {
            Console.ForegroundColor = Fore;
            Console.BackgroundColor = Back;
            return Symbol.ToString();
        }
    }
}
