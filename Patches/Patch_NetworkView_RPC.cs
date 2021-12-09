using HarmonyLib;
using GadgetCore.API;
using UnityEngine;
using System.Reflection;
using System;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(NetworkView))]
    [HarmonyPatch("RPC")]
    [HarmonyPatch(new Type[] { typeof(string), typeof(RPCMode), typeof(object[]) })]
    static class Patch_NetworkView_RPC
    {
        public static readonly MethodInfo RPCMethod = typeof(NetworkView).GetMethod("RPC", new Type[] { typeof(string), typeof(RPCMode), typeof(object[]) });

        [HarmonyPrefix]
        public static bool Prefix(NetworkView __instance, string name, RPCMode mode, ref object[] args)
        {
            if (Assembly.GetCallingAssembly().Equals(typeof(GameScript).Assembly))
            {
                switch (name)
                {
                    case "Set":
                        if (args.Length == 3)
                        {
                            if (args[0] is int)
                            {
                                if (!GadgetNetwork.MatrixReady && GadgetNetwork.GetTimeSinceConnect() < GadgetNetwork.MatrixTimeout)
                                {
                                    InstanceTracker.GameScript.StartCoroutine(GadgetUtils.WaitAndInvoke(RPCMethod, GadgetNetwork.MatrixTimeout - GadgetNetwork.GetTimeSinceConnect(), () => GadgetNetwork.MatrixReady, __instance, name, mode, args));
                                    return true;
                                }
                                GadgetNetwork.ConvertIDToHost(PlanetRegistry.Singleton, ref args[0]);
                            }
                        }
                        else if (args.Length == 1)
                        {
                            if (args[0] is int[] arr && arr.Length == 2)
                            {
                                if (!GadgetNetwork.MatrixReady && GadgetNetwork.GetTimeSinceConnect() < GadgetNetwork.MatrixTimeout)
                                {
                                    InstanceTracker.GameScript.StartCoroutine(GadgetUtils.WaitAndInvoke(RPCMethod, GadgetNetwork.MatrixTimeout - GadgetNetwork.GetTimeSinceConnect(), () => GadgetNetwork.MatrixReady, __instance, name, mode, args));
                                    return true;
                                }
                                ItemStandScript itemStandScript = __instance.GetComponent<ItemStandScript>();
                                if (itemStandScript == null || !itemStandScript.isChipStand)
                                {
                                    args[0] = new int[]
                                    {
                                        GadgetNetwork.ConvertIDToHost(ItemRegistry.Singleton, arr[0]),
                                        arr[1],
                                    };
                                }
                                else
                                {
                                    args[0] = new int[]
                                    {
                                        GadgetNetwork.ConvertIDToHost(ChipRegistry.Singleton, arr[0]),
                                        arr[1],
                                    };
                                }
                            }
                            else if (args[0] is Package2 pack2)
                            {
                                if (!GadgetNetwork.MatrixReady && GadgetNetwork.GetTimeSinceConnect() < GadgetNetwork.MatrixTimeout)
                                {
                                    InstanceTracker.GameScript.StartCoroutine(GadgetUtils.WaitAndInvoke(RPCMethod, GadgetNetwork.MatrixTimeout - GadgetNetwork.GetTimeSinceConnect(), () => GadgetNetwork.MatrixReady, __instance, name, mode, args));
                                    return true;
                                }
                                GadgetNetwork.ConvertIDToHost(ItemRegistry.Singleton /* ProjectileRegistry */, ref pack2.id);
                            }
                        }
                        break;
                    case "UA":
                        if (args.Length == 3)
                        {
                            if (args[0] is int[] arr && arr.Length == 8)
                            {
                                if (!GadgetNetwork.MatrixReady && GadgetNetwork.GetTimeSinceConnect() < GadgetNetwork.MatrixTimeout)
                                {
                                    InstanceTracker.GameScript.StartCoroutine(GadgetUtils.WaitAndInvoke(RPCMethod, GadgetNetwork.MatrixTimeout - GadgetNetwork.GetTimeSinceConnect(), () => GadgetNetwork.MatrixReady, __instance, name, mode, args));
                                    return true;
                                }
                                args[0] = new int[]
                                {
                                    GadgetNetwork.ConvertIDToHost(ItemRegistry.Singleton, arr[0]),
                                    GadgetNetwork.ConvertIDToHost(ItemRegistry.Singleton, arr[1]),
                                    GadgetNetwork.ConvertIDToHost(ItemRegistry.Singleton, arr[2]),
                                    GadgetNetwork.ConvertIDToHost(ItemRegistry.Singleton, arr[3]),
                                    GadgetNetwork.ConvertIDToHost(CharacterRaceRegistry.Singleton, arr[4]),
                                    arr[5],
                                    GadgetNetwork.ConvertIDToHost(CharacterUniformRegistry.Singleton, arr[6]),
                                    GadgetNetwork.ConvertIDToHost(CharacterAugmentRegistry.Singleton, arr[7])
                                };
                            }
                        }
                        else if (args.Length == 1)
                        {
                            if (args[0] is int[] arr && arr.Length == 3)
                            {
                                if (!GadgetNetwork.MatrixReady && GadgetNetwork.GetTimeSinceConnect() < GadgetNetwork.MatrixTimeout)
                                {
                                    InstanceTracker.GameScript.StartCoroutine(GadgetUtils.WaitAndInvoke(RPCMethod, GadgetNetwork.MatrixTimeout - GadgetNetwork.GetTimeSinceConnect(), () => GadgetNetwork.MatrixReady, __instance, name, mode, args));
                                    args[0] = new int[] { 1000, 1000, 1000 }; // Fix to bug with host droids in multiplayer
                                    return true;
                                }
                                args[0] = new int[]
                                {
                                    GadgetNetwork.ConvertIDToHost(ItemRegistry.Singleton, arr[0]),
                                    GadgetNetwork.ConvertIDToHost(ItemRegistry.Singleton, arr[1]),
                                    GadgetNetwork.ConvertIDToHost(ItemRegistry.Singleton, arr[2])
                                };
                            }
                        }
                        break;
                    case "ShootProjectile":
                        if (args.Length == 3)
                        {
                            if (args[0] is int)
                            {
                                if (!GadgetNetwork.MatrixReady && GadgetNetwork.GetTimeSinceConnect() < GadgetNetwork.MatrixTimeout)
                                {
                                    InstanceTracker.GameScript.StartCoroutine(GadgetUtils.WaitAndInvoke(RPCMethod, GadgetNetwork.MatrixTimeout - GadgetNetwork.GetTimeSinceConnect(), () => GadgetNetwork.MatrixReady, __instance, name, mode, args));
                                    return true;
                                }
                                GadgetNetwork.ConvertIDToHost(ItemRegistry.Singleton /* ProjectileRegistry */, ref args[0]);
                            }
                        }
                        break;
                    case "CreateWorld":
                        if (args.Length == 1)
                        {
                            if (args[0] is int[] arr && arr.Length > 1)
                            {
                                if (!GadgetNetwork.MatrixReady && GadgetNetwork.GetTimeSinceConnect() < GadgetNetwork.MatrixTimeout)
                                {
                                    InstanceTracker.GameScript.StartCoroutine(GadgetUtils.WaitAndInvoke(RPCMethod, GadgetNetwork.MatrixTimeout - GadgetNetwork.GetTimeSinceConnect(), () => GadgetNetwork.MatrixReady, __instance, name, mode, args));
                                    return true;
                                }
                                int[] newArr = new int[arr.Length];
                                Array.Copy(arr, newArr, arr.Length);
                                GadgetNetwork.ConvertIDToHost(PlanetRegistry.Singleton, ref newArr[0]);
                                args[0] = newArr;
                            }
                        }
                        break;
                    case "CreateTown":
                        if (args.Length == 1)
                        {
                            if (args[0] is int[] arr && arr.Length == 2)
                            {
                                if (!GadgetNetwork.MatrixReady && GadgetNetwork.GetTimeSinceConnect() < GadgetNetwork.MatrixTimeout)
                                {
                                    InstanceTracker.GameScript.StartCoroutine(GadgetUtils.WaitAndInvoke(RPCMethod, GadgetNetwork.MatrixTimeout - GadgetNetwork.GetTimeSinceConnect(), () => GadgetNetwork.MatrixReady, __instance, name, mode, args));
                                    return true;
                                }
                                args[0] = new int[]
                                {
                                    GadgetNetwork.ConvertIDToHost(PlanetRegistry.Singleton, arr[0]),
                                    arr[1],
                                };
                            }
                        }
                        break;
                    case "SpawnProjectile":
                        if (args.Length == 4)
                        {
                            if (args[1] is int)
                            {
                                if (!GadgetNetwork.MatrixReady && GadgetNetwork.GetTimeSinceConnect() < GadgetNetwork.MatrixTimeout)
                                {
                                    InstanceTracker.GameScript.StartCoroutine(GadgetUtils.WaitAndInvoke(RPCMethod, GadgetNetwork.MatrixTimeout - GadgetNetwork.GetTimeSinceConnect(), () => GadgetNetwork.MatrixReady, __instance, name, mode, args));
                                    return true;
                                }
                                GadgetNetwork.ConvertIDToHost(ItemRegistry.Singleton /* ProjectileRegistry */, ref args[1]);
                            }
                        }
                        break;
                    case "SetNetworked":
                        if (args.Length == 3)
                        {
                            if (args[1] is int)
                            {
                                if (!GadgetNetwork.MatrixReady && GadgetNetwork.GetTimeSinceConnect() < GadgetNetwork.MatrixTimeout)
                                {
                                    InstanceTracker.GameScript.StartCoroutine(GadgetUtils.WaitAndInvoke(RPCMethod, GadgetNetwork.MatrixTimeout - GadgetNetwork.GetTimeSinceConnect(), () => GadgetNetwork.MatrixReady, __instance, name, mode, args));
                                    return true;
                                }
                                GadgetNetwork.ConvertIDToHost(ItemRegistry.Singleton /* ProjectileRegistry */, ref args[1]);
                            }
                        }
                        break;
                    case "SetName":
                        if (args.Length == 1)
                        {
                            if (args[0] is int)
                            {
                                if (!GadgetNetwork.MatrixReady && GadgetNetwork.GetTimeSinceConnect() < GadgetNetwork.MatrixTimeout)
                                {
                                    InstanceTracker.GameScript.StartCoroutine(GadgetUtils.WaitAndInvoke(RPCMethod, GadgetNetwork.MatrixTimeout - GadgetNetwork.GetTimeSinceConnect(), () => GadgetNetwork.MatrixReady, __instance, name, mode, args));
                                    return true;
                                }
                                GadgetNetwork.ConvertIDToHost(PlanetRegistry.Singleton, ref args[0]);
                            }
                        }
                        break;
                    case "Staff":
                        if (args.Length == 3)
                        {
                            if (args[0] is int)
                            {
                                if (!GadgetNetwork.MatrixReady && GadgetNetwork.GetTimeSinceConnect() < GadgetNetwork.MatrixTimeout)
                                {
                                    InstanceTracker.GameScript.StartCoroutine(GadgetUtils.WaitAndInvoke(RPCMethod, GadgetNetwork.MatrixTimeout - GadgetNetwork.GetTimeSinceConnect(), () => GadgetNetwork.MatrixReady, __instance, name, mode, args));
                                    return true;
                                }
                                GadgetNetwork.ConvertIDToHost(ItemRegistry.Singleton /* ProjectileRegistry */, ref args[0]);
                            }
                        }
                        break;
                    case "ShootSpecial":
                        if (args.Length == 3)
                        {
                            if (args[0] is int)
                            {
                                if (!GadgetNetwork.MatrixReady && GadgetNetwork.GetTimeSinceConnect() < GadgetNetwork.MatrixTimeout)
                                {
                                    InstanceTracker.GameScript.StartCoroutine(GadgetUtils.WaitAndInvoke(RPCMethod, GadgetNetwork.MatrixTimeout - GadgetNetwork.GetTimeSinceConnect(), () => GadgetNetwork.MatrixReady, __instance, name, mode, args));
                                    return true;
                                }
                                GadgetNetwork.ConvertIDToHost(ItemRegistry.Singleton /* ProjectileRegistry */, ref args[0]);
                            }
                        }
                        break;
                    case "ShootProjectile2":
                        if (args.Length == 4)
                        {
                            if (args[0] is int)
                            {
                                if (!GadgetNetwork.MatrixReady && GadgetNetwork.GetTimeSinceConnect() < GadgetNetwork.MatrixTimeout)
                                {
                                    InstanceTracker.GameScript.StartCoroutine(GadgetUtils.WaitAndInvoke(RPCMethod, GadgetNetwork.MatrixTimeout - GadgetNetwork.GetTimeSinceConnect(), () => GadgetNetwork.MatrixReady, __instance, name, mode, args));
                                    return true;
                                }
                                GadgetNetwork.ConvertIDToHost(ItemRegistry.Singleton /* ProjectileRegistry */, ref args[0]);
                            }
                        }
                        break;
                    case "NetworkedProjectile":
                        if (args.Length == 4)
                        {
                            if (args[0] is int)
                            {
                                if (!GadgetNetwork.MatrixReady && GadgetNetwork.GetTimeSinceConnect() < GadgetNetwork.MatrixTimeout)
                                {
                                    InstanceTracker.GameScript.StartCoroutine(GadgetUtils.WaitAndInvoke(RPCMethod, GadgetNetwork.MatrixTimeout - GadgetNetwork.GetTimeSinceConnect(), () => GadgetNetwork.MatrixReady, __instance, name, mode, args));
                                    return true;
                                }
                                GadgetNetwork.ConvertIDToHost(ItemRegistry.Singleton /* ProjectileRegistry */, ref args[0]);
                            }
                        }
                        break;
                    case "Init":
                        if (args.Length == 1)
                        {
                            if (args[0] is int[] arr && arr.Length >= 11)
                            {
                                if (!GadgetNetwork.MatrixReady && GadgetNetwork.GetTimeSinceConnect() < GadgetNetwork.MatrixTimeout)
                                {
                                    InstanceTracker.GameScript.StartCoroutine(GadgetUtils.WaitAndInvoke(RPCMethod, GadgetNetwork.MatrixTimeout - GadgetNetwork.GetTimeSinceConnect(), () => GadgetNetwork.MatrixReady, __instance, name, mode, args));
                                    return true;
                                }
                                int[] newArr = new int[arr.Length];
                                Array.Copy(arr, newArr, arr.Length);
                                GadgetNetwork.ConvertIDToHost(ItemRegistry.Singleton, ref newArr[0]);
                                GadgetNetwork.ConvertIDToHost(ItemRegistry.Singleton, ref newArr[5]);
                                GadgetNetwork.ConvertIDToHost(ItemRegistry.Singleton, ref newArr[6]);
                                GadgetNetwork.ConvertIDToHost(ItemRegistry.Singleton, ref newArr[7]);
                                args[0] = newArr;
                            }
                        }
                        break;
                    case "Chip":
                        if (args.Length == 1)
                        {
                            if (args[0] is int)
                            {
                                if (!GadgetNetwork.MatrixReady && GadgetNetwork.GetTimeSinceConnect() < GadgetNetwork.MatrixTimeout)
                                {
                                    InstanceTracker.GameScript.StartCoroutine(GadgetUtils.WaitAndInvoke(RPCMethod, GadgetNetwork.MatrixTimeout - GadgetNetwork.GetTimeSinceConnect(), () => GadgetNetwork.MatrixReady, __instance, name, mode, args));
                                    return true;
                                }
                                GadgetNetwork.ConvertIDToHost(ChipRegistry.Singleton, ref args[0]);
                            }
                        }
                        break;
                    case "SpawnItem":
                        if (args.Length == 2)
                        {
                            if (args[0] is int[] arr && arr.Length >= 11)
                            {
                                if (!GadgetNetwork.MatrixReady && GadgetNetwork.GetTimeSinceConnect() < GadgetNetwork.MatrixTimeout)
                                {
                                    InstanceTracker.GameScript.StartCoroutine(GadgetUtils.WaitAndInvoke(RPCMethod, GadgetNetwork.MatrixTimeout - GadgetNetwork.GetTimeSinceConnect(), () => GadgetNetwork.MatrixReady, __instance, name, mode, args));
                                    return true;
                                }
                                int[] newArr = new int[arr.Length];
                                Array.Copy(arr, newArr, arr.Length);
                                GadgetNetwork.ConvertIDToHost(ItemRegistry.Singleton, ref newArr[0]);
                                GadgetNetwork.ConvertIDToHost(ItemRegistry.Singleton, ref newArr[5]);
                                GadgetNetwork.ConvertIDToHost(ItemRegistry.Singleton, ref newArr[6]);
                                GadgetNetwork.ConvertIDToHost(ItemRegistry.Singleton, ref newArr[7]);
                                args[0] = newArr;
                            }
                        }
                        break;
                    case "ChangePortal":
                        if (args.Length == 2)
                        {
                            if (args[0] is int)
                            {
                                if (!GadgetNetwork.MatrixReady && GadgetNetwork.GetTimeSinceConnect() < GadgetNetwork.MatrixTimeout)
                                {
                                    InstanceTracker.GameScript.StartCoroutine(GadgetUtils.WaitAndInvoke(RPCMethod, GadgetNetwork.MatrixTimeout - GadgetNetwork.GetTimeSinceConnect(), () => GadgetNetwork.MatrixReady, __instance, name, mode, args));
                                    return true;
                                }
                                GadgetNetwork.ConvertIDToHost(PlanetRegistry.Singleton, ref args[0]);
                            }
                        }
                        break;
                    case "ChangePortal2":
                        if (args.Length == 2)
                        {
                            if (args[0] is int)
                            {
                                if (!GadgetNetwork.MatrixReady && GadgetNetwork.GetTimeSinceConnect() < GadgetNetwork.MatrixTimeout)
                                {
                                    InstanceTracker.GameScript.StartCoroutine(GadgetUtils.WaitAndInvoke(RPCMethod, GadgetNetwork.MatrixTimeout - GadgetNetwork.GetTimeSinceConnect(), () => GadgetNetwork.MatrixReady, __instance, name, mode, args));
                                    return true;
                                }
                                GadgetNetwork.ConvertIDToHost(PlanetRegistry.Singleton, ref args[0]);
                            }
                        }
                        break;
                    case "RefreshWall":
                        if (args.Length == 1)
                        {
                            if (args[0] is int[] arr)
                            {
                                if (!GadgetNetwork.MatrixReady && GadgetNetwork.GetTimeSinceConnect() < GadgetNetwork.MatrixTimeout)
                                {
                                    InstanceTracker.GameScript.StartCoroutine(GadgetUtils.WaitAndInvoke(RPCMethod, GadgetNetwork.MatrixTimeout - GadgetNetwork.GetTimeSinceConnect(), () => GadgetNetwork.MatrixReady, __instance, name, mode, args));
                                    return true;
                                }
                                int[] newArr = new int[arr.Length];
                                for (int i = 0; i < arr.Length; i++)
                                {
                                    newArr[i] = GadgetNetwork.ConvertIDToHost(TileRegistry.Singleton, arr[i]);
                                }
                                args[0] = newArr;
                            }
                        }
                        break;
                    case "RefreshShip":
                        if (args.Length == 2)
                        {
                            if (args[0] is int[] arr1 && args[1] is int[] arr2)
                            {
                                if (!GadgetNetwork.MatrixReady && GadgetNetwork.GetTimeSinceConnect() < GadgetNetwork.MatrixTimeout)
                                {
                                    InstanceTracker.GameScript.StartCoroutine(GadgetUtils.WaitAndInvoke(RPCMethod, GadgetNetwork.MatrixTimeout - GadgetNetwork.GetTimeSinceConnect(), () => GadgetNetwork.MatrixReady, __instance, name, mode, args));
                                    return true;
                                }
                                int[] newArr1 = new int[arr1.Length];
                                for (int i = 0; i < arr1.Length; i++)
                                {
                                    newArr1[i] = GadgetNetwork.ConvertIDToHost(TileRegistry.Singleton, arr1[i]);
                                }
                                args[0] = newArr1;
                                int[] newArr2 = new int[arr2.Length];
                                for (int i = 0; i < arr2.Length; i++)
                                {
                                    newArr2[i] = GadgetNetwork.ConvertIDToHost(TileRegistry.Singleton, arr2[i]);
                                }
                                args[1] = newArr2;
                            }
                        }
                        break;
                    case "SetMaterial":
                        if (args.Length == 1)
                        {
                            if (args[0] is int)
                            {
                                if (!GadgetNetwork.MatrixReady && GadgetNetwork.GetTimeSinceConnect() < GadgetNetwork.MatrixTimeout)
                                {
                                    InstanceTracker.GameScript.StartCoroutine(GadgetUtils.WaitAndInvoke(RPCMethod, GadgetNetwork.MatrixTimeout - GadgetNetwork.GetTimeSinceConnect(), () => GadgetNetwork.MatrixReady, __instance, name, mode, args));
                                    return true;
                                }
                                GadgetNetwork.ConvertIDToHost(PlanetRegistry.Singleton, ref args[0]);
                            }
                        }
                        break;
                }
            }
            return true;
        }
    }
}