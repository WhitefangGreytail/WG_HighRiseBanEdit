using ColossalFramework.Math;
using System.Collections.Generic;


namespace WG_HighRiseBanEdit
{
    public class DataStore
    {
        private static DataStore instance = null;
        public static DataStore getInstance()
        {
            if (instance == null)
            {
                instance = new DataStore();
            }
            return instance;
        }

        public static void releaseInstance()
        {
            instance = null;
        }

        public float resHeightLimit = 40.0f;
        public float comHeightLimit = 40.0f;
        public float indHeightLimit = 40.0f;
        public float offHeightLimit = 40.0f;
    }

    public static class RandomizerSeedToBuildingId
    {
        public static Dictionary<ulong, ushort> seedToId = new Dictionary<ulong, ushort>(131071);

        static RandomizerSeedToBuildingId()
        { // Need to instantiate?
            // Currently 65536
            for (ushort i = 0; i < ((64*1024) - 1); ++i)  // Up to 256k buildings apparently is ok
            {
                // This creates a unique number
                Randomizer r = new Randomizer((int)i);
                try
                {
                    Debugging.writeDebugToFile(r.seed + " -> " + i);
                    seedToId.Add(r.seed, i);
                }
                catch (System.ArgumentException)
                {
                    Debugging.writeDebugToFile("Seed collision at number: "+ i);
                }
            }
        } // 
    }
}
