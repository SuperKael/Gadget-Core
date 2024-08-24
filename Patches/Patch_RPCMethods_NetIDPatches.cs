using HarmonyLib;
using GadgetCore.API;
using System.Reflection;
using UnityEngine;

namespace GadgetCore.Patches.NetIDPatches
{
    [HarmonyPatch(typeof(TurretScript))]
    [HarmonyPatch("ShootProjectile")]
    internal static class Patch_TurretScript_ShootProjectile
    {
        public static readonly MethodInfo RPCMethod = typeof(TurretScript).GetMethod("ShootProjectile", BindingFlags.Public | BindingFlags.Instance);

        [HarmonyPrefix]
        public static void Prefix(TurretScript __instance, ref int id, Vector3 targ, int dmg)
        {
            if (!GadgetNetwork.MatrixReady && GadgetNetwork.GetTimeSinceConnect() < GadgetNetwork.MatrixTimeout)
            {
                __instance.StartCoroutine(GadgetUtils.WaitAndInvoke(RPCMethod, GadgetNetwork.MatrixTimeout - GadgetNetwork.GetTimeSinceConnect(), () => GadgetNetwork.MatrixReady, __instance, id, targ, dmg));
                return;
            }
            ItemRegistry.Singleton.ConvertIDToLocal(ref id);
        }
    }

    [HarmonyPatch(typeof(SpawnerScript))]
    [HarmonyPatch("CreateWorld")]
    internal static class Patch_SpawnerScript_CreateWorld
    {
        public static readonly MethodInfo RPCMethod = typeof(SpawnerScript).GetMethod("CreateWorld", BindingFlags.Public | BindingFlags.Instance);

        [HarmonyPrefix]
        public static void Prefix(SpawnerScript __instance, ref int[] s)
        {
            if (!GadgetNetwork.MatrixReady && GadgetNetwork.GetTimeSinceConnect() < GadgetNetwork.MatrixTimeout)
            {
                __instance.StartCoroutine(GadgetUtils.WaitAndInvoke(RPCMethod, GadgetNetwork.MatrixTimeout - GadgetNetwork.GetTimeSinceConnect(), () => GadgetNetwork.MatrixReady, __instance, s));
                return;
            }
            PlanetRegistry.Singleton.ConvertIDToLocal(ref s[0]);
            for (int i = 1;i < s.Length;i++)
            {
                TileRegistry.Singleton.ConvertIDToLocal(ref s[i]);
            }
        }
    }

    [HarmonyPatch(typeof(SpawnerScript))]
    [HarmonyPatch("CreateTown")]
    internal static class Patch_SpawnerScript_CreateTown
    {
        public static readonly MethodInfo RPCMethod = typeof(SpawnerScript).GetMethod("CreateTown", BindingFlags.Public | BindingFlags.Instance);

        [HarmonyPrefix]
        public static void Prefix(SpawnerScript __instance, ref int[] s)
        {
            if (!GadgetNetwork.MatrixReady && GadgetNetwork.GetTimeSinceConnect() < GadgetNetwork.MatrixTimeout)
            {
                __instance.StartCoroutine(GadgetUtils.WaitAndInvoke(RPCMethod, GadgetNetwork.MatrixTimeout - GadgetNetwork.GetTimeSinceConnect(), () => GadgetNetwork.MatrixReady, __instance, s));
                return;
            }
            PlanetRegistry.Singleton.ConvertIDToLocal(ref s[0]);
        }
    }

    [HarmonyPatch(typeof(ProjectileMaker))]
    [HarmonyPatch("SpawnProjectile")]
    internal static class Patch_ProjectileMaker_SpawnProjectile
    {
        public static readonly MethodInfo RPCMethod = typeof(ProjectileMaker).GetMethod("SpawnProjectile", BindingFlags.Public | BindingFlags.Instance);

        [HarmonyPrefix]
        public static void Prefix(ProjectileMaker __instance, NetworkViewID viewID, ref int id, Vector3 targ, int dmg)
        {
            if (!GadgetNetwork.MatrixReady && GadgetNetwork.GetTimeSinceConnect() < GadgetNetwork.MatrixTimeout)
            {
                __instance.StartCoroutine(GadgetUtils.WaitAndInvoke(RPCMethod, GadgetNetwork.MatrixTimeout - GadgetNetwork.GetTimeSinceConnect(), () => GadgetNetwork.MatrixReady, __instance, viewID, id, targ, dmg));
                return;
            }
            ItemRegistry.Singleton.ConvertIDToLocal(ref id);
        }
    }

    [HarmonyPatch(typeof(Projectile))]
    [HarmonyPatch("SetNetworked")]
    internal static class Patch_Projectile_SetNetworked
    {
        public static readonly MethodInfo RPCMethod = typeof(Projectile).GetMethod("SetNetworked", BindingFlags.Public | BindingFlags.Instance);

        [HarmonyPrefix]
        public static void Prefix(Projectile __instance, int dmg, ref int id, Vector3 dir)
        {
            if (!GadgetNetwork.MatrixReady && GadgetNetwork.GetTimeSinceConnect() < GadgetNetwork.MatrixTimeout)
            {
                __instance.StartCoroutine(GadgetUtils.WaitAndInvoke(RPCMethod, GadgetNetwork.MatrixTimeout - GadgetNetwork.GetTimeSinceConnect(), () => GadgetNetwork.MatrixReady, __instance, dmg, id, dir));
                return;
            }
            ItemRegistry.Singleton.ConvertIDToLocal(ref id);
        }
    }

    [HarmonyPatch(typeof(PortalScript))]
    [HarmonyPatch("SetName")]
    internal static class Patch_PortalScript_SetName
    {
        public static readonly MethodInfo RPCMethod = typeof(PortalScript).GetMethod("SetName", BindingFlags.Public | BindingFlags.Instance);

        [HarmonyPrefix]
        public static void Prefix(PortalScript __instance, ref int a)
        {
            if (!GadgetNetwork.MatrixReady && GadgetNetwork.GetTimeSinceConnect() < GadgetNetwork.MatrixTimeout)
            {
                __instance.StartCoroutine(GadgetUtils.WaitAndInvoke(RPCMethod, GadgetNetwork.MatrixTimeout - GadgetNetwork.GetTimeSinceConnect(), () => GadgetNetwork.MatrixReady, __instance, a));
                return;
            }
            PlanetRegistry.Singleton.ConvertIDToLocal(ref a);
        }
    }

    [HarmonyPatch(typeof(PortalAppearance))]
    [HarmonyPatch("Set")]
    internal static class Patch_PortalAppearance_Set
    {
        public static readonly MethodInfo RPCMethod = typeof(PortalAppearance).GetMethod("Set", BindingFlags.Public | BindingFlags.Instance);

        [HarmonyPrefix]
        public static void Prefix(PortalAppearance __instance, ref int b, int h, int num)
        {
            if (!GadgetNetwork.MatrixReady && GadgetNetwork.GetTimeSinceConnect() < GadgetNetwork.MatrixTimeout)
            {
                __instance.StartCoroutine(GadgetUtils.WaitAndInvoke(RPCMethod, GadgetNetwork.MatrixTimeout - GadgetNetwork.GetTimeSinceConnect(), () => GadgetNetwork.MatrixReady, __instance, b, h, num));
                return;
            }
            PlanetRegistry.Singleton.ConvertIDToLocal(ref b);
        }
    }

    [HarmonyPatch(typeof(PlayerScript))]
    [HarmonyPatch("Staff")]
    internal static class Patch_PlayerScript_Staff
    {
        public static readonly MethodInfo RPCMethod = typeof(PlayerScript).GetMethod("Staff", BindingFlags.Public | BindingFlags.Instance);

        [HarmonyPrefix]
        public static void Prefix(PlayerScript __instance, ref int id, int dmg, NetworkPlayer pp)
        {
            if (!GadgetNetwork.MatrixReady && GadgetNetwork.GetTimeSinceConnect() < GadgetNetwork.MatrixTimeout)
            {
                __instance.StartCoroutine(GadgetUtils.WaitAndInvoke(RPCMethod, GadgetNetwork.MatrixTimeout - GadgetNetwork.GetTimeSinceConnect(), () => GadgetNetwork.MatrixReady, __instance, id, dmg, pp));
                return;
            }
            ItemRegistry.Singleton.ConvertIDToLocal(ref id);
        }
    }

    [HarmonyPatch(typeof(PlayerScript))]
    [HarmonyPatch("ShootSpecial")]
    internal static class Patch_PlayerScript_ShootSpecial
    {
        public static readonly MethodInfo RPCMethod = typeof(PlayerScript).GetMethod("ShootSpecial", BindingFlags.Public | BindingFlags.Instance);

        [HarmonyPrefix]
        public static void Prefix(PlayerScript __instance, ref int id, Vector3 targ, int dmg)
        {
            if (!GadgetNetwork.MatrixReady && GadgetNetwork.GetTimeSinceConnect() < GadgetNetwork.MatrixTimeout)
            {
                __instance.StartCoroutine(GadgetUtils.WaitAndInvoke(RPCMethod, GadgetNetwork.MatrixTimeout - GadgetNetwork.GetTimeSinceConnect(), () => GadgetNetwork.MatrixReady, __instance, id, targ, dmg));
                return;
            }
            ItemRegistry.Singleton.ConvertIDToLocal(ref id);
        }
    }

    [HarmonyPatch(typeof(PlayerScript))]
    [HarmonyPatch("ShootProjectile")]
    internal static class Patch_PlayerScript_ShootProjectile
    {
        public static readonly MethodInfo RPCMethod = typeof(PlayerScript).GetMethod("ShootProjectile", BindingFlags.Public | BindingFlags.Instance);

        [HarmonyPrefix]
        public static void Prefix(PlayerScript __instance, ref int id, Vector3 targ, int dmg)
        {
            if (!GadgetNetwork.MatrixReady && GadgetNetwork.GetTimeSinceConnect() < GadgetNetwork.MatrixTimeout)
            {
                __instance.StartCoroutine(GadgetUtils.WaitAndInvoke(RPCMethod, GadgetNetwork.MatrixTimeout - GadgetNetwork.GetTimeSinceConnect(), () => GadgetNetwork.MatrixReady, __instance, id, targ, dmg));
                return;
            }
            ItemRegistry.Singleton.ConvertIDToLocal(ref id);
        }

        [HarmonyPostfix]
        public static void Postfix()
        {

        }
    }

    [HarmonyPatch(typeof(PlayerScript))]
    [HarmonyPatch("ShootProjectile2")]
    internal static class Patch_PlayerScript_ShootProjectile2
    {
        public static readonly MethodInfo RPCMethod = typeof(PlayerScript).GetMethod("ShootProjectile2", BindingFlags.Public | BindingFlags.Instance);

        [HarmonyPrefix]
        public static void Prefix(PlayerScript __instance, ref int id, Vector3 targ, int dmg, Vector3 burst)
        {
            if (!GadgetNetwork.MatrixReady && GadgetNetwork.GetTimeSinceConnect() < GadgetNetwork.MatrixTimeout)
            {
                __instance.StartCoroutine(GadgetUtils.WaitAndInvoke(RPCMethod, GadgetNetwork.MatrixTimeout - GadgetNetwork.GetTimeSinceConnect(), () => GadgetNetwork.MatrixReady, __instance, id, targ, dmg, burst));
                return;
            }
            ItemRegistry.Singleton.ConvertIDToLocal(ref id);
        }

        [HarmonyPostfix]
        public static void Postfix()
        {

        }
    }

    [HarmonyPatch(typeof(PlayerScript))]
    [HarmonyPatch("NetworkedProjectile")]
    internal static class Patch_PlayerScript_NetworkedProjectile
    {
        public static readonly MethodInfo RPCMethod = typeof(PlayerScript).GetMethod("NetworkedProjectile", BindingFlags.Public | BindingFlags.Instance);

        [HarmonyPrefix]
        public static void Prefix(PlayerScript __instance, ref int id, int dmg, NetworkPlayer pp, Vector3 dir)
        {
            if (!GadgetNetwork.MatrixReady && GadgetNetwork.GetTimeSinceConnect() < GadgetNetwork.MatrixTimeout)
            {
                __instance.StartCoroutine(GadgetUtils.WaitAndInvoke(RPCMethod, GadgetNetwork.MatrixTimeout - GadgetNetwork.GetTimeSinceConnect(), () => GadgetNetwork.MatrixReady, __instance, id, dmg, pp, dir));
                return;
            }
            ItemRegistry.Singleton.ConvertIDToLocal(ref id);
        }
    }

    [HarmonyPatch(typeof(PlayerAppearance))]
    [HarmonyPatch("UA")]
    internal static class Patch_PlayerAppearance_UA
    {
        public static readonly MethodInfo RPCMethod = typeof(PlayerAppearance).GetMethod("UA", BindingFlags.Public | BindingFlags.Instance);

        [HarmonyPrefix]
        public static void Prefix(PlayerAppearance __instance, ref int[] ei, int a, bool dead)
        {
            if (!GadgetNetwork.MatrixReady && GadgetNetwork.GetTimeSinceConnect() < GadgetNetwork.MatrixTimeout)
            {
                __instance.StartCoroutine(GadgetUtils.WaitAndInvoke(RPCMethod, GadgetNetwork.MatrixTimeout - GadgetNetwork.GetTimeSinceConnect(), () => GadgetNetwork.MatrixReady, __instance, ei, a, dead));
                return;
            }
            ItemRegistry.Singleton.ConvertIDToLocal(ref ei[0]);
            ItemRegistry.Singleton.ConvertIDToLocal(ref ei[1]);
            ItemRegistry.Singleton.ConvertIDToLocal(ref ei[2]);
            ItemRegistry.Singleton.ConvertIDToLocal(ref ei[3]);
            CharacterRaceRegistry.Singleton.ConvertIDToLocal(ref ei[4]);
            CharacterUniformRegistry.Singleton.ConvertIDToLocal(ref ei[6]);
            CharacterAugmentRegistry.Singleton.ConvertIDToLocal(ref ei[7]);
        }
    }

    [HarmonyPatch(typeof(KylockeStand))]
    [HarmonyPatch("Set")]
    internal static class Patch_KylockeStand_Set
    {
        public static readonly MethodInfo RPCMethod = typeof(KylockeStand).GetMethod("Set", BindingFlags.Public | BindingFlags.Instance);


        [HarmonyPrefix]
        public static void Prefix(KylockeStand __instance, ref int[] p)
        {
            if (!GadgetNetwork.MatrixReady && GadgetNetwork.GetTimeSinceConnect() < GadgetNetwork.MatrixTimeout)
            {
                __instance.StartCoroutine(GadgetUtils.WaitAndInvoke(RPCMethod, GadgetNetwork.MatrixTimeout - GadgetNetwork.GetTimeSinceConnect(), () => GadgetNetwork.MatrixReady, __instance, p));
                return;
            }
            ItemRegistry.Singleton.ConvertIDToLocal(ref p[0]);
        }
    }

    [HarmonyPatch(typeof(ItemStandScript))]
    [HarmonyPatch("Set")]
    internal static class Patch_ItemStandScript_Set
    {
        public static readonly MethodInfo RPCMethod = typeof(ItemStandScript).GetMethod("Set", BindingFlags.Public | BindingFlags.Instance);

        [HarmonyPrefix]
        public static void Prefix(ItemStandScript __instance, ref int[] p)
        {
            if (!GadgetNetwork.MatrixReady && GadgetNetwork.GetTimeSinceConnect() < GadgetNetwork.MatrixTimeout)
            {
                __instance.StartCoroutine(GadgetUtils.WaitAndInvoke(RPCMethod, GadgetNetwork.MatrixTimeout - GadgetNetwork.GetTimeSinceConnect(), () => GadgetNetwork.MatrixReady, __instance, p));
                return;
            }
            (__instance.isChipStand ? (Registry)ChipRegistry.Singleton : ItemRegistry.Singleton).ConvertIDToLocal(ref p[0]);
        }
    }

    [HarmonyPatch(typeof(ItemScript))]
    [HarmonyPatch("Chip")]
    internal static class Patch_ItemScript_Chip
    {
        public static readonly MethodInfo RPCMethod = typeof(ItemScript).GetMethod("Chip", BindingFlags.Public | BindingFlags.Instance);

        [HarmonyPrefix]
        public static void Prefix(ItemScript __instance, ref int id)
        {
            if (!GadgetNetwork.MatrixReady && GadgetNetwork.GetTimeSinceConnect() < GadgetNetwork.MatrixTimeout)
            {
                __instance.StartCoroutine(GadgetUtils.WaitAndInvoke(RPCMethod, GadgetNetwork.MatrixTimeout - GadgetNetwork.GetTimeSinceConnect(), () => GadgetNetwork.MatrixReady, __instance, id));
                return;
            }
            ChipRegistry.Singleton.ConvertIDToLocal(ref id);
        }
    }

    [HarmonyPatch(typeof(GameScript))]
    [HarmonyPatch("SpawnItem")]
    internal static class Patch_GameScript_SpawnItem
    {
        public static readonly MethodInfo RPCMethod = typeof(GameScript).GetMethod("SpawnItem", BindingFlags.Public | BindingFlags.Instance);

        [HarmonyPrefix]
        public static void Prefix(GameScript __instance, ref int[] st, Vector3 pos)
        {
            if (!GadgetNetwork.MatrixReady && GadgetNetwork.GetTimeSinceConnect() < GadgetNetwork.MatrixTimeout)
            {
                __instance.StartCoroutine(GadgetUtils.WaitAndInvoke(RPCMethod, GadgetNetwork.MatrixTimeout - GadgetNetwork.GetTimeSinceConnect(), () => GadgetNetwork.MatrixReady, __instance, st, pos));
                return;
            }
            ItemRegistry.Singleton.ConvertIDToLocal(ref st[0]);
            ItemRegistry.Singleton.ConvertIDToLocal(ref st[5]);
            ItemRegistry.Singleton.ConvertIDToLocal(ref st[6]);
            ItemRegistry.Singleton.ConvertIDToLocal(ref st[7]);
        }
    }

    [HarmonyPatch(typeof(GameScript))]
    [HarmonyPatch("ChangePortal")]
    internal static class Patch_GameScript_ChangePortal
    {
        public static readonly MethodInfo RPCMethod = typeof(GameScript).GetMethod("ChangePortal", BindingFlags.Public | BindingFlags.Instance);

        [HarmonyPrefix]
        public static void Prefix(GameScript __instance, ref int a, int challenge)
        {
            if (!GadgetNetwork.MatrixReady && GadgetNetwork.GetTimeSinceConnect() < GadgetNetwork.MatrixTimeout)
            {
                __instance.StartCoroutine(GadgetUtils.WaitAndInvoke(RPCMethod, GadgetNetwork.MatrixTimeout - GadgetNetwork.GetTimeSinceConnect(), () => GadgetNetwork.MatrixReady, __instance, a, challenge));
                return;
            }
            PlanetRegistry.Singleton.ConvertIDToLocal(ref a);
        }
    }

    [HarmonyPatch(typeof(GameScript))]
    [HarmonyPatch("ChangePortal2")]
    internal static class Patch_GameScript_ChangePortal2
    {
        public static readonly MethodInfo RPCMethod = typeof(GameScript).GetMethod("ChangePortal2", BindingFlags.Public | BindingFlags.Instance);

        [HarmonyPrefix]
        public static void Prefix(GameScript __instance, ref int a, int challenge)
        {
            if (!GadgetNetwork.MatrixReady && GadgetNetwork.GetTimeSinceConnect() < GadgetNetwork.MatrixTimeout)
            {
                __instance.StartCoroutine(GadgetUtils.WaitAndInvoke(RPCMethod, GadgetNetwork.MatrixTimeout - GadgetNetwork.GetTimeSinceConnect(), () => GadgetNetwork.MatrixReady, __instance, a, challenge));
                return;
            }
            PlanetRegistry.Singleton.ConvertIDToLocal(ref a);
        }
    }

    [HarmonyPatch(typeof(DroidScript))]
    [HarmonyPatch("ShootProjectile")]
    internal static class Patch_DroidScript_ShootProjectile
    {
        public static readonly MethodInfo RPCMethod = typeof(DroidScript).GetMethod("ShootProjectile", BindingFlags.Public | BindingFlags.Instance);

        [HarmonyPrefix]
        public static void Prefix(DroidScript __instance, ref int id, Vector3 targ, int dmg)
        {
            if (!GadgetNetwork.MatrixReady && GadgetNetwork.GetTimeSinceConnect() < GadgetNetwork.MatrixTimeout)
            {
                __instance.StartCoroutine(GadgetUtils.WaitAndInvoke(RPCMethod, GadgetNetwork.MatrixTimeout - GadgetNetwork.GetTimeSinceConnect(), () => GadgetNetwork.MatrixReady, __instance, id, targ, dmg));
                return;
            }
            ItemRegistry.Singleton.ConvertIDToLocal(ref id);
        }
    }

    [HarmonyPatch(typeof(DroidManager))]
    [HarmonyPatch("UA")]
    internal static class Patch_DroidManager_UA
    {
        public static readonly MethodInfo RPCMethod = typeof(DroidManager).GetMethod("UA", BindingFlags.Public | BindingFlags.Instance);

        [HarmonyPrefix]
        public static void Prefix(DroidManager __instance, ref int[] d)
        {
            if (!GadgetNetwork.MatrixReady && GadgetNetwork.GetTimeSinceConnect() < GadgetNetwork.MatrixTimeout)
            {
                __instance.StartCoroutine(GadgetUtils.WaitAndInvoke(RPCMethod, GadgetNetwork.MatrixTimeout - GadgetNetwork.GetTimeSinceConnect(), () => GadgetNetwork.MatrixReady, __instance, d));
                d = new[] { 1000, 1000, 1000 }; // Fix to bug with host droids in multiplayer
                return;
            }
            ItemRegistry.Singleton.ConvertIDToLocal(ref d[0]);
            ItemRegistry.Singleton.ConvertIDToLocal(ref d[1]);
            ItemRegistry.Singleton.ConvertIDToLocal(ref d[2]);
        }
    }

    [HarmonyPatch(typeof(ChunkWorld))]
    [HarmonyPatch("RefreshWall")]
    internal static class Patch_ChunkWorld_RefreshWall
    {
        public static readonly MethodInfo RPCMethod = typeof(ChunkWorld).GetMethod("RefreshWall", BindingFlags.Public | BindingFlags.Instance);

        [HarmonyPrefix]
        public static void Prefix(ChunkWorld __instance, ref int[] gg)
        {
            if (!GadgetNetwork.MatrixReady && GadgetNetwork.GetTimeSinceConnect() < GadgetNetwork.MatrixTimeout)
            {
                __instance.StartCoroutine(GadgetUtils.WaitAndInvoke(RPCMethod, GadgetNetwork.MatrixTimeout - GadgetNetwork.GetTimeSinceConnect(), () => GadgetNetwork.MatrixReady, __instance, gg));
                return;
            }
            TileRegistry.Singleton.ConvertIDsToLocal(ref gg);
        }
    }

    [HarmonyPatch(typeof(ChunkWorld))]
    [HarmonyPatch("RefreshShip")]
    internal static class Patch_ChunkWorld_RefreshShip
    {
        public static readonly MethodInfo RPCMethod = typeof(ChunkWorld).GetMethod("RefreshShip", BindingFlags.Public | BindingFlags.Instance);

        [HarmonyPrefix]
        public static void Prefix(ChunkWorld __instance, ref int[] gg, ref int[] ggs)
        {
            if (!GadgetNetwork.MatrixReady && GadgetNetwork.GetTimeSinceConnect() < GadgetNetwork.MatrixTimeout)
            {
                __instance.StartCoroutine(GadgetUtils.WaitAndInvoke(RPCMethod, GadgetNetwork.MatrixTimeout - GadgetNetwork.GetTimeSinceConnect(), () => GadgetNetwork.MatrixReady, __instance, gg, ggs));
                return;
            }
            TileRegistry.Singleton.ConvertIDsToLocal(ref gg);
            TileRegistry.Singleton.ConvertIDsToLocal(ref ggs);
        }
    }

    [HarmonyPatch(typeof(Chunk))]
    [HarmonyPatch("SetMaterial")]
    internal static class Patch_Chunk_SetMaterial
    {
        public static readonly MethodInfo RPCMethod = typeof(Chunk).GetMethod("SetMaterial", BindingFlags.Public | BindingFlags.Instance);

        [HarmonyPrefix]
        public static void Prefix(Chunk __instance, ref int a)
        {
            if (!GadgetNetwork.MatrixReady && GadgetNetwork.GetTimeSinceConnect() < GadgetNetwork.MatrixTimeout)
            {
                __instance.StartCoroutine(GadgetUtils.WaitAndInvoke(RPCMethod, GadgetNetwork.MatrixTimeout - GadgetNetwork.GetTimeSinceConnect(), () => GadgetNetwork.MatrixReady, __instance, a));
                return;
            }
            PlanetRegistry.Singleton.ConvertIDToLocal(ref a);
        }
    }
}