using HarmonyLib;
using GadgetCore.API;
using UnityEngine;
using System.Reflection;
using System;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(NetworkView))]
    [HarmonyPatch("RPC")]
    [HarmonyPatch(new[] { typeof(string), typeof(RPCMode), typeof(object[]) })]
    internal static class Patch_NetworkView_RPC
    {
        public static readonly MethodInfo RPCMethod = typeof(NetworkView).GetMethod("RPC", new[] { typeof(string), typeof(RPCMode), typeof(object[]) });

        [HarmonyPrefix]
        public static void Prefix(NetworkView __instance, string name, RPCMode mode, ref object[] args)
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
                                    return;
                                }
                                PlanetRegistry.Singleton.ConvertIDToHost(ref args[0]);
                            }
                        }
                        else if (args.Length == 1)
                        {
                            if (args[0] is int[] arr && arr.Length == 2)
                            {
                                if (!GadgetNetwork.MatrixReady && GadgetNetwork.GetTimeSinceConnect() < GadgetNetwork.MatrixTimeout)
                                {
                                    InstanceTracker.GameScript.StartCoroutine(GadgetUtils.WaitAndInvoke(RPCMethod, GadgetNetwork.MatrixTimeout - GadgetNetwork.GetTimeSinceConnect(), () => GadgetNetwork.MatrixReady, __instance, name, mode, args));
                                    return;
                                }
                                ItemStandScript itemStandScript = __instance.GetComponent<ItemStandScript>();
                                if (itemStandScript == null || !itemStandScript.isChipStand)
                                {
                                    args[0] = new[]
                                    {
                                        ItemRegistry.Singleton.ConvertIDToHost(arr[0]),
                                        arr[1],
                                    };
                                }
                                else
                                {
                                    args[0] = new[]
                                    {
                                        ChipRegistry.Singleton.ConvertIDToHost(arr[0]),
                                        arr[1],
                                    };
                                }
                            }
                            else if (args[0] is Package2 pack2)
                            {
                                if (!GadgetNetwork.MatrixReady && GadgetNetwork.GetTimeSinceConnect() < GadgetNetwork.MatrixTimeout)
                                {
                                    InstanceTracker.GameScript.StartCoroutine(GadgetUtils.WaitAndInvoke(RPCMethod, GadgetNetwork.MatrixTimeout - GadgetNetwork.GetTimeSinceConnect(), () => GadgetNetwork.MatrixReady, __instance, name, mode, args));
                                    return;
                                }
                                ItemRegistry.Singleton.ConvertIDToHost(ref pack2.id);
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
                                    return;
                                }
                                args[0] = new[]
                                {
                                    ItemRegistry.Singleton.ConvertIDToHost(arr[0]),
                                    ItemRegistry.Singleton.ConvertIDToHost(arr[1]),
                                    ItemRegistry.Singleton.ConvertIDToHost(arr[2]),
                                    ItemRegistry.Singleton.ConvertIDToHost(arr[3]),
                                    CharacterRaceRegistry.Singleton.ConvertIDToHost(arr[4]),
                                    arr[5],
                                    CharacterUniformRegistry.Singleton.ConvertIDToHost(arr[6]),
                                    CharacterAugmentRegistry.Singleton.ConvertIDToHost(arr[7])
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
                                    args[0] = new[] { 1000, 1000, 1000 }; // Fix to bug with host droids in multiplayer
                                    return;
                                }
                                args[0] = new[]
                                {
                                    ItemRegistry.Singleton.ConvertIDToHost(arr[0]),
                                    ItemRegistry.Singleton.ConvertIDToHost(arr[1]),
                                    ItemRegistry.Singleton.ConvertIDToHost(arr[2])
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
                                    return;
                                }
                                ItemRegistry.Singleton.ConvertIDToHost(ref args[0]);
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
                                    return;
                                }
                                int[] newArr = new int[arr.Length];
                                Array.Copy(arr, newArr, arr.Length);
                                PlanetRegistry.Singleton.ConvertIDToHost(ref newArr[0]);
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
                                    return;
                                }
                                args[0] = new[]
                                {
                                    PlanetRegistry.Singleton.ConvertIDToHost(arr[0]),
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
                                    return;
                                }
                                ItemRegistry.Singleton.ConvertIDToHost(ref args[1]);
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
                                    return;
                                }
                                ItemRegistry.Singleton.ConvertIDToHost(ref args[1]);
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
                                    return;
                                }
                                PlanetRegistry.Singleton.ConvertIDToHost(ref args[0]);
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
                                    return;
                                }
                                ItemRegistry.Singleton.ConvertIDToHost(ref args[0]);
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
                                    return;
                                }
                                ItemRegistry.Singleton.ConvertIDToHost(ref args[0]);
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
                                    return;
                                }
                                ItemRegistry.Singleton.ConvertIDToHost(ref args[0]);
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
                                    return;
                                }
                                ItemRegistry.Singleton.ConvertIDToHost(ref args[0]);
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
                                    return;
                                }
                                int[] newArr = new int[arr.Length];
                                Array.Copy(arr, newArr, arr.Length);
                                ItemRegistry.Singleton.ConvertIDToHost(ref newArr[0]);
                                ItemRegistry.Singleton.ConvertIDToHost(ref newArr[5]);
                                ItemRegistry.Singleton.ConvertIDToHost(ref newArr[6]);
                                ItemRegistry.Singleton.ConvertIDToHost(ref newArr[7]);
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
                                    return;
                                }
                                ChipRegistry.Singleton.ConvertIDToHost(ref args[0]);
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
                                    return;
                                }
                                int[] newArr = new int[arr.Length];
                                Array.Copy(arr, newArr, arr.Length);
                                ItemRegistry.Singleton.ConvertIDToHost(ref newArr[0]);
                                ItemRegistry.Singleton.ConvertIDToHost(ref newArr[5]);
                                ItemRegistry.Singleton.ConvertIDToHost(ref newArr[6]);
                                ItemRegistry.Singleton.ConvertIDToHost(ref newArr[7]);
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
                                    return;
                                }
                                PlanetRegistry.Singleton.ConvertIDToHost(ref args[0]);
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
                                    return;
                                }
                                PlanetRegistry.Singleton.ConvertIDToHost(ref args[0]);
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
                                    return;
                                }
                                args[0] = TileRegistry.Singleton.ConvertIDsToHost(arr);
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
                                    return;
                                }
                                args[0] = TileRegistry.Singleton.ConvertIDsToHost(arr1);
                                args[1] = TileRegistry.Singleton.ConvertIDsToHost(arr2);
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
                                    return;
                                }
                                PlanetRegistry.Singleton.ConvertIDToHost(ref args[0]);
                            }
                        }
                        break;
                }
            }
        }
    }
}