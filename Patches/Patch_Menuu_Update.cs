using HarmonyLib;
using GadgetCore.API;
using UnityEngine;
using System.Collections;
using GadgetCore.API.ConfigMenu;

namespace GadgetCore.Patches
{
    [HarmonyPatch(typeof(Menuu))]
    [HarmonyPatch("Update")]
    static class Patch_Menuu_Update
    {
        [HarmonyPrefix]
        public static bool Prefix(Menuu __instance, ref Ray ___ray, ref RaycastHit ___hit)
        {
            if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
            {
                ___ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(___ray, out ___hit, 10f))
                {
                    if (___hit.transform.gameObject.name.Equals("bModMenu"))
                    {
                        __instance.GetComponent<AudioSource>().PlayOneShot((AudioClip)Resources.Load("Au/confirm"), Menuu.soundLevel / 10f);
                        __instance.StartCoroutine(ModMenu(__instance));
                        return false;
                    }
                    else if (___hit.transform.gameObject.name.Equals("bQuit"))
                    {
                        foreach (System.Diagnostics.Process process in ModMenuController.ConfigHandles)
                        {
                            if (process != null && !process.HasExited) process.Kill();
                        }
                    }
                }
            }
            return true;
        }
        private static IEnumerator ModMenu(Menuu instance)
        {
            ModBrowser.CloseModBrowser();
            SceneInjector.ModMenuPanel.Rebuild();
            SceneInjector.ModMenuBackButtonBeam.transform.localScale = new Vector3(30f, 0f, 1f);
            SceneInjector.ModMenuBackButtonHolder.transform.position = new Vector3(-40f, 0f, 0f);
            instance.menuMain.SetActive(false);
            SceneInjector.ModMenu.SetActive(true);
            SceneInjector.ModMenuCanvas.GetComponent<CanvasGroup>().alpha = 1;
            SceneInjector.ModMenuCanvas.GetComponent<CanvasGroup>().interactable = true;
            SceneInjector.ModMenuCanvas.GetComponent<CanvasGroup>().blocksRaycasts = true;
            instance.StartCoroutine(DelayRebuildConfigMenus());
            SceneInjector.ModMenuBackButtonBeam.GetComponent<Animation>().Play();
            yield return new WaitForSeconds(0.1f);
            SceneInjector.ModMenuBackButtonHolder.GetComponent<Animation>().Play();
            yield return null;
            yield break;
        }

        private static IEnumerator DelayRebuildConfigMenus()
        {
            yield return new WaitForEndOfFrame();
            GadgetModConfigs.ResetAllConfigMenus();
            yield break;
        }
    }
}