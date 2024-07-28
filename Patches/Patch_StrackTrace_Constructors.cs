using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using GadgetCore.Loader;
using HarmonyLib;

namespace GadgetCore.Patches
{
    [HarmonyPatch]
    static class StackTracePatch
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
