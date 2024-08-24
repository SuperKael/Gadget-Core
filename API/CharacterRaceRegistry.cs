namespace GadgetCore.API
{
    /// <summary>
    /// This registry is filled with CharacterRaceInfos, and is used for registering custom character races to the game.
    /// </summary>
    public class CharacterRaceRegistry : CharacterFeatureRegistry<CharacterRaceRegistry, CharacterRaceInfo>
    {
        /// <summary>
        /// The name of this registry.
        /// </summary>
        public const string REGISTRY_NAME = "Race";
        /// <summary>
        /// The name of this registry.
        /// </summary>
        public const CharacterFeatureType REGISTRY_TYPE = CharacterFeatureType.RACE;

        /// <summary>
        /// Called when a feature within this character feature registry is selected during character creation. Generally, you should not call this yourself.
        /// </summary>
        public override void SelectFeature(int feature)
        {
            Menuu.curRace = feature;
            Menuu.curVariant = 0;
            base.SelectFeature(feature);
        }

        /// <summary>
        /// Gets the name of this registry. Must be constant. Returns <see cref="REGISTRY_NAME"/>.
        /// </summary>
        public override string GetRegistryName()
        {
            return REGISTRY_NAME;
        }

        /// <summary>
        /// Gets the type of this registry. Must be constant. Returns <see cref="REGISTRY_TYPE"/>.
        /// </summary>
        public override CharacterFeatureType GetFeatureType()
        {
            return REGISTRY_TYPE;
        }
    }
}
