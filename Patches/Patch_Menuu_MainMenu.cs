using HarmonyLib;
using GadgetCore.API;
using GadgetCore;
using UnityEngine;
using System.Collections;
using System;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(Menuu))]
    [HarmonyPatch("MainMenu")]
    static class Patch_Menuu_MainMenu
    {
        [HarmonyPrefix]
        public static bool Prefix(Menuu __instance, Ray ___ray, RaycastHit ___hit)
        {
            if (ModDescPanelController.RestartNeeded)
            {
                foreach (System.Diagnostics.Process process in ModDescPanelController.configHandles)
                {
                    if (process != null && !process.HasExited) process.Kill();
                }
                Application.Quit();
                return false;
            }
            SceneInjector.modMenuBeam.transform.localScale = new Vector3(30f, 0f, 1f);
            SceneInjector.modMenuButtonHolder.transform.position = new Vector3(-40f, 0f, 0f);
            SceneInjector.modMenu.SetActive(false);
            __instance.StartCoroutine(AnimateModMenuButton(__instance));
            return true;
        }
        private static IEnumerator AnimateModMenuButton(Menuu instance)
        {
            SceneInjector.modMenuBeam.GetComponent<Animation>().Play();
            yield return new WaitForSeconds(0.3f);
            SceneInjector.modMenuButtonHolder.GetComponent<Animation>().Play();
            yield break;
        }
    }
}