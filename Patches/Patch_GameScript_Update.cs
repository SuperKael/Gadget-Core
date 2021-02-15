using HarmonyLib;
using GadgetCore.API;
using UnityEngine;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Linq;
using System.Reflection;
using GadgetCore.Util;
using System.IO;
using System.Collections;
using System;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(GameScript))]
    [HarmonyPatch("Update")]
    static class Patch_GameScript_Update
    {
        public static readonly MethodInfo RefreshSlot = typeof(GameScript).GetMethod("RefreshSlot", BindingFlags.Public | BindingFlags.Instance);

        [HarmonyPrefix]
        public static void Prefix(ref Item ___holdingItem, ref Item __state)
        {
            __state = GadgetCoreAPI.CopyItem(___holdingItem);
        }

        [HarmonyPostfix]
        public static void Postfix(GameScript __instance, ref RaycastHit ___hit, ref int ___craftType, ref int ___slotID, ref bool ___shifting, ref Item[] ___inventory, ref Item[] ___craft, ref Item[] ___modSlot, ref int ___curBlockSlot, ref ChunkWorld ___chunkWorld, ref ChunkWorld ___chunkWorldWall, ref Item __state)
        {
            if (!GameScript.pausing && !GameScript.inventoryOpen && GameScript.buildMode && Input.GetMouseButtonDown(0) && ___inventory[___curBlockSlot].q > 0 && ItemRegistry.Singleton.HasEntry(___inventory[___curBlockSlot].id))
            {
                int num4 = (int)__instance.hoverBuild.transform.position.x;
                int num5 = (int)__instance.hoverBuild.transform.position.y;
                int num6 = (num4 + 62) / 4;
                int num7 = (num5 + 62) / 4;
                if (num6 < 64 && num6 >= 0 && num7 < 64 && num7 >= 0)
                {
                    ItemInfo item = ItemRegistry.Singleton.GetEntry(___inventory[___curBlockSlot].id);
                    if (item.Tile != null && item.Tile.TryPlaceTile())
                    {
                        if (item.Tile.Type == TileType.WALL)
                        {
                            if (___chunkWorldWall.grid[num6, num7] == 0)
                            {
                                int[] value = new int[]
                                {
                                        num4,
                                        num5,
                                        GameScript.curBlockID
                                };
                                __instance.WallManager.SendMessage("PlaceTile", value);
                                __instance.hoverBuild.SendMessage("Refresh");
                                __instance.GetComponent<AudioSource>().PlayOneShot((AudioClip)Resources.Load("Au/emblem"), Menuu.soundLevel / 10f);
                                ___inventory[___curBlockSlot].q--;
                                RefreshSlot.Invoke(__instance, new object[] { ___curBlockSlot });
                                if (___inventory[___curBlockSlot].q <= 0 && GameScript.buildMode)
                                {
                                    __instance.ExitBuildMode();
                                    __instance.hoverSprite.GetComponent<Renderer>().material = (Material)Resources.Load("mat/trans");
                                    GameScript.curBlockID = 0;
                                }
                            }
                        }
                        else if (item.Tile.Type == TileType.SOLID)
                        {
                            if (___chunkWorld.gridSpecial[num6, num7] == 0 && ___chunkWorld.grid[num6, num7] == 0)
                            {
                                int[] value3 = new int[]
                                {
                                    num4,
                                    num5,
                                    GameScript.curBlockID
                                };
                                __instance.TileManager.SendMessage("PlaceTile", value3);
                                __instance.hoverBuild.SendMessage("Refresh");
                                __instance.GetComponent<AudioSource>().PlayOneShot((AudioClip)Resources.Load("Au/emblem"), Menuu.soundLevel / 10f);
                                ___inventory[___curBlockSlot].q--;
                                RefreshSlot.Invoke(__instance, new object[] { ___curBlockSlot });
                                if (___inventory[___curBlockSlot].q <= 0 && GameScript.buildMode)
                                {
                                    __instance.ExitBuildMode();
                                    __instance.hoverSprite.GetComponent<Renderer>().material = (Material)Resources.Load("mat/trans");
                                    GameScript.curBlockID = 0;
                                }
                            }
                        }
                        else
                        {
                            if (___chunkWorld.gridSpecial[num6, num7] == 0 && ___chunkWorld.grid[num6, num7] == 0)
                            {
                                if (GameScript.curBlockID == 2107 && __instance.gatherer)
                                {
                                    __instance.Error(11);
                                }
                                else
                                {
                                    int[] value2 = new int[]
                                    {
                                            num4,
                                            num5,
                                            GameScript.curBlockID
                                    };
                                    __instance.TileManager.SendMessage("PlaceTileSpecial", value2);
                                    __instance.hoverBuild.SendMessage("Refresh");
                                    __instance.GetComponent<AudioSource>().PlayOneShot((AudioClip)Resources.Load("Au/emblem"), Menuu.soundLevel / 10f);
                                    ___inventory[___curBlockSlot].q--;
                                    RefreshSlot.Invoke(__instance, new object[] { ___curBlockSlot });
                                    if (___inventory[___curBlockSlot].q <= 0 && GameScript.buildMode)
                                    {
                                        __instance.ExitBuildMode();
                                        __instance.hoverSprite.GetComponent<Renderer>().material = (Material)Resources.Load("mat/trans");
                                        GameScript.curBlockID = 0;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else if (MenuScript.player && !GameScript.pausing)
            {
                if (Input.GetMouseButtonDown(0) && GameScript.inventoryOpen && Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out ___hit, 7f))
                {
                    if (___hit.transform.gameObject.layer == 16)
                    {
                        if (___hit.transform.gameObject.tag == "craft")
                        {
                            if (MenuRegistry.Singleton[___craftType] is CraftMenuInfo craftMenu)
                            {
                                ___slotID = int.Parse(___hit.transform.gameObject.name);
                                if (__state.id != 0)
                                {
                                    Item holdingItem = __state;
                                    Item[] craftItems = ___craft;
                                    int slotID = ___slotID;
                                    if (craftMenu.SlotValidators.Any(x => x(holdingItem, craftItems, slotID)))
                                    {
                                        if (___craft[___slotID].id == __state.id)
                                        {
                                            __instance.InvokeMethod("CombineItemCraft", ___slotID);
                                        }
                                        else if (___craft[___slotID].id == 0)
                                        {
                                            __instance.InvokeMethod("PlaceItemCraft", ___slotID);
                                        }
                                    }
                                }
                                else if (___craft[___slotID].id != 0)
                                {
                                    if (!___shifting)
                                    {
                                        __instance.InvokeMethod("SelectItemCraft", ___slotID);
                                    }
                                    else if (___shifting && __instance.menuCraft.activeSelf)
                                    {
                                        __instance.StartCoroutine(__instance.InvokeMethod<IEnumerator>("ShiftClickCraft", ___slotID));
                                    }
                                }
                            }
                            else if (___craftType == 0)
                            {
                                ___slotID = int.Parse(___hit.transform.gameObject.name);
                                if (__state.id != 0 && ___craft[___slotID].id != __state.id)
                                {
                                    if ((__state.id <= 100 || __state.id >= 200) && __instance.InvokeMethod<bool>("CanPlaceCraft", __state.id))
                                    {
                                        ItemType? itemType = ItemRegistry.GetItem(__state.id)?.Type;
                                        if (itemType.HasValue && (itemType.Value & (ItemType.EMBLEM | ItemType.EQUIPABLE)) == ItemType.EMBLEM)
                                        {
                                            if (___craft[___slotID].id == 0)
                                            {
                                                __instance.InvokeMethod("PlaceItemCraft", ___slotID);
                                            }
                                            else
                                            {
                                                __instance.InvokeMethod("SwapItemCraft", ___slotID);
                                            }
                                        }
                                    }
                                }
                            }
                            else if (___craftType == 1)
                            {
                                ___slotID = int.Parse(___hit.transform.gameObject.name);
                                if (__state.id != 0 && ___craft[___slotID].id != __state.id)
                                {
                                    if ((__state.id <= 10 || __state.id >= 40) && __instance.InvokeMethod<bool>("CanPlaceCraft", __state.id))
                                    {
                                        ItemType? itemType = ItemRegistry.GetItem(__state.id)?.Type;
                                        if (itemType.HasValue && (itemType.Value & (ItemType.EMBLEM | ItemType.EQUIPABLE)) == ItemType.LOOT && (itemType.Value & ItemType.ORGANIC) == ItemType.ORGANIC)
                                        {
                                            if (___craft[___slotID].id == 0)
                                            {
                                                __instance.InvokeMethod("PlaceItemCraft", ___slotID);
                                            }
                                            else
                                            {
                                                __instance.InvokeMethod("SwapItemCraft", ___slotID);
                                            }
                                        }
                                    }
                                }
                            }
                            else if (___craftType == 2)
                            {
                                ___slotID = int.Parse(___hit.transform.gameObject.name);
                                if (__state.id != 0)
                                {
                                    if (___slotID == 0 && ___craft[___slotID].id != __state.id)
                                    {
                                        if (___craft[0].id == 0 &&
                                            (__state.id < 1000 || __state.id >= 1006) && (__state.id < 450 || __state.id >= 456) &&
                                            (__state.id < 500 || __state.id >= 506) && (__state.id < 600 || __state.id >= 612) &&
                                            (__state.id < 550 || __state.id >= 562) && (__state.id < 350 || __state.id >= 362) &&
                                            (__state.id < 400 || __state.id >= 412) && (__state.id < 300 || __state.id >= 312) &&
                                            (__state.id < 700 || __state.id >= 712) && (__state.id < 800 || __state.id >= 812))
                                        {
                                            Item holdingItem = __state;
                                            if (GadgetCoreAPI.ultimateForgeRecipes.Any(x => x.Key.Item1 == holdingItem.id))
                                            {
                                                if (__instance.InvokeMethod<int>("GetItemLevel", holdingItem.exp) >= 10)
                                                {
                                                    __instance.InvokeMethod("PlaceItemCraft", ___slotID);
                                                }
                                                else
                                                {
                                                    __instance.InvokeMethod("Error", 12);
                                                }
                                            }
                                        }
                                    }
                                    else if (___slotID == 2)
                                    {
                                        if (___craft[2].id == 0 && (__state.id < 89 || __state.id >= 92))
                                        {
                                            Item holdingItem = __state;
                                            if (GadgetCoreAPI.ultimateForgeRecipes.Any(x => x.Key.Item2 == holdingItem.id))
                                            {
                                                __instance.InvokeMethod("PlaceItemCraft", ___slotID);
                                            }
                                        }
                                    }
                                }
                            }
                            else if (___craftType == 3)
                            {
                                ___slotID = int.Parse(___hit.transform.gameObject.name);
                                if (__state.id != 0 && ___craft[___slotID].id != __state.id)
                                {
                                    if ((__state.id <= 103 || __state.id >= 107) && (__state.id <= 113 || __state.id >= 117) &&
                                        (__state.id <= 123 || __state.id >= 127) && (__state.id <= 133 || __state.id >= 137)
                                        && __instance.InvokeMethod<bool>("CanPlaceCraft2", __state.id))
                                    {
                                        Item holdingItem = __state;
                                        if (GadgetCoreAPI.creationMachineRecipes.Any(x => x.Key == holdingItem.id))
                                        {
                                            if (___craft[___slotID].id == 0)
                                            {
                                                __instance.InvokeMethod("PlaceItemCraft", ___slotID);
                                            }
                                            else
                                            {
                                                __instance.InvokeMethod("SwapItemCraft", ___slotID);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else if (___hit.transform.gameObject.tag == "planet")
                        {
                            ___slotID = int.Parse(___hit.transform.gameObject.name);
                            if (___slotID >= 14)
                            {
                                while (___slotID >= 14)
                                {
                                    ___slotID -= 14;
                                    PlanetRegistry.PlanetSelectorPage++;
                                }
                                PlanetRegistry.UpdatePlanetSelector();
                            }
                            int planetIndex = (PlanetRegistry.PlanetSelectorPage - 2) * 14 + ___slotID;
                            if (planetIndex >= 0 && planetIndex < PlanetRegistry.selectorPlanets.Length && PlanetRegistry.selectorPlanets[planetIndex] is PlanetInfo planet)
                            {
                                if (planet.PortalUses > 0 || planet.PortalUses == -1)
                                {
                                    InstanceTracker.GameScript.GetComponent<AudioSource>().PlayOneShot((AudioClip)Resources.Load("Au/confirm"), Menuu.soundLevel / 10f);
                                    InstanceTracker.GameScript.planetSelector.transform.position = new Vector3(InstanceTracker.GameScript.planetGuard[___slotID].transform.position.x, InstanceTracker.GameScript.planetGuard[___slotID].transform.position.y, InstanceTracker.GameScript.planetSelector.transform.position.z);
                                    GameScript.curPlanet = ___slotID;
                                    InstanceTracker.GameScript.planetObj.GetComponent<Renderer>().material = (Material)Resources.Load("mat/planet" + planet.ID);
                                    InstanceTracker.GameScript.planetObj.GetComponent<Animation>().Play();
                                    InstanceTracker.GameScript.txtPlanetName[0].text = planet.Name;
                                    InstanceTracker.GameScript.txtPlanetName[1].text = InstanceTracker.GameScript.txtPlanetName[0].text;
                                    InstanceTracker.GameScript.txtHostile[0].text = "Hostile Lv." + InstanceTracker.GameScript.InvokeMethod("GetPlanetHostile", planet.ID);
                                    InstanceTracker.GameScript.txtHostile[1].text = InstanceTracker.GameScript.txtHostile[0].text;
                                    if (planet.PortalUses > 0)
                                    {
                                        InstanceTracker.GameScript.txtPortalUses[0].text = "Portal Uses: " + planet.PortalUses;
                                        InstanceTracker.GameScript.txtPortalUses[1].text = InstanceTracker.GameScript.txtPortalUses[0].text;
                                    }
                                    else if (planet.PortalUses == -1)
                                    {
                                        InstanceTracker.GameScript.txtPortalUses[0].text = "Portal Uses: Infinite";
                                        InstanceTracker.GameScript.txtPortalUses[1].text = InstanceTracker.GameScript.txtPortalUses[0].text;
                                    }
                                }
                            }
                        }
                        else if (___hit.transform.gameObject.tag == "mod")
                        {
                            ___slotID = int.Parse(___hit.transform.gameObject.name);
                            Item holdingItem = __state;
                            if (holdingItem.id != 0)
                            {
                                if ((holdingItem.id < 300 || holdingItem.id >= 1000) && (ItemRegistry.GetItem(holdingItem.id).Type & ItemType.MODABLE) == ItemType.MODABLE)
                                {
                                    if (___modSlot[___slotID].id == 0)
                                    {
                                        __instance.PlaceItemMod(___slotID);
                                    }
                                    else
                                    {
                                        __instance.SwapItemMod(___slotID);
                                    }
                                }
                            }
                            else if ((___modSlot[___slotID].id < 300 || ___modSlot[___slotID].id >= 1000) && (ItemRegistry.GetItem(___modSlot[___slotID].id).Type & ItemType.MODABLE) == ItemType.MODABLE)
                            {
                                __instance.SelectItemMod(___slotID);
                            }
                        }
                    }
                    else if (___hit.transform.gameObject.layer != 17)
                    {
                        switch (___hit.transform.gameObject.name)
                        {
                            case "bPlanetPageBack":
                                __instance.GetComponent<AudioSource>().PlayOneShot((AudioClip)Resources.Load("Au/confirm"), Menuu.soundLevel / 10f);
                                if (PlanetRegistry.PlanetSelectorPage <= 1)
                                {
                                    PlanetRegistry.PlanetSelectorPage = PlanetRegistry.PlanetSelectorPages;
                                }
                                else
                                {
                                    PlanetRegistry.PlanetSelectorPage--;
                                }
                                PlanetRegistry.UpdatePlanetSelector();
                                break;
                            case "bPlanetPageForward":
                                __instance.GetComponent<AudioSource>().PlayOneShot((AudioClip)Resources.Load("Au/confirm"), Menuu.soundLevel / 10f);
                                if (PlanetRegistry.PlanetSelectorPage >= PlanetRegistry.PlanetSelectorPages)
                                {
                                    PlanetRegistry.PlanetSelectorPage = 1;
                                }
                                else
                                {
                                    PlanetRegistry.PlanetSelectorPage++;
                                }
                                PlanetRegistry.UpdatePlanetSelector();
                                break;
                        }
                    }
                }
                else if (Input.GetMouseButtonDown(1) && GameScript.inventoryOpen && Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out ___hit, 7f))
                {
                    if (___hit.transform.gameObject.layer == 16)
                    {
                        if (___hit.transform.gameObject.tag == "craft")
                        {
                            if (MenuRegistry.Singleton.GetEntry(___craftType) is CraftMenuInfo craftMenu)
                            {
                                ___slotID = int.Parse(___hit.transform.gameObject.name);
                                if (__state.id != 0)
                                {
                                    Item holdingItem = __state;
                                    Item[] craftItems = ___craft;
                                    int slotID = ___slotID;
                                    if (craftMenu.SlotValidators.Any(x => x(holdingItem, craftItems, slotID)))
                                    {
                                        if (___craft[___slotID].id == 0 || (GadgetCoreAPI.CanItemsStack(___craft[___slotID], __state) && ___craft[___slotID].q < 9999))
                                        {
                                            __instance.InvokeMethod("PlaceOneItemCraft", ___slotID);
                                        }
                                        else
                                        {
                                            __instance.InvokeMethod("SwapItemCraft", ___slotID);
                                        }
                                    }
                                }
                            }
                        }
                        else if (___hit.transform.gameObject.tag == "mod")
                        {
                            ___slotID = int.Parse(___hit.transform.gameObject.name);
                            Item holdingItem = __state;
                            if (holdingItem.id != 0)
                            {
                                if ((holdingItem.id < 300 || holdingItem.id >= 1000) && (ItemRegistry.GetItem(holdingItem.id).Type & ItemType.MODABLE) == ItemType.MODABLE)
                                {
                                    if (___modSlot[___slotID].id == 0)
                                    {
                                        __instance.PlaceItemMod(___slotID);
                                    }
                                    else
                                    {
                                        __instance.SwapItemMod(___slotID);
                                    }
                                }
                            }
                            else if ((___modSlot[___slotID].id < 300 || ___modSlot[___slotID].id >= 1000) && (ItemRegistry.GetItem(holdingItem.id).Type & ItemType.MODABLE) == ItemType.MODABLE)
                            {
                                __instance.SelectItemMod(___slotID);
                            }
                        }
                    }
                }
            }
        }

        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            CodeInstruction[] codes = instructions.ToArray();
            List<CodeInstruction> newCodes = new List<CodeInstruction>();
            for (int i = 0; i < 11; i++) newCodes.Add(codes[i]);
            for (int i = 11; i < codes.Length; i++)
            {
                newCodes.Add(codes[i]);
                if ((codes[i - 1] != null && codes[i - 1].opcode == OpCodes.Ldsfld && codes[i - 1].operand.ToString().Equals("System.Boolean pausing")
                 && codes[i] != null && codes[i].opcode == OpCodes.Brtrue) ||
                    (codes[i - 2] != null && codes[i - 2].opcode == OpCodes.Ldc_I4_S && ((sbyte)codes[i - 2].operand) == 98
                 && codes[i - 1] != null && codes[i - 1].opcode == OpCodes.Call && (codes[i - 1].operand as MethodBase).Name == "GetKeyDown"
                 && codes[i] != null && codes[i].opcode == OpCodes.Brfalse))
                {
                    newCodes.Add(new CodeInstruction(OpCodes.Call, typeof(GadgetCoreAPI).GetMethod("IsInputFrozen", BindingFlags.Public | BindingFlags.Static)));
                    newCodes.Add(new CodeInstruction(OpCodes.Brtrue, codes[i].operand));
                }
                else if (codes[i - 11] != null && codes[i - 11].opcode == OpCodes.Ldloc_S
                 && codes[i - 10] != null && codes[i - 10].opcode == OpCodes.Ldc_I4_S
                 && codes[i - 9] != null && codes[i - 9].opcode == OpCodes.Bge
                 && codes[i - 8] != null && codes[i - 8].opcode == OpCodes.Ldloc_S
                 && codes[i - 7] != null && codes[i - 7].opcode == OpCodes.Ldc_I4_0
                 && codes[i - 6] != null && codes[i - 6].opcode == OpCodes.Blt
                 && codes[i - 5] != null && codes[i - 5].opcode == OpCodes.Ldloc_S
                 && codes[i - 4] != null && codes[i - 4].opcode == OpCodes.Ldc_I4_S
                 && codes[i - 3] != null && codes[i - 3].opcode == OpCodes.Bge
                 && codes[i - 2] != null && codes[i - 2].opcode == OpCodes.Ldloc_S
                 && codes[i - 1] != null && codes[i - 1].opcode == OpCodes.Ldc_I4_0
                 && codes[i] != null && codes[i].opcode == OpCodes.Blt)
                {
                    if (codes.Length > i + 1 && codes[i + 1].opcode == OpCodes.Ldsfld)
                    {
                        newCodes.Add(new CodeInstruction(OpCodes.Ldsfld, codes[i + 1].operand));
                        newCodes.Add(new CodeInstruction(OpCodes.Ldc_I4, TileRegistry.Singleton.GetIDStart()));
                        newCodes.Add(new CodeInstruction(OpCodes.Bge, codes[i].operand));
                    }
                }
            }

            var p = TranspilerHelper.CreateProcessor(newCodes, generator);

            var forgeEmblemBlock = p.FindRefByInsn(new CodeInstruction(OpCodes.Call, "Void ForgeEmblem(Int32)"));
            p.InjectInsn(forgeEmblemBlock.GetRefByOffset(-21), new CodeInstruction(OpCodes.Brtrue, p.GetInsn(forgeEmblemBlock.GetRefByOffset(-20)).operand), false);
            p.RemoveInsns(forgeEmblemBlock.GetRefByOffset(-20), 17);
            p.RemoveInsns(forgeEmblemBlock.GetRefByOffset(1), 6);

            var prismItemBlock = p.FindRefByInsn(new CodeInstruction(OpCodes.Call, "Void PrismItem(Int32)"));
            p.InjectInsn(prismItemBlock.GetRefByOffset(-21), new CodeInstruction(OpCodes.Brtrue, p.GetInsn(prismItemBlock.GetRefByOffset(-20)).operand), false);
            p.RemoveInsns(prismItemBlock.GetRefByOffset(-20), 17);
            p.RemoveInsns(prismItemBlock.GetRefByOffset(1), 6);

            return p.GetInstructions();
        }
    }
}