using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace GadgetCore.API
{
    /// <summary>
    /// Defines a custom Item. Make sure to call Register on it to register your Item.
    /// </summary>
    public class ItemInfo : RegistryEntry<ItemInfo, ItemType>
    {
        private static readonly MethodInfo ATKSOUND = typeof(PlayerScript).GetMethod("ATKSOUND", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly MethodInfo MageMash = typeof(PlayerScript).GetMethod("MageMash", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly MethodInfo SwordEffects = typeof(PlayerScript).GetMethod("SwordEffects", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly MethodInfo LanceEffects = typeof(PlayerScript).GetMethod("LanceEffects", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly MethodInfo GunEffects = typeof(PlayerScript).GetMethod("GunEffects", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly MethodInfo HyperBeam = typeof(PlayerScript).GetMethod("HyperBeam", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly MethodInfo Cerberus = typeof(PlayerScript).GetMethod("Cerberus", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly MethodInfo TripleShot = typeof(PlayerScript).GetMethod("TripleShot", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly MethodInfo TurretS = typeof(PlayerScript).GetMethod("TurretS", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly MethodInfo TurretF = typeof(PlayerScript).GetMethod("TurretF", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly MethodInfo Turret = typeof(PlayerScript).GetMethod("Turret", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly MethodInfo SiriusS = typeof(PlayerScript).GetMethod("SiriusS", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly MethodInfo Plasma = typeof(PlayerScript).GetMethod("Plasma", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly MethodInfo Staff = typeof(PlayerScript).GetMethod("Staff", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly MethodInfo SchegI = typeof(PlayerScript).GetMethod("SchegI", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly MethodInfo AirSplit = typeof(PlayerScript).GetMethod("AirSplit", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly MethodInfo Shock = typeof(PlayerScript).GetMethod("Shock", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly MethodInfo CasterSword = typeof(PlayerScript).GetMethod("CasterSword", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo canAttack = typeof(PlayerScript).GetField("canAttack", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo attacking = typeof(PlayerScript).GetField("attacking", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo hyper = typeof(PlayerScript).GetField("hyper", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo triple = typeof(PlayerScript).GetField("triple", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo r = typeof(PlayerScript).GetField("r", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo curShot = typeof(PlayerScript).GetField("curShot", BindingFlags.NonPublic | BindingFlags.Instance);

        /// <summary>
        /// The ItemType of this Item
        /// </summary>
        public readonly ItemType Type;
        /// <summary>
        /// The name of this Item
        /// </summary>
        public readonly string Name;
        /// <summary>
        /// The description of this Item
        /// </summary>
        public readonly string Desc;
        /// <summary>
        /// This EquipStats of this Item
        /// </summary>
        public readonly EquipStats Stats;

        /// <summary>
        /// The sell-value of this Item. Represents the amount of credits given when taken to the item trasher.
        /// </summary>
        public virtual int Value { get; protected set; }
        /// <summary>
        /// The TileInfo that this item can place. Use this ItemInfo in the TileInfo's constructor to ensure it is registered correctly.
        /// </summary>
        public virtual TileInfo Tile { get; protected set; }
        /// <summary>
        /// The Texture associated with this Item. May be null.
        /// </summary>
        public virtual Texture Tex { get; protected set; }
        /// <summary>
        /// The Texture associated with this item when it is held in the main- or off-hand. May be null.
        /// </summary>
        public virtual Texture HeldTex { get; protected set; }
        /// <summary>
        /// The Texture associated with this item when it is worn as a helmet, or the Texture used by this droid's head. May be null.
        /// </summary>
        public virtual Texture HeadTex { get; protected set; }
        /// <summary>
        /// The Texture associated with this item when it is worn as an armor, or the Texture used by this droid's body. May be null.
        /// </summary>
        public virtual Texture BodyTex { get; protected set; }
        /// <summary>
        /// The Texture associated with this item when it is worn as an armor; this texture is used for the player's arms. May be null.
        /// </summary>
        public virtual Texture ArmTex { get; protected set; }
        /// <summary>
        /// The Material associated with this item. May be null.
        /// </summary>
        public virtual Material Mat { get; protected set; }
        /// <summary>
        /// The Material associated with this item when it is held in the main- or off-hand. May be null.
        /// </summary>
        public virtual Material HeldMat { get; protected set; }
        /// <summary>
        /// The Material associated with this item when it is worn as a helmet, or the Texture used by this droid's head. May be null.
        /// </summary>
        public virtual Material HeadMat { get; protected set; }
        /// <summary>
        /// The Material associated with this item when it is worn as an armor, or the Texture used by this droid's body. May be null.
        /// </summary>
        public virtual Material BodyMat { get; protected set; }
        /// <summary>
        /// The Material associated with this item when it is worn as an armor; this texture is used for the player's arms. May be null.
        /// </summary>
        public virtual Material ArmMat { get; protected set; }
        /// <summary>
        /// The ID of the projectile fired by this weapon (if applicable).
        /// </summary>
        public virtual int ProjectileID { get; protected set; } = -1;
        /// <summary>
        /// An array of multipliers used for weapon scaling. May be null if this item is not a weapon.
        /// </summary>
        public virtual float[] WeaponScaling { get; protected set; }
        /// <summary>
        /// The sound that is played when the player attacks with this weapon.
        /// </summary>
        public virtual AudioClip AttackSound { get; protected set; }
        /// <summary>
        /// A bonus % to crit chance for this weapon.
        /// </summary>
        public virtual float CritChanceBonus { get; protected set; }
        /// <summary>
        /// A bonus crit damage multiplier that is added to the base 1.5x crit power for this weapon.
        /// </summary>
        public virtual float CritPowerBonus { get; protected set; }

        /// <summary>
        /// Use to create a new ItemInfo. Make sure to call Register on it to register your Item.
        /// </summary>
        public ItemInfo(ItemType Type, string Name, string Desc, Texture Tex, int Value = -1, EquipStats Stats = default, Texture HeldTex = null, Texture HeadTex = null, Texture BodyTex = null, Texture ArmTex = null)
        {
            Tex.filterMode = FilterMode.Point;
            this.Type = Type;
            this.Name = Name;
            this.Desc = Desc;
            this.Value = Value;
            this.Stats = Stats;
            this.Tex = Tex;
            this.HeldTex = HeldTex;
            this.HeadTex = HeadTex;
            this.BodyTex = BodyTex;
            this.ArmTex = ArmTex;
        }

        /// <summary>
        /// Use to create a new ItemInfo. Make sure to call Register on it to register your Item.
        /// </summary>
        public ItemInfo(ItemType Type, string Name, string Desc, Material Mat, int Value = -1, EquipStats Stats = default, Material HeldMat = null, Material HeadMat = null, Material BodyMat = null, Material ArmMat = null)
        {
            this.Type = Type;
            this.Name = Name;
            this.Desc = Desc;
            this.Value = Value;
            this.Stats = Stats;
            this.Mat = Mat;
            this.HeldMat = HeldMat;
            this.HeadMat = HeadMat;
            this.BodyMat = BodyMat;
            this.ArmMat = ArmMat;
        }

        /// <summary>
        /// Sets the special info unique to weapons. This must be called before Register. If this is item is a weapon and you are using one of the provided OnAttack routines, you must call this.
        /// </summary>
        [Obsolete("Use the version that takes float crit parameters!")]
        public ItemInfo SetWeaponInfo(float[] WeaponScaling, AudioClip AttackSound, int ProjectileID = -1, int CritChanceBonus = 0, int CritPowerBonus = 0)
        {
            this.WeaponScaling = WeaponScaling;
            this.AttackSound = AttackSound;
            this.ProjectileID = ProjectileID;
            this.CritChanceBonus = CritChanceBonus;
            this.CritPowerBonus = CritPowerBonus;
            return this;
        }

        /// <summary>
        /// Sets the special info unique to weapons. This must be called before Register. If this is item is a weapon and you are using one of the provided OnAttack routines, you must call this. When setting the crit bonuses, explicitly pass the values as floats to avoid ambiguity with the obsolete version.
        /// </summary>
        public ItemInfo SetWeaponInfo(float[] WeaponScaling, AudioClip AttackSound, float CritChanceBonus = 0, float CritPowerBonus = 0, int ProjectileID = -1)
        {
            this.WeaponScaling = WeaponScaling;
            this.AttackSound = AttackSound;
            this.ProjectileID = ProjectileID;
            this.CritChanceBonus = CritChanceBonus;
            this.CritPowerBonus = CritPowerBonus;
            return this;
        }

        /// <summary>
        /// Registers this ItemInfo to the ItemRegistry.
        /// </summary>
        /// <param name="name">The registry name to use.</param>
        /// <param name="preferredID">If specified, will use this registry ID.</param>
        /// <param name="overrideExisting">If false, will not register if the preferred ID is already used. Ignored if no preferred ID is specified.</param>
        public virtual ItemInfo Register(string name, int preferredID = -1, bool overrideExisting = true)
        {
            return RegisterInternal(name, preferredID, overrideExisting) as ItemInfo;
        }

        /// <summary>
        /// Sets the tile placed by this item. You should probably not use this yourself, rather, you should put this ItemInfo into a TileInfo's constructor.
        /// </summary>
        public virtual ItemInfo SetTile(TileInfo Tile)
        {
            this.Tile = Tile;
            return this;
        }

        /// <summary>
        /// Called after this Registry Entry has been registered to its Registry. You should never call this yourself.
        /// </summary>
        protected internal override void PostRegister()
        {
            if ((Type & ItemType.BASIC_MASK) == (ItemType.WEAPON & ItemType.BASIC_MASK) && WeaponScaling == null) throw new ArgumentException("WeaponScaling must be set on weapons!");
            if (ProjectileID < 0) ProjectileID = ID;
            if (Mat == null)
            {
                Mat = new Material(Shader.Find("Unlit/Transparent"))
                {
                    mainTexture = Tex
                };
            }
            else
            {
                Tex = Mat.mainTexture;
            }
            GadgetCoreAPI.AddCustomResource("i/i" + ID, Mat);
            if (HeldTex != null || HeldMat != null)
            {
                if (HeldMat == null)
                {
                    HeldMat = new Material(Shader.Find("Unlit/Transparent Cutout"))
                    {
                        mainTexture = HeldTex
                    };
                }
                else
                {
                    HeldTex = HeldMat.mainTexture;
                }
                if ((Type & (ItemType.BASIC_MASK | ItemType.TYPE_MASK)) == ItemType.WEAPON) GadgetCoreAPI.AddCustomResource("ie/ie" + ID, HeldMat);
                if ((Type & (ItemType.BASIC_MASK | ItemType.TYPE_MASK)) == ItemType.OFFHAND) GadgetCoreAPI.AddCustomResource("o/o" + ID, HeldMat);
            }
            if (HeadTex != null || HeadMat != null)
            {
                if (HeadMat == null)
                {
                    HeadMat = new Material(Shader.Find("Unlit/Transparent Cutout"))
                    {
                        mainTexture = HeadTex
                    };
                }
                else
                {
                    HeadTex = HeadMat.mainTexture;
                }
                GadgetCoreAPI.AddCustomResource((Type & (ItemType.BASIC_MASK | ItemType.TYPE_MASK)) == ItemType.DROID ? ("droid/d" + ID + "h") : "h/h" + ID, HeadMat);
            }
            if (BodyTex != null || BodyMat != null)
            {
                if (BodyMat == null)
                {
                    BodyMat = new Material(Shader.Find("Unlit/Transparent Cutout"))
                    {
                        mainTexture = BodyTex
                    };
                }
                else
                {
                    BodyTex = BodyMat.mainTexture;
                }
                GadgetCoreAPI.AddCustomResource((Type & (ItemType.BASIC_MASK | ItemType.TYPE_MASK)) == ItemType.DROID ? ("droid/d" + ID + "b") : "b/b" + ID, BodyMat);
            }
            if (ArmTex != null || ArmMat != null)
            {
                if (ArmMat == null)
                {
                    ArmMat = new Material(Shader.Find("Unlit/Transparent Cutout"))
                    {
                        mainTexture = ArmTex
                    };
                }
                else
                {
                    ArmTex = ArmMat.mainTexture;
                }
                GadgetCoreAPI.AddCustomResource("a/a" + ID, ArmMat);
            }

            if (Value < 0)
            {
                if (ID == 52 || ID == 53)
                {
                    Value = 1;
                }
                else if (ID == 59)
                {
                    Value = 9999;
                }
                else if ((Type & (ItemType.BASIC_MASK | ItemType.TYPE_MASK)) == ItemType.LOOT)
                {
                    Value = 2;
                }
                else if ((Type & (ItemType.BASIC_MASK | ItemType.TYPE_MASK)) == ItemType.EMBLEM)
                {
                    Value = 15;
                }
                else if ((Type & ItemType.MOD) == ItemType.MOD)
                {
                    Value = 15;
                }
                else if ((Type & ItemType.EQUIPABLE) == ItemType.EQUIPABLE)
                {
                    Value = 20;
                }
                else if (ID < 100)
                {
                    Value = 2;
                }
                else if (ID < 300)
                {
                    Value = 15;
                }
                else if (ID < 2000)
                {
                    Value = 20;
                }
                else if (ID > 2500 && ID <= 3000)
                {
                    Value = 700;
                }
                else
                {
                    Value = 0;
                }
            }
            if (AttackSound != null)
            {
                GadgetCoreAPI.AddCustomResource("Au/i/i" + ID, AttackSound);
            }
        }

        /// <summary>
        /// Gets the amount of damage that this item will do. Returns 0 if WeaponScaling is null. Does not account for a crit. Preserves the ID-specific behavior of the base game, so if the ItemInfo's ID matches the ID of a vanilla item, it will behave in the exact same way that the vanilla item of the same ID would.
        /// </summary>
        public virtual int GetDamage(PlayerScript script)
        {
            if (WeaponScaling == null) return 0;
            if (GameScript.debugMode) return 99999;
            float dmg = 0;
            dmg += InstanceTracker.GameScript.GetFinalStat(0) * WeaponScaling[0];
            dmg += InstanceTracker.GameScript.GetFinalStat(1) * WeaponScaling[1];
            dmg += InstanceTracker.GameScript.GetFinalStat(2) * WeaponScaling[2];
            dmg += InstanceTracker.GameScript.GetFinalStat(3) * WeaponScaling[3];
            dmg += InstanceTracker.GameScript.GetFinalStat(4) * WeaponScaling[4];
            dmg += InstanceTracker.GameScript.GetFinalStat(5) * WeaponScaling[5];
            switch (ID)
            {
                case 321:
                    dmg += (float)(3 * (GameScript.maxmana - GameScript.mana));
                    break;
                case 366:
                    dmg += (float)(4 * (GameScript.maxhp - GameScript.hp));
                    break;
                case 370:
                    if (!PlayerScript.grounded)
                    {
                        dmg += InstanceTracker.GameScript.GetFinalStat(5) + InstanceTracker.GameScript.GetFinalStat(1);
                    }
                    break;
                case 376:
                    if (GameScript.mana == GameScript.maxmana)
                    {
                        dmg += InstanceTracker.GameScript.GetFinalStat(4) + InstanceTracker.GameScript.GetFinalStat(5);
                    }
                    break;
                case 379:
                    if (GameScript.mana >= 10)
                    {
                        dmg += InstanceTracker.GameScript.GetFinalStat(2);
                    }
                    break;
                case 424:
                    dmg += GameScript.mana;
                    break;
                case 429:
                    dmg += (GameScript.maxhp - GameScript.hp) * 4;
                    break;
                case 464:
                    dmg /= 2;
                    break;
                case 465:
                    dmg /= 2;
                    break;
                case 479:
                    dmg = 1200;
                    break;
                case 570:
                    if (GameScript.hp == 100)
                    {
                        dmg += InstanceTracker.GameScript.GetFinalStat(0) / 2;
                        dmg += InstanceTracker.GameScript.GetFinalStat(4) / 2;
                    }
                    break;
                case 578:
                    dmg += (GameScript.maxhp - GameScript.hp) * 7;
                    break;
            }
            return (int)dmg;
        }

        /// <summary>
        /// Gets the amount of damage that this item will do on a crit. Returns 0 if WeaponScaling is null. Preserves the ID-specific behavior of the base game, so if the ItemInfo's ID matches the ID of a vanilla item, it will behave in the exact same way that the vanilla item of the same ID would.
        /// </summary>
        public virtual int MultiplyCrit(PlayerScript script, int dmg)
        {
            if (GameScript.debugMode) return 99999;
            float num = CritPowerBonus;
            return ID != 596 ? (int)(dmg * (1.5f + (PlayerGearModsTracker.GetGearMods(script)[12] * 0.05f) + num)) : 700;
        }

        /// <summary>
        /// Mathematically attempts to trigger a critical attack. Returns true if a crit should occur.
        /// </summary>
        public virtual bool TryCrit(PlayerScript script)
        {
            float num = CritChanceBonus;
            if (Menuu.curUniform == 1)
            {
                num += 5;
            }
            return UnityEngine.Random.Range(0, 100) + (PlayerGearModsTracker.GetGearMods(script)[11] * 0.9f) + num >= 95f;
        }

        /// <summary>
        /// Gets the item's sell-value.
        /// </summary>
        public virtual int GetValue()
        {
            return Value;
        }

        /// <summary>
        /// Gets the item's name.
        /// </summary>
        public virtual string GetName()
        {
            return Name;
        }

        /// <summary>
        /// Gets the item's description.
        /// </summary>
        public virtual string GetDesc()
        {
            return Desc;
        }

        /// <summary>
        /// Gets the item's tier. Returns -1 if the item does not have a tier.
        /// </summary>
        public virtual int GetTier(Item item)
        {
            if ((Type & ItemType.LOOT) == ItemType.LOOT && (Type & ItemType.EQUIPABLE) == ItemType.GENERIC)
            {
                return (Type & ItemType.TIER_MASK) != ItemType.TIER10 ? ((int)(Type & ItemType.TIER_MASK) >> 8) : 10;
            }
            return -1;
        }

        /// <summary>
        /// Returns the single digit representative of the item's tier. 1-9 represents tiers 1-9, 0 represents tier 10. Returns -1 if the item does not have a tier.
        /// </summary>
        public virtual int GetTierDigit(Item item)
        {
            if ((Type & ItemType.LOOT) == ItemType.LOOT && (Type & ItemType.EQUIPABLE) == ItemType.GENERIC)
            {
                return (int)(Type & ItemType.TIER_MASK) >> 8;
            }
            return -1;
        }

        /// <summary>
        /// Returns the Registry Entry's Type enum. Used in the registration process, although it is safe to check this yourself by directly accessing the <see cref="Type"/> property.
        /// </summary>
        public override ItemType GetEntryType()
        {
            return Type;
        }

        /// <summary>
        /// Returns the singleton of the registry used for storing this type of Registry Entry.
        /// </summary>
        public override Registry<ItemInfo, ItemType> GetRegistry()
        {
            return Registry<ItemRegistry, ItemInfo, ItemType>.GetSingleton();
        }

        /// <summary>
        /// Returns whether the specified ID is valid for this Registry Entry's Type.
        /// </summary>
        public override bool IsValidIDForType(int id)
        {
            if (id <= 0) return false;
            if ((Type & ItemType.LOOT) == ItemType.LOOT && (Type & ItemType.EQUIPABLE) == ItemType.GENERIC)
            {
                return id % 10 == ((int)(Type & ItemType.TIER_MASK) >> 8);
            }
            return true;
        }

        /// <summary>
        /// Returns the next valid ID for this Registry Entry's Type, after the provided lastValidID. Should skip the vanilla ID range.
        /// </summary>
        public override int GetNextIDForType(int lastValidID)
        {
            if (lastValidID < GetRegistry().GetIDStart() - 1) lastValidID = GetRegistry().GetIDStart() - 1;
            if ((Type & ItemType.LOOT) == ItemType.LOOT && (Type & ItemType.EQUIPABLE) == ItemType.GENERIC)
            {
                int tier = (int)(Type & ItemType.TIER_MASK) >> 8;
                int lastValidIDTier = lastValidID % 10;
                if (tier == lastValidIDTier)
                {
                    return lastValidID + 10;
                }
                else if (tier > lastValidIDTier)
                {
                    return (lastValidID / 10 * 10) | tier;
                }
                else
                {
                    return ((lastValidID / 10 * 10) | tier) + 10;
                }
            }
            return ++lastValidID;
        }

        /// <summary>
        /// Returns true if this ItemInfo is ready to be registered. Returns false if it has already been registered, or if it is a weapon and SetWeaponInfo has not been called.
        /// </summary>
        public override bool ReadyToRegister()
        {
            return base.ReadyToRegister() && !((Type & ItemType.BASIC_MASK) == (ItemType.WEAPON & ItemType.BASIC_MASK) && WeaponScaling == null);
        }

        /// <summary>
        /// This event is invoked whenever the player uses the item while it is in the hotbar. Not to be confused with when a weapon is equipped and the player attacks with it. The inventory slot of the item is passed as the parameter. Return false to prevent the item from being used.
        /// </summary>
        public event Func<int, bool> OnUse;
        /// <summary>
        /// This event is invoked whenever the player uses the item while it is in the hotbar. Not to be confused with when a weapon is equipped and the player attacks with it. The inventory slot of the item is passed as the parameter. It must return an IEnumerator, so you can add Coroutines for this event. It is valid to return null if it is not a coroutine.
        /// </summary>
        public event Func<int, IEnumerator> OnUseFinal;
        /// <summary>
        /// This event is invoked whenever the player equips this item. (Weapon, Offhand, Helmet, Armor, Ring, or Droid). The inventory slot of this item is passed as the parameter.
        /// </summary>
        public event Action<int> OnEquip;
        /// <summary>
        /// This event is invoked whenever the player dequips this item. (Weapon, Offhand, Helmet, Armor, Ring, or Droid). The inventory slot of this item is passed as the parameter.
        /// </summary>
        public event Action<int> OnDequip;
        /// <summary>
        /// This event is invoked whenever the player attacks with this item. It must be a <see cref="ItemType.WEAPON"/>, and it must be equipped to the weapon slot. It must return an IEnumerator, so you can add Coroutines for this event. It is valid to return null if it is not a coroutine.
        /// </summary>
        public event Func<PlayerScript, IEnumerator> OnAttack;

        internal bool InvokeOnUse(int slot) { return OnUse?.Invoke(slot) ?? false; }
        internal void InvokeOnUseFinal(int slot) { OnUseFinal?.GetInvocationList().All(x => { InstanceTracker.GameScript.StartCoroutine((x as Func<int, IEnumerator>)?.Invoke(slot) ?? GadgetCoreAPI.EmptyEnumerator()); return true; }); }
        internal void InvokeOnEquip(int slot) { OnEquip?.Invoke(slot); }
        internal void InvokeOnDequip(int slot) { OnDequip?.Invoke(slot); }
        internal void InvokeOnAttack(PlayerScript script) { OnAttack?.GetInvocationList().All(x => { script.StartCoroutine((x as Func<PlayerScript, IEnumerator>)?.Invoke(script) ?? GadgetCoreAPI.EmptyEnumerator()); return true; }); }

        /// <summary>
        /// Gets the default attack routine for the given ItemInfo, assuming that it is has a vanilla ID. It is recommended to set OnAttack to this if you are overriding a vanilla weapon. This is only valid to use without specifying the ID parameter after registering the item.
        /// </summary>
        public Func<PlayerScript, IEnumerator> GetDefaultAttackType(int WepID = -1)
        {
            if (WepID == -1) WepID = ID;
            if (WepID < 300) return null;
            else if (WepID < 350)
            {
                if (WepID == 302 || WepID == 320 || WepID == 347 || WepID == 319 || WepID == 324 || WepID == 345)
                {
                    return SwingGiantSword;
                }
                else
                {
                    return SwingSword;
                }
            }
            else if (WepID < 400) return ThrustLance;
            else if (WepID < 450) return ShootGun;
            else if (WepID < 500) return ShootCannon;
            else if (WepID < 550) return CastGauntlet;
            else if (WepID < 600) return CastStaff;
            else return null;
        }

        /// <summary>
        /// Attack routine for swinging a sword. Register this to OnAttack to make your weapon behave this way. Preserves the ID-specific behavior of the base game, so if the ItemInfo's ID matches the ID of a vanilla item, it will behave in the exact same way that the vanilla item of the same ID would.
        /// </summary>
        public IEnumerator SwingSword(PlayerScript script)
        {
            canAttack.SetValue(script, false);
            attacking.SetValue(script, true);
            script.StartCoroutine(ATKSOUND.Invoke(InstanceTracker.PlayerScript, new object[] { }) as IEnumerator);
            script.StartCoroutine(SwordEffects.Invoke(script, new object[] { ID }) as IEnumerator);
            script.Animate(3);
            yield return new WaitForSeconds(0.3f);
            script.attackCube.SetActive(true);
            if (PlayerScript.inmagemash > 0)
            {
                MageMash.Invoke(script, new object[] { });
            }
            yield return new WaitForSeconds(0.2f);
            script.attackCube.SetActive(false);
            attacking.SetValue(script, false);
            yield return new WaitForSeconds(0.1f);
            canAttack.SetValue(script, true);
            yield break;
        }

        /// <summary>
        /// Attack routine for swinging a giant sword, which is 50% larger than a regular sword. Giant swords in the base game are the Colossus, Heaven's Cloud, Caius' Demonblade, Zweihander, Claymore, and Azazel's Blade. Register this to OnAttack to make your weapon behave this way. Preserves the ID-specific behavior of the base game, so if the ItemInfo's ID matches the ID of a vanilla item, it will behave in the exact same way that the vanilla item of the same ID would.
        /// </summary>
        public IEnumerator SwingGiantSword(PlayerScript script)
        {
            canAttack.SetValue(script, false);
            attacking.SetValue(script, true);
            script.StartCoroutine(ATKSOUND.Invoke(script, new object[] { }) as IEnumerator);
            script.StartCoroutine(SwordEffects.Invoke(script, new object[] { ID }) as IEnumerator);
            script.Animate(3);
            yield return new WaitForSeconds(0.3f);
            script.attackCube2.SetActive(true);
            if (PlayerScript.inmagemash > 0)
            {
                MageMash.Invoke(script, new object[] { });
            }
            yield return new WaitForSeconds(0.2f);
            script.attackCube2.SetActive(false);
            attacking.SetValue(script, false);
            yield return new WaitForSeconds(0.1f);
            canAttack.SetValue(script, true);
            yield break;
        }

        /// <summary>
        /// Attack routine for thrusting a lance. Register this to OnAttack to make your weapon behave this way. Preserves the ID-specific behavior of the base game, so if the ItemInfo's ID matches the ID of a vanilla item, it will behave in the exact same way that the vanilla item of the same ID would.
        /// </summary>
        public IEnumerator ThrustLance(PlayerScript script)
        {
            canAttack.SetValue(script, false);
            attacking.SetValue(script, true);
            script.StartCoroutine(ATKSOUND.Invoke(script, new object[] { }) as IEnumerator);
            script.StartCoroutine(LanceEffects.Invoke(script, new object[] { ID }) as IEnumerator);
            script.Animate(5);
            yield return new WaitForSeconds(0.3f);
            script.attackCube3.SetActive(true);
            if (PlayerScript.inmagemash > 0)
            {
                MageMash.Invoke(script, new object[] { });
            }
            float bonus = 0.2f;
            if (ID == 370)
            {
                bonus = 0.4f;
            }
            yield return new WaitForSeconds(bonus);
            script.attackCube3.SetActive(false);
            attacking.SetValue(script, false);
            yield return new WaitForSeconds(0.1f);
            canAttack.SetValue(script, true);
            yield break;
        }

        /// <summary>
        /// Attack routine for shooting a gun. Register this to OnAttack to make your weapon behave this way. Preserves the ID-specific behavior of the base game, so if the ItemInfo's ID matches the ID of a vanilla item, it will behave in the exact same way that the vanilla item of the same ID would.
        /// </summary>
        public IEnumerator ShootGun(PlayerScript script)
        {
            canAttack.SetValue(script, false);
            attacking.SetValue(script, true);
            script.StartCoroutine(ATKSOUND.Invoke(script, new object[] { }) as IEnumerator);
            script.StartCoroutine(GunEffects.Invoke(script, new object[] { ID }) as IEnumerator);
            script.GetComponent<AudioSource>().PlayOneShot((AudioClip)Resources.Load("Au/shoot"), Menuu.soundLevel / 10f);
            script.Animate(4);
            yield return new WaitForSeconds(0.3f);
            int dmg = GetDamage(script);
            switch (ID)
            {
                case 413:
                    TurretS.Invoke(script, new object[] { InstanceTracker.GameScript.GetFinalStat(3) / 3 });
                    break;
                case 415:
                    TurretS.Invoke(script, new object[] { InstanceTracker.GameScript.GetFinalStat(4) / 2 });
                    break;
                case 421:
                    script.StartCoroutine(SiriusS.Invoke(script, new object[] { }) as IEnumerator);
                    break;
            }
            if ((bool)hyper.GetValue(script))
            {
                hyper.SetValue(script, false);
                HyperBeam.Invoke(script, new object[] { });
            }
            if (ID == 428)
            {
                script.StartCoroutine(Cerberus.Invoke(script, new object[] { }) as IEnumerator);
            }
            else
            {
                if (TryCrit(script))
                {
                    dmg = MultiplyCrit(script, dmg);
                    script.GetComponent<AudioSource>().PlayOneShot(script.critSound, Menuu.soundLevel / 10f);
                    UnityEngine.Object.Instantiate(script.crit, script.transform.position, Quaternion.identity);
                }
                Vector3 vector = GadgetCoreAPI.GetCursorPos() - script.transform.position;
                Package2 value = new Package2(vector, dmg, ProjectileID, PlayerGearModsTracker.GetGearMods(script)[10]);
                GameObject gameObject = (GameObject)UnityEngine.Object.Instantiate(Resources.Load("proj/shot" + ProjectileID), script.shot.transform.position, Quaternion.identity);
                gameObject.SendMessage("Set", value);
                script.GetComponent<NetworkView>().RPC("ShootProjectile", RPCMode.Others, new object[]
                {
                    ProjectileID,
                    vector,
                    dmg
                });
                if ((bool)triple.GetValue(script))
                {
                    script.StartCoroutine(TripleShot.Invoke(script, new object[] { }) as IEnumerator);
                }
            }
            yield return new WaitForSeconds(0.3f);
            attacking.SetValue(script, false);
            yield return new WaitForSeconds(0.1f);
            canAttack.SetValue(script, true);
            yield break;
        }
        
        /// <summary>
        /// Attack routine for shooting a cannon. Register this to OnAttack to make your weapon behave this way. Preserves the ID-specific behavior of the base game, so if the ItemInfo's ID matches the ID of a vanilla item, it will behave in the exact same way that the vanilla item of the same ID would.
        /// </summary>
        public IEnumerator ShootCannon(PlayerScript script)
        {
            canAttack.SetValue(script, false);
            attacking.SetValue(script, true);
            script.StartCoroutine(ATKSOUND.Invoke(script, new object[] { }) as IEnumerator);
            script.GetComponent<AudioSource>().PlayOneShot((AudioClip)Resources.Load("Au/shoot"), Menuu.soundLevel / 10f);
            script.Animate(4);
            yield return new WaitForSeconds(0.3f);
            int dmg = GetDamage(script);
            switch (ID)
            {
                case 466:
                    UnityEngine.Object.Instantiate(Resources.Load("burst"), script.shot.transform.position, Quaternion.identity);
                    break;
            }
            if ((bool)hyper.GetValue(script))
            {
                hyper.SetValue(script, false);
                HyperBeam.Invoke(script, new object[] { });
            }
            if (TryCrit(script))
            {
                dmg = MultiplyCrit(script, dmg);
                script.GetComponent<AudioSource>().PlayOneShot(script.critSound, Menuu.soundLevel / 10f);
                UnityEngine.Object.Instantiate<GameObject>(script.crit, script.transform.position, Quaternion.identity);
            }
            if (ID == 472)
            {
                GameScript.energy -= 7;
                InstanceTracker.GameScript.UpdateEnergy();
            }
            if (GadgetCoreAPI.GetCursorPos().x > script.transform.position.x)
            {
                (r.GetValue(script) as Rigidbody).velocity = new Vector3(-10f, (r.GetValue(script) as Rigidbody).velocity.y + 5f, 0f);
            }
            else
            {
                (r.GetValue(script) as Rigidbody).velocity = new Vector3(10f, (r.GetValue(script) as Rigidbody).velocity.y + 5f, 0f);
            }
            Vector3 targ = GadgetCoreAPI.GetCursorPos() - script.transform.position;
            float dmgdmg = (dmg / 2);
            if (ID == 497)
            {
                Package2 value2 = new Package2(targ, (int)dmgdmg, ProjectileID, PlayerGearModsTracker.GetGearMods(script)[10]);
                for (int i = 0; i < 3; i++)
                {
                    GameObject gameObject2 = (GameObject)UnityEngine.Object.Instantiate(Resources.Load("proj/shot" + ProjectileID), script.shot.transform.position, Quaternion.identity);
                    gameObject2.SendMessage("Set", value2);
                    script.GetComponent<NetworkView>().RPC("ShootProjectile", RPCMode.Others, new object[]
                    {
                        ProjectileID,
                        targ,
                        (int)dmgdmg
                    });
                    if ((bool)triple.GetValue(script))
                    {
                        script.StartCoroutine(TripleShot.Invoke(script, new object[] { }) as IEnumerator);
                    }
                }
            }
            else if (ID == 464)
            {
                Package2 value3 = new Package2(targ, dmg, ProjectileID, PlayerGearModsTracker.GetGearMods(script)[10]);
                GameObject gameObject3 = (GameObject)UnityEngine.Object.Instantiate(Resources.Load("proj/shot" + ProjectileID), script.shot.transform.position, Quaternion.identity);
                gameObject3.SendMessage("Set", value3);
                script.GetComponent<NetworkView>().RPC("ShootProjectile", RPCMode.Others, new object[]
                {
                    ProjectileID,
                    targ,
                    dmg
                });
                script.StartCoroutine(TripleShot.Invoke(script, new object[] { }) as IEnumerator);
            }
            else if (ID == 465)
            {
                Package2 value4 = new Package2(targ, dmg, ProjectileID, PlayerGearModsTracker.GetGearMods(script)[10]);
                GameObject gameObject4 = (GameObject)UnityEngine.Object.Instantiate(Resources.Load("proj/shot" + ProjectileID), script.shot.transform.position, Quaternion.identity);
                gameObject4.SendMessage("Set", value4);
                script.GetComponent<NetworkView>().RPC("ShootProjectile", RPCMode.Others, new object[]
                {
                    ProjectileID,
                    targ,
                    dmg
                });
                if ((bool)triple.GetValue(script))
                {
                    script.StartCoroutine(TripleShot.Invoke(script, new object[] { }) as IEnumerator);
                }
                Package2 value5 = new Package2(targ, dmg, ProjectileID, PlayerGearModsTracker.GetGearMods(script)[10]);
                gameObject4 = (GameObject)UnityEngine.Object.Instantiate(Resources.Load("proj/shot" + ProjectileID), new Vector3(script.shot.transform.position.x, script.shot.transform.position.y + 2f, 0f), Quaternion.identity);
                gameObject4.SendMessage("Set", value5);
                script.GetComponent<NetworkView>().RPC("ShootProjectile", RPCMode.Others, new object[]
                {
                    ProjectileID,
                    targ,
                    dmg
                });
                if ((bool)triple.GetValue(script))
                {
                    script.StartCoroutine(TripleShot.Invoke(script, new object[] { }) as IEnumerator);
                }
                Package2 value6 = new Package2(targ, dmg, ProjectileID, PlayerGearModsTracker.GetGearMods(script)[10]);
                gameObject4 = (GameObject)UnityEngine.Object.Instantiate(Resources.Load("proj/shot" + ProjectileID), new Vector3(script.shot.transform.position.x, script.shot.transform.position.y - 2f, 0f), Quaternion.identity);
                gameObject4.SendMessage("Set", value6);
                script.GetComponent<NetworkView>().RPC("ShootProjectile", RPCMode.Others, new object[]
                {
                    ProjectileID,
                    targ,
                    dmg
                });
                if ((bool)triple.GetValue(script))
                {
                    script.StartCoroutine(TripleShot.Invoke(script, new object[] { }) as IEnumerator);
                }
            }
            else if (ID == 469)
            {
                Package2 value7 = new Package2(targ, dmg, ProjectileID, PlayerGearModsTracker.GetGearMods(script)[10]);
                GameObject gameObject5 = (GameObject)UnityEngine.Object.Instantiate(Resources.Load("proj/shot" + ProjectileID), script.shot.transform.position, Quaternion.identity);
                gameObject5.SendMessage("Set", value7);
                script.GetComponent<NetworkView>().RPC("ShootProjectile", RPCMode.Others, new object[]
                {
                    ProjectileID,
                    targ,
                    dmg
                });
                script.StartCoroutine(TripleShot.Invoke(script, new object[] { }) as IEnumerator);
            }
            else if (ID == 473)
            {
                if (UnityEngine.Random.Range(0, 3) == 0)
                {
                    Turret.Invoke(script, new object[] { InstanceTracker.GameScript.GetFinalStat(3) });
                }
                else
                {
                    Plasma.Invoke(script, new object[] { });
                }
            }
            else if (ID == 477)
            {
                targ = GadgetCoreAPI.GetCursorPos() - new Vector3(script.transform.position.x, script.transform.position.y + 5f, 0f);
                Package2 value8 = new Package2(targ, dmg, ProjectileID, PlayerGearModsTracker.GetGearMods(script)[10]);
                GameObject gameObject6 = (GameObject)UnityEngine.Object.Instantiate(Resources.Load("proj/shot" + ProjectileID), script.shot.transform.position, Quaternion.identity);
                gameObject6.SendMessage("Set", value8);
                script.GetComponent<NetworkView>().RPC("ShootProjectile", RPCMode.Others, new object[]
                {
                    ProjectileID,
                    targ,
                    dmg
                });
                if ((bool)triple.GetValue(script))
                {
                    script.StartCoroutine(TripleShot.Invoke(script, new object[] { }) as IEnumerator);
                }
                targ = GadgetCoreAPI.GetCursorPos() - new Vector3(script.transform.position.x, script.transform.position.y - 5f, 0f);
                Package2 value9 = new Package2(targ, dmg, ProjectileID, PlayerGearModsTracker.GetGearMods(script)[10]);
                GameObject gameObject7 = (GameObject)UnityEngine.Object.Instantiate(Resources.Load("proj/shot" + ProjectileID), script.shot.transform.position, Quaternion.identity);
                gameObject7.SendMessage("Set", value9);
                script.GetComponent<NetworkView>().RPC("ShootProjectile", RPCMode.Others, new object[]
                {
                    ProjectileID,
                    targ,
                    dmg
                });
                if ((bool)triple.GetValue(script))
                {
                    script.StartCoroutine(TripleShot.Invoke(script, new object[] { }) as IEnumerator);
                }
            }
            else if (ID == 478)
            {
                if (Network.isServer)
                {
                    Staff.Invoke(script, new object[] { ProjectileID, dmg, Network.player });
                }
                else
                {
                    script.GetComponent<NetworkView>().RPC("Staff", RPCMode.Server, new object[]
                    {
                        ProjectileID,
                        dmg,
                        Network.player
                    });
                }
            }
            else if (ID == 495)
            {
                if ((int)curShot.GetValue(script) == 0)
                {
                    Package2 value10 = new Package2(targ, dmg, ProjectileID, PlayerGearModsTracker.GetGearMods(script)[10]);
                    GameObject gameObject8 = (GameObject)UnityEngine.Object.Instantiate(Resources.Load("proj/schegF"), script.shot.transform.position, Quaternion.identity);
                    gameObject8.SendMessage("Set", value10);
                    script.GetComponent<NetworkView>().RPC("ShootProjectile", RPCMode.Others, new object[]
                    {
                        ProjectileID,
                        targ,
                        dmg
                    });
                    script.GetComponent<AudioSource>().PlayOneShot((AudioClip)Resources.Load("Au/schegF"), Menuu.soundLevel / 10f);
                }
                else if ((int)curShot.GetValue(script) == 1)
                {
                    Package2 value11 = new Package2(targ, dmg, ProjectileID, PlayerGearModsTracker.GetGearMods(script)[10]);
                    GameObject gameObject9 = (GameObject)UnityEngine.Object.Instantiate(Resources.Load("proj/schegT"), script.shot.transform.position, Quaternion.identity);
                    gameObject9.SendMessage("Set", value11);
                    script.GetComponent<NetworkView>().RPC("ShootProjectile", RPCMode.Others, new object[]
                    {
                        ProjectileID,
                        targ,
                        dmg
                    });
                    script.GetComponent<AudioSource>().PlayOneShot((AudioClip)Resources.Load("Au/schegT"), Menuu.soundLevel / 10f);
                }
                else if ((int)curShot.GetValue(script) == 2)
                {
                    script.StartCoroutine(SchegI.Invoke(script, new object[] { dmg }) as IEnumerator);
                }
                curShot.SetValue(script, (int)curShot.GetValue(script) + 1);
                if ((int)curShot.GetValue(script) > 2)
                {
                    curShot.SetValue(script, 0);
                }
            }
            else
            {
                Package2 value12 = new Package2(targ, dmg, ProjectileID, PlayerGearModsTracker.GetGearMods(script)[10]);
                GameObject gameObject10 = (GameObject)UnityEngine.Object.Instantiate(Resources.Load("proj/shot" + ProjectileID), script.shot.transform.position, Quaternion.identity);
                gameObject10.SendMessage("Set", value12);
                script.GetComponent<NetworkView>().RPC("ShootProjectile", RPCMode.Others, new object[]
                {
                    ProjectileID,
                    targ,
                    dmg
                });
                if ((bool)triple.GetValue(script))
                {
                    script.StartCoroutine(TripleShot.Invoke(script, new object[] { }) as IEnumerator);
                }
            }
            yield return new WaitForSeconds(0.3f);
            attacking.SetValue(script, false);
            yield return new WaitForSeconds(0.1f);
            canAttack.SetValue(script, true);
            yield break;
        }

        /// <summary>
        /// Attack routine for casting a gauntlet. Register this to OnAttack to make your weapon behave this way. Preserves the ID-specific behavior of the base game, so if the ItemInfo's ID matches the ID of a vanilla item, it will behave in the exact same way that the vanilla item of the same ID would.
        /// </summary>
        public IEnumerator CastGauntlet(PlayerScript script)
        {
            canAttack.SetValue(script, false);
            attacking.SetValue(script, true);
            script.StartCoroutine(ATKSOUND.Invoke(script, new object[] { }) as IEnumerator);
            script.GetComponent<AudioSource>().PlayOneShot((AudioClip)Resources.Load("Au/shoot"), Menuu.soundLevel / 10f);
            script.Animate(3);
            yield return new WaitForSeconds(0.3f);
            int dmg = GetDamage(script);
            if (ID == 526)
            {
                GameScript.mana -= 5;
                InstanceTracker.GameScript.UpdateMana();
            }
            else if (ID == 522)
            {
                GameScript.mana -= 3;
                InstanceTracker.GameScript.UpdateMana();
            }
            if (TryCrit(script))
            {
                dmg = MultiplyCrit(script, dmg);
                script.GetComponent<AudioSource>().PlayOneShot(script.critSound, Menuu.soundLevel / 10f);
                UnityEngine.Object.Instantiate<GameObject>(script.crit, script.transform.position, Quaternion.identity);
            }
            if (ID == 547)
            {
                Vector3 vector2 = GadgetCoreAPI.GetCursorPos();
                GameObject gameObject11 = (GameObject)UnityEngine.Object.Instantiate(Resources.Load("proj/shot" + ProjectileID), script.shot.transform.position, Quaternion.identity);
                Package2 value13 = new Package2(vector2, dmg, ProjectileID, PlayerGearModsTracker.GetGearMods(script)[10]);
                gameObject11.SendMessage("Set", value13);
                script.GetComponent<NetworkView>().RPC("ShootProjectile", RPCMode.Others, new object[]
                {
                    ProjectileID,
                    vector2,
                    dmg
                });
            }
            else if (ID == 546)
            {
                Vector3 vector3 = GadgetCoreAPI.GetCursorPos() - script.transform.position;
                vector3 = new Vector3(vector3.x + (float)UnityEngine.Random.Range(-2, 3), vector3.y + (float)UnityEngine.Random.Range(-2, 3), 0f);
                Package2 value14 = new Package2(vector3, dmg, ProjectileID, PlayerGearModsTracker.GetGearMods(script)[10]);
                GameObject gameObject12 = (GameObject)UnityEngine.Object.Instantiate(Resources.Load("proj/shot" + ProjectileID), new Vector3(script.shot.transform.position.x, script.shot.transform.position.y + 5f, 0f), Quaternion.identity);
                gameObject12.SendMessage("Set", value14);
                script.GetComponent<NetworkView>().RPC("ShootProjectile", RPCMode.Others, new object[]
                {
                    ProjectileID,
                    vector3,
                    dmg
                });
                if ((bool)triple.GetValue(script))
                {
                    script.StartCoroutine(TripleShot.Invoke(script, new object[] { }) as IEnumerator);
                }
                Vector3 dir = new Vector3(vector3.x + (float)UnityEngine.Random.Range(-2, 3), vector3.y + (float)UnityEngine.Random.Range(-2, 3), 0f);
                Package2 value15 = new Package2(dir, dmg, ProjectileID, PlayerGearModsTracker.GetGearMods(script)[10]);
                GameObject gameObject13 = (GameObject)UnityEngine.Object.Instantiate(Resources.Load("proj/shot" + ProjectileID), new Vector3(script.shot.transform.position.x, script.shot.transform.position.y + 10f, 0f), Quaternion.identity);
                gameObject13.SendMessage("Set", value15);
                script.GetComponent<NetworkView>().RPC("ShootProjectile", RPCMode.Others, new object[]
                {
                    ProjectileID,
                    vector3,
                    dmg
                });
                if ((bool)triple.GetValue(script))
                {
                    script.StartCoroutine(TripleShot.Invoke(script, new object[] { }) as IEnumerator);
                }
            }
            else if (ID == 513)
            {
                Vector3 vector4 = new Vector3((float)(UnityEngine.Random.Range(-1, 2) * 5), (float)(UnityEngine.Random.Range(-1, 2) * 5), 0f);
                Vector3 vector5 = GadgetCoreAPI.GetCursorPos();
                Package3 value16 = new Package3(vector5, vector4, dmg, PlayerGearModsTracker.GetGearMods(script)[10]);
                GameObject gameObject14 = (GameObject)UnityEngine.Object.Instantiate(Resources.Load("proj/shot" + ProjectileID), script.shot.transform.position, Quaternion.identity);
                gameObject14.SendMessage("Set2", value16);
                script.GetComponent<NetworkView>().RPC("ShootProjectile2", RPCMode.Others, new object[]
                {
                    ProjectileID,
                    vector5,
                    dmg,
                    vector4
                });
                Vector3 vector6 = new Vector3((float)(UnityEngine.Random.Range(-1, 2) * 5), (float)(UnityEngine.Random.Range(-1, 2) * 5), 0f);
                Vector3 vector7 = GadgetCoreAPI.GetCursorPos();
                Package3 value17 = new Package3(vector7, vector6, dmg, PlayerGearModsTracker.GetGearMods(script)[10]);
                GameObject gameObject15 = (GameObject)UnityEngine.Object.Instantiate(Resources.Load("proj/shot" + ProjectileID), script.shot.transform.position, Quaternion.identity);
                gameObject15.SendMessage("Set2", value17);
                script.GetComponent<NetworkView>().RPC("ShootProjectile2", RPCMode.Others, new object[]
                {
                    ProjectileID,
                    vector7,
                    dmg,
                    vector6
                });
            }
            else if (ID == 517)
            {
                Vector3 vector8 = new Vector3((float)(UnityEngine.Random.Range(-1, 2) * 5), (float)(UnityEngine.Random.Range(-1, 2) * 5), 0f);
                Vector3 vector9 = GadgetCoreAPI.GetCursorPos();
                Package3 value18 = new Package3(Vector3.up, vector8, dmg, PlayerGearModsTracker.GetGearMods(script)[10]);
                GameObject gameObject16 = (GameObject)UnityEngine.Object.Instantiate(Resources.Load("proj/shot" + ProjectileID), new Vector3(vector9.x, vector9.y, 0f), Quaternion.identity);
                gameObject16.SendMessage("Set2", value18);
                script.GetComponent<NetworkView>().RPC("ShootProjectile2", RPCMode.Others, new object[]
                {
                    ProjectileID,
                    vector9,
                    dmg,
                    vector8
                });
            }
            else if (ID == 519)
            {
                TurretF.Invoke(script, new object[] { dmg });
            }
            else if (ID == 528)
            {
                Vector3 vector10 = new Vector3((float)(UnityEngine.Random.Range(-1, 2) * 5), (float)(UnityEngine.Random.Range(-1, 2) * 5), 0f);
                Vector3 vector11 = GadgetCoreAPI.GetCursorPos();
                Package3 value19 = new Package3(vector11, vector10, dmg, PlayerGearModsTracker.GetGearMods(script)[10]);
                GameObject gameObject17 = (GameObject)UnityEngine.Object.Instantiate(Resources.Load("proj/shot" + ProjectileID), script.shot.transform.position, Quaternion.identity);
                gameObject17.SendMessage("Set2", value19);
                script.GetComponent<NetworkView>().RPC("ShootProjectile2", RPCMode.Others, new object[]
                {
                    ProjectileID,
                    vector11,
                    dmg,
                    vector10
                });
                Vector3 vector12 = new Vector3((float)(UnityEngine.Random.Range(-1, 2) * 5), (float)(UnityEngine.Random.Range(-1, 2) * 5), 0f);
                Vector3 vector13 = GadgetCoreAPI.GetCursorPos();
                Package3 value20 = new Package3(vector13, vector12, dmg, PlayerGearModsTracker.GetGearMods(script)[10]);
                GameObject gameObject18 = (GameObject)UnityEngine.Object.Instantiate(Resources.Load("proj/shot" + ProjectileID), script.shot.transform.position, Quaternion.identity);
                gameObject18.SendMessage("Set2", value20);
                script.GetComponent<NetworkView>().RPC("ShootProjectile2", RPCMode.Others, new object[]
                {
                    ProjectileID,
                    vector13,
                    dmg,
                    vector12
                });
                Vector3 vector14 = new Vector3((float)(UnityEngine.Random.Range(-1, 2) * 5), (float)(UnityEngine.Random.Range(-1, 2) * 5), 0f);
                Vector3 vector15 = GadgetCoreAPI.GetCursorPos();
                Package3 value21 = new Package3(vector15, vector14, dmg, PlayerGearModsTracker.GetGearMods(script)[10]);
                GameObject gameObject19 = (GameObject)UnityEngine.Object.Instantiate(Resources.Load("proj/shot" + ProjectileID), script.shot.transform.position, Quaternion.identity);
                gameObject19.SendMessage("Set2", value21);
                script.GetComponent<NetworkView>().RPC("ShootProjectile2", RPCMode.Others, new object[]
                {
                    ProjectileID,
                    vector15,
                    dmg,
                    vector14
                });
            }
            else if (ID == 525)
            {
                script.StartCoroutine(AirSplit.Invoke(script, new object[] { }) as IEnumerator);
            }
            else
            {
                Vector3 vector16 = new Vector3((float)(UnityEngine.Random.Range(-1, 2) * 5), (float)(UnityEngine.Random.Range(-1, 2) * 5), 0f);
                Vector3 vector17 = GadgetCoreAPI.GetCursorPos();
                Package3 value22 = new Package3(vector17, vector16, dmg, PlayerGearModsTracker.GetGearMods(script)[10]);
                GameObject gameObject20 = (GameObject)UnityEngine.Object.Instantiate(Resources.Load("proj/shot" + ProjectileID), script.shot.transform.position, Quaternion.identity);
                gameObject20.SendMessage("Set2", value22);
                script.GetComponent<NetworkView>().RPC("ShootProjectile2", RPCMode.Others, new object[]
                {
                    ProjectileID,
                    vector17,
                    dmg,
                    vector16
                });
            }
            yield return new WaitForSeconds(0.3f);
            attacking.SetValue(script, false);
            yield return new WaitForSeconds(0.1f);
            canAttack.SetValue(script, true);
            yield break;
        }

        /// <summary>
        /// Attack routine for casting a staff. Register this to OnAttack to make your weapon behave this way. Preserves the ID-specific behavior of the base game, so if the ItemInfo's ID matches the ID of a vanilla item, it will behave in the exact same way that the vanilla item of the same ID would.
        /// </summary>
        public IEnumerator CastStaff(PlayerScript script)
        {
            canAttack.SetValue(script, false);
            attacking.SetValue(script, true);
            script.StartCoroutine(ATKSOUND.Invoke(script, new object[] { }) as IEnumerator);
            script.Animate(3);
            yield return new WaitForSeconds(0.3f);
            int dmg = GetDamage(script);
            if (ID == 563 || ID == 574)
            {
                GameScript.mana -= 2;
                InstanceTracker.GameScript.UpdateMana();
            }
            else if (ID == 565 || ID == 573 || ID == 579)
            {
                GameScript.mana -= 5;
                InstanceTracker.GameScript.UpdateMana();
            }
            else if (ID == 577)
            {
                GameScript.mana -= 8;
                InstanceTracker.GameScript.UpdateMana();
            }
            else if (ID == 569)
            {
                if (GameScript.hp < GameScript.maxhp)
                {
                    GameScript.hp++;
                    InstanceTracker.GameScript.UpdateHP();
                }
                if (GameScript.burn > 0)
                {
                    GameScript.burn -= 10;
                }
                if (GameScript.frost > 0)
                {
                    GameScript.frost -= 10;
                }
                if (GameScript.poison > 0)
                {
                    GameScript.poison -= 10;
                }
            }
            if (TryCrit(script) || ID == 576)
            {
                dmg = MultiplyCrit(script, dmg);
                script.GetComponent<AudioSource>().PlayOneShot(script.critSound, Menuu.soundLevel / 10f);
                UnityEngine.Object.Instantiate<GameObject>(script.crit, script.transform.position, Quaternion.identity);
            }
            if (ID == 597)
            {
                Vector3 vector18 = GadgetCoreAPI.GetCursorPos() - script.transform.position;
                Package2 value23 = new Package2(vector18, dmg, ProjectileID, PlayerGearModsTracker.GetGearMods(script)[10]);
                GameObject gameObject21 = (GameObject)UnityEngine.Object.Instantiate(Resources.Load("proj/shot" + ProjectileID), new Vector3(script.shot.transform.position.x, script.shot.transform.position.y + 5f, 0f), Quaternion.identity);
                gameObject21.SendMessage("Set", value23);
                script.GetComponent<NetworkView>().RPC("ShootProjectile", RPCMode.Others, new object[]
                {
                    ProjectileID,
                    vector18,
                    dmg
                });
                if ((bool)triple.GetValue(script))
                {
                    script.StartCoroutine(TripleShot.Invoke(script, new object[] { }) as IEnumerator);
                }
            }
            else if (ID == 567)
            {
                Vector3 vector19 = new Vector3((float)(UnityEngine.Random.Range(-1, 2) * 5), (float)(UnityEngine.Random.Range(-1, 2) * 5), 0f);
                Vector3 vector20 = GadgetCoreAPI.GetCursorPos();
                Package3 value24 = new Package3(vector20, vector19, dmg, PlayerGearModsTracker.GetGearMods(script)[10]);
                GameObject gameObject22 = (GameObject)UnityEngine.Object.Instantiate(Resources.Load("proj/shot" + ProjectileID), script.shot.transform.position, Quaternion.identity);
                gameObject22.SendMessage("Set2", value24);
                script.GetComponent<NetworkView>().RPC("ShootProjectile2", RPCMode.Others, new object[]
                {
                    ProjectileID,
                    vector20,
                    dmg,
                    vector19
                });
            }
            else if (ID == 574)
            {
                Shock.Invoke(script, new object[] { });
            }
            else if (ID == 575)
            {
                script.StartCoroutine(CasterSword.Invoke(script, new object[] { }) as IEnumerator);
            }
            else
            {
                if (Network.isServer)
                {
                    Staff.Invoke(script, new object[] { ProjectileID, dmg, Network.player });
                }
                else
                {
                    script.GetComponent<NetworkView>().RPC("Staff", RPCMode.Server, new object[]
                    {
                        ProjectileID,
                        dmg,
                        Network.player
                    });
                }
            }
            yield return new WaitForSeconds(0.3f);
            attacking.SetValue(script, false);
            yield return new WaitForSeconds(0.1f);
            canAttack.SetValue(script, true);
            yield break;
        }
    }
}
