using System.Collections;
using System.Collections.Generic;

namespace GadgetCore.API
{
    /// <summary>
    /// This is a wrapper for vanilla Item IDs to a Gadget Core ItemInfo. Use to get in-depth info about a vanilla item, in Gadget Core's terms. Can also be used to register events to occur upon firing vanilla weapons.
    /// </summary>
    public class VanillaItemInfo : ItemInfo
    {
        internal static Dictionary<int, VanillaItemInfo> Wrappers = new Dictionary<int, VanillaItemInfo>();

        /// <summary>
        /// Indicates whether the player is currently using a vanilla-wrapped item.
        /// </summary>
        public static bool Using { get; internal set; }
        /// <summary>
        /// Indicates whether the player is currently attacking with a vanilla-wrapped weapon.
        /// </summary>
        public static bool Attacking { get; internal set; }

        /// <summary>
        /// Constructs a new VanillaItemInfo based upon the given ID. Do not try to call this yourself - use <see cref="Wrap"/>
        /// </summary>
        /// <param name="ID">The vanilla ID to be wrapped.</param>
        /// <param name="WrapForTile">If true, the Tile property should not be set by this constructor, as it will be set later as part of a TileInfo's constructor.</param>
        protected VanillaItemInfo(int ID, bool WrapForTile) : base(ItemRegistry.GetDefaultTypeByID(ID), GadgetCoreAPI.GetItemName(ID), GadgetCoreAPI.GetItemDesc(ID), GadgetCoreAPI.GetItemMaterial(ID), -1, GadgetCoreAPI.GetTrueGearBaseStats(ID), GadgetCoreAPI.GetWeaponMaterial(ID), ID >= 1000 && ID < 2000 ? GadgetCoreAPI.GetDroidHeadMaterial(ID) : GadgetCoreAPI.GetHeadMaterial(ID), ID >= 1000 && ID < 2000 ? GadgetCoreAPI.GetDroidBodyMaterial(ID) : GadgetCoreAPI.GetBodyMaterial(ID), GadgetCoreAPI.GetArmMaterial(ID))
        {
            this.ID = ID;
            if ((Type & ItemType.EQUIP_MASK) == (ItemType.WEAPON & ItemType.EQUIP_MASK))
            {
                SetWeaponInfo(ItemRegistry.GetDefaultWeaponScalingByID(ID), GadgetCoreAPI.GetAttackSound(ID), ItemRegistry.GetDefaultCritChanceBonus(ID), ItemRegistry.GetDefaultCritPowerBonus(ID), ID);
                OnAttack += VanillaAttackDelegate;
            }
            else if ((Type & ItemType.USABLE) == ItemType.USABLE)
            {
                OnUse += (slot) => { Using = true; InstanceTracker.GameScript.StartCoroutine(InstanceTracker.GameScript.UseItemFinal(slot)); return false; };
            }
            if (!WrapForTile)
            {
                if (TileRegistry.Singleton.HasEntry(ID)) SetTile(TileRegistry.Singleton.GetEntry(ID));
                else SetTile(VanillaTileInfo.Wrap(ID, false));
            }
        }

        /// <summary>
        /// Provides a wrapper for the given vanilla ID. If the given ID has already been wrapped before, it will return the same wrapper instance as was returned before. If register is true, then the wrapper will be registered to its ID in the appropriate registry.
        /// </summary>
        public static VanillaItemInfo Wrap(int ID, bool register = false)
        {
            VanillaItemInfo itemInfo = Wrappers.ContainsKey(ID) ? Wrappers[ID] : (Wrappers[ID] = new VanillaItemInfo(ID, false));
            if (register && itemInfo.RegistryName == null) itemInfo.Register(itemInfo.Name, ID, true);
            return itemInfo;
        }

        internal static VanillaItemInfo WrapForTile(int ID, bool register = false)
        {
            VanillaItemInfo itemInfo = Wrappers.ContainsKey(ID) ? Wrappers[ID] : (Wrappers[ID] = new VanillaItemInfo(ID, true));
            if (register && itemInfo.RegistryName == null) itemInfo.Register(itemInfo.Name, ID, true);
            return itemInfo;
        }

        /// <summary>
        /// Delegate for invoking the vanilla Attack method. It does not make much sense to call this yourself, but you can reference it to unsubscribe the delegate
        /// from the OnAttack event if you wish.
        /// </summary>
        public static IEnumerator VanillaAttackDelegate(PlayerScript script)
        {
            Attacking = true;
            yield return script.Attack();
            Attacking = false;
        }
    }
}
