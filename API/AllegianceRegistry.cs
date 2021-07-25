namespace GadgetCore.API
{
    /// <summary>
    /// This registry is filled with AllegianceInfos, and is used for registering custom allegiances to the game.
    /// </summary>
    public class AllegianceRegistry : UnaryTypedRegistry<AllegianceRegistry, AllegianceInfo>
    {
        /// <summary>
        /// The name of this registry.
        /// </summary>
        public const string REGISTRY_NAME = "Allegiance";

        private static int[] allegianceArray;
        private static int selectedAllegiance;

        /// <summary>
        /// Gets the name of this registry. Must be constant. Returns <see cref="REGISTRY_NAME"/>.
        /// </summary>
        public override string GetRegistryName()
        {
            return REGISTRY_NAME;
        }

        /// <summary>
        /// Gets the ID that modded IDs should start at for this registry. <see cref="AllegianceRegistry"/> always returns 6.
        /// </summary>
        public override int GetIDStart()
        {
            return 6;
        }

        /// <summary>
        /// Returns the total number of allegiances, which is the registered amount plus the 4 vanilla allegiances.
        /// </summary>
        /// <returns></returns>
        public static int GetAllegianceCount()
        {
            return 4 + Singleton.GetEntryCount();
        }

        /// <summary>
        /// Rebuilds the allegiance array, used for the allegiance selection on the character creation screen.
        /// </summary>
        public static void RebuildAllegianceArray()
        {
            allegianceArray = new int[GetAllegianceCount()];
            allegianceArray[0] = 0;
            allegianceArray[1] = 1;
            allegianceArray[2] = 2;
            allegianceArray[3] = 3;
            AllegianceInfo[] allegiances = Singleton.GetAllEntries();
            for (int i = 0; i < allegiances.Length; i++) allegianceArray[i + 4] = allegiances[i].GetID();
            selectedAllegiance = 0;
        }

        /// <summary>
        /// Cycles the value of <see cref="Menuu.curAllegiance"/>, for use during character creation.
        /// </summary>
        public static void CycleAllegianceSelection(bool forward = true)
        {
            if (forward)
            {
                selectedAllegiance++;
                if (selectedAllegiance >= allegianceArray.Length)
                {
                    selectedAllegiance = 0;
                }
            }
            else
            {
                selectedAllegiance--;
                if (selectedAllegiance < 0)
                {
                    selectedAllegiance = allegianceArray.Length - 1;
                }
            }
            Menuu.curAllegiance = allegianceArray[selectedAllegiance];
        }
    }
}
