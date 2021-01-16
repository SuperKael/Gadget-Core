using System;
using System.Collections.Generic;

namespace GadgetCore.API
{
    /// <summary>
    /// This registry is filled with ItemInfos, and is used for registering custom items to the game.
    /// </summary>
    public class ItemRegistry : Registry<ItemRegistry, ItemInfo, ItemType>
    {
        private static Dictionary<string, int> itemIDsByName;
        private static Dictionary<string, int> itemIDsByRegistryName;

        /// <summary>
        /// The name of this registry.
        /// </summary>
        public const string REGISTRY_NAME = "Item";

        static ItemRegistry()
        {
            InitializeVanillaItemIDNames();
        }

        /// <summary>
        /// Gets the name of this registry. Must be constant. Returns <see cref="REGISTRY_NAME"/>.
        /// </summary>
        public override string GetRegistryName()
        {
            return REGISTRY_NAME;
        }

        /// <summary>
        /// Gets the item ID for the given name. Case-insensitive. Returns -1 if there is no item with the given name.
        /// </summary>
        public static int GetItemIDByName(string name)
        {
            name = name.ToLowerInvariant();
            if (itemIDsByName.ContainsKey(name))
            {
                return itemIDsByName[name];
            }
            return -1;
        }

        /// <summary>
        /// Gets the item ID for the given registry name. Case-insensitive. Returns -1 if there is no item with the given name.
        /// </summary>
        public static int GetItemIDByRegistryName(string name)
        {
            name = name.ToLowerInvariant();
            if (itemIDsByRegistryName.ContainsKey(name))
            {
                return itemIDsByRegistryName[name];
            }
            return 0;
        }

        /// <summary>
        /// Called after the specified Registry Entry has been registered. You should never call this yourself. Note that this is called before <see cref="RegistryEntry{E, T}.PostRegister"/>
        /// </summary>
        protected override void PostRegistration(ItemInfo entry)
        {
            itemIDsByName[entry.Name] = entry.ID;
            itemIDsByRegistryName[entry.RegistryName] = entry.ID;
        }

        /// <summary>
        /// Called just before an entry is removed from the registry by <see cref="Registry.UnregisterGadget(GadgetInfo)"/>
        /// </summary>
        protected override void OnUnregister(ItemInfo entry)
        {
            itemIDsByName.Remove(entry.Name);
            itemIDsByRegistryName.Remove(entry.RegistryName);
        }

        /// <summary>
        /// Gets the default type of the given ID, assuming that it is a vanilla ID.
        /// </summary>
        public static ItemType GetDefaultTypeByID(int ID)
        {
            if (ID > 0 && ID <= 40)
            {
                if (ID <= 10) return ItemType.LOOT | ItemType.ORE;
                if (ID <= 20) return ItemType.LOOT | ItemType.PLANT;
                if (ID <= 30) return ItemType.LOOT | ItemType.MONSTER;
                return ItemType.LOOT | ItemType.BUG;
            }
            if (ID > 100 && ID <= 200)
            {
                if (ID <= 110) return ItemType.EMBLEM | ItemType.ORE;
                if (ID <= 120) return ItemType.EMBLEM | ItemType.PLANT;
                if (ID <= 130) return ItemType.EMBLEM | ItemType.MONSTER;
                if (ID <= 140) return ItemType.EMBLEM | ItemType.BUG;
                return ItemType.EMBLEM | ItemType.OTHER;
            }
            if (ID > 200 && ID < 300) return ItemType.MOD;
            if (ID >= 300 && ID < 600) return ItemType.WEAPON;
            if (ID >= 600 && ID < 700) return ItemType.OFFHAND;
            if (ID >= 700 && ID < 800) return ItemType.HELMET;
            if (ID >= 800 && ID < 900) return ItemType.ARMOR;
            if (ID >= 900 && ID < 1000) return ItemType.RING;
            if (ID >= 1000 && ID < 2000) return ItemType.DROID;
            switch (ID)
            {
                case 41:
                    return ItemType.CONSUMABLE;
                case 44:
                    return ItemType.CONSUMABLE;
                case 46:
                    return ItemType.CONSUMABLE;
                case 47:
                    return ItemType.CONSUMABLE;
                case 48:
                    return ItemType.CONSUMABLE;
                case 50:
                    return ItemType.CONSUMABLE;
                case 54:
                    return ItemType.CONSUMABLE;
                case 55:
                    return ItemType.CONSUMABLE;
                case 56:
                    return ItemType.CONSUMABLE;
                case 58:
                    return ItemType.CONSUMABLE;
                case 60:
                    return ItemType.CONSUMABLE;
                case 61:
                    return ItemType.CONSUMABLE;
                case 62:
                    return ItemType.CONSUMABLE;
                case 63:
                    return ItemType.CONSUMABLE;
                case 64:
                    return ItemType.CONSUMABLE;
                case 65:
                    return ItemType.CONSUMABLE;
                case 66:
                    return ItemType.CONSUMABLE;
                case 67:
                    return ItemType.CONSUMABLE;
                case 68:
                    return ItemType.CONSUMABLE;
                case 69:
                    return ItemType.CONSUMABLE;
                case 70:
                    return ItemType.CONSUMABLE;
                case 71:
                    return ItemType.CONSUMABLE;
                case 72:
                    return ItemType.CONSUMABLE;
                case 73:
                    return ItemType.CONSUMABLE;
                case 74:
                    return ItemType.CONSUMABLE;
                default:
                    return ItemType.GENERIC;
            }
        }

        /// <summary>
        /// Gets the default weapon scaling of the given ID, assuming it is the ID of a vanilla weapon. Does not include any special-case scaling that varies depending on external factors such as the player's current health.
        /// </summary>
        public static float[] GetDefaultWeaponScalingByID(int ID)
        {
            float[] scaling = new float[6];
            if (ID < 300 || ID >= 600) return scaling;
            else if (ID < 400) scaling[1] += 1;
            else if (ID < 500) scaling[2] += 1;
            else if (ID < 550) scaling[4] += 1;
            else if (ID < 600) scaling[5] += 0.25f;
            switch (ID)
            {
                case 314:
                    scaling[2] += 1;
                    break;
                case 316:
                    scaling[3] += 1;
                    break;
                case 319:
                    scaling[4] += 0.5f;
                    break;
                case 324:
                    scaling[3] += 0.5f;
                    break;
                case 328:
                    scaling[0] += 1;
                    break;
                case 329:
                    scaling[2] += 0.5f;
                    break;
                case 362:
                    scaling[2] += 0.5f;
                    break;
                case 363:
                    scaling[2] += 2;
                    break;
                case 364:
                    scaling[2] += 1;
                    break;
                case 367:
                    scaling[2] += 0.5f;
                    scaling[5] += 0.5f;
                    break;
                case 369:
                    scaling[4] += 0.5f;
                    break;
                case 372:
                    scaling[0] += 1f / 3f;
                    break;
                case 373:
                    scaling[0] += 0.5f;
                    break;
                case 377:
                    scaling[2] += 0.5f;
                    break;
                case 378:
                    scaling[2] += 0.5f;
                    break;
                case 412:
                    scaling[3] += 0.5f;
                    break;
                case 417:
                    scaling[3] += 1;
                    break;
                case 418:
                    scaling[4] += 0.5f;
                    break;
                case 420:
                    scaling[4] += 0.5f;
                    break;
                case 421:
                    scaling[3] += 1;
                    break;
                case 423:
                    scaling[0] += 0.5f;
                    break;
                case 425:
                    scaling[2] += 1;
                    break;
                case 426:
                    scaling[3] += 0.5f;
                    break;
                case 427:
                    scaling[5] += 0.5f;
                    break;
                case 429:
                    scaling[4] += 0.5f;
                    break;
                case 462:
                    scaling[0] += 1;
                    break;
                case 463:
                    scaling[0] += 0.5f;
                    break;
                case 464:
                    scaling[3] += 1;
                    break;
                case 465:
                    scaling[4] += 1;
                    break;
                case 466:
                    scaling[4] += 1;
                    break;
                case 467:
                    scaling[4] += 0.5f;
                    break;
                case 468:
                    scaling[0] += 1;
                    scaling[3] += 1;
                    break;
                case 469:
                    scaling[1] += 0.5f;
                    break;
                case 470:
                    scaling[1] += 1;
                    break;
                case 471:
                    scaling[3] += 0.5f;
                    scaling[0] += 0.5f;
                    break;
                case 472:
                    scaling[0] += 1;
                    scaling[4] += 1;
                    break;
                case 474:
                    scaling[3] += 0.5f;
                    break;
                case 475:
                    scaling[5] += 0.5f;
                    break;
                case 476:
                    scaling[0] += 0.5f;
                    break;
                case 477:
                    scaling[4] += 0.5f;
                    break;
                case 478:
                    scaling[4] += 0.5f;
                    break;
                case 495:
                    scaling[3] += 1;
                    break;
                case 512:
                    scaling[5] += 1;
                    break;
                case 513:
                    scaling[0] += 0.5f;
                    break;
                case 514:
                    scaling[5] += 0.5f;
                    break;
                case 516:
                    scaling[4] += 1;
                    break;
                case 517:
                    scaling[2] += 0.5f;
                    break;
                case 518:
                    scaling[5] += 0.5f;
                    break;
                case 519:
                    scaling[3] += 0.5f;
                    break;
                case 520:
                    scaling[5] += 1;
                    break;
                case 521:
                    scaling[2] += 1;
                    break;
                case 522:
                    scaling[3] += 1;
                    scaling[2] += 1;
                    break;
                case 523:
                    scaling[1] += 1;
                    break;
                case 524:
                    scaling[5] += 0.5f;
                    break;
                case 525:
                    scaling[1] += 0.5f;
                    break;
                case 526:
                    scaling[0] += 1;
                    scaling[3] += 1;
                    break;
                case 527:
                    scaling[2] += 1;
                    break;
                case 528:
                    scaling[2] += 0.5f;
                    break;
                case 529:
                    scaling[4] += 2;
                    break;
                case 549:
                    scaling[4] += 1;
                    break;
                case 562:
                    scaling[0] += 0.5f;
                    break;
                case 563:
                    scaling[4] += 0.5f;
                    break;
                case 565:
                    scaling[3] += 0.5f;
                    scaling[4] += 0.5f;
                    break;
                case 566:
                    scaling[4] += 0.5f;
                    break;
                case 567:
                    scaling[1] += 0.5f;
                    break;
                case 568:
                    scaling[0] += 0.5f;
                    break;
                case 569:
                    scaling[0] += 0.5f;
                    break;
                case 571:
                    scaling[3] += 0.5f;
                    break;
                case 572:
                    scaling[3] += 0.5f;
                    break;
                case 573:
                    scaling[0] += 1;
                    break;
                case 574:
                    scaling[4] += 0.5f;
                    break;
                case 575:
                    scaling[1] += 0.5f;
                    break;
                case 576:
                    scaling[4] += 0.25f;
                    break;
                case 577:
                    scaling[0] += 0.5f;
                    scaling[4] += 0.5f;
                    break;
                case 578:
                    scaling[4] += 0.5f;
                    break;
                case 579:
                    scaling[4] += 0.5f;
                    scaling[0] += 0.5f;
                    break;
            }
            return scaling;
        }

        /// <summary>
        /// Gets the default weapon crit chance bonus of the given ID, assuming it is the ID of a vanilla weapon. The returned value is a percentage, I.E., 5 for a 5% bonus.
        /// </summary>
        public static int GetDefaultCritChanceBonus(int ID)
        {
            switch (ID)
            {
                case 369:
                    return 10;
                case 372:
                    return 20;
                case 423:
                    return 10;
                case 475:
                    return 10;
                case 518:
                    return 25;
                default:
                    return 0;
            }
        }

        /// <summary>
        /// Gets the default weapon crit power bonus of the given ID, assuming it is the ID of a vanilla weapon. The returned value is an additive multiplier, I.E., 0.5 for a 50% bonus.
        /// </summary>
        public static float GetDefaultCritPowerBonus(int ID)
        {
            switch (ID)
            {
                case 372:
                    return 0.5f;
                case 417:
                    return 0.5f;
                case 423:
                    return 0.5f;
                case 475:
                    return 0.5f;
                default:
                    return 0;
            }
        }

        /// <summary>
        /// Gets the type of the specified ID. Applies to vanilla items as well as Gadget items.
        /// </summary>
        public static ItemType GetTypeByID(int ID)
        {
            ItemInfo itemInfo = Singleton.GetEntry(ID);
            return itemInfo != null ? itemInfo.Type : GetDefaultTypeByID(ID);
        }

        /// <summary>
        /// Gets the ID that modded IDs should start at for this registry. <see cref="ItemRegistry"/> always returns 10000.
        /// </summary>
        public override int GetIDStart()
        {
            return 10000;
        }

        /// <summary>
        /// Gets the <see cref="ItemInfo"/> for the given ID. Will return a <see cref="VanillaItemInfo"/> if the given ID is not in the registry, but is within the vanilla ID range. Otherwise, returns null.
        /// </summary>
        public static ItemInfo GetItem(int ID)
        {
            return Singleton.HasEntry(ID) ? Singleton.GetEntry(ID) : ID > 0 && ID < Singleton.GetIDStart() ? VanillaItemInfo.Wrap(ID) : null;
        }

        private static void InitializeVanillaItemIDNames()
        {
            itemIDsByName = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
            {
                ["Planet Stone"] = 1,
                ["Orichalcum"] = 2,
                ["Galacticite"] = 3,
                ["Zephyr"] = 4,
                ["Flamethyst"] = 5,
                ["Existence Gem"] = 6,
                ["Tasty Herb"] = 11,
                ["Glowshroom"] = 12,
                ["Spicy Seed"] = 13,
                ["Star Fruit"] = 14,
                ["Aether Bulb"] = 15,
                ["Chaos Leaf"] = 16,
                ["Creature Eyeball"] = 21,
                ["Monster Claw"] = 22,
                ["Chitin Fragment"] = 23,
                ["Beast Heart"] = 24,
                ["Shiny Scale"] = 25,
                ["Ectoplasm"] = 26,
                ["Flutterfly"] = 31,
                ["Dung Beetle"] = 32,
                ["Ghast Bug"] = 33,
                ["Thunderworm"] = 34,
                ["Glowfly"] = 35,
                ["Plasma Moth"] = 36,
                ["Darkfire Scroll"] = 41,
                ["Ruined Clue"] = 42,
                ["Starlight Treaty"] = 43,
                ["Arena Ticket"] = 44,
                ["Magicite"] = 45,
                ["Aethercrystal"] = 46,
                ["Platinum Badge"] = 47,
                ["Remembrance Ticket"] = 48,
                ["Champion Badge"] = 49,
                ["Glob of Aether"] = 50,
                ["World Fragment"] = 51,
                ["Credit"] = 52,
                ["Ashen Dust"] = 53,
                ["Mystery Gift"] = 54,
                ["Ion Ticket"] = 55,
                ["Lightsworn Crystal"] = 56,
                ["Scrap Metal"] = 57,
                ["Quest Prize"] = 58,
                ["Wealth Trophy"] = 59,
                ["Health Pack I"] = 60,
                ["Mana Pack I"] = 61,
                ["Energy Pack I"] = 62,
                ["Nuldmg Potion"] = 63,
                ["Anti-Poison"] = 64,
                ["Health Pack II"] = 65,
                ["Mana Pack II"] = 66,
                ["Energy Pack II"] = 67,
                ["Anti-Frost"] = 68,
                ["Anti-Heat"] = 69,
                ["Health Pack III"] = 70,
                ["Mana Pack III"] = 71,
                ["Energy Pack III"] = 72,
                ["Elixir"] = 73,
                ["Aetherdew"] = 74,
                ["Aetherlite Shard"] = 86,
                ["Darkened Shard"] = 87,
                ["Omega Shard"] = 88,
                ["Aetherlite Prism"] = 89,
                ["Darkened Prism"] = 90,
                ["Omega Prism"] = 91,
                ["Planet Emblem"] = 101,
                ["Orichalcum Emblem"] = 102,
                ["Galacticite Emblem"] = 103,
                ["Zephyr Emblem"] = 104,
                ["Flame Emblem"] = 105,
                ["Existence Emblem"] = 106,
                ["Herb Emblem"] = 111,
                ["Shroom Emblem"] = 112,
                ["Seed Emblem"] = 113,
                ["Star Emblem"] = 114,
                ["Aether Emblem"] = 115,
                ["Chaos Emblem"] = 116,
                ["Eyeball Emblem"] = 121,
                ["Claw Emblem"] = 122,
                ["Fragment Emblem"] = 123,
                ["Beast Emblem"] = 124,
                ["Shiny Emblem"] = 125,
                ["Ectoplasm Emblem"] = 126,
                ["Flutterfly Emblem"] = 131,
                ["Beetle Emblem"] = 132,
                ["Ghast Emblem"] = 133,
                ["Thunderworm Emblem"] = 134,
                ["Glowfly Emblem"] = 135,
                ["Plasma Emblem"] = 136,
                ["BonusVIT+"] = 201,
                ["BonusSTR+"] = 202,
                ["BonusDEX+"] = 203,
                ["BonusTEC+"] = 204,
                ["BonusMAG+"] = 205,
                ["BonusFTH+"] = 206,
                ["ResistHeat+"] = 207,
                ["ResistFrost+"] = 208,
                ["ResistPoison+"] = 209,
                ["ProjectileRange+"] = 210,
                ["CritRate+"] = 211,
                ["CritDmg+"] = 212,
                ["HealthRegen+"] = 213,
                ["ManaRegen+"] = 214,
                ["StaminaRegen+"] = 215,
                ["MoveSpeed+"] = 216,
                ["DashSpeed+"] = 217,
                ["JumpHeight+"] = 218,
                ["OreHarvest+"] = 219,
                ["PlantHarvest+"] = 220,
                ["MonsterDrops+"] = 221,
                ["BugHarvest+"] = 222,
                ["ExpBoost+"] = 223,
                ["CreditBoost+"] = 224,
                ["Aetherblade"] = 300,
                ["Bolt Edge"] = 301,
                ["Colossus"] = 302,
                ["Doomguard"] = 303,
                ["Gadget Saber"] = 304,
                ["Ragnarok"] = 305,
                ["Magemasher"] = 312,
                ["Fractured Soul"] = 313,
                ["Arcwind"] = 314,
                ["Trigoddess"] = 315,
                ["Flamberge"] = 316,
                ["Key of Hearts"] = 317,
                ["Excalibur"] = 318,
                ["Zweihander"] = 319,
                ["Heaven's Cloud"] = 320,
                ["Death Exalted"] = 321,
                ["Dark Messenger"] = 322,
                ["Ruin"] = 323,
                ["Claymore"] = 324,
                ["Forgeblade"] = 325,
                ["Lost Voice"] = 326,
                ["Valflame"] = 327,
                ["Helswath"] = 328,
                ["Awakened Force"] = 329,
                ["Azazel's Blade"] = 345,
                ["Ringabolt's Axe"] = 346,
                ["Caius' Demonblade"] = 347,
                ["Glitterblade"] = 348,
                ["4th Age Sword"] = 349,
                ["Aetherlance"] = 350,
                ["Runelance"] = 351,
                ["The Highwind"] = 352,
                ["Gallatria's Spire"] = 353,
                ["Abraxas"] = 354,
                ["Cain's Lance"] = 355,
                ["Galaxy Lance"] = 362,
                ["World's End"] = 363,
                ["Emblem Fates"] = 364,
                ["Doombringer"] = 365,
                ["Darkforce"] = 366,
                ["World Reborn"] = 367,
                ["King's Lance"] = 368,
                ["Gungnir"] = 369,
                ["Spirit Lance"] = 370,
                ["Firestorm"] = 371,
                ["Heartseeker"] = 372,
                ["Longinus"] = 373,
                ["Stormbringer"] = 374,
                ["Devilsbane"] = 375,
                ["Rampart Golem"] = 376,
                ["Dragon Whisker"] = 377,
                ["Vengeful Spirit"] = 378,
                ["Dragoon Lance"] = 379,
                ["Wallace's Lance"] = 397,
                ["Urugorak's Tooth"] = 398,
                ["4th Age Lance"] = 399,
                ["Aethergun Mk.IV"] = 400,
                ["Arcfire"] = 401,
                ["Frost"] = 402,
                ["Vengeance"] = 403,
                ["Judgement"] = 404,
                ["Golden Eye"] = 405,
                ["Thrasher"] = 406,
                ["Athena XVI"] = 407,
                ["Star Destroyer"] = 412,
                ["Quicksilver"] = 413,
                ["Repeater"] = 414,
                ["Magma"] = 415,
                ["Chaingun"] = 416,
                ["Oblivion"] = 417,
                ["Avalanche"] = 418,
                ["Atma Weapon"] = 419,
                ["Coldsnap"] = 420,
                ["Sirius"] = 421,
                ["Sonicsteel"] = 422,
                ["Death Penalty"] = 423,
                ["Fomalhaut"] = 424,
                ["Cataclysm"] = 425,
                ["Betrayer"] = 426,
                ["Golden Suns"] = 427,
                ["Cerberus"] = 428,
                ["Poopmaker"] = 429,
                ["Axelark's Blaster"] = 447,
                ["Plaguesteel"] = 448,
                ["4th Age Gun"] = 449,
                ["Aethercannon"] = 450,
                ["Typhoon IX"] = 451,
                ["Auto Rifle"] = 452,
                ["Viking XXVII"] = 453,
                ["Bazooka"] = 454,
                ["The Wyvern"] = 455,
                ["Aqualung"] = 456,
                ["Heat Cannon"] = 457,
                ["Commando"] = 462,
                ["Hypercannon"] = 463,
                ["Missile RPG"] = 464,
                ["Vorpal RPG"] = 465,
                ["The Dominator"] = 466,
                ["The Zapper"] = 467,
                ["Stormcannon"] = 468,
                ["The Machine"] = 469,
                ["Volt Sniper"] = 470,
                ["War-Forged Gun"] = 471,
                ["Carbine"] = 472,
                ["Gadget RPG"] = 473,
                ["Tropic Thunder"] = 474,
                ["Hand Cannon"] = 475,
                ["Flak Cannon"] = 476,
                ["Dragon Cannon"] = 477,
                ["Flame Swathe"] = 478,
                ["Wyvern Bone"] = 479,
                ["Scheg's Bow"] = 495,
                ["MEGA WEAPON"] = 496,
                ["GALACTIC FLAMEBLASTER"] = 497,
                ["Pirate Musket"] = 498,
                ["4th Age Cannon"] = 499,
                ["Mage Gauntlet"] = 500,
                ["Soul Reaver"] = 501,
                ["Flame Lash"] = 502,
                ["Wolt's Thunder"] = 503,
                ["Gaia's Gale"] = 504,
                ["Decimator"] = 505,
                ["Dargon Idol"] = 506,
                ["Red Lightning"] = 507,
                ["Mystic Arrow"] = 512,
                ["Elementalizer"] = 513,
                ["Flareblade"] = 514,
                ["Ice Wall"] = 515,
                ["Wrath Aura"] = 516,
                ["Nether Torrent"] = 517,
                ["Bolganone"] = 518,
                ["Caius' Pyre"] = 519,
                ["Elfire"] = 520,
                ["Gafgard's Maelstrom"] = 521,
                ["Maalurk Totem"] = 522,
                ["Monk Gauntlet"] = 523,
                ["Tornado"] = 524,
                ["Airsplitter"] = 525,
                ["Gruu's Talisman"] = 526,
                ["Destruction Wave"] = 527,
                ["Annihilation"] = 528,
                ["Banana"] = 529,
                ["Baalfog's Avalanche"] = 546,
                ["Moloch's Wrath"] = 547,
                ["Shroomhazzard"] = 548,
                ["4th Age Gauntlet"] = 549,
                ["Aetherstaff"] = 550,
                ["Pyroclasm"] = 551,
                ["Astra"] = 552,
                ["Thornwall"] = 553,
                ["Seraphim"] = 554,
                ["Nirvana"] = 555,
                ["Twilight Staff"] = 562,
                ["Enigma"] = 563,
                ["Summoner's Staff"] = 564,
                ["Armageddon"] = 565,
                ["Doomsayer"] = 566,
                ["Merciless Gladiator"] = 567,
                ["Bubblegum Staff"] = 568,
                ["Cherry Blossom"] = 569,
                ["The Whitemage"] = 570,
                ["Vinewhip"] = 571,
                ["Jungle King"] = 572,
                ["Sage's Staff"] = 573,
                ["Lightning Rod"] = 574,
                ["Caster Sword"] = 575,
                ["Seeker of Stars"] = 576,
                ["Maelstrom"] = 577,
                ["Darkness"] = 578,
                ["The Blackmage"] = 579,
                ["Dragonbolt's Mast"] = 595,
                ["Plain Stick"] = 596,
                ["Perceval's Wand"] = 597,
                ["Hivemind Rod"] = 598,
                ["4th Age Staff"] = 599,
                ["Aethershield"] = 600,
                ["Cadet Buckler"] = 601,
                ["Aegis"] = 602,
                ["Bolt Shield"] = 603,
                ["Gallatria Sigil"] = 604,
                ["Force Guard"] = 605,
                ["Eagle Shield"] = 612,
                ["Leader's Crest"] = 613,
                ["Twilight Shield"] = 614,
                ["Arc's Buckler"] = 615,
                ["Soul Infusion"] = 616,
                ["Supernova"] = 617,
                ["Purifier"] = 618,
                ["Oathkeeper"] = 619,
                ["Tower Aegis"] = 620,
                ["King's Crest"] = 621,
                ["Champion Shield"] = 622,
                ["Spiked Lightning"] = 623,
                ["Blood Shield"] = 624,
                ["Heater Shield"] = 625,
                ["Fungi Shield"] = 626,
                ["Sunlight"] = 627,
                ["Blood Shield"] = 628,
                ["Peacekeeper"] = 629,
                ["Aether Shield"] = 646,
                ["4th Age Shield"] = 647,
                ["Scarab Shell"] = 648,
                ["Recruit Helm"] = 700,
                ["Dunerider Hood"] = 701,
                ["Nautilus Helm"] = 702,
                ["Vorpal Hood"] = 703,
                ["Titan Helm"] = 704,
                ["Isaac Helm"] = 705,
                ["Ultrom Helm"] = 706,
                ["Brute Helm"] = 707,
                ["Yoshimitsu Helm"] = 708,
                ["Ghost Helm"] = 709,
                ["Vigilante Helm"] = 710,
                ["Wraith Helm"] = 711,
                ["4th Age Helm [STR]"] = 712,
                ["4th Age Helm [DEX]"] = 713,
                ["4th Age Helm [MAG]"] = 714,
                ["Captain's Hat"] = 725,
                ["Urugorak's Hat"] = 726,
                ["Bolgon's Helm"] = 730,
                ["Broccoli Helm"] = 731,
                ["Dredger Helm"] = 732,
                ["Overworld Helm"] = 733,
                ["Scourge Helm"] = 734,
                ["Wallace's Helm"] = 735,
                ["Gromwell'S Helm"] = 736,
                ["Ringabolt's Helm"] = 737,
                ["Perceval's Helm"] = 738,
                ["Baalfog's Eye"] = 739,
                ["Azazel's Helm"] = 740,
                ["Axelark's Helm"] = 741,
                ["Queen's Helm"] = 742,
                ["Nolic Beats"] = 743,
                ["Elite Helm"] = 750,
                ["Voyager Helm"] = 751,
                ["Siege Helm"] = 752,
                ["Krabshell Helm"] = 753,
                ["Dunecloth Helm"] = 754,
                ["Drifter Helm"] = 755,
                ["Leviathan Helm"] = 756,
                ["Kraken Helm"] = 757,
                ["Chaos Helm"] = 758,
                ["Ultima Helm"] = 759,
                ["Destruction Helm"] = 760,
                ["Ithaca's Helm"] = 761,
                ["Champion Helm"] = 762,
                ["Heroic Helm"] = 763,
                ["Deathgod Helm"] = 764,
                ["Shatterspell Helm"] = 765,
                ["Towermage Helm"] = 766,
                ["Deus Helm"] = 767,
                ["Plasma Helm"] = 768,
                ["Rapture Helm"] = 769,
                ["Firegod Helm"] = 770,
                ["Bruiser Helm"] = 771,
                ["Inferno Helm"] = 772,
                ["Ironforge Helm"] = 773,
                ["Yojimbo Helm"] = 774,
                ["Oni Helm"] = 775,
                ["Aku Helm"] = 776,
                ["Recon Helm"] = 777,
                ["Force Helm"] = 778,
                ["Helloworld Helm"] = 779,
                ["Darknight Helm"] = 780,
                ["Onslaught Helm"] = 781,
                ["Whitewhorl Helm"] = 782,
                ["Maelstrom Helm"] = 783,
                ["Ruin Helm"] = 784,
                ["Pyroclasm Helm"] = 785,
                ["Aether Helm"] = 786,
                ["Slimecraft Helm"] = 787,
                ["Mykonogre H"] = 790,
                ["Catastrophia H"] = 791,
                ["Fellbug H"] = 792,
                ["Might Shroom H"] = 793,
                ["Ironclad H"] = 794,
                ["Apocalypse H"] = 795,
                ["Glaedria H"] = 796,
                ["Exodus H"] = 797,
                ["Kawaii Lemon"] = 798,
                ["Old Chap's Hat"] = 799,
                ["Recruit Armor"] = 800,
                ["Dunerider Armor"] = 801,
                ["Nautilus Armor"] = 802,
                ["Vorpal Armor"] = 803,
                ["Titan Armor"] = 804,
                ["Isaac Armor"] = 805,
                ["Ultrom Armor"] = 806,
                ["Brute Armor"] = 807,
                ["Yoshimitsu Armor"] = 808,
                ["Ghost Armor"] = 809,
                ["Vigilante Armor"] = 810,
                ["Wraith Armor"] = 811,
                ["4th Age Armor"] = 812,
                ["Bolgon's Armor"] = 830,
                ["Broccoli Armor"] = 831,
                ["Dredger Armor"] = 832,
                ["Baalfog's Suit"] = 839,
                ["Azazel's Armor"] = 840,
                ["Axelark's Armor"] = 841,
                ["Queen's Armor"] = 842,
                ["Elite Armor"] = 850,
                ["Voyager Armor"] = 851,
                ["Siege Armor"] = 852,
                ["Krabshell Armor"] = 853,
                ["Dunecloth Armor"] = 854,
                ["Drifter Armor"] = 855,
                ["Leviathan Armor"] = 856,
                ["Kraken Armor"] = 857,
                ["Chaos Armor"] = 858,
                ["Ultima Armor"] = 859,
                ["Destruction Armor"] = 860,
                ["Ithaca's Armor"] = 861,
                ["Champion Armor"] = 862,
                ["Heroic Armor"] = 863,
                ["Deathgod Armor"] = 864,
                ["Shatterspell Armor"] = 865,
                ["Towermage Armor"] = 866,
                ["Deus Armor"] = 867,
                ["Plasma Armor"] = 868,
                ["Rapture Armor"] = 869,
                ["Firegod Armor"] = 870,
                ["Bruiser Armor"] = 871,
                ["Inferno Armor"] = 872,
                ["Ironforge Armor"] = 873,
                ["Yojimbo Armor"] = 874,
                ["Oni Armor"] = 875,
                ["Aku Armor"] = 876,
                ["Recon Armor"] = 877,
                ["Force Armor"] = 878,
                ["Helloworld Armor"] = 879,
                ["Darknight Armor"] = 880,
                ["Onslaught Armor"] = 881,
                ["Whitewhorl Armor"] = 882,
                ["Maelstrom Armor"] = 883,
                ["Ruin Armor"] = 884,
                ["Pyroclasm Armor"] = 885,
                ["Aether Armor"] = 886,
                ["Mykonogre A"] = 890,
                ["Catastrophia A"] = 891,
                ["Fellbug A"] = 892,
                ["Might Shroom A"] = 893,
                ["Ironclad A"] = 894,
                ["Apocalypse A"] = 895,
                ["Glaedria A"] = 896,
                ["Exodus A"] = 897,
                ["Gallahad Ring"] = 900,
                ["Ezerius Ring"] = 901,
                ["Anelice Ring"] = 902,
                ["Gromwell Ring"] = 903,
                ["Brym Ring"] = 904,
                ["Falstadt Ring"] = 905,
                ["Roehn Ring"] = 906,
                ["Perceval Ring"] = 907,
                ["Owain Ring"] = 908,
                ["Tydus Ring"] = 909,
                ["Vaati Ring"] = 910,
                ["RCK 22"] = 1000,
                ["FLWR 08"] = 1001,
                ["BAT 17"] = 1002,
                ["OBSIDIAN 64"] = 1003,
                ["HELPR 55"] = 1004,
                ["GUARDIAN 07"] = 1005,
                ["SOLAR 05"] = 1012,
                ["PRISM 88"] = 1013,
                ["MONOLTH 25"] = 1014,
                ["FARMHAND 78"] = 1015,
                ["BULB 88"] = 1016,
                ["BTTRFLY 8"] = 1017,
                ["DRAGON 67"] = 1018,
                ["WYVRN 77"] = 1019,
                ["MEGAZORD 36"] = 1020,
                ["STEEL 65"] = 1021,
                ["DIAMND 66"] = 1022,
                ["BOGBOT 67"] = 1023,
                ["AIDBOT 56"] = 1024,
                ["HELLBOT 57"] = 1025,
                ["iBOT 58"] = 1026,
                ["WHITEMAG 09"] = 1027,
                ["OVERSEER 06"] = 1028,
                ["MRGRGRR 05"] = 1029,
                ["GOLD 15"] = 1030,
                ["Scrap Metal Block"] = 2000,
                ["Glass Block"] = 2001,
                ["Firesteel Block"] = 2002,
                ["Storage Block"] = 2100,
                ["Forge Block"] = 2101,
                ["Emblem Block"] = 2102,
                ["Combat Block"] = 2103,
                ["Alchemy Block"] = 2104,
                ["Computer Block"] = 2105,
                ["Portal Block"] = 2106,
                ["Ship Droid Block"] = 2107,
                ["Door Block"] = 2108,
                ["Scrap Metal Wall"] = 2200,
                ["Scrap Metal Platform"] = 2300,
                ["Engine Block"] = 2400,
                ["Blue Light"] = 2401,
                ["Red Light"] = 2402,
                ["Spawn Location"] = 2403,
                ["Shmoo Card"] = 2501,
                ["Eyepod Card"] = 2502,
                ["Dunebug Card"] = 2503,
                ["Worm Card"] = 2504,
                ["Wasp Card"] = 2505,
                ["Urugorak Card"] = 2506,
                ["Sluglord Card"] = 2507,
                ["Slugmother Card"] = 2508,
                ["Chamcham Card"] = 2509,
                ["Rhinobug Card"] = 2510,
                ["Hivemind Card"] = 2511,
                ["Glibglob Card"] = 2512,
                ["Slime Card"] = 2513,
                ["Rock Spider Card"] = 2514,
                ["Sploopy Card"] = 2515,
                ["Rock Scarab Card"] = 2516,
                ["Shroom Card"] = 2517,
                ["Blue Shroom Card"] = 2518,
                ["Shroom Bully Card"] = 2519,
                ["Relicfish Card"] = 2520,
                ["Ancient Guard Card"] = 2521,
                ["Ancient Beast Card"] = 2522,
                ["Roach Card"] = 2523,
                [" Card"] = 2524,
                ["Squirm Card"] = 2525,
                ["Plague Caster Card"] = 2526,
                ["Glitterbug Card"] = 2527,
                ["Plaguebeast Card"] = 2528,
                ["Space Pirate Card"] = 2529,
                ["Wicked Card"] = 2530,
                ["Wisp Card"] = 2531,
                ["Yeti Card"] = 2532,
                ["Mammoth Card"] = 2533,
                ["Wyvern Card"] = 2534,
                ["Lava Blob Card"] = 2535,
                ["Fire Slime Card"] = 2536,
                ["Lava Dragon Card"] = 2537,
                ["Tyrannog Card"] = 2538,
                ["Beelzeblob Card"] = 2539,
                ["Gruu Card"] = 2540,
                ["Treant Card"] = 2541,
                ["Willowwart Card"] = 2542,
                ["Caius Card"] = 2543,
                ["Moloch Card"] = 2544,
                ["Brave Badge"] = 2600,
                ["Magicite Badge"] = 2601,
                ["Destroyer Badge"] = 2602,
                ["Faust Badge"] = 2603,
                ["Creator Badge"] = 2604,
                ["Machine Badge"] = 2605,
                ["Rebellion Badge"] = 2606,
                ["Starlight Badge"] = 2607,
                ["Justice Badge"] = 2608,
                ["Enigma Badge"] = 2609,
                ["Darkweapon Badge"] = 2610,
                ["Zeig Badge"] = 2611
            };
            itemIDsByRegistryName = new Dictionary<string, int>(itemIDsByName.Comparer);
            foreach (KeyValuePair<string, int> item in itemIDsByName)
            {
                itemIDsByRegistryName["Roguelands:" + item.Key] = item.Value;
            }
        }
    }

    /// <summary>
    /// Specifies what type of item this is. These represent a set of flags, so they can be combined and masked using the | and &amp; operators respectively. I.E: 'ItemType.LOOT | ItemType.TIER1 | ItemType.ROCK'. Please note that EQUIPABLE is incompatible with LOOT, EMBLEM, USABLE, and CONSUMABLE, and attempts to combine them will result in unexpected results. Also note that the top 16 bits are flags for general use, and will be ignored by Gadget Core. They can be set using FLAG1-FLAG16
    /// </summary>
    [Flags]
    public enum ItemType : uint
    {
        /// <summary>
        /// There is absolutely nothing special about this item. It can't be used or equipped, and it isn't any sort of loot or other tiered item.
        /// </summary>
        GENERIC     = 0b0000000000000000,
        /// <summary>
        /// This item is loot, as is used in the emblem forge and the alchemy table. Note that a tier and loot type should also be set. Incompatible with EQUIPABLE.
        /// </summary>
        LOOT        = 0b0000000000000010,
        /// <summary>
        /// This item is an emblem, as is used in the gear forge and creation machine. Note that a tier and loot type should also be set. Incompatible with EQUIPABLE.
        /// </summary>
        EMBLEM      = 0b0000000000000011,
        /// <summary>
        /// This item can be used from the hotbar, and doing so will invoke OnUse. Incompatible with EQUIPABLE.
        /// </summary>
        USABLE      = 0b0000000000000100,
        /// <summary>
        /// When this item is used, one of it should be consumed. Implies USABLE.
        /// </summary>
        CONSUMABLE  = 0b0000000000001100,

        /// <summary>
        /// This item can be equipped. Note that this alone does not actually make it equipable, since no equip slot will take it. Incompatible with LOOT, EMBLEM, USABLE, and CONSUMABLE.
        /// </summary>
        EQUIPABLE   = 0b0000000000010000,
        /// <summary>
        /// This item can stack. Note that this flag is meaningless to set, and is only intended to be used for querying as such: ({Item Type} &amp; ItemType.NONSTACKING) == ItemType.STACKING
        /// </summary>
        STACKING    = 0b0000000000000000,
        /// <summary>
        /// This item cannot stack. If an equipable item does not have this flag set, unexpected behavior may occur.
        /// </summary>
        NONSTACKING = 0b0000000000100000,
        /// <summary>
        /// This item is able to level up. Also causes the item to show the background that displays the item's rarity tier. If a leveling item is able to stack, unexpected behavior may occur.
        /// </summary>
        LEVELING    = 0b0000000001000000,
        /// <summary>
        /// This item is able to have mods installed into it. Note that this is non-functional in this version of Gadget Core.
        /// </summary>
        MODABLE     = 0b0000000010100000,
        /// <summary>
        /// This item is a gear mod, and as such can be installed into weapons, offhands, helmets, armors, and rings. Note that this is non-functional in this version of Gadget Core.
        /// </summary>
        MOD         = 0b0000000010000000,
        /// <summary>
        /// This item is a weapon, and can as such can be equipped to the weapon slot. Implies EQUIPABLE, NONSTACKING, LEVELING, and MODABLE. Use BASIC_MASK to strip all but the EQUIPABLE implication.
        /// </summary>
        WEAPON      = 0b0000000011110000,
        /// <summary>
        /// This item is an offhand, and can as such can be equipped to the offhand slot. Implies EQUIPABLE, NONSTACKING, LEVELING, and MODABLE. Use BASIC_MASK to strip all but the EQUIPABLE implication.
        /// </summary>
        OFFHAND     = 0b0000000011110001,
        /// <summary>
        /// This item is a helmet, and can as such can be equipped to the helmet slot. Implies EQUIPABLE, NONSTACKING, LEVELING, and MODABLE. Use BASIC_MASK to strip all but the EQUIPABLE implication.
        /// </summary>
        HELMET      = 0b0000000011110010,
        /// <summary>
        /// This item is an armor, and can as such can be equipped to the armor slot. Implies EQUIPABLE, NONSTACKING, LEVELING, and MODABLE. Use BASIC_MASK to strip all but the EQUIPABLE implication.
        /// </summary>
        ARMOR       = 0b0000000011110011,
        /// <summary>
        /// This item is a ring, and can as such can be equipped to a ring slot. Implies EQUIPABLE, NONSTACKING, LEVELING, and MODABLE. Use BASIC_MASK to strip all but the EQUIPABLE implication.
        /// </summary>
        RING        = 0b0000000011110100,
        /// <summary>
        /// This item is a droid, and can as such can be equipped to a droid slot. Implies EQUIPABLE, NONSTACKING, and LEVELING. Use BASIC_MASK to strip all but the EQUIPABLE implication.
        /// </summary>
        DROID       = 0b0000000001110101,

        /// <summary>
        /// This item is tier 1. This is meaningless if the item is not either a LOOT or an EMBLEM.
        /// </summary>
        TIER1       = 0b0000000100000000,
        /// <summary>
        /// This item is tier 2. This is meaningless if the item is not either a LOOT or an EMBLEM.
        /// </summary>
        TIER2       = 0b0000001000000000,
        /// <summary>
        /// This item is tier 3. This is meaningless if the item is not either a LOOT or an EMBLEM.
        /// </summary>
        TIER3       = 0b0000001100000000,
        /// <summary>
        /// This item is tier 4. This is meaningless if the item is not either a LOOT or an EMBLEM.
        /// </summary>
        TIER4       = 0b0000010000000000,
        /// <summary>
        /// This item is tier 5. This is meaningless if the item is not either a LOOT or an EMBLEM.
        /// </summary>
        TIER5       = 0b0000010100000000,
        /// <summary>
        /// This item is tier 6. This is meaningless if the item is not either a LOOT or an EMBLEM.
        /// </summary>
        TIER6       = 0b0000011000000000,
        /// <summary>
        /// This item is tier 7. This is meaningless if the item is not either a LOOT or an EMBLEM.
        /// </summary>
        TIER7       = 0b0000011100000000,
        /// <summary>
        /// This item is tier 8. This is meaningless if the item is not either a LOOT or an EMBLEM.
        /// </summary>
        TIER8       = 0b0000100000000000,
        /// <summary>
        /// This item is tier 9. This is meaningless if the item is not either a LOOT or an EMBLEM.
        /// </summary>
        TIER9       = 0b0000100100000000,
        /// <summary>
        /// This item is tier 10. This is meaningless if the item is not either a LOOT or an EMBLEM.
        /// </summary>
        TIER10      = 0b0000000000000000,

        /// <summary>
        /// This item is a rock material. This is meaningless if the item is not either a LOOT or an EMBLEM.
        /// </summary>
        ORE        = 0b0000000000000000,
        /// <summary>
        /// This item is a plant material. This is meaningless if the item is not either a LOOT or an EMBLEM. Implies ORGANIC.
        /// </summary>
        PLANT       = 0b0101000000000000,
        /// <summary>
        /// This item is a monster material. This is meaningless if the item is not either a LOOT or an EMBLEM. Implies ORGANIC.
        /// </summary>
        MONSTER     = 0b0110000000000000,
        /// <summary>
        /// This item is a bug material. This is meaningless if the item is not either a LOOT or an EMBLEM. Implies ORGANIC.
        /// </summary>
        BUG         = 0b0111000000000000,
        /// <summary>
        /// This item is some other form of material besides the standard four. This is meaningless if the item is not either a LOOT or an EMBLEM.
        /// </summary>
        OTHER       = 0b1000000000000000,

        /// <summary>
        /// This item is considered to be an organic material, and as such can be placed into the alchemy station if it is also a LOOT.
        /// </summary>
        ORGANIC     = 0b0100000000000000,

        /// <summary>
        /// Generic flag 1. Gadget Core ignores this, so it can be used for whatever you wish.
        /// </summary>
        FLAG1       = 0b00000000000000010000000000000000,
        /// <summary>
        /// Generic flag 2. Gadget Core ignores this, so it can be used for whatever you wish.
        /// </summary>
        FLAG2       = 0b00000000000000100000000000000000,
        /// <summary>
        /// Generic flag 3. Gadget Core ignores this, so it can be used for whatever you wish.
        /// </summary>
        FLAG3       = 0b00000000000001000000000000000000,
        /// <summary>
        /// Generic flag 4. Gadget Core ignores this, so it can be used for whatever you wish.
        /// </summary>
        FLAG4       = 0b00000000000010000000000000000000,
        /// <summary>
        /// Generic flag 5. Gadget Core ignores this, so it can be used for whatever you wish.
        /// </summary>
        FLAG5       = 0b00000000000100000000000000000000,
        /// <summary>
        /// Generic flag 6. Gadget Core ignores this, so it can be used for whatever you wish.
        /// </summary>
        FLAG6       = 0b00000000001000000000000000000000,
        /// <summary>
        /// Generic flag 7. Gadget Core ignores this, so it can be used for whatever you wish.
        /// </summary>
        FLAG7       = 0b00000000010000000000000000000000,
        /// <summary>
        /// Generic flag 8. Gadget Core ignores this, so it can be used for whatever you wish.
        /// </summary>
        FLAG8       = 0b00000000100000000000000000000000,
        /// <summary>
        /// Generic flag 9. Gadget Core ignores this, so it can be used for whatever you wish.
        /// </summary>
        FLAG9       = 0b00000001000000000000000000000000,
        /// <summary>
        /// Generic flag 10. Gadget Core ignores this, so it can be used for whatever you wish.
        /// </summary>
        FLAG10      = 0b00000010000000000000000000000000,
        /// <summary>
        /// Generic flag 11. Gadget Core ignores this, so it can be used for whatever you wish.
        /// </summary>
        FLAG11      = 0b00000100000000000000000000000000,
        /// <summary>
        /// Generic flag 12. Gadget Core ignores this, so it can be used for whatever you wish.
        /// </summary>
        FLAG12      = 0b00001000000000000000000000000000,
        /// <summary>
        /// Generic flag 13. Gadget Core ignores this, so it can be used for whatever you wish.
        /// </summary>
        FLAG13      = 0b00010000000000000000000000000000,
        /// <summary>
        /// Generic flag 14. Gadget Core ignores this, so it can be used for whatever you wish.
        /// </summary>
        FLAG14      = 0b00100000000000000000000000000000,
        /// <summary>
        /// Generic flag 15. Gadget Core ignores this, so it can be used for whatever you wish.
        /// </summary>
        FLAG15      = 0b01000000000000000000000000000000,
        /// <summary>
        /// Generic flag 16. Gadget Core ignores this, so it can be used for whatever you wish.
        /// </summary>
        FLAG16      = 0b10000000000000000000000000000000,

        /// <summary>
        /// A bitmask that filters out the LOOT, EMBLEM, USABLE, CONSUMABLE flags.
        /// </summary>
        BASIC_MASK  = 0b0000000000001111,
        /// <summary>
        /// A bitmask that filters out the LOOT, EMBLEM, USABLE, CONSUMABLE, and EQUIPABLE flags.
        /// </summary>
        EQUIP_MASK  = 0b0000000000011111,
        /// <summary>
        /// A bitmask that filters out the EQUIPABLE, NONSTACKING, LEVELING, MODABLE, and MOD flags.
        /// </summary>
        TYPE_MASK   = 0b0000000011110000,
        /// <summary>
        /// A bitmask that filters out the TIER* flags.
        /// </summary>
        TIER_MASK   = 0b0000111100000000,
        /// <summary>
        /// A bitmask that filters out the ROCK, PLANT, MONSTER, BUG, OTHER, and ORGANIC flags.
        /// </summary>
        LOOT_MASK   = 0b1111000000000000,
        /// <summary>
        /// A bitmask that filters out the FLAG* generic flags.
        /// </summary>
        FLAGS_MASK  = 0b11111111111111110000000000000000
    }
}
