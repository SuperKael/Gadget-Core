using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace GadgetCore.API
{
    public static class GadgetCoreAPI
    {
        public const string VERSION = "1.0.0.0";

        internal static Dictionary<string, UnityEngine.Object> resources = new Dictionary<string, UnityEngine.Object>();
        internal static List<SpriteSheetEntry> spriteSheetSprites = new List<SpriteSheetEntry>();
        internal static int spriteSheetSize = -1;
        internal static Texture2D spriteSheet;

        /// <summary>
        /// Use to spawn an item into the game world.
        /// You may notice that the vanilla game's source-code uses Resources.Load to spawn items. Do not use that.
        /// </summary>
        /// <param name="pos">The position to spawn the item at. Note that despite being a 2D game, Roguelands uses 3D space. That being said, the z-coordinate should nearly always be 0.</param>
        /// <param name="item">The item to spawn.</param>
        /// <param name="isChip">True to drop a chip instead of a normal item.</param>
        public static ItemScript SpawnItem(Vector3 pos, Item item, bool isChip = false)
        {
            if (!isChip)
            {
                int[] st = new int[]
                {
                    item.id,
                    item.q,
                    item.exp,
                    item.tier,
                    item.corrupted,
                    item.aspect[0],
                    item.aspect[1],
                    item.aspect[2],
                    item.aspectLvl[0],
                    item.aspectLvl[1],
                    item.aspectLvl[2]
                };
                ItemScript itemScript = ((GameObject)Network.Instantiate(Resources.Load("i"), pos, Quaternion.identity, 0)).GetComponent<ItemScript>();
                itemScript.SendMessage("Init", st);
                if (ItemRegistry.GetSingleton().HasEntry(item.id) && (ItemRegistry.GetSingleton().GetEntry(item.id).Type & ItemType.EQUIPMENT_F) == ItemType.EQUIPMENT_F) itemScript.back.SetActive(true);
                return itemScript;
            }
            else
            {
                ItemScript itemScript = ((GameObject)Network.Instantiate(Resources.Load("i"), pos, Quaternion.identity, 0)).GetComponent<ItemScript>();
                itemScript.SendMessage("Chip", item.id);
                return itemScript;
            }
        }

        /// <summary>
        /// Use to spawn an item into the local player's world.
        /// You may notice that the vanilla game's source-code uses Resources.Load to spawn items. Do not use that.
        /// </summary>
        /// <param name="pos">The position to spawn the item at. Note that despite being a 2D game, Roguelands uses 3D space. That being said, the z-coordinate should nearly always be 0.</param>
        /// <param name="item">The item to spawn.</param>
        /// <param name="isChip">True to drop a chip instead of a normal item.</param>
        public static ItemScript SpawnItemLocal(Vector3 pos, Item item, bool isChip = false)
        {
            if (!isChip)
            {
                int[] st = new int[]
                {
                    item.id,
                    item.q,
                    item.exp,
                    item.tier,
                    item.corrupted,
                    item.aspect[0],
                    item.aspect[1],
                    item.aspect[2],
                    item.aspectLvl[0],
                    item.aspectLvl[1],
                    item.aspectLvl[2]
                };
                ItemScript itemScript = ((GameObject)UnityEngine.Object.Instantiate(Resources.Load("i3"), pos, Quaternion.identity)).GetComponent<ItemScript>();
                itemScript.SendMessage("InitL", st);
                if (ItemRegistry.GetSingleton().HasEntry(item.id) && (ItemRegistry.GetSingleton().GetEntry(item.id).Type & ItemType.EQUIPMENT_F) == ItemType.EQUIPMENT_F) itemScript.back.SetActive(true);
                return itemScript;
            }
            else
            {
                ItemScript itemScript = ((GameObject)UnityEngine.Object.Instantiate(Resources.Load("i3"), pos, Quaternion.identity)).GetComponent<ItemScript>();
                itemScript.SendMessage("ChipL", item.id);
                return itemScript;
            }
        }

        /// <summary>
        /// Use to spawn exp into the world.
        /// You may notice that the vanilla game's source-code uses Resources.Load to spawn exp. Do not use that.
        /// </summary>
        /// <param name="pos">The position to spawn the item at. Note that despite being a 2D game, Roguelands uses 3D space. That being said, the z-coordinate should nearly always be 0.</param>
        /// <param name="exp">The amount of exp points to spawn.</param>
        public static void SpawnExp(Vector3 pos, int exp)
        {
            while (exp > 0)
            {
                if (exp - 1000 > 0)
                {
                    exp -= 1000;
                    UnityEngine.Object.Instantiate(Resources.Load("exp/exp7"), pos, Quaternion.identity);
                }
                else if (exp - 250 > 0)
                {
                    exp -= 250;
                    UnityEngine.Object.Instantiate(Resources.Load("exp/exp6"), pos, Quaternion.identity);
                }
                else if (exp - 60 > 0)
                {
                    exp -= 60;
                    UnityEngine.Object.Instantiate(Resources.Load("exp/exp5"), pos, Quaternion.identity);
                }
                else if (exp - 20 > 0)
                {
                    exp -= 20;
                    UnityEngine.Object.Instantiate(Resources.Load("exp/exp4"), pos, Quaternion.identity);
                }
                else if (exp - 15 > 0)
                {
                    exp -= 15;
                    UnityEngine.Object.Instantiate(Resources.Load("exp/exp3"), pos, Quaternion.identity);
                }
                else if (exp - 10 > 0)
                {
                    exp -= 10;
                    UnityEngine.Object.Instantiate(Resources.Load("exp/exp2"), pos, Quaternion.identity);
                }
                else if (exp - 5 > 0)
                {
                    exp -= 5;
                    UnityEngine.Object.Instantiate(Resources.Load("exp/exp1"), pos, Quaternion.identity);
                }
                else
                {
                    exp--;
                    UnityEngine.Object.Instantiate(Resources.Load("exp/exp0"), pos, Quaternion.identity);
                }
            }
        }

        /// <summary>
        /// Use to spawn an item into the game world as if dropped by a player.
        /// You may notice that the vanilla game's source-code uses Resources.Load to spawn items. Do not use that.
        /// </summary>
        /// <param name="pos">The position to spawn the item at. Note that despite being a 2D game, Roguelands uses 3D space. That being said, the z-coordinate should nearly always be 0.</param>
        /// <param name="item">The item to spawn.</param>
        /// <param name="isChip">True to drop a chip instead of a normal item.</param>
        public static ItemScript DropItem(Vector3 pos, Item item, bool isChip = false)
        {
            if (!isChip)
            {
                int[] st = new int[]
                {
                    item.id,
                    item.q,
                    item.exp,
                    item.tier,
                    item.corrupted,
                    item.aspect[0],
                    item.aspect[1],
                    item.aspect[2],
                    item.aspectLvl[0],
                    item.aspectLvl[1],
                    item.aspectLvl[2]
                };
                ItemScript itemScript = ((GameObject)Network.Instantiate(Resources.Load("i2"), pos, Quaternion.identity, 0)).GetComponent<ItemScript>();
                itemScript.SendMessage("Init", st);
                if (ItemRegistry.GetSingleton().HasEntry(item.id) && (ItemRegistry.GetSingleton().GetEntry(item.id).Type & ItemType.EQUIPMENT_F) == ItemType.EQUIPMENT_F) itemScript.back.SetActive(true);
                return itemScript;
            }
            else
            {
                ItemScript itemScript = ((GameObject)Network.Instantiate(Resources.Load("i2"), pos, Quaternion.identity, 0)).GetComponent<ItemScript>();
                itemScript.SendMessage("Chip", item.id);
                return itemScript;
            }
        }

        /// <summary>
        /// Use to manually add new resources to the game, or overwrite existing ones. May only be called from the Initialize method of a GadgetMod.
        /// </summary>
        /// <param name="path">The pseudo-file-path to place the resource on.</param>
        /// <param name="resource">The resource to register.</param>
        public static void AddCustomResource(string path, UnityEngine.Object resource)
        {
            if (!Registry.registeringVanilla && Registry.modRegistering < 0) throw new InvalidOperationException("Data registration may only be performed by the Initialize method of a GadgetMod!");
            resource.hideFlags |= HideFlags.HideAndDontSave;
            if (resource is GameObject)
            {
                if (!(resource as GameObject).active)
                {
                    resource.hideFlags |= HideFlags.HideInInspector;
                }
                else
                {
                    (resource as GameObject).SetActive(false);
                }
            }
            resources[path] = resource;
        }

        /// <summary>
        /// Use to register a texture for the tile spritesheet. You probably shouldn't use this yourself - it is automatically called by TileInfo after registration. May only be called from the Initialize method of a GadgetMod.
        /// </summary>
        /// <param name="sprite">The Texture2D to register to the spritesheet</param>
        public static SpriteSheetEntry AddTextureToSheet(Texture sprite)
        {
            if (!Registry.registeringVanilla && Registry.modRegistering < 0) throw new InvalidOperationException("Data registration may only be performed by the Initialize method of a GadgetMod!");
            SpriteSheetEntry entry = new SpriteSheetEntry(sprite, spriteSheetSprites.Count);
            spriteSheetSprites.Add(entry);
            return entry;
        }

        public static bool IsCustomResourceRegistered(string path)
        {
            return resources.ContainsKey(path);
        }

        public static UnityEngine.Object GetCustomResource(string path)
        {
            return resources[path];
        }

        public static Material GetItemMaterial(int id)
        {
            return (Material)Resources.Load("i/i" + id);
        }

        public static Material GetTileMaterial(int id)
        {
            return (Material)Resources.Load("construct/c" + id);
        }

        public static Material GetChipMaterial(int id)
        {
            return (Material)Resources.Load("cc/cc" + id);
        }

        public static Material GetWeaponMaterial(int id)
        {
            return (Material)Resources.Load("ie/ie" + id);
        }


        public static Material GetOffhandMaterial(int id)
        {
            return (Material)Resources.Load("o/o" + id);
        }

        public static Material GetHeadMaterial(int id)
        {
            return (Material)Resources.Load("h/h" + id);
        }

        public static Material GetBodyMaterial(int id)
        {
            return (Material)Resources.Load("b/b" + id);
        }

        public static Material GetArmMaterial(int id)
        {
            return (Material)Resources.Load("a/a" + id);
        }

        public static Material GetDroidHeadMaterial(int id)
        {
            return (Material)Resources.Load("droid/d" + id + "h");
        }

        public static Material GetDroidBodyMaterial(int id)
        {
            return (Material)Resources.Load("droid/d" + id + "b");
        }

        public static Material GetRaceMaterial(int id, int variation)
        {
            return (Material)Resources.Load("r/r" + id + "v" + variation);
        }

        public static Material GetSignMaterial(int id)
        {
            return (Material)Resources.Load("sign/sign" + id);
        }

        public static Material GetTerrainSideMaterial(int id, bool vertical)
        {
            return (Material)Resources.Load("side/side" + (vertical ? "v" : "h") + id);
        }

        public static Material GetTerrainZoneMaterial(int id)
        {
            return (Material)Resources.Load("z/z" + id);
        }

        public static Material GetTerrainEntranceMaterial(int id)
        {
            return (Material)Resources.Load("z/entrance" + id);
        }

        public static Material GetTerrainMidChunkMaterial(int id, bool opening)
        {
            return (Material)Resources.Load("z/midchunk" + (opening ? 1 : 0) + "b" + id);
        }

        public static Material GetWorldBackgroundMaterial(int id, int depthIndex)
        {
            return (Material)Resources.Load("bg/b" + id + "bg" + depthIndex);
        }

        public static Material GetWorldParallaxMaterial(int id)
        {
            return (Material)Resources.Load("par/parallax" + id);
        }

        public static Material GetFactionFlagMaterial(int id)
        {
            return (Material)Resources.Load("flag/flag" + id);
        }

        public static Material GetDifficultyFlagMaterial(int id)
        {
            return (Material)Resources.Load("flag/prof" + id);
        }

        public static Material GetChatPortraitMaterial(int id)
        {
            return (Material)Resources.Load("mat/por/portrait" + id);
        }

        public static Material GetMiscellaneousMaterial(string name)
        {
            return (Material)Resources.Load("mat/" + name);
        }

        public static GameObject GetPropResource(int id)
        {
            return (GameObject)Resources.Load("prop/" + id);
        }

        public static GameObject GetCreatureResource(string name)
        {
            return (GameObject)Resources.Load("e/" + name);
        }

        public static GameObject GetProjectileResource(string name)
        {
            return (GameObject)Resources.Load("proj/" + name);
        }

        public static GameObject GetSpecialProjectileResource(string name)
        {
            return (GameObject)Resources.Load("spec/" + name);
        }

        public static GameObject GetWeaponProjectileResource(int id)
        {
            return (GameObject)Resources.Load("proj/shot" + id);
        }

        public static GameObject GetHazardResource(string name)
        {
            return (GameObject)Resources.Load("haz/" + name);
        }

        public static GameObject GetObjectResource(string name)
        {
            return (GameObject)Resources.Load("obj/" + name);
        }

        public static GameObject GetNPCResource(string name)
        {
            return (GameObject)Resources.Load("npc/" + name);
        }

        public static GameObject GetPlaceableNPCResource(int id)
        {
            return (GameObject)Resources.Load("npc/npc" + id);
        }

        public static AudioClip GetAudioClip(string name)
        {
            return (AudioClip)Resources.Load("au/" + name);
        }

        public static AudioClip GetItemAudioClip(int id)
        {
            return (AudioClip)Resources.Load("au/i/i" + id);
        }

        public sealed class SpriteSheetEntry
        {
            internal readonly Texture tex;
            internal readonly int index;

            internal Vector2 coords;

            internal SpriteSheetEntry(Texture tex, int index)
            {
                if (tex.width != 32 || tex.height != 32) throw new InvalidOperationException("SpriteSheet textures must be 32x32!");
                this.tex = tex;
                this.index = index;
            }

            public Texture GetTex()
            {
                return tex;
            }

            public Vector2 GetCoords()
            {
                if (spriteSheet != null) return coords;
                throw new InvalidOperationException("The SpriteSheet has not been generated yet!");
            }
        }
    }
}