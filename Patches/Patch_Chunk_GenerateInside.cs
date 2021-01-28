using HarmonyLib;
using GadgetCore.API;
using GadgetCore.Util;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using System.Reflection;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(Chunk))]
    [HarmonyPatch("GenerateInside")]
    static class Patch_Chunk_GenerateInside
    {
        public static readonly FieldInfo networkStuffField = typeof(Chunk).GetField("networkStuff", BindingFlags.Public | BindingFlags.Instance);
        public static readonly FieldInfo tempField = typeof(Chunk).GetField("temp", BindingFlags.Public | BindingFlags.Instance);

        [HarmonyPostfix]
        public static void Postfix(Chunk __instance, ref IEnumerator __result)
        {
            if (PlanetRegistry.Singleton[SpawnerScript.curBiome] is PlanetInfo planet)
            {
                __result = GenerateWrapper(__result, __instance, planet);
            }
        }

        private static IEnumerator GenerateWrapper(IEnumerator generateRoutine, Chunk instance, PlanetInfo planet)
        {
            yield return generateRoutine;
            IEnumerable<GameObject> objs = planet.InvokeOnGenerateInside(instance);
            if (objs == null) yield break;
            GameObject[] networkStuff = networkStuffField.GetValue<GameObject[]>(instance);
            int temp = tempField.GetValue<int>(instance);
            foreach (GameObject obj in objs)
            {
                if (obj == null) continue;
                networkStuff[temp] = obj;
                temp++;
            }
            tempField.SetValue(instance, temp);
            yield break;
        }
    }
}