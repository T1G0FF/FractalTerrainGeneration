using System;
using System.Text;
using System.IO;

namespace FractalTerrainGen
{
    class BaseMap
    {
        public const float DEFAULTSCALE = 1.0F;
        public const int DEFAULTPASSES = 3;

        public byte[,] TerrainMap;

        public int Size { get; protected set; }
        public float Scale { get; protected set; }
        public int Seed { get; protected set; }
        public int Passes { get; protected set; }

        public void ToFile(string filePath)
        {
            using (StreamWriter sw = new StreamWriter(filePath + Seed + ".csv"))
            {
                StringBuilder sb = new StringBuilder();
                int xMax = TerrainMap.GetUpperBound(0) + 1;
                int yMax = TerrainMap.GetUpperBound(1) + 1;
                for (int yCoord = 0; yCoord < yMax; yCoord++)
                {
                    for (int xCoord = 0; xCoord < xMax; xCoord++)
                    {
                        sb.AppendFormat("{0,3},", TerrainMap[xCoord, yCoord]);
                    }
                    sb.AppendLine();
                }
                sb.AppendFormat("Size: {0} Seed: {1}", Size, Seed);
                sw.Write(sb.ToString());
            }
        }
    }
}
