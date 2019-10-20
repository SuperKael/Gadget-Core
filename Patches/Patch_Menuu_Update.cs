using HarmonyLib;
using GadgetCore.API;
using GadgetCore;
using UnityEngine;
using System.Collections;

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
                        __instance.StartCoroutine(ModMenu(__instance));
                        __instance.GetComponent<AudioSource>().PlayOneShot((AudioClip)Resources.Load("Au/confirm"), Menuu.soundLevel / 10f);
                        return false;
                    }
                    else if (___hit.transform.gameObject.name.Equals("bQuit"))
                    {
                        foreach (System.Diagnostics.Process process in ModDescPanelController.configHandles)
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
            SceneInjector.modMenuBackButtonBeam.transform.localScale = new Vector3(30f, 0f, 1f);
            SceneInjector.modMenuBackButtonHolder.transform.position = new Vector3(-40f, 0f, 0f);
            instance.menuMain.SetActive(false);
            SceneInjector.modMenu.SetActive(true);
            SceneInjector.modMenuBackButtonBeam.GetComponent<Animation>().Play();
            yield return new WaitForSeconds(0.1f);
            SceneInjector.modMenuBackButtonHolder.GetComponent<Animation>().Play();
            yield return null;
            yield break;
        }
    }
}