using HarmonyLib;
using GadgetCore.API;
using System.Reflection;
using UnityEngine;

namespace GadgetCore.Patches.NetIDPatches
{
    [HarmonyPatch(typeof(TurretScript))]
    [HarmonyPatch("ShootProjectile")]
    static class Patch_TurretScript_ShootProjectile
    {
        public static readonly MethodInfo RPCMethod = typeof(TurretScript).GetMethod("ShootProjectile", BindingFlags.Public | BindingFlags.Instance);

        [HarmonyPrefix]
        public static bool Prefix(TurretScript __instance, ref int id, Vector3 targ, int dmg)
        {
            if (!GadgetNetwork.MatrixReady && GadgetNetwork.GetTimeSinceConnect() < GadgetNetwork.MatrixTimeout)
            {
                __instance.StartCoroutine(GadgetUtils.WaitAndInvoke(RPCMethod, GadgetNetwork.MatrixTimeout - GadgetNetwork.GetTimeSinceConnect(), () => GadgetNetwork.MatrixReady, __instance, id, targ, dmg));
                return true;
            }
            GadgetNetwork.ConvertIDToLocal(ItemRegistry.Singleton /* ProjectileRegistry */, ref id);
            return true;
        }
    }

    [HarmonyPatch(typeof(SpawnerScript))]
    [HarmonyPatch("CreateWorld")]
    static class Patch_SpawnerScript_CreateWorld
    {
        public static readonly MethodInfo RPCMethod = typeof(SpawnerScript).GetMethod("CreateWorld", BindingFlags.Public | BindingFlags.Instance);

        [HarmonyPrefix]
        public static bool Prefix(SpawnerScript __instance, ref int[] s)
        {
            if (!GadgetNetwork.MatrixReady && GadgetNetwork.GetTimeSinceConnect() < GadgetNetwork.MatrixTimeout)
            {
                __instance.StartCoroutine(GadgetUtils.WaitAndInvoke(RPCMethod, GadgetNetwork.MatrixTimeout - GadgetNetwork.GetTimeSinceConnect(), () => GadgetNetwork.MatrixReady, __instance, s));
                return true;
            }
            GadgetNetwork.ConvertIDToLocal(PlanetRegistry.Singleton, ref s[0]);
            for (int i = 1;i < s.Length;i++)
            {
                GadgetNetwork.ConvertIDToLocal(TileRegistry.Singleton, ref s[i]);
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(SpawnerScript))]
    [HarmonyPatch("CreateTown")]
    static class Patch_SpawnerScript_CreateTown
    {
        public static readonly MethodInfo RPCMethod = typeof(SpawnerScript).GetMethod("CreateTown", BindingFlags.Public | BindingFlags.Instance);

        [HarmonyPrefix]
        public static bool Prefix(SpawnerScript __instance, ref int[] s)
        {
            if (!GadgetNetwork.MatrixReady && GadgetNetwork.GetTimeSinceConnect() < GadgetNetwork.MatrixTimeout)
            {
                __instance.StartCoroutine(GadgetUtils.WaitAndInvoke(RPCMethod, GadgetNetwork.MatrixTimeout - GadgetNetwork.GetTimeSinceConnect(), () => GadgetNetwork.MatrixReady, __instance, s));
                return true;
            }
            GadgetNetwork.ConvertIDToLocal(PlanetRegistry.Singleton, ref s[0]);
            return true;
        }
    }

    [HarmonyPatch(typeof(ProjectileMaker))]
    [HarmonyPatch("SpawnProjectile")]
    static class Patch_ProjectileMaker_SpawnProjectile
    {
        public static readonly MethodInfo RPCMethod = typeof(ProjectileMaker).GetMethod("SpawnProjectile", BindingFlags.Public | BindingFlags.Instance);

        [HarmonyPrefix]
        public static bool Prefix(ProjectileMaker __instance, NetworkViewID viewID, ref int id, Vector3 targ, int dmg)
        {
            if (!GadgetNetwork.MatrixReady && GadgetNetwork.GetTimeSinceConnect() < GadgetNetwork.MatrixTimeout)
            {
                __instance.StartCoroutine(GadgetUtils.WaitAndInvoke(RPCMethod, GadgetNetwork.MatrixTimeout - GadgetNetwork.GetTimeSinceConnect(), () => GadgetNetwork.MatrixReady, __instance, viewID, id, targ, dmg));
                return true;
            }
            GadgetNetwork.ConvertIDToLocal(ItemRegistry.Singleton /* ProjectileRegistry */, ref id);
            return true;
        }
    }

    [HarmonyPatch(typeof(Projectile))]
    [HarmonyPatch("SetNetworked")]
    static class Patch_Projectile_SetNetworked
    {
        public static readonly MethodInfo RPCMethod = typeof(Projectile).GetMethod("SetNetworked", BindingFlags.Public | BindingFlags.Instance);

        [HarmonyPrefix]
        public static bool Prefix(Projectile __instance, int dmg, ref int id, Vector3 dir)
        {
            if (!GadgetNetwork.MatrixReady && GadgetNetwork.GetTimeSinceConnect() < GadgetNetwork.MatrixTimeout)
            {
                __instance.StartCoroutine(GadgetUtils.WaitAndInvoke(RPCMethod, GadgetNetwork.MatrixTimeout - GadgetNetwork.GetTimeSinceConnect(), () => GadgetNetwork.MatrixReady, __instance, dmg, id, dir));
                return true;
            }
            GadgetNetwork.ConvertIDToLocal(ItemRegistry.Singleton /* ProjectileRegistry */, ref id);
            return true;
        }
    }

    [HarmonyPatch(typeof(PortalScript))]
    [HarmonyPatch("SetName")]
    static class Patch_PortalScript_SetName
    {
        public static readonly MethodInfo RPCMethod = typeof(PortalScript).GetMethod("SetName", BindingFlags.Public | BindingFlags.Instance);

        [HarmonyPrefix]
        public static bool Prefix(PortalScript __instance, ref int a)
        {
            if (!GadgetNetwork.MatrixReady && GadgetNetwork.GetTimeSinceConnect() < GadgetNetwork.MatrixTimeout)
            {
                __instance.StartCoroutine(GadgetUtils.WaitAndInvoke(RPCMethod, GadgetNetwork.MatrixTimeout - GadgetNetwork.GetTimeSinceConnect(), () => GadgetNetwork.MatrixReady, __instance, a));
                return true;
            }
            GadgetNetwork.ConvertIDToLocal(PlanetRegistry.Singleton, ref a);
            return true;
        }
    }

    [HarmonyPatch(typeof(PortalAppearance))]
    [HarmonyPatch("Set")]
    static class Patch_PortalAppearance_Set
    {
        public static readonly MethodInfo RPCMethod = typeof(PortalAppearance).GetMethod("Set", BindingFlags.Public | BindingFlags.Instance);

        [HarmonyPrefix]
        public static bool Prefix(PortalAppearance __instance, ref int b, int h, int num)
        {
            if (!GadgetNetwork.MatrixReady && GadgetNetwork.GetTimeSinceConnect() < GadgetNetwork.MatrixTimeout)
            {
                __instance.StartCoroutine(GadgetUtils.WaitAndInvoke(RPCMethod, GadgetNetwork.MatrixTimeout - GadgetNetwork.GetTimeSinceConnect(), () => GadgetNetwork.MatrixReady, __instance, b, h, num));
                return true;
            }
            GadgetNetwork.ConvertIDToLocal(PlanetRegistry.Singleton, ref b);
            return true;
        }
    }

    [HarmonyPatch(typeof(PlayerScript))]
    [HarmonyPatch("Staff")]
    static class Patch_PlayerScript_Staff
    {
        public static readonly MethodInfo RPCMethod = typeof(PlayerScript).GetMethod("Staff", BindingFlags.Public | BindingFlags.Instance);

        [HarmonyPrefix]
        public static bool Prefix(PlayerScript __instance, ref int id, int dmg, NetworkPlayer pp)
        {
            if (!GadgetNetwork.MatrixReady && GadgetNetwork.GetTimeSinceConnect() < GadgetNetwork.MatrixTimeout)
            {
                __instance.StartCoroutine(GadgetUtils.WaitAndInvoke(RPCMethod, GadgetNetwork.MatrixTimeout - GadgetNetwork.GetTimeSinceConnect(), () => GadgetNetwork.MatrixReady, __instance, id, dmg, pp));
                return true;
            }
            GadgetNetwork.ConvertIDToLocal(ItemRegistry.Singleton /* ProjectileRegistry */, ref id);
            return true;
        }
    }

    [HarmonyPatch(typeof(PlayerScript))]
    [HarmonyPatch("ShootSpecial")]
    static class Patch_PlayerScript_ShootSpecial
    {
        public static readonly MethodInfo RPCMethod = typeof(PlayerScript).GetMethod("ShootSpecial", BindingFlags.Public | BindingFlags.Instance);

        [HarmonyPrefix]
        public static bool Prefix(PlayerScript __instance, ref int id, Vector3 targ, int dmg)
        {
            if (!GadgetNetwork.MatrixReady && GadgetNetwork.GetTimeSinceConnect() < GadgetNetwork.MatrixTimeout)
            {
                __instance.StartCoroutine(GadgetUtils.WaitAndInvoke(RPCMethod, GadgetNetwork.MatrixTimeout - GadgetNetwork.GetTimeSinceConnect(), () => GadgetNetwork.MatrixReady, __instance, id, targ, dmg));
                return true;
            }
            GadgetNetwork.ConvertIDToLocal(ItemRegistry.Singleton /* ProjectileRegistry */, ref id);
            return true;
        }
    }

    [HarmonyPatch(typeof(PlayerScript))]
    [HarmonyPatch("ShootProjectile")]
    static class Patch_PlayerScript_ShootProjectile
    {
        public static readonly MethodInfo RPCMethod = typeof(PlayerScript).GetMethod("ShootProjectile", BindingFlags.Public | BindingFlags.Instance);

        [HarmonyPrefix]
        public static bool Prefix(PlayerScript __instance, ref int id, Vector3 targ, int dmg)
        {
            if (!GadgetNetwork.MatrixReady && GadgetNetwork.GetTimeSinceConnect() < GadgetNetwork.MatrixTimeout)
            {
                __instance.StartCoroutine(GadgetUtils.WaitAndInvoke(RPCMethod, GadgetNetwork.MatrixTimeout - GadgetNetwork.GetTimeSinceConnect(), () => GadgetNetwork.MatrixReady, __instance, id, targ, dmg));
                return true;
            }
            GadgetNetwork.ConvertIDToLocal(ItemRegistry.Singleton /* ProjectileRegistry */, ref id);
            return true;
        }

        [HarmonyPostfix]
        public static void Postfix()
        {

        }
    }

    [HarmonyPatch(typeof(PlayerScript))]
    [HarmonyPatch("ShootProjectile2")]
    static class Patch_PlayerScript_ShootProjectile2
    {
        public static readonly MethodInfo RPCMethod = typeof(PlayerScript).GetMethod("ShootProjectile2", BindingFlags.Public | BindingFlags.Instance);

        [HarmonyPrefix]
        public static bool Prefix(PlayerScript __instance, ref int id, Vector3 targ, int dmg, Vector3 burst)
        {
            if (!GadgetNetwork.MatrixReady && GadgetNetwork.GetTimeSinceConnect() < GadgetNetwork.MatrixTimeout)
            {
                __instance.StartCoroutine(GadgetUtils.WaitAndInvoke(RPCMethod, GadgetNetwork.MatrixTimeout - GadgetNetwork.GetTimeSinceConnect(), () => GadgetNetwork.MatrixReady, __instance, id, targ, dmg, burst));
                return true;
            }
            GadgetNetwork.ConvertIDToLocal(ItemRegistry.Singleton /* ProjectileRegistry */, ref id);
            return true;
        }

        [HarmonyPostfix]
        public static void Postfix()
        {

        }
    }

    [HarmonyPatch(typeof(PlayerScript))]
    [HarmonyPatch("NetworkedProjectile")]
    static class Patch_PlayerScript_NetworkedProjectile
    {
        public static readonly MethodInfo RPCMethod = typeof(PlayerScript).GetMethod("NetworkedProjectile", BindingFlags.Public | BindingFlags.Instance);

        [HarmonyPrefix]
        public static bool Prefix(PlayerScript __instance, ref int id, int dmg, NetworkPlayer pp, Vector3 dir)
        {
            if (!GadgetNetwork.MatrixReady && GadgetNetwork.GetTimeSinceConnect() < GadgetNetwork.MatrixTimeout)
            {
                __instance.StartCoroutine(GadgetUtils.WaitAndInvoke(RPCMethod, GadgetNetwork.MatrixTimeout - GadgetNetwork.GetTimeSinceConnect(), () => GadgetNetwork.MatrixReady, __instance, id, dmg, pp, dir));
                return true;
            }
            GadgetNetwork.ConvertIDToLocal(ItemRegistry.Singleton /* ProjectileRegistry */, ref id);
            return true;
        }
    }

    [HarmonyPatch(typeof(PlayerAppearance))]
    [HarmonyPatch("UA")]
    static class Patch_PlayerAppearance_UA
    {
        public static readonly MethodInfo RPCMethod = typeof(PlayerAppearance).GetMethod("UA", BindingFlags.Public | BindingFlags.Instance);

        [HarmonyPrefix]
        public static bool Prefix(PlayerAppearance __instance, ref int[] ei, int a, bool dead)
        {
            if (!GadgetNetwork.MatrixReady && GadgetNetwork.GetTimeSinceConnect() < GadgetNetwork.MatrixTimeout)
            {
                __instance.StartCoroutine(GadgetUtils.WaitAndInvoke(RPCMethod, GadgetNetwork.MatrixTimeout - GadgetNetwork.GetTimeSinceConnect(), () => GadgetNetwork.MatrixReady, __instance, ei, a, dead));
                return true;
            }
            GadgetNetwork.ConvertIDToLocal(ItemRegistry.Singleton, ref ei[0]);
            GadgetNetwork.ConvertIDToLocal(ItemRegistry.Singleton, ref ei[1]);
            GadgetNetwork.ConvertIDToLocal(ItemRegistry.Singleton, ref ei[2]);
            GadgetNetwork.ConvertIDToLocal(ItemRegistry.Singleton, ref ei[3]);
            GadgetNetwork.ConvertIDToLocal(null /* RaceRegistry */, ref ei[4]);
            GadgetNetwork.ConvertIDToLocal(null /* UniformRegistry */, ref ei[6]);
            GadgetNetwork.ConvertIDToLocal(null /* AugmentRegistry */, ref ei[7]);
            return true;
        }
    }

    [HarmonyPatch(typeof(KylockeStand))]
    [HarmonyPatch("Set")]
    static class Patch_KylockeStand_Set
    {
        public static readonly MethodInfo RPCMethod = typeof(KylockeStand).GetMethod("Set", BindingFlags.Public | BindingFlags.Instance);


        [HarmonyPrefix]
        public static bool Prefix(KylockeStand __instance, ref int[] p)
        {
            if (!GadgetNetwork.MatrixReady && GadgetNetwork.GetTimeSinceConnect() < GadgetNetwork.MatrixTimeout)
            {
                __instance.StartCoroutine(GadgetUtils.WaitAndInvoke(RPCMethod, GadgetNetwork.MatrixTimeout - GadgetNetwork.GetTimeSinceConnect(), () => GadgetNetwork.MatrixReady, __instance, p));
                return true;
            }
            GadgetNetwork.ConvertIDToLocal(ItemRegistry.Singleton, ref p[0]);
            return true;
        }
    }

    [HarmonyPatch(typeof(ItemStandScript))]
    [HarmonyPatch("Set")]
    static class Patch_ItemStandScript_Set
    {
        public static readonly MethodInfo RPCMethod = typeof(ItemStandScript).GetMethod("Set", BindingFlags.Public | BindingFlags.Instance);

        [HarmonyPrefix]
        public static bool Prefix(ItemStandScript __instance, ref int[] p)
        {
            if (!GadgetNetwork.MatrixReady && GadgetNetwork.GetTimeSinceConnect() < GadgetNetwork.MatrixTimeout)
            {
                __instance.StartCoroutine(GadgetUtils.WaitAndInvoke(RPCMethod, GadgetNetwork.MatrixTimeout - GadgetNetwork.GetTimeSinceConnect(), () => GadgetNetwork.MatrixReady, __instance, p));
                return true;
            }
            GadgetNetwork.ConvertIDToLocal(__instance.isChipStand ? (Registry)ChipRegistry.Singleton : ItemRegistry.Singleton, ref p[0]);
            return true;
        }
    }

    [HarmonyPatch(typeof(ItemScript))]
    [HarmonyPatch("Chip")]
    static class Patch_ItemScript_Chip
    {
        public static readonly MethodInfo RPCMethod = typeof(ItemScript).GetMethod("Chip", BindingFlags.Public | BindingFlags.Instance);

        [HarmonyPrefix]
        public static bool Prefix(ItemScript __instance, ref int id)
        {
            if (!GadgetNetwork.MatrixReady && GadgetNetwork.GetTimeSinceConnect() < GadgetNetwork.MatrixTimeout)
            {
                __instance.StartCoroutine(GadgetUtils.WaitAndInvoke(RPCMethod, GadgetNetwork.MatrixTimeout - GadgetNetwork.GetTimeSinceConnect(), () => GadgetNetwork.MatrixReady, __instance, id));
                return true;
            }
            GadgetNetwork.ConvertIDToLocal(ChipRegistry.Singleton, ref id);
            return true;
        }
    }

    [HarmonyPatch(typeof(GameScript))]
    [HarmonyPatch("SpawnItem")]
    static class Patch_GameScript_SpawnItem
    {
        public static readonly MethodInfo RPCMethod = typeof(GameScript).GetMethod("SpawnItem", BindingFlags.Public | BindingFlags.Instance);

        [HarmonyPrefix]
        public static bool Prefix(GameScript __instance, ref int[] st, Vector3 pos)
        {
            if (!GadgetNetwork.MatrixReady && GadgetNetwork.GetTimeSinceConnect() < GadgetNetwork.MatrixTimeout)
            {
                __instance.StartCoroutine(GadgetUtils.WaitAndInvoke(RPCMethod, GadgetNetwork.MatrixTimeout - GadgetNetwork.GetTimeSinceConnect(), () => GadgetNetwork.MatrixReady, __instance, st, pos));
                return true;
            }
            GadgetNetwork.ConvertIDToLocal(ItemRegistry.Singleton, ref st[0]);
            GadgetNetwork.ConvertIDToLocal(ItemRegistry.Singleton, ref st[5]);
            GadgetNetwork.ConvertIDToLocal(ItemRegistry.Singleton, ref st[6]);
            GadgetNetwork.ConvertIDToLocal(ItemRegistry.Singleton, ref st[7]);
            return true;
        }
    }

    [HarmonyPatch(typeof(GameScript))]
    [HarmonyPatch("ChangePortal")]
    static class Patch_GameScript_ChangePortal
    {
        public static readonly MethodInfo RPCMethod = typeof(GameScript).GetMethod("ChangePortal", BindingFlags.Public | BindingFlags.Instance);

        [HarmonyPrefix]
        public static bool Prefix(GameScript __instance, ref int a, int challenge)
        {
            if (!GadgetNetwork.MatrixReady && GadgetNetwork.GetTimeSinceConnect() < GadgetNetwork.MatrixTimeout)
            {
                __instance.StartCoroutine(GadgetUtils.WaitAndInvoke(RPCMethod, GadgetNetwork.MatrixTimeout - GadgetNetwork.GetTimeSinceConnect(), () => GadgetNetwork.MatrixReady, __instance, a, challenge));
                return true;
            }
            GadgetNetwork.ConvertIDToLocal(PlanetRegistry.Singleton, ref a);
            return true;
        }
    }

    [HarmonyPatch(typeof(GameScript))]
    [HarmonyPatch("ChangePortal2")]
    static class Patch_GameScript_ChangePortal2
    {
        public static readonly MethodInfo RPCMethod = typeof(GameScript).GetMethod("ChangePortal2", BindingFlags.Public | BindingFlags.Instance);

        [HarmonyPrefix]
        public static bool Prefix(GameScript __instance, ref int a, int challenge)
        {
            if (!GadgetNetwork.MatrixReady && GadgetNetwork.GetTimeSinceConnect() < GadgetNetwork.MatrixTimeout)
            {
                __instance.StartCoroutine(GadgetUtils.WaitAndInvoke(RPCMethod, GadgetNetwork.MatrixTimeout - GadgetNetwork.GetTimeSinceConnect(), () => GadgetNetwork.MatrixReady, __instance, a, challenge));
                return true;
            }
            GadgetNetwork.ConvertIDToLocal(PlanetRegistry.Singleton, ref a);
            return true;
        }
    }

    [HarmonyPatch(typeof(DroidScript))]
    [HarmonyPatch("ShootProjectile")]
    static class Patch_DroidScript_ShootProjectile
    {
        public static readonly MethodInfo RPCMethod = typeof(DroidScript).GetMethod("ShootProjectile", BindingFlags.Public | BindingFlags.Instance);

        [HarmonyPrefix]
        public static bool Prefix(DroidScript __instance, ref int id, Vector3 targ, int dmg)
        {
            if (!GadgetNetwork.MatrixReady && GadgetNetwork.GetTimeSinceConnect() < GadgetNetwork.MatrixTimeout)
            {
                __instance.StartCoroutine(GadgetUtils.WaitAndInvoke(RPCMethod, GadgetNetwork.MatrixTimeout - GadgetNetwork.GetTimeSinceConnect(), () => GadgetNetwork.MatrixReady, __instance, id, targ, dmg));
                return true;
            }
            GadgetNetwork.ConvertIDToLocal(ItemRegistry.Singleton /* ProjectileRegistry */, ref id);
            return true;
        }
    }

    [HarmonyPatch(typeof(DroidManager))]
    [HarmonyPatch("UA")]
    static class Patch_DroidManager_UA
    {
        public static readonly MethodInfo RPCMethod = typeof(DroidManager).GetMethod("UA", BindingFlags.Public | BindingFlags.Instance);

        [HarmonyPrefix]
        public static bool Prefix(DroidManager __instance, ref int[] d)
        {
            if (!GadgetNetwork.MatrixReady && GadgetNetwork.GetTimeSinceConnect() < GadgetNetwork.MatrixTimeout)
            {
                __instance.StartCoroutine(GadgetUtils.WaitAndInvoke(RPCMethod, GadgetNetwork.MatrixTimeout - GadgetNetwork.GetTimeSinceConnect(), () => GadgetNetwork.MatrixReady, __instance, d));
                d = new int[] { 1000, 1000, 1000 }; // Fix to bug with host droids in multiplayer
                return true;
            }
            GadgetNetwork.ConvertIDToLocal(ItemRegistry.Singleton, ref d[0]);
            GadgetNetwork.ConvertIDToLocal(ItemRegistry.Singleton, ref d[1]);
            GadgetNetwork.ConvertIDToLocal(ItemRegistry.Singleton, ref d[2]);
            return true;
        }
    }

    [HarmonyPatch(typeof(ChunkWorld))]
    [HarmonyPatch("RefreshWall")]
    static class Patch_ChunkWorld_RefreshWall
    {
        public static readonly MethodInfo RPCMethod = typeof(ChunkWorld).GetMethod("RefreshWall", BindingFlags.Public | BindingFlags.Instance);

        [HarmonyPrefix]
        public static bool Prefix(ChunkWorld __instance, ref int[] gg)
        {
            if (!GadgetNetwork.MatrixReady && GadgetNetwork.GetTimeSinceConnect() < GadgetNetwork.MatrixTimeout)
            {
                __instance.StartCoroutine(GadgetUtils.WaitAndInvoke(RPCMethod, GadgetNetwork.MatrixTimeout - GadgetNetwork.GetTimeSinceConnect(), () => GadgetNetwork.MatrixReady, __instance, gg));
                return true;
            }
            for (int i = 0;i < gg.Length;i++)
            {
                GadgetNetwork.ConvertIDToLocal(TileRegistry.Singleton, ref gg[i]);
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(ChunkWorld))]
    [HarmonyPatch("RefreshShip")]
    static class Patch_ChunkWorld_RefreshShip
    {
        public static readonly MethodInfo RPCMethod = typeof(ChunkWorld).GetMethod("RefreshShip", BindingFlags.Public | BindingFlags.Instance);

        [HarmonyPrefix]
        public static bool Prefix(ChunkWorld __instance, ref int[] gg, ref int[] ggs)
        {
            if (!GadgetNetwork.MatrixReady && GadgetNetwork.GetTimeSinceConnect() < GadgetNetwork.MatrixTimeout)
            {
                __instance.StartCoroutine(GadgetUtils.WaitAndInvoke(RPCMethod, GadgetNetwork.MatrixTimeout - GadgetNetwork.GetTimeSinceConnect(), () => GadgetNetwork.MatrixReady, __instance, gg, ggs));
                return true;
            }
            for (int i = 0; i < gg.Length; i++)
            {
                GadgetNetwork.ConvertIDToLocal(TileRegistry.Singleton, ref gg[i]);
            }
            for (int i = 0; i < ggs.Length; i++)
            {
                GadgetNetwork.ConvertIDToLocal(TileRegistry.Singleton, ref ggs[i]);
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Chunk))]
    [HarmonyPatch("SetMaterial")]
    static class Patch_Chunk_SetMaterial
    {
        public static readonly MethodInfo RPCMethod = typeof(Chunk).GetMethod("SetMaterial", BindingFlags.Public | BindingFlags.Instance);

        [HarmonyPrefix]
        public static bool Prefix(Chunk __instance, ref int a)
        {
            if (!GadgetNetwork.MatrixReady && GadgetNetwork.GetTimeSinceConnect() < GadgetNetwork.MatrixTimeout)
            {
                __instance.StartCoroutine(GadgetUtils.WaitAndInvoke(RPCMethod, GadgetNetwork.MatrixTimeout - GadgetNetwork.GetTimeSinceConnect(), () => GadgetNetwork.MatrixReady, __instance, a));
                return true;
            }
            GadgetNetwork.ConvertIDToLocal(PlanetRegistry.Singleton, ref a);
            return true;
        }
    }
}