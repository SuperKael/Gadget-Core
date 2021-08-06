using HarmonyLib;
using UnityEngine;
using System.Collections;
using GadgetCore.API.ConfigMenu;
using GadgetCore.API;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(Menuu))]
    [HarmonyPatch("MainMenu")]
    static class Patch_Menuu_MainMenu
    {
        [HarmonyPrefix]
        [HarmonyOverrides]
        public static bool Prefix(Menuu __instance, Ray ___ray, RaycastHit ___hit, ref IEnumerator __result)
        {
            __result = GadgetCoreAPI.EmptyEnumerator();
            if (GadgetCoreAPI.IsInputFrozen()) return false;
            if (ModMenuController.RestartNeeded || ModBrowser.RestartNeeded)
            {
                foreach (System.Diagnostics.Process process in ModMenuController.ConfigHandles)
                {
                    if (process != null && !process.HasExited) process.Kill();
                }
                GadgetCoreAPI.Quit();
                return false;
            }
            ModBrowser.CloseModBrowser();
            SceneInjector.ModMenuBeam.transform.localScale = new Vector3(30f, 0f, 1f);
            SceneInjector.ModMenuButtonHolder.transform.position = new Vector3(-40f, 0f, 0f);
            SceneInjector.ModMenu.SetActive(false);
            SceneInjector.ModMenuCanvas.GetComponent<CanvasGroup>().alpha = 0;
            SceneInjector.ModMenuCanvas.GetComponent<CanvasGroup>().interactable = false;
            SceneInjector.ModMenuCanvas.GetComponent<CanvasGroup>().blocksRaycasts = false;
            GadgetModConfigs.CloseAllConfigMenus();
            __instance.StartCoroutine(AnimateModMenuButton(__instance));
            return true;
        }
        private static IEnumerator AnimateModMenuButton(Menuu instance)
        {
            SceneInjector.ModMenuBeam.GetComponent<Animation>().Play();
            yield return new WaitForSeconds(0.3f);
            SceneInjector.ModMenuButtonHolder.GetComponent<Animation>().Play();
            yield break;
        }
    }
}