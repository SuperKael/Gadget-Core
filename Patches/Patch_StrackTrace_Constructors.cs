using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using GadgetCore.Loader;
using HarmonyLib;

namespace GadgetCore.Patches
{
    [HarmonyPatch]
    internal static class StackTracePatch
    {
        [HarmonyTargetMethods]
        public static IEnumerable<MethodBase> TargetMethods()
        {
            return typeof(StackTrace).GetConstructors(AccessTools.all);
        }

        [HarmonyPostfix]
        public static void Postfix(StackTrace __instance)
        {
            var frames = __instance.GetFrames();
            if (frames == null) return;
            foreach (var frame in frames) GadgetLoader.InjectSymbolsIntoStackFrame(frame);
        }
    }
}
