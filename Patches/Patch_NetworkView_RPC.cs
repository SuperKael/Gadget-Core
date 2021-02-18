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
                                    return false;
                                }
                                GadgetNetwork.ConvertIDToHost(PlanetRegistry.Singleton, ref args[0]);
                            }
                        }
                        else if (args.Length == 1)
                        {
                            if (args[0] is int[] && (args[0] as int[]).Length == 2)
                            {
                                if (!GadgetNetwork.MatrixReady && GadgetNetwork.GetTimeSinceConnect() < GadgetNetwork.MatrixTimeout)
                                {
                                    InstanceTracker.GameScript.StartCoroutine(GadgetUtils.WaitAndInvoke(RPCMethod, GadgetNetwork.MatrixTimeout - GadgetNetwork.GetTimeSinceConnect(), () => GadgetNetwork.MatrixReady, __instance, name, mode, args));
                                    return false;
                                }
                                ItemStandScript itemStandScript = __instance.GetComponent<ItemStandScript>();
                                if (itemStandScript == null || !itemStandScript.isChipStand)
                                {
                                    GadgetNetwork.ConvertIDToHost(ItemRegistry.Singleton, ref (args[0] as int[])[0]);
                                }
                                else
                                {
                                    GadgetNetwork.ConvertIDToHost(ChipRegistry.Singleton, ref (args[0] as int[])[0]);
                                }
                            }
                            else if (args[0] is Package2)
                            {
                                if (!GadgetNetwork.MatrixReady && GadgetNetwork.GetTimeSinceConnect() < GadgetNetwork.MatrixTimeout)
                                {
                                    InstanceTracker.GameScript.StartCoroutine(GadgetUtils.WaitAndInvoke(RPCMethod, GadgetNetwork.MatrixTimeout - GadgetNetwork.GetTimeSinceConnect(), () => GadgetNetwork.MatrixReady, __instance, name, mode, args));
                                    return false;
                                }
                                GadgetNetwork.ConvertIDToHost(ItemRegistry.Singleton /* ProjectileRegistry */, ref (args[0] as Package2).id);
                            }
                        }
                        break;
                    case "UA":
                        if (args.Length == 3)
                        {
                            if (args[0] is int[] && (args[0] as int[]).Length == 8)
                            {
                                if (!GadgetNetwork.MatrixReady && GadgetNetwork.GetTimeSinceConnect() < GadgetNetwork.MatrixTimeout)
                                {
                                    InstanceTracker.GameScript.StartCoroutine(GadgetUtils.WaitAndInvoke(RPCMethod, GadgetNetwork.MatrixTimeout - GadgetNetwork.GetTimeSinceConnect(), () => GadgetNetwork.MatrixReady, __instance, name, mode, args));
                                    return false;
                                }
                                GadgetNetwork.ConvertIDToHost(ItemRegistry.Singleton, ref (args[0] as int[])[0]);
                                GadgetNetwork.ConvertIDToHost(ItemRegistry.Singleton, ref (args[0] as int[])[1]);
                                GadgetNetwork.ConvertIDToHost(ItemRegistry.Singleton, ref (args[0] as int[])[2]);
                                GadgetNetwork.ConvertIDToHost(ItemRegistry.Singleton, ref (args[0] as int[])[3]);
                                GadgetNetwork.ConvertIDToHost(null /* RaceRegistry */, ref (args[0] as int[])[4]);
                                GadgetNetwork.ConvertIDToHost(null /* UniformRegistry */, ref (args[0] as int[])[6]);
                                GadgetNetwork.ConvertIDToHost(null /* AugmentRegistry */, ref (args[0] as int[])[7]);
                            }
                        }
                        else if (args.Length == 1)
                        {
                            if (args[0] is int[] && (args[0] as int[]).Length == 3)
                            {
                                if (!GadgetNetwork.MatrixReady && GadgetNetwork.GetTimeSinceConnect() < GadgetNetwork.MatrixTimeout)
                                {
                                    InstanceTracker.GameScript.StartCoroutine(GadgetUtils.WaitAndInvoke(RPCMethod, GadgetNetwork.MatrixTimeout - GadgetNetwork.GetTimeSinceConnect(), () => GadgetNetwork.MatrixReady, __instance, name, mode, args));
                                    return false;
                                }
                                GadgetNetwork.ConvertIDToHost(ItemRegistry.Singleton, ref (args[0] as int[])[0]);
                                GadgetNetwork.ConvertIDToHost(ItemRegistry.Singleton, ref (args[0] as int[])[1]);
                                GadgetNetwork.ConvertIDToHost(ItemRegistry.Singleton, ref (args[0] as int[])[2]);
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
                                    return false;
                                }
                                GadgetNetwork.ConvertIDToHost(ItemRegistry.Singleton /* ProjectileRegistry */, ref args[0]);
                            }
                        }
                        break;
                    case "CreateWorld":
                        if (args.Length == 1)
                        {
                            if (args[0] is int[] && (args[0] as int[]).Length > 1)
                            {
                                if (!GadgetNetwork.MatrixReady && GadgetNetwork.GetTimeSinceConnect() < GadgetNetwork.MatrixTimeout)
                                {
                                    InstanceTracker.GameScript.StartCoroutine(GadgetUtils.WaitAndInvoke(RPCMethod, GadgetNetwork.MatrixTimeout - GadgetNetwork.GetTimeSinceConnect(), () => GadgetNetwork.MatrixReady, __instance, name, mode, args));
                                    return false;
                                }
                                GadgetNetwork.ConvertIDToHost(PlanetRegistry.Singleton, ref (args[0] as int[])[0]);
                                for (int i = 1; i < (args[0] as int[]).Length; i++)
                                {
                                    GadgetNetwork.ConvertIDToHost(TileRegistry.Singleton, ref (args[0] as int[])[i]);
                                }
                            }
                        }
                        break;
                    case "CreateTown":
                        if (args.Length == 1)
                        {
                            if (args[0] is int[] && (args[0] as int[]).Length == 2)
                            {
                                if (!GadgetNetwork.MatrixReady && GadgetNetwork.GetTimeSinceConnect() < GadgetNetwork.MatrixTimeout)
                                {
                                    InstanceTracker.GameScript.StartCoroutine(GadgetUtils.WaitAndInvoke(RPCMethod, GadgetNetwork.MatrixTimeout - GadgetNetwork.GetTimeSinceConnect(), () => GadgetNetwork.MatrixReady, __instance, name, mode, args));
                                    return false;
                                }
                                GadgetNetwork.ConvertIDToHost(PlanetRegistry.Singleton, ref (args[0] as int[])[0]);
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
                                    return false;
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
                                    return false;
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
                                    return false;
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
                                    return false;
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
                                    return false;
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
                                    return false;
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
                                    return false;
                                }
                                GadgetNetwork.ConvertIDToHost(ItemRegistry.Singleton /* ProjectileRegistry */, ref args[0]);
                            }
                        }
                        break;
                    case "Init":
                        if (args.Length == 1)
                        {
                            if (args[0] is int[] && (args[0] as int[]).Length >= 11)
                            {
                                if (!GadgetNetwork.MatrixReady && GadgetNetwork.GetTimeSinceConnect() < GadgetNetwork.MatrixTimeout)
                                {
                                    InstanceTracker.GameScript.StartCoroutine(GadgetUtils.WaitAndInvoke(RPCMethod, GadgetNetwork.MatrixTimeout - GadgetNetwork.GetTimeSinceConnect(), () => GadgetNetwork.MatrixReady, __instance, name, mode, args));
                                    return false;
                                }
                                GadgetNetwork.ConvertIDToHost(ItemRegistry.Singleton, ref (args[0] as int[])[0]);
                                GadgetNetwork.ConvertIDToHost(ItemRegistry.Singleton, ref (args[0] as int[])[5]);
                                GadgetNetwork.ConvertIDToHost(ItemRegistry.Singleton, ref (args[0] as int[])[6]);
                                GadgetNetwork.ConvertIDToHost(ItemRegistry.Singleton, ref (args[0] as int[])[7]);
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
                                    return false;
                                }
                                GadgetNetwork.ConvertIDToHost(ChipRegistry.Singleton, ref args[0]);
                            }
                        }
                        break;
                    case "SpawnItem":
                        if (args.Length == 2)
                        {
                            if (args[0] is int[] && (args[0] as int[]).Length >= 11)
                            {
                                if (!GadgetNetwork.MatrixReady && GadgetNetwork.GetTimeSinceConnect() < GadgetNetwork.MatrixTimeout)
                                {
                                    InstanceTracker.GameScript.StartCoroutine(GadgetUtils.WaitAndInvoke(RPCMethod, GadgetNetwork.MatrixTimeout - GadgetNetwork.GetTimeSinceConnect(), () => GadgetNetwork.MatrixReady, __instance, name, mode, args));
                                    return false;
                                }
                                GadgetNetwork.ConvertIDToHost(ItemRegistry.Singleton, ref (args[0] as int[])[0]);
                                GadgetNetwork.ConvertIDToHost(ItemRegistry.Singleton, ref (args[0] as int[])[5]);
                                GadgetNetwork.ConvertIDToHost(ItemRegistry.Singleton, ref (args[0] as int[])[6]);
                                GadgetNetwork.ConvertIDToHost(ItemRegistry.Singleton, ref (args[0] as int[])[7]);
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
                                    return false;
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
                                    return false;
                                }
                                GadgetNetwork.ConvertIDToHost(PlanetRegistry.Singleton, ref args[0]);
                            }
                        }
                        break;
                    case "RefreshWall":
                        if (args.Length == 1)
                        {
                            if (args[0] is int[])
                            {
                                for (int i = 0;i < (args[0] as int[]).Length;i++)
                                {
                                    if (!GadgetNetwork.MatrixReady && GadgetNetwork.GetTimeSinceConnect() < GadgetNetwork.MatrixTimeout)
                                    {
                                        InstanceTracker.GameScript.StartCoroutine(GadgetUtils.WaitAndInvoke(RPCMethod, GadgetNetwork.MatrixTimeout - GadgetNetwork.GetTimeSinceConnect(), () => GadgetNetwork.MatrixReady, __instance, name, mode, args));
                                        return false;
                                    }
                                    GadgetNetwork.ConvertIDToHost(TileRegistry.Singleton, ref (args[0] as int[])[i]);
                                }
                            }
                        }
                        break;
                    case "RefreshShip":
                        if (args.Length == 2)
                        {
                            if (args[0] is int[] && args[1] is int[])
                            {
                                for (int i = 0; i < (args[0] as int[]).Length; i++)
                                {
                                    if (!GadgetNetwork.MatrixReady && GadgetNetwork.GetTimeSinceConnect() < GadgetNetwork.MatrixTimeout)
                                    {
                                        InstanceTracker.GameScript.StartCoroutine(GadgetUtils.WaitAndInvoke(RPCMethod, GadgetNetwork.MatrixTimeout - GadgetNetwork.GetTimeSinceConnect(), () => GadgetNetwork.MatrixReady, __instance, name, mode, args));
                                        return false;
                                    }
                                    GadgetNetwork.ConvertIDToHost(TileRegistry.Singleton, ref (args[0] as int[])[i]);
                                }
                                for (int i = 0; i < (args[1] as int[]).Length; i++)
                                {
                                    if (!GadgetNetwork.MatrixReady && GadgetNetwork.GetTimeSinceConnect() < GadgetNetwork.MatrixTimeout)
                                    {
                                        InstanceTracker.GameScript.StartCoroutine(GadgetUtils.WaitAndInvoke(RPCMethod, GadgetNetwork.MatrixTimeout - GadgetNetwork.GetTimeSinceConnect(), () => GadgetNetwork.MatrixReady, __instance, name, mode, args));
                                        return false;
                                    }
                                    GadgetNetwork.ConvertIDToHost(TileRegistry.Singleton, ref (args[1] as int[])[i]);
                                }
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
                                    return false;
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