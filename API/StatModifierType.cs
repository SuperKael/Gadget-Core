namespace GadgetCore.API
{
    /// <summary>
    /// Indicates the type of stat modifier the modifier being applied is. Note that they are applied in the order Flat -> AddMult -> ExpMult. Also note that the 'AddMult' types are additive multipliers, meaning that, for example, returning EquipStats.ONE will double the base stats.
    /// </summary>
    public enum StatModifierType
    {
        /// <summary>
        /// Adds a flat modifier to the item's true base stats.
        /// </summary>
        BaseFlat,
        /// <summary>
        /// Adds a multiplicative modifier based off of the item's true base stats to the item's modified base stats.
        /// </summary>
        BaseAddMult,
        /// <summary>
        /// Applies a multiplicative modifier based off of the item's modified base stats.
        /// </summary>
        BaseExpMult,
        /// <summary>
        /// Adds an otherwise flat modifier that scales with the item's level.
        /// </summary>
        LevelFlat,
        /// <summary>
        /// Adds a multiplicative modifier based off of the item's base stats that scales with each level.
        /// </summary>
        LevelAddMult,
        /// <summary>
        /// Applies a multiplicative modifier based off the item's modified stats that scales with each level.
        /// WARNING: Think about what you are doing before using this one. Thanks to the level scaling, things can get out of hand very quickly.
        /// </summary>
        LevelExpMult,
        /// <summary>
        /// Adds a simple flat modifier to the item's stats. Behaves the same way as the bonuses from item quality, or stat-boosting gear mods do.
        /// </summary>
        Flat,
        /// <summary>
        /// Adds a simple multiplicative modifier based of the item's base stats.
        /// </summary>
        AddMult,
        /// <summary>
        /// Applies a simple multiplicative modifier based of the item's modified stats.
        /// </summary>
        ExpMult,
        /// <summary>
        /// Adds a flat modifier that is applied after all non-Final modifiers have been applied.
        /// </summary>
        FinalFlat,
        /// <summary>
        /// Adds a multiplicative modifier based off of the item's final stats pre-modification that is applied after all non-Final modifiers have been applied.
        /// </summary>
        FinalAddMult,
        /// <summary>
        /// Applies a multiplicative modifier based off of the item's final modified stats that is applied after all other modifiers have been applied.
        /// </summary>
        FinalExpMult
    }
}
