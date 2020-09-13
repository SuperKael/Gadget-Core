using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace GadgetCore.API
{
    /// <summary>
    /// This is a wrapper for vanilla Item IDs to a Gadget Core ItemInfo. Use to get in-depth info about a vanilla item, in Gadget Core's terms. Can also be used to register events to occur upon firing vanilla weapons.
    /// </summary>
    public class VanillaItemInfo : ItemInfo
    {
        private static MethodInfo UseMethod = typeof(PlayerScript).GetMethod("UseItemFinal", BindingFlags.NonPublic | BindingFlags.Instance);
        private static MethodInfo AttackMethod = typeof(PlayerScript).GetMethod("Attack", BindingFlags.NonPublic | BindingFlags.Instance);

        internal static Dictionary<int, VanillaItemInfo> Wrappers = new Dictionary<int, VanillaItemInfo>();

        internal static bool Using;
        internal static bool Attacking;

        /// <summary>
        /// Constructs a new VanillaItemInfo based upon the given ID. Do not try to call this yourself - use <see cref="Wrap"/>
        /// </summary>
        /// <param name="ID">The vanilla ID to be wrapped.</param>
        /// <param name="WrapForTile">If true, the Tile property should not be set by this constructor, as it will be set later as part of a TileInfo's constructor.</param>
        protected VanillaItemInfo(int ID, bool WrapForTile) : base(ItemRegistry.GetDefaultTypeByID(ID), GadgetCoreAPI.GetItemName(ID), GadgetCoreAPI.GetItemDesc(ID), GadgetCoreAPI.GetItemMaterial(ID), -1, GadgetCoreAPI.GetTrueGearBaseStats(ID), GadgetCoreAPI.GetWeaponMaterial(ID), ID >= 1000 && ID < 2000 ? GadgetCoreAPI.GetDroidHeadMaterial(ID) : GadgetCoreAPI.GetHeadMaterial(ID), ID >= 1000 && ID < 2000 ? GadgetCoreAPI.GetDroidBodyMaterial(ID) : GadgetCoreAPI.GetBodyMaterial(ID), GadgetCoreAPI.GetArmMaterial(ID))
        {
            this.ID = ID;
            if ((Type & ItemType.BASIC_MASK) == (ItemType.WEAPON & ItemType.BASIC_MASK))
            {
                SetWeaponInfo(ItemRegistry.GetDefaultWeaponScalingByID(ID), GadgetCoreAPI.GetAttackSound(ID), ItemRegistry.GetDefaultCritChanceBonus(ID), ItemRegistry.GetDefaultCritPowerBonus(ID), ID);
                OnAttack += (script) => { Attacking = true; IEnumerator ie = AttackMethod.Invoke(script, new object[] { }) as IEnumerator; Attacking = false; return ie; };
            }
            else if ((Type & ItemType.USABLE) == ItemType.USABLE)
            {
                OnUse += (slot) => { Using = true; InstanceTracker.GameScript.StartCoroutine(UseMethod.Invoke(InstanceTracker.GameScript, new object[] { slot }) as IEnumerator); return false; };
            }
            if (!WrapForTile)
            {
                if (TileRegistry.GetSingleton().HasEntry(ID)) SetTile(TileRegistry.GetSingleton().GetEntry(ID));
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
    }
}
